using Mimic.Actors;
using Minimap.API;
using UnityEngine;
using UnityEngine.UI;

namespace Minimap
{
    internal class Compass
    {
        private List<Vector3> teleporterPositions = new List<Vector3>();
        private GameObject compassObj;
        private Text northText, eastText, southText, westText;
        private Vector2 offset = new Vector2(45, -40);
        private int heightOffset = 36;
        private int widthOffset = 36;
        private bool useTeleporterAngle = false;
        private float teleporterAngle = 0f;

        public void DestroyCompass()
        {
            if (compassObj != null)
            {
                UnityEngine.Object.Destroy(compassObj);
                compassObj = null;
                northText = eastText = southText = westText = null;
            }
        }

        public void UpdateCompass(ProtoActor actor)
        {
            if (actor == null || actor.transform == null)
                return;

            useTeleporterAngle = false;
            GamePlayScene gamePlayScene = HubAPI.GetGamePlayScene();
            if (gamePlayScene != null)
            {
                teleporterPositions = gamePlayScene.GetTeleporterPositions(gamePlayScene.CheckActorIsIndoor(actor));
                if (teleporterPositions.Count > 0)
                {
                    useTeleporterAngle = true;
                    teleporterAngle = CalculateTeleporterAngle(actor, teleporterPositions);
                }
            }

            float playerYaw = actor.transform.eulerAngles.y;
            float baseAngle = useTeleporterAngle ? teleporterAngle : -playerYaw;
            UpdateCompassLabels(baseAngle);
        }

        private float CalculateTeleporterAngle(ProtoActor actor, List<Vector3> teleporters)
        {
            Vector3 closest = Vector3.zero;
            float minDist = float.MaxValue;
            foreach (var pos in teleporters)
            {
                float dist = (actor.transform.position - pos).sqrMagnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = pos;
                }
            }
            Vector3 dir = closest - actor.transform.position;
            dir.y = 0f;
			Vector3 forward = actor.transform.forward;
			float num2 = Vector3.Angle(dir, forward);
			if (Vector3.Cross(forward, dir).y < 0f)
			{
				num2 = 0f - num2;
			}
			float y = (num2 + 270f + 360f + 15f) % 360f;
            return y;
        }

        public void CreateCompass(Transform bgObj)
        {
            compassObj = new GameObject("MiniMapCompass");
            compassObj.transform.SetParent(bgObj.transform, false);
            var compassRect = compassObj.AddComponent<RectTransform>();
            compassRect.anchorMin = Vector2.zero;
            compassRect.anchorMax = Vector2.one;
            compassRect.pivot = new Vector2(0.5f, 0.5f);
            compassRect.offsetMin = Vector2.zero;
            compassRect.offsetMax = Vector2.zero;

            northText = CreateCompassLabel("N", compassObj.transform);
            eastText = CreateCompassLabel("E", compassObj.transform);
            southText = CreateCompassLabel("S", compassObj.transform);
            westText = CreateCompassLabel("W", compassObj.transform);
        }

        private Text CreateCompassLabel(string label, Transform parent)
        {
            var obj = new GameObject($"Compass{label}");
            obj.transform.SetParent(parent, false);
            var text = obj.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 18;
            text.color = Color.white;
            var rect = text.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            return text;
        }

        private Vector2 GetCompassEdgePosition(float angleDeg, float w, float h, float margin)
        {
            float angle = (angleDeg + (useTeleporterAngle ? 70f : -105f)) * Mathf.Deg2Rad;
            float dx = Mathf.Sin(angle);
            float dy = Mathf.Cos(angle);
            float halfW = (w + widthOffset) / 2f - margin;
            float halfH = (h + heightOffset) / 2f - margin;
            float absDx = Mathf.Abs(dx);
            float absDy = Mathf.Abs(dy);
            float scale = absDx * halfH > absDy * halfW ? halfW / absDx : halfH / absDy;
            return new Vector2(dx * scale, dy * scale) + offset;
        }

        private void UpdateCompassLabels(float baseAngle)
        {
            if (compassObj == null) return;
            var compassRect = compassObj.GetComponent<RectTransform>();
            float w = compassRect.rect.width;
            float h = compassRect.rect.height;
            float margin = 16f;
            if (northText != null)
                northText.rectTransform.anchoredPosition = GetCompassEdgePosition(0f + baseAngle, w, h, margin);
            if (eastText != null)
                eastText.rectTransform.anchoredPosition = GetCompassEdgePosition(90f + baseAngle, w, h, margin);
            if (southText != null)
                southText.rectTransform.anchoredPosition = GetCompassEdgePosition(180f + baseAngle, w, h, margin);
            if (westText != null)
                westText.rectTransform.anchoredPosition = GetCompassEdgePosition(270f + baseAngle, w, h, margin);
        }
    }
}
