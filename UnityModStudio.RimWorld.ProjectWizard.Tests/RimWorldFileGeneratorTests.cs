using System.Text;
using System.Xml.Linq;
using UnityModStudio.Common;

namespace UnityModStudio.RimWorld.ProjectWizard.Tests;

[TestClass]
public class RimWorldFileGeneratorTests
{
    [TestMethod]
    public void WhenUpdateProjectInvokedWithoutHarmony_RemoveHarmonyPackageReferences()
    {
        var document = XDocument.Load(@"TestFiles\Project_Initial.xml", LoadOptions.PreserveWhitespace);

        RimWorldFileGenerator.UpdateProject(document, useHarmony: false);

        Assert.AreEqual(File.ReadAllText(@"TestFiles\Project_WithoutHarmony.xml"), document.ToString());
    }

    [TestMethod]
    public void WhenUpdateProjectInvokedWithHarmony_KeepHarmonyPackageReferences()
    {
        var document = XDocument.Load(@"TestFiles\Project_Initial.xml", LoadOptions.PreserveWhitespace);

        RimWorldFileGenerator.UpdateProject(document, useHarmony: true);

        Assert.AreEqual(File.ReadAllText(@"TestFiles\Project_WithHarmony.xml"), document.ToString());
    }

    [TestMethod]
    public void WhenUpdateMetadataInvokedWithoutHarmony_RemoveHarmonyReference()
    {
        var document = XDocument.Load(@"TestFiles\About_Initial.xml", LoadOptions.PreserveWhitespace);

        RimWorldFileGenerator.UpdateMetadata(document, "Author.MyMod", "Author", "My mod",
            "This is test mod.", ["1.5", "1.6"], false);

        Assert.AreEqual(File.ReadAllText(@"TestFiles\About_WithoutHarmony.xml"), ToStringFull(document));
    }


    [TestMethod]
    public void WhenUpdateMetadataInvokedWithHarmony_KeepHarmonyReference()
    {
        var document = XDocument.Load(@"TestFiles\About_Initial.xml", LoadOptions.PreserveWhitespace);

        RimWorldFileGenerator.UpdateMetadata(document, "Author.MyMod", "Author", "My mod",
            "This is test mod.", ["1.5", "1.6"], true);

        Assert.AreEqual(File.ReadAllText(@"TestFiles\About_WithHarmony.xml"), ToStringFull(document));
    }

    [TestMethod]
    public void WhenUpdatePreviewImageInvoked_DrawModName()
    {
        var newImagePath = Path.GetTempFileName();
        File.Copy(@"TestFiles\Preview_Initial.png", newImagePath, true);

        RimWorldFileGenerator.UpdatePreviewImage(newImagePath, "My Awesome Mod");

        var newImageFile = new FileInfo(newImagePath);
        using var newImageStream = newImageFile.OpenRead();
        var expectedImageFile = new FileInfo(@"TestFiles\Preview_Updated.png");
        using var expectedImageStream = expectedImageFile.OpenRead();
        Assert.IsTrue(newImageFile.Exists);
        Assert.IsGreaterThanOrEqualTo(6000, newImageFile.Length);
        // When running on CI, the generated image may differ slightly due to different graphics rendering implementations.
        //Assert.AreEqual(expectedImageFile.Length, newImageFile.Length);
        //Assert.IsTrue(Utils.AreStreamsEqual(expectedImageStream, newImageStream));
    }

    private static string ToStringFull(XDocument document)
    {
        var writer = new Utf8StringWriter();
        document.Save(writer);
        return writer.ToString();
    }
}