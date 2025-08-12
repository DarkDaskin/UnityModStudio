using System.Text.Json;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Common.Tests;

[TestClass]
public class GeneralSettingsManagerTests : StoreTestsBase
{
    [TestMethod]
    public void WhenCreated_UseDefaults()
    {
        IGeneralSettingsManager generalSettingsManager = new GeneralSettingsManager(StorePath);

        Assert.AreEqual(true, generalSettingsManager.Settings.AmbientGame.IsResolutionAllowed);
        Assert.AreEqual(DoorstopMode.Debugging, generalSettingsManager.Settings.AmbientGame.DoorstopMode);
        Assert.AreEqual(false, generalSettingsManager.Settings.AmbientGame.UseAlternateDoorstopDllName);
        Assert.IsFalse(generalSettingsManager.WatchForChanges);
    }

    [TestMethod]
    public async Task WhenLoadedFromEmptyFile_UseDefaults()
    {
        IGeneralSettingsManager generalSettingsManager = new GeneralSettingsManager(StorePath);

        await generalSettingsManager.LoadAsync();

        Assert.AreEqual(true, generalSettingsManager.Settings.AmbientGame.IsResolutionAllowed);
        Assert.AreEqual(DoorstopMode.Debugging, generalSettingsManager.Settings.AmbientGame.DoorstopMode);
        Assert.AreEqual(false, generalSettingsManager.Settings.AmbientGame.UseAlternateDoorstopDllName);
        Assert.IsFalse(generalSettingsManager.WatchForChanges);
    }

    [TestMethod]
    public async Task WhenLoadedFromNonExistingFile_UseDefaults()
    {
        IGeneralSettingsManager generalSettingsManager = new GeneralSettingsManager(StorePath);
        File.Delete(StorePath);

        await generalSettingsManager.LoadAsync();

        Assert.AreEqual(true, generalSettingsManager.Settings.AmbientGame.IsResolutionAllowed);
        Assert.AreEqual(DoorstopMode.Debugging, generalSettingsManager.Settings.AmbientGame.DoorstopMode);
        Assert.AreEqual(false, generalSettingsManager.Settings.AmbientGame.UseAlternateDoorstopDllName);
        Assert.IsFalse(generalSettingsManager.WatchForChanges);
    }

    [TestMethod]
    public async Task WhenLoadedFromMalformedFile_Throw()
    {
        IGeneralSettingsManager generalSettingsManager = new GeneralSettingsManager(StorePath);
        await File.WriteAllTextAsync(StorePath, "!@$%^&*()");

        await Assert.ThrowsExactlyAsync<JsonException>(() => generalSettingsManager.LoadAsync());
    }

    [TestMethod]
    public async Task WhenLoadedFromValidFile_LoadSettings()
    {
        IGeneralSettingsManager generalSettingsManager = new GeneralSettingsManager(StorePath);
        SetStoreFile("GeneralSettings.json");

        await generalSettingsManager.LoadAsync();

        Assert.AreEqual(false, generalSettingsManager.Settings.AmbientGame.IsResolutionAllowed);
        Assert.AreEqual(DoorstopMode.DebuggingAndModLoading, generalSettingsManager.Settings.AmbientGame.DoorstopMode);
        Assert.AreEqual(true, generalSettingsManager.Settings.AmbientGame.UseAlternateDoorstopDllName);
    }

    [TestMethod]
    public async Task WhenSaving_WriteJson()
    {
        IGeneralSettingsManager generalSettingsManager = new GeneralSettingsManager(StorePath);

        generalSettingsManager.Settings.AmbientGame.IsResolutionAllowed = false;
        generalSettingsManager.Settings.AmbientGame.DoorstopMode = DoorstopMode.DebuggingAndModLoading;
        generalSettingsManager.Settings.AmbientGame.UseAlternateDoorstopDllName = true;
        await generalSettingsManager.SaveAsync();

        VerifyStoreEquals("GeneralSettings.json");
    }

    [TestMethod]
    public void WhenEnablingWatchWithMissingGameRegistry_Succeed()
    {
        IGeneralSettingsManager generalSettingsManager = new GeneralSettingsManager(StorePath);
        File.Delete(StorePath);

        generalSettingsManager.WatchForChanges = true;

        Assert.IsTrue(generalSettingsManager.WatchForChanges);
    }

    [TestMethod]
    public async Task WhenEnablingWatch_ReloadOnExternalChange()
    {
        IGeneralSettingsManager generalSettingsManager = new GeneralSettingsManager(StorePath);
        IGeneralSettingsManager generalSettingsManager2 = new GeneralSettingsManager(StorePath);

        generalSettingsManager.WatchForChanges = true;
        generalSettingsManager2.Settings.AmbientGame.DoorstopMode = DoorstopMode.Disabled;
        await generalSettingsManager2.SaveAsync();
        await Task.Delay(200);

        Assert.AreEqual(DoorstopMode.Disabled, generalSettingsManager.Settings.AmbientGame.DoorstopMode);
    }
}