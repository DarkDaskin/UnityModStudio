using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Options
{
    public class GamePropertiesViewModel : GamePropertiesViewModelBase
    {
        public string DisplayName
        {
            get;
            set => SetProperty(ref field, value);
        }

        public ModDeploymentMode ModDeploymentMode
        {
            get;
            set => SetProperty(ref field, value);
        }

        public bool DeploySourceCode
        {
            get;
            set => SetProperty(ref field, value);
        }

        public DoorstopMode DoorstopMode
        {
            get;
            set => SetProperty(ref field, value);
        }

        public bool UseAlternateDoorstopDllName
        {
            get;
            set => SetProperty(ref field, value);
        }

        public ICommand BrowseForGamePathCommand { get; }

        public ICommand BrowseForModsPathCommand { get; }

        [Import]
        public IFolderBrowserService? FolderBrowserService { get; set; }

        [Import]
        public IGameManager? GameManager
        {
            get;
            set
            {
                SetProperty(ref field, value);

                Validate(nameof(DisplayName));
                Validate(nameof(GameVersion));
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

            AddRule(() => DisplayName, displayName => !string.IsNullOrWhiteSpace(displayName), "Display name must not be empty.");
            AddRule(() => DisplayName, displayName => GameManager is null || !GameManager.GameRegistry.FindGamesByDisplayName(displayName).Except([Game]).Any(), 
                "Display name must be unique.");
            AddRule(() => GameVersion, 
                gameVersion => GameName is null || GameManager is null || !GameManager.GameRegistry.FindGamesByGameNameAndVersion(GameName, gameVersion).Except([Game]).Any(), 
                "Game version must be unique across games with same game name.");
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

        protected override void OnValidGamePathChanged()
        {
            if (string.IsNullOrWhiteSpace(DisplayName))
                DisplayName = GameName ?? "";
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