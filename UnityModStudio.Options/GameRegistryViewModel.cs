using System.ComponentModel.Composition;
using System.Windows.Input;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Options
{
    public class GameRegistryViewModel : ObservableObject
    {
        private IGameManager? _gameManager;

        public SuspendableObservableCollection<Game> Games { get; } = new();

        public ICommand AddGameCommand { get; }
        public ICommand UpdateGameCommand { get; }
        public ICommand RemoveGameCommand { get; }
        public ICommand ImportFromRegistryCommand { get; }
        public ICommand ImportFromSteamCommand { get; }

        [Import]
        public IGameManager? GameManager
        {
            get => _gameManager;
            set
            {
                SetProperty(ref _gameManager, value);

                LoadGames();
            }
        }

        public GameRegistryViewModel()
        {
            AddGameCommand = new DelegateCommand(AddGame, null, ThreadHelper.JoinableTaskFactory);
            UpdateGameCommand = new DelegateCommand<Game>(UpdateGame, IsGameSelected, ThreadHelper.JoinableTaskFactory);
            RemoveGameCommand = new DelegateCommand<Game>(RemoveGame, IsGameSelected, ThreadHelper.JoinableTaskFactory);
            ImportFromRegistryCommand = new DelegateCommand(ImportFromRegistry, null, ThreadHelper.JoinableTaskFactory);
            ImportFromSteamCommand = new DelegateCommand(ImportFromSteam, null, ThreadHelper.JoinableTaskFactory);
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

        public void LoadGames()
        {
            if (GameManager == null)
                return;

            using (Games.SuspendChangeNotification())
            {
                Games.Clear();
                foreach (var game in GameManager.GameRegistry.Games)
                    Games.Add(game);
            }
        }

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
    }
}