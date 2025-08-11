using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace UnityModStudio.Common.Options;

[InheritedExport]
public interface IGameRegistry : IStore
{
    IReadOnlyCollection<Game> Games { get; }

    void AddGame(Game game);
    void RemoveGame(Game game);
    void EnsureAllGameProperties(Game game);
    Game? FindGameById(Guid id);
    Game? FindGameByDisplayName(string name);
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
            return;

        game.GameName = gameInformation!.Name;
        game.GameExecutableFileName = gameInformation.GameExecutableFile.Name;
        game.Architecture = gameInformation.Architecture.ToString();
        game.UnityVersion = gameInformation.UnityVersion;
        game.TargetFrameworkMoniker = gameInformation.TargetFrameworkMoniker;
        game.MonoProfile = gameInformation.GetMonoProfileString();
    }

    public Game? FindGameById(Guid id) => _games.TryGetValue(id, out var game) ? game : null;

    public Game? FindGameByDisplayName(string name) => FindGamesByDisplayName(name).FirstOrDefault();

    private IEnumerable<Game> FindGamesByDisplayName(string name) => Games.Where(game => string.Equals(game.DisplayName, name, StringComparison.CurrentCultureIgnoreCase));

    public GameMatchResult FindGameByProperties(IReadOnlyDictionary<string, string> properties, bool strictMatch)
    {
        if (properties.TryGetValue(nameof(Game.Id), out var idString))
        {
            if (_games.TryGetValue(Guid.Parse(idString), out var match))
                return new GameMatchResult.Match(match);
            if (strictMatch)
                return new GameMatchResult.NoMatch();
        }

        if (properties.TryGetValue(nameof(Game.DisplayName), out var displayName))
        {
            var matches = FindGamesByDisplayName(displayName).ToArray();
            if (matches.Length > 0 || strictMatch)
                return GameMatchResult.Create(matches, "");
        }

        if (properties.TryGetValue(nameof(Game.GameName), out var gameName))
        {
            var query = Games.Where(game => game.GameName == gameName);

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
        }

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