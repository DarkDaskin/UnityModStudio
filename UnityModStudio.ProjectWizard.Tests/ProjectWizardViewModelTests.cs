using System.Collections.Specialized;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Data;
using Moq;
using UnityModStudio.Common.Options;
using UnityModStudio.Options;
using UnityModStudio.Options.Tests;

namespace UnityModStudio.ProjectWizard.Tests;

[TestClass]
public sealed class ProjectWizardViewModelTests : GameManagerTestBase
{
    [TestMethod]
    public void WhenCreated_InitialStateIsCorrect()
    {
        var vm = new ProjectWizardViewModel();

        Assert.IsNull(vm.GameManager);
        Assert.AreEqual("", vm.Error);
        Assert.AreEqual(Visibility.Visible, vm.ErrorVisibility);
        Assert.AreEqual(Visibility.Hidden, vm.GameInformationVisibility);
        Assert.IsTrue(vm.Games.SequenceEqual([]));
        Assert.AreEqual("", vm.ModDeploymentModeString);
        Assert.AreEqual("", vm.DeploySourceCodeString);
        Assert.AreEqual("", vm.DoorstopModeString);
        Assert.AreEqual("", vm.DoorstopDllName);
        Assert.IsFalse(vm.IsDoorstopDllNameVisible);
        Assert.IsTrue(vm.GameVersions.SequenceEqual([]));
        Assert.IsFalse(vm.IsMultiVersionPanelVisible);
        Assert.AreEqual("<default>", vm.GameVersionString);
        Assert.IsNull(vm.Game);
        Assert.IsNull(vm.GamePath);
        Assert.IsNull(vm.ModsPath);
        Assert.IsNull(vm.GameVersion);
        Assert.IsNull(vm.GameName);
        Assert.IsNull(vm.Architecture);
        Assert.IsNull(vm.UnityVersion);
        Assert.IsNull(vm.MonoProfile);
        Assert.IsNull(vm.TargetFrameworkMoniker);
        Assert.IsNull(vm.GameExecutableFileName);
        Assert.IsNull(vm.GameIcon);
        Assert.IsFalse(vm.HasValidGamePath);
        Assert.IsFalse(vm.HasErrors);
        Assert.IsTrue(vm.NewGameCommand.CanExecute(null));
        Assert.IsFalse(vm.UpdateGameCommand.CanExecute(null));
        Assert.IsFalse(vm.ConfirmCommand.CanExecute(null));
        Assert.IsTrue(vm.CancelCommand.CanExecute(null));
    }

    [TestMethod]
    public void WhenInitialized_InitialStateIsCorrect()
    {
        var game = new Game
        {
            DisplayName = "Game 1"
        };
        var vm = new ProjectWizardViewModel { GameManager = SetupGameManager(game) };

        Assert.AreEqual("", vm.Error);
        Assert.AreEqual(Visibility.Visible, vm.ErrorVisibility);
        Assert.AreEqual(Visibility.Hidden, vm.GameInformationVisibility);
        Assert.IsTrue(vm.Games.SequenceEqual([game]));
        Assert.AreEqual("", vm.ModDeploymentModeString);
        Assert.AreEqual("", vm.DeploySourceCodeString);
        Assert.AreEqual("", vm.DoorstopModeString);
        Assert.AreEqual("", vm.DoorstopDllName);
        Assert.IsFalse(vm.IsDoorstopDllNameVisible);
        Assert.IsTrue(vm.GameVersions.SequenceEqual([]));
        Assert.IsFalse(vm.IsMultiVersionPanelVisible);
        Assert.AreEqual("<default>", vm.GameVersionString);
        Assert.IsNull(vm.Game);
        Assert.IsNull(vm.GamePath);
        Assert.IsNull(vm.ModsPath);
        Assert.IsNull(vm.GameVersion);
        Assert.IsNull(vm.GameName);
        Assert.IsNull(vm.Architecture);
        Assert.IsNull(vm.UnityVersion);
        Assert.IsNull(vm.MonoProfile);
        Assert.IsNull(vm.TargetFrameworkMoniker);
        Assert.IsNull(vm.GameExecutableFileName);
        Assert.IsNull(vm.GameIcon);
        Assert.IsFalse(vm.HasValidGamePath);
        Assert.IsFalse(vm.HasErrors);
        Assert.IsTrue(vm.NewGameCommand.CanExecute(null));
        Assert.IsFalse(vm.UpdateGameCommand.CanExecute(null));
        Assert.IsFalse(vm.ConfirmCommand.CanExecute(null));
        Assert.IsTrue(vm.CancelCommand.CanExecute(null));
    }

