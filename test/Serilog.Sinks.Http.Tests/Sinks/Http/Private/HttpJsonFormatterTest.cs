using System;
using System.IO;
using Newtonsoft.Json.Linq;
using Serilog.Support;
using Xunit;

namespace Serilog.Sinks.Http.Private
{
    public class HttpJsonFormatterTest
    {
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

		private static void AssertValidJson(Action<ILogger> act)
		{
			var output = new StringWriter();
			var formatter = new HttpJsonFormatter();
			var log = new LoggerConfiguration()
				.WriteTo.Sink(new TextWriterSink(output, formatter))
				.CreateLogger();

			act(log);

			var json = output.ToString();

			// Unfortunately this will not detect all JSON formatting issues; better than nothing
			// however
			JObject.Parse(json);
		}
	}
}
