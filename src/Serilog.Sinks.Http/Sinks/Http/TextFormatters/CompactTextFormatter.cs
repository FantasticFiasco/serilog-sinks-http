// Copyright 2015-2025 Serilog Contributors
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

using System.Collections.Generic;
using System.IO;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using Serilog.Parsing;

namespace Serilog.Sinks.Http.TextFormatters;

/// <summary>
/// JSON formatter serializing log events with minimizing size as a priority and normalizing
/// its data. The lack of a rendered message means even smaller network load compared to
/// <see cref="CompactRenderedTextFormatter"/> and should be used in situations where bandwidth
/// is of importance. Often this formatter is complemented with a log server that is capable of
/// rendering the messages of the incoming log events.
/// </summary>
/// <seealso cref="NormalTextFormatter" />
/// <seealso cref="NormalRenderedTextFormatter" />
/// <seealso cref="CompactRenderedTextFormatter" />
/// <seealso cref="NamespacedTextFormatter" />
/// <seealso cref="ITextFormatter" />
public class CompactTextFormatter : NormalTextFormatter
{
    /// <summary>
    /// Default Constructor used to setup tag names
    /// </summary>
    public CompactTextFormatter()
    {
        TimestampTag = "@t";
        MessageTemplateTag = "@mt";
        RenderedMessageTag = "@m";
        LogLevelTag = "@l";
        ExceptionTag = "@x";
        TraceIdTag = "@tr";
        SpanIdTag = "@sp";
        RenderingsTag = "@r";
    }

    /// <summary>
    /// Writes the Trace ID to the output.
    /// </summary>
    /// <param name="logEvent">The event to format.</param>
    /// <param name="output">The output.</param>
    protected override void WriteTraceId(LogEvent logEvent, TextWriter output) =>
        Write(TraceIdTag, logEvent.TraceId?.ToHexString() ?? "", output);

    /// <summary>
    /// Writes the Span ID to the output.
    /// </summary>
    /// <param name="logEvent">The event to format.</param>
    /// <param name="output">The output.</param>
    protected override void WriteSpanId(LogEvent logEvent, TextWriter output) =>
        Write(SpanIdTag, logEvent.SpanId?.ToHexString() ?? "", output);

    /// <summary>
    /// Writes the collection of properties to the output.
    /// </summary>
    /// <param name="properties">The collection of log properties.</param>
    /// <param name="output">The output.</param>
    protected override void WriteProperties(
        IReadOnlyDictionary<string, LogEventPropertyValue> properties,
        TextWriter output)
    {
        foreach (var property in properties)
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
            ValueFormatter.Instance.Format(property.Value, output);
        }
    }

    /// <summary>
    /// Writes the items with rendering formats to the output.
    /// </summary>
    /// <param name="tokensWithFormat">The collection of tokens that have formats.</param>
    /// <param name="properties">The collection of properties to fill the tokens.</param>
    /// <param name="output">The output.</param>
    protected override void WriteRenderings(
        IEnumerable<PropertyToken> tokensWithFormat,
        IReadOnlyDictionary<string, LogEventPropertyValue> properties,
        TextWriter output)
    {
        output.Write(",\"");
        output.Write(RenderingsTag);
        output.Write("\":[");
        var delim = string.Empty;
        foreach (var r in tokensWithFormat)
        {
            output.Write(delim);
            delim = ",";
            var space = new StringWriter();
            r.Render(properties, space);
            JsonValueFormatter.WriteQuotedJsonString(space.ToString(), output);
        }
        output.Write(']');
    }
}
