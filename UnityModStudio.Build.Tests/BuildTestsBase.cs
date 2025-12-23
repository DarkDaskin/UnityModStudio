using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Mono.Cecil;
using UnityModStudio.Common;
using UnityModStudio.Common.Options;
using UnityModStudio.Common.Tests;

namespace UnityModStudio.Build.Tests;

public abstract class BuildTestsBase
{
    private static readonly string[] ProjectSubDirectoriesToRemove = ["bin", "obj"];

#if DEBUG
    protected const string Configuration = "Debug";
#else
    protected const string Configuration = "Release";
#endif

    protected readonly ProjectOptions ProjectOptions = new()
    {
        GlobalProperties = new Dictionary<string, string>
        {
            ["BuildPackageVersion"] = Utils.GetPackageVersion(),
            ["Configuration"] = Configuration,
        }
    };

    private string _gameRegistryPath = null!;
    private string _generalSettingsPath = null!;

    private readonly List<DirectoryInfo> _scratchDirs = [];

    public TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public void CommonTestInitialize()
    {
        _gameRegistryPath = Path.GetTempFileName();
        _generalSettingsPath = Path.GetTempFileName();
        ProjectOptions.GlobalProperties["GameRegistryPath"] = _gameRegistryPath;
        ProjectOptions.GlobalProperties["GeneralSettingsPath"] = _generalSettingsPath;

        AssemblyFixture.BinaryLogger.InitializeTest(TestContext);
    }

    [TestCleanup]
    public void CommonTestCleanup()
    {
        AssemblyFixture.BinaryLogger.Shutdown();

        // Clean cached targets files. Without this resolved data files will be stale and tests will fail when run in sequence.
        // TODO: ensure this caching does not cause problems in Visual Studio.
        ProjectCollection.GlobalProjectCollection.UnloadAllProjects();

        if (File.Exists(_gameRegistryPath))
            File.Delete(_gameRegistryPath);
        if (File.Exists(_generalSettingsPath))
            File.Delete(_generalSettingsPath);

        foreach (var scratchDir in _scratchDirs)
            if (scratchDir.Exists)
                scratchDir.Delete(true);
    }

    protected (ProjectInstance, TestLogger) GetProjectWithRestore(string projectPath, 
        IReadOnlyDictionary<string, string>? additionalGlobalProperties = null)
    {
        // Clean up previous build results to ensure reproducibility.
        var projectDirectory = new DirectoryInfo(Path.GetDirectoryName(projectPath)!);
        foreach (var subDirectory in ProjectSubDirectoriesToRemove.SelectMany(projectDirectory.EnumerateDirectories))
            subDirectory.Delete(true);

        var projectOptions = ProjectOptions;
        if (additionalGlobalProperties != null)
        {
            projectOptions = new ProjectOptions { GlobalProperties = new Dictionary<string, string>(projectOptions.GlobalProperties) };
            foreach (var kv in additionalGlobalProperties)
                projectOptions.GlobalProperties[kv.Key] = kv.Value;
        }

        var project = ProjectInstance.FromFile(projectPath, projectOptions);
        var logger = new TestLogger();

        var success = project.Build(["Restore"], [logger]);
        Assert.IsTrue(success, $"Failed to restore the project. Errors:\n{string.Join("\n", logger.BuildErrors.Select(args => args.Message))}");

        // Must reload the project to import NuGet-provided build files.
        project = ProjectInstance.FromFile(projectPath, projectOptions);
        return (project, logger);
    }

    protected void SetupGameRegistry(params Game[] games)
    {
        var gameRegistry = new GameRegistry(_gameRegistryPath);
        foreach (var game in games) 
            gameRegistry.AddGame(game);
        gameRegistry.Save();
    }

    protected GameRegistry LoadGameRegistry()
    {
        var gameRegistry = new GameRegistry(_gameRegistryPath);
        gameRegistry.Load();
        return gameRegistry;
    }

    protected void SetupGeneralSettings(Action<GeneralSettings> action)
    {
        var generalSettingsManager = new GeneralSettingsManager(_generalSettingsPath);
        action(generalSettingsManager.Settings);
        generalSettingsManager.Save();
    }

    protected string MakeGameCopy(string gameType)
    {
        var scratchDir = CreateScratchDir();
        TestUtils.CopyDirectory(Path.Combine(SampleGameInfo.DownloadPath, gameType), scratchDir.FullName);
        return scratchDir.FullName;
    }

    protected string GetScratchPath() => CreateScratchDir().FullName;

    protected DirectoryInfo CreateScratchDir()
    {
        var scratchDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        _scratchDirs.Add(scratchDir);
        scratchDir.Create();
        return scratchDir;
    }

    protected static void ResolveGameProperties(Game game)
    {
        var success = GameInformationResolver.TryGetGameInformation(game.Path, out var gameInformation, out _, out _);
        Assert.IsTrue(success);
        Assert.IsNotNull(gameInformation);

        if (string.IsNullOrEmpty(game.DisplayName) && !string.IsNullOrEmpty(gameInformation.Name))
            game.DisplayName = gameInformation.Name;
        game.GameName = gameInformation.Name;
        game.GameExecutableFileName = gameInformation.GameExecutableFile.Name;
        game.Architecture = gameInformation.Architecture.ToString();
        game.UnityVersion = gameInformation.UnityVersion;
        game.MonoProfile = gameInformation.GetMonoProfileString();
    }

    protected static void VerifyModAssemblyGameVersion(string modAssemblyPath, string gameVersion)
    {
        using var modAssembly = AssemblyDefinition.ReadAssembly(modAssemblyPath);
        var gameAssemblyReference = modAssembly.MainModule.AssemblyReferences.Single(reference => reference.Name == "Assembly-CSharp");
        Assert.AreEqual(gameVersion, gameAssemblyReference.Version.ToString(2));
    }

    protected static void VerifyModAssemblyConstants(string modAssemblyPath, params string[] constants)
    {
        using var modAssembly = AssemblyDefinition.ReadAssembly(modAssemblyPath);
        var constantsType = modAssembly.MainModule.GetType("Constants");
        Assert.IsNotNull(constantsType, "Constants type not found in mod assembly.");
        Assert.AreEqual(constants.Length, constantsType.Fields.Count, "Wrong number of constants.");
        foreach (var constant in constants)
        {
            var field = constantsType.Fields.FirstOrDefault(f => f.Name == constant);
            Assert.IsNotNull(field, $"Constant '{constant}' not found in Constants type.");
        }
    }
}