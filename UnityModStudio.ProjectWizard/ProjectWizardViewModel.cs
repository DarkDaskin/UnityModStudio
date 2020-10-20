using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.PlatformUI;
using UnityModStudio.Common;
using IWin32Window = System.Windows.Forms.IWin32Window;

namespace UnityModStudio.ProjectWizard
{
    public class ProjectWizardViewModel : ObservableObject
    {
        private readonly IWin32Window _owner;
        private string? _gamePath;
        private string? _error;
        private string? _gameName;
        private string? _unityVersion;
        private string? _monoProfile;
        private string? _targetFrameworkMoniker;
        private ImageSource? _gameIcon;

        public string? GamePath
        {
            get => _gamePath;
            set
            {
                SetProperty(ref _gamePath, value);
                NotifyPropertyChanged(nameof(HasValidGamePath));
                VerifyUnityGame();
            }
        }

        public string? Error
        {
            get => _error;
            set
            {
                SetProperty(ref _error, value);
                NotifyPropertyChanged(nameof(HasValidGamePath));
                NotifyPropertyChanged(nameof(ErrorVisibility));
                NotifyPropertyChanged(nameof(GameInformationVisibility));
            }
        }

        public string? GameName
        {
            get => _gameName;
            set => SetProperty(ref _gameName, value);
        }

        public string? UnityVersion
        {
            get => _unityVersion;
            set => SetProperty(ref _unityVersion, value);
        }

        public string? MonoProfile
        {
            get => _monoProfile;
            set => SetProperty(ref _monoProfile, value);
        }

        public string? TargetFrameworkMoniker
        {
            get => _targetFrameworkMoniker;
            set => SetProperty(ref _targetFrameworkMoniker, value);
        }

        public ImageSource? GameIcon
        {
            get => _gameIcon;
            set => SetProperty(ref _gameIcon, value);
        }

        public bool HasValidGamePath => Error == null && !string.IsNullOrWhiteSpace(GamePath);

        public Visibility ErrorVisibility => HasValidGamePath ? Visibility.Collapsed : Visibility.Visible;
        public Visibility GameInformationVisibility => HasValidGamePath ? Visibility.Visible : Visibility.Hidden;
        
        public ICommand BrowseForGamePathCommand { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool>? Closed;

        public ProjectWizardViewModel(IWin32Window owner)
        {
            _owner = owner;
            BrowseForGamePathCommand = new DelegateCommand(BrowseForGamePath);
            ConfirmCommand = new DelegateCommand(Confirm, () => HasValidGamePath);
            CancelCommand = new DelegateCommand(Cancel);
        }

        private void BrowseForGamePath()
        {
            var dialog = new FolderBrowserDialog
            {
                Description = "Select the root folder of the base game.",
                SelectedPath = GamePath,
                RootFolder = Environment.SpecialFolder.MyComputer,
                ShowNewFolderButton = false,
            };
            var result = dialog.ShowDialog(_owner);
            if (result == DialogResult.OK) 
                GamePath = dialog.SelectedPath;
        }

        private void Confirm()
        {
            if (VerifyUnityGame()) 
                Closed?.Invoke(true);
        }

        private void Cancel() => Closed?.Invoke(false);

        private bool VerifyUnityGame()
        {
            if (string.IsNullOrWhiteSpace(GamePath))
            {
                Error = "Specify a path.";
                return false;
            }

            if (!GameInformationResolver.TryGetGameInformation(GamePath, out var gameInformation, out var error))
            {
                Error = error;
                return false;
            }
            
            Error = null;
            GameName = gameInformation.Name;
            UnityVersion = gameInformation.UnityVersion;
            TargetFrameworkMoniker = gameInformation.TargetFrameworkMoniker;
            MonoProfile = GetMonoProfileString(gameInformation);
            GameIcon = GetGameIcon(gameInformation);
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
    }
}