using Serilog.Sinks.HttpTests.LogServer;

var builder = Host
    .CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
    });

using var app = builder.Build();
app.Run();
