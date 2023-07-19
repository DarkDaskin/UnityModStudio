using Microsoft.Win32;
using System.Collections.Generic;

namespace UnityModStudio.Options;

public sealed class AddGamesFromRegistryViewModel : AddGamesViewModelBase
{
    protected override IEnumerable<GameEntry> FindGames()
    {
        using var uninstallKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
        if (uninstallKey == null)
            yield break;
        
        foreach (var subKeyName in uninstallKey.GetSubKeyNames())
        {
            using var subKey = uninstallKey.OpenSubKey(subKeyName)!;
            var displayName = subKey.GetValue("DisplayName") as string;
            var installLocation = subKey.GetValue("InstallLocation") as string;
            if (!string.IsNullOrEmpty(displayName) && !string.IsNullOrEmpty(installLocation))
                yield return new GameEntry(displayName!, installLocation!);
        }
    }
}