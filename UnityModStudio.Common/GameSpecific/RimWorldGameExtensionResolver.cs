using System.Collections.Generic;

namespace UnityModStudio.Common.GameSpecific;

public class RimWorldGameExtensionResolver : IGameExtensionResolver
{
    public IReadOnlyCollection<GameExtension> GetGameExtensions(GameInformation gameInformation)
    {
        if (gameInformation.Name == "RimWorld by Ludeon Studios")
            return
            [
                new GameExtension
                {
                    ExtensionName = "Unity Mod Studio for RimWorld",
                    TemplateName = "RimWorld mod (C#)",
                    HasNativeModSupport = true,
                }
            ];

        return [];
    }
}