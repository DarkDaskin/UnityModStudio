using System.Text.Json;
using UnityModStudio.Common.Options;
using UnityModStudio.Common.Tests;
using UnityModStudio.RimWorld.Common.Options;

namespace UnityModStudio.RimWorld.Common.Tests;

[TestClass]
public sealed class RimWorldSettingsManagerTests : StoreTestsBase
{
    [ClassInitialize]
    public static void ClassInitialize(TestContext context) => ResourceType = typeof(RimWorldSettingsManagerTests);

    [TestMethod]
    public void WhenCreated_UseDefaults()
    {
        using IRimWorldSettingsManager settingsManager = new RimWorldSettingsManager(StorePath);

        Assert.IsNotNull(settingsManager.Settings);
        Assert.IsNotNull(settingsManager.Settings.ProjectDefaults);
        Assert.IsNull(settingsManager.Settings.ProjectDefaults.ModPackageIdPrefix);
        Assert.IsNull(settingsManager.Settings.ProjectDefaults.ModAuthor);
        Assert.AreEqual(true, settingsManager.Settings.ProjectDefaults.UseHarmony);
        Assert.AreEqual(ProjectLayout.AssetsAtTopLevel, settingsManager.Settings.ProjectDefaults.ProjectLayout);
        Assert.IsFalse(settingsManager.WatchForChanges);
    }

    [TestMethod]
    public async Task WhenLoadedFromEmptyFile_UseDefaults()
    {
        using IRimWorldSettingsManager settingsManager = new RimWorldSettingsManager(StorePath);

        await settingsManager.LoadAsync();

        Assert.IsNotNull(settingsManager.Settings);
        Assert.IsNotNull(settingsManager.Settings.ProjectDefaults);
        Assert.IsNull(settingsManager.Settings.ProjectDefaults.ModPackageIdPrefix);
        Assert.IsNull(settingsManager.Settings.ProjectDefaults.ModAuthor);
        Assert.AreEqual(true, settingsManager.Settings.ProjectDefaults.UseHarmony);
        Assert.AreEqual(ProjectLayout.AssetsAtTopLevel, settingsManager.Settings.ProjectDefaults.ProjectLayout);
        Assert.IsFalse(settingsManager.WatchForChanges);
    }

    [TestMethod]
    public async Task WhenLoadedFromNonExistingFile_UseDefaults()
    {
        using IRimWorldSettingsManager settingsManager = new RimWorldSettingsManager(StorePath);
        File.Delete(StorePath);

        await settingsManager.LoadAsync();

        Assert.IsNotNull(settingsManager.Settings);
        Assert.IsNotNull(settingsManager.Settings.ProjectDefaults);
        Assert.IsNull(settingsManager.Settings.ProjectDefaults.ModPackageIdPrefix);
        Assert.IsNull(settingsManager.Settings.ProjectDefaults.ModAuthor);
        Assert.AreEqual(true, settingsManager.Settings.ProjectDefaults.UseHarmony);
        Assert.AreEqual(ProjectLayout.AssetsAtTopLevel, settingsManager.Settings.ProjectDefaults.ProjectLayout);
        Assert.IsFalse(settingsManager.WatchForChanges);
    }

    [TestMethod]
    public async Task WhenLoadedFromMalformedFile_Throw()
    {
        using IRimWorldSettingsManager settingsManager = new RimWorldSettingsManager(StorePath);
        await File.WriteAllTextAsync(StorePath, "!@$%^&*()");

        await Assert.ThrowsExactlyAsync<JsonException>(() => settingsManager.LoadAsync());
    }

    [TestMethod]
    public async Task WhenLoadedFromValidFile_LoadSettings()
    {
        using IRimWorldSettingsManager settingsManager = new RimWorldSettingsManager(StorePath);
        SetStoreFile("RimWorldSettings.json");

        await settingsManager.LoadAsync();

        Assert.IsNotNull(settingsManager.Settings);
        Assert.IsNotNull(settingsManager.Settings.ProjectDefaults);
        Assert.AreEqual("com.example", settingsManager.Settings.ProjectDefaults.ModPackageIdPrefix);
        Assert.AreEqual("User", settingsManager.Settings.ProjectDefaults.ModAuthor);
        Assert.AreEqual(false, settingsManager.Settings.ProjectDefaults.UseHarmony);
        Assert.AreEqual(ProjectLayout.ProjectAtTopLevel, settingsManager.Settings.ProjectDefaults.ProjectLayout);
    }

    [TestMethod]
    public async Task WhenSaving_WriteJson()
    {
        using IRimWorldSettingsManager settingsManager = new RimWorldSettingsManager(StorePath);

        settingsManager.Settings.ProjectDefaults.ModPackageIdPrefix = "com.example";
        settingsManager.Settings.ProjectDefaults.ModAuthor = "User";
        settingsManager.Settings.ProjectDefaults.UseHarmony = false;
        settingsManager.Settings.ProjectDefaults.ProjectLayout = ProjectLayout.ProjectAtTopLevel;
        await settingsManager.SaveAsync();

        VerifyStoreEquals("RimWorldSettings.json");
    }

    [TestMethod]
    public void WhenEnablingWatchWithMissingGameRegistry_Succeed()
    {
        using IGeneralSettingsManager settingsManager = new GeneralSettingsManager(StorePath);
        File.Delete(StorePath);

        settingsManager.WatchForChanges = true;

        Assert.IsTrue(settingsManager.WatchForChanges);
    }

    [TestMethod]
    public async Task WhenEnablingWatch_ReloadOnExternalChange()
    {
        using IGeneralSettingsManager settingsManager = new GeneralSettingsManager(StorePath);
        using IGeneralSettingsManager settingsManager2 = new GeneralSettingsManager(StorePath);

        settingsManager.WatchForChanges = true;
        settingsManager2.Settings.AmbientGame.DoorstopMode = DoorstopMode.Disabled;
        await settingsManager2.SaveAsync();
        await Task.Delay(200);

        Assert.AreEqual(DoorstopMode.Disabled, settingsManager.Settings.AmbientGame.DoorstopMode);
    }

}
