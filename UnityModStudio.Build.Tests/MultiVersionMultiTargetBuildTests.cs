using UnityModStudio.Common.Options;

namespace UnityModStudio.Build.Tests;

[TestClass]
public class MultiVersionMultiTargetBuildTests : BuildTestsBase
{
    [TestMethod]
    public void WhenGameHasDefaultSettings_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("2018-net4"), Version = "1.0" };
        var game11 = new Game { Path = MakeGameCopy("2018-netstandard20"), Version = "1.1" };
        var game20 = new Game { Path = MakeGameCopy("2018-netstandard20"), Version = "2.0" };
        ResolveGameProperties(game);
        ResolveGameProperties(game11);
        ResolveGameProperties(game20);
        SetupGameRegistry(game, game11, game20);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\MultiVersionMultiTarget\MultiVersionMultiTarget.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"MultiVersionMultiTarget\MultiVersionMultiTarget.dll")));
        Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "MultiVersionMultiTarget"), false));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=MultiVersionMultiTarget\MultiVersionMultiTarget.dll"));
        Assert.IsTrue(File.Exists(Path.Combine(game11.Path, @"MultiVersionMultiTarget\MultiVersionMultiTarget.dll")));
        Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game11.Path, "MultiVersionMultiTarget"), false));
        Assert.IsTrue(File.Exists(Path.Combine(game11.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game11.Path, "version.dll")));
        var doorstopConfigPath11 = Path.Combine(game11.Path, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath11));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath11).Contains(@"target_assembly=MultiVersionMultiTarget\MultiVersionMultiTarget.dll"));
        Assert.IsFalse(File.Exists(Path.Combine(game20.Path, @"MultiVersionMultiTarget\MultiVersionMultiTarget.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game20.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game20.Path, "version.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game20.Path, "doorstop_config.ini")));

        // TODO: verify that mod assemblies reference correct game assemblies (this requeres that game assemblies are versioned)
    }

    [TestMethod]
    public void WhenProjectHasModSourcePath_BuildAndDeploy()
    {
        var game10 = new Game { Path = MakeGameCopy("2018-net4"), Version = "1.0" };
        var game11 = new Game { Path = MakeGameCopy("2018-netstandard20"), Version = "1.1" };
        ResolveGameProperties(game10);
        ResolveGameProperties(game11);
        SetupGameRegistry(game10, game11);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\MultiVersionMultiTargetWithModSourcePath\Sources\MultiVersionMultiTargetWithModSourcePath.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        foreach (var game in new[] { game10, game11 })
        {
            Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"MultiVersionMultiTargetWithModSourcePath\1.0\Assemblies\MultiVersionMultiTargetWithModSourcePath.dll")));
            Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"MultiVersionMultiTargetWithModSourcePath\1.1\Assemblies\MultiVersionMultiTargetWithModSourcePath.dll")));
            Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "MultiVersionMultiTargetWithModSourcePath"), false));
            Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
            Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
            var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
            Assert.IsTrue(File.Exists(doorstopConfigPath));
            Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=MultiVersionMultiTargetWithModSourcePath\1.0\Assemblies\MultiVersionMultiTargetWithModSourcePath.dll"));
            Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=MultiVersionMultiTargetWithModSourcePath\1.1\Assemblies\MultiVersionMultiTargetWithModSourcePath.dll"));
            Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"MultiVersionMultiTargetWithModSourcePath\Defs\Defs.xml")));
            Assert.IsFalse(Directory.Exists(Path.Combine(game.Path, @"MultiVersionMultiTargetWithModSourcePath\Sources")));
        }

        // TODO: verify that mod assemblies reference correct game assemblies (this requeres that game assemblies are versioned)
    }

    [TestMethod]
    public void WhenProjectHasModSourcePathAndGameHasDeploySourceCode_BuildAndDeploy()
    {
        var game10 = new Game { Path = MakeGameCopy("2018-net4"), Version = "1.0" };
        var game11 = new Game { Path = MakeGameCopy("2018-netstandard20"), Version = "1.1", DeploySourceCode = true };
        ResolveGameProperties(game10);
        ResolveGameProperties(game11);
        SetupGameRegistry(game10, game11);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\MultiVersionMultiTargetWithModSourcePath\Sources\MultiVersionMultiTargetWithModSourcePath.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        foreach (var game in new[] { game10, game11 })
        {
            Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"MultiVersionMultiTargetWithModSourcePath\1.0\Assemblies\MultiVersionMultiTargetWithModSourcePath.dll")));
            Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"MultiVersionMultiTargetWithModSourcePath\1.1\Assemblies\MultiVersionMultiTargetWithModSourcePath.dll")));
            Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "MultiVersionMultiTargetWithModSourcePath"), false));
            Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
            Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
            var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
            Assert.IsTrue(File.Exists(doorstopConfigPath));
            Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=MultiVersionMultiTargetWithModSourcePath\1.0\Assemblies\MultiVersionMultiTargetWithModSourcePath.dll"));
            Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=MultiVersionMultiTargetWithModSourcePath\1.1\Assemblies\MultiVersionMultiTargetWithModSourcePath.dll"));
            Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"MultiVersionMultiTargetWithModSourcePath\Defs\Defs.xml")));
        }
        Assert.IsFalse(Directory.Exists(Path.Combine(game10.Path, @"MultiVersionMultiTargetWithModSourcePath\Sources\")));
        Assert.IsTrue(File.Exists(Path.Combine(game11.Path, @"MultiVersionMultiTargetWithModSourcePath\Sources\MultiVersionMultiTargetWithModSourcePath.csproj")));

        // TODO: verify that mod assemblies reference correct game assemblies (this requeres that game assemblies are versioned)
    }
}