using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace UnityModStudio.Common
{
    public static class GameInformationResolver
    {
        public static bool TryGetGameInformation(
            [NotNullWhen(true)] string? gamePath,
            [NotNullWhen(true)] out GameInformation? gameInformation,
            [NotNullWhen(false)] out string? error,
            out string? errorCode)
        {
            gameInformation = null;

            try
            {
                if (!Directory.Exists(gamePath))
                {
                    error = "Game directory does not exist.";
                    errorCode = "UMS1001";
                    return false;
                }

                var gameDirectory = new DirectoryInfo(gamePath);
                if (!TryGetGameDataDirectory(gameDirectory, out var gameExecutableFile, out var gameDataDirectory, out error, out errorCode))
                    return false;

                if (!TryGetGameManagedDirectory(gameDataDirectory, out var gameManagedDirectory, out error, out errorCode))
                    return false;
            
                var assemblyFiles = GetAssemblies(gameManagedDirectory);

                var (moniker, isSubset) = GetTargetFrameworkInfo(assemblyFiles);
            
                var (gameName, company) = GetAppInfo(gameDataDirectory);

                var (frameworkAssemblyFiles, gameAssemblyFiles) = GroupAssemblies(assemblyFiles);

                gameInformation = new GameInformation
                {
                    Name = gameName,
                    Company = company,
                    Architecture = GetExecutableArchitecture(gameExecutableFile),
                    UnityVersion = GetUnityVersion(gameExecutableFile),
                    TargetFrameworkMoniker = moniker,
                    IsSubsetProfile = isSubset,
                    GameDirectory = gameDirectory,
                    GameExecutableFile = gameExecutableFile,
                    GameDataDirectory = gameDataDirectory,
                    FrameworkAssemblyFiles = frameworkAssemblyFiles,
                    GameAssemblyFiles = gameAssemblyFiles,
                };
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                errorCode = exception.GetErrorCode();
                return false;
            }
        }

        private static bool TryGetGameDataDirectory(DirectoryInfo gameDirectory,
            [NotNullWhen(true)] out FileInfo? gameExecutableFile,
            [NotNullWhen(true)] out DirectoryInfo? gameDataDirectory,
            [NotNullWhen(false)] out string? error, 
            [NotNullWhen(false)] out string? errorCode)
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
                errorCode = null;
                return true;
            }

            error = candidates.Count == 0 ? "Unable to determine game data directory." : "Ambiguous game data directory.";
            errorCode = candidates.Count == 0 ? "UMS1002" : "UMS1003";
            gameExecutableFile = null;
            gameDataDirectory = null;
            return false;
        }

        private static bool TryGetGameManagedDirectory(
            DirectoryInfo gameDataDirectory,
            [NotNullWhen(true)] out DirectoryInfo? gameManagedDirectory,
            [NotNullWhen(false)] out string? error, 
            [NotNullWhen(false)] out string? errorCode)
        {
            gameManagedDirectory = gameDataDirectory.EnumerateDirectories("Managed").SingleOrDefault();
            if (gameManagedDirectory != null)
            {
                error = null;
                errorCode = null;
                return true;
            }

            error = "Game managed assembly directory does not exist.";
            errorCode = "UMS1004";
            return false;
        }

        private static FileInfo[] GetAssemblies(DirectoryInfo gameManagedDirectory) => gameManagedDirectory.GetFiles("*.dll");

        private static (string moniker, bool isSubset) GetTargetFrameworkInfo(IReadOnlyCollection<FileInfo> assemblyFiles)
        {
            var mscorlibFile = GetAssemblyFile(assemblyFiles, "mscorlib.dll");
            if (mscorlibFile == null)
                throw new NotSupportedException("'mscorlib.dll' is missing.").WithErrorCode("UMS1005");
            var mscorlibVersion = Version.Parse(FileVersionInfo.GetVersionInfo(mscorlibFile.FullName).FileVersion);
            var systemCoreFile = GetAssemblyFile(assemblyFiles, "System.Core.dll");
            var systemXmlFile = GetAssemblyFile(assemblyFiles, "System.Xml.dll");
            var unityEngineCoreFile = GetAssemblyFile(assemblyFiles, "UnityEngine.CoreModule.dll");
            var netstandardFile = GetAssemblyFile(assemblyFiles, "netstandard.dll");
            var hasSystemCore = systemCoreFile != null;
            var hasSystemXml = systemXmlFile != null;
            var hasUnityEngineCore = unityEngineCoreFile != null;
            var netStandardVersion = netstandardFile is null
                ? null
                : Version.Parse(FileVersionInfo.GetVersionInfo(netstandardFile.FullName).FileVersion);

            return mscorlibVersion.Major switch
            {
                // Unity 2021.x+ .NET Standard 2.1 profile (actually it's also .NET 4.8)
                4 when netStandardVersion is { Major: 2, Minor: 1 } => ("netstandard2.1", false),
                // Unity 2018.x+ .NET Standard 2.0 profile
                4 when netStandardVersion is { Major: 2, Minor: 0 } => ("netstandard2.0", false),
                // Unity 2018.x+ .NET 4.x profile
                4 when hasUnityEngineCore => ("net472", false),
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
                _ => throw new NotSupportedException("Specified assembly set does not correspond to any known target framework.").WithErrorCode("UMS1006")
            };
        }

        private static FileInfo? GetAssemblyFile(IEnumerable<FileInfo> assemblyFiles, string name) =>
            assemblyFiles.SingleOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        private static (IReadOnlyCollection<FileInfo> frameworkAssemblyFiles, IReadOnlyCollection<FileInfo> gameAssemblyFiles)
            GroupAssemblies(IReadOnlyCollection<FileInfo> assemblyFiles)
        {
            var netstandardAssemblyFile = GetAssemblyFile(assemblyFiles, "netstandard.dll");
            var frameworkAssemblyNames = netstandardAssemblyFile != null ? NetStandardFrameworkAssemblyNames : FullFrameworkAssemblyNames;
            var frameworkAssemblyFiles = assemblyFiles.Where(f => frameworkAssemblyNames.Contains(f.Name, StringComparer.OrdinalIgnoreCase)).ToList();
            var gameAssemblyFiles = assemblyFiles.Except(frameworkAssemblyFiles).ToList();
            return (frameworkAssemblyFiles, gameAssemblyFiles);
        }

        // See https://stackoverflow.com/a/1002672/280778 and https://docs.microsoft.com/en-us/windows/win32/debug/pe-format
        private static Architecture GetExecutableArchitecture(FileInfo executableFile)
        {
            const int peHeaderOffsetOffset = 0x3c;
            const uint peSignature = 0x00004550; // 'PE\0\0'
            // ReSharper disable InconsistentNaming
            const ushort IMAGE_FILE_MACHINE_I386 = 0x14c;
            const ushort IMAGE_FILE_MACHINE_AMD64 = 0x8664;
            const ushort IMAGE_FILE_MACHINE_ARM = 0x1c0;
            const ushort IMAGE_FILE_MACHINE_ARMNT = 0x1c4;
            const ushort IMAGE_FILE_MACHINE_ARM64= 0xaa64;
            // ReSharper restore InconsistentNaming

            using var reader = new BinaryReader(File.OpenRead(executableFile.FullName));
            reader.BaseStream.Position = peHeaderOffsetOffset;
            var peHeaderOffset = reader.ReadInt32();
            reader.BaseStream.Position = peHeaderOffset;
            var actualPeSignature = reader.ReadUInt32();
            if (actualPeSignature != peSignature)
                throw new BadImageFormatException("Invalid PE header.").WithErrorCode("UMS1007");

            var machineType = reader.ReadUInt16();
            return machineType switch
            {
                IMAGE_FILE_MACHINE_I386 => Architecture.X86,
                IMAGE_FILE_MACHINE_AMD64 => Architecture.X64,
                IMAGE_FILE_MACHINE_ARM => Architecture.Arm,
                IMAGE_FILE_MACHINE_ARMNT => Architecture.Arm,
                IMAGE_FILE_MACHINE_ARM64 => Architecture.Arm64,
                _ => throw new BadImageFormatException("Unsupported PE architecture.").WithErrorCode("UMS1008")
            };
        }

        private static string GetUnityVersion(FileInfo gameExecutableFile)
        {
            var unityVersion = VersionInfo.GetStringFileInfo(gameExecutableFile.FullName, "Unity Version");
            if (unityVersion != null)
                return unityVersion.Split('_').First();

            return FileVersionInfo.GetVersionInfo(gameExecutableFile.FullName).ProductVersion.Split(' ').First();
        }

        private static (string? gameName, string? company) GetAppInfo(DirectoryInfo gameDataDirectory)
        {
            var appInfoFile = gameDataDirectory.EnumerateFiles("app.info").SingleOrDefault();
            if (appInfoFile == null) 
                return default;

            var appInfo = File.ReadAllLines(appInfoFile.FullName);
            return (appInfo.ElementAtOrDefault(1), appInfo.ElementAtOrDefault(0));
        }

        private static readonly string[] FullFrameworkAssemblyNames =
        {
            "mscorlib.dll",
            "System.dll",
            "System.Configuration.dll",
            "System.Core.dll",
            "System.Security.dll",
            "System.Xml.dll",
        };

        private static readonly string[] NetStandardFrameworkAssemblyNames =
        {
            "netstandard.dll",
            "mscorlib.dll",
            "System.dll",
            "System.ComponentModel.Composition.dll",
            "System.Configuration.dll",
            "System.Core.dll",
            "System.Data.DataSetExtensions.dll",
            "System.Data.dll",
            "System.Diagnostics.StackTrace.dll",
            "System.Drawing.dll",
            "System.EnterpriseServices.dll",
            "System.Globalization.Extensions.dll",
            "System.IO.Compression.dll",
            "System.IO.Compression.FileSystem.dll",
            "System.Net.Http.dll",
            "System.Numerics.dll",
            "System.Runtime.dll",
            "System.Runtime.Serialization.dll",
            "System.Runtime.Serialization.Xml.dll",
            "System.Security.dll",
            "System.ServiceModel.Internals.dll",
            "System.Transactions.dll",
            "System.Xml.dll",
            "System.Xml.Linq.dll",
            "System.Xml.XPath.XDocument.dll",
        };
    }
}