using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Linq;
using UnityModStudio.Common;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Build.Tasks
{
    public class ResolveGameProperties : Task
    {
        [Required]
        public ITaskItem[] LookupProperties { get; set; } = Array.Empty<ITaskItem>();

        public bool BuildingInsideVisualStudio { get; set; }

        [Output]
        public ITaskItem? GamePath { get; private set; }

        [Output]
        public string? GameInstanceId { get; private set; }
        
        [Output]
        public string? DoorstopMode { get; private set; }

        [Output]
        public bool UseAlternateDoorstopDllName { get; private set; }

        public override bool Execute()
        {
            var properties = LookupProperties.ToDictionary(item => item.ItemSpec, item => item.GetMetadata("Value"));
            if (properties.Count == 0)
            {
                Log.LogError("No game properties are defined.");
                return false;
            }

            Log.LogMessage("Looking up the game registry by the following properties:\n  " +
                           string.Join("\n  ", properties.Select(kv => $"{kv.Key} = {kv.Value}")));

            // TODO: retrieve from VS?
            var gameRegistry = GetGameRegistry();

            switch (gameRegistry.FindGameByProperties(properties))
            {
                case GameMatchResult.Match match:
                    GamePath = new TaskItem(Utils.AppendTrailingSlash(match.Game.Path));
                    GameInstanceId = match.Game.Id.ToString();
                    DoorstopMode = match.Game.DoorstopMode.ToString();
                    UseAlternateDoorstopDllName = match.Game.UseAlternateDoorstopDllName;
                    return true;

                case GameMatchResult.NoMatch:
                    LogGameRegistryError("No game registry entries match speified game properties.");
                    return false;

                case GameMatchResult.AmbiguousMatch match:
                    LogGameRegistryError("Multiple game registry entries match speified game properties:\n  " +
                                         string.Join("\n  ", match.Games.Select(game => game.DisplayName)));
                    return false;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IGameRegistry GetGameRegistry()
        {
            var gameRegistry = (IGameRegistry?) BuildEngine4.GetRegisteredTaskObject(typeof(IGameRegistry), RegisteredTaskObjectLifetime.AppDomain);
            if (gameRegistry != null) 
                return gameRegistry;

            gameRegistry = new GameRegistry();
            gameRegistry.LoadAsync().GetAwaiter().GetResult();
            gameRegistry.WatchForChanges = true;
            BuildEngine4.RegisterTaskObject(typeof(IGameRegistry), gameRegistry, RegisteredTaskObjectLifetime.AppDomain, true);
            return gameRegistry;
        }

        private void LogGameRegistryError(string message)
        {
            if (BuildingInsideVisualStudio)
                message += "\nGo to Unity Mod Studio options to update the game registry.";

            Log.LogError(message);
        }
    }
}