// Copyright 2015-2020 Serilog Contributors
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

namespace Serilog.Sinks.Http.BatchFormatters
{
    /// <summary>
    /// Formatter serializing batches of log events into a JSON object under a property called "events".
    /// <para/>
    /// Example:
    /// {
    ///   "events": [
    ///     { event n },
    ///     { event n+1 }
    ///   ]
    /// }
    /// </summary>
    public class DefaultBatchFormatter : IBatchFormatter
    {
        /// <inheritdoc />
        public void Format(IEnumerable<string> logEvents, TextWriter output)
        {
            if (logEvents == null) throw new ArgumentNullException(nameof(logEvents));
            if (output == null) throw new ArgumentNullException(nameof(output));

            // Abort if sequence of log events is empty
            if (!logEvents.Any())
            {
                return;
            }

            output.Write("{\"events\":[");

            var delimStart = string.Empty;

            foreach (var logEvent in logEvents)
            {
                if (string.IsNullOrWhiteSpace(logEvent))
                {
                    continue;
                }

                output.Write(delimStart);
                output.Write(logEvent);
                delimStart = ",";
            }

            output.Write("]}");
        }
    }
}
