// Copyright 2015-2024 Serilog Contributors
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

using Serilog.Formatting;

namespace Serilog.Sinks.Http.TextFormatters;

/// <summary>
/// JSON formatter serializing log events with minimizing size as a priority but still render
/// the message template into a message. This formatter greatly reduce the network load and
/// should be used in situations where bandwidth is of importance.
/// </summary>
/// <seealso cref="NormalTextFormatter" />
/// <seealso cref="NormalRenderedTextFormatter" />
/// <seealso cref="CompactTextFormatter" />
/// <seealso cref="NamespacedTextFormatter" />
/// <seealso cref="ITextFormatter" />
public class CompactRenderedTextFormatter : CompactTextFormatter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompactRenderedTextFormatter"/> class.
    /// </summary>
    public CompactRenderedTextFormatter()
    {
        IsRenderingMessage = true;
    }
}