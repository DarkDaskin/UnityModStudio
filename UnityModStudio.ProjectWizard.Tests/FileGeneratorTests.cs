using System.Collections.Immutable;
using System.IO;
using System.Text.Json.Nodes;
using System.Xml;
using System.Xml.Linq;
using UnityModStudio.Common;
using UnityModStudio.Common.Options;

namespace UnityModStudio.ProjectWizard.Tests;

[TestClass]
public class FileGeneratorTests
{
    [TestMethod]
    public async Task WhenUpdateJsonFileAsyncInvokedOnValidFile_UpdateJsonFile()
    {
        var path = Path.GetTempFileName();
        File.Copy(@"TestFiles\JsonObject_Initial.json", path, true);
        var action = new Action<JsonObject>(root =>
        {
            root["newProperty"] = "newValue";
            root["qux"] = 34;
        });

        await FileGenerator.UpdateJsonFileAsync(path, action);

        using var initialStream = OpenReadAndDeleteOnClose(path);
        using var updatedStream = File.OpenRead(@"TestFiles\JsonObject_Updated.json");
        Assert.IsTrue(Utils.AreStreamsEqual(initialStream, updatedStream));
    }

    [TestMethod]
    public async Task WhenUpdateJsonFileAsyncInvokedOnInalidFile_Throw()
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, "42");

        try
        {
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => FileGenerator.UpdateJsonFileAsync(path, _ => { }));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void WhenUpdateLaunchSettingsInvokedWithVersions_AppendProfiles()
    {
        var root = new JsonObject
        {
            ["$schema"] = "https://json.schemastore.org/launchsettings",
            ["profiles"] = new JsonObject
            {
                ["ExistingProfile"] = new JsonObject
                {
                    ["commandName"] = "ExistingCommand"
                }
            }
        };

        FileGenerator.UpdateLaunchSettings(root,new Dictionary<string, string>
        {
            ["Version1"] = "1.0",
            ["Version2"] = "2.0"
        });

        var expectedRoot = new JsonObject
        {
            ["$schema"] = "https://json.schemastore.org/launchsettings",
            ["profiles"] = new JsonObject
            {
                ["ExistingProfile"] = new JsonObject
                {
                    ["commandName"] = "ExistingCommand"
                },
                ["Version1"] = new JsonObject
                {
                    ["commandName"] = "Executable",
                    ["gameVersion"] = "1.0"
                },
                ["Version2"] = new JsonObject
                {
                    ["commandName"] = "Executable",
                    ["gameVersion"] = "2.0"
                }
            }
        };
        Assert.AreEqual(expectedRoot.ToJsonString(), root.ToJsonString());
    }

    [TestMethod]
    public void WhenUpdateLaunchSettingsInvokedWithNoVersions_AppendProfile()
    {
        var root = new JsonObject
        {
            ["$schema"] = "https://json.schemastore.org/launchsettings",
            ["profiles"] = new JsonObject
            {
                ["ExistingProfile"] = new JsonObject
                {
                    ["commandName"] = "ExistingCommand"
                }
            }
        };

        FileGenerator.UpdateLaunchSettings(root, ImmutableDictionary<string, string>.Empty);

        var expectedRoot = new JsonObject
        {
            ["$schema"] = "https://json.schemastore.org/launchsettings",
            ["profiles"] = new JsonObject
            {
                ["ExistingProfile"] = new JsonObject
                {
                    ["commandName"] = "ExistingCommand"
                },
                ["TestGame"] = new JsonObject
                {
                    ["commandName"] = "Executable"
                }
            }
        };
        Assert.AreEqual(expectedRoot.ToJsonString(), root.ToJsonString());
    }

    [TestMethod]
    public async Task WhenUpdateXmlFileAsyncInvokedOnValidFile_UpdateXmlFile()
    {
        var path = Path.GetTempFileName();
        File.Copy(@"TestFiles\XmlFile_Initial.xml", path, true);
        var action = new Action<XDocument>(document =>
        {
            var existingElement = document.Root!.Element("ExistingElement")!;
            existingElement.SetAttributeValue("Attribute", "NewValue");
            existingElement.Value = "NewValue2";
            existingElement.AddAfterSelf("\n  ", new XElement("NewElement", new XAttribute("Attribute", "NewValue3"), "NewValue4"));
        });

        await FileGenerator.UpdateXmlFileAsync(path, action);

        using var initialStream = OpenReadAndDeleteOnClose(path);
        using var updatedStream = File.OpenRead(@"TestFiles\XmlFile_Updated.xml");
        Assert.IsTrue(Utils.AreStreamsEqual(initialStream, updatedStream));
    }

    [TestMethod]
    public async Task WhenUpdateXmlFileAsyncInvokedOnInalidFile_Throw()
    {
        var path = Path.GetTempFileName();

        try
        {
            await Assert.ThrowsExactlyAsync<XmlException>(() => FileGenerator.UpdateXmlFileAsync(path, _ => { }));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void WhenUpdateProjectInvokedWithNoGames_Throw()
    {
        var document = XDocument.Load(@"TestFiles\Project_Initial_SingleTargetFramework.xml", LoadOptions.PreserveWhitespace);

        Assert.ThrowsExactly<InvalidOperationException>(() => FileGenerator.UpdateProject(document, []));
    }

    [TestMethod]
    public void WhenUpdateProjectInvokedWithSingleGameWithoutVersion_UpdateProject()
    {
        var document = XDocument.Load(@"TestFiles\Project_Initial_SingleTargetFramework.xml", LoadOptions.PreserveWhitespace);
        var game = new Game { Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0") };
        ResolveGameProperties(game);

        FileGenerator.UpdateProject(document, [game]);

        Assert.AreEqual(File.ReadAllText(@"TestFiles\Project_SingleGameNoVersion.xml"), document.ToString());
    }

    [TestMethod]
    public void WhenUpdateProjectInvokedWithSingleGameWithVersion_UpdateProject()
    {
        var document = XDocument.Load(@"TestFiles\Project_Initial_SingleTargetFramework.xml", LoadOptions.PreserveWhitespace);
        var game = new Game { Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"), Version = "1.0" };
        ResolveGameProperties(game);

        FileGenerator.UpdateProject(document, [game]);

        Assert.AreEqual(File.ReadAllText(@"TestFiles\Project_SingleGameWithVersion.xml"), document.ToString());
    }

    [TestMethod]
    public void WhenUpdateProjectInvokedWithMultipleGames_UpdateProject()
    {
        var document = XDocument.Load(@"TestFiles\Project_Initial_SingleTargetFramework.xml", LoadOptions.PreserveWhitespace);
        var game = new Game { Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"), Version = "1.0" };
        var game2 = new Game { Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.1"), Version = "1.1" };
        ResolveGameProperties(game);
        ResolveGameProperties(game2);

        FileGenerator.UpdateProject(document, [game, game2]);

        Assert.AreEqual(File.ReadAllText(@"TestFiles\Project_MultipleGames.xml"), document.ToString());
    }

    [TestMethod]
    public void WhenUpdateProjectInvokedWithMultipleTargetFrameworks_UpdateProject()
    {
        var document = XDocument.Load(@"TestFiles\Project_Initial_MultipleTargetFrameworks.xml", LoadOptions.PreserveWhitespace);
        var game = new Game { Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"), Version = "1.0" };
        var game2 = new Game { Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.1"), Version = "1.1" };
        var game3 = new Game { Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-netstandard20-v2.0"), Version = "2.0" };
        ResolveGameProperties(game);
        ResolveGameProperties(game2);
        ResolveGameProperties(game3);

        FileGenerator.UpdateProject(document, [game, game2, game3]);

        Assert.AreEqual(File.ReadAllText(@"TestFiles\Project_MultipleTargetFrameworks.xml"), document.ToString());
    }

    private static FileStream OpenReadAndDeleteOnClose(string path) => new FileStream(path, FileMode.Open, FileAccess.Read,
        FileShare.ReadWrite | FileShare.Delete, 4096, FileOptions.DeleteOnClose);

    private static void ResolveGameProperties(Game game)
    {
        var success = GameInformationResolver.TryGetGameInformation(game.Path, out var gameInformation, out _, out _);
        Assert.IsTrue(success);
        Assert.IsNotNull(gameInformation);

        if (string.IsNullOrEmpty(game.DisplayName) && !string.IsNullOrEmpty(gameInformation.Name))
            game.DisplayName = gameInformation.Name!;
        game.GameName = gameInformation.Name;
        game.TargetFrameworkMoniker = gameInformation.TargetFrameworkMoniker;
    }
}