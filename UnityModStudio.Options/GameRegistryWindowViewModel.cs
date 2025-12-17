using System;
using System.Windows.Input;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace UnityModStudio.Options;

public class GameRegistryWindowViewModel : ObservableObject
{
    public GameRegistryViewModel InnerViewModel { get; }

    public ICommand ConfirmCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action<bool>? Closed;

    public GameRegistryWindowViewModel(GameRegistryViewModel innerViewModel)
    {
        InnerViewModel = innerViewModel;
        ConfirmCommand = new DelegateCommand(Confirm, null, ThreadHelper.JoinableTaskFactory);
        CancelCommand = new DelegateCommand(Cancel, null, ThreadHelper.JoinableTaskFactory);
    }

    private void Confirm()
    {
        SaveGames();

        Closed?.Invoke(true);
    }

    private void Cancel()
    {
        LoadGames();

        Closed?.Invoke(false);
    }

    public void LoadGames()
    {
        if (InnerViewModel.GameManager?.GameRegistry is not null)
            ThreadHelper.JoinableTaskFactory.Run(InnerViewModel.GameManager.GameRegistry.LoadSafeAsync);
    }

    private void SaveGames()
    {
        if (InnerViewModel.GameManager?.GameRegistry is not null)
            ThreadHelper.JoinableTaskFactory.Run(InnerViewModel.GameManager.GameRegistry.SaveSafeAsync);
    }
}