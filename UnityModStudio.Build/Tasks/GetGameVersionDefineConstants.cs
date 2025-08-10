using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using UnityModStudio.Common;

namespace UnityModStudio.Build.Tasks;

public class GetGameVersionDefineConstants : Task
{
    [Required]
    public string[] Versions { get; set; } = [];

    [Required]
    public string CurrentVersion { get; set; } = "";

    [Output]
    public string[] DefineConstants { get; private set; } = [];

    public override bool Execute()
    {
        if (!Versions.Contains(CurrentVersion))
        {
            Log.LogError("Versions does not contain current version.");
            return false;
        }

        var comparer = new GameVersionComparer();
        Array.Sort(Versions, comparer);

        List<string> constants = [$"GAME_{Utils.SanitizeGameVersion(CurrentVersion)}"];
        foreach (var version in Versions)
        {
            constants.Add($"GAME_{Utils.SanitizeGameVersion(version)}_OR_GREATER");
            if (comparer.Compare(version, CurrentVersion) == 0)
                break;
        }
        DefineConstants = constants.ToArray();

        return true;
    }
}