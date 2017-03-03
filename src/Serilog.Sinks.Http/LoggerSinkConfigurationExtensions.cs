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
using Serilog.Sinks.Http.Private;

namespace Serilog
{
	/// <summary>
	/// Adds the WriteTo.Http() and WriteTo.DurableHttp() extension method to
	/// <see cref="LoggerConfiguration"/>.
	/// </summary>
	public static class LoggerSinkConfigurationExtensions
	{
		/// <summary>
		/// Adds a sink that sends volatile log events using HTTP POST over the network.
		/// </summary>
		/// <param name="sinkConfiguration">The logger configuration.</param>
		/// <param name="options">The sink options.</param>
		/// <param name="restrictedToMinimumLevel">
		/// The minimum level for events passed through the sink. The default is
		/// <see cref="LevelAlias.Minimum"/>.
		/// </param>
		/// <param name="httpClient">
		/// A custom <see cref="IHttpClient"/> implementation. Default value is
		/// <see cref="HttpClient"/>.
		/// </param>
		///  <returns>Logger configuration, allowing configuration to continue.</returns>
		public static LoggerConfiguration Http(
			this LoggerSinkConfiguration sinkConfiguration,
			Options options,
			LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
			IHttpClient httpClient = null)
		{
			if (sinkConfiguration == null)
				throw new ArgumentNullException(nameof(sinkConfiguration));
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			var sink = new HttpSink(
				httpClient ?? new HttpClientWrapper(),
				options);

			return sinkConfiguration.Sink(sink, restrictedToMinimumLevel);
		}

		/// <summary>
		/// Adds a sink that sends durable log events using HTTP POST over the network.
		/// </summary>
		/// <param name="sinkConfiguration">The logger configuration.</param>
		/// <param name="options">The sink options.</param>
		/// <param name="restrictedToMinimumLevel">
		/// The minimum level for events passed through the sink. The default is
		/// <see cref="LevelAlias.Minimum"/>.
		/// </param>
		/// <param name="httpClient">
		/// A custom <see cref="IHttpClient"/> implementation. Default value is
		/// <see cref="HttpClient"/>.
		/// </param>
		/// <returns>Logger configuration, allowing configuration to continue.</returns>
		public static LoggerConfiguration DurableHttp(
			this LoggerSinkConfiguration sinkConfiguration,
			DurableOptions options,
			LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
			IHttpClient httpClient = null)
		{
			if (sinkConfiguration == null)
				throw new ArgumentNullException(nameof(sinkConfiguration));
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			var sink = new DurableHttpSink(
				httpClient ?? new HttpClientWrapper(),
				options);

			return sinkConfiguration.Sink(sink, restrictedToMinimumLevel);
		}
	}
}
