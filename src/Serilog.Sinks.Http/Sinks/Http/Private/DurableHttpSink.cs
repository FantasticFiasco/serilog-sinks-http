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
using System.Text;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.RollingFile;

namespace Serilog.Sinks.Http.Private
{
	internal class DurableHttpSink : ILogEventSink, IDisposable
	{
		private readonly HttpLogShipper shipper;
		private readonly RollingFileSink sink;

		public DurableHttpSink(
			IHttpClient client,
			DurableOptions options)
		{
			if (options.BufferFileSizeLimitBytes.HasValue && options.BufferFileSizeLimitBytes < 0)
				throw new ArgumentOutOfRangeException(nameof(options.BufferFileSizeLimitBytes), "Negative value provided; file size limit must be non-negative.");

			shipper = new HttpLogShipper(
				client,
				options.RequestUri,
				options.BufferBaseFilename,
				options.BatchPostingLimit,
				options.Period,
				options.EventBodyLimitBytes);

			sink = new RollingFileSink(
				options.BufferBaseFilename + "-{Date}.json",
				new CompactJsonFormatter(),
				options.BufferFileSizeLimitBytes,
				null,
				Encoding.UTF8);
		}

		public void Emit(LogEvent logEvent)
		{
			sink.Emit(logEvent);
		}

		public void Dispose()
		{
			sink.Dispose();
			shipper.Dispose();
		}
	}
}