using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace UnityModStudio.Common.Tests;

[TestClass]
public sealed class GameInformationResolverTests
{
    public TestContext TestContext { get; set; } = null!;

    private DirectoryInfo? _scratchDir;
    
    [TestCleanup]
    public void TestCleanup()
    {
        if (_scratchDir?.Exists ?? false)
            _scratchDir.Delete(true);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("(*&#^:<$*&#$(")]
    [DataRow(@"C:\NonExistentPath")]
    public void WhenGamePathIsWrong_ReturnError(string? gamePath)
    {
        var success = GameInformationResolver.TryGetGameInformation(gamePath, out var gameInformation, out var error, out var errorCode);

        Assert.IsFalse(success);
        Assert.IsNull(gameInformation);
        Assert.AreEqual("Game directory does not exist.", error);
        Assert.AreEqual("UMS1001", errorCode);
    }

    [TestMethod]
    public void WhenGameDirectoryIsEmpty_ReturnError()
    {
        CreateScratchDir();
        
        var success = GameInformationResolver.TryGetGameInformation(_scratchDir.FullName, out var gameInformation, out var error, out var errorCode);

        Assert.IsFalse(success);
        Assert.IsNull(gameInformation);
        Assert.AreEqual("Unable to determine game data directory.", error);
        Assert.AreEqual("UMS1002", errorCode);
    }

    [TestMethod]
    public void WhenGameDataDirectoryDoesNotExist_ReturnError()
    {
        CreateScratchDir();
        File.Copy(Path.Combine(SampleGameInfo.DownloadPath, @"567-net20\567-net20.exe"), Path.Combine(_scratchDir.FullName, "567-net20.exe"));

        var success = GameInformationResolver.TryGetGameInformation(_scratchDir.FullName, out var gameInformation, out var error, out var errorCode);

        Assert.IsFalse(success);
        Assert.IsNull(gameInformation);
        Assert.AreEqual("Unable to determine game data directory.", error);
        Assert.AreEqual("UMS1002", errorCode);
    }

    [TestMethod]
    public void WhenMultipleGameDataDirectoriesExist_ReturnError()
    {
        CreateScratchDir();
        TestUtils.CopyDirectory(Path.Combine(SampleGameInfo.DownloadPath, "567-net20"), _scratchDir.FullName);
        TestUtils.CopyDirectory(Path.Combine(SampleGameInfo.DownloadPath, "2017-net46"), _scratchDir.FullName);

        var success = GameInformationResolver.TryGetGameInformation(_scratchDir.FullName, out var gameInformation, out var error, out var errorCode);

        Assert.IsFalse(success);
        Assert.IsNull(gameInformation);
        Assert.AreEqual("Ambiguous game data directory.", error);
        Assert.AreEqual("UMS1003", errorCode);
    }

    [TestMethod]
    public void WhenManagedDirectoryDoesNotExist_ReturnError()
    {
        CreateScratchDir();
        TestUtils.CopyDirectory(Path.Combine(SampleGameInfo.DownloadPath, "567-net20"), _scratchDir.FullName, path => Path.GetFileName(path) != "Managed");

        var success = GameInformationResolver.TryGetGameInformation(_scratchDir.FullName, out var gameInformation, out var error, out var errorCode);

        Assert.IsFalse(success);
        Assert.IsNull(gameInformation);
        Assert.AreEqual("Game managed assembly directory does not exist.", error);
        Assert.AreEqual("UMS1004", errorCode);
    }

    [TestMethod]
    public void WhenManagedDirectoryIsEmpty_ReturnError()
    {
        CreateScratchDir();
        TestUtils.CopyDirectory(Path.Combine(SampleGameInfo.DownloadPath, "567-net20"), _scratchDir.FullName, path => Path.GetFileName(Path.GetDirectoryName(path)) != "Managed");

        var success = GameInformationResolver.TryGetGameInformation(_scratchDir.FullName, out var gameInformation, out var error, out var errorCode);

        Assert.IsFalse(success);
        Assert.IsNull(gameInformation);
        Assert.AreEqual("'mscorlib.dll' is missing.", error);
        Assert.AreEqual("UMS1005", errorCode);
    }

