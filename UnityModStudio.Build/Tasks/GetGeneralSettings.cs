using Microsoft.Build.Framework;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Build.Tasks;

public class GetGeneralSettings : StoreTaskBase<IGeneralSettingsManager>
{

    [Output]
    public bool IsAmbientGameResolutionAllowed { get; private set; }

    [Output]
    public string? AmbientGamesDoorstopMode { get; private set; }

    [Output]
    public bool UseAlternateDoorstopDllNameForAmbientGames { get; private set; }

    public override bool Execute()
    {
        if (!base.Execute())
            return false;

        IsAmbientGameResolutionAllowed = Store.Settings.AmbientGame.IsResolutionAllowed;
        AmbientGamesDoorstopMode = Store.Settings.AmbientGame.DoorstopMode.ToString();
        UseAlternateDoorstopDllNameForAmbientGames = Store.Settings.AmbientGame.UseAlternateDoorstopDllName;
        return true;
    }
    
    protected override string StoreName => "general settings";

    protected override IGeneralSettingsManager CreateStore(string storePath) => new GeneralSettingsManager(storePath);
}