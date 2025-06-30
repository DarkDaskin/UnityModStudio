using System.Collections.Specialized;
using Moq;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Options.Tests;

[TestClass]
public sealed class GameRegistryViewModelTests : GameManagerTestBase
{
    [TestMethod]
    public void WhenCreated_InitialStateIsCorrect()
    {
        var vm = new GameRegistryViewModel();

        Assert.IsNull(vm.GameManager);
        Assert.IsTrue(vm.AddGameCommand.CanExecute(null));
        Assert.IsFalse(vm.UpdateGameCommand.CanExecute(null));
        Assert.IsFalse(vm.RemoveGameCommand.CanExecute(null));
        Assert.IsTrue(vm.ImportFromRegistryCommand.CanExecute(null));
        Assert.IsTrue(vm.ImportFromSteamCommand.CanExecute(null));
        Assert.IsTrue(vm.Games.SequenceEqual([]));
    }

    [TestMethod]
    public void WhenInitialized_StateIsCorrect()
    {
        var gameCollectionChangedNotifications = new List<NotifyCollectionChangedEventArgs>();
        var vm = new GameRegistryViewModel();
        vm.Games.CollectionChanged += (sender, args) => gameCollectionChangedNotifications.Add(args);
        var game = new Game
        {
            DisplayName = "Game 1"
        };

        vm.GameManager = SetupGameManager(game);

        Assert.IsTrue(vm.AddGameCommand.CanExecute(null));
        Assert.IsFalse(vm.UpdateGameCommand.CanExecute(null));
        Assert.IsFalse(vm.RemoveGameCommand.CanExecute(null));
        Assert.IsTrue(vm.ImportFromRegistryCommand.CanExecute(null));
        Assert.IsTrue(vm.ImportFromSteamCommand.CanExecute(null));
        Assert.IsTrue(vm.Games.SequenceEqual([game]));
        Assert.AreEqual(1, gameCollectionChangedNotifications.Count);
        Assert.AreEqual(NotifyCollectionChangedAction.Reset, gameCollectionChangedNotifications[0].Action);
    }

    [TestMethod]
    public void WhenGameIsSelected_UpdateCommands()
    {
        var vm = new GameRegistryViewModel();
        var game = new Game
        {
            DisplayName = "Game 1"
        };
        vm.GameManager = SetupGameManager(game);

        Assert.IsTrue(vm.UpdateGameCommand.CanExecute(game));
        Assert.IsTrue(vm.RemoveGameCommand.CanExecute(game));
    }

    [TestMethod]
    public void WhenAddGameInvoked_ShowDialogAndAddGame()
    {
        var gameCollectionChangedNotifications = new List<NotifyCollectionChangedEventArgs>();
        var vm = new GameRegistryViewModel { GameManager = SetupGameManager() };
        vm.Games.CollectionChanged += (sender, args) => gameCollectionChangedNotifications.Add(args);
        Mock.Get(vm.GameManager).Setup(gameManager => gameManager.ShowEditDialog(It.IsNotNull<Game>())).Returns(true).Callback((Game game) => game.DisplayName = "Game 1");
        Mock.Get(vm.GameManager.GameRegistry).Setup(gameRegistry => gameRegistry.AddGame(It.IsNotNull<Game>()));

        vm.AddGameCommand.Execute(null);

        Assert.AreEqual(1, vm.Games.Count);
        Assert.AreEqual("Game 1", vm.Games[0].DisplayName);
        Assert.AreEqual(1, gameCollectionChangedNotifications.Count);
        Assert.AreEqual(NotifyCollectionChangedAction.Add, gameCollectionChangedNotifications[0].Action);
        Mock.Get(vm.GameManager).VerifyAll();
        Mock.Get(vm.GameManager).VerifyNoOtherCalls();
        Mock.Get(vm.GameManager.GameRegistry).VerifyAll();
        Mock.Get(vm.GameManager.GameRegistry).VerifyNoOtherCalls();
    }

