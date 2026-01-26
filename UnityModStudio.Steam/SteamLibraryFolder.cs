using System.Collections.Generic;
using System.IO;
using Gameloop.Vdf;

namespace UnityModStudio.Steam;

public class SteamLibraryFolder(string path)
{
    public string Path { get; set; } = path;
    public HashSet<uint> InstalledAppIds { get; } = [];

    public SteamAppInfo? FindApplication(uint appId)
    {
        var acfFilePath = System.IO.Path.Combine(Path, $@"steamapps\appmanifest_{appId}.acf");
        if (!File.Exists(acfFilePath))
            return null;

        using var reader = new StreamReader(acfFilePath);
        var vdf = VdfConvert.Deserialize(reader);
        var name = vdf.Value.Value<string>("name");
        var installDir = vdf.Value.Value<string>("installdir");
        return new SteamAppInfo(appId, name, 
            System.IO.Path.Combine(Path, @"steamapps\common", installDir),
            System.IO.Path.Combine(Path, @"steamapps\workshop\content", appId.ToString()));
    }
}