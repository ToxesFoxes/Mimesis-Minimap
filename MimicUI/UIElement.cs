using UnityEngine;
using UnityEngine.UI;

namespace Minimap.MimicUI
{
    /// <summary>
    /// Base class for all MimicUI elements.
    /// Wraps a GameObject with a RectTransform and provides helpers.
    /// </summary>
    internal abstract class UIElement
    {
        public GameObject GameObject { get; protected set; }
        public RectTransform Rect { get; protected set; }

        protected UIElement(string name, Transform parent)
        {
            GameObject = new GameObject(name);
            GameObject.transform.SetParent(parent, false);
            Rect = GameObject.AddComponent<RectTransform>();
        }

        public void SetActive(bool active) => GameObject.SetActive(active);

        /// <summary>
        /// Stretch to fill the parent.
        /// </summary>
        protected void StretchToParent()
        {
            Rect.anchorMin = Vector2.zero;
            Rect.anchorMax = Vector2.one;
            Rect.offsetMin = Vector2.zero;
            Rect.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// Creates a child GameObject with a RectTransform.
        /// </summary>
        protected static RectTransform CreateChild(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<RectTransform>();
        }

        /// <summary>
        /// Adds a background Image to this element's GameObject.
        /// </summary>
        protected Image AddBackground(Color color)
        {
            var img = GameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }
    }
}
