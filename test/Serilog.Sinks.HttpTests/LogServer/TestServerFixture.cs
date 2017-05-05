using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Serilog.Sinks.Http.IntegrationTests.Server;

namespace Serilog.LogServer
{
	public abstract class TestServerFixture : IDisposable
	{
		protected TestServerFixture()
		{
			Server = new TestServer(new WebHostBuilder()
				.UseStartup<Startup>());

			Api = new ApiModel(Server.CreateClient());
		}

		protected TestServer Server { get; }

		protected ApiModel Api { get; }

		public virtual void Dispose()
		{
			Server?.Dispose();
		}
	}
}
