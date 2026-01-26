using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityModStudio.Steam;

namespace UnityModStudio.Options;

public sealed class AddGamesFromSteamViewModel : AddGamesViewModelBase
{
    protected override IEnumerable<GameEntry> FindGames()
    {
        SteamLibraryFolders? libraryFolders = null;
        try
        {
            libraryFolders = SteamLibraryFolders.ForSteamInstallation();
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Error while reading Steam library folders: {exception.Message}");
        }

        if (libraryFolders == null)
            yield break;

        foreach (var libraryFolder in libraryFolders)
        foreach (var gameEntry in GetGames(libraryFolder))
            yield return gameEntry;
    }

    private static IEnumerable<GameEntry> GetGames(SteamLibraryFolder libraryFolder)
    {
        foreach (var appId in libraryFolder.InstalledAppIds)
        {
            SteamAppInfo? appInfo = null;
            try
            {
                appInfo = libraryFolder.FindApplication(appId);
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Error while reading app info for {appId}: {exception.Message}");
            }

            if (appInfo == null)
                continue;

            if (!string.IsNullOrEmpty(appInfo.Name) && Directory.Exists(appInfo.InstallDirectory))
                yield return new GameEntry(appInfo.Name, appInfo.InstallDirectory);
        }
    }
}