using UnityModStudio.Common.Options;

namespace UnityModStudio.Build.Tests;

[TestClass]
public class SingleVersionBuildTests : BuildTestsBase
{
    [TestMethod]
    public void WhenGameHasDefaultSettings_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("2018-net4-v1.0"), Version = "1.0" };
        var game11 = new Game { Path = MakeGameCopy("2018-net4-v1.1"), Version = "1.1" };
        ResolveGameProperties(game);
        ResolveGameProperties(game11);
        SetupGameRegistry(game, game11);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\SingleVersion\SingleVersion.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        var modAssemblyPath = Path.Combine(game.Path, @"SingleVersion\SingleVersion.dll");
        Assert.IsTrue(File.Exists(modAssemblyPath));
        VerifyModAssemblyConstants(modAssemblyPath, "IsGame10", "IsGame10OrGreater");
        Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "SingleVersion"), false));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=SingleVersion\SingleVersion.dll"));
        Assert.IsFalse(File.Exists(Path.Combine(game11.Path, @"SingleVersion\SingleVersion.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game11.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game11.Path, "version.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game11.Path, "doorstop_config.ini")));
    }

    [TestMethod]
    public void WhenProjectHasModSourcePath_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("2018-net4-v1.0"), Version = "1.0" };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\SingleVersionWithModSourcePath\Sources\SingleVersionWithModSourcePath.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"SingleVersionWithModSourcePath\1.0\Assemblies\SingleVersionWithModSourcePath.dll")));
        Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "SingleVersionWithModSourcePath"), false));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=SingleVersionWithModSourcePath\1.0\Assemblies\SingleVersionWithModSourcePath.dll"));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"SingleVersionWithModSourcePath\Defs\Defs.xml")));
        Assert.IsFalse(Directory.Exists(Path.Combine(game.Path, @"SingleVersionWithModSourcePath\Sources")));
    }

    [TestMethod]
    public void WhenProjectHasModSourcePathAndGameHasDeploySourceCode_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("2018-net4-v1.0"), Version = "1.0", DeploySourceCode = true };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\SingleVersionWithModSourcePath\Sources\SingleVersionWithModSourcePath.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"SingleVersionWithModSourcePath\1.0\Assemblies\SingleVersionWithModSourcePath.dll")));
        Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "SingleVersionWithModSourcePath"), false));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=SingleVersionWithModSourcePath\1.0\Assemblies\SingleVersionWithModSourcePath.dll"));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"SingleVersionWithModSourcePath\Defs\Defs.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"SingleVersionWithModSourcePath\Sources\SingleVersionWithModSourcePath.csproj")));
    }
}