    [TestMethod]
    public void WhenCorLibHasUnknownVersion_ReturnError()
    {
        CreateScratchDir();
        TestUtils.CopyDirectory(Path.Combine(SampleGameInfo.DownloadPath, "567-net20"), _scratchDir.FullName, path => Path.GetFileName(Path.GetDirectoryName(path)) != "Managed");
        File.Copy(Path.Combine(SampleGameInfo.DownloadPath, @"567-net20\567-net20.exe"), Path.Combine(_scratchDir.FullName, @"567-net20_Data\Managed\mscorlib.dll"));

        var success = GameInformationResolver.TryGetGameInformation(_scratchDir.FullName, out var gameInformation, out var error, out var errorCode);

        Assert.IsFalse(success);
        Assert.IsNull(gameInformation);
        Assert.AreEqual("Specified assembly set does not correspond to any known target framework.", error);
        Assert.AreEqual("UMS1006", errorCode);
    }

    [TestMethod]
    public void WhenGameExecutableIsNotExecutable_ReturnError()
    {
        CreateScratchDir();
        TestUtils.CopyDirectory(Path.Combine(SampleGameInfo.DownloadPath, "567-net20"), _scratchDir.FullName, path => Path.GetExtension(path) != ".exe");
        File.WriteAllText(Path.Combine(_scratchDir.FullName, "567-net20.exe"), "");

        var success = GameInformationResolver.TryGetGameInformation(_scratchDir.FullName, out var gameInformation, out var error, out _);

        Assert.IsFalse(success);
        Assert.IsNull(gameInformation);
        Assert.IsNotNull(error);
    }

