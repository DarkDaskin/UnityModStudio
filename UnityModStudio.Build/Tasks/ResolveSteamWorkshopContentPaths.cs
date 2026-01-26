using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using UnityModStudio.Steam;

namespace UnityModStudio.Build.Tasks;

public class ResolveSteamWorkshopContentPaths : Task
{
    [Required]
    public uint SteamAppId { get; set; }

    [Required]
    public ITaskItem[] ContentReferences { get; set; } = [];

    [Output]
    public ITaskItem[] ResolvedContentReferences { get; set; } = [];

    [Output]
    public ITaskItem[] UnresolvedContentReferences { get; set; } = [];

    static ResolveSteamWorkshopContentPaths()
    {
        // Ensure proper version of Microsoft.Win32.Registry is loaded.
        FrameworkDependentAssemblyLoader.Enable();
    }

    public override bool Execute()
    {
        var (libraryFolders, exception) = GetSteamLibraryFoldersSafe();
        if (libraryFolders == null)
        {
            Log.LogErrorWithCode("UMSRW0005", "Could not locate Steam installation{0}", GetMessageSuffix(exception));
            return false;
        }

        (var appInfo, exception) = GetSteamAppInfoSafe(libraryFolders);
        if (appInfo == null)
        {
            Log.LogErrorWithCode("UMSRW0006", "Could not locate Steam application with AppID {0}{1}", SteamAppId, GetMessageSuffix(exception));
            return false;
        }

        var resolvedContentReferences = new List<ITaskItem>();
        var unresolvedContentReferences = new List<ITaskItem>();
        foreach (var contentReference in ContentReferences)
        {
            var path = Path.Combine(appInfo.WorkshopDirectory, contentReference.ItemSpec);
            if (Directory.Exists(path))
            {
                var resolvedReference = new TaskItem(contentReference);
                resolvedReference.SetMetadata("Path", path);
                resolvedContentReferences.Add(resolvedReference);
            }
            else
                unresolvedContentReferences.Add(contentReference);
        }

        ResolvedContentReferences = resolvedContentReferences.ToArray();
        UnresolvedContentReferences = unresolvedContentReferences.ToArray();
        return true;
    }

    private static (SteamLibraryFolders?, Exception?) GetSteamLibraryFoldersSafe()
    {
        try
        {
            return (SteamLibraryFolders.ForSteamInstallation(), null);
        }
        catch (Exception exception)
        {
            return (null, exception);
        }
    }

    private (SteamAppInfo?, Exception?) GetSteamAppInfoSafe(SteamLibraryFolders libraryFolders)
    {
        try
        {
            return (libraryFolders.FindFolderForAppId(SteamAppId)?.FindApplication(SteamAppId), null);
        }
        catch (Exception exception)
        {
            return (null, exception);
        }
    }

    private static string GetMessageSuffix(Exception? exception) => exception != null ? $": {exception.Message}" : ".";
}