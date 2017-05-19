// Copyright 2015-2017 Serilog Contributors
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
using System.Text;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Sinks.Http.BatchFormatters
{
    /// <summary>
    /// Formatter serializing the events payload.
    /// </summary>
    public class DefaultBatchFormatter : IBatchFormatter
    {
        private readonly long? eventBodyLimitBytes;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBatchFormatter"/> class.
        /// </summary>
        /// <param name="eventBodyLimitBytes">
        /// The maximum size, in bytes, that the JSON representation of an event may take before it
        /// is dropped rather than being sent to the server. Specify null for no limit. Default
        /// value is 265 KB.
        /// </param>
        public DefaultBatchFormatter(long? eventBodyLimitBytes = 256 * 1024)
        {
            this.eventBodyLimitBytes = eventBodyLimitBytes;
        }

        /// <summary>
        /// Format the log events into a payload.
        /// </summary>
        /// <param name="logEvents">
        /// The events to format.
        /// </param>
        /// <param name="formatter">
        /// The formatter turning the log events into a textual representation.
        /// </param>
        /// <param name="output">
        /// The payload to send over the network.
        /// </param>
        public void Format(IEnumerable<LogEvent> logEvents, ITextFormatter formatter, TextWriter output)
        {
            if (logEvents == null)
                throw new ArgumentNullException(nameof(logEvents));
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            IEnumerable<string> formattedLogEvents = logEvents.Select(
                logEvent =>
                {
                    var writer = new StringWriter();
                    formatter.Format(logEvent, writer);
                    return writer.ToString();
                });

            Format(formattedLogEvents, output);
        }

        /// <summary>
        /// Format the log events into a payload.
        /// </summary>
        /// <param name="logEvents">
        /// The events to format.
        /// </param>
        /// <param name="output">
        /// The payload to send over the network.
        /// </param>
        public void Format(IEnumerable<string> logEvents, TextWriter output)
        {
            if (logEvents == null)
                throw new ArgumentNullException(nameof(logEvents));
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            output.Write("{\"events\":[");

            var delimStart = string.Empty;

            foreach (var logEvent in logEvents)
            {
                if (string.IsNullOrWhiteSpace(logEvent))
                {
                    continue;
                }

                if (CheckEventBodySize(logEvent))
                {
                    output.Write(delimStart);
                    output.Write(logEvent);
                    delimStart = ",";
                }
            }

            output.Write("]}");
        }

        private bool CheckEventBodySize(string json)
        {
            if (eventBodyLimitBytes.HasValue &&
                Encoding.UTF8.GetByteCount(json) > eventBodyLimitBytes.Value)
            {
                SelfLog.WriteLine(
                    "Event JSON representation exceeds the byte size limit of {0} set for this sink and will be dropped; data: {1}",
                    eventBodyLimitBytes,
                    json);

                return false;
            }

            return true;
        }
    }
}