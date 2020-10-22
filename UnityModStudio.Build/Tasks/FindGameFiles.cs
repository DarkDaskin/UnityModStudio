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
        public ITaskItem[]? FrameworkAssemblies { get; private set; }

        [Output]
        public ITaskItem[]? GameAssemblies { get; private set; }

        [Output]
        public ITaskItem? Architecture { get; private set; }

        public override bool Execute()
        {
            var gamePath = GamePath?.GetMetadata("FullPath");
            if (!GameInformationResolver.TryGetGameInformation(gamePath, out var gameInformation, out var error))
            {
                Log.LogError(error);
                return false;
            }

            GameDataPath = GetTaskItem(gameInformation.GameDataDirectory);
            FrameworkAssemblies = GetTaskItems(gameInformation.FrameworkAssemblyFiles);
            GameAssemblies = GetTaskItems(gameInformation.GameAssemblyFiles);
            Architecture = new TaskItem(gameInformation.Architecture.ToString());
            
            return true;
        }

        private static ITaskItem GetTaskItem(FileSystemInfo file) => new TaskItem(file.FullName);

        private static ITaskItem[] GetTaskItems(IReadOnlyCollection<FileSystemInfo> files)
        {
            var items = new ITaskItem[files.Count];
            var index = 0;
            foreach (var file in files)
                items[index++] = new TaskItem(file.FullName);
            return items;
        }
    }
}