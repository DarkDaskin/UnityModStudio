using System.Reflection;

namespace UnityModStudio.Common
{
    public static class Utils
    {
        /// <summary>
        /// Get NuGet package version of a Unity Mod Studio assembly.
        /// </summary>
        /// <param name="assembly">An assembly to inspect.</param>
        /// <returns>Version string or <c>null</c> if none is defined.</returns>
        public static string? GetPackageVersion(Assembly assembly) =>
            assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0];
    }
}