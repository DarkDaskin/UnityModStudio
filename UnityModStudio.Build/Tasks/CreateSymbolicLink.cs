using System;
using System.IO;
using FileSystemLinks;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace UnityModStudio.Build.Tasks;

public class CreateSymbolicLink : Task
{
    [Required]
    public string? SymbolicLink { get; set; }

    [Required]
    public string? Target { get; set; }

    public override bool Execute()
    {
        if (string.IsNullOrWhiteSpace(SymbolicLink))
        {
            Log.LogError("SymbolicLink is empty.");
            return false;
        }
        if (string.IsNullOrWhiteSpace(Target))
        {
            Log.LogError("Target is empty.");
            return false;
        }

        try
        {
            if (File.Exists(Target))
            {
                Log.LogMessage(MessageImportance.Normal, "Creating symbolic link to file \"{0}\".", Target!);
                FileSystemLink.CreateFileSymbolicLink(SymbolicLink!, Target!);
            }
            else if (Directory.Exists(Target))
            {
                Log.LogMessage(MessageImportance.Normal, "Creating symbolic link to directory \"{0}\".", Target!);
                FileSystemLink.CreateDirectorySymbolicLink(SymbolicLink!, Target!);
            }
            else
            {
                Log.LogError("Target does not exist.");
                return false;
            }
        }
        catch (Exception exception)
        {
            Log.LogErrorFromException(exception);
            return false;
        }

        return true;
    }
}