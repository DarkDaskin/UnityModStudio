namespace UnityModStudio.RimWorld.Common.Options;

public class RimWorldSettings
{
    public ProjectDefaultsSettings ProjectDefaults { get; set; } = new();

    public class ProjectDefaultsSettings
    {
        public bool UseHarmony { get; set; } = true;
        public string? ModAuthor { get; set; }
        public string? ModPackageIdPrefix { get; set; }
        public ProjectLayout ProjectLayout { get; set; } = ProjectLayout.AssetsAtTopLevel;        
    }
}