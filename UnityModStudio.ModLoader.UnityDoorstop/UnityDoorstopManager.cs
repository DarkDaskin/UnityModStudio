using System.IO;
using UnityModStudio.Common.ModLoader;

namespace UnityModStudio.ModLoader.UnityDoorstop
{
    public class UnityDoorstopManager : ModLoaderManagerBase
    {
        public override string Id => "UnityDoorstop";
        public override string Name => "Unity Doorstop";
        public override string? PackageName => GetConventionalPackageName();
        public override string? PackageVersion => GetConventionalPackageVersion();

        public override bool IsInstalled(string gamePath) => File.Exists(Path.Combine(gamePath, "doorstop_config.ini"));

        public override string? GetExampleTemplatePath(string language) =>
            GetConventionalExampleTemplatePath(language, "UnityModStudio.UnityDoorstopExample", "UnityModStudio");
    }
}
