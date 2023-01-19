using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Options
{
    public class GamePropertiesViewModel : GamePropertiesViewModelBase
    {
        private string _displayName = "";
        private DoorstopMode _doorstopMode;
        private bool _useAlternateDoorstopDllName;

        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (!SetProperty(ref _displayName, value.Trim()))
                    return;
                
                ValidateDisplayName();
                NotifyErrorsChanged();
            }
        }

        public DoorstopMode DoorstopMode
        {
            get => _doorstopMode;
            set => SetProperty(ref _doorstopMode, value);
        }

        public bool UseAlternateDoorstopDllName
        {
            get => _useAlternateDoorstopDllName;
            set => SetProperty(ref _useAlternateDoorstopDllName, value);
        }

        public ICommand BrowseForGamePathCommand { get; }

        [Import]
        public IFolderBrowserService? FolderBrowserService { get; set; }

        [Import]
        public IGameManager? GameManager { get; set; }
        

        public GamePropertiesViewModel(Game game)
        {
            Game = game;
            DisplayName = Game.DisplayName;
            DoorstopMode = Game.DoorstopMode;
            UseAlternateDoorstopDllName = Game.UseAlternateDoorstopDllName;
            
            BrowseForGamePathCommand = new DelegateCommand(BrowseForGamePath, null, ThreadHelper.JoinableTaskFactory);
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

        private void ValidateDisplayName()
        {
            ClearErrors(nameof(DisplayName));

            if (string.IsNullOrWhiteSpace(DisplayName))
                AddError("Display name must not be empty.", nameof(DisplayName));
            else if(GameManager?.GameRegistry.FindGameByDisplayName(DisplayName) is { } other && other != Game)
                AddError("Display name must be unique.", nameof(DisplayName));
        }

        protected override bool ValidateGamePath()
        {
            if (!base.ValidateGamePath())
                return false;

            if (string.IsNullOrWhiteSpace(DisplayName))
                DisplayName = GameName ?? "";

            return true;
        }

        protected override void OnConfirm()
        {
            base.OnConfirm();

            Game!.DisplayName = DisplayName;
            Game.DoorstopMode = DoorstopMode;
            Game.UseAlternateDoorstopDllName = UseAlternateDoorstopDllName;
        }
    }
}