using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MelonLoader;

namespace Minimap
{
    internal class Helpers
    {
        private const BindingFlags DefaultFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
        public static T GetPropertyValue<T>(object target, string propertyName)
        {
            if (target == null)
                return default;

            PropertyInfo prop = target.GetType().GetProperty(propertyName, DefaultFlags);
            if (prop == null)
            {
                MelonLogger.Msg($"[Helpers] Property '{propertyName}' not found on target of type '{target.GetType()}'");
                return default;
            }

            object value = prop.GetValue(target);
            return value == null ? default : (T)value;
        }
        public static object GetFieldValue(object target, string fieldName)
        {
            if (target == null)
                return null;

            FieldInfo field = target.GetType().GetField(fieldName, DefaultFlags);
            return field?.GetValue(target);
        }

        public static object GetFieldValue(Type type, string fieldName)
        {
            FieldInfo field = type.GetField(fieldName, DefaultFlags);
            return field?.GetValue(null);
        }

        public static T GetFieldValue<T>(object target, string fieldName)
        {
            object value = GetFieldValue(target, fieldName);
            return value == null ? default : (T)value;
        }

        public static void SetFieldValue(object target, string fieldName, object value)
        {
            if (target == null)
                return;

            FieldInfo field = target.GetType().GetField(fieldName, DefaultFlags);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }

        public static object InvokeMethod(object target, string methodName, params object[] parameters)
        {
            if (target == null)
                return null;

            MethodInfo method = target.GetType().GetMethod(methodName, DefaultFlags);
            if (method == null)
                return null;

            return method.Invoke(target, parameters.Length > 0 ? parameters : null);
        }
    }
}
