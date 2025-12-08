using UnityModStudio.Build.Tests;
using UnityModStudio.Common.Options;

namespace UnityModStudio.BepInEx.Build.Tests;

[TestClass]
public class BepInEx5BuildTests : BuildTestsBase
{
    [TestMethod]
    public void WhenProjectHasNoVersion_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("2018-net4-v1.0") };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\BepInEx5\NonVersioned\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        Assert.IsEmpty(logger.BuildWarnings);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"BepInEx\plugins\Mod\Mod.dll")));
        Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, @"BepInEx\plugins\Mod"), false));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsTrue(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=BepInEx\core\BepInEx.Preloader.dll"));
    }

    [TestMethod]
    public void WhenProjectHasSingleVersion_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("2018-net4-v1.0"), Version = "1.0" };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\BepInEx5\SingleVersion\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        Assert.IsEmpty(logger.BuildWarnings);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"BepInEx\plugins\Mod\Mod.dll")));
        Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, @"BepInEx\plugins\Mod"), false));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsTrue(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=BepInEx\core\BepInEx.Preloader.dll"));
    }

    [TestMethod]
    public void WhenProjectHasMultipleVersions_BuildAndDeploy()
    {
        var game10 = new Game { Path = MakeGameCopy("2018-net4-v1.0"), Version = "1.0" };
        var game11 = new Game { Path = MakeGameCopy("2018-net4-v1.1"), Version = "1.1" };
        ResolveGameProperties(game10);
        ResolveGameProperties(game11);
        SetupGameRegistry(game10, game11);
        var (project, logger) = GetProjectWithRestore(@"Projects\BepInEx5\MultiVersion\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        Assert.IsEmpty(logger.BuildWarnings);
        foreach (var game in new[] { game10, game11 })
        {
            var modAssemblyPath = Path.Combine(game.Path, @"BepInEx\plugins\Mod\Mod.dll");
            Assert.IsTrue(File.Exists(modAssemblyPath));
            VerifyModAssemblyGameVersion(modAssemblyPath, game.Version!);
            Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, @"BepInEx\plugins\Mod"), false));
            Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
            Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
            var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
            Assert.IsTrue(File.Exists(doorstopConfigPath));
            Assert.IsTrue(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=BepInEx\core\BepInEx.Preloader.dll"));
        }
    }

    [TestMethod]
    public void WhenProjectHasMultipleVersionsAndMultipleFrameworks_BuildAndDeploy()
    {
        var game10 = new Game { Path = MakeGameCopy("2018-net4-v1.0"), Version = "1.0" };
        var game11 = new Game { Path = MakeGameCopy("2018-net4-v1.1"), Version = "1.1" };
        var game20 = new Game { Path = MakeGameCopy("2018-netstandard20-v2.0"), Version = "2.0" };
        ResolveGameProperties(game10);
        ResolveGameProperties(game11);
        ResolveGameProperties(game20);
        SetupGameRegistry(game10, game11, game20);
        var (project, logger) = GetProjectWithRestore(@"Projects\BepInEx5\MultiVersionMultiTarget\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        Assert.IsEmpty(logger.BuildWarnings);
        foreach (var game in new[] { game10, game11, game20 })
        {
            var modAssemblyPath = Path.Combine(game.Path, @"BepInEx\plugins\Mod\Mod.dll");
            Assert.IsTrue(File.Exists(modAssemblyPath));
            VerifyModAssemblyGameVersion(modAssemblyPath, game.Version!);
            Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, @"BepInEx\plugins\Mod"), false));
            Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
            Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
            var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
            Assert.IsTrue(File.Exists(doorstopConfigPath));
            Assert.IsTrue(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=BepInEx\core\BepInEx.Preloader.dll"));
        }
    }

    [TestMethod]
    public void WhenProjectHasModSourcePath_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("2018-net4-v1.0") };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\BepInEx5\WithModSourcePath\Sources\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        Assert.IsEmpty(logger.BuildWarnings);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"BepInEx\plugins\Mod\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"BepInEx\plugins\Mod\Defs\Defs.xml")));
        Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, @"BepInEx\plugins\Mod"), false));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsTrue(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=BepInEx\core\BepInEx.Preloader.dll"));
    }
}