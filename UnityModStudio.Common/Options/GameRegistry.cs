using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityModStudio.Common.GameSpecific.Versions;

namespace UnityModStudio.Common.Options;

[InheritedExport]
public interface IGameRegistry : IStore
{
    IReadOnlyCollection<Game> Games { get; }

    void AddGame(Game game);
    void RemoveGame(Game game);
    void EnsureAllGameProperties(Game game);
    void UpdateAllGameProperties(Game game, IEnumerable<IGameVersionResolver>? gameVersionResolvers = null);
    Game? FindGameById(Guid id);
    IReadOnlyCollection<Game> FindGamesByDisplayNameAndVersion(string displayName, string? version);
    IReadOnlyCollection<Game> FindGamesByGameNameAndVersion(string gameName, string? version);
    GameMatchResult FindGameByProperties(IReadOnlyDictionary<string, string> properties, bool strictMatch);
}

public sealed class GameRegistry(string storePath) : StoreBase<Game[]>(storePath), IGameRegistry
{
    private readonly Dictionary<Guid, Game> _games = new();
    
    public GameRegistry() : this(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        @"UnityModStudio\GameRegistry.json")) { }
    
    public IReadOnlyCollection<Game> Games => _games.Values;

    public void AddGame(Game game) => _games.Add(game.Id, game);

    public void RemoveGame(Game game) => _games.Remove(game.Id);

    public void EnsureAllGameProperties(Game game)
    {
        if (game is { GameName: not null, GameExecutableFileName: not null, Architecture: not null, UnityVersion: not null, TargetFrameworkMoniker: not null, MonoProfile: not null })
            return;

        if (!GameInformationResolver.TryGetGameInformation(game.Path, out var gameInformation, out _, out _))
        {
            Debug.WriteLine($"EnsureAllGameProperties failed to resolve game information for path '{game.Path}'.");
            return;
        }

        UpdateGame(game, gameInformation);
    }

    public void UpdateAllGameProperties(Game game, IEnumerable<IGameVersionResolver>? gameVersionResolvers = null)
    {
        if (!GameInformationResolver.TryGetGameInformation(game.Path, out var gameInformation, out _, out _))
        {
            Debug.WriteLine($"UpdateAllGameProperties failed to resolve game information for path '{game.Path}'.");
            return;
        }

        UpdateGame(game, gameInformation);

        var newVersion = gameVersionResolvers?.ResolveGameVersion(gameInformation);
        if (newVersion != null)
            game.Version = newVersion;
    }

    private static void UpdateGame(Game game, GameInformation gameInformation)
    {
        game.GameName = gameInformation.Name;
        game.GameExecutableFileName = gameInformation.GameExecutableFile.Name;
        game.Architecture = gameInformation.Architecture.ToString();
        game.UnityVersion = gameInformation.UnityVersion;
        game.TargetFrameworkMoniker = gameInformation.TargetFrameworkMoniker;
        game.MonoProfile = gameInformation.GetMonoProfileString();  
    }

    public Game? FindGameById(Guid id) => _games.TryGetValue(id, out var game) ? game : null;
    
    public IReadOnlyCollection<Game> FindGamesByDisplayNameAndVersion(string displayName, string? version) => 
        Games.Where(game => string.Equals(game.DisplayName, displayName, StringComparison.CurrentCultureIgnoreCase) &&
                                     string.Equals(game.Version ?? "", version ?? "", StringComparison.InvariantCultureIgnoreCase))
            .ToArray();

    public IReadOnlyCollection<Game> FindGamesByGameNameAndVersion(string gameName, string? version) => 
        Games.Where(game => string.Equals(game.GameName, gameName, StringComparison.InvariantCultureIgnoreCase) && 
                                     string.Equals(game.Version ?? "", version ?? "", StringComparison.InvariantCultureIgnoreCase))
            .ToArray();

    public GameMatchResult FindGameByProperties(IReadOnlyDictionary<string, string> properties, bool strictMatch)
    {
        // First match by ID alone. In non-strict mode check other properties, allowing ID to differ.
        if (properties.TryGetValue(nameof(Game.Id), out var idString))
        {
            if (_games.TryGetValue(Guid.Parse(idString), out var match))
                return new GameMatchResult.Match(match);
            if (strictMatch)
                return new GameMatchResult.NoMatch();
        }

        // Get both display name and game name, returning no match if neither is defined.
        if (!properties.TryGetValue(nameof(Game.DisplayName), out var displayName) & !properties.TryGetValue(nameof(Game.GameName), out var gameName))
            return new GameMatchResult.NoMatch();

        // In strict mode both must match when present, in non-strict mode allow display name to differ.
        var query = Games.AsEnumerable();
        if (strictMatch)
        {
            if (displayName != null)
                query = query.Where(game => game.DisplayName == displayName);
            if (gameName != null)
                query = query.Where(game => game.GameName == gameName);
        }
        else
            query = gameName != null
                ? query.Where(game => game.GameName == gameName)
                : query.Where(game => game.DisplayName == displayName);

        if (properties.TryGetValue(nameof(Game.Version), out var version))
            query = query.Where(game => game.Version == version);

        var matches = query.ToList();

        // Only match by empty version if ambiguous, so old projects which miss GameVersion won't fail.
        if (matches.Count > 1 && string.IsNullOrEmpty(version) && !strictMatch)
        {
            var matchesWithEmptyVersion = matches.FindAll(game => string.IsNullOrEmpty(game.Version));
            if (matchesWithEmptyVersion.Count > 0)
                matches = matchesWithEmptyVersion;
        }

        if (matches.Count > 0)
            return GameMatchResult.Create(matches, "");
        
        return new GameMatchResult.NoMatch();
    }

    protected override void Reset() => _games.Clear();

    protected override void Import(Game[] data)
    {
        foreach (var game in data)
            AddGame(game);
    }

    protected override Game[] Export() => Games.ToArray();

    public override string StoreType => "game registry";
}