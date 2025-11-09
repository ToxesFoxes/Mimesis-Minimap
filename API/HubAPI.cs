using MelonLoader;

namespace Minimap.API
{
    internal class HubAPI
    {
        public static Hub GetHub()
        {
            var hub = Hub.s;
            MelonLogger.Msg($"[HubAPI] Hub instance: {(hub != null ? "Found" : "Not Found")}");
            return Hub.s;
        }

        public static CameraManager GetCameraManager()
        {
            Hub hub = GetHub();
            return hub != null ? Helpers.GetPropertyValue<CameraManager>(hub, "cameraman") : null;
        }
    }
}
