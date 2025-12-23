using UnityModStudio.Build.Tests;
using UnityModStudio.Common.Options;
using UnityModStudio.Common.Tests;

namespace UnityModStudio.RimWorld.Build.Tests;

[TestClass]
public sealed class BuildTests : BuildTestsBase
{
    [TestMethod]
    public void WhenProjectIsAtTopLevelAndHasSingleVersion_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("1.6.4633"), Version = "1.6" };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\ProjectAtTopLevel\SingleVersion\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        //Assert.IsEmpty(logger.BuildWarnings); // Some warnings are expected because RimRef has some assembly version inconsistencies.
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\About\About.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "doorstop_config.ini")));
    }

    [TestMethod]
    public void WhenProjectIsAtTopLevelAndHasMultipleVersions_BuildAndDeploy()
    {
        var game14 = new Game { Path = MakeGameCopy("1.4.3901"), Version = "1.4" };
        var game15 = new Game { Path = MakeGameCopy("1.5.4409"), Version = "1.5" };
        ResolveGameProperties(game14);
        ResolveGameProperties(game15);
        SetupGameRegistry(game14, game15);
        var (project, logger) = GetProjectWithRestore(@"Projects\ProjectAtTopLevel\MultiVersion\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        //Assert.IsEmpty(logger.BuildWarnings); // Some warnings are expected because RimRef has some assembly version inconsistencies.
        Assert.IsTrue(File.Exists(Path.Combine(game14.Path, @"Mods\Mod\1.4\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game14.Path, @"Mods\Mod\1.5\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game14.Path, @"Mods\Mod\About\About.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game14.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game14.Path, "version.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game14.Path, "doorstop_config.ini")));
        Assert.IsTrue(File.Exists(Path.Combine(game15.Path, @"Mods\Mod\1.4\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game15.Path, @"Mods\Mod\1.5\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game15.Path, @"Mods\Mod\About\About.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game15.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game15.Path, "version.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game15.Path, "doorstop_config.ini")));
    }

    [TestMethod]
    public void WhenProjectIsAtTopLevelAndHasMultipleVersionsAndMultipleFrameworks_BuildAndDeploy()
    {
        var game15 = new Game { Path = MakeGameCopy("1.5.4409"), Version = "1.5" };
        var game16 = new Game { Path = MakeGameCopy("1.6.4633"), Version = "1.6" };
        ResolveGameProperties(game15);
        ResolveGameProperties(game16);
        SetupGameRegistry(game15, game16);
        var (project, logger) = GetProjectWithRestore(@"Projects\ProjectAtTopLevel\MultiVersionMultiTarget\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        //Assert.IsEmpty(logger.BuildWarnings); // Some warnings are expected because RimRef has some assembly version inconsistencies.
        Assert.IsTrue(File.Exists(Path.Combine(game15.Path, @"Mods\Mod\1.5\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game15.Path, @"Mods\Mod\1.6\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game15.Path, @"Mods\Mod\About\About.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game15.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game15.Path, "version.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game15.Path, "doorstop_config.ini")));
        Assert.IsTrue(File.Exists(Path.Combine(game16.Path, @"Mods\Mod\1.5\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game16.Path, @"Mods\Mod\1.6\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game16.Path, @"Mods\Mod\About\About.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game16.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game16.Path, "version.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game16.Path, "doorstop_config.ini")));
    }

    [TestMethod]
    public void WhenAssetsAreAtTopLevelAndProjectHasSingleVersion_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("1.6.4633"), Version = "1.6" };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\AssetsAtTopLevel\SingleVersion\Sources\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        //Assert.IsEmpty(logger.BuildWarnings); // Some warnings are expected because RimRef has some assembly version inconsistencies.
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\About\About.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "doorstop_config.ini")));
    }

    [TestMethod]
    public void WhenAssetsAreAtTopLevelAndProjectHasSingleVersionAndIsNested_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("1.6.4633"), Version = "1.6" };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\AssetsAtTopLevel\SingleVersionNested\Sources\Mod\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        //Assert.IsEmpty(logger.BuildWarnings); // Some warnings are expected because RimRef has some assembly version inconsistencies.
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\About\About.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "doorstop_config.ini")));
    }

    [TestMethod]
    public void WhenAssetsAreAtTopLevelAndProjectHasMultipleVersions_BuildAndDeploy()
    {
        var game14 = new Game { Path = MakeGameCopy("1.4.3901"), Version = "1.4" };
        var game15 = new Game { Path = MakeGameCopy("1.5.4409"), Version = "1.5" };
        ResolveGameProperties(game14);
        ResolveGameProperties(game15);
        SetupGameRegistry(game14, game15);
        var (project, logger) = GetProjectWithRestore(@"Projects\AssetsAtTopLevel\MultiVersion\Sources\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        //Assert.IsEmpty(logger.BuildWarnings); // Some warnings are expected because RimRef has some assembly version inconsistencies.
        Assert.IsTrue(File.Exists(Path.Combine(game14.Path, @"Mods\Mod\1.4\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game14.Path, @"Mods\Mod\1.5\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game14.Path, @"Mods\Mod\About\About.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game14.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game14.Path, "version.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game14.Path, "doorstop_config.ini")));
        Assert.IsTrue(File.Exists(Path.Combine(game15.Path, @"Mods\Mod\1.4\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game15.Path, @"Mods\Mod\1.5\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game15.Path, @"Mods\Mod\About\About.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game15.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game15.Path, "version.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game15.Path, "doorstop_config.ini")));
    }

    [TestMethod]
    public void WhenAssetsAreAtTopLevelAndProjectHasMultipleVersionsAndMultipleFrameworks_BuildAndDeploy()
    {
        var game15 = new Game { Path = MakeGameCopy("1.5.4409"), Version = "1.5" };
        var game16 = new Game { Path = MakeGameCopy("1.6.4633"), Version = "1.6" };
        ResolveGameProperties(game15);
        ResolveGameProperties(game16);
        SetupGameRegistry(game15, game16);
        var (project, logger) = GetProjectWithRestore(@"Projects\AssetsAtTopLevel\MultiVersionMultiTarget\Sources\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        //Assert.IsEmpty(logger.BuildWarnings); // Some warnings are expected because RimRef has some assembly version inconsistencies.
        Assert.IsTrue(File.Exists(Path.Combine(game15.Path, @"Mods\Mod\1.5\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game15.Path, @"Mods\Mod\1.6\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game15.Path, @"Mods\Mod\About\About.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game15.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game15.Path, "version.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game15.Path, "doorstop_config.ini")));
        Assert.IsTrue(File.Exists(Path.Combine(game16.Path, @"Mods\Mod\1.5\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game16.Path, @"Mods\Mod\1.6\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game16.Path, @"Mods\Mod\About\About.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game16.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game16.Path, "version.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game16.Path, "doorstop_config.ini")));
    }

#pragma warning disable MSTEST0036
    private new string MakeGameCopy(string version)
#pragma warning restore MSTEST0036
    {
        var scratchDir = CreateScratchDir();
        TestUtils.CopyDirectory(Path.Combine(GameInfo.Path, version), scratchDir.FullName);
        return scratchDir.FullName;
    }
}