    [TestMethod]
    public void WhenValidGameIsSelected_ShowGameInfoAndNotify()
    {
        var changedProperties = new List<string>();
        var changedErrorProperties = new List<string>();
        var game = new Game
        {
            DisplayName = "Unity2018Test",
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
        };
        var vm = new ProjectWizardViewModel { GameManager = SetupGameManager(game) };
        vm.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);
        vm.ErrorsChanged += (_, args) => changedErrorProperties.Add(args.PropertyName);

        vm.Game = game;

        Assert.AreEqual("", vm.Error);
        Assert.AreEqual(Visibility.Collapsed, vm.ErrorVisibility);
        Assert.AreEqual(Visibility.Visible, vm.GameInformationVisibility);
        Assert.IsTrue(vm.Games.SequenceEqual([game]));
        Assert.AreEqual("Copy", vm.ModDeploymentModeString);
        Assert.AreEqual("No", vm.DeploySourceCodeString);
        Assert.AreEqual("Use for debugging", vm.DoorstopModeString);
        Assert.AreEqual("winhttp.dll", vm.DoorstopDllName);
        Assert.IsTrue(vm.IsDoorstopDllNameVisible);
        Assert.IsTrue(vm.GameVersions.SequenceEqual([]));
        Assert.IsFalse(vm.IsMultiVersionPanelVisible);
        Assert.AreEqual("<default>", vm.GameVersionString);
        Assert.AreEqual(game.Path, vm.GamePath);
        Assert.IsNull(vm.ModsPath);
        Assert.IsNull(vm.GameVersion);
        Assert.AreEqual("Unity2018Test", vm.GameName);
        Assert.AreEqual("X64", vm.Architecture);
        Assert.AreEqual("2018.4.36f1", vm.UnityVersion);
        Assert.AreEqual(".NET 4.6", vm.MonoProfile);
        Assert.AreEqual("net46", vm.TargetFrameworkMoniker);
        Assert.AreEqual("Unity2018Test.exe", vm.GameExecutableFileName);
        Assert.IsNotNull(vm.GameIcon);
        Assert.IsTrue(vm.HasValidGamePath);
        Assert.IsFalse(vm.HasErrors);
        Assert.IsTrue(vm.NewGameCommand.CanExecute(null));
        Assert.IsTrue(vm.UpdateGameCommand.CanExecute(null));
        Assert.IsTrue(vm.ConfirmCommand.CanExecute(null));
        Assert.IsTrue(vm.CancelCommand.CanExecute(null));
        // TODO: ensure there are no excess notifications
        Assert.IsTrue(changedProperties.ToHashSet().IsSupersetOf([
            nameof(ProjectWizardViewModel.Game),
            nameof(ProjectWizardViewModel.Error),
            nameof(ProjectWizardViewModel.ErrorVisibility),
            nameof(ProjectWizardViewModel.GameInformationVisibility),
            nameof(ProjectWizardViewModel.ModDeploymentModeString),
            nameof(ProjectWizardViewModel.DeploySourceCodeString),
            nameof(ProjectWizardViewModel.DoorstopModeString),
            nameof(ProjectWizardViewModel.DoorstopDllName),
            nameof(ProjectWizardViewModel.IsDoorstopDllNameVisible),
            nameof(ProjectWizardViewModel.GamePath),
            nameof(ProjectWizardViewModel.GameName),
            nameof(ProjectWizardViewModel.Architecture),
            nameof(ProjectWizardViewModel.UnityVersion),
            nameof(ProjectWizardViewModel.MonoProfile),
            nameof(ProjectWizardViewModel.TargetFrameworkMoniker),
            nameof(ProjectWizardViewModel.GameExecutableFileName),
            nameof(ProjectWizardViewModel.GameIcon),
            nameof(ProjectWizardViewModel.HasValidGamePath),
        ]));
        Assert.IsTrue(changedErrorProperties.ToHashSet().SetEquals([nameof(ProjectWizardViewModel.GamePath)]));
    }

    [TestMethod]
    public void WhenInvalidGameIsSelected_ShowErrorAndNotify()
    {
        var changedProperties = new List<string>();
        var changedErrorProperties = new List<string>();
        var game = new Game
        {
            DisplayName = "NotAGame",
            Path = @"C:\Program Files\Windows Mail",
        };
        var vm = new ProjectWizardViewModel { GameManager = SetupGameManager(game) };
        vm.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);
        vm.ErrorsChanged += (_, args) => changedErrorProperties.Add(args.PropertyName);

        vm.Game = game;

        Assert.AreEqual("Unable to determine game data directory.", vm.Error);
        Assert.AreEqual(Visibility.Visible, vm.ErrorVisibility);
        Assert.AreEqual(Visibility.Hidden, vm.GameInformationVisibility);
        Assert.IsTrue(vm.Games.SequenceEqual([game]));
        Assert.AreEqual("Copy", vm.ModDeploymentModeString);
        Assert.AreEqual("No", vm.DeploySourceCodeString);
        Assert.AreEqual("Use for debugging", vm.DoorstopModeString);
        Assert.AreEqual("winhttp.dll", vm.DoorstopDllName);
        Assert.IsTrue(vm.IsDoorstopDllNameVisible);
        Assert.IsTrue(vm.GameVersions.SequenceEqual([]));
        Assert.IsFalse(vm.IsMultiVersionPanelVisible);
        Assert.AreEqual("<default>", vm.GameVersionString);
        Assert.AreEqual(game.Path, vm.GamePath);
        Assert.IsNull(vm.ModsPath);
        Assert.IsNull(vm.GameVersion);
        Assert.IsNull(vm.GameName);
        Assert.IsNull(vm.Architecture);
        Assert.IsNull(vm.UnityVersion);
        Assert.IsNull(vm.MonoProfile);
        Assert.IsNull(vm.TargetFrameworkMoniker);
        Assert.IsNull(vm.GameExecutableFileName);
        Assert.IsNull(vm.GameIcon);
        Assert.IsFalse(vm.HasValidGamePath);
        Assert.IsTrue(vm.HasErrors);
        Assert.IsTrue(vm.NewGameCommand.CanExecute(null));
        Assert.IsTrue(vm.UpdateGameCommand.CanExecute(null));
        Assert.IsFalse(vm.ConfirmCommand.CanExecute(null));
        Assert.IsTrue(vm.CancelCommand.CanExecute(null));
        // TODO: ensure there are no excess notifications
        Assert.IsTrue(changedProperties.ToHashSet().IsSupersetOf([
            nameof(ProjectWizardViewModel.Game),
            nameof(ProjectWizardViewModel.Error),
            nameof(ProjectWizardViewModel.ErrorVisibility),
            nameof(ProjectWizardViewModel.GameInformationVisibility),
        ]));
        Assert.IsTrue(changedErrorProperties.ToHashSet().SetEquals([nameof(ProjectWizardViewModel.GamePath)]));
    }

    [TestMethod]
    public void WhenMultiVersionGameIsSelected_ShowMultiVersionPanelAndNotify()
    {
        var changedProperties = new List<string>();
        var game1 = new Game
        {
            DisplayName = "Unity2018Test [2.0]",
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-netstandard20-v2.0"),
            GameName = "Unity2018Test",
            Version = "2.0",
        };
        var game2 = new Game
        {
            DisplayName = "Unity2018Test [1.0]",
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
            GameName = "Unity2018Test",
            Version = "1.0",
        };
        var vm = new ProjectWizardViewModel { GameManager = SetupGameManager(game1, game2) };
        vm.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

        vm.Game = game1;

        Assert.IsTrue(vm.Games.SequenceEqual([game1, game2]));
        Assert.AreEqual(2, vm.GameVersions.Count);
        Assert.AreEqual("1.0", vm.GameVersions[0].Version);
        Assert.IsTrue(vm.GameVersions[0].IsEnabled);
        Assert.IsFalse(vm.GameVersions[0].IsSelected);
        Assert.AreEqual("2.0", vm.GameVersions[1].Version);
        Assert.IsFalse(vm.GameVersions[1].IsEnabled);
        Assert.IsTrue(vm.GameVersions[1].IsSelected);
        Assert.IsTrue(vm.IsMultiVersionPanelVisible);
        Assert.AreEqual("2.0", vm.GameVersionString);
        Assert.AreEqual("2.0", vm.GameVersion);
        Assert.IsTrue(vm.HasValidGamePath);
        Assert.IsFalse(vm.HasErrors);
        Assert.IsTrue(changedProperties.ToHashSet().IsSupersetOf([
            nameof(ProjectWizardViewModel.Game),
            nameof(ProjectWizardViewModel.GameVersion),
            nameof(ProjectWizardViewModel.GameVersionString),
            nameof(ProjectWizardViewModel.GameVersions),
            nameof(ProjectWizardViewModel.IsMultiVersionPanelVisible),
        ]));
    }

    [TestMethod]
    public void WhenNewGameIsInvoked_ShowDialogAndAddGameAndNotify()
    {
        var changedProperties = new List<string>();
        var gamesChangeNotifications = new List<NotifyCollectionChangedEventArgs>();
        var games = new List<Game>();
        var vm = new ProjectWizardViewModel { GameManager = SetupGameManagerWithLoad() };
        vm.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);
        Mock.Get(vm.GameManager)
            .Setup(gameManager => gameManager.ShowEditDialog(It.IsNotNull<Game>()))
            .Returns(true)
            .Callback((Game game) =>
            {
                game.DisplayName = "Unity2018Test";
                game.Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0");
            });
        Mock.Get(vm.GameManager.GameRegistry)
            .Setup(gameRegistry => gameRegistry.AddGame(It.IsNotNull<Game>()))
            .Callback((Game gameToAdd) => games.Add(gameToAdd));
        Mock.Get(vm.GameManager.GameRegistry)
            .SetupGet(gameRegistry => gameRegistry.Games)
            .Returns(games);
        CollectionViewSource.GetDefaultView(vm.Games).CollectionChanged += (_, args) => gamesChangeNotifications.Add(args);

        vm.NewGameCommand.Execute(null);

        var game = games.FirstOrDefault();
        Assert.IsNotNull(game);
        Assert.IsTrue(vm.Games.SequenceEqual([game]));
        Assert.AreEqual(game, vm.Game);
        Assert.AreEqual("Unity2018Test", vm.GameName);
        Assert.IsTrue(vm.HasValidGamePath);
        Assert.IsFalse(vm.HasErrors);
        Assert.IsTrue(changedProperties.ToHashSet().IsSupersetOf([
            nameof(ProjectWizardViewModel.Game),
            nameof(ProjectWizardViewModel.GamePath),
            nameof(ProjectWizardViewModel.HasValidGamePath),
        ]));
        Assert.AreEqual(1, gamesChangeNotifications.Count);
        Assert.AreEqual(NotifyCollectionChangedAction.Reset, gamesChangeNotifications[0].Action);
        Mock.Get(vm.GameManager).VerifyAll();
        Mock.Get(vm.GameManager).VerifyNoOtherCalls();
    }

    [TestMethod]
    public void WhenNewGameIsInvokedAndDialogCancelled_DoNotAddGameAndKeepSelection()
    {
        var changedProperties = new List<string>();
        var game = new Game
        {
            DisplayName = "Unity2018Test",
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
        };
        var vm = new ProjectWizardViewModel { GameManager = SetupGameManagerWithLoad(game), Game = game };
        vm.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);
        Mock.Get(vm.GameManager)
            .Setup(gameManager => gameManager.ShowEditDialog(It.IsNotNull<Game>()))
            .Returns(false);

        vm.NewGameCommand.Execute(null);

        Assert.IsTrue(vm.Games.SequenceEqual([game]));
        Assert.AreEqual(game, vm.Game);
        Assert.IsTrue(vm.HasValidGamePath);
        Assert.IsFalse(vm.HasErrors);
        Assert.AreEqual(0, changedProperties.Count);
        Mock.Get(vm.GameManager).VerifyAll();
        Mock.Get(vm.GameManager).VerifyNoOtherCalls();
    }

    [TestMethod]
    public void WhenUpdateGameIsInvoked_ShowDialogAndUpdateGameAndNotify()
    {
        var gamesChangeNotifications = new List<NotifyCollectionChangedEventArgs>();
        var game = new Game
        {
            DisplayName = "Unity2018Test",
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
        };
        var vm = new ProjectWizardViewModel { GameManager = SetupGameManagerWithLoad(game), Game = game };
        CollectionViewSource.GetDefaultView(vm.Games).CollectionChanged += (_, args) => gamesChangeNotifications.Add(args);
        Mock.Get(vm.GameManager)
            .Setup(gameManager => gameManager.ShowEditDialog(It.IsNotNull<Game>()))
            .Returns(true)
            .Callback((Game gameToUpdate) => gameToUpdate.DisplayName = "New name");

        vm.UpdateGameCommand.Execute(null);

        Assert.IsTrue(vm.Games.SequenceEqual([game]));
        Assert.AreEqual(game, vm.Game);
        Assert.AreEqual("New name", game.DisplayName);
        Assert.IsTrue(vm.HasValidGamePath);
        Assert.IsFalse(vm.HasErrors);
        Assert.AreEqual(1, gamesChangeNotifications.Count);
        Assert.AreEqual(NotifyCollectionChangedAction.Reset, gamesChangeNotifications[0].Action);
        Mock.Get(vm.GameManager).VerifyAll();
        Mock.Get(vm.GameManager).VerifyNoOtherCalls();
    }

    [TestMethod]
    public void WhenUpdateGameIsInvokedAndDialogCancelled_DoNotUpdateGameAndKeepSelection()
    {
        var changedProperties = new List<string>();
        var gamesChangeNotifications = new List<NotifyCollectionChangedEventArgs>();
        var game = new Game
        {
            DisplayName = "Unity2018Test",
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
        };
        var vm = new ProjectWizardViewModel { GameManager = SetupGameManagerWithLoad(game), Game = game };
        vm.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);
        CollectionViewSource.GetDefaultView(vm.Games).CollectionChanged += (_, args) => gamesChangeNotifications.Add(args);
        Mock.Get(vm.GameManager)
            .Setup(gameManager => gameManager.ShowEditDialog(It.IsNotNull<Game>()))
            .Returns(false);

        vm.UpdateGameCommand.Execute(null);

        Assert.IsTrue(vm.Games.SequenceEqual([game]));
        Assert.AreEqual(game, vm.Game);
        Assert.IsTrue(vm.HasValidGamePath);
        Assert.IsFalse(vm.HasErrors);
        Assert.AreEqual(0, changedProperties.Count);
        Assert.AreEqual(0, gamesChangeNotifications.Count);
        Mock.Get(vm.GameManager).VerifyAll();
        Mock.Get(vm.GameManager).VerifyNoOtherCalls();
    }

    [TestMethod]
    public void WhenConfirmIsInvoked_SaveGameRegistryAndCloseWindow()
    {
        var closedInvocations = new List<bool>();
        var game = new Game
        {
            DisplayName = "Unity2018Test",
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
        };
        var vm = new ProjectWizardViewModel { GameManager = SetupGameManagerWithLoad(game), Game = game };
        vm.Closed += success => closedInvocations.Add(success);
        Mock.Get(vm.GameManager.GameRegistry).Setup(gameRegistry => gameRegistry.SaveAsync()).Returns(Task.CompletedTask);

        vm.ConfirmCommand.Execute(null);

        Assert.IsTrue(closedInvocations.SequenceEqual([true]));
        Mock.Get(vm.GameManager).VerifyAll();
        Mock.Get(vm.GameManager).VerifyNoOtherCalls();
    }

    [TestMethod]
    public void WhenCancelIsInvoked_DoNotSaveGameRegistryAndCloseWindow()
    {
        var closedInvocations = new List<bool>();
        var game = new Game
        {
            DisplayName = "Unity2018Test",
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
        };
        var vm = new ProjectWizardViewModel { GameManager = SetupGameManagerWithLoad(game), Game = game };
        vm.Closed += success => closedInvocations.Add(success);

        vm.CancelCommand.Execute(null);

        Assert.IsTrue(closedInvocations.SequenceEqual([false]));
        Mock.Get(vm.GameManager.GameRegistry).Verify(gameRegistry => gameRegistry.SaveAsync(), Times.Never);
        // Mock.Get(vm.GameManager).VerifyNoOtherCalls();
    }

    private static IGameManager SetupGameManagerWithLoad(params Game[] games)
    {
        var gameManager = SetupGameManager(games);
        Mock.Get(gameManager.GameRegistry)
            .Setup(gameRegistry => gameRegistry.LoadAsync())
            .Returns(Task.CompletedTask);
        return gameManager;
    }
}
