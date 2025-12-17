using System.Text.Json;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Common.Tests;

[TestClass]
public class GeneralSettingsManagerTests : StoreTestsBase
{
    [TestMethod]
    public void WhenCreated_UseDefaults()
    {
        using IGeneralSettingsManager settingsManager = new GeneralSettingsManager(StorePath);

        Assert.IsNotNull(settingsManager.Settings);
        Assert.IsNotNull(settingsManager.Settings.AmbientGame);
        Assert.AreEqual(true, settingsManager.Settings.AmbientGame.IsResolutionAllowed);
        Assert.AreEqual(DoorstopMode.Debugging, settingsManager.Settings.AmbientGame.DoorstopMode);
        Assert.AreEqual(false, settingsManager.Settings.AmbientGame.UseAlternateDoorstopDllName);
        Assert.IsNull(settingsManager.Settings.LastSelectedGameId);
        Assert.IsFalse(settingsManager.WatchForChanges);
    }

    [TestMethod]
    public async Task WhenLoadedFromEmptyFile_UseDefaults()
    {
        using IGeneralSettingsManager settingsManager = new GeneralSettingsManager(StorePath);

        await settingsManager.LoadAsync();

        Assert.IsNotNull(settingsManager.Settings);
        Assert.IsNotNull(settingsManager.Settings.AmbientGame);
        Assert.AreEqual(true, settingsManager.Settings.AmbientGame.IsResolutionAllowed);
        Assert.AreEqual(DoorstopMode.Debugging, settingsManager.Settings.AmbientGame.DoorstopMode);
        Assert.AreEqual(false, settingsManager.Settings.AmbientGame.UseAlternateDoorstopDllName);
        Assert.IsNull(settingsManager.Settings.LastSelectedGameId);
        Assert.IsFalse(settingsManager.WatchForChanges);
    }

    [TestMethod]
    public async Task WhenLoadedFromNonExistingFile_UseDefaults()
    {
        using IGeneralSettingsManager settingsManager = new GeneralSettingsManager(StorePath);
        File.Delete(StorePath);

        await settingsManager.LoadAsync();

        Assert.IsNotNull(settingsManager.Settings);
        Assert.IsNotNull(settingsManager.Settings.AmbientGame);
        Assert.AreEqual(true, settingsManager.Settings.AmbientGame.IsResolutionAllowed);
        Assert.AreEqual(DoorstopMode.Debugging, settingsManager.Settings.AmbientGame.DoorstopMode);
        Assert.AreEqual(false, settingsManager.Settings.AmbientGame.UseAlternateDoorstopDllName);
        Assert.IsNull(settingsManager.Settings.LastSelectedGameId);
        Assert.IsFalse(settingsManager.WatchForChanges);
    }

    [TestMethod]
    public async Task WhenLoadedFromMalformedFile_Throw()
    {
        using IGeneralSettingsManager settingsManager = new GeneralSettingsManager(StorePath);
        await File.WriteAllTextAsync(StorePath, "!@$%^&*()");

        await Assert.ThrowsExactlyAsync<JsonException>(() => settingsManager.LoadAsync());
    }

    [TestMethod]
    public async Task WhenLoadedFromValidFile_LoadSettings()
    {
        using IGeneralSettingsManager settingsManager = new GeneralSettingsManager(StorePath);
        SetStoreFile("GeneralSettings.json");

        await settingsManager.LoadAsync();

        Assert.IsNotNull(settingsManager.Settings);
        Assert.IsNotNull(settingsManager.Settings.AmbientGame);
        Assert.AreEqual(false, settingsManager.Settings.AmbientGame.IsResolutionAllowed);
        Assert.AreEqual(DoorstopMode.DebuggingAndModLoading, settingsManager.Settings.AmbientGame.DoorstopMode);
        Assert.AreEqual(true, settingsManager.Settings.AmbientGame.UseAlternateDoorstopDllName);
        Assert.AreEqual(new Guid("b875ba73-84e8-4a51-a305-20edfa5d58f6"), settingsManager.Settings.LastSelectedGameId);
    }

    [TestMethod]
    public async Task WhenSaving_WriteJson()
    {
        using IGeneralSettingsManager settingsManager = new GeneralSettingsManager(StorePath);

        settingsManager.Settings.AmbientGame.IsResolutionAllowed = false;
        settingsManager.Settings.AmbientGame.DoorstopMode = DoorstopMode.DebuggingAndModLoading;
        settingsManager.Settings.AmbientGame.UseAlternateDoorstopDllName = true;
        settingsManager.Settings.LastSelectedGameId = new Guid("b875ba73-84e8-4a51-a305-20edfa5d58f6");
        await settingsManager.SaveAsync();

        VerifyStoreEquals("GeneralSettings.json");
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