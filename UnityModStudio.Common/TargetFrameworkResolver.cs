using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UnityModStudio.Common
{
    public static class TargetFrameworkResolver
    {
        public static string GetTargetFrameworkMoniker(IReadOnlyCollection<FileInfo> assemblyFiles) =>
            GetTargetFrameworkInfo(assemblyFiles).moniker;

        public static (string moniker, bool isSubset) GetTargetFrameworkInfo(IReadOnlyCollection<FileInfo> assemblyFiles)
        {
            var mscorlibFile = GetAssemblyFile(assemblyFiles, "mscorlib.dll");
            if (mscorlibFile == null)
                throw new NotSupportedException("'mscorlib.dll' is missing.");
            var mscorlibVersion = Version.Parse(FileVersionInfo.GetVersionInfo(mscorlibFile.FullName).FileVersion);
            var systemCoreFile = GetAssemblyFile(assemblyFiles, "System.Core.dll");
            var systemXmlFile = GetAssemblyFile(assemblyFiles, "System.Xml.dll");
            var hasSystemCore = systemCoreFile != null;
            var hasSystemXml = systemXmlFile != null;

            return mscorlibVersion.Major switch
            {
                // Unity 2017.x+ .NET 4.x runtime
                4 => (mscorlibVersion.Minor switch
                {
                    // Unity 2017.x+ .NET 4.6 profile
                    6 => "net46",
                    // Unity 2018.x+ .NET Standard 2.0 profile
                    0 => "netstandard2.0",
                    _ => throw UnknownTargetFrameworkException()
                }, false),
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
    }
}