using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Build.Tasks;

public class UpdateGameRegistry : GameRegistryTaskBase
{
    public string? Id { get; set; }

    public string? DisplayName { get; set; }

    public string? GameName { get; set; }

    public string? Version { get; set; }

    public string? Path { get; set; }

    public string? ModsPath { get; set; }

    public string? ModDeploymentMode { get; set; }

    public string? DeploySourceCode { get; set; }

    public string? DoorstopMode { get; set; }

    public string? UseAlternateDoorstopDllName { get; set; }

    public override bool Execute()
    {
        var properties = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(Id))
            properties[nameof(Game.Id)] = Id!;
        if (!string.IsNullOrWhiteSpace(DisplayName))
            properties[nameof(Game.DisplayName)] = DisplayName!;
        if (!string.IsNullOrWhiteSpace(GameName))
            properties[nameof(Game.GameName)] = GameName!;
        if (!string.IsNullOrWhiteSpace(Version))
            properties[nameof(Game.Version)] = Version!;

        LogLookupProperties(properties);

        switch (GameRegistry.FindGameByProperties(properties, true))
        {
            case GameMatchResult.Match match:
                if (!string.IsNullOrWhiteSpace(DisplayName))
                    match.Game.DisplayName = DisplayName!;
                if (!string.IsNullOrWhiteSpace(Version))
                    match.Game.Version = Version!;
                if (!string.IsNullOrWhiteSpace(Path))
                    match.Game.Path = Path!;
                if (!string.IsNullOrWhiteSpace(ModsPath))
                    match.Game.ModsPath = ModsPath!;
                if (TryParseEnum(ModDeploymentMode, nameof(ModDeploymentMode), out ModDeploymentMode modDeploymentMode))
                    match.Game.ModDeploymentMode = modDeploymentMode;
                if (TryParseBoolean(DeploySourceCode, nameof(DeploySourceCode), out var deploySourceCode))
                    match.Game.DeploySourceCode = deploySourceCode;
                if (TryParseEnum(DoorstopMode, nameof(DoorstopMode), out DoorstopMode doorstopMode))
                    match.Game.DoorstopMode = doorstopMode;
                if (TryParseBoolean(UseAlternateDoorstopDllName, nameof(UseAlternateDoorstopDllName), out var useAlternateDoorstopDllName))
                    match.Game.UseAlternateDoorstopDllName = useAlternateDoorstopDllName;

                GameRegistry.Save();

                Log.LogMessage(MessageImportance.High, "Updated the game with ID '{0}' and display name '{1}' in the game registry.", match.Game.Id, match.Game.DisplayName);
                return true;

            case GameMatchResult.NoMatch:
                Log.LogError(NoMatchMessage);
                return false;

            case GameMatchResult.AmbiguousMatch match:
                Log.LogError(GetAmbiguousMatchMessage(match));
                return false;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}