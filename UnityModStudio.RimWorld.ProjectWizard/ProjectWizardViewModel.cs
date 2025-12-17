using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using UnityModStudio.Common;
using UnityModStudio.Common.Options;
using UnityModStudio.Options;
using UnityModStudio.RimWorld.Common.Options;

namespace UnityModStudio.RimWorld.ProjectWizard;

public class ProjectWizardViewModel : ObservableObjectWithValidation
{
    public IReadOnlyList<GameVersionViewModel> GameVersions
    {
        get;
        private set
        {
            SetProperty(ref field, value);
            NotifyPropertyChanged(nameof(AreGameVersionsAbsent));
        }
    } = [];

    public bool AreGameVersionsAbsent => GameVersions.Count == 0;

    public bool UseHarmony
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string? ModPackageId
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string? ModName
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string? ModAuthor
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string? ModDescription
    {
        get;
        set => SetProperty(ref field, value);
    }

    public ProjectLayout ProjectLayout
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string? ProjectName
    {
        get;
        set
        {
            if (!SetProperty(ref field, value))
                return;

            SetDefaults();
        }
    }

    [Import]
    public IGameManager? GameManager
    {
        get;
        set
        {
            if (!SetProperty(ref field, value))
                return;

            if (field != null)
                ThreadHelper.JoinableTaskFactory.Run(field.GameRegistry.LoadSafeAsync);

            FillGames();
        }
    }

    [Import]
    public IRimWorldSettingsManager? RimWorldSettingsManager
    {
        get;
        set
        {
            if (!SetProperty(ref field, value))
                return;

            if (field != null)
                ThreadHelper.JoinableTaskFactory.Run(field.LoadSafeAsync);

            SetDefaults();
        }
    }

    public ICommand ConfirmCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand OpenGameRegistryCommand { get; }

    public event Action<bool>? Closed;

    public ProjectWizardViewModel()
    {
        ConfirmCommand = new DelegateCommand(Confirm, CanConfirm, ThreadHelper.JoinableTaskFactory);
        CancelCommand = new DelegateCommand(Cancel, null, ThreadHelper.JoinableTaskFactory);
        OpenGameRegistryCommand = new DelegateCommand(OpenGameRegistry, null, ThreadHelper.JoinableTaskFactory);

        AddRule(() => ModPackageId, v => !string.IsNullOrWhiteSpace(v), "Mod package ID is required.");
        AddRule(() => ModAuthor, v => !string.IsNullOrWhiteSpace(v), "Mod author is required.");
        AddRule(() => ModName, v => !string.IsNullOrWhiteSpace(v), "Mod name is required.");
        AddRule(() => ModDescription, v => !string.IsNullOrWhiteSpace(v), "Mod description is required.");
        ValidateAll();
    }

    private void FillGames()
    {
        if (GameManager is null)
            return;

        var games = GameManager.GameRegistry.Games.Where(game => game.GameName == "RimWorld by Ludeon Studios");
        GameVersions = games
            .Where(game => !string.IsNullOrWhiteSpace(game.Version))
            .Select(game => new GameVersionViewModel(game))
            .OrderBy(vm => vm.Version, new GameVersionComparer())
            .ToArray();
    }

    private void SetDefaults()
    {
        if (RimWorldSettingsManager is null || ProjectName is null)
            return;

        UseHarmony = RimWorldSettingsManager.Settings.ProjectDefaults.UseHarmony;
        ProjectLayout = RimWorldSettingsManager.Settings.ProjectDefaults.ProjectLayout;
        ModAuthor ??= RimWorldSettingsManager.Settings.ProjectDefaults.ModAuthor ?? Environment.UserName;

        ModPackageId ??= ProjectName;
        ModName ??= ProjectName;
        ModDescription ??= ProjectName;

        var defaultPrefix = RimWorldSettingsManager.Settings.ProjectDefaults.ModPackageIdPrefix;
        if (!string.IsNullOrWhiteSpace(defaultPrefix) && !ModPackageId.StartsWith(defaultPrefix, StringComparison.InvariantCultureIgnoreCase))
            ModPackageId = $"{defaultPrefix}.{ModPackageId}";
    }

    private void OpenGameRegistry()
    {
        if (GameManager is null)
            return;

        if (GameManager.ShowGameRegistryDialog())
            FillGames();
    }

    private void Confirm()
    {
        ValidateAll();

        if (!CanConfirm())
            return;

        Closed?.Invoke(true);
    }

    private bool CanConfirm() => !HasErrors && GameVersions.Any(vm => vm.IsSelected);

    private void Cancel() => Closed?.Invoke(false);

    public IReadOnlyList<Game> GetSelectedGames() =>
        GameVersions
            .Where(vm => vm.IsSelected)
            .Select(vm => vm.Game)
            .ToArray();


    public class GameVersionViewModel(Game game)
    {
        public Game Game { get; } = game;
        public string Version { get; } = game.Version ?? throw new ArgumentException("Game version must be specified.", nameof(game));
        public bool IsSelected { get; set; }
    }
}