using UnityModStudio.Common.Options;

namespace UnityModStudio.Build.Tests;

[TestClass]
public class MultiVersionBuildTests : BuildTestsBase
{
    [TestMethod]
    public void WhenGameHasDefaultSettings_BuildAndDeploy()
    {
        var game10 = new Game { Path = MakeGameCopy("2018-net4-v1.0"), Version = "1.0" };
        var game11 = new Game { Path = MakeGameCopy("2018-net4-v1.1"), Version = "1.1" };
        var game20 = new Game { Path = MakeGameCopy("2018-netstandard20-v2.0"), Version = "2.0" };
        ResolveGameProperties(game10);
        ResolveGameProperties(game11);
        ResolveGameProperties(game20);
        SetupGameRegistry(game10, game11, game20);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\MultiVersion\MultiVersion.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        foreach (var game in new[] { game10, game11 })
        {
            var modAssemblyPath = Path.Combine(game.Path, @"MultiVersion\MultiVersion.dll");
            Assert.IsTrue(File.Exists(modAssemblyPath));
            VerifyModAssemblyGameVersion(modAssemblyPath, game.Version!);
            Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "MultiVersion"), false));
            Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
            Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
            var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
            Assert.IsTrue(File.Exists(doorstopConfigPath));
            Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=MultiVersion\MultiVersion.dll"));
        }
        Assert.IsFalse(File.Exists(Path.Combine(game20.Path, @"MultiVersion\MultiVersion.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game20.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game20.Path, "version.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game20.Path, "doorstop_config.ini")));
    }

    [TestMethod]
    public void WhenProjectHasModSourcePath_BuildAndDeploy()
    {
        var game10 = new Game { Path = MakeGameCopy("2018-net4-v1.0"), Version = "1.0" };
        var game11 = new Game { Path = MakeGameCopy("2018-net4-v1.1"), Version = "1.1" };
        ResolveGameProperties(game10);
        ResolveGameProperties(game11);
        SetupGameRegistry(game10, game11);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\MultiVersionWithModSourcePath\Sources\MultiVersionWithModSourcePath.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        foreach (var game in new[] { game10, game11 })
        {
            var modAssemblyPath10 = Path.Combine(game.Path, @"MultiVersionWithModSourcePath\1.0\Assemblies\MultiVersionWithModSourcePath.dll");
            Assert.IsTrue(File.Exists(modAssemblyPath10));
            VerifyModAssemblyGameVersion(modAssemblyPath10, "1.0");
            var modAssemblyPath11 = Path.Combine(game.Path, @"MultiVersionWithModSourcePath\1.1\Assemblies\MultiVersionWithModSourcePath.dll");
            Assert.IsTrue(File.Exists(modAssemblyPath11));
            VerifyModAssemblyGameVersion(modAssemblyPath11, "1.1");
            Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "MultiVersionWithModSourcePath"), false));
            Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
            Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
            var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
            Assert.IsTrue(File.Exists(doorstopConfigPath));
            Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=MultiVersionWithModSourcePath\1.0\Assemblies\MultiVersionWithModSourcePath.dll"));
            Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=MultiVersionWithModSourcePath\1.1\Assemblies\MultiVersionWithModSourcePath.dll"));
            Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"MultiVersionWithModSourcePath\Defs\Defs.xml")));
            Assert.IsFalse(Directory.Exists(Path.Combine(game.Path, @"MultiVersionWithModSourcePath\Sources")));            
        }
    }

    [TestMethod]
    public void WhenProjectHasModSourcePathAndGameHasDeploySourceCode_BuildAndDeploy()
    {
        var game10 = new Game { Path = MakeGameCopy("2018-net4-v1.0"), Version = "1.0" };
        var game11 = new Game { Path = MakeGameCopy("2018-net4-v1.1"), Version = "1.1", DeploySourceCode = true };
        ResolveGameProperties(game10);
        ResolveGameProperties(game11);
        SetupGameRegistry(game10, game11);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\MultiVersionWithModSourcePath\Sources\MultiVersionWithModSourcePath.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        foreach (var game in new[] { game10, game11 })
        {
            var modAssemblyPath10 = Path.Combine(game.Path, @"MultiVersionWithModSourcePath\1.0\Assemblies\MultiVersionWithModSourcePath.dll");
            Assert.IsTrue(File.Exists(modAssemblyPath10));
            VerifyModAssemblyGameVersion(modAssemblyPath10, "1.0");
            var modAssemblyPath11 = Path.Combine(game.Path, @"MultiVersionWithModSourcePath\1.1\Assemblies\MultiVersionWithModSourcePath.dll");
            Assert.IsTrue(File.Exists(modAssemblyPath11));
            VerifyModAssemblyGameVersion(modAssemblyPath11, "1.1");
            Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "MultiVersionWithModSourcePath"), false));
            Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
            Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
            var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
            Assert.IsTrue(File.Exists(doorstopConfigPath));
            Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=MultiVersionWithModSourcePath\1.0\Assemblies\MultiVersionWithModSourcePath.dll"));
            Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=MultiVersionWithModSourcePath\1.1\Assemblies\MultiVersionWithModSourcePath.dll"));
            Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"MultiVersionWithModSourcePath\Defs\Defs.xml")));
        }
        Assert.IsFalse(Directory.Exists(Path.Combine(game10.Path, @"MultiVersionWithModSourcePath\Sources\")));
        Assert.IsTrue(File.Exists(Path.Combine(game11.Path, @"MultiVersionWithModSourcePath\Sources\MultiVersionWithModSourcePath.csproj")));
    }

    [TestMethod]
    public void WhenGameVersionIsSpecified_BuildAndDeploySingleVersion()
    {
        var game10 = new Game { Path = MakeGameCopy("2018-net4-v1.0"), Version = "1.0" };
        var game11 = new Game { Path = MakeGameCopy("2018-net4-v1.1"), Version = "1.1" };
        ResolveGameProperties(game10);
        ResolveGameProperties(game11);
        SetupGameRegistry(game10, game11);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\MultiVersion\MultiVersion.csproj", 
            new Dictionary<string, string>{{"GameVersion", "1.1"}});

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        Assert.IsFalse(Directory.Exists(Path.Combine(game10.Path, "MultiVersion")));
        Assert.IsFalse(File.Exists(Path.Combine(game10.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game10.Path, "version.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game10.Path, "doorstop_config.ini")));
        var modAssemblyPath = Path.Combine(game11.Path, @"MultiVersion\MultiVersion.dll");
        Assert.IsTrue(File.Exists(modAssemblyPath));
        VerifyModAssemblyGameVersion(modAssemblyPath, "1.1");
        Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game11.Path, "MultiVersion"), false));
        Assert.IsTrue(File.Exists(Path.Combine(game11.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game11.Path, "version.dll")));
        var doorstopConfigPath = Path.Combine(game11.Path, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=MultiVersion\MultiVersion.dll"));
    }

    [TestMethod]
    public void WhenProjectHasModSourcePathAndGameVersionIsSpecified_BuildAndDeploySingleVersion()
    {
        var game10 = new Game { Path = MakeGameCopy("2018-net4-v1.0"), Version = "1.0" };
        var game11 = new Game { Path = MakeGameCopy("2018-net4-v1.1"), Version = "1.1" };
        ResolveGameProperties(game10);
        ResolveGameProperties(game11);
        SetupGameRegistry(game10, game11);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\MultiVersionWithModSourcePath\Sources\MultiVersionWithModSourcePath.csproj", 
            new Dictionary<string, string>{{"GameVersion", "1.1"}});

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        Assert.IsFalse(Directory.Exists(Path.Combine(game10.Path, "MultiVersionWithModSourcePath")));
        Assert.IsFalse(File.Exists(Path.Combine(game10.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game10.Path, "version.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game10.Path, "doorstop_config.ini")));
        var modAssemblyPath10 = Path.Combine(game11.Path, @"MultiVersionWithModSourcePath\1.0\Assemblies\MultiVersionWithModSourcePath.dll");
        Assert.IsFalse(File.Exists(modAssemblyPath10));
        var modAssemblyPath11 = Path.Combine(game11.Path, @"MultiVersionWithModSourcePath\1.1\Assemblies\MultiVersionWithModSourcePath.dll");
        Assert.IsTrue(File.Exists(modAssemblyPath11));
        VerifyModAssemblyGameVersion(modAssemblyPath11, "1.1");
        Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game11.Path, "MultiVersionWithModSourcePath"), false));
        Assert.IsTrue(File.Exists(Path.Combine(game11.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game11.Path, "version.dll")));
        var doorstopConfigPath = Path.Combine(game11.Path, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=MultiVersionWithModSourcePath\1.0\Assemblies\MultiVersionWithModSourcePath.dll"));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=MultiVersionWithModSourcePath\1.1\Assemblies\MultiVersionWithModSourcePath.dll"));
        Assert.IsTrue(File.Exists(Path.Combine(game11.Path, @"MultiVersionWithModSourcePath\Defs\Defs.xml")));
        Assert.IsFalse(Directory.Exists(Path.Combine(game11.Path, @"MultiVersionWithModSourcePath\Sources")));
    }
}