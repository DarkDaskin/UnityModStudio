using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;

namespace UnityModStudio.Options;

public sealed class AddGamesFromSteamViewModel : AddGamesViewModelBase
{
    protected override IEnumerable<GameEntry> FindGames()
    {
        using var steamKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam");
        if (steamKey == null)
            yield break;

        var steamPath = steamKey.GetValue("SteamPath") as string;
        if (string.IsNullOrEmpty(steamPath))
            yield break;

        foreach (var libraryFolder in GetSteamLibraryFolders(steamPath!))
        foreach (var gameEntry in GetGames(libraryFolder))
            yield return gameEntry;
    }

    private static IEnumerable<string> GetSteamLibraryFolders(string steamPath)
    {
        var libraryFoldersFile = Path.Combine(steamPath, @"steamapps\libraryfolders.vdf");
        var vdf = TryDeserialize(libraryFoldersFile);
        if (vdf == null)
            yield break;

        foreach (var folder in vdf.Value.Children<VProperty>())
            yield return folder.Value.Value<string>("path");
    }

    private static IEnumerable<GameEntry> GetGames(string libraryFolder)
    {
        foreach (var acfFile in Directory.EnumerateFiles(Path.Combine(libraryFolder, "steamapps"), "*.acf"))
        {
            var vdf = TryDeserialize(acfFile);
            if (vdf == null)
                continue;

            var name = vdf.Value.Value<string>("name");
            var installDir = vdf.Value.Value<string>("installdir");
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(installDir))
                yield return new GameEntry(name, Path.Combine(libraryFolder, @"steamapps\common", installDir));
        }
    }

    private static VProperty? TryDeserialize(string path)
    {
        try
        {
            using var reader = new StreamReader(path);
            return VdfConvert.Deserialize(reader);
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Error while reading '{path}': {exception.Message}");
            return null;
        }
    }
}