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

        await WaitNoWarningAsync(vm.LoadingStarted);

        Assert.IsTrue(vm.IsLoading);

        vm.Finish();
        await WaitNoWarningAsync(vm.LoadingFinished);

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
        await WaitNoWarningAsync(vm.LoadingFinished);

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
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
        });
        await WaitNoWarningAsync(vm.LoadingFinished);

        Assert.AreEqual(1, vm.Games.Count);
        Assert.AreEqual("Unity2018Test (1)", vm.Games[0].DisplayName);
    }

#pragma warning disable VSTHRD003
    private static async Task WaitNoWarningAsync(Task task) => await task;
#pragma warning restore VSTHRD003


    private class TestViewModel(bool withDelay = false) : AddGamesViewModelBase
    {
        private readonly ManualResetEventSlim _event = new();
        private readonly TaskCompletionSource<bool> _loadingStartedCompletionSource = new();
        private readonly TaskCompletionSource<bool> _loadingFinishedCompletionSource = new();

        public void Finish() => _event.Set();

        public Task LoadingStarted => _loadingStartedCompletionSource.Task;

        public Task LoadingFinished => _loadingFinishedCompletionSource.Task;

        protected override IEnumerable<GameEntry> FindGames()
        {
            if (withDelay)
                _event.Wait();

            yield return new GameEntry("Unity2018Test", Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"));
            yield return new GameEntry("Unity2018Test", Path.Combine(SampleGameInfo.DownloadPath, "2018-netstandard20-v2.0"));
            yield return new GameEntry("NotAGame", @"C:\Program Files\Windows Mail");
        }

        private protected override async Task InitializeCoreAsync()
        {
            _loadingStartedCompletionSource.SetResult(true);

            await base.InitializeCoreAsync();

            _loadingFinishedCompletionSource.SetResult(true);
        }
    }
}