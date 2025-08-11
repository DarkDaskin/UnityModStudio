using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace UnityModStudio.Build.Tasks;

public class ConfigureDoorstop : Task
{
    [Required]
    public ITaskItem? ConfigPath { get; set; }

    public ITaskItem? TargetAssemblyPath { get; set; }

    public bool UseForModLoading { get; set; }

    public override bool Execute()
    {
        var configPath = ConfigPath!.GetMetadata("FullPath");
        if (!File.Exists(configPath))
        {
            Log.LogErrorWithCode("UMS0012", "Unity Doorstop config file does not exist.");
            return false;
        }

        var values = new List<(string, string)>
        {
            ("enabled", "true")
        };

        if (UseForModLoading)
        {
            var targetAssemblyPath = TargetAssemblyPath?.GetMetadata("FullPath");
            if (!File.Exists(targetAssemblyPath))
            {
                Log.LogErrorWithCode("UMS0013", "Target assembly file does not exist.");
                return false;
            }
            values.Add(("target_assembly", GetRelativePath(targetAssemblyPath!, configPath)));
        }

        SetIniValues(configPath, "General", values);

        return true;
    }

    private static string GetRelativePath(string path, string relativeToPath) =>
        new Uri(Path.GetFullPath(relativeToPath))
            .MakeRelativeUri(new Uri(Path.GetFullPath(path))).ToString()
            .Replace('/', Path.DirectorySeparatorChar);

    private static readonly char[] IniEntrySeparator = ['='];
    private static readonly char[] IniCommentSeparator = ['#'];

    private static void SetIniValues(string iniFilePath, string section, IEnumerable<(string key, string value)> values)
    {
        var lines = File.ReadAllLines(iniFilePath);
        var linesInSection = lines
            .Select((line, index) => (text: line.TrimStart(), index))
            .SkipWhile(line => !line.text.StartsWith($"[{section}]"))
            .Skip(1)
            .TakeWhile(line => !line.text.StartsWith("["));
        var linesToChange =
            from line in linesInSection
            where !line.text.StartsWith("#")
            let parts = line.text.Split(IniEntrySeparator, 2)
            where parts.Length > 1
            let comment = parts[1].Split(IniCommentSeparator, 2).ElementAtOrDefault(1)
            join kv in values on parts[0] equals kv.key 
            select (line.index, kv.key, kv.value, comment);
        foreach (var (index, key, value, comment) in linesToChange)
            lines[index] = $"{key}={value}" + (comment != null ? $" # {comment}" : "");
        File.WriteAllLines(iniFilePath, lines);
    }
}