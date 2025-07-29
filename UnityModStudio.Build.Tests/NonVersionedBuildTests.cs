using Microsoft.Build.Execution;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Build.Tests;

[TestClass]
public sealed class NonVersionedBuildTests : BuildTestsBase
{
    [TestMethod]
    public void WhenGameHasDefaultSettings_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("2018-net4") };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"NonVersioned\NonVersioned.dll")));
        Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "NonVersioned"), false));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=NonVersioned\NonVersioned.dll"));
    }

    [TestMethod]
    public void WhenGameHasModLoadingEnabled_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("2018-net4"), DoorstopMode = DoorstopMode.DebuggingAndModLoading };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"NonVersioned\NonVersioned.dll")));
        Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "NonVersioned"), false));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsTrue(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=NonVersioned\NonVersioned.dll"));
    }

    [TestMethod]
    public void WhenGameUsesAlternateDoorstopDllName_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("2018-net4"), UseAlternateDoorstopDllName = true };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"NonVersioned\NonVersioned.dll")));
        Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "NonVersioned"), false));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "version.dll")));
        var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=NonVersioned\NonVersioned.dll"));
    }

    [TestMethod]
    public void WhenGameHasDoorstopDisabled_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("2018-net4"), DoorstopMode = DoorstopMode.Disabled };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"NonVersioned\NonVersioned.dll")));
        Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "NonVersioned"), false));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "doorstop_config.ini")));
    }

    [TestMethod]
    public void WhenGameHasDeploymentDisabled_Build()
    {
        var game = new Game { Path = MakeGameCopy("2018-net4"), DoorstopMode = DoorstopMode.Disabled, ModDeploymentMode = ModDeploymentMode.Disabled };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, @"NonVersioned\NonVersioned.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "doorstop_config.ini")));
    }

    [TestMethod]
    public void WhenGameHasDeploymentModeLink_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("2018-net4"), ModDeploymentMode = ModDeploymentMode.Link };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"NonVersioned\NonVersioned.dll")));
        var linkTarget = File.ResolveLinkTarget(Path.Combine(game.Path, "NonVersioned"), false);
        Assert.IsNotNull(linkTarget);
        Assert.AreEqual(Path.GetFullPath($@"Projects\Correct\NonVersioned\bin\{Configuration}\net46\"), linkTarget.FullName);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=NonVersioned\NonVersioned.dll"));
    }

    [TestMethod]
    public void WhenGameHasDeploySourceCode_BuildAndDeployWithWarning()
    {
        var game = new Game { Path = MakeGameCopy("2018-net4"), DeploySourceCode = true };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(1, logger.BuildWarnings.Count);
        Assert.AreEqual("UMS0004", logger.BuildWarnings[0].Code);
        Assert.AreEqual("No source code files found.", logger.BuildWarnings[0].Message);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"NonVersioned\NonVersioned.dll")));
        Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "NonVersioned"), false));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=NonVersioned\NonVersioned.dll"));
    }

    [TestMethod]
    public void WhenProjectHasModSourcePath_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("2018-net4") };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersionedWithModSourcePath\Sources\NonVersionedWithModSourcePath.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"NonVersionedWithModSourcePath\Assemblies\NonVersionedWithModSourcePath.dll")));
        Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "NonVersionedWithModSourcePath"), false));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=NonVersionedWithModSourcePath\Assemblies\NonVersionedWithModSourcePath.dll"));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"NonVersionedWithModSourcePath\Defs\Defs.xml")));
        Assert.IsFalse(Directory.Exists(Path.Combine(game.Path, @"NonVersionedWithModSourcePath\Sources")));
    }

    [TestMethod]
    public void WhenProjectHasModSourcePathAndGameHasDeploySourceCode_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("2018-net4"), DeploySourceCode = true };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersionedWithModSourcePath\Sources\NonVersionedWithModSourcePath.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"NonVersionedWithModSourcePath\Assemblies\NonVersionedWithModSourcePath.dll")));
        Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "NonVersionedWithModSourcePath"), false));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=NonVersionedWithModSourcePath\Assemblies\NonVersionedWithModSourcePath.dll"));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"NonVersionedWithModSourcePath\Defs\Defs.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"NonVersionedWithModSourcePath\Sources\NonVersionedWithModSourcePath.csproj")));
    }

    [TestMethod]
    public void WhenProjectHasModSourcePathAndGameHasDeploymentModeLink_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("2018-net4"), ModDeploymentMode = ModDeploymentMode.Link };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersionedWithModSourcePath\Sources\NonVersionedWithModSourcePath.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"NonVersionedWithModSourcePath\Assemblies\NonVersionedWithModSourcePath.dll")));
        var linkTarget = File.ResolveLinkTarget(Path.Combine(game.Path, "NonVersionedWithModSourcePath"), false);
        Assert.IsNotNull(linkTarget);
        Assert.AreEqual(Path.GetFullPath(@"Projects\Correct\NonVersionedWithModSourcePath\"), linkTarget.FullName);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=NonVersionedWithModSourcePath\Assemblies\NonVersionedWithModSourcePath.dll"));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"NonVersionedWithModSourcePath\Defs\Defs.xml")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"NonVersionedWithModSourcePath\Sources\NonVersionedWithModSourcePath.csproj")));
    }

    [TestMethod]
    public void WhenGameIsBuiltTwice_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("2018-net4") };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        const string projectPath = @"Projects\Correct\NonVersioned\NonVersioned.csproj";
        var (project, logger) = GetProjectWithRestore(projectPath);

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);
        Assert.IsTrue(success);

        // Project must be reloaded, so resolved data files are imported.
        AssemblyFixture.BinaryLogger.SetSuffix("_2");
        project = ProjectInstance.FromFile(projectPath, ProjectOptions);
        success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"NonVersioned\NonVersioned.dll")));
        Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "NonVersioned"), false));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=NonVersioned\NonVersioned.dll"));
    }

    [TestMethod]
    public void WhenGameIsBuiltAgainWithUseAlternateDoorstopDllName_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("2018-net4"), UseAlternateDoorstopDllName = false };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        const string projectPath = @"Projects\Correct\NonVersioned\NonVersioned.csproj";
        var (project, logger) = GetProjectWithRestore(projectPath);

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);
        Assert.IsTrue(success);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"NonVersioned\NonVersioned.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));

        // Project must be reloaded, so resolved data files are imported.
        game.UseAlternateDoorstopDllName = true;
        SetupGameRegistry(game);
        AssemblyFixture.BinaryLogger.SetSuffix("_2");
        project = ProjectInstance.FromFile(projectPath, ProjectOptions);
        success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"NonVersioned\NonVersioned.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "version.dll")));
    }

    [TestMethod]
    public void WhenGameIsBuiltAgainWithDeploymentModeChangedToLink_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("2018-net4"), ModDeploymentMode = ModDeploymentMode.Copy };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        const string projectPath = @"Projects\Correct\NonVersioned\NonVersioned.csproj";
        var (project, logger) = GetProjectWithRestore(projectPath);

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);
        Assert.IsTrue(success);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"NonVersioned\NonVersioned.dll")));
        Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "NonVersioned"), false));

        // Project must be reloaded, so resolved data files are imported.
        game.ModDeploymentMode = ModDeploymentMode.Link;
        SetupGameRegistry(game);
        AssemblyFixture.BinaryLogger.SetSuffix("_2");
        project = ProjectInstance.FromFile(projectPath, ProjectOptions);
        success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"NonVersioned\NonVersioned.dll")));
        var linkTarget = File.ResolveLinkTarget(Path.Combine(game.Path, "NonVersioned"), false);
        Assert.IsNotNull(linkTarget);
        Assert.AreEqual(Path.GetFullPath($@"Projects\Correct\NonVersioned\bin\{Configuration}\net46\"), linkTarget.FullName);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=NonVersioned\NonVersioned.dll"));
    }

    [TestMethod]
    public void WhenGameIsBuiltAgainWithDeploymentModeChangedToCopy_BuildAndDeploy()
    {
        var game = new Game { Path = MakeGameCopy("2018-net4"), ModDeploymentMode = ModDeploymentMode.Link };
        ResolveGameProperties(game);
        SetupGameRegistry(game);
        const string projectPath = @"Projects\Correct\NonVersioned\NonVersioned.csproj";
        var (project, logger) = GetProjectWithRestore(projectPath);

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);
        Assert.IsTrue(success);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"NonVersioned\NonVersioned.dll")));
        var linkTarget = File.ResolveLinkTarget(Path.Combine(game.Path, "NonVersioned"), false);
        Assert.IsNotNull(linkTarget);
        Assert.AreEqual(Path.GetFullPath($@"Projects\Correct\NonVersioned\bin\{Configuration}\net46\"), linkTarget.FullName);

        // Project must be reloaded, so resolved data files are imported.
        game.ModDeploymentMode = ModDeploymentMode.Copy;
        SetupGameRegistry(game);
        AssemblyFixture.BinaryLogger.SetSuffix("_2");
        project = ProjectInstance.FromFile(projectPath, ProjectOptions);
        success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsTrue(success);
        Assert.AreEqual(0, logger.BuildErrors.Count);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, @"NonVersioned\NonVersioned.dll")));
        Assert.IsNull(File.ResolveLinkTarget(Path.Combine(game.Path, "NonVersioned"), false));
        Assert.IsTrue(File.Exists(Path.Combine(game.Path, "winhttp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(game.Path, "version.dll")));
        var doorstopConfigPath = Path.Combine(game.Path, "doorstop_config.ini");
        Assert.IsTrue(File.Exists(doorstopConfigPath));
        Assert.IsFalse(File.ReadAllLines(doorstopConfigPath).Contains(@"target_assembly=NonVersioned\NonVersioned.dll"));
    }
}
