using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;

namespace UnityModStudio.Steam;

public class SteamLibraryFolders : IReadOnlyList<SteamLibraryFolder>
{
    private readonly IReadOnlyList<SteamLibraryFolder> _contents;

    public SteamLibraryFolders(string path)
    {
        _contents = LoadLibraryFolders(File.OpenText(path), true);
    }

    public SteamLibraryFolders(TextReader reader)
    {
        _contents = LoadLibraryFolders(reader, false);
    }

    private static IReadOnlyList<SteamLibraryFolder> LoadLibraryFolders(TextReader reader, bool ownsReader)
    {
        var libraryFolders = new List<SteamLibraryFolder>();

        var vdf = VdfConvert.Deserialize(reader);
        foreach (var folderElement in vdf.Value.Children<VProperty>())
        {
            var folder = new SteamLibraryFolder(folderElement.Value.Value<string>("path"));
            foreach (var appElement in folderElement.Value.Value<VObject>("apps").Children<VProperty>())
                folder.InstalledAppIds.Add(uint.Parse(appElement.Key));
            libraryFolders.Add(folder);
        }

        if (ownsReader)
            reader.Close();

        return libraryFolders;
    }

    public static SteamLibraryFolders? ForSteamInstallation()
    {
        using var steamKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam");
        if (steamKey == null)
            return null;

        var steamPath = steamKey.GetValue("SteamPath") as string;
        if (string.IsNullOrEmpty(steamPath))
            return null;

        var libraryFoldersFile = Path.Combine(steamPath, @"steamapps\libraryfolders.vdf");
        if (!File.Exists(libraryFoldersFile))
            return null;

        return new SteamLibraryFolders(libraryFoldersFile);
    }

    public IEnumerator<SteamLibraryFolder> GetEnumerator() => _contents.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _contents.Count;

    public SteamLibraryFolder this[int index] => _contents[index];

    public SteamLibraryFolder? FindFolderForAppId(uint appId) =>
        _contents.FirstOrDefault(folder => folder.InstalledAppIds.Contains(appId));
}