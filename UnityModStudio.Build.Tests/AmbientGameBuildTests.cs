using UnityModStudio.Common.Options;
using UnityModStudio.Common.Tests;

namespace UnityModStudio.Build.Tests;

[TestClass]
public class AmbientGameBuildTests : BuildTestsBase
{
    [TestInitialize]
    public void TestInitialize()
    {
        ProjectOptions.GlobalProperties["RestoreAdditionalProjectSources"] =
            Path.GetFullPath(@$"..\..\..\..\UnityModStudio.Build\bin\{Configuration}\");
    }

    [TestMethod]
    public void WhenProjectHasNoGameProperties_Build()
    {
        var gamePath = MakeGameCopy("2018-net4-v1.0");
        var modPath = Path.Combine(gamePath, @"Mods\Mod");
        Directory.CreateDirectory(modPath);
        TestUtils.CopyDirectory(@"Projects\AmbientGame\NoGameProperties", modPath);
        var projectPath = Path.Combine(modPath, @"Sources\Mod.csproj");
        var (project, logger) = GetProjectWithRestore(projectPath);

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        Assert.IsEmpty(logger.BuildWarnings);
        Assert.IsTrue(File.Exists(Path.Combine(modPath, @"Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(gamePath, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "version.dll")));
        var doorstopConfigPath = Path.Combine(gamePath, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=Mods\Mod\Assemblies\Mod.dll"));
    }

    [TestMethod]
    public void WhenProjectHasNonMatchingGameName_BuildWithWarning()
    {
        var gamePath = MakeGameCopy("2018-net4-v1.0");
        var modPath = Path.Combine(gamePath, @"Mods\Mod");
        Directory.CreateDirectory(modPath);
        TestUtils.CopyDirectory(@"Projects\AmbientGame\WrongGameName", modPath);
        var projectPath = Path.Combine(modPath, @"Sources\Mod.csproj");
        var (project, logger) = GetProjectWithRestore(projectPath);

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        Assert.HasCount(1, logger.BuildWarnings);
        Assert.AreEqual("UMS0011", logger.BuildWarnings[0].Code);
        Assert.AreEqual("Ambient game name is 'Unity2018Test', but 'WrongGame' is defined by the project.", logger.BuildWarnings[0].Message);
        Assert.IsTrue(File.Exists(Path.Combine(modPath, @"Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(gamePath, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "version.dll")));
        var doorstopConfigPath = Path.Combine(gamePath, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=Mods\Mod\Assemblies\Mod.dll"));
    }

    [TestMethod]
    public void WhenProjectHasMatchingGameName_Build()
    {
        var gamePath = MakeGameCopy("2018-net4-v1.0");
        var modPath = Path.Combine(gamePath, @"Mods\Mod");
        Directory.CreateDirectory(modPath);
        TestUtils.CopyDirectory(@"Projects\AmbientGame\WithGameName", modPath);
        var projectPath = Path.Combine(modPath, @"Sources\Mod.csproj");
        var (project, logger) = GetProjectWithRestore(projectPath);

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        Assert.IsEmpty(logger.BuildWarnings);
        Assert.IsTrue(File.Exists(Path.Combine(modPath, @"Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(gamePath, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "version.dll")));
        var doorstopConfigPath = Path.Combine(gamePath, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=Mods\Mod\Assemblies\Mod.dll"));
    }

    [TestMethod]
    public void WhenProjectHasGameVersion_Build()
    {
        var gamePath = MakeGameCopy("2018-net4-v1.0");
        var modPath = Path.Combine(gamePath, @"Mods\Mod");
        Directory.CreateDirectory(modPath);
        TestUtils.CopyDirectory(@"Projects\AmbientGame\WithGameVersion", modPath);
        var projectPath = Path.Combine(modPath, @"Sources\Mod.csproj");
        var (project, logger) = GetProjectWithRestore(projectPath);

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        Assert.IsEmpty(logger.BuildWarnings);
        Assert.IsTrue(File.Exists(Path.Combine(modPath, @"Assemblies\Mod.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(gamePath, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "version.dll")));
        var doorstopConfigPath = Path.Combine(gamePath, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=Mods\Mod\Assemblies\Mod.dll"));
    }

    [TestMethod]
    public void WhenProjectHasMultipleGameVersions_ProduceError()
    {
        var gamePath = MakeGameCopy("2018-net4-v1.0");
        var modPath = Path.Combine(gamePath, @"Mods\Mod");
        Directory.CreateDirectory(modPath);
        TestUtils.CopyDirectory(@"Projects\AmbientGame\MultipleGameVersions", modPath);
        var projectPath = Path.Combine(modPath, @"Sources\Mod.csproj");
        var (project, logger) = GetProjectWithRestore(projectPath);

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.HasCount(1, logger.BuildErrors);
        Assert.AreEqual("Building multi-version mod for an ambient game is not supported.", logger.BuildErrors[0].Message);
        Assert.IsEmpty(logger.BuildWarnings);
        Assert.IsFalse(File.Exists(Path.Combine(modPath, @"Assemblies\Mod.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "version.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "doorstop_config.ini")));
    }

    [TestMethod]
    public void WhenDoorstopIsDisabled_Build()
    {
        SetupGeneralSettings(settings => settings.AmbientGame.DoorstopMode = DoorstopMode.Disabled);
        var gamePath = MakeGameCopy("2018-net4-v1.0");
        var modPath = Path.Combine(gamePath, @"Mods\Mod");
        Directory.CreateDirectory(modPath);
        TestUtils.CopyDirectory(@"Projects\AmbientGame\NoGameProperties", modPath);
        var projectPath = Path.Combine(modPath, @"Sources\Mod.csproj");
        var (project, logger) = GetProjectWithRestore(projectPath);

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        Assert.IsEmpty(logger.BuildWarnings);
        Assert.IsTrue(File.Exists(Path.Combine(modPath, @"Assemblies\Mod.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "version.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "doorstop_config.ini")));
    }

    [TestMethod]
    public void WhenDoorstopHasAlternateConfiguration_Build()
    {
        SetupGeneralSettings(settings =>
        {
            settings.AmbientGame.DoorstopMode = DoorstopMode.DebuggingAndModLoading;
            settings.AmbientGame.UseAlternateDoorstopDllName = true;
        });
        var gamePath = MakeGameCopy("2018-net4-v1.0");
        var modPath = Path.Combine(gamePath, @"Mods\Mod");
        Directory.CreateDirectory(modPath);
        TestUtils.CopyDirectory(@"Projects\AmbientGame\NoGameProperties", modPath);
        var projectPath = Path.Combine(modPath, @"Sources\Mod.csproj");
        var (project, logger) = GetProjectWithRestore(projectPath);

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        Assert.IsEmpty(logger.BuildWarnings);
        Assert.IsTrue(File.Exists(Path.Combine(modPath, @"Assemblies\Mod.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "winhttp.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(gamePath, "version.dll")));
        var doorstopConfigPath = Path.Combine(gamePath, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsTrue(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=Mods\Mod\Assemblies\Mod.dll"));
    }

    [TestMethod]
    public void WhenAmbientGameResolutionIsDisabled_ProduceError()
    {
        SetupGeneralSettings(settings => settings.AmbientGame.IsResolutionAllowed = false);
        var gamePath = MakeGameCopy("2018-net4-v1.0");
        var modPath = Path.Combine(gamePath, @"Mods\Mod");
        Directory.CreateDirectory(modPath);
        TestUtils.CopyDirectory(@"Projects\AmbientGame\NoGameProperties", modPath);
        var projectPath = Path.Combine(modPath, @"Sources\Mod.csproj");
        var (project, logger) = GetProjectWithRestore(projectPath);

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.HasCount(1, logger.BuildErrors);
        Assert.AreEqual("No game properties are defined.", logger.BuildErrors[0].Message);
        Assert.IsEmpty(logger.BuildWarnings);
        Assert.IsFalse(File.Exists(Path.Combine(modPath, @"Assemblies\Mod.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "version.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "doorstop_config.ini")));
    }
}