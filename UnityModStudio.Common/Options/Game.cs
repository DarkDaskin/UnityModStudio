using System;

namespace UnityModStudio.Common.Options
{
    public class Game
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Properties for project configuration (user-editable):
        public string DisplayName { get; set; } = "";
        public string Path { get; set; } = "";
        public string? ModsPath { get; set; }
        public string? Version { get; set; }
        public ModDeploymentMode ModDeploymentMode { get; set; } = ModDeploymentMode.Copy;
        public bool DeploySourceCode { get; set; }
        public DoorstopMode DoorstopMode { get; set; } = DoorstopMode.Debugging;
        public bool UseAlternateDoorstopDllName { get; set; }

        // Properties for search (auto-resolved):
        public string? GameName { get; set; }

        // Properties for display in game list (auto-resolved):
        public string? GameExecutableFileName { get; set; }
        public string? Architecture { get; set; }
        public string? UnityVersion { get; set; }
        public string? MonoProfile { get; set; }
    }
}