using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using UnityModStudio.Common;
using DirectoryIO = System.IO.Directory;

namespace UnityModStudio.Build.Tasks;

public class GetIgnoredFiles : Task
{
    [Required]
    public string? Directory { get; set; }

    public ITaskItem[] IgnoreListFiles { get; set; } = [];

    [Output]
    public ITaskItem[] IgnoredFiles { get; set; } = [];

    public override bool Execute()
    {
        if (string.IsNullOrWhiteSpace(Directory))
        {
            Log.LogError("Directory is empty.");
            return false;
        }

        var rootDirectoryPath = Utils.AppendTrailingSlash(Path.GetFullPath(Directory!));
        var ignoredFiles = new List<string>();

        foreach (var ignoreListFile in IgnoreListFiles)
        {
            var ignoreListFilePath = ignoreListFile.GetMetadata("FullPath");
            if (!File.Exists(ignoreListFilePath))
            {
                Log.LogWarning("Ignore list file \"{0}\" does not exist. Skipping.", ignoreListFilePath);
                continue;
            }

            var ignoreList = LoadIgnoreListFile(ignoreListFilePath);
            var applicableDirectoryPath = GetApplicableDirectoryPath(ignoreListFilePath, rootDirectoryPath);
            foreach (var filePath in DirectoryIO.EnumerateFiles(applicableDirectoryPath, "*", SearchOption.AllDirectories))
            {
                var relativeFilePath = filePath.Substring(applicableDirectoryPath.Length).Replace(Path.DirectorySeparatorChar, '/');
                if (ignoreList.IsIgnored(relativeFilePath))
                    ignoredFiles.Add(filePath);
            }
        }

        IgnoredFiles = ignoredFiles
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(fileName => new TaskItem(fileName))
            .ToArray<ITaskItem>();

        return true;
    }

    private static Ignore.Ignore LoadIgnoreListFile(string path)
    {
        var lines = File.ReadLines(path);
        var ignore = new Ignore.Ignore();
        ignore.Add(lines);
        return ignore;
    }

    private static string GetApplicableDirectoryPath(string ignoreListFilePath, string rootDirectoryPath)
    {
        var ignoreListDirectoryPath = Path.GetDirectoryName(ignoreListFilePath)!;
        if (ignoreListDirectoryPath.StartsWith(rootDirectoryPath, StringComparison.OrdinalIgnoreCase))
            return ignoreListDirectoryPath;
        return rootDirectoryPath;
    }
}