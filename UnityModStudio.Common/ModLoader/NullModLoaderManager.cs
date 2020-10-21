namespace UnityModStudio.Common.ModLoader
{
    public class NullModLoaderManager : ModLoaderManagerBase
    {
        public static readonly NullModLoaderManager Instance = new NullModLoaderManager();

        public override string Id => "None";
        public override string Name => "None";
        public override int Priority => -1;

        public override bool IsInstalled(string gamePath) => true;

        // Treat every instance as the same object.
        public override bool Equals(object obj) => obj is NullModLoaderManager;

        public override int GetHashCode() => GetType().GetHashCode();
    }
}