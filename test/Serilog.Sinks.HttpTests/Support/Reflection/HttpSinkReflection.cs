using Serilog.Core;
using Serilog.Sinks.Http;
using Serilog.Sinks.Http.Private.NonDurable;

namespace Serilog.Support.Reflection;

public class HttpSinkReflection
{
    private readonly HttpSink sink;

    public HttpSinkReflection(Logger logger)
    {
        sink = logger.GetSink<HttpSink>();
    }

    public HttpSinkReflection SetRequestUri(string requestUri)
    {
        sink.SetNonPublicInstanceField("requestUri", requestUri);
        return this;
    }

    public HttpSinkReflection SetHttpClient(IHttpClient httpClient)
    {
        sink.SetNonPublicInstanceField("httpClient", httpClient);
        return this;
    }
}