    [TestMethod]
    public void WhenAddGameInvokedAndDialogCancelled_DoNotAddGame()
    {
        var gameCollectionChangedNotifications = new List<NotifyCollectionChangedEventArgs>();
        var vm = new GameRegistryViewModel { GameManager = SetupGameManager() };
        vm.Games.CollectionChanged += (sender, args) => gameCollectionChangedNotifications.Add(args);
        Mock.Get(vm.GameManager).Setup(gameManager => gameManager.ShowEditDialog(It.IsNotNull<Game>())).Returns(false);

        vm.AddGameCommand.Execute(null);

        Assert.AreEqual(0, vm.Games.Count);
        Assert.AreEqual(0, gameCollectionChangedNotifications.Count);
        Mock.Get(vm.GameManager).VerifyAll();
        Mock.Get(vm.GameManager).VerifyNoOtherCalls();
        Mock.Get(vm.GameManager.GameRegistry).VerifyAll();
        Mock.Get(vm.GameManager.GameRegistry).VerifyNoOtherCalls();
    }

    [TestMethod]
    public void WhenUpdateGameInvoked_ShowDialogAndUpdateGame()
    {
        var game1 = new Game
        {
            DisplayName = "Game 1"
        };
        var gameCollectionChangedNotifications = new List<NotifyCollectionChangedEventArgs>();
        var vm = new GameRegistryViewModel { GameManager = SetupGameManager(game1) };
        vm.Games.CollectionChanged += (sender, args) => gameCollectionChangedNotifications.Add(args);
        Mock.Get(vm.GameManager).Setup(gameManager => gameManager.ShowEditDialog(It.IsNotNull<Game>())).Returns(true).Callback((Game game) => game.DisplayName = "Game 2");

        vm.UpdateGameCommand.Execute(game1);

        Assert.AreEqual(1, vm.Games.Count);
        Assert.AreEqual("Game 2", vm.Games[0].DisplayName);
        Assert.AreEqual(2, gameCollectionChangedNotifications.Count);
        Assert.AreEqual(NotifyCollectionChangedAction.Remove, gameCollectionChangedNotifications[0].Action);
        Assert.AreEqual(NotifyCollectionChangedAction.Add, gameCollectionChangedNotifications[1].Action);
        Mock.Get(vm.GameManager).VerifyAll();
        Mock.Get(vm.GameManager).VerifyNoOtherCalls();
        Mock.Get(vm.GameManager.GameRegistry).VerifyNoOtherCalls();
    }

    [TestMethod]
    public void WhenUpdateGameInvokedAndDialogCancelled_DoNotUpdateGame()
    {
        var game1 = new Game
        {
            DisplayName = "Game 1"
        };
        var gameCollectionChangedNotifications = new List<NotifyCollectionChangedEventArgs>();
        var vm = new GameRegistryViewModel { GameManager = SetupGameManager(game1) };
        vm.Games.CollectionChanged += (sender, args) => gameCollectionChangedNotifications.Add(args);
        Mock.Get(vm.GameManager).Setup(gameManager => gameManager.ShowEditDialog(It.IsNotNull<Game>())).Returns(false);

        vm.UpdateGameCommand.Execute(game1);

        Assert.AreEqual(1, vm.Games.Count);
        Assert.AreEqual("Game 1", vm.Games[0].DisplayName);
        Assert.AreEqual(0, gameCollectionChangedNotifications.Count);
        Mock.Get(vm.GameManager).VerifyAll();
        Mock.Get(vm.GameManager).VerifyNoOtherCalls();
        Mock.Get(vm.GameManager.GameRegistry).VerifyNoOtherCalls();
    }

