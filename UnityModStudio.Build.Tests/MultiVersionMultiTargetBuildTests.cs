using UnityModStudio.Common.Options;

namespace UnityModStudio.Build.Tests;

[TestClass]
public class MultiVersionMultiTargetBuildTests : BuildTestsBase
{
    [TestMethod]
    public void WhenGameHasDefaultSettings_BuildAndDeploy()
    {
        var game10 = new Game { Path = MakeGameCopy("2018-net4-v1.0"), Version = "1.0" };
        var game11 = new Game { Path = MakeGameCopy("2018-net4-v1.1"), Version = "1.1" };
        var game20 = new Game { Path = MakeGameCopy("2018-netstandard20-v2.0"), Version = "2.0" };
        var game21 = new Game { Path = MakeGameCopy("2018-netstandard20-v2.1"), Version = "2.1" };
        ResolveGameProperties(game10);
        ResolveGameProperties(game11);
        ResolveGameProperties(game20);
        ResolveGameProperties(game21);
        SetupGameRegistry(game10, game11, game20, game21);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\MultiVersionMultiTarget\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        foreach (var game in new[] { game10, game11, game20 })
        {
            var modAssemblyPath = Path.Combine(game.Path, @"Mod\Mod.dll");
            Assert.IsTrue(File.Exists(modAssemblyPath));
            VerifyModAssemblyGameVersion(modAssemblyPath, game.Version!);
            if (game == game10)
                VerifyModAssemblyConstants(modAssemblyPath, "IsGame10", "IsGame10OrGreater");
            if (game == game11)
                VerifyModAssemblyConstants(modAssemblyPath, "IsGame11", "IsGame10OrGreater", "IsGame11OrGreater");
            if (game == game20)
                VerifyModAssemblyConstants(modAssemblyPath, "IsGame20", "IsGame10OrGreater", "IsGame11OrGreater", "IsGame20OrGreater");
            Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "Mod"), false));
            Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
            Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
            var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
            Assert.IsTrue(File.Exists(doorstopConfigPath));
            Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=Mod\Mod.dll"));
        }
        Assert.IsFalse(File.Exists(Path.Combine(game21.Path, @"Mod\Mod.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game21.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game21.Path, "version.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game21.Path, "doorstop_config.ini")));
    }

    [TestMethod]
    public void WhenProjectHasModSourcePath_BuildAndDeploy()
    {
        var game10 = new Game { Path = MakeGameCopy("2018-net4-v1.0"), Version = "1.0" };
        var game11 = new Game { Path = MakeGameCopy("2018-net4-v1.1"), Version = "1.1" };
        var game20 = new Game { Path = MakeGameCopy("2018-netstandard20-v2.0"), Version = "2.0" };
        ResolveGameProperties(game10);
        ResolveGameProperties(game11);
        ResolveGameProperties(game20);
        SetupGameRegistry(game10, game11, game20);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\MultiVersionMultiTargetWithModSourcePath\Sources\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        foreach (var game in new[] { game10, game11, game20 })
        {
            var modAssemblyPath10 = Path.Combine(game.Path, @"Mod\1.0\Assemblies\Mod.dll");
            Assert.IsTrue(File.Exists(modAssemblyPath10));
            VerifyModAssemblyGameVersion(modAssemblyPath10, "1.0");
            var modAssemblyPath11 = Path.Combine(game.Path, @"Mod\1.1\Assemblies\Mod.dll");
            Assert.IsTrue(File.Exists(modAssemblyPath11));
            VerifyModAssemblyGameVersion(modAssemblyPath11, "1.1");
            var modAssemblyPath20 = Path.Combine(game.Path, @"Mod\2.0\Assemblies\Mod.dll");
            Assert.IsTrue(File.Exists(modAssemblyPath20));
            VerifyModAssemblyGameVersion(modAssemblyPath20, "2.0");
            Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "Mod"), false));
            Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
            Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
            var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
            Assert.IsTrue(File.Exists(doorstopConfigPath));
            Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=Mod\1.0\Assemblies\Mod.dll"));
            Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=Mod\1.1\Assemblies\Mod.dll"));
            Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=Mod\2.0\Assemblies\Mod.dll"));
            Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mod\Defs\Defs.xml")));
            Assert.IsFalse(Directory.Exists(Path.Combine(game.Path, @"Mod\Sources")));
        }
    }

    [TestMethod]
    public void WhenProjectHasModSourcePathAndGameHasDeploySourceCode_BuildAndDeploy()
    {
        var game10 = new Game { Path = MakeGameCopy("2018-net4-v1.0"), Version = "1.0" };
        var game11 = new Game { Path = MakeGameCopy("2018-net4-v1.1"), Version = "1.1", DeploySourceCode = true };
        var game20 = new Game { Path = MakeGameCopy("2018-netstandard20-v2.0"), Version = "2.0" };
        ResolveGameProperties(game10);
        ResolveGameProperties(game11);
        ResolveGameProperties(game20);
        SetupGameRegistry(game10, game11, game20);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\MultiVersionMultiTargetWithModSourcePath\Sources\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        foreach (var game in new[] { game10, game11, game20 })
        {
            var modAssemblyPath10 = Path.Combine(game.Path, @"Mod\1.0\Assemblies\Mod.dll");
            Assert.IsTrue(File.Exists(modAssemblyPath10));
            VerifyModAssemblyGameVersion(modAssemblyPath10, "1.0");
            var modAssemblyPath11 = Path.Combine(game.Path, @"Mod\1.1\Assemblies\Mod.dll");
            Assert.IsTrue(File.Exists(modAssemblyPath11));
            VerifyModAssemblyGameVersion(modAssemblyPath11, "1.1");
            var modAssemblyPath20 = Path.Combine(game.Path, @"Mod\2.0\Assemblies\Mod.dll");
            Assert.IsTrue(File.Exists(modAssemblyPath20));
            VerifyModAssemblyGameVersion(modAssemblyPath20, "2.0");
            Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "Mod"), false));
            Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
            Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
            var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
            Assert.IsTrue(File.Exists(doorstopConfigPath));
            Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=Mod\1.0\Assemblies\Mod.dll"));
            Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=Mod\1.1\Assemblies\Mod.dll"));
            Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=Mod\2.0\Assemblies\Mod.dll"));
            Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"Mod\Defs\Defs.xml")));
        }
        Assert.IsFalse(Directory.Exists(Path.Combine(game10.Path, @"Mod\Sources\")));
        Assert.IsFalse(Directory.Exists(Path.Combine(game20.Path, @"Mod\Sources\")));
        Assert.IsTrue(File.Exists(Path.Combine(game11.Path, @"Mod\Sources\Mod.csproj")));
    }
}