using MelonLoader;

namespace Minimap.API
{
    internal class HubAPI
    {
        public static Hub GetHub()
        {
            var hub = Hub.s;
            return Hub.s;
        }

        public static CameraManager GetCameraManager()
        {
            Hub hub = GetHub();
            return hub != null ? Helpers.GetPropertyValue<CameraManager>(hub, "cameraman") : null;
        }

        public static Hub.PersistentData GetPersistentData()
        {
            Hub hub = GetHub();
            return hub != null ? Helpers.GetFieldValue<Hub.PersistentData>(hub, "pdata") : null;
        }

        public static GamePlayScene GetGamePlayScene()
        {
            Hub.PersistentData pdata = GetPersistentData();
            if (pdata.main is GamePlayScene gamePlayScene)
            {
                return gamePlayScene;
            }
            return null;
        }
    }
}
