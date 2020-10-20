using System.Collections.Generic;
using System.IO;

namespace UnityModStudio.Common
{
    public class GameInformation
    {
        public string? Name { get; set; }
        public string? Company { get; set; }

        public string UnityVersion { get; set; } = null!;
        public string TargetFrameworkMoniker { get; set; } = null!;
        public bool IsSubsetProfile { get; set; }

        public FileInfo GameExecutableFile { get; set; } = null!;
        public DirectoryInfo GameDataDirectory { get; set; } = null!;
        public IReadOnlyCollection<FileInfo> FrameworkAssemblyFiles { get; set; } = null!;
        public IReadOnlyCollection<FileInfo> GameAssemblyFiles { get; set; } = null!;

    }
}