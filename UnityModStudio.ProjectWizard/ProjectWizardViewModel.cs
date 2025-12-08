using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using UnityModStudio.Common;
using UnityModStudio.Common.GameSpecific;
using UnityModStudio.Common.Options;
using UnityModStudio.Options;

namespace UnityModStudio.ProjectWizard;

public class ProjectWizardViewModel : GamePropertiesViewModelBase
{
    private readonly Dictionary<Game, GameAssociatedInfo> _gameAssociatedInfo = [];
    private IReadOnlyList<Game> _games = [];
    private string? _templateRecommendations;
    private bool _isBasicTemplate;
    private string? _modLoaderId;
    private IGameManager? _gameManager;
    private IGameExtensionResolver[] _gameExtensionResolvers = [];
    private Game? _previousGame;

    public string? Error => string.Join("\n", GetErrors(nameof(GamePath)));
    public Visibility ErrorVisibility => HasValidGamePath ? Visibility.Collapsed : Visibility.Visible;
    public Visibility GameInformationVisibility => HasValidGamePath ? Visibility.Visible : Visibility.Hidden;

    public IReadOnlyList<Game> Games
    {
        get => _games;
        private set => SetProperty(ref _games, value);
    }

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

    public string? TemplateRecommendations
    {
        get => _templateRecommendations;
        set => SetProperty(ref _templateRecommendations, value);
    }

    public bool AreTemplateRecommendationsVisible => !string.IsNullOrEmpty(TemplateRecommendations);

    public string? ModLoaderId
    {
        get => _modLoaderId;
        set
        {
            SetProperty(ref _modLoaderId, value);

            FillGames();
        }
    }

    public bool IsBasicTemplate
    {
        get => _isBasicTemplate;
        set
        {
            SetProperty(ref _isBasicTemplate, value);

            FillRecommendations();
        }
    }

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

            FillGames();
        }
    }

    [ImportMany]
    public IGameExtensionResolver[] GameExtensionResolvers
    {
        get => _gameExtensionResolvers;
        set
        {
            SetProperty(ref _gameExtensionResolvers, value);

            FillGames();
        }
    }
    
    public ProjectWizardViewModel()
    {
        NewGameCommand = new DelegateCommand(NewGame, null, ThreadHelper.JoinableTaskFactory);
        UpdateGameCommand = new DelegateCommand(UpdateGame, () => Game != null, ThreadHelper.JoinableTaskFactory);
    }

    private void FillGames()
    {
        if (GameManager == null)
            return;

        IEnumerable<Game> games = GameManager.GameRegistry.Games;

        if (!string.IsNullOrEmpty(ModLoaderId))
            games = games.Where(SupportsModLoader);

        Games = games.ToList();
    }

    private bool SupportsModLoader(Game game) => GetGameExtensions(game).Any(extension => extension.ModLoaderId == ModLoaderId);

    private bool TryGetGameInformation(Game game, [NotNullWhen(true)] out GameInformation? gameInformation, [NotNullWhen(true)] out GameAssociatedInfo? info)
    {
        gameInformation = null;
        if (_gameAssociatedInfo.TryGetValue(game, out info))
            gameInformation = info.GameInformation;

        if (gameInformation == null)
        {
            if (!GameInformationResolver.TryGetGameInformation(game.Path, out gameInformation, out _, out _))
                return false;

            if (info == null)
            {
                info = new GameAssociatedInfo { GameInformation = gameInformation };
                _gameAssociatedInfo.Add(game, info);
            }
        }

        return true;
    }

    private IReadOnlyCollection<GameExtension> GetGameExtensions(Game game)
    {
        if (!TryGetGameInformation(game, out var gameInformation, out var info))
            return [];

        var extensions = info.Extensions;
        if (extensions == null)
        {
            extensions = GameExtensionResolvers
                .SelectMany(resolver => resolver.GetGameExtensions(gameInformation))
                .ToArray();
            info.Extensions = extensions;
        }
        return extensions;
    }

    private void InvalidateGame(Game game) => _gameAssociatedInfo.Remove(game);
    
    private void NewGame()
    {
        if (GameManager == null)
            return;

        var game = new Game();
        if (!GameManager.ShowEditDialog(game))
            return;

        GameManager.GameRegistry.AddGame(game);

        FillGames();

        Game = game;
    }

    private void UpdateGame()
    {
        if (GameManager == null || Game == null)
            return;
            
        if (!GameManager.ShowEditDialog(Game))
            return;

        InvalidateGame(Game);

        FillGames();

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

        FillRecommendations();
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

    private void FillRecommendations()
    {
        if (Game == null)
            return;

        try
        {
            if (!IsBasicTemplate)
            {
                TemplateRecommendations = null;
                return;
            }

            var extensions = GetGameExtensions(Game);

            var chosenExtensions = extensions.Where(extension => extension.HasNativeModSupport).ToArray();
            if (chosenExtensions.Any())
            {
                TemplateRecommendations = $"The selected game has native mod support.\n{GetExtensionString(chosenExtensions)}";
                return;
            }

            chosenExtensions = extensions.Where(extension => extension.IsModLoaderInstalled).ToArray();
            if (chosenExtensions.Any())
            {
                TemplateRecommendations = $"The selected game has a mod loader installed.\n{GetExtensionString(chosenExtensions)}";
                return;
            }

            if (extensions.Any())
            {
                TemplateRecommendations = $"The selected game can be used with a mod loader.\n{GetExtensionString(extensions)}";
                return;
            }

            TemplateRecommendations = null;
        }
        finally
        {
            NotifyPropertyChanged(nameof(AreTemplateRecommendationsVisible));
        }


        static string GetExtensionString(IReadOnlyCollection<GameExtension> extensions)
        {
            Debug.Assert(extensions.Count > 0);
            var what = extensions.Count > 1 ? "templates are" : "template is";
            var extensionString = string.Join("\n", extensions.Select(extension => $"- **{extension.TemplateName}** (from the **{extension.ExtensionName}** extension)"));
            return $"The following {what} recommended to use instead:\n{extensionString}";
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

    private class GameAssociatedInfo
    {
        public GameInformation? GameInformation { get; set; }
        public GameExtension[]? Extensions { get; set; }
    }
}