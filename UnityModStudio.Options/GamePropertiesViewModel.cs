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
        private ModDeploymentMode _modDeploymentMode;
        private bool _deploySourceCode;
        private DoorstopMode _doorstopMode;
        private bool _useAlternateDoorstopDllName;
        private IGameManager? _gameManager;

        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (!SetProperty(ref _displayName, value))
                    return;
                
                ValidateDisplayName();
                NotifyErrorsChanged();
            }
        }

        public ModDeploymentMode ModDeploymentMode
        {
            get => _modDeploymentMode;
            set => SetProperty(ref _modDeploymentMode, value);
        }

        public bool DeploySourceCode
        {
            get => _deploySourceCode;
            set => SetProperty(ref _deploySourceCode, value);
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

        public ICommand BrowseForModsPathCommand { get; }

        [Import]
        public IFolderBrowserService? FolderBrowserService { get; set; }

        [Import]
        public IGameManager? GameManager
        {
            get => _gameManager;
            set
            {
                SetProperty(ref _gameManager, value);

                ValidateDisplayName();
            }
        }


        public GamePropertiesViewModel(Game game)
        {
            Game = game;
            DisplayName = Game.DisplayName;
            ModDeploymentMode = Game.ModDeploymentMode;
            DeploySourceCode = game.DeploySourceCode;
            DoorstopMode = Game.DoorstopMode;
            UseAlternateDoorstopDllName = Game.UseAlternateDoorstopDllName;
            
            BrowseForGamePathCommand = new DelegateCommand(BrowseForGamePath, null, ThreadHelper.JoinableTaskFactory);
            BrowseForModsPathCommand = new DelegateCommand(BrowseForModsPath, null, ThreadHelper.JoinableTaskFactory);
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

        private void BrowseForModsPath()
        {
            Debug.Assert(FolderBrowserService != null, nameof(FolderBrowserService) + " != null");

            var initialPath = Directory.Exists(ModsPath) ? ModsPath : Directory.Exists(GamePath) ? GamePath : null;
            var selectedPath = ThreadHelper.JoinableTaskFactory.Run(() => 
                FolderBrowserService!.BrowseForFolderAsync("Select the mods folder", initialPath));
            if (selectedPath != null)
                ModsPath = selectedPath;
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

            Game!.DisplayName = DisplayName.Trim();
            Game.ModDeploymentMode = ModDeploymentMode;
            Game.DeploySourceCode = DeploySourceCode;
            Game.DoorstopMode = DoorstopMode;
            Game.UseAlternateDoorstopDllName = UseAlternateDoorstopDllName;
        }
    }
}