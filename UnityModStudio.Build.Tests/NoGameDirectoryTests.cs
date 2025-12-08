using UnityModStudio.Common.Tests;

namespace UnityModStudio.Build.Tests;

[TestClass]
public class NoGameDirectoryTests : BuildTestsBase
{
    [TestMethod]
    public void WhenProjectHasNoGameVersion_Build()
    {
        var scratchPath = GetScratchPath();
        TestUtils.CopyDirectory(@"Projects\NoGame\NonVersioned", scratchPath);
        var projectPath = Path.Combine(scratchPath, @"Sources\Mod.csproj");
        var (project, logger) = GetProjectWithRestore(projectPath);

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        Assert.IsEmpty(logger.BuildWarnings);
        Assert.IsTrue(File.Exists(Path.Combine(scratchPath, @"Assemblies\Mod.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(scratchPath, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(scratchPath, "version.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(scratchPath, "doorstop_config.ini")));
    }

    [TestMethod]
    public void WhenProjectHasSingleGameVersion_Build()
    {
        var scratchPath = GetScratchPath();
        TestUtils.CopyDirectory(@"Projects\NoGame\SingleVersion", scratchPath);
        var projectPath = Path.Combine(scratchPath, @"Sources\Mod.csproj");
        var (project, logger) = GetProjectWithRestore(projectPath);

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        Assert.IsEmpty(logger.BuildWarnings);
        Assert.IsTrue(File.Exists(Path.Combine(scratchPath, @"Assemblies\Mod.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(scratchPath, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(scratchPath, "version.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(scratchPath, "doorstop_config.ini")));
    }

    [TestMethod]
    public void WhenProjectHasMultipleGameVersions_ProduceError()
    {
        var scratchPath = GetScratchPath();
        TestUtils.CopyDirectory(@"Projects\NoGame\MultiVersion", scratchPath);
        var projectPath = Path.Combine(scratchPath, @"Sources\Mod.csproj");
        var (project, logger) = GetProjectWithRestore(projectPath);

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.HasCount(1, logger.BuildErrors);
        Assert.AreEqual("UMS0017", logger.BuildErrors[0].Code);
        Assert.AreEqual("Building multi-version mod without game directory is not supported.", logger.BuildErrors[0].Message);
        Assert.IsEmpty(logger.BuildWarnings);
        Assert.IsFalse(File.Exists(Path.Combine(scratchPath, @"Assemblies\Mod.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(scratchPath, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(scratchPath, "version.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(scratchPath, "doorstop_config.ini")));
    }
}