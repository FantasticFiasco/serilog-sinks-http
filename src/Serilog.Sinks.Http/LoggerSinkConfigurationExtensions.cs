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
using System.Net.Http;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.Http;
using Serilog.Sinks.Http.Private.Http;
using Serilog.Sinks.Http.Private.Sinks;

namespace Serilog
{
	/// <summary>
	/// Adds the WriteTo.Http() and WriteTo.DurableHttp() extension method to
	/// <see cref="LoggerConfiguration"/>.
	/// </summary>
	public static class LoggerSinkConfigurationExtensions
	{
        /// <summary>
        /// Adds a non durable sink that sends log events using HTTP POST over the network. A
        /// non-durable sink will loose data after a system or process restart.
        /// </summary>
        /// <param name="sinkConfiguration">The logger configuration.</param>
        /// <param name="requestUri">The URI the request is sent to.</param>
        /// <param name="batchPostingLimit">
        /// The maximum number of events to post in a single batch. Default value is 1000.
        /// </param>
        /// <param name="period">
        /// The time to wait between checking for event batches. Default value is 2 seconds.
        /// </param>
        /// <param name="eventBodyLimitBytes">
        /// The maximum size, in bytes, that the JSON representation of an event may take before it
        /// is dropped rather than being sent to the server. Specify null for no limit. Default
        /// value is 265 KB.
        /// </param>
        /// <param name="formattingType">
        /// The formatting type. Default value is <see cref="FormattingType.NormalRendered"/>.
        /// </param>
        /// <param name="restrictedToMinimumLevel">
        /// The minimum level for events passed through the sink. Default value is
        /// <see cref="LevelAlias.Minimum"/>.
        /// </param>
        /// <param name="httpClient">
        /// A custom <see cref="IHttpClient"/> implementation. Default value is
        /// <see cref="HttpClient"/>.
        /// </param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        public static LoggerConfiguration Http(
			this LoggerSinkConfiguration sinkConfiguration,
			string requestUri,
			int batchPostingLimit = 1000,
			TimeSpan? period = null,
			long? eventBodyLimitBytes = 256 * 1024,
			FormattingType formattingType = FormattingType.NormalRendered,
			LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
			IHttpClient httpClient = null)
		{
			if (sinkConfiguration == null)
				throw new ArgumentNullException(nameof(sinkConfiguration));

			var sink = new HttpSink(
				requestUri,
			    batchPostingLimit,
                period ?? TimeSpan.FromSeconds(2),
                eventBodyLimitBytes,
                formattingType,
			    httpClient ?? new HttpClientWrapper());

			return sinkConfiguration.Sink(sink, restrictedToMinimumLevel);
		}

        /// <summary>
        /// Adds a durable sink that sends log events using HTTP POST over the network. A durable
        /// sink will persist log events on disk before sending them over the network, thus
        ///  protecting against data loss after a system or process restart.
        /// </summary>
        /// <param name="sinkConfiguration">The logger configuration.</param>
        /// <param name="requestUri">The URI the request is sent to.</param>
        /// <param name="bufferBaseFilename">
        /// The path for a set of files that will be used to buffer events until they can be
        /// successfully transmitted across the network. Individual files will be created using the
        /// pattern <paramref name="bufferBaseFilename"/>-{Date}.json. Default value is 'Buffer'.
        /// </param>
        /// <param name="bufferFileSizeLimitBytes"></param>
        /// The maximum size, in bytes, to which the buffer log file for a specific date will be
        /// allowed to grow. By default no limit will be applied.
        /// <param name="batchPostingLimit">
        /// The maximum number of events to post in a single batch. Default value is 1000.
        /// </param>
        /// <param name="period">
        /// The time to wait between checking for event batches. Default value is 2 seconds.
        /// </param>
        /// <param name="eventBodyLimitBytes">
        /// The maximum size, in bytes, that the JSON representation of an event may take before it
        /// is dropped rather than being sent to the server. Specify null for no limit. Default
        /// value is 265 KB.
        /// </param>
        /// <param name="formattingType">
        /// The formatting type. Default value is <see cref="FormattingType.NormalRendered"/>.
        /// </param>
        /// <param name="restrictedToMinimumLevel">
        /// The minimum level for events passed through the sink. Default value is
        /// <see cref="LevelAlias.Minimum"/>.
        /// </param>
        /// <param name="httpClient">
        /// A custom <see cref="IHttpClient"/> implementation. Default value is
        /// <see cref="HttpClient"/>.
        /// </param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        public static LoggerConfiguration DurableHttp(
			this LoggerSinkConfiguration sinkConfiguration,
			string requestUri,
			string bufferBaseFilename = "Buffer",
			long? bufferFileSizeLimitBytes = null,
            int batchPostingLimit = 1000,
            TimeSpan? period = null,
            long? eventBodyLimitBytes = 256 * 1024,
            FormattingType formattingType = FormattingType.NormalRendered,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            IHttpClient httpClient = null)
		{
			if (sinkConfiguration == null)
				throw new ArgumentNullException(nameof(sinkConfiguration));

			var sink = new DurableHttpSink(
				requestUri,
				bufferBaseFilename,
				bufferFileSizeLimitBytes,
				batchPostingLimit,
				period ?? TimeSpan.FromSeconds(2),
                eventBodyLimitBytes,
                formattingType,
                httpClient ?? new HttpClientWrapper());

			return sinkConfiguration.Sink(sink, restrictedToMinimumLevel);
		}
	}
}
