using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minimap.MimicUI.Components
{
    internal class UISlider : UIElement
    {
        private readonly TextMeshProUGUI _label;
        private readonly TextMeshProUGUI _valueText;
        private readonly Slider _slider;

        public event Action<float>? OnValueChanged;

        public UISlider(string name, Transform parent, string labelText, float min, float max, float initialValue)
            : base(name, parent)
        {
            Rect.sizeDelta = new Vector2(0f, 36f);
            Rect.anchorMin = new Vector2(0f, 0f);
            Rect.anchorMax = new Vector2(1f, 0f);

            AddBackground(new Color(0.15f, 0.15f, 0.15f, 0.8f));

            var labelRect = CreateChild("Label", GameObject.transform);
            _label = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
            _label.text = labelText;
            _label.fontSize = 14;
            _label.color = Color.white;
            _label.alignment = TextAlignmentOptions.MidlineLeft;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(0.35f, 1f);
            labelRect.offsetMin = new Vector2(8f, 0f);
            labelRect.offsetMax = Vector2.zero;

            var valRect = CreateChild("ValueText", GameObject.transform);
            valRect.anchorMin = new Vector2(0.85f, 0.1f);
            valRect.anchorMax = new Vector2(1f, 0.9f);
            valRect.offsetMin = new Vector2(2f, 0f);
            valRect.offsetMax = new Vector2(-8f, 0f);
            _valueText = valRect.gameObject.AddComponent<TextMeshProUGUI>();
            _valueText.text = initialValue.ToString("F1");
            _valueText.fontSize = 12;
            _valueText.color = Color.white;
            _valueText.alignment = TextAlignmentOptions.Center;

            var sliderRect = CreateChild("Slider", GameObject.transform);
            sliderRect.anchorMin = new Vector2(0.35f, 0.2f);
            sliderRect.anchorMax = new Vector2(0.85f, 0.8f);
            sliderRect.offsetMin = new Vector2(4f, 0f);
            sliderRect.offsetMax = new Vector2(-4f, 0f);

            _slider = sliderRect.gameObject.AddComponent<Slider>();
            _slider.minValue = min;
            _slider.maxValue = max;
            _slider.value = initialValue;

            var bgRect = CreateChild("Background", sliderRect);
            bgRect.anchorMin = new Vector2(0f, 0.25f);
            bgRect.anchorMax = new Vector2(1f, 0.75f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bgRect.gameObject.AddComponent<Image>();
            bgImg.color = new Color(0.3f, 0.3f, 0.3f);
            _slider.targetGraphic = bgImg;

            var fillAreaRect = CreateChild("Fill Area", sliderRect);
            fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRect.offsetMin = new Vector2(5f, 0f);
            fillAreaRect.offsetMax = new Vector2(-5f, 0f);
            var fillRect = CreateChild("Fill", fillAreaRect);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImg = fillRect.gameObject.AddComponent<Image>();
            fillImg.color = new Color(0.3f, 0.6f, 0.9f);
            _slider.fillRect = fillRect;

            var handleAreaRect = CreateChild("Handle Slide Area", sliderRect);
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(10f, 0f);
            handleAreaRect.offsetMax = new Vector2(-10f, 0f);
            var handleRect = CreateChild("Handle", handleAreaRect);
            handleRect.anchorMin = new Vector2(0f, 0f);
            handleRect.anchorMax = new Vector2(0f, 1f);
            handleRect.sizeDelta = new Vector2(20f, 0f);
            var handleImg = handleRect.gameObject.AddComponent<Image>();
            handleImg.color = Color.white;
            _slider.handleRect = handleRect;
            _slider.targetGraphic = handleImg;

            _slider.onValueChanged.AddListener(v =>
            {
                _valueText.text = v.ToString("F1");
                OnValueChanged?.Invoke(v);
            });
        }

        public float Value => _slider.value;

        public void SetValue(float value)
        {
            _slider.value = value;
            _valueText.text = value.ToString("F1");
        }
    }
}
