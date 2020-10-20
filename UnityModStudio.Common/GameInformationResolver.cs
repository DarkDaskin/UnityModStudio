using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace UnityModStudio.Common
{
    public static class GameInformationResolver
    {
        public static bool TryGetGameInformation(
            string? gamePath,
            [NotNullWhen(true)] out GameInformation? gameInformation,
            [NotNullWhen(false)] out string? error)
        {
            gameInformation = null;

            try
            {
                if (!Directory.Exists(gamePath))
                {
                    error = "Game directory does not exist.";
                    return false;
                }

                if (!TryGetGameDataDirectory(new DirectoryInfo(gamePath!), out var gameExecutableFile, out var gameDataDirectory, out error))
                    return false;

                if (!TryGetGameManagedDirectory(gameDataDirectory, out var gameManagedDirectory, out error))
                    return false;
            
                var gameAssemblyFiles = FindGameAssemblies(gameManagedDirectory);

                var (moniker, isSubset) = GetTargetFrameworkInfo(gameAssemblyFiles);
            
                var (gameName, company) = GetAppInfo(gameDataDirectory);

                gameInformation = new GameInformation
                {
                    Name = gameName,
                    Company = company,
                    UnityVersion = GetUnityVersion(gameExecutableFile),
                    TargetFrameworkMoniker = moniker,
                    IsSubsetProfile = isSubset,
                    GameExecutableFile = gameExecutableFile,
                    GameDataDirectory = gameDataDirectory,
                    GameAssemblyFiles = gameAssemblyFiles,
                };
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }

        private static bool TryGetGameDataDirectory(
            DirectoryInfo gameDirectory,
            [NotNullWhen(true)] out FileInfo? gameExecutableFile,
            [NotNullWhen(true)] out DirectoryInfo? gameDataDirectory,
            [NotNullWhen(false)] out string? error)
        {
            var query =
                from exeFile in gameDirectory.EnumerateFiles("*.exe")
                let dataDirectoryName = Path.GetFileNameWithoutExtension(exeFile.Name) + "_Data"
                from dataDirectory in gameDirectory.EnumerateDirectories(dataDirectoryName)
                select (exeFile, dataDirectory);
            var candidates = query.ToList();

            if (candidates.Count == 1)
            {
                gameExecutableFile = candidates[0].exeFile;
                gameDataDirectory = candidates[0].dataDirectory;
                error = null;
                return true;
            }

            error = candidates.Count == 0 ? "Unable to determine game data directory." : "Ambiguous game data directory.";
            gameExecutableFile = null;
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

        private static (string moniker, bool isSubset) GetTargetFrameworkInfo(IReadOnlyCollection<FileInfo> assemblyFiles)
        {
            var mscorlibFile = GetAssemblyFile(assemblyFiles, "mscorlib.dll");
            if (mscorlibFile == null)
                throw new NotSupportedException("'mscorlib.dll' is missing.");
            var mscorlibVersion = Version.Parse(FileVersionInfo.GetVersionInfo(mscorlibFile.FullName).FileVersion);
            var systemCoreFile = GetAssemblyFile(assemblyFiles, "System.Core.dll");
            var systemXmlFile = GetAssemblyFile(assemblyFiles, "System.Xml.dll");
            var netstandardFile = GetAssemblyFile(assemblyFiles, "netstandard.dll");
            var hasSystemCore = systemCoreFile != null;
            var hasSystemXml = systemXmlFile != null;
            var hasNetStandard = netstandardFile != null;

            return mscorlibVersion.Major switch
            {
                // Unity 2018.x+ .NET Standard 2.0 profile
                4 when hasNetStandard => ("netstandard2.0", false),
                // Unity 2017.x+ .NET 4.6 profile
                4 => ("net46", false),
                // Unity 4.x .NET 2.0 Subset profile
                3 when hasSystemCore => ("net35", true),
                // Unity 3.x .NET 2.0 Subset profile
                3 => ("net20", true),
                // Unity 4.x+ .NET 2.0 profile
                2 when hasSystemCore && hasSystemXml => ("net35", false),
                // Unity 5.x .NET 2.0 Subset profile
                2 when hasSystemCore => ("net35", true),
                // Unity 3.x .NET 2.0 profile
                2 => ("net20", false),
                // Probably some future version of Unity.
                _ => throw UnknownTargetFrameworkException()
            };
        }

        private static FileInfo? GetAssemblyFile(IEnumerable<FileInfo> assemblyFiles, string name) =>
            assemblyFiles.SingleOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        private static Exception UnknownTargetFrameworkException() =>
            throw new NotSupportedException("Specified assembly set does not correspond to any known target framework.");

        private static string GetUnityVersion(FileInfo gameExecutableFile) =>
            FileVersionInfo.GetVersionInfo(gameExecutableFile.FullName).FileVersion;

        private static (string? gameName, string? company) GetAppInfo(DirectoryInfo gameDataDirectory)
        {
            var appInfoFile = gameDataDirectory.EnumerateFiles("app.info").SingleOrDefault();
            if (appInfoFile == null) 
                return default;

            var appInfo = File.ReadAllLines(appInfoFile.FullName);
            return (appInfo.ElementAtOrDefault(1), appInfo.ElementAtOrDefault(0));
        }
    }
}