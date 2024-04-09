using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using UnityModStudio.Common;

namespace UnityModStudio.Build.Tasks;

public class ResolveTargetPath : Task
{
    private const string TargetPathMetadataName = "TargetPath";

    [Required] 
    public string RelativeToPath { get; set; } = "";

    [Required]
    public ITaskItem[] Input { get; set; } = [];

    [Output]
    public ITaskItem[]? Output { get; set; }

    public override bool Execute()
    {
        var relativeToFullPath = Utils.AppendTrailingSlash(Path.GetFullPath(RelativeToPath));

        Output = new ITaskItem[Input.Length];
        for (var i = 0; i < Input.Length; i++)
        {
            var outputItem = new TaskItem(Input[i].ItemSpec);
            Input[i].CopyMetadataTo(outputItem);

            string? targetPath = null;
            if (!outputItem.MetadataNames.Cast<string>().Contains(TargetPathMetadataName))
            {
                var fullPath = outputItem.GetMetadata("FullPath");
                var directoryName = Path.GetDirectoryName(fullPath);
                directoryName = directoryName is null ? "" : Utils.AppendTrailingSlash(directoryName);
                if (directoryName.StartsWith(relativeToFullPath, StringComparison.OrdinalIgnoreCase)) 
                    targetPath = directoryName.Substring(relativeToFullPath.Length);
            }
            else
                targetPath = outputItem.GetMetadata(TargetPathMetadataName);

            if (!string.IsNullOrEmpty(targetPath))
                targetPath = Utils.AppendTrailingSlash(targetPath!);

            if (targetPath == "")
                targetPath = Utils.AppendTrailingSlash(".");
            
            if (targetPath is not null)
                outputItem.SetMetadata(TargetPathMetadataName, targetPath);

            Output[i] = outputItem;
        }

        return true;
    }
}