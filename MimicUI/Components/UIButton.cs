using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minimap.MimicUI.Components
{
    internal class UIButton : UIElement
    {
        private readonly TextMeshProUGUI _label;
        private readonly Button _button;

        public UIButton(string name, Transform parent, string labelText, Action onClick)
            : base(name, parent)
        {
            Rect.sizeDelta = new Vector2(160f, 36f);

            AddBackground(new Color(0.2f, 0.2f, 0.2f, 0.9f));

            _button = GameObject.AddComponent<Button>();
            _button.onClick.AddListener(() => onClick());

            var labelRect = CreateChild("Label", GameObject.transform);
            _label = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
            _label.text = labelText;
            _label.fontSize = 14;
            _label.color = Color.white;
            _label.alignment = TextAlignmentOptions.Center;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(4f, 2f);
            labelRect.offsetMax = new Vector2(-4f, -2f);
        }

        public void SetLabel(string text) => _label.text = text;
    }
}
