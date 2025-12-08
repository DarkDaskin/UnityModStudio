using System.IO;
using Moq;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Options.Tests;

[TestClass]
public sealed class GamePropertiesViewModelTests : GameManagerTestBase
{
    [TestMethod]
    public void WhenCreatedWithValidGame_InitialStateIsCorrect()
    {
        var game = new Game
        {
            DisplayName = "Unity2018Test",
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
        };
        var vm = new GamePropertiesViewModel(game);

        Assert.IsNull(vm.GameManager);
        Assert.IsNull(vm.FolderBrowserService);
        Assert.AreEqual(game, vm.Game);
        Assert.AreEqual(game.DisplayName, vm.DisplayName);
        Assert.AreEqual(game.Path, vm.GamePath);
        Assert.AreEqual(game.ModsPath, vm.ModsPath);
        Assert.AreEqual(game.Version, vm.GameVersion);
        Assert.AreEqual(game.ModDeploymentMode, vm.ModDeploymentMode);
        Assert.AreEqual(game.DeploySourceCode, vm.DeploySourceCode);
        Assert.AreEqual(game.DoorstopMode, vm.DoorstopMode);
        Assert.AreEqual(game.UseAlternateDoorstopDllName, vm.UseAlternateDoorstopDllName);
        Assert.AreEqual("Unity2018Test", vm.GameName);
        Assert.AreEqual("X64", vm.Architecture);
        Assert.AreEqual("2018.4.36f1", vm.UnityVersion);
        Assert.AreEqual(".NET 4.7.2", vm.MonoProfile);
        Assert.AreEqual("net472", vm.TargetFrameworkMoniker);
        Assert.AreEqual("Unity2018Test.exe", vm.GameExecutableFileName);
        Assert.IsNotNull(vm.GameIcon);
        Assert.IsTrue(vm.HasValidGamePath);
        Assert.IsFalse(vm.HasErrors);
        Assert.IsTrue(vm.BrowseForGamePathCommand.CanExecute(null));
        Assert.IsTrue(vm.BrowseForModsPathCommand.CanExecute(null));
        Assert.IsTrue(vm.ConfirmCommand.CanExecute(null));
        Assert.IsTrue(vm.CancelCommand.CanExecute(null));
        Assert.IsTrue(vm.GetErrors(nameof(GamePropertiesViewModel.GamePath)).SequenceEqual([]));
    }

    [TestMethod]
    public void WhenCreatedWithInvalidGame_InitialStateIsCorrect()
    {
        var game = new Game
        {
            DisplayName = "NotAGame",
            Path = @"C:\Program Files\Windows Mail",
        };
        var vm = new GamePropertiesViewModel(game);

        Assert.IsNull(vm.GameManager);
        Assert.IsNull(vm.FolderBrowserService);
        Assert.AreEqual(game, vm.Game);
        Assert.AreEqual(game.DisplayName, vm.DisplayName);
        Assert.AreEqual(game.Path, vm.GamePath);
        Assert.AreEqual(game.ModsPath, vm.ModsPath);
        Assert.AreEqual(game.Version, vm.GameVersion);
        Assert.AreEqual(game.ModDeploymentMode, vm.ModDeploymentMode);
        Assert.AreEqual(game.DeploySourceCode, vm.DeploySourceCode);
        Assert.AreEqual(game.DoorstopMode, vm.DoorstopMode);
        Assert.AreEqual(game.UseAlternateDoorstopDllName, vm.UseAlternateDoorstopDllName);
        Assert.IsNull(vm.GameName);
        Assert.IsNull(vm.Architecture);
        Assert.IsNull(vm.UnityVersion);
        Assert.IsNull(vm.MonoProfile);
        Assert.IsNull(vm.TargetFrameworkMoniker);
        Assert.IsNull(vm.GameExecutableFileName);
        Assert.IsNull(vm.GameIcon);
        Assert.IsFalse(vm.HasValidGamePath);
        Assert.IsTrue(vm.HasErrors);
        Assert.IsTrue(vm.BrowseForGamePathCommand.CanExecute(null));
        Assert.IsTrue(vm.BrowseForModsPathCommand.CanExecute(null));
        Assert.IsFalse(vm.ConfirmCommand.CanExecute(null));
        Assert.IsTrue(vm.CancelCommand.CanExecute(null));
        Assert.IsTrue(vm.GetErrors(nameof(GamePropertiesViewModel.GamePath)).SequenceEqual(["Unable to determine game data directory."]));
    }

