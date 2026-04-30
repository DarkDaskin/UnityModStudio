using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows.Input;
using UnityModStudio.Common.GameSpecific.Versions;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Options;

public class GameRegistryViewModel : ObservableObject
{
    public SuspendableObservableCollection<Game> Games { get; } = [];

    public bool IsUpdating
    {
        get; 
        private set => SetProperty(ref field, value);
    }

    public ICommand AddGameCommand { get; }
    public ICommand UpdateGameCommand { get; }
    public ICommand RemoveGameCommand { get; }
    public ICommand ImportFromRegistryCommand { get; }
    public ICommand ImportFromSteamCommand { get; }
    public ICommand UpdateAllCommand { get; }

    [Import]
    public IGameManager? GameManager
    {
        get;
        set
        {
            SetProperty(ref field, value);

            LoadGames();
        }
    }

    [ImportMany]
    public IGameVersionResolver[]? GameVersionResolvers
    {
        get;
        set => SetProperty(ref field, value);
    }

    public GameRegistryViewModel()
    {
        AddGameCommand = new DelegateCommand(AddGame, null, ThreadHelper.JoinableTaskFactory);
        UpdateGameCommand = new DelegateCommand<Game>(UpdateGame, IsGameSelected, ThreadHelper.JoinableTaskFactory);
        RemoveGameCommand = new DelegateCommand<Game>(RemoveGame, IsGameSelected, ThreadHelper.JoinableTaskFactory);
        ImportFromRegistryCommand = new DelegateCommand(ImportFromRegistry, null, ThreadHelper.JoinableTaskFactory);
        ImportFromSteamCommand = new DelegateCommand(ImportFromSteam, null, ThreadHelper.JoinableTaskFactory);
        UpdateAllCommand = new DelegateCommand(UpdateAll, null, ThreadHelper.JoinableTaskFactory);
    }

    private void AddGame()
    {
        var game = new Game();
        if (GameManager?.ShowEditDialog(game) ?? false)
        {
            GameManager.GameRegistry.AddGame(game);
            Games.Add(game);
        }
    }

    private void UpdateGame(Game game)
    {
        if (!GameManager?.ShowEditDialog(game) ?? false)
            return;

        // Trigger change notification.
        // TODO: better way which preserves selection
        var index = Games.IndexOf(game);
        Games.RemoveAt(index);
        Games.Insert(index, game);
    }

    private void RemoveGame(Game game)
    {
        GameManager?.GameRegistry.RemoveGame(game);
        Games.Remove(game);
    }

    private static bool IsGameSelected(Game? game) => game != null;

    public void LoadGames() => ReloadGames(false);

    private void ImportFromRegistry()
    {
        if (GameManager == null)
            return;

        var games = GameManager.ShowAddGamesDialog<AddGamesFromRegistryViewModel>();

        using (Games.SuspendChangeNotification())
        {
            foreach (var game in games)
            {
                GameManager.GameRegistry.AddGame(game);
                Games.Add(game);
            }
        }
    }

    private void ImportFromSteam()
    {
        if (GameManager == null)
            return;

        var games = GameManager.ShowAddGamesDialog<AddGamesFromSteamViewModel>();

        using (Games.SuspendChangeNotification())
        {
            foreach (var game in games)
            {
                GameManager.GameRegistry.AddGame(game);
                Games.Add(game);
            }
        }
    }

    private void UpdateAll() => ReloadGames(true);

    private void ReloadGames(bool update)
    {
        if (GameManager == null || (update && GameVersionResolvers == null))
            return;

        if (update)
            IsUpdating = true;

        using (Games.SuspendChangeNotification())
        {
            Games.Clear();
            foreach (var game in GameManager.GameRegistry.Games)
            {
                if (update)
                    ThreadHelper.JoinableTaskFactory.Run(() => Task.Run(() => 
                        GameManager.GameRegistry.UpdateAllGameProperties(game, GameVersionResolvers)));

                Games.Add(game);
            }
        }
        
        if (update)
            IsUpdating = false;
    }
}