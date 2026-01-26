namespace UnityModStudio.Steam;

public class SteamAppInfo(uint appId, string name, string installDirectory, string workshopDirectory)
{
    public uint AppId { get; } = appId;
    public string Name { get; } = name;
    public string InstallDirectory { get; } = installDirectory;
    public string WorkshopDirectory { get; } = workshopDirectory;
}