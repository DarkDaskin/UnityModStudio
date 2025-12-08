using System;

namespace UnityModStudio.Common.Options;

public class GeneralSettings
{
    public AmbientGameSettings AmbientGame { get; set; } = new();
    public Guid? LastSelectedGameId { get; set; }

    public class AmbientGameSettings
    {
        public bool IsResolutionAllowed { get; set; } = true;
        public DoorstopMode DoorstopMode { get; set; } = DoorstopMode.Debugging;
        public bool UseAlternateDoorstopDllName { get; set; }
    }
}