using UnityModStudio.Common.Options;

namespace UnityModStudio.RimWorld.Build.Tests;

[TestClass]
public class LoadFoldersBuildTests : BuildTestsBase
{
    [TestMethod]
    public void WhenProjectIsAtTopLevelAndHasNoLoadFolders_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("1.6.4633"), Version = "1.6" };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\LoadFolders\ProjectAtTopLevel\NoLoadFolders\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\About\About.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Defs\Defs.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Common\Defs\Defs.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\Defs\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Common\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\IgnoreMe\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\IgnoreMe\Defs\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Common\IgnoreMe\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Common\IgnoreMe\Defs\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\IgnoreMe\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\IgnoreMe\Defs\Defs.xml")));
    }

    [TestMethod]
    public void WhenProjectIsAtTopLevelAndHasLoadFolders_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("1.6.4633"), Version = "1.6" };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\LoadFolders\ProjectAtTopLevel\WithLoadFolders\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\About\About.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Defs\Defs.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Old\Defs\Defs.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\New\Defs\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Old\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Old\IgnoreMe\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\New\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\New\IgnoreMe\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Common\Defs\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\Defs\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Common\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\IgnoreMe\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\IgnoreMe\Defs\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Common\IgnoreMe\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Common\IgnoreMe\Defs\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\IgnoreMe\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\IgnoreMe\Defs\Defs.xml")));
    }

    [TestMethod]
    public void WhenAssetsAreAtTopLevelAndProjectHasNoLoadFolders_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("1.6.4633"), Version = "1.6" };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\LoadFolders\AssetsAtTopLevel\NoLoadFolders\Sources\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\About\About.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Defs\Defs.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Common\Defs\Defs.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\Defs\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Common\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\IgnoreMe\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\IgnoreMe\Defs\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Common\IgnoreMe\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Common\IgnoreMe\Defs\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\IgnoreMe\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\IgnoreMe\Defs\Defs.xml")));
    }

    [TestMethod]
    public void WhenAssetsAreAtTopLevelAndProjectHasLoadFolders_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("1.6.4633"), Version = "1.6" };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\LoadFolders\AssetsAtTopLevel\WithLoadFolders\Sources\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\About\About.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Defs\Defs.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Old\Defs\Defs.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mods\Mod\New\Defs\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Old\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Old\IgnoreMe\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\New\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\New\IgnoreMe\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Common\Defs\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\Defs\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Common\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\IgnoreMe\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\IgnoreMe\Defs\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Common\IgnoreMe\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\Common\IgnoreMe\Defs\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\IgnoreMe\Defs.xml")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"Mods\Mod\1.6\IgnoreMe\Defs\Defs.xml")));
    }

    [TestMethod]
    public void WhenAssetsAreAtTopLevelAndProjectHasInvalidLoadFolders_ProduceError()
    {
        var game = new Game { Path = MakeGameCopy("1.6.4633"), Version = "1.6" };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\LoadFolders\AssetsAtTopLevel\InvalidLoadFolders\Sources\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.HasCount(1, logger.BuildErrors);
        Assert.AreEqual("UMSRW0001", logger.BuildErrors[0].Code);
        Assert.AreEqual("Error while parsing LoadFolders.xml: Missing or incorrect root element.", logger.BuildErrors[0].Message);
    }
}