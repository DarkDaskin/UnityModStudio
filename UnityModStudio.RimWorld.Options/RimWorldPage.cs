using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using UnityModStudio.Options;
using UnityModStudio.RimWorld.Common.Options;

namespace UnityModStudio.RimWorld.Options;

[ComVisible(true), Guid("6027EEE1-7DD3-4966-B5BF-31DB79ACFCBD")]
public class RimWorldPage : DialogPage
{
    public const string CategoryName = GameRegistryPage.CategoryName;
    public const string PageName = "RimWorld";
    public const string ProjectDefaultsCategory = "New project defaults";

    private IComponentModel ComponentModel => (IComponentModel)GetService(typeof(SComponentModel))!;
    private IRimWorldSettingsManager RimWorldSettingsManager => ComponentModel.GetService<IRimWorldSettingsManager>();

    [Category(ProjectDefaultsCategory)]
    [DisplayName("Use Harmony library")]
    [Description("When enabled, the Harmony library will be added to the RimWorld mod projects by default.")]
    [DefaultValue(true)]
    public bool UseHarmonyByDefault { get; set; }

    [Category(ProjectDefaultsCategory)]
    [DisplayName("Mod author")]
    [Description("The default value of the mod author field of the project creation wizard.")]
    public string? DefaultModAuthor { get; set; }

    [Category(ProjectDefaultsCategory)]
    [DisplayName("Mod package ID prefix")]
    [Description("When set, this value will pe prepenced to the project name in the mod package ID prefix field of the project creation wizard.")]
    public string? DefaultModPackageIdPrefix { get; set; }

    [Category(ProjectDefaultsCategory)]
    [DisplayName("Project layout")]
    [Description("Determines whether the solution will have the mod assets at top level and the mod source code in a subdirectory, or vice versa.")]
    [DefaultValue(ProjectLayout.AssetsAtTopLevel)]
    [TypeConverter(typeof(EnumDescriptionConverter))]
    public ProjectLayout DefaultProjectLayout { get; set; }

    public override void LoadSettingsFromStorage()
    {
        ThreadHelper.JoinableTaskFactory.Run(RimWorldSettingsManager.LoadSafeAsync);

        var settings = RimWorldSettingsManager.Settings;
        UseHarmonyByDefault = settings.ProjectDefaults.UseHarmony;
        DefaultModAuthor = settings.ProjectDefaults.ModAuthor;
        DefaultModPackageIdPrefix = settings.ProjectDefaults.ModPackageIdPrefix;
    }

    public override void SaveSettingsToStorage()
    {
        var settings = RimWorldSettingsManager.Settings;
        settings.ProjectDefaults.UseHarmony = UseHarmonyByDefault;
        settings.ProjectDefaults.ModAuthor = DefaultModAuthor?.Trim();
        settings.ProjectDefaults.ModPackageIdPrefix = DefaultModPackageIdPrefix?.Trim();

        ThreadHelper.JoinableTaskFactory.Run(RimWorldSettingsManager.SaveSafeAsync);
    }
}