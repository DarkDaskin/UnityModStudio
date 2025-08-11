using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Options;

[ComVisible(true), Guid("2F33CFB0-8B1A-45B8-B3D6-44F81A8447C7")]
public class GeneralPage : DialogPage
{
    public const string CategoryName = GameRegistryPage.CategoryName;
    public const string PageName = "General";
    public const string AmbientGameCategory = "Ambient games";

    private IComponentModel ComponentModel => (IComponentModel)GetService(typeof(SComponentModel))!;
    private IGeneralSettingsManager GeneralSettingsManager => ComponentModel.GetService<IGeneralSettingsManager>();

    [Category(AmbientGameCategory)]
    [DisplayName("Allow ambient game resolution")]
    [Description("When enabled, game resolution will fall back to directories above the project directory. " +
                 "This allows building mods for games that are not registered in the game registry.")]
    [DefaultValue(true)]
    public bool IsAmbientGameResolutionAllowed { get; set; }

    [Category(AmbientGameCategory)]
    [DisplayName("Unity Doorstop mode")]
    [Description("Specifies the Unity Doorstop mode for ambient games. " +
                 "This setting is used when the game is not registered in the game registry and " +
                 "the ambient game resolution is allowed.")]
    [DefaultValue(DoorstopMode.Debugging)]
    public DoorstopMode AmbientGamesDoorstopMode { get; set; }

    [Category(AmbientGameCategory)]
    [DisplayName("Use alternate Unity Doorstop DLL name")]
    [Description("When enabled, the Unity Doorstop DLL will be named 'version.dll' instead of 'winhttp.dll' " +
                 "for ambient games.")]
    [DefaultValue(false)]
    public bool UseAlternateDoorstopDllNameForAmbientGames { get; set; }

    public override void LoadSettingsFromStorage()
    {
        ThreadHelper.JoinableTaskFactory.Run(GeneralSettingsManager.LoadSafeAsync);

        var settings = GeneralSettingsManager.Settings;
        IsAmbientGameResolutionAllowed = settings.AmbientGame.IsResolutionAllowed;
        AmbientGamesDoorstopMode = settings.AmbientGame.DoorstopMode;
        UseAlternateDoorstopDllNameForAmbientGames = settings.AmbientGame.UseAlternateDoorstopDllName;
    }

    public override void SaveSettingsToStorage()
    {
        var settings = GeneralSettingsManager.Settings;
        settings.AmbientGame.IsResolutionAllowed = IsAmbientGameResolutionAllowed;
        settings.AmbientGame.DoorstopMode = AmbientGamesDoorstopMode;
        settings.AmbientGame.UseAlternateDoorstopDllName = UseAlternateDoorstopDllNameForAmbientGames;

        ThreadHelper.JoinableTaskFactory.Run(GeneralSettingsManager.SaveSafeAsync);
    }
}