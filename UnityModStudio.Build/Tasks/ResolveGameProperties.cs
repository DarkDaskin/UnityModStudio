﻿using System;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using UnityModStudio.Common;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Build.Tasks
{
    public class ResolveGameProperties : GameRegistryTaskBase
    {
        [Required]
        public ITaskItem[] LookupProperties { get; set; } = [];

        public bool BuildingInsideVisualStudio { get; set; }

        [Output]
        public ITaskItem? GamePath { get; private set; }

        [Output]
        public ITaskItem? GameModsPath { get; private set; }

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

        public override bool Execute()
        {
            var properties = LookupProperties.ToDictionary(item => item.ItemSpec, item => item.GetMetadata("Value"));
            if (properties.Count == 0)
            {
                Log.LogError("No game properties are defined.");
                return false;
            }

            LogLookupProperties(properties);
            
            switch (GameRegistry.FindGameByProperties(properties, false))
            {
                case GameMatchResult.Match match:
                    GamePath = new TaskItem(Utils.AppendTrailingSlash(match.Game.Path));
                    GameModsPath = match.Game.ModsPath != null ? new TaskItem(Utils.AppendTrailingSlash(match.Game.ModsPath)) : null;
                    GameExecutableFileName = match.Game.GameExecutableFileName;
                    GameInstanceId = match.Game.Id.ToString();
                    ModDeploymentMode = match.Game.ModDeploymentMode.ToString();
                    DeploySourceCode = match.Game.DeploySourceCode;
                    DoorstopMode = match.Game.DoorstopMode.ToString();
                    UseAlternateDoorstopDllName = match.Game.UseAlternateDoorstopDllName;
                    return true;

                case GameMatchResult.NoMatch:
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
    }
}