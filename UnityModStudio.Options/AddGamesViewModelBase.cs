using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using UnityModStudio.Common;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Options;

public abstract class AddGamesViewModelBase : ObservableObject
{
    private IGameManager? _gameManager;
    private bool _isLoading;

    [Import]
    public IGameManager? GameManager
    {
        get => _gameManager;
        set
        {
            SetProperty(ref _gameManager, value);
            
            InitializeAsync().FileAndForget("UnityModStudio/AddGames/Initialize");
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public SuspendableObservableCollection<Game> Games { get; } = [];
    public SuspendableObservableCollection<Game> SelectedGames { get; } = [];

    public ICommand SelectAllCommand { get; }
    public ICommand ConfirmCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action<bool>? Closed;

    protected AddGamesViewModelBase()
    {
        SelectAllCommand = new DelegateCommand(SelectAll, null, ThreadHelper.JoinableTaskFactory);
        ConfirmCommand = new DelegateCommand(Confirm, () => SelectedGames.Count > 0, ThreadHelper.JoinableTaskFactory);
        CancelCommand = new DelegateCommand(Cancel, null, ThreadHelper.JoinableTaskFactory);
    }

    private async Task InitializeAsync()
    {
        IsLoading = true;

        await InitializeCoreAsync();
    }

    // A separate method to facilitate unit testing.
    private protected virtual async Task InitializeCoreAsync()
    {
        using (Games.SuspendChangeNotification())
        {
            await TaskScheduler.Default;

            Games.Clear();
            foreach (var game in GetGames())
                Games.Add(game);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        }

        IsLoading = false;
    }

    private IEnumerable<Game> GetGames()
    {
        var names = GameManager!.GameRegistry.Games.Select(g => g.DisplayName).ToHashSet();

        foreach (var gameEntry in FindGames().OrderBy(g => g.Name))
        {
            if (!GameInformationResolver.TryGetGameInformation(gameEntry.Path, out var gameInformation, out _))
                continue;

            if (GameManager!.GameRegistry.Games.Any(g => string.Equals(g.Path, gameEntry.Path, StringComparison.OrdinalIgnoreCase)))
                continue;

            var name = gameEntry.Name;
            if (!names.Add(name))
            {
                var index = 1;
                var newName = $"{name} ({index})";
                while (names.Contains(newName))
                    index++;
                name = newName;
            }

            yield return new Game
            {
                DisplayName = name,
                Path = gameEntry.Path,
                GameName = gameInformation.Name,
                GameExecutableFileName = gameInformation.GameExecutableFile.Name,
                Architecture = gameInformation.Architecture.ToString(),
                UnityVersion = gameInformation.UnityVersion,
                TargetFrameworkMoniker = gameInformation.TargetFrameworkMoniker,
                MonoProfile = gameInformation.GetMonoProfileString(),
            };
        }
    }

    protected abstract IEnumerable<GameEntry> FindGames();

    private void SelectAll()
    {
        using (SelectedGames.SuspendChangeNotification())
        {
            SelectedGames.Clear();
            foreach (var game in Games)
                SelectedGames.Add(game);
        }
    }

    private void Confirm() => Closed?.Invoke(true);

    private void Cancel() => Closed?.Invoke(false);


    protected readonly struct GameEntry(string name, string path)
    {
        public string Name { get; } = name;
        public string Path { get; } = path;
    }
}