using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using UnityModStudio.Common;
using UnityModStudio.Common.Options;

namespace UnityModStudio.ProjectSystem;

public class GameConfiguration
{
    public string? GamePath { get; set; }
    public string? GameExecutableFileName { get; set; }
    public DoorstopMode DoorstopMode { get; set; }

    public string? GameExecutablePath =>
        GamePath == null || GameExecutableFileName == null ? null : Path.Combine(GamePath, GameExecutableFileName);
    
    public static async Task<GameConfiguration> GetAsync(IProjectProperties properties, string? gameVersion)
    {
        var sanitizedGameVersion = Utils.SanitizeGameVersion(gameVersion);

        Enum.TryParse<DoorstopMode>(await properties.GetEvaluatedPropertyValueAsync(GetPropertyName(nameof(DoorstopMode), sanitizedGameVersion)), out var doorstopMode);

        return new GameConfiguration
        {
            GamePath = await properties.GetEvaluatedPropertyValueAsync(GetPropertyName(nameof(GamePath), sanitizedGameVersion)),
            GameExecutableFileName = await properties.GetEvaluatedPropertyValueAsync(GetPropertyName(nameof(GameExecutableFileName), sanitizedGameVersion)),
            DoorstopMode = doorstopMode,
        };
    }

    private static string GetPropertyName(string baseName, string? sanitizedGameVersion) =>
        string.IsNullOrEmpty(sanitizedGameVersion) ? baseName : $"{baseName}_{sanitizedGameVersion}";
}