// Copyright 2015-2018 Serilog Contributors
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

using System.IO;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Sinks.Http.TextFormatters
{
    /// <summary>
    /// JSON formatter serializing log events into a format where the message properties are placed into
    /// their own namespace. It is designed for the micro-service architecture to reduce the risk of
    /// different services sending log events with identical property names but different types,
    /// which the Elastic Stack doesn't support.
    /// </summary>
    /// <seealso cref="NormalTextFormatter" />
    /// <seealso cref="NormalRenderedTextFormatter" />
    /// <seealso cref="CompactTextFormatter" />
    /// <seealso cref="CompactRenderedTextFormatter" />
    /// <seealso cref="ITextFormatter" />
    public abstract class NamespacedTextFormatter : ITextFormatter
    {
        /// <summary>
        /// Gets the component name, which will be serialized into a sub-property of 'Properties'
        /// in the JSON document.
        /// </summary>
        protected abstract string Component { get; }

        /// <summary>
        /// Gets the sub-component name, which will be serialized into a sub-property of
        /// <see cref="Component"/> in the JSON document. If value is null or an empty string it
        /// will be omitted from the serialized JSON document.
        /// </summary>
        protected abstract string SubComponent { get; }

        /// <summary>
        /// Gets a value indicating whether the message is rendered into JSON.
        /// </summary>
        protected abstract bool IsRenderingMessage { get; }

        /// <summary>
        /// Format the log event into the output.
        /// </summary>
        /// <param name="logEvent">The event to format.</param>
        /// <param name="output">The output.</param>
        public void Format(LogEvent logEvent, TextWriter output)
        {
            throw new System.NotImplementedException();
        }
    }
}
