using UnityModStudio.Common.Options;

namespace UnityModStudio.Build.Tests;

[TestClass]
public class GameRegistryManagementTests : BuildTestsBase
{
    [TestMethod]
    public void WhenAddGameToRegistryCalledWithoutGamePath_ProduceError()
    {
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj");

        var success = project.Build(["AddGameToRegistry"], [logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.AreEqual(1, logger.BuildErrors.Count);
        Assert.AreEqual("UMS0005", logger.BuildErrors[0].Code);
        Assert.AreEqual("GamePath property is not set.", logger.BuildErrors[0].Message);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
    }

    [TestMethod]
    public void WhenAddGameToRegistryCalledWithInvalidGameRegistryPath_ProduceError()
    {
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj",
            new Dictionary<string, string>
            {
                { "GamePath", Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0") },
                { "GameRegistryPath", "!@#$%^&*()" },
            });

        var success = project.Build(["AddGameToRegistry"], [logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.AreEqual(1, logger.BuildErrors.Count);
        // Exception message may be localized, check only prefix.
        Assert.IsTrue(logger.BuildErrors[0].Message?.StartsWith("Unable to initialize game registry:"));
        Assert.AreEqual(0, logger.BuildWarnings.Count);
    }

    [TestMethod]
    public void WhenAddGameToRegistryCalledWithAllProperties_AddGameToRegistry()
    {
        var gamePath = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0");
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj",
            new Dictionary<string, string>
            {
                { "GamePath", gamePath },
                { "GameDisplayName", "AddGameToRegistryTest" },
                { "GameModsPath", "Mods" },
                { "GameVersion", "1.0" },
                { "ModDeploymentMode", "Link" },
                { "DeploySourceCode", "true" },
                { "DoorstopMode", "DebuggingAndModLoading" },
                { "UseAlternateDoorstopDllName", "true" },
            });

        var success = project.Build(["AddGameToRegistry"], [logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        var gameRegistry = LoadGameRegistry();
        Assert.AreEqual(1, gameRegistry.Games.Count);
        var game = gameRegistry.Games.First();
        Assert.AreEqual(gamePath, game.Path);
        Assert.AreEqual("AddGameToRegistryTest", game.DisplayName);
        Assert.AreEqual("Mods", game.ModsPath);
        Assert.AreEqual("1.0", game.Version);
        Assert.AreEqual(ModDeploymentMode.Link, game.ModDeploymentMode);
        Assert.AreEqual(true, game.DeploySourceCode);
        Assert.AreEqual(DoorstopMode.DebuggingAndModLoading, game.DoorstopMode);
        Assert.AreEqual(true, game.UseAlternateDoorstopDllName);
        Assert.AreEqual("Unity2018Test", game.GameName);
        Assert.AreEqual("Unity2018Test.exe", game.GameExecutableFileName);
        Assert.AreEqual("X64", game.Architecture);
        Assert.AreEqual("2018.4.36f1", game.UnityVersion);
        Assert.AreEqual(".NET 4.6", game.MonoProfile);
    }

    [TestMethod]
    public void WhenAddGameToRegistryCalledWithOnlyGamePath_AddGameToRegistry()
    {
        var gamePath = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0");
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj",
            new Dictionary<string, string>
            {
                { "GamePath", gamePath },
            });

        var success = project.Build(["AddGameToRegistry"], [logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        var gameRegistry = LoadGameRegistry();
        Assert.AreEqual(1, gameRegistry.Games.Count);
        var game = gameRegistry.Games.First();
        Assert.AreEqual(gamePath, game.Path);
        Assert.AreEqual("Unity2018Test", game.DisplayName);
        Assert.IsNull(game.ModsPath);
        Assert.IsNull(game.Version);
        Assert.AreEqual(ModDeploymentMode.Copy, game.ModDeploymentMode);
        Assert.AreEqual(false, game.DeploySourceCode);
        Assert.AreEqual(DoorstopMode.Debugging, game.DoorstopMode);
        Assert.AreEqual(false, game.UseAlternateDoorstopDllName);
        Assert.AreEqual("Unity2018Test", game.GameName);
        Assert.AreEqual("Unity2018Test.exe", game.GameExecutableFileName);
        Assert.AreEqual("X64", game.Architecture);
        Assert.AreEqual("2018.4.36f1", game.UnityVersion);
        Assert.AreEqual(".NET 4.6", game.MonoProfile);
    }

    [TestMethod]
    public void WhenAddGameToRegistryCalledWithOnlyGamePathAndGameVersion_AddGameToRegistry()
    {
        var gamePath = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0");
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj",
            new Dictionary<string, string>
            {
                { "GamePath", gamePath },
                { "GameVersion", "1.0" },
            });

        var success = project.Build(["AddGameToRegistry"], [logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        var gameRegistry = LoadGameRegistry();
        Assert.AreEqual(1, gameRegistry.Games.Count);
        var game = gameRegistry.Games.First();
        Assert.AreEqual(gamePath, game.Path);
        Assert.AreEqual("Unity2018Test [1.0]", game.DisplayName);
        Assert.IsNull(game.ModsPath);
        Assert.AreEqual("1.0", game.Version);
        Assert.AreEqual(ModDeploymentMode.Copy, game.ModDeploymentMode);
        Assert.AreEqual(false, game.DeploySourceCode);
        Assert.AreEqual(DoorstopMode.Debugging, game.DoorstopMode);
        Assert.AreEqual(false, game.UseAlternateDoorstopDllName);
        Assert.AreEqual("Unity2018Test", game.GameName);
        Assert.AreEqual("Unity2018Test.exe", game.GameExecutableFileName);
        Assert.AreEqual("X64", game.Architecture);
        Assert.AreEqual("2018.4.36f1", game.UnityVersion);
        Assert.AreEqual(".NET 4.6", game.MonoProfile);
    }

    [TestMethod]
    public void WhenAddGameToRegistryCalledWithExistingDisplayName_AddGameToRegistryWithGeneratedDisplayName()
    {
        SetupGameRegistry(new Game
        {
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-netstandard20-v2.0"),
            DisplayName = "Test",
        });
        var gamePath = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0");
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj",
            new Dictionary<string, string>
            {
                { "GamePath", gamePath },
                { "GameDisplayName", "Test" },
            });

        var success = project.Build(["AddGameToRegistry"], [logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        var gameRegistry = LoadGameRegistry();
        Assert.AreEqual(2, gameRegistry.Games.Count);
        var game = gameRegistry.Games.First(g => g.Path == gamePath);
        Assert.AreEqual(gamePath, game.Path);
        Assert.AreEqual("Test (1)", game.DisplayName);
        Assert.IsNull(game.ModsPath);
        Assert.IsNull(game.Version);
        Assert.AreEqual(ModDeploymentMode.Copy, game.ModDeploymentMode);
        Assert.AreEqual(false, game.DeploySourceCode);
        Assert.AreEqual(DoorstopMode.Debugging, game.DoorstopMode);
        Assert.AreEqual(false, game.UseAlternateDoorstopDllName);
        Assert.AreEqual("Unity2018Test", game.GameName);
        Assert.AreEqual("Unity2018Test.exe", game.GameExecutableFileName);
        Assert.AreEqual("X64", game.Architecture);
        Assert.AreEqual("2018.4.36f1", game.UnityVersion);
        Assert.AreEqual(".NET 4.6", game.MonoProfile);
    }

    [TestMethod]
    public void WhenUpdateGameRegistryCalledWithInvalidGameRegistryPath_ProduceError()
    {
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj",
            new Dictionary<string, string>
            {
                { "GameRegistryPath", "!@#$%^&*()" },
            });

        var success = project.Build(["UpdateGameRegistry"], [logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.AreEqual(1, logger.BuildErrors.Count);
        Assert.IsTrue(logger.BuildErrors[0].Message?.StartsWith("Unable to initialize game registry:"));
        Assert.AreEqual(0, logger.BuildWarnings.Count);
    }

    [TestMethod]
    public void WhenUpdateGameRegistryCalledWithoutAnyLookupProperty_ProduceError()
    {
        var (project, logger) = GetProjectWithRestore(@"Projects\Incorrect\NoGameProperties\NoGameProperties.csproj");

        var success = project.Build(["UpdateGameRegistry"], [logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.AreEqual(1, logger.BuildErrors.Count);
        Assert.AreEqual("No game registry entries match speified game properties.", logger.BuildErrors[0].Message);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
    }

    [TestMethod]
    public void WhenUpdateGameRegistryCalledWithNonExistingGameName_ProduceError()
    {
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj");

        var success = project.Build(["UpdateGameRegistry"], [logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.AreEqual(1, logger.BuildErrors.Count);
        Assert.AreEqual("No game registry entries match speified game properties.", logger.BuildErrors[0].Message);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
    }

    [TestMethod]
    public void WhenUpdateGameRegistryCalledWithGameInstanceId_UpdateGameRegistry()
    {
        var existingGame = new Game
        {
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
        };
        ResolveGameProperties(existingGame);
        SetupGameRegistry(existingGame);
        var newGamePath = Path.Combine(SampleGameInfo.DownloadPath, "2018-netstandard20-v2.0");
        var (project, logger) = GetProjectWithRestore(@"Projects\Incorrect\NoGameProperties\NoGameProperties.csproj",
            new Dictionary<string, string>
            {
                { "GameInstanceId", existingGame.Id.ToString() },
                { "GamePath", newGamePath },
                { "GameDisplayName", "UpdateGameRegistryTest" },
                { "GameModsPath", "Mods" },
                { "GameVersion", "1.0" },
                { "ModDeploymentMode", "Link" },
                { "DeploySourceCode", "true" },
                { "DoorstopMode", "DebuggingAndModLoading" },
                { "UseAlternateDoorstopDllName", "true" },
            });

        var success = project.Build(["UpdateGameRegistry"], [logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        var gameRegistry = LoadGameRegistry();
        Assert.AreEqual(1, gameRegistry.Games.Count);
        var game = gameRegistry.Games.First();
        Assert.AreEqual(newGamePath, game.Path);
        Assert.AreEqual("UpdateGameRegistryTest", game.DisplayName);
        Assert.AreEqual("Mods", game.ModsPath);
        Assert.AreEqual("1.0", game.Version);
        Assert.AreEqual(ModDeploymentMode.Link, game.ModDeploymentMode);
        Assert.AreEqual(true, game.DeploySourceCode);
        Assert.AreEqual(DoorstopMode.DebuggingAndModLoading, game.DoorstopMode);
        Assert.AreEqual(true, game.UseAlternateDoorstopDllName);
        Assert.AreEqual("Unity2018Test", game.GameName);
        Assert.AreEqual("Unity2018Test.exe", game.GameExecutableFileName);
        Assert.AreEqual("X64", game.Architecture);
        Assert.AreEqual("2018.4.36f1", game.UnityVersion);
        Assert.AreEqual(".NET Standard 2.0", game.MonoProfile);
    }

    [TestMethod]
    public void WhenUpdateGameRegistryCalledWithGameName_UpdateGameRegistry()
    {
        var existingGame = new Game
        {
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
        };
        ResolveGameProperties(existingGame);
        SetupGameRegistry(existingGame);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj",
            new Dictionary<string, string>
            {
                { "GameDisplayName", "UpdateGameRegistryTest" },
            });

        var success = project.Build(["UpdateGameRegistry"], [logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        var gameRegistry = LoadGameRegistry();
        Assert.AreEqual(1, gameRegistry.Games.Count);
        var game = gameRegistry.Games.First();
        Assert.AreEqual(existingGame.Path, game.Path);
        Assert.AreEqual("UpdateGameRegistryTest", game.DisplayName);
        Assert.IsNull(game.ModsPath);
        Assert.IsNull(game.Version);
        Assert.AreEqual(ModDeploymentMode.Copy, game.ModDeploymentMode);
        Assert.AreEqual(false, game.DeploySourceCode);
        Assert.AreEqual(DoorstopMode.Debugging, game.DoorstopMode);
        Assert.AreEqual(false, game.UseAlternateDoorstopDllName);
        Assert.AreEqual("Unity2018Test", game.GameName);
        Assert.AreEqual("Unity2018Test.exe", game.GameExecutableFileName);
        Assert.AreEqual("X64", game.Architecture);
        Assert.AreEqual("2018.4.36f1", game.UnityVersion);
        Assert.AreEqual(".NET 4.6", game.MonoProfile);
    }

    [TestMethod]
    public void WhenUpdateGameRegistryCalledWithGameNameAndVersion_UpdateGameRegistry()
    {
        var existingGame10 = new Game
        {
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
            Version = "1.0",
            DisplayName = "Game [1.0]",
        };
        var existingGame11 = new Game
        {
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.1"),
            Version = "1.1",
            DisplayName = "Game [1.1]",
        };
        ResolveGameProperties(existingGame10);
        ResolveGameProperties(existingGame11);
        SetupGameRegistry(existingGame10, existingGame11);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\SingleVersion\SingleVersion.csproj",
            new Dictionary<string, string>
            {
                { "GameDisplayName", "UpdateGameRegistryTest" },
            });

        var success = project.Build(["UpdateGameRegistry"], [logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        var gameRegistry = LoadGameRegistry();
        Assert.AreEqual(2, gameRegistry.Games.Count);
        var game10 = gameRegistry.Games.First(g => g.Version == "1.0");
        Assert.AreEqual(existingGame10.Path, game10.Path);
        Assert.AreEqual("UpdateGameRegistryTest", game10.DisplayName);
        Assert.IsNull(game10.ModsPath);
        Assert.AreEqual("1.0", game10.Version);
        Assert.AreEqual(ModDeploymentMode.Copy, game10.ModDeploymentMode);
        Assert.AreEqual(false, game10.DeploySourceCode);
        Assert.AreEqual(DoorstopMode.Debugging, game10.DoorstopMode);
        Assert.AreEqual(false, game10.UseAlternateDoorstopDllName);
        Assert.AreEqual("Unity2018Test", game10.GameName);
        Assert.AreEqual("Unity2018Test.exe", game10.GameExecutableFileName);
        Assert.AreEqual("X64", game10.Architecture);
        Assert.AreEqual("2018.4.36f1", game10.UnityVersion);
        Assert.AreEqual(".NET 4.6", game10.MonoProfile);
        var game11 = gameRegistry.Games.First(g => g.Version == "1.1");
        Assert.AreEqual(existingGame11.Path, game11.Path);
        Assert.AreEqual(existingGame11.DisplayName, game11.DisplayName);
        Assert.AreEqual(existingGame11.ModsPath, game11.ModsPath);
        Assert.AreEqual(existingGame11.Version, game11.Version);
        Assert.AreEqual(existingGame11.ModDeploymentMode, game11.ModDeploymentMode);
        Assert.AreEqual(existingGame11.DeploySourceCode, game11.DeploySourceCode);
        Assert.AreEqual(existingGame11.DoorstopMode, game11.DoorstopMode);
        Assert.AreEqual(existingGame11.UseAlternateDoorstopDllName, game11.UseAlternateDoorstopDllName);
        Assert.AreEqual(existingGame11.GameName, game11.GameName);
        Assert.AreEqual(existingGame11.GameExecutableFileName, game11.GameExecutableFileName);
        Assert.AreEqual(existingGame11.Architecture, game11.Architecture);
        Assert.AreEqual(existingGame11.UnityVersion, game11.UnityVersion);
        Assert.AreEqual(existingGame11.MonoProfile, game11.MonoProfile);
    }

    [TestMethod]
    public void WhenUpdateGameRegistryCalledWithGameNameAndVersionInMultiTargetProject_UpdateGameRegistry()
    {
        var existingGame10 = new Game
        {
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
            Version = "1.0",
            DisplayName = "Game [1.0]",
        };
        var existingGame11 = new Game
        {
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.1"),
            Version = "1.1",
            DisplayName = "Game [1.1]",
        };
        ResolveGameProperties(existingGame10);
        ResolveGameProperties(existingGame11);
        SetupGameRegistry(existingGame10, existingGame11);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\MultiVersionMultiTarget\MultiVersionMultiTarget.csproj",
            new Dictionary<string, string>
            {
                { "GameDisplayName", "UpdateGameRegistryTest" },
                { "GameVersion", "1.0" },
            });

        var success = project.Build(["UpdateGameRegistry"], [logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        var gameRegistry = LoadGameRegistry();
        Assert.AreEqual(2, gameRegistry.Games.Count);
        var game10 = gameRegistry.Games.First(g => g.Version == "1.0");
        Assert.AreEqual(existingGame10.Path, game10.Path);
        Assert.AreEqual("UpdateGameRegistryTest", game10.DisplayName);
        Assert.IsNull(game10.ModsPath);
        Assert.AreEqual("1.0", game10.Version);
        Assert.AreEqual(ModDeploymentMode.Copy, game10.ModDeploymentMode);
        Assert.AreEqual(false, game10.DeploySourceCode);
        Assert.AreEqual(DoorstopMode.Debugging, game10.DoorstopMode);
        Assert.AreEqual(false, game10.UseAlternateDoorstopDllName);
        Assert.AreEqual("Unity2018Test", game10.GameName);
        Assert.AreEqual("Unity2018Test.exe", game10.GameExecutableFileName);
        Assert.AreEqual("X64", game10.Architecture);
        Assert.AreEqual("2018.4.36f1", game10.UnityVersion);
        Assert.AreEqual(".NET 4.6", game10.MonoProfile);
        var game11 = gameRegistry.Games.First(g => g.Version == "1.1");
        Assert.AreEqual(existingGame11.Path, game11.Path);
        Assert.AreEqual(existingGame11.DisplayName, game11.DisplayName);
        Assert.AreEqual(existingGame11.ModsPath, game11.ModsPath);
        Assert.AreEqual(existingGame11.Version, game11.Version);
        Assert.AreEqual(existingGame11.ModDeploymentMode, game11.ModDeploymentMode);
        Assert.AreEqual(existingGame11.DeploySourceCode, game11.DeploySourceCode);
        Assert.AreEqual(existingGame11.DoorstopMode, game11.DoorstopMode);
        Assert.AreEqual(existingGame11.UseAlternateDoorstopDllName, game11.UseAlternateDoorstopDllName);
        Assert.AreEqual(existingGame11.GameName, game11.GameName);
        Assert.AreEqual(existingGame11.GameExecutableFileName, game11.GameExecutableFileName);
        Assert.AreEqual(existingGame11.Architecture, game11.Architecture);
        Assert.AreEqual(existingGame11.UnityVersion, game11.UnityVersion);
        Assert.AreEqual(existingGame11.MonoProfile, game11.MonoProfile);
    }

    [TestMethod]
    public void WhenUpdateGameRegistryCalledWithGameDisplayName_UpdateGameRegistry()
    {
        var existingGame = new Game
        {
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
            DisplayName = "UpdateGameRegistryTest",
        };
        ResolveGameProperties(existingGame);
        SetupGameRegistry(existingGame);
        var newGamePath = Path.Combine(SampleGameInfo.DownloadPath, "2018-netstandard20-v2.0");
        var (project, logger) = GetProjectWithRestore(@"Projects\Incorrect\NoGameProperties\NoGameProperties.csproj",
            new Dictionary<string, string>
            {
                { "GamePath", newGamePath },
                { "GameDisplayName", "UpdateGameRegistryTest" },
            });

        var success = project.Build(["UpdateGameRegistry"], [logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        var gameRegistry = LoadGameRegistry();
        Assert.AreEqual(1, gameRegistry.Games.Count);
        var game = gameRegistry.Games.First();
        Assert.AreEqual(newGamePath, game.Path);
        Assert.AreEqual("UpdateGameRegistryTest", game.DisplayName);
        Assert.IsNull(game.ModsPath);
        Assert.IsNull(game.Version);
        Assert.AreEqual(ModDeploymentMode.Copy, game.ModDeploymentMode);
        Assert.AreEqual(false, game.DeploySourceCode);
        Assert.AreEqual(DoorstopMode.Debugging, game.DoorstopMode);
        Assert.AreEqual(false, game.UseAlternateDoorstopDllName);
        Assert.AreEqual("Unity2018Test", game.GameName);
        Assert.AreEqual("Unity2018Test.exe", game.GameExecutableFileName);
        Assert.AreEqual("X64", game.Architecture);
        Assert.AreEqual("2018.4.36f1", game.UnityVersion);
        Assert.AreEqual(".NET Standard 2.0", game.MonoProfile);
    }

    [TestMethod]
    public void WhenRemoveGameFromRegistryCalledWithInvalidGameRegistryPath_ProduceError()
    {
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj",
            new Dictionary<string, string>
            {
                { "GameRegistryPath", "!@#$%^&*()" },
            });

        var success = project.Build(["RemoveGameFromRegistry"], [logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.AreEqual(1, logger.BuildErrors.Count);
        // Exception message may be localized, check only prefix.
        Assert.IsTrue(logger.BuildErrors[0].Message?.StartsWith("Unable to initialize game registry:"));
        Assert.AreEqual(0, logger.BuildWarnings.Count);
    }

    [TestMethod]
    public void WhenRemoveGameFromRegistryCalledWithoutAnyLookupProperty_ProduceError()
    {
        var (project, logger) = GetProjectWithRestore(@"Projects\Incorrect\NoGameProperties\NoGameProperties.csproj");

        var success = project.Build(["RemoveGameFromRegistry"], [logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.AreEqual(1, logger.BuildErrors.Count);
        Assert.AreEqual("No game registry entries match speified game properties.", logger.BuildErrors[0].Message);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
    }

    [TestMethod]
    public void WhenRemoveGameFromRegistryCalledWithNonExistingGameName_ProduceError()
    {
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj");

        var success = project.Build(["RemoveGameFromRegistry"], [logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.AreEqual(1, logger.BuildErrors.Count);
        Assert.AreEqual("No game registry entries match speified game properties.", logger.BuildErrors[0].Message);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
    }

    [TestMethod]
    public void WhenRemoveGameFromRegistryCalledWithGameInstanceId_RemoveGameFromRegistry()
    {
        var existingGame = new Game
        {
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
            DisplayName = "Game",
        };
        var existingGame2 = new Game
        {
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-netstandard20-v2.0"),
            DisplayName = "Game2",
        };
        ResolveGameProperties(existingGame);
        ResolveGameProperties(existingGame2);
        SetupGameRegistry(existingGame, existingGame2);
        var (project, logger) = GetProjectWithRestore(@"Projects\Incorrect\NoGameProperties\NoGameProperties.csproj",
            new Dictionary<string, string>
            {
                { "GameInstanceId", existingGame.Id.ToString() },
            });

        var success = project.Build(["RemoveGameFromRegistry"], [logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        var gameRegistry = LoadGameRegistry();
        Assert.AreEqual(1, gameRegistry.Games.Count);
        Assert.AreEqual(existingGame2.DisplayName, gameRegistry.Games.Single().DisplayName);
    }

    [TestMethod]
    public void WhenRemoveGameFromRegistryCalledWithGameName_RemoveGameFromRegistry()
    {
        var existingGame = new Game
        {
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
            DisplayName = "Game",
        };
        var existingGame2 = new Game
        {
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2022-net4"),
            DisplayName = "Game2",
        };
        ResolveGameProperties(existingGame);
        ResolveGameProperties(existingGame2);
        SetupGameRegistry(existingGame, existingGame2);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj");

        var success = project.Build(["RemoveGameFromRegistry"], [logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        var gameRegistry = LoadGameRegistry();
        Assert.AreEqual(1, gameRegistry.Games.Count);
        Assert.AreEqual(existingGame2.DisplayName, gameRegistry.Games.Single().DisplayName);
    }

    [TestMethod]
    public void WhenRemoveGameFromRegistryCalledWithGameNameAndVersion_RemoveGameFromRegistry()
    {
        var existingGame = new Game
        {
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
            Version = "1.0",
            DisplayName = "Game",
        };
        var existingGame2 = new Game
        {
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.1"),
            Version = "1.1",
            DisplayName = "Game2",
        };
        ResolveGameProperties(existingGame);
        ResolveGameProperties(existingGame2);
        SetupGameRegistry(existingGame, existingGame2);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\SingleVersion\SingleVersion.csproj");

        var success = project.Build(["RemoveGameFromRegistry"], [logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        var gameRegistry = LoadGameRegistry();
        Assert.AreEqual(1, gameRegistry.Games.Count);
        Assert.AreEqual(existingGame2.DisplayName, gameRegistry.Games.Single().DisplayName);
    }

    [TestMethod]
    public void WhenRemoveGameFromRegistryCalledWithGameDisplayName_RemoveGameFromRegistry()
    {
        var existingGame = new Game
        {
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
            DisplayName = "Game",
        };
        var existingGame2 = new Game
        {
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-netstandard20-v2.0"),
            DisplayName = "Game2",
        };
        ResolveGameProperties(existingGame);
        ResolveGameProperties(existingGame2);
        SetupGameRegistry(existingGame, existingGame2);
        var (project, logger) = GetProjectWithRestore(@"Projects\Incorrect\NoGameProperties\NoGameProperties.csproj",
            new Dictionary<string, string>
            {
                { "GameDisplayName", "Game" },
            });

        var success = project.Build(["RemoveGameFromRegistry"], [logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        var gameRegistry = LoadGameRegistry();
        Assert.AreEqual(1, gameRegistry.Games.Count);
        Assert.AreEqual(existingGame2.DisplayName, gameRegistry.Games.Single().DisplayName);
    }
}