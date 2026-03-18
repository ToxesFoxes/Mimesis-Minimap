using MelonLoader;

namespace Minimap.ModCore
{
    public enum MinimapPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight,
        Manual,
    }

    public static class Settings
    {
        private static MelonPreferences_Category? _category;

        public static MelonPreferences_Entry<string>? ToggleKey { get; private set; }
        public static MelonPreferences_Entry<bool>? DungeonModeAuto { get; private set; }
        public static MelonPreferences_Entry<bool>? CompassVisible { get; private set; }
        public static MelonPreferences_Entry<int>? MapPosition { get; private set; }
        public static MelonPreferences_Entry<float>? MapZoom { get; private set; }
        public static MelonPreferences_Entry<float>? MapSize { get; private set; }
        public static MelonPreferences_Entry<float>? MapPosX { get; private set; }
        public static MelonPreferences_Entry<float>? MapPosY { get; private set; }
        public static MelonPreferences_Entry<string>? SettingsKey { get; private set; }
        public static MelonPreferences_Entry<int>? SettingsModifier { get; private set; }

        public static readonly string[] ModifierNames =
            { "None", "Left Shift", "Right Shift", "Left Ctrl", "Right Ctrl", "Left Alt", "Right Alt" };
        public static readonly string[] ModifierPaths =
            { "", "<Keyboard>/leftShift", "<Keyboard>/rightShift", "<Keyboard>/leftCtrl", "<Keyboard>/rightCtrl", "<Keyboard>/leftAlt", "<Keyboard>/rightAlt" };

        public static MinimapPosition Position => (MinimapPosition)(MapPosition?.Value ?? (int)MinimapPosition.BottomRight);

        public static void Initialize()
        {
            _category = MelonPreferences.CreateCategory("Minimap");
            ToggleKey = _category.CreateEntry("ToggleKey", "F4", "Toggle Key");
            DungeonModeAuto = _category.CreateEntry("DungeonModeAuto", true, "Auto Dungeon Mode");
            CompassVisible = _category.CreateEntry("CompassVisible", true, "Compass Visible");
            MapPosition = _category.CreateEntry("MapPosition", (int)MinimapPosition.BottomRight, "Map Position");
            MapZoom = _category.CreateEntry("MapZoom", 33f, "Map Zoom");
            MapSize = _category.CreateEntry("MapSize", 256f, "Map Size");
            MapPosX = _category.CreateEntry("MapPosX", 0f, "Map Position X");
            MapPosY = _category.CreateEntry("MapPosY", 0f, "Map Position Y");
            SettingsKey = _category.CreateEntry("SettingsKey", "M", "Settings Open Key");
            SettingsModifier = _category.CreateEntry("SettingsModifier", 1, "Settings Modifier"); // 1 = Left Shift
        }

        public static string GetToggleBinding()
        {
            var keyName = ToggleKey?.Value ?? "F4";
            return $"<Keyboard>/{keyName.ToLower()}";
        }

        public static string GetSettingsKeyPath()
        {
            var keyName = SettingsKey?.Value ?? "M";
            return $"<Keyboard>/{keyName.ToLower()}";
        }
    }
}
