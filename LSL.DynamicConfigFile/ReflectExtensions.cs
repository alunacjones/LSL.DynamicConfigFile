using System.Reflection;

namespace LSL.DynamicConfigFile
{
    internal static class ReflectExtensions
    {
        public static IReflect SetStatic(this IReflect type, string fieldName, object value)
        {
            type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, value);

            return type;
        }
    }
}