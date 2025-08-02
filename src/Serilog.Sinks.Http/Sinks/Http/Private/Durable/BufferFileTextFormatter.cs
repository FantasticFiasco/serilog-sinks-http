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

using System;
using System.IO;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Sinks.Http.Private.Durable;

/// <summary>
/// This is a wrapper around the passed in <see cref="ITextFormatter"/>. This will ensure
/// that the log event is formatted and written to the output if the formatting was successful.
/// The main point here is that each entry is written to the output on a new line.
/// </summary>
/// <seealso cref="ITextFormatter" />
public class BufferFileTextFormatter : ITextFormatter
{
    private readonly ITextFormatter textFormatter;

    public BufferFileTextFormatter(ITextFormatter textFormatter)
    {
        this.textFormatter = textFormatter;
    }

    /// <summary>
    /// Format the log event into the output.
    /// </summary>
    /// <param name="logEvent">The event to format.</param>
    /// <param name="output">The output.</param>
    public void Format(LogEvent logEvent, TextWriter output)
    {
        var buffer = new StringWriter();
        textFormatter.Format(logEvent, buffer);

        var logEntry = buffer.ToString();

        // If formatting was successful, write to output
        if (logEntry.Length > 0)
        {
            output.Write(logEntry);
            if (!logEntry.EndsWith(Environment.NewLine))
            {
                output.WriteLine();
            }
        }
    }
}
