using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using UnityModStudio.Build;

namespace UnityModStudio.RimWorld.Build.Tasks;

public class ResolveModReferencePaths : Task
{
    [Required]
    public string? ModsDirectoryPath { get; set; }

    [Required]
    public ITaskItem[] ModReferences { get; set; } = [];

    [Output]
    public ITaskItem[] ResolvedModReferences { get; set; } = [];

    [Output]
    public ITaskItem[] UnresolvedModReferences { get; set; } = [];

    public override bool Execute()
    {
        if (!Directory.Exists(ModsDirectoryPath))
        {
            Log.LogErrorWithCode("UMSRW0003", "Mods directory does not exist.");
            return false;
        }

        var resolvedModReferences = new List<ITaskItem>();
        var unresolvedModReferences = new List<ITaskItem>();
        var allModDirectories = new Dictionary<string, string>();
        using var modDirectoryEnumerator = Directory.EnumerateDirectories(ModsDirectoryPath!).GetEnumerator();
        foreach (var modReference in ModReferences)
        {
            if (allModDirectories.TryGetValue(modReference.ItemSpec, out var modDirectoryPath))
            {
                resolvedModReferences.Add(GetResolvedModReference(modReference, modDirectoryPath));
                continue;
            }

            var found = false;
            // Use an enumerator to lazily resolve mod package IDs, so each sub-directory is checked at most once.
            while (modDirectoryEnumerator.MoveNext())
            {
                modDirectoryPath = modDirectoryEnumerator.Current!;
                var modPackageId = GetModPackageId(modDirectoryPath);

                if (string.IsNullOrWhiteSpace(modPackageId))
                    continue;

                if (allModDirectories.ContainsKey(modPackageId!))
                {
                    Log.LogWarningWithCode("UMSRW0004", "Found multiple mods with same package ID: {0}", modPackageId!);
                    continue;
                }

                allModDirectories.Add(modPackageId!, modDirectoryPath);

                if (string.Equals(modPackageId, modReference.ItemSpec, StringComparison.InvariantCultureIgnoreCase))
                {
                    resolvedModReferences.Add(GetResolvedModReference(modReference, modDirectoryPath));
                    found = true;
                    break;
                }
            }

            if (!found)
                unresolvedModReferences.Add(modReference);
        }

        ResolvedModReferences = resolvedModReferences.ToArray();
        UnresolvedModReferences = unresolvedModReferences.ToArray();
        return true;
    }

    private static ITaskItem GetResolvedModReference(ITaskItem modReference, string modDirectoryPath)
    {
        var result = new TaskItem(modReference);
        result.SetMetadata("Path", modDirectoryPath);
        return result;
    }

    private static string? GetModPackageId(string modDirectoryPath)
    {
        var aboutFilePath = Path.Combine(modDirectoryPath, @"About\About.xml");
        if (!File.Exists(aboutFilePath))
            return null;

        try
        {
            var document = XDocument.Load(aboutFilePath);
            return (string?)document.XPathSelectElement("/ModMetaData/packageId");
        }
        catch
        {
            return null;
        }
    }
}