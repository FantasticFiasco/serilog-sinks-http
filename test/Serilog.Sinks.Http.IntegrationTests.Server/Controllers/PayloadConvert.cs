namespace Serilog.Sinks.Http.IntegrationTests.Server.Controllers
{
	public static class PayloadConvert
	{
		public static Event FromDto(EventDto @event)
		{
			return new Event(
				@event.Timestamp,
				@event.Level,
				@event.MessageTemplate,
				@event.RenderedMessage);
		}

		public static EventDto ToDto(Event @event)
		{
			return new EventDto
			{
				Timestamp = @event.Timestamp,
				Level = @event.Level,
				MessageTemplate = @event.MessageTemplate,
				RenderedMessage = @event.RenderedMessage
			};
		}
	}
}
