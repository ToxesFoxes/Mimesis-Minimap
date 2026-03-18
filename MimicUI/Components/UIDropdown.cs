using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minimap.MimicUI.Components
{
    /// <summary>
    /// A simple dropdown for enum values displayed as a cyclic prev/next control.
    /// </summary>
    internal class UIDropdown : UIElement
    {
        private readonly TextMeshProUGUI _label;
        private readonly TextMeshProUGUI _valueText;
        private readonly Button _prevBtn;
        private readonly Button _nextBtn;
        private readonly List<string> _options;
        private int _selected;

        public event Action<int>? OnValueChanged;

        public UIDropdown(string name, Transform parent, string labelText, List<string> options, int initialIndex)
            : base(name, parent)
        {
            Rect.sizeDelta = new Vector2(0f, 36f);
            Rect.anchorMin = new Vector2(0f, 0f);
            Rect.anchorMax = new Vector2(1f, 0f);

            _options = options;
            _selected = Mathf.Clamp(initialIndex, 0, options.Count - 1);

            AddBackground(new Color(0.15f, 0.15f, 0.15f, 0.8f));

            var labelRect = CreateChild("Label", GameObject.transform);
            _label = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
            _label.text = labelText;
            _label.fontSize = 14;
            _label.color = Color.white;
            _label.alignment = TextAlignmentOptions.MidlineLeft;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(0.45f, 1f);
            labelRect.offsetMin = new Vector2(8f, 0f);
            labelRect.offsetMax = Vector2.zero;

            // < button
            var prevRect = CreateChild("PrevBtn", GameObject.transform);
            prevRect.anchorMin = new Vector2(0.45f, 0.1f);
            prevRect.anchorMax = new Vector2(0.55f, 0.9f);
            prevRect.offsetMin = new Vector2(2f, 0f);
            prevRect.offsetMax = new Vector2(-2f, 0f);
            var prevBg = prevRect.gameObject.AddComponent<Image>();
            prevBg.color = new Color(0.25f, 0.25f, 0.25f);
            _prevBtn = prevRect.gameObject.AddComponent<Button>();
            _prevBtn.targetGraphic = prevBg;
            _prevBtn.onClick.AddListener(Prev);
            var prevTxt = CreateChild("Lbl", prevRect).gameObject.AddComponent<TextMeshProUGUI>();
            prevTxt.text = "<";
            prevTxt.fontSize = 14;
            prevTxt.color = Color.white;
            prevTxt.alignment = TextAlignmentOptions.Center;
            var prevTxtRect = prevTxt.GetComponent<RectTransform>();
            prevTxtRect.anchorMin = Vector2.zero;
            prevTxtRect.anchorMax = Vector2.one;
            prevTxtRect.offsetMin = Vector2.zero;
            prevTxtRect.offsetMax = Vector2.zero;

            // value text
            var valRect = CreateChild("ValueText", GameObject.transform);
            valRect.anchorMin = new Vector2(0.55f, 0.1f);
            valRect.anchorMax = new Vector2(0.85f, 0.9f);
            valRect.offsetMin = new Vector2(2f, 0f);
            valRect.offsetMax = new Vector2(-2f, 0f);
            valRect.gameObject.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
            _valueText = CreateChild("Txt", valRect).gameObject.AddComponent<TextMeshProUGUI>();
            _valueText.text = _options[_selected];
            _valueText.fontSize = 12;
            _valueText.color = Color.white;
            _valueText.alignment = TextAlignmentOptions.Center;
            var vtRect = _valueText.GetComponent<RectTransform>();
            vtRect.anchorMin = Vector2.zero;
            vtRect.anchorMax = Vector2.one;
            vtRect.offsetMin = Vector2.zero;
            vtRect.offsetMax = Vector2.zero;

            // > button
            var nextRect = CreateChild("NextBtn", GameObject.transform);
            nextRect.anchorMin = new Vector2(0.85f, 0.1f);
            nextRect.anchorMax = new Vector2(1f, 0.9f);
            nextRect.offsetMin = new Vector2(2f, 0f);
            nextRect.offsetMax = new Vector2(-8f, 0f);
            var nextBg = nextRect.gameObject.AddComponent<Image>();
            nextBg.color = new Color(0.25f, 0.25f, 0.25f);
            _nextBtn = nextRect.gameObject.AddComponent<Button>();
            _nextBtn.targetGraphic = nextBg;
            _nextBtn.onClick.AddListener(Next);
            var nextTxt = CreateChild("Lbl", nextRect).gameObject.AddComponent<TextMeshProUGUI>();
            nextTxt.text = ">";
            nextTxt.fontSize = 14;
            nextTxt.color = Color.white;
            nextTxt.alignment = TextAlignmentOptions.Center;
            var nextTxtRect = nextTxt.GetComponent<RectTransform>();
            nextTxtRect.anchorMin = Vector2.zero;
            nextTxtRect.anchorMax = Vector2.one;
            nextTxtRect.offsetMin = Vector2.zero;
            nextTxtRect.offsetMax = Vector2.zero;
        }

        private void Prev()
        {
            _selected = (_selected - 1 + _options.Count) % _options.Count;
            _valueText.text = _options[_selected];
            OnValueChanged?.Invoke(_selected);
        }

        private void Next()
        {
            _selected = (_selected + 1) % _options.Count;
            _valueText.text = _options[_selected];
            OnValueChanged?.Invoke(_selected);
        }

        public void SetValue(int index)
        {
            _selected = Mathf.Clamp(index, 0, _options.Count - 1);
            _valueText.text = _options[_selected];
        }

        public int Value => _selected;
    }
}
