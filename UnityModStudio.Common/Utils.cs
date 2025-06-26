using System.IO;

namespace UnityModStudio.Common
{
    public static class Utils
    {
        private static readonly string DirectorySeparator = Path.DirectorySeparatorChar.ToString();

        /// <summary>
        /// Get NuGet package version of Unity Mod Studio.
        /// </summary>
        /// <returns>Version string.</returns>
        /// <remarks>This assumes all projects have the same version, which is ensured by version.json.</remarks>
        public static string GetPackageVersion() =>
            ThisAssembly.IsPublicRelease
                ? ThisAssembly.AssemblyInformationalVersion.Split('+')[0]
                : ThisAssembly.AssemblyInformationalVersion.Replace("+", "-g");

        /// <summary>
        /// Appends trailing slash to a path if missing.
        /// </summary>
        /// <param name="path">Input path.</param>
        /// <returns>Path ending with trailing slash.</returns>
        public static string AppendTrailingSlash(string path) =>
            path.EndsWith(DirectorySeparator) ? path : path + DirectorySeparator;
    }
}