using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Build.Tasks;

public abstract class GameRegistryTaskBase : Task
{
    protected IGameRegistry? GameRegistry { get; private set; }

    [Required] 
    public string GameRegistryPath { get; set; } = "";
    
    [MemberNotNullWhen(true, nameof(GameRegistry))]
    public override bool Execute()
    {
        try
        {
            GameRegistry = GetGameRegistry();
            return true;
        }
        catch (Exception exception)
        {
            Log.LogError($"Unable to initialize game registry: {exception.Message}");
            return false;
        }
    }

    // TODO: retrieve from VS?
    private IGameRegistry GetGameRegistry()
    {
        var holder = (GameRegistryHolder?)BuildEngine4.GetRegisteredTaskObject(typeof(GameRegistryHolder), RegisteredTaskObjectLifetime.AppDomain);
        if (holder != null)
        {
            if (holder.GameRegistryPath == GameRegistryPath) 
                return holder.GameRegistry;

            BuildEngine4.UnregisterTaskObject(typeof(GameRegistryHolder), RegisteredTaskObjectLifetime.AppDomain);
            holder.Dispose();
        }

        var gameRegistry = new GameRegistry(GameRegistryPath);
        gameRegistry.Load();
        gameRegistry.WatchForChanges = true;
        holder = new GameRegistryHolder(gameRegistry, GameRegistryPath);
        BuildEngine4.RegisterTaskObject(typeof(GameRegistryHolder), holder, RegisteredTaskObjectLifetime.AppDomain, true);
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

    private class GameRegistryHolder(IGameRegistry gameRegistry, string? gameRegistryPath) : IDisposable
    {
        public readonly IGameRegistry GameRegistry = gameRegistry;
        public readonly string? GameRegistryPath = gameRegistryPath;

        public void Dispose()
        {
            if (GameRegistry is IDisposable disposable)
                disposable.Dispose();
        }
    }
}