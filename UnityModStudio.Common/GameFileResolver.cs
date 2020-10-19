using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace UnityModStudio.Common
{
    public static class GameFileResolver
    {
        // Supports Unity 3.x and newer. Older versions have different directory layout.
        public static bool TryResolveGameFiles(
            string? gamePath, 
            [NotNullWhen(true)] out DirectoryInfo? gameDataDirectory,
            out FileInfo[] gameAssemblyFiles,
            [NotNullWhen(false)] out string? error)
        {
            gameAssemblyFiles = Array.Empty<FileInfo>();

            if (!Directory.Exists(gamePath))
            {
                gameDataDirectory = null;
                error = "Game directory does not exist.";
                return false;
            }

            if (!TryGetGameDataDirectory(new DirectoryInfo(gamePath!), out gameDataDirectory, out error))
                return false;

            if (!TryGetGameManagedDirectory(gameDataDirectory, out var gameManagedDirectory, out error))
                return false;
            
            gameAssemblyFiles = FindGameAssemblies(gameManagedDirectory);

            error = null;
            return true;
        }

        private static bool TryGetGameDataDirectory(
            DirectoryInfo gameDirectory, 
            [NotNullWhen(true)] out DirectoryInfo? gameDataDirectory,
            [NotNullWhen(false)] out string? error)
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
                error = null;
                return true;
            }

            error = candidates.Count == 0 ? "Unable to determine game data directory." : "Ambiguous game data directory.";
            gameDataDirectory = null;
            return false;
        }

        private static bool TryGetGameManagedDirectory(
            DirectoryInfo gameDataDirectory, 
            [NotNullWhen(true)] out DirectoryInfo? gameManagedDirectory,
            [NotNullWhen(false)] out string? error)
        {
            gameManagedDirectory = gameDataDirectory.EnumerateDirectories("Managed").SingleOrDefault();
            if (gameManagedDirectory != null)
            {
                error = null;
                return true;
            }

            error = "Game managed assembly directory does not exist.";
            return false;
        }
        
        private static FileInfo[] FindGameAssemblies(DirectoryInfo gameManagedDirectory) => gameManagedDirectory.GetFiles("*.dll");
    }
}
