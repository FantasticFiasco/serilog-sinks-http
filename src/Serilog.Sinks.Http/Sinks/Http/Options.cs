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

namespace Serilog.Sinks.Http
{
	/// <summary>
	/// Class describing the options for a HTTP sink.
	/// </summary>
	public class Options
	{
		/// <summary>
		/// Gets or sets the maximum number of events to post in a single batch. Default value is
		/// 1000.
		/// </summary>
		public int BatchPostingLimit { get; set; } = 1000;

		/// <summary>
		/// Gets or sets the time to wait between checking for event batches. Default value is 2
		/// seconds.
		/// </summary>
		public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(2);

		/// <summary>
		/// Gets or sets the maximum size, in bytes, that the JSON representation of an event may
		/// take before it is dropped rather than being sent to the server. Specify null for no limit.
		/// Default value is 265 KB.
		/// </summary>
		public long? EventBodyLimitBytes { get; set; } = 256 * 1024;

		/// <summary>
		/// Gets or sets the formatting type. Default value is
		/// <see cref="Http.FormattingType.NormalRendered"/>.
		/// </summary>
		public FormattingType FormattingType { get; set; } = FormattingType.NormalRendered;

		/// <summary>
		/// Gets or sets the basic authentication user. Default value is empty string
		/// </summary>
		public String User { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the basic authentication password. Default value is empty string
		/// </summary>
		public String Password { get; set; } = string.Empty;
	}
}
