using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using UnityModStudio.Common;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Options
{
    public abstract class GamePropertiesViewModelBase : ObservableObjectWithValidation
    {
        private Game? _game;
        private string? _gamePath;
        private string? _gameName;
        private string? _architecture;
        private string? _unityVersion;
        private string? _monoProfile;
        private string? _targetFrameworkMoniker;
        private string? _gameExecutableFileName;
        private ImageSource? _gameIcon;


        public Game? Game
        {
            get => _game;
            set
            {
                SetProperty(ref _game, value);
                GamePath = Game?.Path;
            }
        }

        public string? GamePath
        {
            get => _gamePath;
            set
            {
                if (!SetProperty(ref _gamePath, value?.Trim()))
                    return;

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

        public bool HasValidGamePath => !string.IsNullOrWhiteSpace(GamePath) && !HasPropertyErrors(nameof(GamePath));
        

        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool>? Closed;


        protected GamePropertiesViewModelBase()
        {
            ConfirmCommand = new DelegateCommand(Confirm, () => !HasErrors, ThreadHelper.JoinableTaskFactory);
            CancelCommand = new DelegateCommand(Cancel, null, ThreadHelper.JoinableTaskFactory);
        }

        private void Confirm()
        {
            if (!ValidateGamePath())
                return;

            OnConfirm();

            Closed?.Invoke(true);
        }

        private void Cancel() => Closed?.Invoke(false);

        private static ImageSource? GetGameIcon(GameInformation gameInformation)
        {
            using var icon = Icon.ExtractAssociatedIcon(gameInformation.GameExecutableFile.FullName);
            if (icon == null)
                return null;

            return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, new Int32Rect(0, 0, icon.Width, icon.Height),
                BitmapSizeOptions.FromEmptyOptions());
        }

        protected virtual bool ValidateGamePath()
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

        protected virtual void OnConfirm()
        {
            Game!.Path = GamePath!;
            Game.GameName = GameName;
            Game.GameExecutableFileName = GameExecutableFileName;
            Game.Architecture = Architecture;
            Game.UnityVersion = UnityVersion;
            Game.MonoProfile = MonoProfile;
        }
    }
}