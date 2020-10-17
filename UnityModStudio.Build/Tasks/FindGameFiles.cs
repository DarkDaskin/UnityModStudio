using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

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
            if (!TryGetGameDirectory(out var gameDirectory))
                return false;

            if (!TryGetGameDataDirectory(gameDirectory, out var gameDataDirectory))
                return false;

            SetGameDataPath(gameDataDirectory);

            if (!TryGetGameManagedDirectory(gameDataDirectory, out var gameManagedDirectory))
                return false;

            SetGameAssemblies(gameManagedDirectory);
            
            return true;
        }

        private bool TryGetGameDirectory(out DirectoryInfo gameDirectory)
        {
            gameDirectory = new DirectoryInfo(GamePath!.GetMetadata("FullPath")!);
            if (gameDirectory.Exists)
                return true;

            Log.LogError("Game directory does not exist.");
            return false;
        }

        private bool TryGetGameDataDirectory(DirectoryInfo gameDirectory, [NotNullWhen(true)] out DirectoryInfo? gameDataDirectory)
        {
            var query =
                from exeFile in gameDirectory.EnumerateFiles("*.exe")
                let dataDirectoryName = Path.GetFileNameWithoutExtension(exeFile.Name) + "_Data"
                from dataDirectory in gameDirectory.EnumerateDirectories(dataDirectoryName)
                select dataDirectory;
            var candidates = query.ToList();

            if (candidates.Count == 1)
            {
                gameDataDirectory = candidates[0];
                return true;
            }

            if (candidates.Count == 0)
            {
                Log.LogError("Unable to determine game data directory.");
            }
            else
            {
                foreach (var directory in candidates) 
                    Log.LogMessage($"Potential game data directory: '{directory.FullName}'.");

                Log.LogError("Ambiguous game data directory.");
            }

            gameDataDirectory = null;
            return false;
        }

        private bool TryGetGameManagedDirectory(DirectoryInfo gameDataDirectory, [NotNullWhen(true)] out DirectoryInfo? gameManagedDirectory)
        {
            gameManagedDirectory = gameDataDirectory.EnumerateDirectories("Managed").FirstOrDefault();
            if (gameManagedDirectory != null)
                return true;

            Log.LogError("Game managed assembly directory does not exist.");
            return false;
        }

        private void SetGameDataPath(DirectoryInfo gameDataDirectory)
        {
            GameDataPath = new TaskItem(gameDataDirectory.FullName);
        }

        private void SetGameAssemblies(DirectoryInfo gameManagedDirectory)
        {
            var assemblyFiles = gameManagedDirectory.GetFiles("*.dll");
            GameAssemblies = new ITaskItem[assemblyFiles.Length];
            for (var i = 0; i < assemblyFiles.Length; i++)
                GameAssemblies[i] = new TaskItem(assemblyFiles[i].FullName);
        }
    }
}