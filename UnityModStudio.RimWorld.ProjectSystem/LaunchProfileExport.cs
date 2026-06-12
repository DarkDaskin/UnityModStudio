using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;

namespace UnityModStudio.RimWorld.ProjectSystem;

internal static class LaunchProfileExport
{
    [ExportPropertyXamlRuleDefinition(
        xamlResourceAssemblyName: $"{ThisAssembly.AssemblyName}, Version={ThisAssembly.AssemblyVersion}, Culture=neutral, PublicKeyToken={ThisAssembly.PublicKeyToken}",
        xamlResourceStreamName: "XamlRuleToCode:UnityModLaunchProfile.xaml",
        context: PropertyPageContexts.Project)]
    [AppliesTo(ProjectCapability.UnityModStudioForRimWorld)]
    [Order(1000)]
    public static int UnityModLaunchProfileRule;
}