using System.Collections.Generic;
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
            if (!GameInformationResolver.TryGetGameInformation(gamePath, out var gameInformation, out var error))
            {
                Log.LogError(error);
                return false;
            }

            SetGameDataPath(gameInformation.GameDataDirectory);
            SetGameAssemblies(gameInformation.GameAssemblyFiles);
            
            return true;
        }

        private void SetGameDataPath(DirectoryInfo gameDataDirectory) => GameDataPath = new TaskItem(gameDataDirectory.FullName);

        private void SetGameAssemblies(IReadOnlyCollection<FileInfo> assemblyFiles)
        {
            GameAssemblies = new ITaskItem[assemblyFiles.Count];
            var index = 0;
            foreach (var assemblyFile in assemblyFiles)
                GameAssemblies[index++] = new TaskItem(assemblyFile.FullName);
        }
    }
}