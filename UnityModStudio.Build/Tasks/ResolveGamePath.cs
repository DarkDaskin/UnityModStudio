using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Linq;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Build.Tasks
{
    public class ResolveGamePath : Task
    {
        [Required]
        public ITaskItem[] LookupProperties { get; set; } = Array.Empty<ITaskItem>();

        public bool BuildingInsideVisualStudio { get; set; }

        [Output]
        public ITaskItem? GamePath { get; private set; }

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

            var gameRegistry = new GameRegistry();
            gameRegistry.LoadAsync().GetAwaiter().GetResult();

            switch (gameRegistry.FindGameByProperties(properties))
            {
                case GameMatchResult.Match match:
                    GamePath = new TaskItem(match.Game.Path);
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

        private void LogGameRegistryError(string message)
        {
            if (BuildingInsideVisualStudio)
                message += "\nGo to Unity Mod Studio options to update the game registry.";

            Log.LogError(message);
        }
    }
}