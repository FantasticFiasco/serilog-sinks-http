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
	/// Enum defining how log events are formatted when sent over the network.
	/// </summary>
	public enum FormattingType
	{
		/// <summary>
		/// The log event is normally formatted and the message template is rendered into a message.
		/// This is the most verbose formatting type and its network load is higher than the other
		/// options.
		/// </summary>
		NormalRendered,

		/// <summary>
		/// The log event is normally formatted and its data normalized. The lack of a rendered message
		/// means improved network load compared to <see cref="NormalRendered"/>. Often this formatting
		/// type is complemented with a log server that is capable of rendering the messages of the
		/// incoming log events.
		/// </summary>
		Normal,

		/// <summary>
		/// The log event is formatted with minimizing size as a priority but still render the message
		/// template into a message. This formatting type greatly reduce the network load and should be
		/// used in situations where bandwidth is of importance.
		/// </summary>
		CompactRendered,

		/// <summary>
		/// The log event is formatted with minimizing size as a priority and its data is normalized. The
		/// lack of a rendered message means even smaller network load compared to
		/// <see cref="CompactRendered"/> and should be used in situations where bandwidth is of
		/// importance. Often this formatting type is complemented with a log server that is capable of
		/// rendering the messages of the incoming log events.
		/// </summary>
		Compact
	}
}
