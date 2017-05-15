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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Sinks.Http.BatchedTextFormatters
{
    /// <summary>
    /// Formatter serializing the events payload.
    /// </summary>
    public class DefaultBatchedTextFormatter : IBatchedTextFormatter
    {
        private readonly ITextFormatter textFormatter;

        private readonly long? eventBodyLimitBytes;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBatchedTextFormatter"/> class.
        /// </summary>
        /// <param name="eventBodyLimitBytes">
        /// The maximum size, in bytes, that the JSON representation of an event may take before it
        /// is dropped rather than being sent to the server. Specify null for no limit. Default
        /// value is 265 KB.
        /// </param>
        /// <param name="textFormatter">
        /// Formatter used to format individual event.
        /// </param>
        public DefaultBatchedTextFormatter(
            long? eventBodyLimitBytes = 256 * 1024, 
            ITextFormatter textFormatter = null)
        {
            this.textFormatter = textFormatter;
            this.eventBodyLimitBytes = eventBodyLimitBytes;
        }

        /// <summary>
        /// Format deserialized events to send.
        /// </summary>
        /// <param name="logEvents">
        /// Deserialized events.
        /// </param>
        /// <param name="output">
        /// Payload to send.
        /// </param>
        public void Format(IEnumerable<LogEvent> logEvents, TextWriter output)
        {
            if (textFormatter == null)
                throw new ArgumentNullException(nameof(textFormatter));

            Format(logEvents.Select(e =>
            {
                StringWriter stringWriter = new StringWriter();
                textFormatter.Format(e, stringWriter);
                return stringWriter.ToString();
            }), output);
        }

        /// <summary>
        /// Format serialized events to send.
        /// </summary>
        /// <param name="logEvents">
        /// Serialized events.
        /// </param>
        /// <param name="output">
        /// Payload to send.
        /// </param>
        public void Format(IEnumerable<string> logEvents, TextWriter output)
        {
            output.Write("{\"events\":[");

            var delimStart = string.Empty;

            foreach (var logEvent in logEvents)
            {
                var json = logEvent;

                if (CheckEventBodySize(json))
                {
                    output.Write(delimStart);
                    output.Write(json);
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