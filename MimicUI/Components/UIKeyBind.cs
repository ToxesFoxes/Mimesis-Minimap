using System;
using System.Collections;
using MelonLoader;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Minimap.MimicUI.Components
{
    /// <summary>
    /// A button that, when clicked, enters "listening" mode and captures the next key press.
    /// </summary>
    internal class UIKeyBind : UIElement
    {
        private readonly TextMeshProUGUI _label;
        private readonly TextMeshProUGUI _keyText;
        private readonly Button _button;
        private string _currentKey;
        private bool _isListening = false;

        public event Action<string>? OnKeyChanged;

        public UIKeyBind(string name, Transform parent, string labelText, string initialKey)
            : base(name, parent)
        {
            Rect.sizeDelta = new Vector2(0f, 36f);
            Rect.anchorMin = new Vector2(0f, 0f);
            Rect.anchorMax = new Vector2(1f, 0f);

            _currentKey = initialKey;
            AddBackground(new Color(0.15f, 0.15f, 0.15f, 0.8f));

            var labelRect = CreateChild("Label", GameObject.transform);
            _label = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
            _label.text = labelText;
            _label.fontSize = 14;
            _label.color = Color.white;
            _label.alignment = TextAlignmentOptions.MidlineLeft;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(0.6f, 1f);
            labelRect.offsetMin = new Vector2(8f, 0f);
            labelRect.offsetMax = Vector2.zero;

            var btnRect = CreateChild("KeyBtn", GameObject.transform);
            btnRect.anchorMin = new Vector2(0.6f, 0.1f);
            btnRect.anchorMax = new Vector2(1f, 0.9f);
            btnRect.offsetMin = new Vector2(-2f, 0f);
            btnRect.offsetMax = new Vector2(-8f, 0f);

            var btnBg = btnRect.gameObject.AddComponent<Image>();
            btnBg.color = new Color(0.2f, 0.2f, 0.4f);

            _button = btnRect.gameObject.AddComponent<Button>();
            _button.targetGraphic = btnBg;
            _button.onClick.AddListener(StartListening);

            var keyRect = CreateChild("KeyText", btnRect);
            _keyText = keyRect.gameObject.AddComponent<TextMeshProUGUI>();
            _keyText.text = _currentKey;
            _keyText.fontSize = 13;
            _keyText.color = Color.white;
            _keyText.alignment = TextAlignmentOptions.Center;
            keyRect.anchorMin = Vector2.zero;
            keyRect.anchorMax = Vector2.one;
            keyRect.offsetMin = Vector2.zero;
            keyRect.offsetMax = Vector2.zero;
        }

        private void StartListening()
        {
            if (_isListening) return;

            _isListening = true;
            _keyText.text = "...";
            _keyText.color = Color.yellow;

            MelonCoroutines.Start(ListenForKeyPress());
        }

        private IEnumerator ListenForKeyPress()
        {
            yield return null; // пропускаем текущий кадр, чтобы не поймать клик мыши

            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                _isListening = false;
                _keyText.text = _currentKey;
                _keyText.color = Color.white;
                yield break;
            }

            var allKeys = (Key[])Enum.GetValues(typeof(Key));

            while (_isListening)
            {
                foreach (var key in allKeys)
                {
                    if (key == Key.None) continue;
                    var kc = keyboard[key];
                    if (kc != null && kc.wasPressedThisFrame)
                    {
                        string keyName = kc.name.ToUpper();
                        _isListening = false;
                        _currentKey = keyName;
                        _keyText.text = keyName;
                        _keyText.color = Color.white;
                        OnKeyChanged?.Invoke(keyName);
                        yield break;
                    }
                }
                yield return null;
            }
        }

        public string Value => _currentKey;
    }
}
