using System;
using System.Reflection;
using Serilog.Core;
using Serilog.Sinks.Http;
using Serilog.Sinks.Http.Private.NonDurable;

namespace Serilog.Support
{
    public static class SerilogReflection
    {
        public static HttpSinkReflection GetHttpSink(Logger logger)
        {
            var _sink = logger
                .GetType()
                .GetField("_sink", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(logger);

            if (_sink == null)
            {
                throw new Exception("Instance of type 'Logger' has no field called '_sink'.");
            }

            var _sinks = _sink
                .GetType()
                .GetField("_sinks", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(_sink) as ILogEventSink[];

            if (_sinks == null)
            {
                throw new Exception("Instance of type 'SafeAggregateSink' has no field called '_sinks'.");
            }

            if (_sinks.Length != 1)
            {
                throw new Exception("Instance of type 'SafeAggregateSink' contains more than one sink.");
            }

            var httpSink = _sinks[0] as HttpSink;
            if (httpSink == null)
            {
                throw new Exception("Instance of type 'SafeAggregateSink' does not contain HttpSink.");
            }

            return new HttpSinkReflection(httpSink);
        }

        public class HttpSinkReflection
        {
            private readonly HttpSink httpSink;

            public HttpSinkReflection(HttpSink httpSink)
            {
                this.httpSink = httpSink ?? throw new ArgumentNullException(nameof(httpSink));
            }

            public HttpSinkReflection SetRequestUri(string requestUri)
            {
                httpSink
                    .GetType()
                    .GetField("requestUri", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(httpSink, requestUri);

                return this;
            }

            public HttpSinkReflection SetHttpClient(IHttpClient httpClient)
            {
                httpSink
                    .GetType()
                    .GetField("httpClient", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(httpSink, httpClient);

                return this;
            }
        }
    }
}
