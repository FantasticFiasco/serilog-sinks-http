using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Serilog.ApiModels;
using Serilog.Sinks.Http.IntegrationTests.Server;


namespace Serilog
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
