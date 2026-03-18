using System.Collections;
using System.Collections.Generic;
using MelonLoader;
using Mimic.Actors;
using Minimap;
using Minimap.API;
using Minimap.ModCore;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(Core), "MiniMap", "1.1.0", "ToxesFoxes", null)]
[assembly: MelonGame("ReLUGames", "MIMESIS")]

namespace Minimap
{
    public class Core : MelonMod
    {
        private static GameObject? mapRootObj;
        private static Camera? mapCamera;
        private static RenderTexture? mapTexture;
        private static GameObject? mapCanvasObj;
        private static RawImage? mapImage;
        private static Transform? playerTransform;

        private static bool isVisible = false;
        private static bool isInDungeon = false;
        private static bool manualDungeonMode = false; // used when DungeonModeAuto == false
        private static InputAction? toggleAction;
        private static InputAction? settingsAction;
        private static ProtoActor? player;
        private static RectTransform? mapBgRect;
        private static MimicUI.SettingsPage? settingsPage;

        private static CursorLockMode _prevCursorLockMode = CursorLockMode.Locked;
        private static bool _prevCursorVisible = false;
        private static readonly List<PlayerInput> _suspendedInputs = new();

        private static readonly float cameraYOffset = 3f;
        private static readonly float nearClipPlane = 1f;
        private static readonly float farClipPlane = 20f;

        private const float zoomMin = 3f;
        private const float zoomMax = 40f;
        private static float OrthoFromZoom(float zoom) => zoomMin + zoomMax - zoom;

        private static readonly ModCore.Compass compass = new();

        public override void OnInitializeMelon()
        {
            Settings.Initialize();
            MelonLogger.Msg($"MiniMap initialized. Press {Settings.ToggleKey?.Value ?? "F4"} to toggle minimap.");
            SetupInput();
            SetupSettingsInput();

            SceneManager.sceneLoaded += OnSceneLoaded;

            MelonLogger.Msg("MiniMap setup complete.");
        }

        // Обработчик события загрузки сцены
        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            MelonLogger.Msg($"Scene loaded: {scene.name}, minimap was visible: {isVisible}");

            playerTransform = null;
            SettingsInjector.Reset();
            settingsPage = null;

            DestroyUI();
            DestroyCamera();
            DestroyRoot();

            // Если миникарта была включена, пересоздаём её
            if (isVisible)
            {
                MelonLogger.Msg("Recreating minimap after scene load...");
                MelonCoroutines.Start(RecreateMinimapAfterDelay());
            }
        }

        // Корутина для пересоздания миникарты с небольшой задержкой
        private static IEnumerator RecreateMinimapAfterDelay()
        {
            // Ждём 1 кадр, чтобы сцена полностью загрузилась
            yield return null;

            CreateRoot();
            CreateCamera();
            CreateUI();
        }

        private static void SetupInput()
        {
            toggleAction?.actionMap?.Disable();

            var actions = new InputActionMap("MiniMap");
            toggleAction = actions.AddAction("Toggle", binding: Settings.GetToggleBinding());
            toggleAction.performed += _ => ToggleMap();

            actions.Enable();
        }

        public static void RefreshToggleKey() => SetupInput();

        public static void RefreshSettingsKey() => SetupSettingsInput();

        private static void SetupSettingsInput()
        {
            settingsAction?.actionMap?.Disable();

            var actions = new InputActionMap("MiniMap_Settings");
            settingsAction = actions.AddAction("OpenSettings");

            var modIdx = Mathf.Clamp(Settings.SettingsModifier?.Value ?? 1, 0, Settings.ModifierPaths.Length - 1);
            var modPath = Settings.ModifierPaths[modIdx];

            if (modIdx == 0)
            {
                settingsAction.AddBinding(Settings.GetSettingsKeyPath());
            }
            else
            {
                settingsAction.AddCompositeBinding("ButtonWithOneModifier")
                    .With("Modifier", modPath)
                    .With("Button", Settings.GetSettingsKeyPath());
            }

            settingsAction.performed += _ => ToggleSettingsPanel();
            actions.Enable();
        }

