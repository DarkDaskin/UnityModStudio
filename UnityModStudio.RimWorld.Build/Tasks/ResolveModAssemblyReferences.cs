using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using UnityModStudio.Build;
using UnityModStudio.Common;

namespace UnityModStudio.RimWorld.Build.Tasks;

public class ResolveModAssemblyReferences : Task
{
    private static readonly GameVersionComparer GameVersionComparer = new();
    private static readonly FileNameComparer FileNameComparer = new();
    private static readonly string[] CommonDirectories = ["Common", "."];

    [Required] 
    public string? GameVersion { get; set; }

    [Required] 
    public ITaskItem[] ModReferences { get; set; } = [];

    [Output]
    public ITaskItem[] ResolvedAssemblyReferences { get; set; } = [];

    public override bool Execute()
    {
        var resolvedAssemblyReferences = new List<ITaskItem>();
        foreach (var modReference in ModReferences)
        {
            var packageId = modReference.ItemSpec;
            var modDirectoryPath = modReference.GetMetadata("Path");
            var assemblyNames = modReference.GetMetadata("Assemblies")?.Split([';'], StringSplitOptions.RemoveEmptyEntries) ?? [];

            var declaredPackageId = GetDeclaredPackageId(modDirectoryPath);
            if (!StringComparer.OrdinalIgnoreCase.Equals(packageId, declaredPackageId))
            {
                Log.LogErrorWithCode("UMSRW0008", "Package ID mismatch for mod at '{0}': expected '{1}', found '{2}'.",
                    modDirectoryPath, packageId, declaredPackageId ?? "<none>");
                continue;
            }

            var loadFolders = GetExplicitLoadFoldersForCurrentVersion(modDirectoryPath) ??
                              GetImplicitLoadFoldersForCurrentVersion(modDirectoryPath);
            var assemblies = loadFolders
                .Select(name => Path.Combine(modDirectoryPath, name))
                .Where(Directory.Exists)
                .SelectMany(path => Directory.EnumerateDirectories(path, "Assemblies")
                    .SelectMany(path2 => Directory.EnumerateFiles(path2, "*.dll")))
                .Distinct(FileNameComparer);
            if (assemblyNames.Any())
                assemblies = assemblies.Where(path => assemblyNames.Contains(Path.GetFileNameWithoutExtension(path)));
            foreach (var assemblyPath in assemblies)
            {
                var item = new TaskItem(Path.GetFileNameWithoutExtension(assemblyPath));
                item.SetMetadata("HintPath", assemblyPath);
                item.SetMetadata("Private", "false");
                item.SetMetadata("IsImplicitlyDefined", "true");
                item.SetMetadata("IsImplicitlyDefinedModReference", "true");
                resolvedAssemblyReferences.Add(item);
            }
        }

        ResolvedAssemblyReferences = resolvedAssemblyReferences.ToArray();
        return !Log.HasLoggedErrors;
    }

    private string? GetDeclaredPackageId(string modDirectoryPath)
    {
        var aboutFilePath = Path.Combine(modDirectoryPath, @"About\About.xml");
        if (!File.Exists(aboutFilePath))
            return null;

        try
        {
            var document = XDocument.Load(aboutFilePath);
            if (document.Root?.Name != "ModMetaData")
                throw new XmlException("Missing or incorrect root element.");

            var packageId = (string?)document.Root.Element("packageId");
            return string.IsNullOrWhiteSpace(packageId) ? null : packageId;
        }
        catch (Exception exception)
        {
            Log.LogErrorWithCode("UMSRW0002", "Error while parsing About.xml: {0}", exception.Message);
            return null;
        }
    }

    private string[]? GetExplicitLoadFoldersForCurrentVersion(string modDirectoryPath)
    {
        var loadFoldersFilePath = Path.Combine(modDirectoryPath, "LoadFolders.xml");
        if (!File.Exists(loadFoldersFilePath))
            return null;

        try
        {
            var document = XDocument.Load(loadFoldersFilePath);
            if (document.Root?.Name != "loadFolders")
                throw new XmlException("Missing or incorrect root element.");

            var versionElement = document.Root.Elements()
                .FirstOrDefault(element => element.Name.LocalName.TrimStart('v') == GameVersion);
            if (versionElement == null)
                return [];
            return versionElement.Elements("li")
                .Select(pathElement => GetLoadFolders.NormalizePath(pathElement.Value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

        }
        catch (Exception exception)
        {
            Log.LogErrorWithCode("UMSRW0001", "Error while parsing LoadFolders.xml: {0}", exception.Message);
            return null;
        }
    }

    private string[] GetImplicitLoadFoldersForCurrentVersion(string modDirectoryPath)
    {
        var versionedDirectory = Directory.EnumerateDirectories(modDirectoryPath)
            .Select(Path.GetFileName)
            .OrderByDescending(version => version, GameVersionComparer)
            .FirstOrDefault(version => GameVersionComparer.Compare(version, GameVersion) <= 0);
        return versionedDirectory == null ? CommonDirectories : [versionedDirectory, ..CommonDirectories];
    }
}