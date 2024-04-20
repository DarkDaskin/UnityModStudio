using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Directory = System.IO.Directory;

namespace UnityModStudio.Common.Options;

[InheritedExport]
public interface IGameRegistry
{
    IReadOnlyCollection<Game> Games { get; }
    bool WatchForChanges { get; set; }

    void AddGame(Game game);
    void RemoveGame(Game game);
    Game? FindGameById(Guid id);
    Game? FindGameByDisplayName(string name);
    GameMatchResult FindGameByProperties(IReadOnlyDictionary<string, string> properties, bool strictMatch);

    Task LoadAsync();
    Task SaveAsync();
}

public class GameRegistry : IGameRegistry
{
    private readonly string _storePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        @"UnityModStudio\GameRegistry.json");
    private readonly Dictionary<Guid, Game> _games = new();
    private readonly FileSystemWatcher _fsWatcher;

    public bool WatchForChanges
    {
        get => _fsWatcher.EnableRaisingEvents;
        set => _fsWatcher.EnableRaisingEvents = value;
    }

    public GameRegistry()
    {
        var directoryName = Path.GetDirectoryName(_storePath)!;
        Directory.CreateDirectory(directoryName);

        _fsWatcher = new FileSystemWatcher(directoryName, Path.GetFileName(_storePath))
        {
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = false,
        };
        _fsWatcher.Changed += OnStoreChanged;
    }

    private async void OnStoreChanged(object sender, FileSystemEventArgs e) => await LoadAsync();

    public IReadOnlyCollection<Game> Games => _games.Values;

    public void AddGame(Game game) => _games.Add(game.Id, game);

    public void RemoveGame(Game game) => _games.Remove(game.Id);

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
                matches = matches.FindAll(game => string.IsNullOrEmpty(game.Version));

            if (matches.Count > 0)
                return GameMatchResult.Create(matches, "");
        }

        return new GameMatchResult.NoMatch();
    }

    public async Task LoadAsync()
    {
        _games.Clear();

        try
        {
            using var stream = File.OpenRead(_storePath);
            foreach (var game in await JsonSerializer.DeserializeAsync<Game[]>(stream) ?? [])
                AddGame(game);
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Failed to load game registry: {exception.Message}");
        }
    }

    public async Task SaveAsync()
    {
        var oldWatchForChanges = WatchForChanges;
        WatchForChanges = false;

        try
        {
            using var stream = File.Open(_storePath, FileMode.Create);
            await JsonSerializer.SerializeAsync(stream, Games);
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Failed to save game registry: {exception.Message}");
        }
        finally
        {
            WatchForChanges = oldWatchForChanges;
        }
    }
}

public static class GameRegistryExtensions
{
    public static void Load(this IGameRegistry gameRegistry) => gameRegistry.LoadAsync().GetAwaiter().GetResult();

    public static void Save(this IGameRegistry gameRegistry) => gameRegistry.SaveAsync().GetAwaiter().GetResult();
}