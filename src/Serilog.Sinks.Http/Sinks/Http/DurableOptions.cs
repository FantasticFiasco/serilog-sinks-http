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

namespace Serilog.Sinks.Http
{
	/// <summary>
	/// Class describing the options for a durable HTTP sink.
	/// </summary>
	public class DurableOptions : Options
	{
		/// <summary>
		/// Gets or sets the path for a set of files that will be used to buffer events until they
		/// can be successfully transmitted across the network. Individual files will be created
		/// using the pattern <see cref="BufferBaseFilename"/>-{Date}.json. Default value is
		/// 'Buffer'.
		/// </summary>
		public string BufferBaseFilename { get; set; } = "Buffer";

		/// <summary>
		/// Gets or sets the maximum size, in bytes, to which the buffer log file for a specific date
		/// will be allowed to grow. By default no limit will be applied.
		/// </summary>
		public long? BufferFileSizeLimitBytes { get; set; }
	}
}
