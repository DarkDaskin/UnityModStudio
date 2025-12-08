namespace UnityModStudio.Common.GameSpecific;

public class GameExtension
{
    public string ExtensionName { get; set; } = null!;
    public string TemplateName { get; set; } = null!;
    public string? ModLoaderId { get; set; }
    public bool IsModLoaderInstalled { get; set; }
    public bool HasNativeModSupport { get; set; }
}