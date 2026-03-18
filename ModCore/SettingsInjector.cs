using MelonLoader;
using Minimap.MimicUI.Components;
using UnityEngine;

namespace Minimap.ModCore
{
    /// <summary>
    /// Polls for UIPrefab_GameSettings(Clone) and injects a "Minimap Settings" button into it.
    /// </summary>
    internal static class SettingsInjector
    {
        private static bool _injected = false;

        public static void Reset()
        {
            _injected = false;
        }

        /// <summary>
        /// Call every frame. Injects once when the game settings object appears in the scene.
        /// </summary>
        public static void TryInject()
        {
            if (_injected) return;

            var root = API.UIManagerAPI.GetGameSettingsRoot();
            if (root == null) return;

            var btn = new UIButton("MinimapSettings", root, "Minimap", Core.ToggleSettingsPanel);
            btn.Rect.anchorMin = new Vector2(0f, 0f);
            btn.Rect.anchorMax = new Vector2(0f, 0f);
            btn.Rect.pivot = new Vector2(0f, 0f);
            btn.Rect.anchoredPosition = new Vector2(180f, 5f);
            btn.Rect.sizeDelta = new Vector2(180f, 40f);

            _injected = true;
            MelonLogger.Msg("[MiniMap] Button injected into UIPrefab_GameSettings.");
        }
    }
}