    [TestMethod]
    public void WhenGamePathIsUpdated_UpdateStateAndNotify()
    {
        var changedProperties = new List<string>();
        var changedErrorProperties = new List<string>();
        var game = new Game();
        var vm = new GamePropertiesViewModel(game) { GameManager = SetupGameManagerWithFind()};
        vm.PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);
        vm.ErrorsChanged += (sender, args) => changedErrorProperties.Add(args.PropertyName);

        var newPath = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0");
        vm.GamePath = newPath;

        Assert.AreEqual(game, vm.Game);
        Assert.AreEqual("Unity2018Test", vm.DisplayName);
        Assert.AreEqual(newPath, vm.GamePath);
        Assert.IsNull(vm.ModsPath);
        Assert.IsNull(vm.GameVersion);
        Assert.AreEqual(ModDeploymentMode.Copy, vm.ModDeploymentMode);
        Assert.AreEqual(false, vm.DeploySourceCode);
        Assert.AreEqual(DoorstopMode.Debugging, vm.DoorstopMode);
        Assert.AreEqual(false, vm.UseAlternateDoorstopDllName);
        Assert.AreEqual("Unity2018Test", vm.GameName);
        Assert.AreEqual("X64", vm.Architecture);
        Assert.AreEqual("2018.4.36f1", vm.UnityVersion);
        Assert.AreEqual(".NET 4.7.2", vm.MonoProfile);
        Assert.AreEqual("net472", vm.TargetFrameworkMoniker);
        Assert.AreEqual("Unity2018Test.exe", vm.GameExecutableFileName);
        Assert.IsNotNull(vm.GameIcon);
        Assert.IsTrue(vm.HasValidGamePath);
        Assert.IsFalse(vm.HasErrors);
        Assert.IsTrue(vm.GetErrors(nameof(GamePropertiesViewModel.GamePath)).SequenceEqual([]));
        Assert.IsTrue(changedProperties.ToHashSet().SetEquals([
            nameof(GamePropertiesViewModel.DisplayName),
            nameof(GamePropertiesViewModel.GamePath),
            nameof(GamePropertiesViewModel.GameName),
            nameof(GamePropertiesViewModel.Architecture),
            nameof(GamePropertiesViewModel.UnityVersion),
            nameof(GamePropertiesViewModel.MonoProfile),
            nameof(GamePropertiesViewModel.TargetFrameworkMoniker),
            nameof(GamePropertiesViewModel.GameExecutableFileName),
            nameof(GamePropertiesViewModel.GameIcon),
            nameof(GamePropertiesViewModel.HasValidGamePath),
            nameof(GamePropertiesViewModel.HasErrors),
        ]));
        Assert.IsTrue(changedErrorProperties.ToHashSet().SetEquals([
            nameof(GamePropertiesViewModel.DisplayName),
            nameof(GamePropertiesViewModel.GamePath),
        ]));
    }

    [TestMethod]
    public void WhenDisplayNameIsEmpty_ProduceError()
    {
        var game = new Game
        {
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
        };
        var vm = new GamePropertiesViewModel(game) { GameManager = SetupGameManagerWithFind() };

        Assert.IsTrue(vm.HasErrors);
        Assert.IsTrue(vm.GetErrors(nameof(GamePropertiesViewModel.DisplayName)).SequenceEqual(["Display name must not be empty."]));
    }

    [TestMethod]
    public void WhenDisplayNameIsNotUnique_ProduceError()
    {
        var game = new Game
        {
            DisplayName = "Unity2018Test",
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
        };
        var existingGame = new Game
        {
            DisplayName = "Unity2018Test",
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-netstandard20-v2.0"),
        };
        var vm = new GamePropertiesViewModel(game) { GameManager = SetupGameManagerWithFind(existingGame) };

        Assert.IsTrue(vm.HasErrors);
        Assert.IsTrue(vm.GetErrors(nameof(GamePropertiesViewModel.DisplayName)).SequenceEqual(["Display name must be unique."]));
    }

    [TestMethod]
    public void WhenGameVersionContainsInvalidChars_ProduceError()
    {
        var game = new Game
        {
            DisplayName = "Unity2018Test",
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
        };
        var vm = new GamePropertiesViewModel(game) { GameManager = SetupGameManagerWithFind() };

        vm.GameVersion = "Some*";

        Assert.IsTrue(vm.HasErrors);
        Assert.IsTrue(vm.GetErrors(nameof(GamePropertiesViewModel.GameVersion)).SequenceEqual(["Game version must not contain any of the following characters: \" < > | : * ? \\ /"]));
    }

    [TestMethod]
    public void WhenGameVersionIsNotUnique_ProduceError()
    {
        var game = new Game
        {
            DisplayName = "Game 1",
            GameName = "Unity2018Test",
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
        };
        var existingGame = new Game
        {
            DisplayName = "Game 2",
            GameName = "Unity2018Test",
            Version = "1",
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.1"),
        };
        var vm = new GamePropertiesViewModel(game) { GameManager = SetupGameManagerWithFind(existingGame) };

        vm.GameVersion = "1";

        Assert.IsTrue(vm.HasErrors);
        Assert.IsTrue(vm.GetErrors(nameof(GamePropertiesViewModel.GameVersion)).SequenceEqual(["Game version must be unique across games with same game name."]));
    }

    [TestMethod]
    public void WhenBrowseForGamePathIsInvoked_UpdateGamePath()
    {
        var oldPath = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0");
        var newPath = Path.Combine(SampleGameInfo.DownloadPath, "2018-netstandard20-v2.0");
        var game = new Game { Path = oldPath };
        var vm = new GamePropertiesViewModel(game)
        {
            GameManager = SetupGameManagerWithFind(),
            FolderBrowserService = Mock.Of<IFolderBrowserService>(fbs => 
                fbs.BrowseForFolderAsync("Select the game root folder", oldPath) == Task.FromResult(newPath)),
        };

        vm.BrowseForGamePathCommand.Execute(null);

        Assert.AreEqual(newPath, vm.GamePath);
        Assert.AreEqual("netstandard2.0", vm.TargetFrameworkMoniker);
        Mock.Get(vm.FolderBrowserService).VerifyAll();
        Mock.Get(vm.FolderBrowserService).VerifyNoOtherCalls();
    }

    [TestMethod]
    public void WhenBrowseForGamePathIsInvokedAndCancelled_DoNotUpdateGamePath()
    {
        var oldPath = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0");
        var game = new Game { Path = oldPath };
        var vm = new GamePropertiesViewModel(game)
        {
            GameManager = SetupGameManagerWithFind(),
            FolderBrowserService = Mock.Of<IFolderBrowserService>(fbs => 
                fbs.BrowseForFolderAsync("Select the game root folder", oldPath) == Task.FromResult<string?>(null)),
        };

        vm.BrowseForGamePathCommand.Execute(null);

        Assert.AreEqual(oldPath, vm.GamePath);
        Mock.Get(vm.FolderBrowserService).VerifyAll();
        Mock.Get(vm.FolderBrowserService).VerifyNoOtherCalls();
    }

    [TestMethod]
    public void WhenBrowseForModsPathIsInvoked_UpdateModsPath()
    {
        var game = new Game { Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0") };
        var vm = new GamePropertiesViewModel(game)
        {
            GameManager = SetupGameManagerWithFind(),
            FolderBrowserService = Mock.Of<IFolderBrowserService>(fbs => 
                fbs.BrowseForFolderAsync("Select the mods folder", game.Path) == Task.FromResult("Mods")),
        };

        vm.BrowseForModsPathCommand.Execute(null);

        Assert.AreEqual("Mods", vm.ModsPath);
        Mock.Get(vm.FolderBrowserService).VerifyAll();
        Mock.Get(vm.FolderBrowserService).VerifyNoOtherCalls();
    }

    [TestMethod]
    public void WhenBrowseForModsPathIsInvokedAndCancelled_DoNotUpdateModsPath()
    {
        var game = new Game { Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0") };
        var vm = new GamePropertiesViewModel(game)
        {
            GameManager = SetupGameManagerWithFind(),
            FolderBrowserService = Mock.Of<IFolderBrowserService>(fbs => 
                fbs.BrowseForFolderAsync("Select the mods folder", game.Path) == Task.FromResult<string?>(null)),
        };

        vm.BrowseForModsPathCommand.Execute(null);

        Assert.IsNull(vm.ModsPath);
        Mock.Get(vm.FolderBrowserService).VerifyAll();
        Mock.Get(vm.FolderBrowserService).VerifyNoOtherCalls();
    }

    [TestMethod]
    public void WhenConfirmIsInvoked_UpdateGameTrimValuesAndCloseWindow()
    {
        var closedInvocations = new List<bool>();
        var oldPath = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0");
        var newPath = Path.Combine(SampleGameInfo.DownloadPath, "2018-netstandard20-v2.0");
        var game = new Game
        {
            DisplayName = "Unity2018Test", 
            Path = oldPath,
            ModDeploymentMode = ModDeploymentMode.Copy,
            DeploySourceCode = true,
            DoorstopMode = DoorstopMode.Disabled,
            UseAlternateDoorstopDllName = false,
            GameName = "Unity2018Test",
            GameExecutableFileName = "Unity2018Test.exe",
            Architecture = "X64",
            UnityVersion = "2018.4.36f1",
            MonoProfile = ".NET 4.7.2",
        };
        var vm = new GamePropertiesViewModel(game) { GameManager = SetupGameManagerWithFind() };
        vm.Closed += success => closedInvocations.Add(success);

        vm.DisplayName = "New name ";
        vm.GameVersion = "1.0 ";
        vm.GamePath = newPath;
        vm.ModsPath = "Mods ";
        vm.ModDeploymentMode = ModDeploymentMode.Link;
        vm.DeploySourceCode = false;
        vm.DoorstopMode = DoorstopMode.Debugging;
        vm.UseAlternateDoorstopDllName = true;
        vm.ConfirmCommand.Execute(null);

        Assert.AreEqual("New name", game.DisplayName);
        Assert.AreEqual("1.0", game.Version);
        Assert.AreEqual(newPath, game.Path);
        Assert.AreEqual("Mods", game.ModsPath);
        Assert.AreEqual(ModDeploymentMode.Link, game.ModDeploymentMode);
        Assert.AreEqual(false, game.DeploySourceCode);
        Assert.AreEqual(DoorstopMode.Debugging, game.DoorstopMode);
        Assert.AreEqual(true, game.UseAlternateDoorstopDllName);
        Assert.AreEqual("Unity2018Test", game.GameName);
        Assert.AreEqual("Unity2018Test.exe", game.GameExecutableFileName);
        Assert.AreEqual("X64", game.Architecture);
        Assert.AreEqual("2018.4.36f1", game.UnityVersion);
        Assert.AreEqual(".NET Standard 2.0", game.MonoProfile);
        Assert.IsTrue(closedInvocations.SequenceEqual([true]));
    }

    [TestMethod]
    public void WhenCancelIsInvoked_DoNotUpdateGameAndCloseWindow()
    {
        var closedInvocations = new List<bool>();
        var oldPath = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0");
        var newPath = Path.Combine(SampleGameInfo.DownloadPath, "2018-netstandard20-v2.0");
        var game = new Game
        {
            DisplayName = "Unity2018Test",
            Path = oldPath,
            ModDeploymentMode = ModDeploymentMode.Copy,
            DeploySourceCode = true,
            DoorstopMode = DoorstopMode.Disabled,
            UseAlternateDoorstopDllName = false,
            GameName = "Unity2018Test",
            GameExecutableFileName = "Unity2018Test.exe",
            Architecture = "X64",
            UnityVersion = "2018.4.36f1",
            MonoProfile = ".NET 4.7.2",
        };
        var vm = new GamePropertiesViewModel(game) { GameManager = SetupGameManagerWithFind() };
        vm.Closed += success => closedInvocations.Add(success);

        vm.DisplayName = "New name";
        vm.GameVersion = "1.0";
        vm.GamePath = newPath;
        vm.ModsPath = "Mods";
        vm.ModDeploymentMode = ModDeploymentMode.Link;
        vm.DeploySourceCode = false;
        vm.DoorstopMode = DoorstopMode.Debugging;
        vm.UseAlternateDoorstopDllName = true;
        vm.CancelCommand.Execute(null);

        Assert.AreEqual("Unity2018Test", game.DisplayName);
        Assert.IsNull(game.Version);
        Assert.AreEqual(oldPath, game.Path);
        Assert.IsNull(game.ModsPath);
        Assert.AreEqual(ModDeploymentMode.Copy, game.ModDeploymentMode);
        Assert.AreEqual(true, game.DeploySourceCode);
        Assert.AreEqual(DoorstopMode.Disabled, game.DoorstopMode);
        Assert.AreEqual(false, game.UseAlternateDoorstopDllName);
        Assert.AreEqual("Unity2018Test", game.GameName);
        Assert.AreEqual("Unity2018Test.exe", game.GameExecutableFileName);
        Assert.AreEqual("X64", game.Architecture);
        Assert.AreEqual("2018.4.36f1", game.UnityVersion);
        Assert.AreEqual(".NET 4.7.2", game.MonoProfile);
        Assert.IsTrue(closedInvocations.SequenceEqual([false]));
    }

    private static IGameManager SetupGameManagerWithFind(params Game[] games)
    {
        var gameManager = SetupGameManager(games);
        Mock.Get(gameManager.GameRegistry)
            .Setup(gameRegistry => gameRegistry.FindGamesByDisplayName(It.IsAny<string>()))
            .Returns((string displayName) => gameManager.GameRegistry.Games.Where(game => game.DisplayName == displayName).ToArray());
        Mock.Get(gameManager.GameRegistry)
            .Setup(gameRegistry => gameRegistry.FindGamesByGameNameAndVersion(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string? gameName, string? version) => gameManager.GameRegistry.Games.Where(game => game.GameName == gameName && game.Version == version).ToArray());
        return gameManager;
    }
}