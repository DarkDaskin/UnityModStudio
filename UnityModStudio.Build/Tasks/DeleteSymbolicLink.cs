using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace UnityModStudio.Build.Tasks;

public class DeleteSymbolicLink : Task
{
    [Required]
    public string? Path { get; set; }

    public override bool Execute()
    {
        if (string.IsNullOrWhiteSpace(Path))
        {
            Log.LogError("Path is empty.");
            return false;
        }

        const string deletingMessageFormat = "Deleting \"{0}\".";
        try
        {
            if (File.Exists(Path))
            {
                Log.LogMessage(MessageImportance.Normal, deletingMessageFormat, Path);
                File.Delete(Path!);
            }
            else if (Directory.Exists(Path))
            {
                Log.LogMessage(MessageImportance.Normal, deletingMessageFormat, Path);
                Directory.Delete(Path!);
            }
            else
                Log.LogMessage(MessageImportance.Low, "File or directory \"{0}\" does not exist. Skipping.", Path);
        }
        catch (Exception exception)
        {
            Log.LogErrorFromException(exception);
            return false;
        }
        
        return true;
    }
}