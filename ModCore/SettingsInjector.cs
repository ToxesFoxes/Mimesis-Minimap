using MelonLoader;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

            var btnGo = new GameObject("MinimapSettingsBtn");
            btnGo.transform.SetParent(root, false);

            var btnRect = btnGo.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0f, 0f);
            btnRect.anchorMax = new Vector2(0f, 0f);
            btnRect.pivot = new Vector2(0f, 0f);
            btnRect.anchoredPosition = new Vector2(10f, 10f);
            btnRect.sizeDelta = new Vector2(180f, 40f);

            var btnBg = btnGo.AddComponent<Image>();
            btnBg.color = new Color(0.2f, 0.2f, 0.4f, 0.9f);

            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = btnBg;
            btn.onClick.AddListener(Core.ToggleSettingsPanel);

            var lblGo = new GameObject("Label");
            lblGo.transform.SetParent(btnGo.transform, false);
            var lblRect = lblGo.AddComponent<RectTransform>();
            lblRect.anchorMin = Vector2.zero;
            lblRect.anchorMax = Vector2.one;
            lblRect.offsetMin = Vector2.zero;
            lblRect.offsetMax = Vector2.zero;
            var lbl = lblGo.AddComponent<TextMeshProUGUI>();
            lbl.text = "⚙ Minimap";
            lbl.fontSize = 15;
            lbl.color = Color.white;
            lbl.alignment = TextAlignmentOptions.Center;

            _injected = true;
            MelonLogger.Msg("[MiniMap] Button injected into UIPrefab_GameSettings.");
        }
    }
}
