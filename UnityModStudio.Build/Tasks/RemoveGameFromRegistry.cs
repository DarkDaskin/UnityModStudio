using Microsoft.Build.Framework;
using System.Collections.Generic;
using System;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Build.Tasks;

public class RemoveGameFromRegistry : GameRegistryTaskBase
{
    public string? Id { get; set; }

    public string? DisplayName { get; set; }

    public string? GameName { get; set; }

    public string? Version { get; set; }
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
                GameRegistry.RemoveGame(match.Game);
                GameRegistry.Save();

                Log.LogMessage(MessageImportance.High, "Removed the game with ID '{0}' and display name '{1}' from the game registry.", match.Game.Id, match.Game.DisplayName);
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