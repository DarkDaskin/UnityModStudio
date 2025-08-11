using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using UnityModStudio.Common;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Build.Tasks
{
    public class ResolveGameProperties : GameRegistryTaskBase
    {
        [Required]
        public ITaskItem[] LookupProperties { get; set; } = [];

        public string? ProjectDirectory { get; set; }

        public bool BuildingInsideVisualStudio { get; set; }

        public bool IsAmbientGameResolutionAllowed { get; set; } = true;

        [Output]
        public string? GamePath { get; private set; }

        [Output]
        public string? GameModsPath { get; private set; }

        [Output]
        public string? GameExecutableFileName { get; private set; }

        [Output]
        public string? GameInstanceId { get; private set; }
        
        [Output]
        public string? ModDeploymentMode { get; private set; }

        [Output]
        public bool DeploySourceCode { get; private set; }

        [Output]
        public string? DoorstopMode { get; private set; }

        [Output]
        public bool UseAlternateDoorstopDllName { get; private set; }

        [Output]
        public bool IsAmbientGame { get; private set; }

        public override bool Execute()
        {
            if (!base.Execute())
                return false;

            var properties = LookupProperties.ToDictionary(item => item.ItemSpec, item => item.GetMetadata("Value"));
            if (properties.Count == 0)
            {
                if (ResolveAmbientGame(properties))
                    return true;

                Log.LogError("No game properties are defined.");
                return false;
            }

            LogLookupProperties(properties);
            
            switch (Store.FindGameByProperties(properties, false))
            {
                case GameMatchResult.Match match:
                    GamePath = Utils.AppendTrailingSlash(match.Game.Path);
                    GameModsPath = match.Game.ModsPath != null ? Utils.AppendTrailingSlash(match.Game.ModsPath) : null;
                    GameExecutableFileName = match.Game.GameExecutableFileName;
                    GameInstanceId = match.Game.Id.ToString();
                    ModDeploymentMode = match.Game.ModDeploymentMode.ToString();
                    DeploySourceCode = match.Game.DeploySourceCode;
                    DoorstopMode = match.Game.DoorstopMode.ToString();
                    UseAlternateDoorstopDllName = match.Game.UseAlternateDoorstopDllName;
                    return true;

                case GameMatchResult.NoMatch:
                    if (ResolveAmbientGame(properties))
                        return true;

                    LogGameRegistryError(NoMatchMessage);
                    return false;

                case GameMatchResult.AmbiguousMatch match:
                    LogGameRegistryError(GetAmbiguousMatchMessage(match));
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

        private bool ResolveAmbientGame(Dictionary<string, string> properties)
        {
            if (!IsAmbientGameResolutionAllowed || !Directory.Exists(ProjectDirectory))
                return false;

            var directory = new DirectoryInfo(ProjectDirectory!).Parent;
            while (directory != null)
            {
                if (GameInformationResolver.TryGetGameInformation(directory.FullName, out var gameInformation, out _))
                {
                    IsAmbientGame = true;
                    GamePath = Utils.AppendTrailingSlash(directory.FullName);
                    GameExecutableFileName = gameInformation.GameExecutableFile.Name;

                    if (properties.TryGetValue(nameof(Game.GameName), out var gameName) && 
                        !string.Equals(gameInformation.Name, gameName, StringComparison.CurrentCultureIgnoreCase)) 
                        Log.LogWarning($"Ambient game name is '{gameInformation.Name}', but '{gameName}' is defined by the project.");

                    return true;
                }

                directory = directory.Parent;
            }

            return false;
        }
    }
}