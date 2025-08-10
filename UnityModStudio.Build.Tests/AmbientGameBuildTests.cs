using UnityModStudio.Common.Tests;

namespace UnityModStudio.Build.Tests;

[TestClass]
public class AmbientGameBuildTests : BuildTestsBase
{
    [TestMethod]
    public void WhenProjectHasNoGameProperties_Build()
    {
        var gamePath = MakeGameCopy("2018-net4-v1.0");
        var modPath = Path.Combine(gamePath, @"Mods\NoGameProperties");
        Directory.CreateDirectory(modPath);
        TestUtils.CopyDirectory(@"Projects\AmbientGame\NoGameProperties", modPath);
        var projectPath = Path.Combine(modPath, @"Sources\NoGameProperties.csproj");
        var (project, logger) = GetProjectWithRestore(projectPath);

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        Assert.IsEmpty(logger.BuildWarnings);
        Assert.IsTrue(File.Exists(Path.Combine(modPath, @"Assemblies\NoGameProperties.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "version.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "doorstop_config.ini")));
    }

    [TestMethod]
    public void WhenProjectHasNonMatchingGameName_BuildWithWarning()
    {
        var gamePath = MakeGameCopy("2018-net4-v1.0");
        var modPath = Path.Combine(gamePath, @"Mods\WrongGameName");
        Directory.CreateDirectory(modPath);
        TestUtils.CopyDirectory(@"Projects\AmbientGame\WrongGameName", modPath);
        var projectPath = Path.Combine(modPath, @"Sources\WrongGameName.csproj");
        var (project, logger) = GetProjectWithRestore(projectPath);

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        Assert.HasCount(1, logger.BuildWarnings);
        Assert.AreEqual("Ambient game name is 'Unity2018Test', but 'WrongGame' is defined by the project.", logger.BuildWarnings[0].Message);
        Assert.IsTrue(File.Exists(Path.Combine(modPath, @"Assemblies\WrongGameName.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "version.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "doorstop_config.ini")));
    }

    [TestMethod]
    public void WhenProjectHasMatchingGameName_Build()
    {
        var gamePath = MakeGameCopy("2018-net4-v1.0");
        var modPath = Path.Combine(gamePath, @"Mods\WithGameName");
        Directory.CreateDirectory(modPath);
        TestUtils.CopyDirectory(@"Projects\AmbientGame\WithGameName", modPath);
        var projectPath = Path.Combine(modPath, @"Sources\WithGameName.csproj");
        var (project, logger) = GetProjectWithRestore(projectPath);

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        Assert.IsEmpty(logger.BuildWarnings);
        Assert.IsTrue(File.Exists(Path.Combine(modPath, @"Assemblies\WithGameName.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "version.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "doorstop_config.ini")));
    }

    [TestMethod]
    public void WhenProjectHasGameVersion_Build()
    {
        var gamePath = MakeGameCopy("2018-net4-v1.0");
        var modPath = Path.Combine(gamePath, @"Mods\WithGameVersion");
        Directory.CreateDirectory(modPath);
        TestUtils.CopyDirectory(@"Projects\AmbientGame\WithGameVersion", modPath);
        var projectPath = Path.Combine(modPath, @"Sources\WithGameVersion.csproj");
        var (project, logger) = GetProjectWithRestore(projectPath);

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        Assert.IsEmpty(logger.BuildWarnings);
        Assert.IsTrue(File.Exists(Path.Combine(modPath, @"Assemblies\WithGameVersion.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "version.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "doorstop_config.ini")));
    }

    [TestMethod]
    public void WhenProjectHasMultipleGameVersions_ProduceError()
    {
        var gamePath = MakeGameCopy("2018-net4-v1.0");
        var modPath = Path.Combine(gamePath, @"Mods\MultipleGameVersions");
        Directory.CreateDirectory(modPath);
        TestUtils.CopyDirectory(@"Projects\AmbientGame\MultipleGameVersions", modPath);
        var projectPath = Path.Combine(modPath, @"Sources\MultipleGameVersions.csproj");
        var (project, logger) = GetProjectWithRestore(projectPath);

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.HasCount(1, logger.BuildErrors);
        Assert.AreEqual("Building multi-version mod for an ambient game is not supported.", logger.BuildErrors[0].Message);
        Assert.IsEmpty(logger.BuildWarnings);
        Assert.IsFalse(File.Exists(Path.Combine(modPath, @"Assemblies\MultipleGameVersions.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "version.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(gamePath, "doorstop_config.ini")));
    }
}