﻿using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Serilog.Sinks.Http.IntegrationTests.Server;
using IOFile = System.IO.File;

namespace Serilog
{
	public abstract class TestServerFixture
	{
		protected TestServerFixture()
		{
			Server = new TestServer(new WebHostBuilder()
				.UseStartup<Startup>());

			Api = new ApiModel(Server.CreateClient());
		}

		protected TestServer Server { get; }

		protected ApiModel Api { get; }

		protected void ClearBufferFiles()
		{
			var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "Buffer*")
				.ToArray();

			foreach (var file in files)
			{
				IOFile.Delete(file);
			}
		}
	}
}
