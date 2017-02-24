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
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.Http;
using Serilog.Sinks.Http.Private;

namespace Serilog
{
    /// <summary>
    /// Adds the WriteTo.Http() extension method to <see cref="LoggerConfiguration"/>.
    /// </summary>
    public static class LoggerSinkConfigurationExtensions
    {
        /// <summary>
        /// Adds a sink that sends log events using HTTP POST over the network.
        /// </summary>
        /// <param name="sinkConfiguration">The logger configuration.</param>
        /// <param name="requestUri">The URI the request is sent to.</param>
        /// <param name="batchPostingLimit">
        /// The maximum number of events to post in a single batch. The default is
        /// <see cref="HttpSink.DefaultBatchPostingLimit"/>.
        /// </param>
        /// <param name="period">
        /// The time to wait between checking for event batches. The default is
        /// <see cref="HttpSink.DefaultPeriod"/>.
        /// </param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="restrictedToMinimumLevel">
        /// The minimum level for events passed through the sink. The default is
        /// <see cref="LevelAlias.Minimum"/>.
        /// </param>
        /// <param name="httpClient">
        /// A custom <see cref="IHttpClient"/> implementation.
        /// </param>
        ///  <returns>Logger configuration, allowing configuration to continue.</returns>
        public static LoggerConfiguration Http(
            this LoggerSinkConfiguration sinkConfiguration,
            string requestUri,
            int? batchPostingLimit = null,
            TimeSpan? period = null,
            IFormatProvider formatProvider = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            IHttpClient httpClient = null)
        {
            if (sinkConfiguration == null)
                throw new ArgumentNullException(nameof(sinkConfiguration));

            var client = httpClient ?? new HttpClientWrapper();

            var sink = new HttpSink(
                client,
                requestUri,
                batchPostingLimit ?? HttpSink.DefaultBatchPostingLimit,
                period ?? HttpSink.DefaultPeriod,
                formatProvider);

            return sinkConfiguration.Sink(sink, restrictedToMinimumLevel);
        }
    }
}
