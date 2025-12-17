using UnityModStudio.RimWorld.Common.Options;

namespace UnityModStudio.RimWorld.ProjectWizard.Tests;

[TestClass]
public class ProjectLayoutManagerTests
{
    private string _scratchDirPath = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _scratchDirPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_scratchDirPath);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        Directory.Delete(_scratchDirPath, true);
    }

    [TestMethod]
    public void WhenProjectIsAtTopLevelAndProjectIsInSolutionDirectory_KeepLayout()
    {
        TestUtils.CopyDirectory(@"TestFiles\ProjectAtRoot", _scratchDirPath);
        var solutionFilePath = Path.Combine(_scratchDirPath, "MyMod.sln");
        var projectFilePath = Path.Combine(_scratchDirPath, "MyMod.csproj");

        ProjectLayoutManager.ApplyLayout(ProjectLayout.ProjectAtTopLevel, projectFilePath, solutionFilePath, 
            out var newProjectFilePath, out var newStartupCsPath, out var newAboutXmlPath);

        Assert.AreEqual(projectFilePath, newProjectFilePath);
        Assert.AreEqual(Path.Combine(_scratchDirPath, "Startup.cs"), newStartupCsPath);
        Assert.AreEqual(Path.Combine(_scratchDirPath, @"Assets\About\About.xml"), newAboutXmlPath);
        Assert.IsTrue(File.Exists(solutionFilePath));
        Assert.IsTrue(File.Exists(projectFilePath));
        Assert.IsTrue(File.Exists(newStartupCsPath));
        Assert.IsTrue(File.Exists(newAboutXmlPath));
        Assert.IsTrue(File.Exists(Path.Combine(_scratchDirPath, @"Properties\launchSettings.json")));
    }

    [TestMethod]
    public void WhenProjectIsAtTopLevelAndProjectIsInSolutionSubDirectory_KeepLayout()
    {
        TestUtils.CopyDirectory(@"TestFiles\ProjectAtSubDirectory", _scratchDirPath);
        var solutionFilePath = Path.Combine(_scratchDirPath, "MyMod.sln");
        var projectDirPath = Path.Combine(_scratchDirPath, "MyMod");
        var projectFilePath = Path.Combine(projectDirPath, "MyMod.csproj");

        ProjectLayoutManager.ApplyLayout(ProjectLayout.ProjectAtTopLevel, projectFilePath, solutionFilePath,
            out var newProjectFilePath, out var newStartupCsPath, out var newAboutXmlPath);

        Assert.AreEqual(projectFilePath, newProjectFilePath);
        Assert.AreEqual(Path.Combine(projectDirPath, "Startup.cs"), newStartupCsPath);
        Assert.AreEqual(Path.Combine(projectDirPath, @"Assets\About\About.xml"), newAboutXmlPath);
        Assert.IsTrue(File.Exists(solutionFilePath));
        Assert.IsTrue(File.Exists(projectFilePath));
        Assert.IsTrue(File.Exists(newStartupCsPath));
        Assert.IsTrue(File.Exists(newAboutXmlPath));
        Assert.IsTrue(File.Exists(Path.Combine(projectDirPath, @"Properties\launchSettings.json")));
    }

    [TestMethod]
    public void WhenAssetsAreAtTopLevelAndProjectIsInSolutionDirectory_ChangeLayout()
    {
        TestUtils.CopyDirectory(@"TestFiles\ProjectAtRoot", _scratchDirPath);
        var solutionFilePath = Path.Combine(_scratchDirPath, "MyMod.sln");
        var projectFilePath = Path.Combine(_scratchDirPath, "MyMod.csproj");

        ProjectLayoutManager.ApplyLayout(ProjectLayout.AssetsAtTopLevel, projectFilePath, solutionFilePath, 
            out var newProjectFilePath, out var newStartupCsPath, out var newAboutXmlPath);

        Assert.AreEqual(Path.Combine(_scratchDirPath, @"Sources\MyMod.csproj"), newProjectFilePath);
        Assert.AreEqual(Path.Combine(_scratchDirPath, @"Sources\Startup.cs"), newStartupCsPath);
        Assert.AreEqual(Path.Combine(_scratchDirPath, @"About\About.xml"), newAboutXmlPath);
        Assert.IsTrue(File.Exists(solutionFilePath));
        Assert.IsTrue(File.Exists(newProjectFilePath));
        Assert.IsTrue(File.Exists(newStartupCsPath));
        Assert.IsTrue(File.Exists(newAboutXmlPath));
        Assert.IsTrue(File.Exists(Path.Combine(_scratchDirPath, @"Sources\Properties\launchSettings.json")));
        Assert.IsFalse(Directory.Exists(Path.Combine(_scratchDirPath, "Assets")));
    }

    [TestMethod]
    public void WhenAssetsAreAtTopLevelAndSolutionPathIsEmpty_AssumeProjectIsInSolutionDirectoryAndChangeLayout()
    {
        TestUtils.CopyDirectory(@"TestFiles\ProjectAtRoot", _scratchDirPath);
        var solutionFilePath = "";
        var projectFilePath = Path.Combine(_scratchDirPath, "MyMod.csproj");

        ProjectLayoutManager.ApplyLayout(ProjectLayout.AssetsAtTopLevel, projectFilePath, solutionFilePath, 
            out var newProjectFilePath, out var newStartupCsPath, out var newAboutXmlPath);

        Assert.AreEqual(Path.Combine(_scratchDirPath, @"Sources\MyMod.csproj"), newProjectFilePath);
        Assert.AreEqual(Path.Combine(_scratchDirPath, @"Sources\Startup.cs"), newStartupCsPath);
        Assert.AreEqual(Path.Combine(_scratchDirPath, @"About\About.xml"), newAboutXmlPath);
        Assert.IsTrue(File.Exists(newProjectFilePath));
        Assert.IsTrue(File.Exists(newStartupCsPath));
        Assert.IsTrue(File.Exists(newAboutXmlPath));
        Assert.IsTrue(File.Exists(Path.Combine(_scratchDirPath, @"Sources\Properties\launchSettings.json")));
        Assert.IsFalse(Directory.Exists(Path.Combine(_scratchDirPath, "Assets")));
    }

    [TestMethod]
    public void WhenAssetsAreAtTopLevelAndProjectIsInSolutionSubDirectory_ChangeLayout()
    {
        TestUtils.CopyDirectory(@"TestFiles\ProjectAtSubDirectory", _scratchDirPath);
        var solutionFilePath = Path.Combine(_scratchDirPath, "MyMod.sln");
        var projectFilePath = Path.Combine(_scratchDirPath, @"MyMod\MyMod.csproj");

        ProjectLayoutManager.ApplyLayout(ProjectLayout.AssetsAtTopLevel, projectFilePath, solutionFilePath, 
            out var newProjectFilePath, out var newStartupCsPath, out var newAboutXmlPath);

        Assert.AreEqual(Path.Combine(_scratchDirPath, @"Sources\MyMod\MyMod.csproj"), newProjectFilePath);
        Assert.AreEqual(Path.Combine(_scratchDirPath, @"Sources\MyMod\Startup.cs"), newStartupCsPath);
        Assert.AreEqual(Path.Combine(_scratchDirPath, @"About\About.xml"), newAboutXmlPath);
        Assert.IsTrue(File.Exists(solutionFilePath));
        Assert.IsTrue(File.Exists(newProjectFilePath));
        Assert.IsTrue(File.Exists(newStartupCsPath));
        Assert.IsTrue(File.Exists(newAboutXmlPath));
        Assert.IsTrue(File.Exists(Path.Combine(_scratchDirPath, @"Sources\MyMod\Properties\launchSettings.json")));
        Assert.IsFalse(Directory.Exists(Path.Combine(_scratchDirPath, "Assets")));
    }
}