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
        var bepInExUnityMonoAssembly = coreDirectory?.EnumerateFiles("BepInEx.Unity.Mono.dll").SingleOrDefault();
        if (bepInExUnityMonoAssembly is not null)
        {
            if (Version.TryParse(FileVersionInfo.GetVersionInfo(bepInExUnityMonoAssembly.FullName).FileVersion, out var bepInExVersion))
            {
                switch (bepInExVersion.Major)
                {
                    case 6:
                        return [GetBepInExV6UnityMonoExtension(true)];
                }
            }
        }

        // BepInEx can be installed into any Unity Mono game.
        if (gameInformation.GameAssemblyFiles.Any())
            return [GetBepInExV5Extension(false), GetBepInExV6UnityMonoExtension(false)];

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

    private static GameExtension GetBepInExV6UnityMonoExtension(bool isInstalled) =>
        new GameExtension
        {
            ExtensionName = ExtensionName,
            TemplateName = "BepInEx 6 Unity Mono mod (C#)",
            ModLoaderId = "BepInEx6",
            IsModLoaderInstalled = isInstalled,
        };
}
