using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Serilog.Sinks.Http.LogServer;

namespace Serilog.LogServer
{
    public abstract class TestServerFixture : IDisposable
    {
        protected TestServerFixture() =>
            Server = new TestServer(new WebHostBuilder().UseStartup<Startup>());

        protected TestServer Server { get; }

        protected EventService EventService =>
            (EventService)Server.Host.Services.GetService(typeof(EventService));

        protected NetworkService NetworkService =>
            (NetworkService)Server.Host.Services.GetService(typeof(NetworkService));

        public virtual void Dispose() =>
            Server?.Dispose();
    }
}
