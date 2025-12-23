using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace UnityModStudio.Common
{
    public class GameInformation
    {
        public string? Name { get; set; }
        public string? Company { get; set; }

        public Architecture Architecture { get; set; }
        public string? UnityVersion { get; set; }
        public string? TargetFrameworkMoniker { get; set; }
        public bool IsSubsetProfile { get; set; }

        public DirectoryInfo GameDirectory { get; set; } = null!;
        public FileInfo GameExecutableFile { get; set; } = null!;
        public DirectoryInfo GameDataDirectory { get; set; } = null!;
        public IReadOnlyCollection<FileInfo> FrameworkAssemblyFiles { get; set; } = null!;
        public IReadOnlyCollection<FileInfo> GameAssemblyFiles { get; set; } = null!;
        
        public string GetMonoProfileString()
        {
            if (TargetFrameworkMoniker is not null)
            {
                var match = Regex.Match(TargetFrameworkMoniker, @"(?<NetStandard>netstandard(?<Version>\d+\.\d+))|(?<NetFull>net(?<Version>\d+))");

                if (match.Groups["NetStandard"].Success)
                    return ".NET Standard " + match.Groups["Version"].Value;

                if (match.Groups["NetFull"].Success)
                    return ".NET " + string.Join(".", match.Groups["Version"].Value.ToCharArray()) + (IsSubsetProfile ? " Subset" : "");
            }

            return "<unknown>";
        }
    }
}