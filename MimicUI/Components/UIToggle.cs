using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minimap.MimicUI.Components
{
    internal class UIToggle : UIElement
    {
        private readonly TextMeshProUGUI _label;
        private readonly TextMeshProUGUI _stateText;
        private readonly Button _button;
        private bool _value;

        public event Action<bool>? OnValueChanged;

        public UIToggle(string name, Transform parent, string labelText, bool initialValue)
            : base(name, parent)
        {
            Rect.sizeDelta = new Vector2(0f, 36f);
            Rect.anchorMin = new Vector2(0f, 0f);
            Rect.anchorMax = new Vector2(1f, 0f);

            _value = initialValue;
            AddBackground(new Color(0.15f, 0.15f, 0.15f, 0.8f));

            var labelRect = CreateChild("Label", GameObject.transform);
            _label = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
            _label.text = labelText;
            _label.fontSize = 14;
            _label.color = Color.white;
            _label.alignment = TextAlignmentOptions.MidlineLeft;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(0.75f, 1f);
            labelRect.offsetMin = new Vector2(8f, 0f);
            labelRect.offsetMax = new Vector2(0f, 0f);

            var btnRect = CreateChild("ToggleBtn", GameObject.transform);
            btnRect.anchorMin = new Vector2(0.75f, 0.1f);
            btnRect.anchorMax = new Vector2(1f, 0.9f);
            btnRect.offsetMin = new Vector2(-2f, 0f);
            btnRect.offsetMax = new Vector2(-8f, 0f);

            var btnBg = btnRect.gameObject.AddComponent<Image>();
            btnBg.color = _value ? new Color(0.2f, 0.7f, 0.2f) : new Color(0.5f, 0.1f, 0.1f);

            _button = btnRect.gameObject.AddComponent<Button>();
            _button.targetGraphic = btnBg;
            _button.onClick.AddListener(Toggle);

            var stateRect = CreateChild("StateText", btnRect);
            _stateText = stateRect.gameObject.AddComponent<TextMeshProUGUI>();
            _stateText.text = _value ? "ON" : "OFF";
            _stateText.fontSize = 12;
            _stateText.color = Color.white;
            _stateText.alignment = TextAlignmentOptions.Center;
            stateRect.anchorMin = Vector2.zero;
            stateRect.anchorMax = Vector2.one;
            stateRect.offsetMin = Vector2.zero;
            stateRect.offsetMax = Vector2.zero;
        }

        private void Toggle()
        {
            SetValue(!_value);
            OnValueChanged?.Invoke(_value);
        }

        public void SetValue(bool value)
        {
            _value = value;
            _stateText.text = _value ? "ON" : "OFF";
            var btnBg = _button.GetComponent<Image>();
            if (btnBg != null)
                btnBg.color = _value ? new Color(0.2f, 0.7f, 0.2f) : new Color(0.5f, 0.1f, 0.1f);
        }

        public bool Value => _value;
    }
}
