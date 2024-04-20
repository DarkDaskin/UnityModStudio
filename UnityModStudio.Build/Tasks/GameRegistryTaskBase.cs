using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Build.Tasks;

public abstract class GameRegistryTaskBase : Task
{
    private readonly Lazy<IGameRegistry> _gameRegistry;

    protected IGameRegistry GameRegistry => _gameRegistry.Value;

    protected GameRegistryTaskBase()
    {
        _gameRegistry = new Lazy<IGameRegistry>(GetGameRegistry);
    }

    // TODO: retrieve from VS?
    private IGameRegistry GetGameRegistry()
    {
        var gameRegistry = (IGameRegistry?)BuildEngine4.GetRegisteredTaskObject(typeof(IGameRegistry), RegisteredTaskObjectLifetime.AppDomain);
        if (gameRegistry != null)
            return gameRegistry;

        gameRegistry = new GameRegistry();
        gameRegistry.Load();
        gameRegistry.WatchForChanges = true;
        BuildEngine4.RegisterTaskObject(typeof(IGameRegistry), gameRegistry, RegisteredTaskObjectLifetime.AppDomain, true);
        return gameRegistry;
    }

    protected void LogLookupProperties(Dictionary<string, string> properties)
    {
        Log.LogMessage("Looking up the game registry by the following properties:\n  " +
                       string.Join("\n  ", properties.Select(kv => $"'{kv.Key}' = '{kv.Value}'")));
    }

    protected const string NoMatchMessage = "No game registry entries match speified game properties.";
    
    protected static string GetAmbiguousMatchMessage(GameMatchResult.AmbiguousMatch match)
    {
        return "Multiple game registry entries match speified game properties:\n  " +
               string.Join("\n  ", match.Games.Select(game => $"'{game.DisplayName}'"));
    }

    private const string ParameterHasInvalidValueMessageFormat = "Parameter '{0}' has invalid value '{1}'. Ignoring.";

    protected bool TryParseBoolean(string? value, string parameterName, out bool result)
    {
        if (bool.TryParse(value, out result))
            return true;
        
        if (!string.IsNullOrEmpty(value))
            Log.LogWarning(ParameterHasInvalidValueMessageFormat, parameterName, value);

        return false;
    }

    protected bool TryParseEnum<TEnum>(string? value, string parameterName, out TEnum result) where TEnum : struct
    {
        if (Enum.TryParse(value, true, out result))
            return true;
        
        if (!string.IsNullOrEmpty(value))
            Log.LogWarning(ParameterHasInvalidValueMessageFormat, parameterName, value);

        return false;
    }
}