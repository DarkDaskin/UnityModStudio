using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using UnityModStudio.Common;

namespace UnityModStudio.Build.Tasks;

public class SanitizeGameVersion : Task
{
    [Required]
    public string? Version { get; set; }

    [Output]
    public string? SanitizedVersion { get; private set; }

    public override bool Execute()
    {
        SanitizedVersion = Utils.SanitizeGameVersion(Version);
        
        return true;
    }
}