        private static void CreateSettingsUI()
        {
            var topCanvas = API.UIManagerAPI.GetTopCanvas();
            if (topCanvas == null)
            {
                MelonLogger.Warning("[MiniMap] Hub/UIManager/Canvas/1 - top not found, settings UI deferred.");
                return;
            }

            settingsPage = new MimicUI.SettingsPage(topCanvas);
            settingsPage.Hide();
            MelonLogger.Msg("[MiniMap] Settings UI created in Hub/UIManager/Canvas/1 - top.");
        }

        public static void ToggleSettingsPanel()
        {
            if (settingsPage == null || !settingsPage.IsValid)
            {
                settingsPage = null;
                CreateSettingsUI();
            }

            bool wasVisible = settingsPage?.IsVisible == true;
            settingsPage?.Toggle();
            bool isNowVisible = settingsPage?.IsVisible == true;

            if (!wasVisible && isNowVisible)
                OnSettingsOpened();
            else if (wasVisible && !isNowVisible)
                OnSettingsClosed();

            MelonLogger.Msg($"[MiniMap] Settings panel: {(isNowVisible ? "shown" : "hidden")}");
        }

        private static void OnSettingsOpened()
        {
            _prevCursorLockMode = Cursor.lockState;
            _prevCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            _suspendedInputs.Clear();
            var allInputs = UnityEngine.Object.FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);
            foreach (var pi in allInputs)
            {
                pi.DeactivateInput();
                _suspendedInputs.Add(pi);
            }
        }

        private static void OnSettingsClosed()
        {
            Cursor.lockState = _prevCursorLockMode;
            Cursor.visible = _prevCursorVisible;

            foreach (var pi in _suspendedInputs)
                if (pi != null) pi.ActivateInput();
            _suspendedInputs.Clear();
        }

        public static void RefreshCompassVisibility()
        {
            compass.SetVisible(Settings.CompassVisible?.Value ?? true);
        }

        public static void RefreshMapPosition()
        {
            ApplyPosition(Settings.Position);
        }

        public static void RefreshMapZoom(float zoom)
        {
            if (mapCamera != null)
                mapCamera.orthographicSize = OrthoFromZoom(zoom);
        }

        public static void RefreshMapSize(float size)
        {
            if (mapBgRect != null)
                mapBgRect.sizeDelta = new Vector2(size, size);
        }

        private static void DestroyUI()
        {
            if (mapCanvasObj != null)
            {
                compass.DestroyCompass();
                GameObject.Destroy(mapCanvasObj);
                mapCanvasObj = null;
                mapImage = null;
                mapBgRect = null;
            }
        }

        private static void DestroyCamera()
        {
            if (mapCamera != null)
            {
                if (mapCamera.gameObject != null)
                {
                    GameObject.Destroy(mapCamera.gameObject);
                }
                mapCamera = null;
                mapTexture = null;
            }
        }

        private static void DestroyRoot()
        {
            if (mapRootObj != null)
            {
                GameObject.Destroy(mapRootObj);
                mapRootObj = null;
            }
        }

        private static void ToggleMap()
        {
            isVisible = !isVisible;
            MelonLogger.Msg($"Toggling MiniMap: {(isVisible ? "Enabled" : "Disabled")}");

            if (isVisible)
            {
                if (mapRootObj == null)
                {
                    CreateRoot();
                }
                if (mapCamera == null)
                {
                    CreateCamera();
                }
                if (mapCanvasObj == null)
                {
                    CreateUI();
                }
                if (mapCamera != null)
                {
                    mapCamera.enabled = true;
                }
            }
            else
            {
                DestroyUI();
                DestroyCamera();
                DestroyRoot();
            }
        }

        private static void CreateRoot()
        {
            if (mapRootObj != null) return;
            mapRootObj = new GameObject("MiniMapRoot");
            UnityEngine.Object.DontDestroyOnLoad(mapRootObj);
        }

        private static void CreateCamera()
        {
            if (mapCamera != null) return;
            if (mapRootObj == null) CreateRoot();

            GameObject camObj = new GameObject("MiniMapCamera");
            camObj.transform.SetParent(mapRootObj!.transform, false);
            mapCamera = camObj.AddComponent<Camera>();
            mapCamera.orthographic = true;
            mapCamera.orthographicSize = OrthoFromZoom(Settings.MapZoom?.Value ?? 33f);
            mapCamera.clearFlags = CameraClearFlags.SolidColor;
            mapCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            mapCamera.cullingMask = ~0;

            mapTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
            mapTexture.Create();
            mapCamera.targetTexture = mapTexture;

            mapCamera.nearClipPlane = isInDungeon ? nearClipPlane : 0.1f;
            mapCamera.farClipPlane = isInDungeon ? farClipPlane : 100f;
        }

        private static void CreateUI()
        {
            if (mapCanvasObj != null) return;
            if (mapRootObj == null) CreateRoot();

            mapCanvasObj = new GameObject("MiniMapCanvas");
            mapCanvasObj.transform.SetParent(mapRootObj!.transform, false);
            var canvas = mapCanvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mapCanvasObj.AddComponent<CanvasScaler>();
            mapCanvasObj.AddComponent<GraphicRaycaster>();

            var bgObj = new GameObject("MiniMapBG");
            bgObj.transform.SetParent(mapCanvasObj.transform, false);
            var bg = bgObj.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.4f);

            var bgRect = bg.GetComponent<RectTransform>();
            var mapSize = Settings.MapSize?.Value ?? 256f;
            bgRect.sizeDelta = new Vector2(mapSize, mapSize);
            ApplyPosition(Settings.Position, bgRect);
            mapBgRect = bgRect;

            var mapObj = new GameObject("MiniMapImage");
            mapObj.transform.SetParent(bgObj.transform, false);
            mapImage = mapObj.AddComponent<RawImage>();
            mapImage.texture = mapTexture;

            var mapRect = mapObj.GetComponent<RectTransform>();
            mapRect.anchorMin = Vector2.zero;
            mapRect.anchorMax = Vector2.one;
            mapRect.offsetMin = new Vector2(5f, 5f);
            mapRect.offsetMax = new Vector2(-5f, -5f);

            compass.CreateCompass(bgObj.transform);
            compass.SetVisible(Settings.CompassVisible?.Value ?? true);
        }

        private static void ApplyPosition(MinimapPosition position, RectTransform? bgRect = null)
        {
            if (bgRect == null)
            {
                if (mapCanvasObj == null) return;
                var bgGo = mapCanvasObj.transform.Find("MiniMapBG");
                if (bgGo == null) return;
                bgRect = bgGo.GetComponent<RectTransform>();
                if (bgRect == null) return;
            }

            const float margin = 10f;
            switch (position)
            {
                case MinimapPosition.TopLeft:
                    bgRect.anchorMin = new Vector2(0f, 1f);
                    bgRect.anchorMax = new Vector2(0f, 1f);
                    bgRect.pivot     = new Vector2(0f, 1f);
                    bgRect.anchoredPosition = new Vector2(margin, -margin);
                    break;
                case MinimapPosition.TopCenter:
                    bgRect.anchorMin = new Vector2(0.5f, 1f);
                    bgRect.anchorMax = new Vector2(0.5f, 1f);
                    bgRect.pivot     = new Vector2(0.5f, 1f);
                    bgRect.anchoredPosition = new Vector2(0f, -margin);
                    break;
                case MinimapPosition.TopRight:
                    bgRect.anchorMin = new Vector2(1f, 1f);
                    bgRect.anchorMax = new Vector2(1f, 1f);
                    bgRect.pivot     = new Vector2(1f, 1f);
                    bgRect.anchoredPosition = new Vector2(-margin, -margin);
                    break;
                case MinimapPosition.MiddleLeft:
                    bgRect.anchorMin = new Vector2(0f, 0.5f);
                    bgRect.anchorMax = new Vector2(0f, 0.5f);
                    bgRect.pivot     = new Vector2(0f, 0.5f);
                    bgRect.anchoredPosition = new Vector2(margin, 0f);
                    break;
                case MinimapPosition.MiddleRight:
                    bgRect.anchorMin = new Vector2(1f, 0.5f);
                    bgRect.anchorMax = new Vector2(1f, 0.5f);
                    bgRect.pivot     = new Vector2(1f, 0.5f);
                    bgRect.anchoredPosition = new Vector2(-margin, 0f);
                    break;
                case MinimapPosition.BottomLeft:
                    bgRect.anchorMin = new Vector2(0f, 0f);
                    bgRect.anchorMax = new Vector2(0f, 0f);
                    bgRect.pivot     = new Vector2(0f, 0f);
                    bgRect.anchoredPosition = new Vector2(margin, margin);
                    break;
                case MinimapPosition.BottomCenter:
                    bgRect.anchorMin = new Vector2(0.5f, 0f);
                    bgRect.anchorMax = new Vector2(0.5f, 0f);
                    bgRect.pivot     = new Vector2(0.5f, 0f);
                    bgRect.anchoredPosition = new Vector2(0f, margin);
                    break;
                default: // BottomRight
                    bgRect.anchorMin = new Vector2(1f, 0f);
                    bgRect.anchorMax = new Vector2(1f, 0f);
                    bgRect.pivot     = new Vector2(1f, 0f);
                    bgRect.anchoredPosition = new Vector2(-margin, margin);
                    break;
                case MinimapPosition.Manual:
                    bgRect.anchorMin = new Vector2(0.5f, 0.5f);
                    bgRect.anchorMax = new Vector2(0.5f, 0.5f);
                    bgRect.pivot     = new Vector2(0.5f, 0.5f);
                    bgRect.anchoredPosition = new Vector2(
                        Settings.MapPosX?.Value ?? 0f,
                        Settings.MapPosY?.Value ?? 0f);
                    break;
            }
        }

        public static ProtoActor? GetCurrentSpectatingActor()
        {
            var alivePlayers = ActorAPI.GetAlivePlayers();
            if (alivePlayers == null) return null;
            if (alivePlayers.Length != 0)
            {
                var cameraManager = HubAPI.GetCameraManager();
                if (cameraManager != null)
                {
                    var targetActorID = cameraManager.SpectatorTargetActorID;
                    if (targetActorID != null)
                    {
                        var targetPlayer = ActorAPI.GetActorByID(targetActorID);
                        return targetPlayer;
                    }
                }
            }
            return null;
        }

        public static bool IsActorInDungeon(ProtoActor? actor)
        {
            if (actor == null) return false;
            if (Settings.DungeonModeAuto?.Value == false)
                return manualDungeonMode;
            return actor?.transform?.position.y < -10f;
        }

        public static void SetManualDungeonMode(bool value)
        {
            manualDungeonMode = value;
        }

        public static void SetCurrentPlayer(ProtoActor? newPlayer)
        {
            var oldPlayer = player;
            player = newPlayer;
            isInDungeon = IsActorInDungeon(player);

            if (newPlayer == null && oldPlayer != null)
            {
                MelonLogger.Msg("No player to follow on minimap.");
                return;
            }
            if (oldPlayer == null && newPlayer != null)
            {
                MelonLogger.Msg($"Now following player {ActorAPI.GetActorName(newPlayer)} on minimap.");
                return;
            }
            if (oldPlayer?.ActorID != newPlayer?.ActorID)
            {
                MelonLogger.Msg($"Current player changed to {ActorAPI.GetActorName(newPlayer)}.");
                return;
            }
        }

        public override void OnLateUpdate()
        {
            SettingsInjector.TryInject();

            if (!isVisible || mapCamera == null) return;

            var localPlayer = ActorAPI.GetLocalPlayer();
            if (localPlayer != null && localPlayer?.dead == true)
            {
                var spectatingActor = GetCurrentSpectatingActor();
                if (spectatingActor != null)
                {
                    SetCurrentPlayer(spectatingActor);
                }
                else
                {
                    SetCurrentPlayer(null);
                    return;
                }
            }
            else
            {
                SetCurrentPlayer(localPlayer);
            }

            if (player)
            {
                playerTransform = player.transform;
                if (playerTransform != null)
                {
                    Vector3 playerPos = playerTransform.position;
                    var playerYWithOffset = playerPos.y + cameraYOffset;
                    var pos = new Vector3(playerPos.x, Mathf.Round(playerYWithOffset), playerPos.z);

                    mapCamera.transform.position = pos;
                    mapCamera.transform.rotation = Quaternion.Euler(90f, playerTransform.eulerAngles.y, 0f);

                    mapCamera.nearClipPlane = isInDungeon ? nearClipPlane : 0.1f;
                    mapCamera.farClipPlane = isInDungeon ? farClipPlane : 100f;
                }
                if (mapImage != null)
                {
                    if (isInDungeon)
                    {
                            Material greenFilter = new Material(Shader.Find("UI/Default"));
                            greenFilter.color = new Color(0f, 1f, 0f, 1f); // Зелёный цвет
                            mapImage.material = greenFilter;
                    }
                    else
                    {
                        mapImage.material = null;
                    }
                }

                compass.UpdateCompass(player);
            }
        }
    }
}