    [TestMethod]
    public void WhenRemoveGameInvoked_RemoveGame()
    {
        var game1 = new Game
        {
            DisplayName = "Game 1"
        };
        var gameCollectionChangedNotifications = new List<NotifyCollectionChangedEventArgs>();
        var vm = new GameRegistryViewModel { GameManager = SetupGameManager(game1) };
        vm.Games.CollectionChanged += (sender, args) => gameCollectionChangedNotifications.Add(args);
        Mock.Get(vm.GameManager.GameRegistry).Setup(gameRegistry => gameRegistry.RemoveGame(game1));

        vm.RemoveGameCommand.Execute(game1);

        Assert.AreEqual(0, vm.Games.Count);
        Assert.AreEqual(1, gameCollectionChangedNotifications.Count);
        Assert.AreEqual(NotifyCollectionChangedAction.Remove, gameCollectionChangedNotifications[0].Action);
        // https://github.com/devlooped/moq/issues/858
        // Mock.Get(vm.GameManager).VerifyNoOtherCalls();
        Mock.Get(vm.GameManager.GameRegistry).VerifyAll();
        Mock.Get(vm.GameManager.GameRegistry).VerifyNoOtherCalls();
    }

    [TestMethod]
    public void WhenImportFromRegistryInvoked_AddGames()
    {
        var game1 = new Game
        {
            DisplayName = "Game 1"
        };
        var gameCollectionChangedNotifications = new List<NotifyCollectionChangedEventArgs>();
        var vm = new GameRegistryViewModel { GameManager = SetupGameManager() };
        vm.Games.CollectionChanged += (sender, args) => gameCollectionChangedNotifications.Add(args);
        Mock.Get(vm.GameManager).Setup(gameManager => gameManager.ShowAddGamesDialog<AddGamesFromRegistryViewModel>()).Returns([game1]);
        Mock.Get(vm.GameManager.GameRegistry).Setup(gameRegistry => gameRegistry.AddGame(game1));

        vm.ImportFromRegistryCommand.Execute(game1);

        Assert.AreEqual(1, vm.Games.Count);
        Assert.AreEqual("Game 1", vm.Games[0].DisplayName);
        Assert.AreEqual(1, gameCollectionChangedNotifications.Count);
        Assert.AreEqual(NotifyCollectionChangedAction.Reset, gameCollectionChangedNotifications[0].Action);
        Mock.Get(vm.GameManager).VerifyAll();
        Mock.Get(vm.GameManager).VerifyNoOtherCalls();
        Mock.Get(vm.GameManager.GameRegistry).VerifyAll();
        Mock.Get(vm.GameManager.GameRegistry).VerifyNoOtherCalls();
    }

    [TestMethod]
    public void WhenImportFromSteamInvoked_AddGames()
    {
        var game1 = new Game
        {
            DisplayName = "Game 1"
        };
        var gameCollectionChangedNotifications = new List<NotifyCollectionChangedEventArgs>();
        var vm = new GameRegistryViewModel { GameManager = SetupGameManager() };
        vm.Games.CollectionChanged += (sender, args) => gameCollectionChangedNotifications.Add(args);
        Mock.Get(vm.GameManager).Setup(gameManager => gameManager.ShowAddGamesDialog<AddGamesFromSteamViewModel>()).Returns([game1]);
        Mock.Get(vm.GameManager.GameRegistry).Setup(gameRegistry => gameRegistry.AddGame(game1));

        vm.ImportFromSteamCommand.Execute(game1);

        Assert.AreEqual(1, vm.Games.Count);
        Assert.AreEqual("Game 1", vm.Games[0].DisplayName);
        Assert.AreEqual(1, gameCollectionChangedNotifications.Count);
        Assert.AreEqual(NotifyCollectionChangedAction.Reset, gameCollectionChangedNotifications[0].Action);
        Mock.Get(vm.GameManager).VerifyAll();
        Mock.Get(vm.GameManager).VerifyNoOtherCalls();
        Mock.Get(vm.GameManager.GameRegistry).VerifyAll();
        Mock.Get(vm.GameManager.GameRegistry).VerifyNoOtherCalls();
    }
}