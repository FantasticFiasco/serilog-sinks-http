//using Microsoft.Extensions.Configuration;
//using Serilog.Core;
//using Serilog.Support.Fixtures;
//using Xunit;

//namespace Serilog
//{
//    // TODO: Add test that congiguration is passed to HTTP client
//
//    public class DurableHttpSinkUsingFileSizeRolledBuffersGivenAppSettingsShould
//        : SinkFixture, IClassFixture<WebServerFixture>
//    {
//        public DurableHttpSinkUsingFileSizeRolledBuffersGivenAppSettingsShould(WebServerFixture webServerFixture)
//            : base(webServerFixture)
//        {
//            var configuration = new ConfigurationBuilder()
//                .AddJsonFile("appsettings_durable_http_using_file_size_rolled_buffers.json")
//                .Build();

//            Logger = new LoggerConfiguration()
//                .ReadFrom.Configuration(configuration)
//                .CreateLogger();
//        }

//        protected override Logger Logger { get; }
//    }
//}
