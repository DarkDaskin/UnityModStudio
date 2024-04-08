using System.IO;
using System;
using FileSystemLinks;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace UnityModStudio.Build.Tasks;

public class GetSymbolicLinkTarget : Task
{
    [Required]
    public string? SymbolicLink { get; set; }

    [Output]
    public string? Target { get; set; }

    public override bool Execute()
    {
        if (string.IsNullOrWhiteSpace(SymbolicLink))
        {
            Log.LogError("SymbolicLink is empty.");
            return false;
        }

        try
        {
            if (File.Exists(SymbolicLink))
                Target = FileSystemLink.GetFileLinkTarget(SymbolicLink!);
            else if (Directory.Exists(SymbolicLink))
                Target = FileSystemLink.GetDirectoryLinkTarget(SymbolicLink!);
        }
        catch (Exception exception)
        {
            Log.LogErrorFromException(exception);
            return false;
        }

        return true;
    }
}