using System.Collections.Specialized;
using System.IO;
using Microsoft.VisualStudio.Shell;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Options.Tests;

[TestClass]
public sealed class AddGamesViewModelBaseTests : GameManagerTestBase
{
    [TestMethod]
    public void WhenCreated_InitialStateIsCorrect()
    {
        var vm = new TestViewModel();

        Assert.IsNull(vm.GameManager);
        Assert.IsFalse(vm.IsLoading);
        Assert.IsTrue(vm.CancelCommand.CanExecute(null));
        Assert.IsFalse(vm.ConfirmCommand.CanExecute(null));
        Assert.IsTrue(vm.SelectAllCommand.CanExecute(null));
    }

    [TestMethod]
    public async Task WhenInitialized_AddGames()
    {
        var gamesNotifications = new List<NotifyCollectionChangedEventArgs>();
        var selectedGamesNotifications = new List<NotifyCollectionChangedEventArgs>();
        var vm = new TestViewModel(true);
        vm.Games.CollectionChanged += (sender, args) => gamesNotifications.Add(args);
        vm.SelectedGames.CollectionChanged += (sender, args) => selectedGamesNotifications.Add(args);
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        vm.GameManager = SetupGameManager();

        await Task.Delay(50);

        Assert.IsTrue(vm.IsLoading);

        await Task.Delay(300);

        Assert.IsFalse(vm.IsLoading);
        Assert.AreEqual(2, vm.Games.Count);
        Assert.AreEqual("Unity2018Test", vm.Games[0].DisplayName);
        Assert.AreEqual("Unity2018Test (1)", vm.Games[1].DisplayName);
        Assert.AreEqual(1, gamesNotifications.Count);
        Assert.AreEqual(NotifyCollectionChangedAction.Reset, gamesNotifications[0].Action);
        Assert.AreEqual(0, selectedGamesNotifications.Count);
    }

    [TestMethod]
    public async Task WhenSelectAllInvoked_SelectAllAndAllowConfirm()
    {
        var gamesNotifications = new List<NotifyCollectionChangedEventArgs>();
        var selectedGamesNotifications = new List<NotifyCollectionChangedEventArgs>();
        var vm = new TestViewModel();
        vm.Games.CollectionChanged += (sender, args) => gamesNotifications.Add(args);
        vm.SelectedGames.CollectionChanged += (sender, args) => selectedGamesNotifications.Add(args);
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        vm.GameManager = SetupGameManager();
        await Task.Delay(200);

        vm.SelectAllCommand.Execute(null);

        Assert.AreEqual(2, vm.Games.Count);
        Assert.AreEqual(2, vm.SelectedGames.Count);
        Assert.AreEqual("Unity2018Test", vm.SelectedGames[0].DisplayName);
        Assert.AreEqual("Unity2018Test (1)", vm.SelectedGames[1].DisplayName);
        Assert.AreEqual(1, gamesNotifications.Count);
        Assert.AreEqual(NotifyCollectionChangedAction.Reset, gamesNotifications[0].Action);
        Assert.AreEqual(1, selectedGamesNotifications.Count);
        Assert.AreEqual(NotifyCollectionChangedAction.Reset, selectedGamesNotifications[0].Action);
        Assert.IsTrue(vm.ConfirmCommand.CanExecute(null));
    }

    [TestMethod]
    public async Task WhenGameAlreadyExists_DoNotAddIt()
    {
        var vm = new TestViewModel();
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        vm.GameManager = SetupGameManager(new Game
        {
            DisplayName = "Unity2018Test",
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4"),
        });
        await Task.Delay(200);

        Assert.AreEqual(1, vm.Games.Count);
        Assert.AreEqual("Unity2018Test (1)", vm.Games[0].DisplayName);
    }


    private class TestViewModel(bool withDelay = false) : AddGamesViewModelBase
    {
        protected override IEnumerable<GameEntry> FindGames()
        {
            if (withDelay)
                Thread.Sleep(200);

            yield return new GameEntry("Unity2018Test", Path.Combine(SampleGameInfo.DownloadPath, "2018-net4"));
            yield return new GameEntry("Unity2018Test", Path.Combine(SampleGameInfo.DownloadPath, "2018-netstandard20"));
            yield return new GameEntry("NotAGame", @"C:\Program Files\Windows Mail");
        }
    }
}