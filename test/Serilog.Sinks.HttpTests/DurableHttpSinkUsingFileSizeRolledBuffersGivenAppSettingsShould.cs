﻿using Microsoft.Extensions.Configuration;
using Serilog.Core;

namespace Serilog
{
    public class DurableHttpSinkUsingFileSizeRolledBuffersGivenAppSettingsShould : SinkFixture
    {
        public DurableHttpSinkUsingFileSizeRolledBuffersGivenAppSettingsShould()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings_durable_http_using_file_size_rolled_buffers.json")
                .Build();

            Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            Configuration = configuration;
        }

        protected override Logger Logger { get; }

        protected override IConfiguration Configuration { get; }
    }
}
