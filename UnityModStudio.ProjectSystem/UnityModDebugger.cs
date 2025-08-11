using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;
using Microsoft.VisualStudio.Shell.Interop;
using UnityModStudio.Common.Options;
using Process = System.Diagnostics.Process;

namespace UnityModStudio.ProjectSystem;

[Export(typeof(IDebugProfileLaunchTargetsProvider)), Order(2000)]
[AppliesTo(ProjectCapability.UnityModStudio)]
[method: ImportingConstructor]
public class UnityModDebugger(ConfiguredProject configuredProject)
    : DebugLaunchProviderBase(configuredProject), IDebugProfileLaunchTargetsProvider
{
    private const string UnityDebugEngineName = "Unity";
    private static readonly Guid UnityDebugEngineGuid = new("f18a0491-a310-4822-b12f-12cc30404eec");
    private static readonly Guid UnityPackageGuid = new("b6546c9c-e5fe-4095-8d39-c080d9bd6a85");

    private IPEndPoint? _endPoint;

    public override async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions)
    {
        var noDebug = (launchOptions & DebugLaunchOptions.NoDebug) != 0;
        if (noDebug)
            return [];

        await LoadUnityToolsAsync();

        _endPoint = new IPEndPoint(IPAddress.Loopback, GetAvailablePort());
        var debugHostType = GetDebugHostType();

        var launchSettings = new DebugLaunchSettings(launchOptions | DebugLaunchOptions.WaitForAttachComplete | DebugLaunchOptions.DetachOnStop)
        {
            LaunchOperation = DebugLaunchOperation.Custom,
            Executable = UnityDebugEngineName,
            LaunchDebugEngineGuid = UnityDebugEngineGuid,
            Options = $"{_endPoint}|{debugHostType?.AssemblyQualifiedName}",
        };
        return [launchSettings];
    }

    private static int GetAvailablePort()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)socket.LocalEndPoint).Port;
    }

    private static Type? GetDebugHostType()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var unityToolsAssembly = assemblies.FirstOrDefault(assembly => assembly.GetName().Name == "SyntaxTree.VisualStudio.Unity");
        return unityToolsAssembly?.GetType("SyntaxTree.VisualStudio.Unity.Debugger.DebugEngineHost");
    }

    private async Task LoadUnityToolsAsync()
    {
        await ThreadingService.SwitchToUIThread();

        var shell = (IVsShell) ServiceProvider.GetService(typeof(SVsShell));
        if (shell == null)
            throw new InvalidOperationException("IVsShell is null.");

        var vstuGuid = UnityPackageGuid;
        shell.LoadPackage(ref vstuGuid, out _);
    }

    public override Task<bool> CanLaunchAsync(DebugLaunchOptions launchOptions) => IsEnabledAsync(null);

    // TODO: only support proper profile
    public bool SupportsProfile(ILaunchProfile profile)
    {
        if (profile.CommandName != "Executable")
            return false;

        return ThreadingService.ExecuteSynchronously(() => IsEnabledAsync(profile));
    }

    private async Task<bool> IsEnabledAsync(ILaunchProfile? profile)
    {
        var configuration = await GetGameConfigurationAsync(profile);
        return !string.IsNullOrEmpty(configuration.GameExecutablePath);
    }

    private async Task<GameConfiguration> GetGameConfigurationAsync(ILaunchProfile? profile)
    {
        var properties = ConfiguredProject.Services.ProjectPropertiesProvider!.GetCommonProperties();
        var gameVersion = profile?.OtherSettings.TryGetValue("gameVersion", out var obj) ?? false ? obj as string : null;
        var configuration = await GameConfiguration.GetAsync(properties, gameVersion);
        if (string.IsNullOrEmpty(configuration.GameExecutablePath) && !string.IsNullOrEmpty(gameVersion))
            configuration = await GameConfiguration.GetAsync(properties, null);
        return configuration;
    }

    public async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions, ILaunchProfile profile)
    {
        var configuration = await GetGameConfigurationAsync(profile);
        if (configuration.DoorstopMode == DoorstopMode.Disabled)
            return [];

        return await QueryDebugTargetsAsync(launchOptions);
    }

    public async Task OnBeforeLaunchAsync(DebugLaunchOptions launchOptions, ILaunchProfile profile)
    {
        var configuration = await GetGameConfigurationAsync(profile);
        var exePath = configuration.GameExecutablePath;

        if (exePath == null)
            throw new InvalidOperationException("Unable to determine game executable path.");

        var psi = new ProcessStartInfo(exePath)
        {
            UseShellExecute = false,
            WorkingDirectory = configuration.GamePath ?? throw new InvalidOperationException("Unable to determine game path."),
        };

        var noDebug = (launchOptions & DebugLaunchOptions.NoDebug) != 0 || configuration.DoorstopMode == DoorstopMode.Disabled;
        if (!noDebug)
            psi.Arguments = $"--doorstop-mono-debug-enabled true --doorstop-mono-debug-suspend true --doorstop-mono-debug-address {_endPoint}";

        var process = Process.Start(psi);
        if (process == null)
            throw new InvalidOperationException("Unable to launch the game.");

        // TODO: wait for restart by Steam, if any
    }

    public Task OnAfterLaunchAsync(DebugLaunchOptions launchOptions, ILaunchProfile profile) => Task.CompletedTask;
}