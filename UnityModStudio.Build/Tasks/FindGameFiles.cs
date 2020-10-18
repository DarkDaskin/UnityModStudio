using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using UnityModStudio.Common;

namespace UnityModStudio.Build.Tasks
{
    public class FindGameFiles : Task
    {
        [Required]
        public ITaskItem? GamePath { get; set; }

        [Output]
        public ITaskItem? GameDataPath { get; private set; }

        [Output]
        public ITaskItem[]? GameAssemblies { get; private set; }

        public override bool Execute()
        {
            var gamePath = GamePath?.GetMetadata("FullPath");
            if (!GameFileResolver.TryResolveGameFiles(gamePath, out var gameDataDirectory, out var gameAssemblyFiles, out var error))
            {
                Log.LogError(error);
                return false;
            }

            SetGameDataPath(gameDataDirectory);
            SetGameAssemblies(gameAssemblyFiles);
            
            return true;
        }

        private void SetGameDataPath(DirectoryInfo gameDataDirectory) => GameDataPath = new TaskItem(gameDataDirectory.FullName);

        private void SetGameAssemblies(FileInfo[] assemblyFiles)
        {
            GameAssemblies = new ITaskItem[assemblyFiles.Length];
            for (var i = 0; i < assemblyFiles.Length; i++)
                GameAssemblies[i] = new TaskItem(assemblyFiles[i].FullName);
        }
    }
}