using Mono.Cecil;
using UnityModStudio.Common.Options;

namespace UnityModStudio.RimWorld.Build.Tests;

[TestClass]
public class ModReferenceTests : BuildTestsBase
{
    private Game _game15 = null!;
    private Game _game16 = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _game15 = new Game { Path = MakeGameCopy("1.5.4409"), Version = "1.5" };
        _game16 = new Game { Path = MakeGameCopy("1.6.4633"), Version = "1.6" };
        ResolveGameProperties(_game15);
        ResolveGameProperties(_game16);
        SetupGameRegistry(_game15, _game16);

        AssemblyFixture.BinaryLogger.SetSuffix("_BaseMod_WithoutLoadFolders");
        var (project, logger) = GetProjectWithRestore(@"Projects\ModReference\BaseMod\WithoutLoadFolders\Sources\BaseModWithoutLoadFolders.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        //Assert.IsEmpty(logger.BuildWarnings); // Some warnings are expected because RimRef has some assembly version inconsistencies.
        Assert.IsTrue(File.Exists(Path.Combine(_game15.Path, @"Mods\BaseModWithoutLoadFolders\1.5\Assemblies\BaseModWithoutLoadFolders.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(_game15.Path, @"Mods\BaseModWithoutLoadFolders\1.6\Assemblies\BaseModWithoutLoadFolders.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(_game15.Path, @"Mods\BaseModWithoutLoadFolders\About\About.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(_game16.Path, @"Mods\BaseModWithoutLoadFolders\1.5\Assemblies\BaseModWithoutLoadFolders.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(_game16.Path, @"Mods\BaseModWithoutLoadFolders\1.6\Assemblies\BaseModWithoutLoadFolders.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(_game16.Path, @"Mods\BaseModWithoutLoadFolders\About\About.xml")));

        AssemblyFixture.BinaryLogger.SetSuffix("_BaseMod_WithLoadFolders");
        (project, logger) = GetProjectWithRestore(@"Projects\ModReference\BaseMod\WithLoadFolders\Sources\BaseModWithLoadFolders.csproj");

        success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        //Assert.IsEmpty(logger.BuildWarnings); // Some warnings are expected because RimRef has some assembly version inconsistencies.
        Assert.IsTrue(File.Exists(Path.Combine(_game15.Path, @"Mods\BaseModWithLoadFolders\Old\Assemblies\BaseModWithLoadFolders.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(_game15.Path, @"Mods\BaseModWithLoadFolders\New\Assemblies\BaseModWithLoadFolders.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(_game15.Path, @"Mods\BaseModWithLoadFolders\About\About.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(_game16.Path, @"Mods\BaseModWithLoadFolders\Old\Assemblies\BaseModWithLoadFolders.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(_game16.Path, @"Mods\BaseModWithLoadFolders\New\Assemblies\BaseModWithLoadFolders.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(_game16.Path, @"Mods\BaseModWithLoadFolders\About\About.xml")));

        AssemblyFixture.BinaryLogger.SetSuffix("");
    }

    [TestMethod]
    public void WhenAssetsAreAtTopLevelAndExplicitReferenceToBaseModWithoutLoadFoldersIsProvided_BuildAndDeploy()
    {
        var (project, logger) = GetProjectWithRestore(@"Projects\ModReference\AssetsAtTopLevel\ExplicitReference\Sources\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        //Assert.IsEmpty(logger.BuildWarnings); // Some warnings are expected because RimRef has some assembly version inconsistencies.
        VerifyModAssemblyExistsAndReferencesCorrectVersion(Path.Combine(_game15.Path, @"Mods\Mod\1.5\Assemblies\Mod.dll"), "1.5");
        VerifyModAssemblyExistsAndReferencesCorrectVersion(Path.Combine(_game15.Path, @"Mods\Mod\1.6\Assemblies\Mod.dll"), "1.6");
        Assert.IsTrue(File.Exists(Path.Combine(_game15.Path, @"Mods\Mod\About\About.xml")));
        VerifyModAssemblyExistsAndReferencesCorrectVersion(Path.Combine(_game16.Path, @"Mods\Mod\1.5\Assemblies\Mod.dll"), "1.5");
        VerifyModAssemblyExistsAndReferencesCorrectVersion(Path.Combine(_game16.Path, @"Mods\Mod\1.6\Assemblies\Mod.dll"), "1.6");
        Assert.IsTrue(File.Exists(Path.Combine(_game16.Path, @"Mods\Mod\About\About.xml")));
    }

    [TestMethod]
    public void WhenAssetsAreAtTopLevelAndExplicitReferenceToBaseModWithLoadFoldersIsProvided_BuildAndDeploy()
    {
        var (project, logger) = GetProjectWithRestore(@"Projects\ModReference\AssetsAtTopLevel\ExplicitReferenceWithLoadFolders\Sources\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        //Assert.IsEmpty(logger.BuildWarnings); // Some warnings are expected because RimRef has some assembly version inconsistencies.
        VerifyModAssemblyExistsAndReferencesCorrectVersion(Path.Combine(_game15.Path, @"Mods\Mod\1.5\Assemblies\Mod.dll"), "1.5");
        VerifyModAssemblyExistsAndReferencesCorrectVersion(Path.Combine(_game15.Path, @"Mods\Mod\1.6\Assemblies\Mod.dll"), "1.6");
        Assert.IsTrue(File.Exists(Path.Combine(_game15.Path, @"Mods\Mod\About\About.xml")));
        VerifyModAssemblyExistsAndReferencesCorrectVersion(Path.Combine(_game16.Path, @"Mods\Mod\1.5\Assemblies\Mod.dll"), "1.5");
        VerifyModAssemblyExistsAndReferencesCorrectVersion(Path.Combine(_game16.Path, @"Mods\Mod\1.6\Assemblies\Mod.dll"), "1.6");
        Assert.IsTrue(File.Exists(Path.Combine(_game16.Path, @"Mods\Mod\About\About.xml")));
    }

    [TestMethod]
    public void WhenAssetsAreAtTopLevelAndImplicitReferenceToBaseModIsProvided_BuildAndDeploy()
    {
        var (project, logger) = GetProjectWithRestore(@"Projects\ModReference\AssetsAtTopLevel\ImplicitReference\Sources\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        //Assert.IsEmpty(logger.BuildWarnings); // Some warnings are expected because RimRef has some assembly version inconsistencies.
        VerifyModAssemblyExistsAndReferencesCorrectVersion(Path.Combine(_game15.Path, @"Mods\Mod\1.5\Assemblies\Mod.dll"), "1.5");
        VerifyModAssemblyExistsAndReferencesCorrectVersion(Path.Combine(_game15.Path, @"Mods\Mod\1.6\Assemblies\Mod.dll"), "1.6");
        Assert.IsTrue(File.Exists(Path.Combine(_game15.Path, @"Mods\Mod\About\About.xml")));
        VerifyModAssemblyExistsAndReferencesCorrectVersion(Path.Combine(_game16.Path, @"Mods\Mod\1.5\Assemblies\Mod.dll"), "1.5");
        VerifyModAssemblyExistsAndReferencesCorrectVersion(Path.Combine(_game16.Path, @"Mods\Mod\1.6\Assemblies\Mod.dll"), "1.6");
        Assert.IsTrue(File.Exists(Path.Combine(_game16.Path, @"Mods\Mod\About\About.xml")));
    }

    [TestMethod]
    public void WhenAssetsAreAtTopLevelAndReferencesToNonExistingModsAreProvided_ProduceErrors()
    {
        var (project, logger) = GetProjectWithRestore(@"Projects\ModReference\AssetsAtTopLevel\NonExistingMods\Sources\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.HasCount(2, logger.BuildErrors);
        Assert.AreEqual("UMSRW0007", logger.BuildErrors[0].Code);
        Assert.AreEqual("Could not resolve mod reference 'NonExisting.SteamMod'.", logger.BuildErrors[0].Message);
        Assert.AreEqual("UMSRW0007", logger.BuildErrors[1].Code);
        Assert.AreEqual("Could not resolve mod reference 'NonExisting.Mod'.", logger.BuildErrors[1].Message);
    }

    [TestMethod]
    public void WhenProjectIsAtTopLevelAndImplicitReferenceToBaseModIsProvided_BuildAndDeploy()
    {
        var (project, logger) = GetProjectWithRestore(@"Projects\ModReference\ProjectAtTopLevel\ImplicitReference\Mod.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.IsEmpty(logger.BuildErrors);
        //Assert.IsEmpty(logger.BuildWarnings); // Some warnings are expected because RimRef has some assembly version inconsistencies.
        VerifyModAssemblyExistsAndReferencesCorrectVersion(Path.Combine(_game15.Path, @"Mods\Mod\1.5\Assemblies\Mod.dll"), "1.5");
        VerifyModAssemblyExistsAndReferencesCorrectVersion(Path.Combine(_game15.Path, @"Mods\Mod\1.6\Assemblies\Mod.dll"), "1.6");
        Assert.IsTrue(File.Exists(Path.Combine(_game15.Path, @"Mods\Mod\About\About.xml")));
        VerifyModAssemblyExistsAndReferencesCorrectVersion(Path.Combine(_game16.Path, @"Mods\Mod\1.5\Assemblies\Mod.dll"), "1.5");
        VerifyModAssemblyExistsAndReferencesCorrectVersion(Path.Combine(_game16.Path, @"Mods\Mod\1.6\Assemblies\Mod.dll"), "1.6");
        Assert.IsTrue(File.Exists(Path.Combine(_game16.Path, @"Mods\Mod\About\About.xml")));
    }

    private static void VerifyModAssemblyExistsAndReferencesCorrectVersion(string modAssemblyPath, string expectedVersion)
    {
        Assert.IsTrue(File.Exists(modAssemblyPath), $"Mod assembly not found at '{modAssemblyPath}'.");
        VerifyModAssemblyVersionConstant(modAssemblyPath, expectedVersion);
    }

    private static void VerifyModAssemblyVersionConstant(string modAssemblyPath, string value)
    {
        using var modAssembly = AssemblyDefinition.ReadAssembly(modAssemblyPath);
        var startupType = modAssembly.MainModule.GetType("MyMod.Startup");
        Assert.IsNotNull(startupType, "Startup type not found in mod assembly.");
        var constantField = startupType.Fields.Single(field => field.Name == "BaseVersion");
        Assert.IsNotNull(constantField, "BaseVersion constant not found.");
        Assert.AreEqual(value, (string)constantField.Constant, "BaseVersion constant has wrong value.");
    }
}