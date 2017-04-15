// Copyright 2015-2016 Serilog Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using System.Linq;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using Serilog.Parsing;

namespace Serilog.Sinks.Http.Private.Formatters
{
    /// <summary>
    /// JSON formatter serializing objects into a compact format.
    /// </summary>
    /// <seealso cref="ITextFormatter" />
    /// <seealso cref="NormalJsonFormatter" />
    public class CompactJsonFormatter : ITextFormatter
	{
		private static readonly JsonValueFormatter ValueFormatter = new JsonValueFormatter();

		private readonly bool isRenderingMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactJsonFormatter"/> class.
        /// </summary>
        /// <param name="isRenderingMessage">
        /// Whether message should be rendered during serialization.
        /// </param>
        public CompactJsonFormatter(bool isRenderingMessage)
		{
			this.isRenderingMessage = isRenderingMessage;
		}

        /// <summary>
        /// Format the log event into the output.
        /// </summary>
        /// <param name="logEvent">The event to format.</param>
        /// <param name="output">The output.</param>
        public void Format(LogEvent logEvent, TextWriter output)
		{
			try
			{
				var buffer = new StringWriter();
				FormatContent(logEvent, buffer);

				// If formatting was successful, write to output
				output.WriteLine(buffer.ToString());
			}
			catch (Exception e)
			{
				LogNonFormattableEvent(logEvent, e);
			}
		}

		private void FormatContent(LogEvent logEvent, TextWriter output)
		{
			if (logEvent == null)
				throw new ArgumentNullException(nameof(logEvent));
			if (output == null)
				throw new ArgumentNullException(nameof(output));

			output.Write("{\"@t\":\"");
			output.Write(logEvent.Timestamp.UtcDateTime.ToString("o"));

			output.Write("\",\"@mt\":");
			JsonValueFormatter.WriteQuotedJsonString(logEvent.MessageTemplate.Text, output);

			if (isRenderingMessage)
			{
				output.Write(",\"@m\":");
				var message = logEvent.MessageTemplate.Render(logEvent.Properties);
				JsonValueFormatter.WriteQuotedJsonString(message, output);
			}

			var tokensWithFormat = logEvent.MessageTemplate.Tokens
				.OfType<PropertyToken>()
				.Where(pt => pt.Format != null);

			// Better not to allocate an array in the 99.9% of cases where this is false
			// ReSharper disable once PossibleMultipleEnumeration
			if (tokensWithFormat.Any())
			{
				output.Write(",\"@r\":[");
				var delim = "";
				foreach (var r in tokensWithFormat)
				{
					output.Write(delim);
					delim = ",";
					var space = new StringWriter();
					r.Render(logEvent.Properties, space);
					JsonValueFormatter.WriteQuotedJsonString(space.ToString(), output);
				}
				output.Write(']');
			}

			if (logEvent.Level != LogEventLevel.Information)
			{
				output.Write(",\"@l\":\"");
				output.Write(logEvent.Level);
				output.Write('\"');
			}

			if (logEvent.Exception != null)
			{
				output.Write(",\"@x\":");
				JsonValueFormatter.WriteQuotedJsonString(logEvent.Exception.ToString(), output);
			}

			foreach (var property in logEvent.Properties)
			{
				var name = property.Key;
				if (name.Length > 0 && name[0] == '@')
				{
					// Escape first '@' by doubling
					name = '@' + name;
				}

				output.Write(',');
				JsonValueFormatter.WriteQuotedJsonString(name, output);
				output.Write(':');
				ValueFormatter.Format(property.Value, output);
			}

			output.Write('}');
		}

		private static void LogNonFormattableEvent(LogEvent logEvent, Exception e)
		{
			SelfLog.WriteLine(
				"Event at {0} with message template {1} could not be formatted into JSON and will be dropped: {2}",
				logEvent.Timestamp.ToString("o"),
				logEvent.MessageTemplate.Text,
				e);
		}
	}
}
