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
using Serilog.Formatting;

namespace Serilog.Sinks.Http.Private.Formatters
{
    /// <summary>
    /// Class converting formatting type into a formatter.
    /// </summary>
    public static class Converter
    {
        /// <summary>
        /// Converts a formatting type into a formatter.
        /// </summary>
        /// <param name="formattingType">The formatting type.</param>
        public static ITextFormatter ToFormatter(FormattingType formattingType)
        {
            switch (formattingType)
            {
                case FormattingType.NormalRendered:
                    return new NormalJsonFormatter(true);

                case FormattingType.Normal:
                    return new NormalJsonFormatter(false);

                case FormattingType.CompactRendered:
                    return new CompactJsonFormatter(true);

                case FormattingType.Compact:
                    return new CompactJsonFormatter(false);

                default:
                    throw new ArgumentException($"Formatting type {formattingType} is not supported");
            }
        }
    }
}
