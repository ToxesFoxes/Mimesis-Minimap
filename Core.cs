using System.Collections;
using MelonLoader;
using Mimic.Actors;
using Minimap.API;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(MiniMap.Core), "MiniMap", "1.0.5", "ToxesFoxes", null)]
[assembly: MelonGame("ReLUGames", "MIMESIS")]

namespace MiniMap
{
    public class Core : MelonMod
    {
        private static GameObject? mapRootObj; // Новый общий родитель
        private static Camera? mapCamera;
        private static RenderTexture? mapTexture;
        private static GameObject? mapCanvasObj;
        private static RawImage? mapImage;
        private static Transform? playerTransform;

        private static bool isVisible = false;
        private static bool isInDungeon = false;
        private static InputAction? toggleAction;
        private static ProtoActor? player;

        private static readonly float cameraYOffset = 3f; // Глобальная переменная для высоты камеры от позиции игрока по Y
        private static readonly float nearClipPlane = 1f; // Смещение ближней части камеры
        private static readonly float farClipPlane = 20f; // Смещение дальней части камеры

        private static readonly Minimap.Compass compass = new();

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("MiniMap initialized. Press F4 to toggle minimap.");
            SetupInput();

            // Подписываемся на события смены сцены
            SceneManager.sceneLoaded += OnSceneLoaded;

            MelonLogger.Msg("MiniMap setup complete.");
        }

        // Обработчик события загрузки сцены
        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            MelonLogger.Msg($"Scene loaded: {scene.name}, minimap was visible: {isVisible}");

            // Сбрасываем ссылку на игрока
            playerTransform = null;

            // Уничтожаем старые объекты
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
            var actions = new InputActionMap("MiniMap");
            toggleAction = actions.AddAction("Toggle", binding: "<Keyboard>/f4");
            toggleAction.performed += _ => ToggleMap();

            actions.Enable();
        }

        private static void DestroyUI()
        {
            if (mapCanvasObj != null)
            {
                compass.DestroyCompass();
                GameObject.Destroy(mapCanvasObj);
                mapCanvasObj = null;
                mapImage = null;
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
            mapCamera.orthographicSize = 10f; // радиус обзора
            mapCamera.clearFlags = CameraClearFlags.SolidColor;
            mapCamera.backgroundColor = new Color(0f, 0f, 0f, 0f); // Прозрачный фон
            mapCamera.cullingMask = ~0; // всё кроме UI

            mapTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
            mapTexture.Create();
            mapCamera.targetTexture = mapTexture;

            // Настройка обрезки камеры
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

            // фон
            var bgObj = new GameObject("MiniMapBG");
            bgObj.transform.SetParent(mapCanvasObj.transform, false);
            var bg = bgObj.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.4f);

            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(1f, 0f);
            bgRect.anchorMax = new Vector2(1f, 0f);
            bgRect.pivot = new Vector2(1f, 0f);
            bgRect.anchoredPosition = new Vector2(-10f, 10f);
            bgRect.sizeDelta = new Vector2(256f, 256f);

            // картинка миникарты
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
            return actor?.transform?.position.y < -10f;
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
