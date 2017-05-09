using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Serilog.Sinks.Http.LogServer;

namespace Serilog.LogServer
{
	public abstract class TestServerFixture : IDisposable
	{
		protected TestServerFixture()
		{
			Server = new TestServer(new WebHostBuilder()
				.UseStartup<Startup>());
		}

		protected TestServer Server { get; }

	    protected IEventService EventService => (IEventService)Server.Host.Services.GetService(typeof(IEventService));

	    public virtual void Dispose()
		{
			Server?.Dispose();
		}
	}
}
