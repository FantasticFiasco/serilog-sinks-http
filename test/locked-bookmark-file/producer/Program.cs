#pragma warning disable 4014

using Serilog;
using Serilog.Debugging;

var id = Guid.NewGuid().ToString();
Console.WriteLine($"id: {id}");

SelfLog.Enable((output) => Console.WriteLine($"SelfLog: {output}"));

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    //.WriteTo.Http($"http://localhost:5283/logs/{id}", null, flushOnClose: false)
    .WriteTo.DurableHttpUsingFileSizeRolledBuffers(requestUri: $"http://localhost:5283/logs/{id}", bufferFileShared: true, flushOnClose: false)
    .CreateLogger();

Task.Run(
    async () =>
    {
        var i = 0;

        while (true)
        {
            var message = $"Log event {i}";

            SelfLog.WriteLine(message);
            Log.Information(message);

            await Task.Delay(TimeSpan.FromSeconds(5));
            i++;
        }
    });


Console.WriteLine("Press any key to continue...");
Console.ReadKey();

Log.CloseAndFlush();
