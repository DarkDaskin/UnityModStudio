using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem.Properties;

namespace UnityModStudio.ProjectSystem;

public class GameConfiguration
{
    public string? GamePath { get; set; }
    public string? GameExecutableFileName { get; set; }

    public string? GameExecutablePath =>
        GamePath == null || GameExecutableFileName == null ? null : Path.Combine(GamePath, GameExecutableFileName);
    
    public static async Task<GameConfiguration> GetAsync(IProjectProperties properties)
    {
        return new GameConfiguration
        {
            GamePath = await properties.GetEvaluatedPropertyValueAsync(nameof(GamePath)),
            GameExecutableFileName = await properties.GetEvaluatedPropertyValueAsync(nameof(GameExecutableFileName)),
        };
    }
}