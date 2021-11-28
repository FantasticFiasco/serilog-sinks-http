using System;
using System.Reflection;
using Serilog.Core;

namespace Serilog.Support.Reflection
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

        public static T GetSink<T>(this Logger logger) where T : ILogEventSink
        {
            var sinks = logger
                .GetNonPublicInstanceField<object>("_sink")
                .GetNonPublicInstanceField<ILogEventSink[]>("_sinks");

            foreach (var sink in sinks)
            {
                if (sink is T t)
                {
                    return t;
                }
            }

            throw new Exception($"Logger does not contain a sink of type {typeof(T)}.");
        }

        public static object GetSink(this Logger logger)
        {
            var sinks = logger
                .GetNonPublicInstanceField<object>("_sink")
                .GetNonPublicInstanceField<ILogEventSink[]>("_sinks");

            if (sinks.Length != 1)
            {
                throw new Exception("Logger contains more than one sink.");
            }

            return sinks[0];
        }
    }
}