    // Game types listed here must also be listed as SampleGameType item in GameTest.targets.
    [TestMethod]
    [DataRow("357-net20")]
    [DataRow("357-net20-subset")]
    [DataRow("472-net20")]
    [DataRow("472-net20-subset")]
    [DataRow("567-net20")]
    [DataRow("567-net20-subset")]
    [DataRow("2017-net20")]
    [DataRow("2017-net20-subset")]
    [DataRow("2017-net46")]
    [DataRow("2018-net20")]
    [DataRow("2018-net20-subset")]
    [DataRow("2018-net4-v1.0")]
    [DataRow("2018-netstandard20-v2.0")]
    [DataRow("2022-net4")]
    [DataRow("2022-netstandard21")]
    public void WhenGameIsValid_ReturnGameInformation(string gameType)
    {
        var success = GameInformationResolver.TryGetGameInformation(Path.Combine(SampleGameInfo.DownloadPath, gameType), out var gameInformation, out var error, out var errorCode);

        Assert.IsTrue(success);
        Assert.IsNull(error);
        Assert.IsNull(errorCode);
        Assert.IsNotNull(gameInformation);
        if (gameType[0] is not ('3' or '4'))
            Assert.AreEqual("DefaultCompany", gameInformation.Company);
        if (gameType[..4] is "2018" or "2022")
        {
            Assert.AreEqual(Path.Combine(SampleGameInfo.DownloadPath, $@"{gameType}\{gameInformation.Name}.exe"), gameInformation.GameExecutableFile.FullName);
            Assert.AreEqual(Path.Combine(SampleGameInfo.DownloadPath, $@"{gameType}\{gameInformation.Name}_Data"), gameInformation.GameDataDirectory.FullName);   
        }
        else
        {
            Assert.AreEqual(Path.Combine(SampleGameInfo.DownloadPath, $@"{gameType}\{gameType}.exe"), gameInformation.GameExecutableFile.FullName);
            Assert.AreEqual(Path.Combine(SampleGameInfo.DownloadPath, $@"{gameType}\{gameType}_Data"), gameInformation.GameDataDirectory.FullName);            
        }
        switch (gameType)
        {
            case "357-net20":
                Assert.IsNull(gameInformation.Name);
                Assert.IsNull(gameInformation.Company);
                Assert.AreEqual(Architecture.X86, gameInformation.Architecture);
                Assert.AreEqual("3.5.7f6", gameInformation.UnityVersion);
                Assert.AreEqual("net20", gameInformation.TargetFrameworkMoniker);
                Assert.IsFalse(gameInformation.IsSubsetProfile);
                Assert.AreEqual(1, gameInformation.FrameworkAssemblyFiles.Count);
                Assert.AreEqual(1, gameInformation.GameAssemblyFiles.Count);
                break;
            case "357-net20-subset":
                Assert.IsNull(gameInformation.Name);
                Assert.IsNull(gameInformation.Company);
                Assert.AreEqual(Architecture.X86, gameInformation.Architecture);
                Assert.AreEqual("3.5.7f6", gameInformation.UnityVersion);
                Assert.AreEqual("net20", gameInformation.TargetFrameworkMoniker);
                Assert.IsTrue(gameInformation.IsSubsetProfile);
                Assert.AreEqual(1, gameInformation.FrameworkAssemblyFiles.Count);
                Assert.AreEqual(1, gameInformation.GameAssemblyFiles.Count);
                break;
            case "472-net20":
                Assert.IsNull(gameInformation.Name);
                Assert.IsNull(gameInformation.Company);
                Assert.AreEqual(Architecture.X86, gameInformation.Architecture);
                Assert.AreEqual("4.7.2f1", gameInformation.UnityVersion);
                Assert.AreEqual("net35", gameInformation.TargetFrameworkMoniker);
                Assert.IsFalse(gameInformation.IsSubsetProfile);
                Assert.AreEqual(6, gameInformation.FrameworkAssemblyFiles.Count);
                Assert.AreEqual(4, gameInformation.GameAssemblyFiles.Count);
                break;
            case "472-net20-subset":
                Assert.IsNull(gameInformation.Name);
                Assert.IsNull(gameInformation.Company);
                Assert.AreEqual(Architecture.X86, gameInformation.Architecture);
                Assert.AreEqual("4.7.2f1", gameInformation.UnityVersion);
                Assert.AreEqual("net35", gameInformation.TargetFrameworkMoniker);
                Assert.IsTrue(gameInformation.IsSubsetProfile);
                Assert.AreEqual(3, gameInformation.FrameworkAssemblyFiles.Count);
                Assert.AreEqual(3, gameInformation.GameAssemblyFiles.Count);
                break;
            case "567-net20":
                Assert.AreEqual("Unity5Test", gameInformation.Name);
                Assert.AreEqual(Architecture.X86, gameInformation.Architecture);
                Assert.AreEqual("5.6.7f1", gameInformation.UnityVersion);
                Assert.AreEqual("net35", gameInformation.TargetFrameworkMoniker);
                Assert.IsFalse(gameInformation.IsSubsetProfile);
                Assert.AreEqual(6, gameInformation.FrameworkAssemblyFiles.Count);
                Assert.AreEqual(5, gameInformation.GameAssemblyFiles.Count);
                break;
            case "567-net20-subset":
                Assert.AreEqual("Unity5Test", gameInformation.Name);
                Assert.AreEqual(Architecture.X86, gameInformation.Architecture);
                Assert.AreEqual("5.6.7f1", gameInformation.UnityVersion);
                Assert.AreEqual("net35", gameInformation.TargetFrameworkMoniker);
                Assert.IsTrue(gameInformation.IsSubsetProfile);
                Assert.AreEqual(3, gameInformation.FrameworkAssemblyFiles.Count);
                Assert.AreEqual(4, gameInformation.GameAssemblyFiles.Count);
                break;
            case "2017-net20":
                Assert.AreEqual("Unity2017Test", gameInformation.Name);
                Assert.AreEqual(Architecture.X64, gameInformation.Architecture);
                Assert.AreEqual("2017.1.0f3", gameInformation.UnityVersion);
                Assert.AreEqual("net35", gameInformation.TargetFrameworkMoniker);
                Assert.IsFalse(gameInformation.IsSubsetProfile);
                Assert.AreEqual(6, gameInformation.FrameworkAssemblyFiles.Count);
                Assert.AreEqual(6, gameInformation.GameAssemblyFiles.Count);
                break;
            case "2017-net20-subset":
                Assert.AreEqual("Unity2017Test", gameInformation.Name);
                Assert.AreEqual(Architecture.X64, gameInformation.Architecture);
                Assert.AreEqual("2017.1.0f3", gameInformation.UnityVersion);
                Assert.AreEqual("net35", gameInformation.TargetFrameworkMoniker);
                Assert.IsTrue(gameInformation.IsSubsetProfile);
                Assert.AreEqual(3, gameInformation.FrameworkAssemblyFiles.Count);
                Assert.AreEqual(5, gameInformation.GameAssemblyFiles.Count);
                break;
            case "2017-net46":
                Assert.AreEqual("Unity2017Test", gameInformation.Name);
                Assert.AreEqual(Architecture.X64, gameInformation.Architecture);
                Assert.AreEqual("2017.1.0f3", gameInformation.UnityVersion);
                Assert.AreEqual("net46", gameInformation.TargetFrameworkMoniker);
                Assert.IsFalse(gameInformation.IsSubsetProfile);
                Assert.AreEqual(6, gameInformation.FrameworkAssemblyFiles.Count);
                Assert.AreEqual(6, gameInformation.GameAssemblyFiles.Count);
                break;
            case "2018-net20":
                Assert.AreEqual("Unity2018Test", gameInformation.Name);
                Assert.AreEqual(Architecture.X64, gameInformation.Architecture);
                Assert.AreEqual("2018.4.36f1", gameInformation.UnityVersion);
                Assert.AreEqual("net35", gameInformation.TargetFrameworkMoniker);
                Assert.IsFalse(gameInformation.IsSubsetProfile);
                Assert.AreEqual(6, gameInformation.FrameworkAssemblyFiles.Count);
                Assert.AreEqual(70, gameInformation.GameAssemblyFiles.Count);
                break;
            case "2018-net20-subset":
                Assert.AreEqual("Unity2018Test", gameInformation.Name);
                Assert.AreEqual(Architecture.X64, gameInformation.Architecture);
                Assert.AreEqual("2018.4.36f1", gameInformation.UnityVersion);
                Assert.AreEqual("net35", gameInformation.TargetFrameworkMoniker);
                Assert.IsTrue(gameInformation.IsSubsetProfile);
                Assert.AreEqual(3, gameInformation.FrameworkAssemblyFiles.Count);
                Assert.AreEqual(69, gameInformation.GameAssemblyFiles.Count);
                break;
            case "2018-net4-v1.0":
                Assert.AreEqual("Unity2018Test", gameInformation.Name);
                Assert.AreEqual(Architecture.X64, gameInformation.Architecture);
                Assert.AreEqual("2018.4.36f1", gameInformation.UnityVersion);
                Assert.AreEqual("net472", gameInformation.TargetFrameworkMoniker);
                Assert.IsFalse(gameInformation.IsSubsetProfile);
                Assert.AreEqual(6, gameInformation.FrameworkAssemblyFiles.Count);
                Assert.AreEqual(71, gameInformation.GameAssemblyFiles.Count);
                break;
            case "2018-netstandard20-v2.0":
                Assert.AreEqual("Unity2018Test", gameInformation.Name);
                Assert.AreEqual(Architecture.X64, gameInformation.Architecture);
                Assert.AreEqual("2018.4.36f1", gameInformation.UnityVersion);
                Assert.AreEqual("netstandard2.0", gameInformation.TargetFrameworkMoniker);
                Assert.IsFalse(gameInformation.IsSubsetProfile);
                Assert.AreEqual(22, gameInformation.FrameworkAssemblyFiles.Count);
                Assert.AreEqual(70, gameInformation.GameAssemblyFiles.Count);
                break;
            case "2022-net4":
                Assert.AreEqual("Unity2022Test", gameInformation.Name);
                Assert.AreEqual(Architecture.X64, gameInformation.Architecture);
                Assert.AreEqual("2022.3.22f1", gameInformation.UnityVersion);
                Assert.AreEqual("netstandard2.1", gameInformation.TargetFrameworkMoniker); // Both builds contain netstandard.dll
                Assert.IsFalse(gameInformation.IsSubsetProfile);
                Assert.AreEqual(21, gameInformation.FrameworkAssemblyFiles.Count);
                Assert.AreEqual(88, gameInformation.GameAssemblyFiles.Count);
                break;
            case "2022-netstandard21":
                Assert.AreEqual("Unity2022Test", gameInformation.Name);
                Assert.AreEqual(Architecture.X64, gameInformation.Architecture);
                Assert.AreEqual("2022.3.22f1", gameInformation.UnityVersion);
                Assert.AreEqual("netstandard2.1", gameInformation.TargetFrameworkMoniker);
                Assert.IsFalse(gameInformation.IsSubsetProfile);
                Assert.AreEqual(21, gameInformation.FrameworkAssemblyFiles.Count);
                Assert.AreEqual(88, gameInformation.GameAssemblyFiles.Count);
                break;
        }
    }

    [MemberNotNull(nameof(_scratchDir))]
    private void CreateScratchDir()
    {
        _scratchDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        _scratchDir.Create();
    }
}