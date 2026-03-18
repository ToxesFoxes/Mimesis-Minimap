using System.Reflection;
using MelonLoader;
using UnityEngine;

namespace Minimap.ModCore
{
    internal static class Test
    {
        private const string TargetPath = "Hub/UIManager/Canvas/2 - main/UIPrefab_MainMenu(Clone)/Vertical Layout Group/Host";
        private static bool _done = false;
        private static int _searchTick = 0;

        public static void TryRun()
        {
            if (_done) return;

            // Print a heartbeat every 120 frames so we know it's running
            _searchTick++;
            if (_searchTick % 120 == 1)
                MelonLogger.Msg($"[Test] Searching for '{TargetPath}' (tick {_searchTick})...");

            var go = GameObject.Find(TargetPath);
            if (go == null) return;

            _done = true;
            MelonLogger.Msg($"[Test] Found '{TargetPath}', dumping hierarchy...");
            LogGameObject(go, 0);
            MelonLogger.Msg("[Test] Dump complete.");
        }

        private static void LogGameObject(GameObject go, int depth)
        {
            var indent = new string(' ', depth * 2);
            MelonLogger.Msg($"{indent}[GO] {go.name}  active={go.activeSelf}  layer={go.layer}");

            foreach (var component in go.GetComponents<Component>())
            {
                if (component == null) continue;
                var type = component.GetType();
                MelonLogger.Msg($"{indent}  <{type.Name}>");

                // Properties
                foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (prop.GetIndexParameters().Length > 0) continue;
                    if (!prop.CanRead) continue;
                    try
                    {
                        var val = prop.GetValue(component);
                        MelonLogger.Msg($"{indent}    [P] {prop.Name} = {FormatValue(val)}");
                    }
                    catch { /* skip unreadable */ }
                }

                // Fields
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    try
                    {
                        var val = field.GetValue(component);
                        MelonLogger.Msg($"{indent}    [F] {field.Name} = {FormatValue(val)}");
                    }
                    catch { /* skip unreadable */ }
                }
            }

            for (int i = 0; i < go.transform.childCount; i++)
                LogGameObject(go.transform.GetChild(i).gameObject, depth + 1);
        }

        private static string FormatValue(object? val)
        {
            if (val == null) return "null";
            if (val is UnityEngine.Object uobj && uobj == null) return "null (destroyed)";
            if (val is string s) return $"\"{s}\"";
            return val.ToString() ?? "null";
        }
    }
}
