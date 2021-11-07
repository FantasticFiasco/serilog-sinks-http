namespace Serilog.Sinks.HttpTests.LogServer.Services
{
    public class LogEventService
    {
        public string[] GetLogEvents()
        {
            return new[]
            {
                "Log events 1", "Log events 2", "Log events 3", "Log events 4", "Log events 5"
            };
        }
    }
}
