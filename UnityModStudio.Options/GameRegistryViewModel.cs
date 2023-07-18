using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;
using UnityModStudio.Common;
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
        public ICommand ImportFromRegistryCommand { get; }

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

        private void ImportFromRegistry()
        {
            if (GameManager == null)
                return;

            foreach (var program in GetInstalledPrograms())
            {
                if (!GameInformationResolver.TryGetGameInformation(program.Value, out var gameInformation, out _))
                    continue;

                if (GameManager.GameRegistry.Games.Any(g => string.Equals(g.Path, program.Value, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var game = new Game
                {
                    DisplayName = program.Key,
                    Path = program.Value,
                    GameName = gameInformation.Name,
                    GameExecutableFileName = gameInformation.GameExecutableFile.Name,
                    Architecture = gameInformation.Architecture.ToString(),
                    UnityVersion = gameInformation.UnityVersion,
                    MonoProfile = gameInformation.GetMonoProfileString(),
                };
                GameManager.GameRegistry.AddGame(game);
                Games.Add(game);
            }
        }

        private static Dictionary<string, string> GetInstalledPrograms()
        {
            var programs = new Dictionary<string, string>();

            using var uninstallKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            if (uninstallKey == null)
                return programs;

            foreach (var subKeyName in uninstallKey.GetSubKeyNames())
            {
                using var subKey = uninstallKey.OpenSubKey(subKeyName)!;
                var displayName = subKey.GetValue("DisplayName") as string;
                var installLocation = subKey.GetValue("InstallLocation") as string;
                if (!string.IsNullOrEmpty(displayName) && !string.IsNullOrEmpty(installLocation))
                {
                    if (programs.ContainsKey(displayName!))
                    {
                        var index = 1;
                        var newName = $"{displayName} ({index})";
                        while (programs.ContainsKey(newName))
                            index++;
                        displayName = newName;
                    }

                    programs.Add(displayName!, installLocation!);
                }
            }

            return programs;
        }
    }
}