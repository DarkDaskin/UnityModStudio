using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using UnityModStudio.Common;
using UnityModStudio.Common.ModLoader;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Options
{
    public class GamePropertiesViewModel : ObservableObjectWithValidation
    {
        private readonly Game _game;
        private string _displayName;
        private string _gamePath;
        private string? _gameName;
        private string? _architecture;
        private string? _unityVersion;
        private string? _monoProfile;
        private string? _targetFrameworkMoniker;
        private string? _gameExecutableFileName;
        private ImageSource? _gameIcon;
        private IModLoaderManager? _selectedModLoader;
        private IModLoaderManager _detectedModLoader = NullModLoaderManager.Instance;
        private IModLoaderManager[] _modLoaders = {NullModLoaderManager.Instance};

        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (_displayName == value)
                    return;

                _displayName = value.Trim();
                NotifyPropertyChanged();
                ValidateDisplayName();
                NotifyErrorsChanged();
            }
        }

        public string GamePath
        {
            get => _gamePath;
            set
            {
                if (_gamePath == value)
                    return;

                _gamePath = value.Trim();
                _selectedModLoader = null;
                NotifyPropertyChanged();
                ValidateGamePath();
                NotifyErrorsChanged();
                NotifyPropertyChanged(nameof(HasValidGamePath));
            }
        }

        public string? GameName
        {
            get => _gameName;
            private set => SetProperty(ref _gameName, value);
        }

        public string? Architecture
        {
            get => _architecture;
            private set => SetProperty(ref _architecture, value);
        }

        public string? UnityVersion
        {
            get => _unityVersion;
            private set => SetProperty(ref _unityVersion, value);
        }

        public string? MonoProfile
        {
            get => _monoProfile;
            private set => SetProperty(ref _monoProfile, value);
        }

        public string? TargetFrameworkMoniker
        {
            get => _targetFrameworkMoniker;
            private set => SetProperty(ref _targetFrameworkMoniker, value);
        }

        public string? GameExecutableFileName
        {
            get => _gameExecutableFileName;
            private set => SetProperty(ref _gameExecutableFileName, value);
        }

        public ImageSource? GameIcon
        {
            get => _gameIcon;
            private set => SetProperty(ref _gameIcon, value);
        }

        public IModLoaderManager? SelectedModLoader
        {
            get => _selectedModLoader;
            set => SetProperty(ref _selectedModLoader, value);
        }

        public IModLoaderManager DetectedModLoader
        {
            get => _detectedModLoader;
            private set => SetProperty(ref _detectedModLoader, value ?? throw new ArgumentNullException());
        }

        public bool HasValidGamePath => !string.IsNullOrWhiteSpace(GamePath) && !HasPropertyErrors(nameof(GamePath));
        
        public ICommand BrowseForGamePathCommand { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool>? Closed;

        [Import]
        public IFolderBrowserService? FolderBrowserService { get; set; }

        [Import]
        public IGameManager? GameManager { get; set; }

        [ImportMany]
        public IModLoaderManager[] ModLoaders
        {
            get => _modLoaders;
            set
            {
                _modLoaders = value ?? throw new ArgumentNullException();
                Debug.Assert(_modLoaders.OfType<NullModLoaderManager>().Any(), "NullModLoaderManager must be always present.");
                Array.Sort(_modLoaders, (x, y) => x.Priority.CompareTo(y.Priority));
                NotifyPropertyChanged();

                SelectedModLoader = _modLoaders.SingleOrDefault(loader => loader.Id == _game.ModLoaderId) ??
                                    _modLoaders.OfType<NullModLoaderManager>().Single();
            }
        }


        public GamePropertiesViewModel(Game game)
        {
            _game = game;
            _displayName = game.DisplayName;
            _gamePath = game.Path;

            BrowseForGamePathCommand = new DelegateCommand(BrowseForGamePath);
            ConfirmCommand = new DelegateCommand(Confirm, () => !HasErrors);
            CancelCommand = new DelegateCommand(Cancel);

            ValidateDisplayName();
            ValidateGamePath();
        }

        private void BrowseForGamePath()
        {
            Debug.Assert(FolderBrowserService != null, nameof(FolderBrowserService) + " != null");

            var initialPath = Directory.Exists(GamePath) ? GamePath : null;
            var selectedPath = ThreadHelper.JoinableTaskFactory.Run(() => 
                FolderBrowserService!.BrowseForFolderAsync("Select the game root folder", initialPath));
            if (selectedPath != null)
                GamePath = selectedPath;
        }

        private void Confirm()
        {
            if (!ValidateGamePath()) 
                return;

            _game.DisplayName = _displayName;
            _game.Path = _gamePath;
            _game.ModLoaderId = _selectedModLoader?.Id;
            _game.GameName = _gameName;
            _game.GameExecutableFileName = _gameExecutableFileName;
            _game.Architecture = _architecture;
            _game.UnityVersion = _unityVersion;
            _game.MonoProfile = _monoProfile;

            Closed?.Invoke(true);
        }

        private void Cancel() => Closed?.Invoke(false);

        private void ValidateDisplayName()
        {
            ClearErrors(nameof(DisplayName));

            if (string.IsNullOrWhiteSpace(DisplayName))
                AddError("Display name must not be empty.", nameof(DisplayName));
            else if(GameManager?.GameRegistry.FindGameByName(DisplayName) is { } other && other != _game)
                AddError("Display name must be unique.", nameof(DisplayName));
        }

        private bool ValidateGamePath()
        {
            ClearErrors(nameof(GamePath));

            if (string.IsNullOrWhiteSpace(GamePath))
            {
                AddError("Specify a path.", nameof(GamePath));
                return false;
            }

            if (!GameInformationResolver.TryGetGameInformation(GamePath, out var gameInformation, out var error))
            {
                AddError(error, nameof(GamePath));
                return false;
            }

            GameName = gameInformation.Name;
            Architecture = gameInformation.Architecture.ToString();
            UnityVersion = gameInformation.UnityVersion;
            TargetFrameworkMoniker = gameInformation.TargetFrameworkMoniker;
            MonoProfile = GetMonoProfileString(gameInformation);
            GameExecutableFileName = gameInformation.GameExecutableFile.Name;
            GameIcon = GetGameIcon(gameInformation);

            if (string.IsNullOrWhiteSpace(DisplayName))
                DisplayName = GameName ?? "";

            DetectedModLoader = DetectModLoader();
            SelectedModLoader ??= DetectedModLoader;

            return true;
        }

        private static string GetMonoProfileString(GameInformation gameInformation)
        {
            var match = Regex.Match(gameInformation.TargetFrameworkMoniker, @"(?<NetStandard>netstandard(?<Version>\d+\.\d+))|(?<NetFull>net(?<Version>\d+))");

            if (match.Groups["NetStandard"].Success)
                return ".NET Standard " + match.Groups["Version"].Value;

            if (match.Groups["NetFull"].Success)
                return ".NET " + string.Join(".", match.Groups["Version"].Value.ToCharArray()) + (gameInformation.IsSubsetProfile ? " Subset" : "");

            return "<unknown>";
        }

        private static ImageSource? GetGameIcon(GameInformation gameInformation)
        {
            using var icon = Icon.ExtractAssociatedIcon(gameInformation.GameExecutableFile.FullName);
            if (icon == null)
                return null;

            return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, new Int32Rect(0, 0, icon.Width, icon.Height),
                BitmapSizeOptions.FromEmptyOptions());
        }

        private IModLoaderManager DetectModLoader()
        {
            var query =
                from loader in ModLoaders
                orderby loader.Priority descending
                where loader.IsInstalled(GamePath!)
                select loader;
            return query.FirstOrDefault() ?? NullModLoaderManager.Instance;
        }
    }
}