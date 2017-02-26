namespace Serilog.Sinks.Http.IntegrationTests.Server
{
    public class Event
    {
	    public Event(string payload)
	    {
		    Payload = payload;
	    }

	    public string Payload { get; }
    }
}
