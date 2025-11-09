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
    }
}
