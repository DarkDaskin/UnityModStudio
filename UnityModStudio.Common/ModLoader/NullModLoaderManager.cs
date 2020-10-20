using System.ComponentModel.Composition;

namespace UnityModStudio.Common.ModLoader
{
    public class NullModLoaderManager : IModLoaderManager
    {
        //[Export(typeof(IModLoaderManager))]
        public static readonly NullModLoaderManager Instance = new NullModLoaderManager();

        public string Id => "None";
        public string Name => "None";
        public int Priority => -1;
        public string? PackageName => null;
        public string? PackageVersion => null;

        public bool IsInstalled(string gamePath) => true;

        // Treat every instance as the same object.
        public override bool Equals(object obj) => obj is NullModLoaderManager;

        public override int GetHashCode() => 0;

        //private NullModLoaderManager() { }
    }
}