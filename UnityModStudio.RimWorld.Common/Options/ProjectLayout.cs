using System.ComponentModel;

namespace UnityModStudio.RimWorld.Common.Options;

public enum ProjectLayout
{
    [Description("Assets at top level")]
    AssetsAtTopLevel,
    [Description("Project at top level")]
    ProjectAtTopLevel,
}