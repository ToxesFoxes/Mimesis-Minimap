using System.Collections.Generic;
using Minimap.ModCore;
using Minimap.MimicUI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minimap.MimicUI
{
    /// <summary>
    /// Standalone settings overlay panel. Toggled by the configured key combo.
    /// </summary>
    internal class SettingsPage
    {
        private readonly GameObject _root;
        private bool _built = false;

        private UIKeyBind? _settingsKeyBind;
        private UIDropdown? _settingsModifierDropdown;
        private UIKeyBind? _toggleKeyBind;
        private UIToggle? _dungeonAutoToggle;
        private UIToggle? _compassToggle;
        private UIDropdown? _positionDropdown;
        private UISlider? _zoomSlider;
        private UISlider? _sizeSlider;
        private UISlider? _posXSlider;
        private UISlider? _posYSlider;

        public SettingsPage(Transform parent)
        {
            _root = new GameObject("MimicUI_MinimapSettings");
            _root.transform.SetParent(parent, false);

            var rect = _root.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(420f, 520f);

            Build();
        }

        private void Build()
        {
            if (_built) return;
            _built = true;

            // Background panel
            var bg = _root.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.08f, 0.25f);

            // Title
            var titleRect = new GameObject("Title").AddComponent<RectTransform>();
            titleRect.SetParent(_root.transform, false);
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -8f);
            titleRect.sizeDelta = new Vector2(0f, 36f);
            var titleTxt = titleRect.gameObject.AddComponent<TextMeshProUGUI>();
            titleTxt.text = "Minimap Settings";
            titleTxt.fontSize = 18;
            titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.color = Color.white;
            titleTxt.alignment = TextAlignmentOptions.Center;

            // Close button (×)
            var closeGo = new GameObject("CloseBtn");
            closeGo.transform.SetParent(_root.transform, false);
            var closeRect = closeGo.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-4f, -4f);
            closeRect.sizeDelta = new Vector2(30f, 30f);
            var closeBg = closeGo.AddComponent<Image>();
            closeBg.color = new Color(0.55f, 0.1f, 0.1f, 0.9f);
            var closeBtn = closeGo.AddComponent<Button>();
            closeBtn.targetGraphic = closeBg;
            closeBtn.onClick.AddListener(() => Core.ToggleSettingsPanel());
            var closeLblGo = new GameObject("Lbl");
            closeLblGo.transform.SetParent(closeGo.transform, false);
            var closeLblRect = closeLblGo.AddComponent<RectTransform>();
            closeLblRect.anchorMin = Vector2.zero;
            closeLblRect.anchorMax = Vector2.one;
            closeLblRect.offsetMin = Vector2.zero;
            closeLblRect.offsetMax = Vector2.zero;
            var closeTxt = closeLblGo.AddComponent<TextMeshProUGUI>();
            closeTxt.text = "×";
            closeTxt.fontSize = 18;
            closeTxt.color = Color.white;
            closeTxt.alignment = TextAlignmentOptions.Center;

            // Scroll / content area - vertical layout
            var content = new GameObject("Content").AddComponent<RectTransform>();
            content.SetParent(_root.transform, false);
            content.anchorMin = new Vector2(0f, 0f);
            content.anchorMax = new Vector2(1f, 1f);
            content.offsetMin = new Vector2(12f, 8f);
            content.offsetMax = new Vector2(-12f, -52f);

            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // --- Settings Open Key ---
            _settingsKeyBind = new UIKeyBind(
                "SettingsKey", content,
                "Settings Key", Settings.SettingsKey?.Value ?? "M");
            _settingsKeyBind.Rect.sizeDelta = new Vector2(0f, 36f);
            _settingsKeyBind.OnKeyChanged += key =>
            {
                if (Settings.SettingsKey != null) Settings.SettingsKey.Value = key;
                Core.RefreshSettingsKey();
            };

            // --- Settings Modifier ---
            _settingsModifierDropdown = new UIDropdown(
                "SettingsModifier", content,
                "Settings Modifier", new List<string>(Settings.ModifierNames),
                Settings.SettingsModifier?.Value ?? 1);
            _settingsModifierDropdown.Rect.sizeDelta = new Vector2(0f, 36f);
            _settingsModifierDropdown.OnValueChanged += v =>
            {
                if (Settings.SettingsModifier != null) Settings.SettingsModifier.Value = v;
                Core.RefreshSettingsKey();
            };

            // --- Toggle Key ---
            _toggleKeyBind = new UIKeyBind(
                "ToggleKey", content,
                "Toggle Key", Settings.ToggleKey?.Value ?? "F4");
            _toggleKeyBind.Rect.sizeDelta = new Vector2(0f, 36f);
            _toggleKeyBind.OnKeyChanged += key =>
            {
                if (Settings.ToggleKey != null) Settings.ToggleKey.Value = key;
                Core.RefreshToggleKey();
            };

            // --- Dungeon Auto ---
            _dungeonAutoToggle = new UIToggle(
                "DungeonAuto", content,
                "Auto Dungeon Mode", Settings.DungeonModeAuto?.Value ?? true);
            _dungeonAutoToggle.Rect.sizeDelta = new Vector2(0f, 36f);
            _dungeonAutoToggle.OnValueChanged += v =>
            {
                if (Settings.DungeonModeAuto != null) Settings.DungeonModeAuto.Value = v;
            };

            // --- Compass Visible ---
            _compassToggle = new UIToggle(
                "CompassVisible", content,
                "Show Compass", Settings.CompassVisible?.Value ?? true);
            _compassToggle.Rect.sizeDelta = new Vector2(0f, 36f);
            _compassToggle.OnValueChanged += v =>
            {
                if (Settings.CompassVisible != null) Settings.CompassVisible.Value = v;
                Core.RefreshCompassVisibility();
            };

            // --- Position Dropdown ---
            var positionNames = new List<string>
            {
                "Top Left", "Top Center", "Top Right",
                "Middle Left", "Middle Right",
                "Bottom Left", "Bottom Center", "Bottom Right",
                "Manual"
            };
            _positionDropdown = new UIDropdown(
                "MapPosition", content,
                "Map Position", positionNames, Settings.MapPosition?.Value ?? (int)MinimapPosition.BottomRight);
            _positionDropdown.Rect.sizeDelta = new Vector2(0f, 36f);
            _positionDropdown.OnValueChanged += v =>
            {
                bool isManual = v == (int)MinimapPosition.Manual;
                _posXSlider?.SetActive(isManual);
                _posYSlider?.SetActive(isManual);
                if (Settings.MapPosition != null) Settings.MapPosition.Value = v;
                Core.RefreshMapPosition();
            };

            // --- Position X (Manual only) ---
            _posXSlider = new UISlider(
                "MapPosX", content,
                "Position X", -960f, 960f, Settings.MapPosX?.Value ?? 0f);
            _posXSlider.Rect.sizeDelta = new Vector2(0f, 36f);
            _posXSlider.OnValueChanged += v =>
            {
                if (Settings.MapPosX != null) Settings.MapPosX.Value = v;
                Core.RefreshMapPosition();
            };

            // --- Position Y (Manual only) ---
            _posYSlider = new UISlider(
                "MapPosY", content,
                "Position Y", -540f, 540f, Settings.MapPosY?.Value ?? 0f);
            _posYSlider.Rect.sizeDelta = new Vector2(0f, 36f);
            _posYSlider.OnValueChanged += v =>
            {
                if (Settings.MapPosY != null) Settings.MapPosY.Value = v;
                Core.RefreshMapPosition();
            };

            bool startManual = (Settings.MapPosition?.Value ?? (int)MinimapPosition.BottomRight) == (int)MinimapPosition.Manual;
            _posXSlider.SetActive(startManual);
            _posYSlider.SetActive(startManual);

            // --- Map Size ---
            _sizeSlider = new UISlider(
                "MapSize", content,
                "Map Size", 100f, 512f, Settings.MapSize?.Value ?? 256f);
            _sizeSlider.Rect.sizeDelta = new Vector2(0f, 36f);
            _sizeSlider.OnValueChanged += v =>
            {
                if (Settings.MapSize != null) Settings.MapSize.Value = v;
                Core.RefreshMapSize(v);
            };

            // --- Zoom Slider ---
            _zoomSlider = new UISlider(
                "MapZoom", content,
                "Map Zoom", 3f, 40f, Settings.MapZoom?.Value ?? 33f);
            _zoomSlider.Rect.sizeDelta = new Vector2(0f, 36f);
            _zoomSlider.OnValueChanged += v =>
            {
                if (Settings.MapZoom != null) Settings.MapZoom.Value = v;
                Core.RefreshMapZoom(v);
            };
        }

        public void Show() => _root.SetActive(true);
        public void Hide() => _root.SetActive(false);
        public bool IsVisible => _root.activeSelf;
        public bool IsValid => _root != null;
        public void Toggle()
        {
            if (IsVisible) Hide(); else Show();
        }

        /// <summary>
        /// Sync UI state from current Settings values (call after loading prefs).
        /// </summary>
        public void SyncFromSettings()
        {
            _toggleKeyBind?.GetType(); // no-op, just ensure built
            // Values already loaded at Build() time via Settings.*?.Value
        }
    }
}
