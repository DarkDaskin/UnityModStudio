using System.ComponentModel.Composition;
using System.Windows.Input;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.PlatformUI;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Options
{
    public class GameRegistryViewModel : ObservableObject
    {
        private IGameManager? _gameManager;

        public SuspendableObservableCollection<Game> Games { get; } = new SuspendableObservableCollection<Game>();

        public ICommand AddGameCommand { get; }
        public ICommand UpdateGameCommand { get; }
        public ICommand RemoveGameCommand { get; }

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
            AddGameCommand = new DelegateCommand(AddGame);
            UpdateGameCommand = new DelegateCommand<Game>(UpdateGame, IsGameSelected);
            RemoveGameCommand = new DelegateCommand<Game>(RemoveGame, IsGameSelected);
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

        private void LoadGames()
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
    }
}