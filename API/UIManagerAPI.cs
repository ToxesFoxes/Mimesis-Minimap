using MelonLoader;
using UnityEngine;

namespace Minimap.API
{
    internal class UIManagerAPI
    {
        private const string GameSettingsPath = "Hub/UIManager/Canvas/1 - top/UIPrefab_GameSettings(Clone)";
        private const string TopCanvasPath = "Hub/UIManager/Canvas/1 - top";

        /// <summary>
        /// Finds Hub/UIManager/Canvas/1 - top. Used as parent for the settings overlay.
        /// </summary>
        public static Transform? GetTopCanvas()
        {
            var go = GameObject.Find(TopCanvasPath);
            return go?.transform;
        }

        /// <summary>
        /// Finds the root transform of the game settings prefab in the scene.
        /// Returns null if the prefab is not yet instantiated (settings not opened).
        /// </summary>
        public static Transform? GetGameSettingsRoot()
        {
            var go = GameObject.Find(GameSettingsPath);
            if (go != null) return go.transform;

            // Fallback: search by name only
            var all = UnityEngine.Object.FindObjectsByType<RectTransform>(FindObjectsSortMode.None);
            foreach (var rt in all)
            {
                if (rt.name == "UIPrefab_GameSettings(Clone)")
                    return rt.transform;
            }

            return null;
        }
    }
}
