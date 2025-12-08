using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UnityModStudio.Common.GameSpecific;

public class BepInExGameExtensionResolver : IGameExtensionResolver
{
    private const string ExtensionName = "Unity Mod Studio for BepInEx";

    public IReadOnlyCollection<GameExtension> GetGameExtensions(GameInformation gameInformation)
    {
        var bepInExDirectory = gameInformation.GameDirectory.EnumerateDirectories("BepInEx").SingleOrDefault();
        var coreDirectory = bepInExDirectory?.EnumerateDirectories("core").SingleOrDefault();
        var bepInExAssembly = coreDirectory?.EnumerateFiles("BepInEx.dll").SingleOrDefault();
        if (bepInExAssembly is not null)
        {
            if (Version.TryParse(FileVersionInfo.GetVersionInfo(bepInExAssembly.FullName).FileVersion, out var bepInExVersion))
            {
                switch (bepInExVersion.Major)
                {
                    case 5:
                        return [GetBepInExV5Extension(true)];
                }
            }
        }

        // BepInEx can be installed into any Unity Mono game.
        if (gameInformation.GameAssemblyFiles.Any())
            return [GetBepInExV5Extension(false)];

        return [];
    }

    private static GameExtension GetBepInExV5Extension(bool isInstalled) =>
        new GameExtension
        {
            ExtensionName = ExtensionName,
            TemplateName = "BepInEx 5 mod (C#)",
            ModLoaderId = "BepInEx5",
            IsModLoaderInstalled = isInstalled,
        };
}
