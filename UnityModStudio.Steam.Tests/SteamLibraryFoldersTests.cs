using Gameloop.Vdf;
using UnityModStudio.Common.Tests;

namespace UnityModStudio.Steam.Tests;

[TestClass]
public class SteamLibraryFoldersTests
{
    [TestMethod]
    public void WhenFileIsInvalid_Throw()
    {
        var reader = new StringReader("!@#$%");
        
        Assert.ThrowsExactly<VdfException>(() => new SteamLibraryFolders(reader));
    }

    [TestMethod]
    public void WhenFileIsValid_ReadLibraryFolders()
    {
        using var reader = new StreamReader(GetResourceStream("libraryfolders_Full.vdf"));

        var libraryFolders = new SteamLibraryFolders(reader);
        var rimWorldFolder = libraryFolders.FindFolderForAppId(294100);
        var nonExistentFolder = libraryFolders.FindFolderForAppId(999999);

        Assert.AreEqual(4, libraryFolders.Count);
        Assert.AreEqual(@"C:\Program Files (x86)\Steam", libraryFolders[0].Path);
        Assert.IsNotNull(rimWorldFolder);
        Assert.AreEqual(@"G:\SteamLibrary", rimWorldFolder.Path);
        Assert.IsNull(nonExistentFolder);
    }

    [TestMethod]
    public void WhenLibraryFolderIsValid_ReadAppInfo()
    {
        var libraryDirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(libraryDirectoryPath);

        try
        {
            var steamappsDirectoryPath = Path.Combine(libraryDirectoryPath, "steamapps");
            Directory.CreateDirectory(steamappsDirectoryPath);
            WriteResourceFile("appmanifest_294100.acf", steamappsDirectoryPath);
            var libraryFoldersVdf = GetSyntheticLibraryFoldersVdf(libraryDirectoryPath);

            var libraryFolders = new SteamLibraryFolders(new StringReader(libraryFoldersVdf));
            var rimWorldFolder = libraryFolders.FindFolderForAppId(294100);
            var rimWorldAppInfo = rimWorldFolder?.FindApplication(294100);

            Assert.IsNotNull(rimWorldFolder);
            Assert.IsNotNull(rimWorldAppInfo);
            Assert.AreEqual(294100u, rimWorldAppInfo.AppId);
            Assert.AreEqual("RimWorld", rimWorldAppInfo.Name);
            Assert.AreEqual(Path.Combine(steamappsDirectoryPath, @"common\RimWorld"), rimWorldAppInfo.InstallDirectory);
            Assert.AreEqual(Path.Combine(steamappsDirectoryPath, @"workshop\content\294100"), rimWorldAppInfo.WorkshopDirectory);
        }
        finally
        {
            Directory.Delete(libraryDirectoryPath, true);
        }
        
        static string GetSyntheticLibraryFoldersVdf(string path)
        {
            using var templateReader = new StreamReader(GetResourceStream("libraryfolders_Synthetic.vdf"));
            var template = templateReader.ReadToEnd();
            return template.Replace("<PATH>", path.Replace(@"\", @"\\"));
        }

        static void WriteResourceFile(string fileName, string directoryPath)
        {
            using var inputStream = GetResourceStream(fileName);
            using var outputStream = File.Create(Path.Combine(directoryPath, fileName));
            inputStream.CopyTo(outputStream);
        }
    }

    private static Stream GetResourceStream(string fileName) => TestUtils.GetResourceStream(fileName, typeof(SteamLibraryFoldersTests));
}