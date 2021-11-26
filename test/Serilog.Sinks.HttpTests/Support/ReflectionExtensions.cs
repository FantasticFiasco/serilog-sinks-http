using System.Reflection;

namespace Serilog.Support
{
    public static class ReflectionExtensions
    {
        public static T GetNonPublicInstanceField<T>(this object self, string fieldName)
        {
            return (T)self
                .GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(self);
        }

        public static void SetNonPublicInstanceField<T>(this object self, string fieldName, T value)
        {
            self
                .GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(self, value);
        }

        public static T InvokeNonPublicStaticMethod<T>(this object self, string methodName, params object[] parameters)
        {
            return (T)self
                .GetType()
                .GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(self, parameters);
        }
    }
}
