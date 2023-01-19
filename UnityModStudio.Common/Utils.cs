using System.IO;
using System.Reflection;

namespace UnityModStudio.Common
{
    public static class Utils
    {
        private static readonly string DirectorySeparator = Path.DirectorySeparatorChar.ToString();

        /// <summary>
        /// Get NuGet package version of a Unity Mod Studio assembly.
        /// </summary>
        /// <param name="assembly">An assembly to inspect.</param>
        /// <returns>Version string or <c>null</c> if none is defined.</returns>
        public static string? GetPackageVersion(Assembly assembly) =>
            assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0];

        /// <summary>
        /// Appends trailing slash to a path if missing.
        /// </summary>
        /// <param name="path">Input path.</param>
        /// <returns>Path ending with trailing slash.</returns>
        public static string AppendTrailingSlash(string path) =>
            path.EndsWith(DirectorySeparator) ? path : path + DirectorySeparator;
    }
}