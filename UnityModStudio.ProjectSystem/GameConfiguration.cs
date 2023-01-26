using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using UnityModStudio.Common.Options;

namespace UnityModStudio.ProjectSystem;

public class GameConfiguration
{
    public string? GamePath { get; set; }
    public string? GameExecutableFileName { get; set; }
    public DoorstopMode DoorstopMode { get; set; }

    public string? GameExecutablePath =>
        GamePath == null || GameExecutableFileName == null ? null : Path.Combine(GamePath, GameExecutableFileName);
    
    public static async Task<GameConfiguration> GetAsync(IProjectProperties properties)
    {
        Enum.TryParse<DoorstopMode>(await properties.GetEvaluatedPropertyValueAsync(nameof(DoorstopMode)), out var doorstopMode);

        return new GameConfiguration
        {
            GamePath = await properties.GetEvaluatedPropertyValueAsync(nameof(GamePath)),
            GameExecutableFileName = await properties.GetEvaluatedPropertyValueAsync(nameof(GameExecutableFileName)),
            DoorstopMode = doorstopMode,
        };
    }
}