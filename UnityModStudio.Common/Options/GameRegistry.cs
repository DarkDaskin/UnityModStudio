using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Directory = System.IO.Directory;

namespace UnityModStudio.Common.Options
{
    [InheritedExport]
    public interface IGameRegistry
    {
        IReadOnlyCollection<Game> Games { get; }

        void AddGame(Game game);
        void RemoveGame(Game game);
        Game? FindGameByName(string name);
        GameMatchResult FindGameByProperties(IReadOnlyDictionary<string, string> properties);

        Task LoadAsync();
        Task SaveAsync();
    }

    public class GameRegistry : IGameRegistry
    {
        private readonly string _storePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"UnityModStudio\GameRegistry.json");
        private readonly List<Game> _games = new();

        public IReadOnlyCollection<Game> Games => _games;

        public void AddGame(Game game) => _games.Add(game);

        public void RemoveGame(Game game) => _games.Remove(game);

        public Game? FindGameByName(string name) => _games.Find(game => string.Equals(game.DisplayName, name, StringComparison.CurrentCultureIgnoreCase));

        public GameMatchResult FindGameByProperties(IReadOnlyDictionary<string, string> properties)
        {
            IReadOnlyList<Game> matches = Array.Empty<Game>();

            // First, check whether the user has moved or renamed the game.
            // Both DisplayName and Path have equal priority and must not point to different records.
            if (properties.TryGetValue(nameof(Game.DisplayName), out var displayName) | properties.TryGetValue(nameof(Game.Path), out var path))
            {
                matches = _games.FindAll(game => game.DisplayName == displayName || game.Path == path);
                if (matches.Count > 0)
                    return GameMatchResult.Create(matches, "");
            }

            // TODO: some way to determine which keys to match
            if (properties.TryGetValue(nameof(Game.GameName), out var gameName))
            {
                matches = _games.FindAll(game => game.GameName == gameName);
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
                _games.AddRange(await JsonSerializer.DeserializeAsync<Game[]>(stream) ?? Array.Empty<Game>());
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Failed to load game registry: {exception.Message}");
            }
        }

        public async Task SaveAsync()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_storePath)!);

                using var stream = File.OpenWrite(_storePath);
                await JsonSerializer.SerializeAsync(stream, _games);
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Failed to save game registry: {exception.Message}");
            }
        }
    }
}