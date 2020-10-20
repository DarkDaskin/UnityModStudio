using System.IO;
using System.Reflection;
using UnityModStudio.Common.ModLoader;

namespace UnityModStudio.ModLoader.UnityDoorstop
{
    public class UnityDoorstopManager : IModLoaderManager
    {
        public string Id => "UnityDoorstop";
        public string Name => "Unity Doorstop";
        public int Priority => 0;
        public string? PackageName => typeof(UnityDoorstopManager).Namespace;
        public string? PackageVersion => 
            typeof(UnityDoorstopManager).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

        public bool IsInstalled(string gamePath) => File.Exists(Path.Combine(gamePath, "doorstop_config.ini"));
    }
}
