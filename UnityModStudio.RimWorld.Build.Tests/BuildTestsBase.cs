using UnityModStudio.Common.Tests;

namespace UnityModStudio.RimWorld.Build.Tests;

public abstract class BuildTestsBase : UnityModStudio.Build.Tests.BuildTestsBase
{
#pragma warning disable MSTEST0036
    protected new string MakeGameCopy(string version)
#pragma warning restore MSTEST0036
    {
        var scratchDir = CreateScratchDir();
        TestUtils.CopyDirectory(Path.Combine(GameInfo.Path, version), scratchDir.FullName);
        return scratchDir.FullName;
    }
}