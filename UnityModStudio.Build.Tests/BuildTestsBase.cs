using System.Diagnostics.CodeAnalysis;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Logging;
using UnityModStudio.Common;
using UnityModStudio.Common.Options;
using UnityModStudio.Common.Tests;

namespace UnityModStudio.Build.Tests;

public abstract class BuildTestsBase
{
    private static readonly string[] ProjectSubDirectoriesToRemove = ["bin", "obj"];

    protected readonly ProjectOptions ProjectOptions = new()
    {
        GlobalProperties = new Dictionary<string, string>
        {
            ["BuildPackageVersion"] = Utils.GetPackageVersion(),
        }
    };

    private string _gameRegistryPath = null!;

    private DirectoryInfo? _scratchDir;

    public TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _gameRegistryPath = Path.GetTempFileName();
        ProjectOptions.GlobalProperties["GameRegistryPath"] = _gameRegistryPath;

        AssemblyFixture.BinaryLogger.InitializeTest(TestContext);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        AssemblyFixture.BinaryLogger.Shutdown();

        // Clean cached targets files. Without this resolved data files will be stale and tests will fail when run in sequence.
        // TODO: ensure this caching does not cause problems in Visual Studio.
        ProjectCollection.GlobalProjectCollection.UnloadAllProjects();

        if (File.Exists(_gameRegistryPath))
            File.Delete(_gameRegistryPath);

        if (_scratchDir?.Exists ?? false)
            _scratchDir.Delete(true);
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
        Assert.IsTrue(success);

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

    protected string MakeGameCopy(string gameType)
    {
        CreateScratchDir();
        TestUtils.CopyDirectory(Path.Combine(SampleGameInfo.DownloadPath, gameType), _scratchDir.FullName);
        return _scratchDir.FullName;
    }

    [MemberNotNull(nameof(_scratchDir))]
    private void CreateScratchDir()
    {
        _scratchDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        _scratchDir.Create();
    }

    protected static void ResolveGameProperties(Game game)
    {
        var success = GameInformationResolver.TryGetGameInformation(game.Path, out var gameInformation, out _);
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
}