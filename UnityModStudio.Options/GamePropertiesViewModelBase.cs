using System;
using System.Drawing;
using System.IO;
using System.Linq;
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
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
        private static readonly string InvalidFileNameCharsString = string.Join(" ", InvalidFileNameChars.Where(c => !char.IsControl(c)));

        public Game? Game
        {
            get;
            set
            {
                SetProperty(ref field, value);

                GamePath = Game?.Path;
                ModsPath = Game?.ModsPath;
                GameVersion = Game?.Version;

                RefreshProperties();
            }
        }

        public string? GamePath
        {
            get;
            set
            {
                if (!SetProperty(ref field, value?.Trim()))
                    return;

                NotifyPropertyChanged(nameof(HasValidGamePath));
            }
        }

        public string? ModsPath
        {
            get;
            set => SetProperty(ref field, value);
        }

        public string? GameVersion
        {
            get;
            set => SetProperty(ref field, value);
        }

        public string? GameName
        {
            get;
            private set => SetProperty(ref field, value);
        }

        public string? Architecture
        {
            get;
            private set => SetProperty(ref field, value);
        }

        public string? UnityVersion
        {
            get;
            private set => SetProperty(ref field, value);
        }

        public string? MonoProfile
        {
            get;
            private set => SetProperty(ref field, value);
        }

        public string? TargetFrameworkMoniker
        {
            get;
            private set => SetProperty(ref field, value);
        }

        public string? GameExecutableFileName
        {
            get;
            private set => SetProperty(ref field, value);
        }

        public ImageSource? GameIcon
        {
            get;
            private set => SetProperty(ref field, value);
        }

        public bool HasValidGamePath => !string.IsNullOrWhiteSpace(GamePath) && !HasPropertyErrors(nameof(GamePath));
        
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool>? Closed;


        protected GamePropertiesViewModelBase()
        {
            ConfirmCommand = new DelegateCommand(Confirm, () => Game != null && !HasErrors, ThreadHelper.JoinableTaskFactory);
            CancelCommand = new DelegateCommand(Cancel, null, ThreadHelper.JoinableTaskFactory);

            AddRule(() => GameVersion,
                v => !(v?.Any(InvalidFileNameChars.Contains) ?? false),
                $"Game version must not contain any of the following characters: {InvalidFileNameCharsString}");
            AddRule(() => GamePath,
                gamePath =>
                {
                    if (string.IsNullOrWhiteSpace(gamePath))
                        return "Specify a path.";

                    if (!GameInformationResolver.TryGetGameInformation(gamePath, out var gameInformation, out var error, out _))
                        return error;

                    OnValidGamePathChanged(gameInformation);

                    return null;
                });

        }

        private void Confirm()
        {
            if (!HasValidGamePath)
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

        private void OnValidGamePathChanged(GameInformation gameInformation)
        {
            GameName = gameInformation.Name;
            Architecture = gameInformation.Architecture.ToString();
            UnityVersion = gameInformation.UnityVersion;
            TargetFrameworkMoniker = gameInformation.TargetFrameworkMoniker;
            MonoProfile = gameInformation.GetMonoProfileString();
            GameExecutableFileName = gameInformation.GameExecutableFile.Name;
            GameIcon = GetGameIcon(gameInformation);

            OnValidGamePathChanged();
        }

        protected virtual void OnValidGamePathChanged() { }

        protected virtual void RefreshProperties()
        {
            NotifyPropertyChanged(nameof(GamePath));
            NotifyPropertyChanged(nameof(ModsPath));
            NotifyPropertyChanged(nameof(GameVersion));
            NotifyPropertyChanged(nameof(GameName));
            NotifyPropertyChanged(nameof(GameExecutableFileName));
            NotifyPropertyChanged(nameof(Architecture));
            NotifyPropertyChanged(nameof(UnityVersion));
            NotifyPropertyChanged(nameof(MonoProfile));
            NotifyPropertyChanged(nameof(GameIcon));
        }

        protected virtual void OnConfirm()
        {
            Game!.Path = GamePath!.Trim();
            Game.ModsPath = ModsPath?.Trim();
            Game.Version = GameVersion?.Trim();
            Game.GameName = GameName;
            Game.GameExecutableFileName = GameExecutableFileName;
            Game.Architecture = Architecture;
            Game.UnityVersion = UnityVersion;
            Game.TargetFrameworkMoniker = TargetFrameworkMoniker;
            Game.MonoProfile = MonoProfile;
        }
    }
}