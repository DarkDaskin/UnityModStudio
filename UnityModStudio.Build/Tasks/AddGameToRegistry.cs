﻿using System;
using Microsoft.Build.Framework;
using UnityModStudio.Common;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Build.Tasks;

public class AddGameToRegistry : GameRegistryTaskBase
{
    [Required]
    public string? Path { get; set; }

    [Output]
    public string? DisplayName { get; set; }

    public string? ModsPath { get; set; }

    public string? Version { get; set; }

    public string? ModDeploymentMode { get; set; }

    public string? DeploySourceCode { get; set; }

    public string? DoorstopMode { get; set; }

    public string? UseAlternateDoorstopDllName { get; set; }
    
    public override bool Execute()
    {
        if (!base.Execute())
            return false;

        if (!GameInformationResolver.TryGetGameInformation(Path, out var gameInformation, out var error, out var errorCode))
        {
            Log.LogErrorWithCode(errorCode, error);
            return false;
        }

        DisplayName = GetUniqueDisplayName(gameInformation);

        var game = new Game
        {
            Path = Path,
            DisplayName = DisplayName,
            ModsPath = ModsPath,
            Version = Version,
            GameName = gameInformation.Name,
            GameExecutableFileName = gameInformation.GameExecutableFile.Name,
            Architecture = gameInformation.Architecture.ToString(),
            UnityVersion = gameInformation.UnityVersion,
            TargetFrameworkMoniker = gameInformation.TargetFrameworkMoniker,
            MonoProfile = gameInformation.GetMonoProfileString(),
        };
        if (TryParseEnum(ModDeploymentMode, nameof(ModDeploymentMode), out ModDeploymentMode modDeploymentMode))
            game.ModDeploymentMode = modDeploymentMode;
        if (TryParseBoolean(DeploySourceCode, nameof(DeploySourceCode), out var deploySourceCode)) 
            game.DeploySourceCode = deploySourceCode;
        if (TryParseEnum(DoorstopMode, nameof(DoorstopMode), out DoorstopMode doorstopMode)) 
            game.DoorstopMode = doorstopMode;
        if (TryParseBoolean(UseAlternateDoorstopDllName, nameof(UseAlternateDoorstopDllName), out var useAlternateDoorstopDllName)) 
            game.UseAlternateDoorstopDllName = useAlternateDoorstopDllName;

        Store.AddGame(game);
        Store.Save();

        Log.LogMessage(MessageImportance.High, "Added a game with ID '{0}' and display name '{1}' to the game registry.", game.Id, game.DisplayName);

        return true;
    }

    private string GetUniqueDisplayName(GameInformation gameInformation)
    {
        var baseDisplayName = string.IsNullOrWhiteSpace(DisplayName) ? gameInformation.Name ?? "Game" : DisplayName!.Trim();
        if (string.IsNullOrWhiteSpace(DisplayName) && !string.IsNullOrWhiteSpace(Version))
            baseDisplayName = $"{baseDisplayName} [{Version}]";
        if (Store!.FindGameByDisplayName(baseDisplayName) is null)
            return baseDisplayName;

        for (var i = 1; i < int.MaxValue; i++)
        {
            var displayNameWithSuffix = $"{baseDisplayName} ({i})";
            if (Store.FindGameByDisplayName(displayNameWithSuffix) is null)
                return displayNameWithSuffix;
        }

        throw new InvalidOperationException();
    }
}