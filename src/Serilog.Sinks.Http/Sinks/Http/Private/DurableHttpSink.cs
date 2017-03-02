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
using Serilog.Formatting.Json;
using Serilog.Sinks.RollingFile;

namespace Serilog.Sinks.Http.Private
{
	internal class DurableHttpSink : ILogEventSink, IDisposable
	{
		private readonly HttpLogShipper shipper;
		private readonly RollingFileSink sink;

		public DurableHttpSink(
			IHttpClient client,
			string requestUri,
			string bufferBaseFilename,
			int batchPostingLimit,
			TimeSpan period,
			long? bufferFileSizeLimitBytes,
			long? eventBodyLimitBytes)
		{
			if (bufferFileSizeLimitBytes.HasValue && bufferFileSizeLimitBytes < 0)
				throw new ArgumentOutOfRangeException(nameof(bufferFileSizeLimitBytes), "Negative value provided; file size limit must be non-negative.");

			shipper = new HttpLogShipper(
				client,
				requestUri,
				bufferBaseFilename,
				batchPostingLimit,
				period,
				eventBodyLimitBytes);

			sink = new RollingFileSink(
				bufferBaseFilename + "-{Date}.json",
				new JsonFormatter(renderMessage: true),
				bufferFileSizeLimitBytes,
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