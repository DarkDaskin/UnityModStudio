using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using UnityModStudio.Common;
using UnityModStudio.Common.Options;
using UnityModStudio.Options;

namespace UnityModStudio.ProjectWizard;

public class ProjectWizardViewModel : GamePropertiesViewModelBase
{
    private IGameManager? _gameManager;
    private Game? _previousGame;

    public string? Error => string.Join("\n", GetErrors(nameof(GamePath)));
    public Visibility ErrorVisibility => HasValidGamePath ? Visibility.Collapsed : Visibility.Visible;
    public Visibility GameInformationVisibility => HasValidGamePath ? Visibility.Visible : Visibility.Hidden;

    public IReadOnlyCollection<Game> Games => GameManager?.GameRegistry.Games ?? [];

    public string ModDeploymentModeString => Game?.ModDeploymentMode.ToString() ?? "";

    public string DeploySourceCodeString => Game?.DeploySourceCode switch
    {
        true => "Yes",
        false => "No",
        _ => ""
    };

    public string DoorstopModeString => Game?.DoorstopMode switch
    {
        DoorstopMode.Disabled => "Disabled",
        DoorstopMode.Debugging => "Use for debugging",
        DoorstopMode.DebuggingAndModLoading => "Use for debugging and mod loading",
        _ => ""
    };

    public string DoorstopDllName => Game?.UseAlternateDoorstopDllName switch
    {
        false => "winhttp.dll",
        true => "version.dll",
        _ => ""
    };

    public bool IsDoorstopDllNameVisible => Game != null && Game.DoorstopMode != DoorstopMode.Disabled;

    public IReadOnlyList<GameVersionViewModel> GameVersions { get; private set; } = [];

    public bool IsMultiVersionPanelVisible => GameVersions.Count > 1;

    public string? GameVersionString => string.IsNullOrWhiteSpace(GameVersion) ? "<default>" : GameVersion;

    public ICommand NewGameCommand { get; }
    public ICommand UpdateGameCommand { get; }

    [Import]
    public IGameManager? GameManager
    {
        get => _gameManager;
        set
        {
            SetProperty(ref _gameManager, value);

            if (_gameManager != null)
                ThreadHelper.JoinableTaskFactory.Run(_gameManager.GameRegistry.LoadSafeAsync);

            NotifyPropertyChanged(nameof(Games));
        }
    }


    public ProjectWizardViewModel()
    {
        NewGameCommand = new DelegateCommand(NewGame, null, ThreadHelper.JoinableTaskFactory);
        UpdateGameCommand = new DelegateCommand(UpdateGame, () => Game != null, ThreadHelper.JoinableTaskFactory);
    }

    private void NewGame()
    {
        if (GameManager == null)
            return;

        var game = new Game();
        if (!GameManager.ShowEditDialog(game))
            return;

        GameManager.GameRegistry.AddGame(game);

        CollectionViewSource.GetDefaultView(Games).Refresh();

        Game = game;
    }

    private void UpdateGame()
    {
        if (GameManager == null || Game == null)
            return;
            
        if (!GameManager.ShowEditDialog(Game))
            return;

        CollectionViewSource.GetDefaultView(Games).Refresh();

        // Trigger property sync.
        GamePath = Game.Path;
        ModsPath = Game.ModsPath;
        GameVersion = Game.Version;

        RefreshProperties();
    }

    protected override bool ValidateGamePath()
    {
        var result = base.ValidateGamePath();

        RefreshProperties();

        return result;
    }

    protected override void RefreshProperties()
    {
        base.RefreshProperties();

        NotifyPropertyChanged(nameof(Error));
        NotifyPropertyChanged(nameof(ErrorVisibility));
        NotifyPropertyChanged(nameof(GameInformationVisibility));
        NotifyPropertyChanged(nameof(GameVersionString));
        NotifyPropertyChanged(nameof(ModDeploymentModeString));
        NotifyPropertyChanged(nameof(DeploySourceCodeString));
        NotifyPropertyChanged(nameof(DoorstopModeString));
        NotifyPropertyChanged(nameof(DoorstopDllName));
        NotifyPropertyChanged(nameof(IsDoorstopDllNameVisible));

        FillVersions();
    }

    private void FillVersions()
    {
        if (GameManager == null)
            return;

        if (Game == _previousGame)
            return;

        GameVersions = Games
            .Where(game => game.Version is not null && EnsureGameName(game) == GameName)
            .Select(game => new GameVersionViewModel(game, game == Game))
            .OrderBy(vm => vm.Version, new GameVersionComparer())
            .ToList();
        foreach (var vm in GameVersions)
            GameManager.GameRegistry.EnsureAllGameProperties(vm.Game);
        NotifyPropertyChanged(nameof(GameVersions));
        NotifyPropertyChanged(nameof(IsMultiVersionPanelVisible));

        _previousGame = Game;


        string EnsureGameName(Game game)
        {
            if (game.GameName is null)
                GameManager!.GameRegistry.EnsureAllGameProperties(game);

            Debug.Assert(game.GameName != null, "game.GameName != null");
            return game.GameName!;
        }
    }

    protected override void OnConfirm()
    {
        base.OnConfirm();

        if (GameManager == null)
            return;

        ThreadHelper.JoinableTaskFactory.Run(GameManager.GameRegistry.SaveSafeAsync);
    }

    public Game[] GetSelectedGames()
    {
        var selectedGames = GameVersions
            .Where(vm => vm.IsSelected)
            .Select(vm => vm.Game)
            .ToArray();
        if (selectedGames.Length == 0 && Game is not null)
            selectedGames = [Game];
        return selectedGames;
    }


    public class GameVersionViewModel(Game game, bool isDefault)
    {
        public Game Game { get; } = game;
        public string Version { get; } = game.Version ?? throw new ArgumentException("Game version must be specified.", nameof(game));
        public bool IsEnabled { get; } = !isDefault;
        public bool IsSelected { get; set; } = isDefault;
    }
}