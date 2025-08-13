using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using UnityModStudio.Common;

namespace UnityModStudio.Build.Tasks;

public class ResolveGameAssemblyReferences : Task
{
    [Required]
    public string? GamePath { get; set; }

    [Required]
    public string? TargetFramework { get; set; }

    public ITaskItem[] ExistingReferences { get; set; } = [];

    [Output]
    public string? Architecture { get; private set; }

    [Output]
    public ITaskItem[] ReferencesToAdd { get; private set; } = [];

    [Output]
    public ITaskItem[] ReferencesToUpdate { get; private set; } = [];

    [Output]
    public ITaskItem[] ReferencesToRemove { get; private set; } = [];

    public override bool Execute()
    {
        if (!GameInformationResolver.TryGetGameInformation(GamePath, out var gameInformation, out var error, out var errorCode))
        {
            Log.LogErrorWithCode(errorCode, error);
            return false;
        }

        Architecture = gameInformation.Architecture.ToString();

        IEnumerable<FileInfo> assemblyFiles = gameInformation.GameAssemblyFiles;
        assemblyFiles = assemblyFiles.Concat(gameInformation.FrameworkAssemblyFiles);
        var resolvedReferences = assemblyFiles.ToDictionary(file => Path.GetFileNameWithoutExtension(file.Name), file => file.FullName,
            StringComparer.OrdinalIgnoreCase);
        
        var referencesToUpdate = new List<ITaskItem>();
        var referencesToRemove = new List<ITaskItem>();
        foreach (var reference in ExistingReferences)
        {
            if (resolvedReferences.TryGetValue(reference.ItemSpec, out var path))
            {
                reference.SetMetadata("HintPath", path);
                reference.SetMetadata("Private", "false");
                referencesToUpdate.Add(reference);
            }
            else if (string.Equals(reference.GetMetadata("IsImplicitlyDefined"), "true", StringComparison.OrdinalIgnoreCase))
                referencesToRemove.Add(reference);
        }

        var existingReferenceNames = new HashSet<string>(ExistingReferences.Select(item => item.ItemSpec), 
            StringComparer.OrdinalIgnoreCase);
        var referencesToAdd = new List<ITaskItem>();
        foreach (var resolvedReference in resolvedReferences)
        {
            if (!existingReferenceNames.Contains(resolvedReference.Key))
            {
                var reference = new TaskItem(resolvedReference.Key);
                reference.SetMetadata("HintPath", resolvedReference.Value);
                reference.SetMetadata("Private", "false");
                reference.SetMetadata("IsImplicitlyDefined", "true");
                reference.SetMetadata("IsImplicitlyDefinedGameReference", "true");
                referencesToAdd.Add(reference);
            }
        }

        ReferencesToAdd = referencesToAdd.ToArray();
        ReferencesToUpdate = referencesToUpdate.ToArray();
        ReferencesToRemove = referencesToRemove.ToArray();

        return true;
    }
}