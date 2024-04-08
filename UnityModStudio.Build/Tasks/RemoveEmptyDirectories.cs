using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace UnityModStudio.Build.Tasks;

public class RemoveEmptyDirectories : Task
{
    [Required]
    public ITaskItem[] Directories { get; set; } = [];

    public override bool Execute()
    {
        foreach (var directory in Directories)
            RemoveEmptyDirectoriesImpl(directory.ItemSpec);

        return true;
    }

    private void RemoveEmptyDirectoriesImpl(string path)
    {
        foreach (var subDirectoryPath in Directory.EnumerateDirectories(path))
            RemoveEmptyDirectoriesImpl(subDirectoryPath);

        if (!Directory.EnumerateFileSystemEntries(path).Any())
        {
            Log.LogMessage("Deleting empty directory \"{0}\".", path);
            Directory.Delete(path);
        }
    }
}