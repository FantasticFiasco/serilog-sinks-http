// Copyright 2015-2026 Serilog Contributors
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using Serilog.Parsing;

namespace Serilog.Sinks.Http.TextFormatters;

/// <summary>
/// JSON formatter serializing log events into a format where the message properties are placed
/// into their own namespace. It is designed for a micro-service architecture where one wish to
/// reduce the risk of having multiple services sending log events with identical property
/// names but different value types, something that is unsupported by the Elastic Stack.
/// </summary>
/// <seealso cref="NormalTextFormatter" />
/// <seealso cref="NormalRenderedTextFormatter" />
/// <seealso cref="CompactTextFormatter" />
/// <seealso cref="CompactRenderedTextFormatter" />
/// <seealso cref="ITextFormatter" />
public abstract class NamespacedTextFormatter : NormalTextFormatter
{
    private readonly string component;
    private readonly string? subComponent;

    private bool isWritingProperties = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="NamespacedTextFormatter"/> class.
    /// </summary>
    /// <param name="component">
    /// The component name, which will be serialized into a sub-property of "Properties" in the
    /// JSON document.
    /// </param>
    /// <param name="subComponent">
    /// The sub-component name, which will be serialized into a sub-property of
    /// <paramref name="component"/> in the JSON document. If value is <see langword="null"/> it
    /// will be omitted from the serialized JSON document, and the message properties will be
    /// serialized as properties of <paramref name="component"/>. Default value is
    /// <see langword="null"/>.
    /// </param>
    protected NamespacedTextFormatter(string component, string? subComponent = null)
    {
        this.component = component ?? throw new ArgumentNullException(nameof(component));
        this.subComponent = subComponent;
        IsRenderingMessage = true;
    }

    /// <inheritdoc />
    protected override void WriteProperties(LogEvent logEvent, TextWriter output)
    {
        isWritingProperties = true;
        output.Write(DELIMITER);
        JsonValueFormatter.WriteQuotedJsonString(PropertiesKey, output);
        output.Write(SEPARATOR);
        output.Write("{");

        var messageTemplateProperties = logEvent.Properties
            .Where(property => logEvent.MessageTemplate.Tokens
                .Any(token => token is PropertyToken namedToken && namedToken.PropertyName == property.Key)            )
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (messageTemplateProperties.Count > 0)
        {
            WriteOpenNamespace(output);

            WritePropertiesValues(messageTemplateProperties, output);

            var tokensWithFormat = GetTokensWithFormat(logEvent);

            // ReSharper disable once PossibleMultipleEnumeration
            if (tokensWithFormat.Any())
            {
                // ReSharper disable once PossibleMultipleEnumeration
                WriteRenderings(tokensWithFormat, logEvent.Properties, output);
            }

            WriteCloseNamespace(output);
        }

        var enrichedProperties = logEvent.Properties
            .Except(messageTemplateProperties)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (enrichedProperties.Count > 0)
        {
            if (messageTemplateProperties.Count > 0)
            {
                output.Write(DELIMITER);
            }

            WritePropertiesValues(enrichedProperties, output);
        }

        output.Write('}');
        isWritingProperties = false;
    }

    private void WriteOpenNamespace(TextWriter output)
    {
        JsonValueFormatter.WriteQuotedJsonString(component, output);
        output.Write(SEPARATOR);
        output.Write("{");
        if (subComponent != null)
        {
            JsonValueFormatter.WriteQuotedJsonString(subComponent, output);
            output.Write(SEPARATOR);
            output.Write("{");
        }
    }

    private void WriteCloseNamespace(TextWriter output)
    {
        output.Write("}");
        if (subComponent != null)
        {
            output.Write("}");
        }
    }

    /// <inheritdoc />
    protected override void WriteRenderings(
        IEnumerable<IGrouping<string, PropertyToken>> tokensGrouped,
        IReadOnlyDictionary<string, LogEventPropertyValue> properties,
        TextWriter output)
    {
        // Only write the renderings during the properties phase as they need to be
        // encapsulated with the namespace
        if (isWritingProperties)
        {
            base.WriteRenderings(tokensGrouped, properties, output);
        }
    }
}
