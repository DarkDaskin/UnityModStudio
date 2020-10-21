using System.ComponentModel.Composition;

namespace UnityModStudio.Common.ModLoader
{
    [InheritedExport]
    public interface IModLoaderManager
    {
        string Id { get; }
        string Name { get; }
        int Priority { get; }
        string? PackageName { get; }
        string? PackageVersion { get; }

        bool IsInstalled(string gamePath);
        string? GetExampleTemplatePath(string language);
    }
}