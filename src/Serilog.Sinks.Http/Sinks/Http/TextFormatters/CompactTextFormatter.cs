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
        TimestampKey = "@t";
        MessageTemplateKey = "@mt";
        RenderedMessageKey = "@m";
        LevelKey = "@l";
        ExceptionKey = "@x";
        TraceIdKey = "@tr";
        SpanIdKey = "@sp";
        RenderingsKey = "@r";
    }

    /// <inheritdoc />
    protected override void WriteLogLevel(LogEvent logEvent, TextWriter output)
    {
        if (logEvent.Level != LogEventLevel.Information)
        {
            base.WriteLogLevel(logEvent, output);
        }
    }

    /// <inheritdoc />
    protected override void WriteTraceId(LogEvent logEvent, TextWriter output) =>
        WriteProperty(TraceIdKey, logEvent.TraceId?.ToHexString() ?? "", output);

    /// <inheritdoc />
    protected override void WriteSpanId(LogEvent logEvent, TextWriter output) =>
        WriteProperty(SpanIdKey, logEvent.SpanId?.ToHexString() ?? "", output);

    /// <inheritdoc />
    protected override void WritePropertyValue(
        string key,
        LogEventPropertyValue value,
        TextWriter output)
    {
        if (key.Length > 0 && key[0] == '@')
        {
            // Escape first '@' by doubling
            key = '@' + key;
        }

        base.WritePropertyValue(key, value, output);
    }

    /// <inheritdoc />
    protected override void WriteProperties(LogEvent logEvent, TextWriter output)
    {
        foreach (var property in logEvent.Properties)
        {
            output.Write(DELIMITER);
            WritePropertyValue(property.Key, property.Value, output);
        }
    }

    /// <inheritdoc />
    protected override void WriteRenderings(
        IEnumerable<PropertyToken> tokensWithFormat,
        IReadOnlyDictionary<string, LogEventPropertyValue> properties,
        TextWriter output)
    {
        output.Write(DELIMITER);
        JsonValueFormatter.WriteQuotedJsonString(RenderingsKey, output);
        output.Write(SEPARATOR);
        output.Write("[");
        var delim = string.Empty;
        foreach (var r in tokensWithFormat)
        {
            output.Write(delim);
            delim = DELIMITER;
            var space = new StringWriter();
            r.Render(properties, space);
            JsonValueFormatter.WriteQuotedJsonString(space.ToString(), output);
        }
        output.Write(']');
    }
}
