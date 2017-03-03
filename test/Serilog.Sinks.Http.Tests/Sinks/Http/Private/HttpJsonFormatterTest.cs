using System;
using System.IO;
using Newtonsoft.Json.Linq;
using Serilog.Support;
using Xunit;

namespace Serilog.Sinks.Http.Private
{
    public class HttpJsonFormatterTest
    {
	    private readonly ILogger logger;
	    private readonly StringWriter output;

		public HttpJsonFormatterTest()
	    {
			output = new StringWriter();
			var formatter = new HttpJsonFormatter();
			logger = new LoggerConfiguration()
				.WriteTo.Sink(new TextWriterSink(output, formatter))
				.CreateLogger();
		}

		[Fact]
		public void EmptyEvent()
		{
			AssertValidJson(log => log.Information("No properties"));
		}

		[Fact]
		public void MinimalEvent()
		{
			AssertValidJson(log => log.Information("One {Property}", 42));
		}

		[Fact]
		public void MultipleProperties()
		{
			AssertValidJson(log => log.Information("Property {First} and {Second}", "One", "Two"));
		}

		[Fact]
		public void Exceptions()
		{
			AssertValidJson(log => log.Information(new DivideByZeroException(), "With exception"));
		}

		[Fact]
		public void ExceptionAndProperties()
		{
			AssertValidJson(log => log.Information(new DivideByZeroException(), "With exception and {Property}", 42));
		}

		[Fact]
		public void Renderings()
		{
			AssertValidJson(log => log.Information("One {Rendering:x8}", 42));
		}

		[Fact]
		public void MultipleRenderings()
		{
			AssertValidJson(log => log.Information("Rendering {First:x8} and {Second:x8}", 1, 2));
		}

	    [Fact]
	    public void NastyException()
	    {
			AssertIsDropped(log => log.Information(new NastyException(), "With exception"));
		}

		private void AssertValidJson(Action<ILogger> act)
		{
			// Act
			act(logger);

			// Assert - Unfortunately this will not detect all JSON formatting issues; better than
			// nothing however
			JObject.Parse(output.ToString());
		}

	    private void AssertIsDropped(Action<ILogger> act)
	    {
			// Act
			act(logger);

			// Assert
			Assert.Equal(string.Empty, output.ToString());
		}
	}
}
