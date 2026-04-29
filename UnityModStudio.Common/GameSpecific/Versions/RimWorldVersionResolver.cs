using System;
using System.Diagnostics;
using System.Linq;

namespace UnityModStudio.Common.GameSpecific.Versions;

public class RimWorldVersionResolver : IGameVersionResolver
{
    public string? GetGameVersion(GameInformation gameInformation)
    {
        if (gameInformation.Name != "RimWorld by Ludeon Studios")
            return null;

        var primaryAssemblyFile = gameInformation.GameAssemblyFiles.FirstOrDefault(file => file.Name == "Assembly-CSharp.dll");
        if (primaryAssemblyFile == null)
            return null;

        return Version.TryParse(FileVersionInfo.GetVersionInfo(primaryAssemblyFile.FullName).FileVersion, out var version) ? version.ToString(2) : null;
    }
}