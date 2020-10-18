using System;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.VisualStudio.PlatformUI;
using UnityModStudio.Common;

namespace UnityModStudio.ProjectWizard
{
    public class ProjectWizardViewModel : ObservableObject
    {
        private readonly IWin32Window _owner;
        private string? _gamePath;

        public string? GamePath
        {
            get => _gamePath;
            set => SetProperty(ref _gamePath, value);
        }

        public ICommand BrowseForGamePathCommand { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool>? Closed;

        public ProjectWizardViewModel(IWin32Window owner)
        {
            _owner = owner;
            BrowseForGamePathCommand = new DelegateCommand(BrowseForGamePath);
            ConfirmCommand = new DelegateCommand(Confirm);
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
            if (!GameFileResolver.TryResolveGameFiles(GamePath, out _, out _, out var error))
            {
                ShowError(error);
                return false;
            }

            return true;
        }

        private void ShowError(string message) => MessageBox.Show(_owner, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}