using UnityEngine;

namespace Minimap.API
{
    internal class UnityObjectAPI
    {
        public static T[] FindObjectsOfType<T>()
            where T : Component
        {
            return UnityEngine.Object.FindObjectsByType<T>(FindObjectsSortMode.None);
        }
    }
}
