namespace UnityModStudio.Common.Options
{
    public class Game
    {
        // Properties for project configuration (user-editable):
        public string DisplayName { get; set; } = "";
        public string Path { get; set; } = "";
        public string? ModLoaderId { get; set; }

        // Properties for search (auto-resolved):
        public string? GameName { get; set; }

        // Properties for display in game list (auto-resolved):
        public string? GameExecutableFileName { get; set; }
        public string? Architecture { get; set; }
        public string? UnityVersion { get; set; }
        public string? MonoProfile { get; set; }
    }
}