using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minimap.MimicUI.Components
{
    internal class UIButton : UIElement
    {
        private readonly TextMeshProUGUI _label;
        private readonly Button _button;

        public UIButton(string name, Transform parent, string labelText, Action onClick)
            : base(name + "Button", parent)
        {
            // 2. Child GO: $(name)
            var btnRect = CreateChild(name, GameObject.transform);

            // 2.1 Image — sliced, sprite "MM_Title_Menu_Selected", raycastPadding; starts transparent
            var image = btnRect.gameObject.AddComponent<Image>();
            var sprite = Resources.FindObjectsOfTypeAll<Sprite>()
                .FirstOrDefault(s => s.name == "MM_Title_Menu_Selected");
            if (sprite != null)
                image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.raycastPadding = new Vector4(0f, 25f, 0f, 25f);
            image.color = new Color(1f, 1f, 1f, 0f);

            // 2.2 Button — disable built-in transition so it doesn't fight with hover coroutines
            _button = btnRect.gameObject.AddComponent<Button>();
            _button.targetGraphic = image;
            _button.transition = Selectable.Transition.None;
            _button.onClick.AddListener(() => onClick());

            // 2.3 LayoutElement
            btnRect.gameObject.AddComponent<LayoutElement>();

            var hlg = btnRect.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            // 2.4 ContentSizeFitter
            var csf = btnRect.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            // 2.5 EventTrigger
            btnRect.gameObject.AddComponent<EventTrigger>();

            // 3. Child of $(name): "Text (TMP)"
            var labelRect = CreateChild("Text (TMP)", btnRect);
            _label = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
            _label.text = labelText;
            _label.fontSize = 26;
            _label.color = Color.white;
            _label.alignment = TextAlignmentOptions.Left;
            _label.textWrappingMode = TextWrappingModes.NoWrap;
            _label.overflowMode = TextOverflowModes.Overflow;
            _label.margin = new Vector4(70f, 0f, 450f, 0f);

            // 2.6 Hover: smooth image fade + text colour
            var hover = btnRect.gameObject.AddComponent<UIButtonHover>();
            hover.Init(image, _label, _button);
        }

        public void SetLabel(string text) => _label.text = text;
    }

    internal class UIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private static readonly Color HoverTextColor = new Color(0.1608f, 0.1137f, 0f, 1f);
        private const float FadeDuration = 0.15f;

        private Image _image = null!;
        private TextMeshProUGUI _label = null!;
        private Button _button = null!;

        public void Init(Image image, TextMeshProUGUI label, Button button)
        {
            _image = image;
            _label = label;
            _button = button;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_button.IsInteractable()) return;
            StopAllCoroutines();
            StartCoroutine(FadeImageAlpha(1f));
            StartCoroutine(FadeTextColor(HoverTextColor));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_button.IsInteractable()) return;
            StopAllCoroutines();
            StartCoroutine(FadeImageAlpha(0f));
            StartCoroutine(FadeTextColor(Color.white));
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.IsInteractable()) return;
            StopAllCoroutines();
            StartCoroutine(FadeImageAlpha(0f));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_button.IsInteractable()) return;
            StopAllCoroutines();
            StartCoroutine(FadeImageAlpha(1f));
        }

        private IEnumerator FadeImageAlpha(float target)
        {
            float start = _image.color.a;
            for (float t = 0f; t < FadeDuration; t += Time.unscaledDeltaTime)
            {
                var c = _image.color;
                c.a = Mathf.Lerp(start, target, t / FadeDuration);
                _image.color = c;
                yield return null;
            }
            var final = _image.color;
            final.a = target;
            _image.color = final;
        }

        private IEnumerator FadeTextColor(Color target)
        {
            Color start = _label.color;
            for (float t = 0f; t < FadeDuration; t += Time.unscaledDeltaTime)
            {
                _label.color = Color.Lerp(start, target, t / FadeDuration);
                yield return null;
            }
            _label.color = target;
        }
    }
}
