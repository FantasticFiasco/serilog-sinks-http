// Copyright 2015-2026 Serilog Contributors
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

using Serilog.Sinks.Http.Private;

namespace Serilog.Sinks.Http;

/// <summary>
/// Class defining various multipliers of the unit byte.
/// </summary>
public static class ByteSize
{
    /// <summary>
    /// Value representing one byte (B).
    /// </summary>
    public const long B = 1;

    /// <summary>
    /// Value representing 1 kilobyte (KB), or kibibyte (KiB) as the unit sometimes is called.
    /// </summary>
    public const long KB = 1024 * B;

    /// <summary>
    /// Value representing 1 megabyte (MB), or mebibyte (MiB) as the unit sometimes is called.
    /// </summary>
    public const long MB = 1024 * KB;

    /// <summary>
    /// Value representing 1 gigabyte (GB), or gibibyte (GiB) as the unit sometimes is called.
    /// </summary>
    public const long GB = 1024 * MB;

    /// <summary>
    /// Returns the number of bytes produced by UTF8 encoding the characters in the specified
    /// string.
    /// </summary>
    /// <param name="text">The string containing the set of characters to encode.</param>
    /// <returns>The number of bytes produced by encoding the specified characters.</returns>
    public static long From(string text)
    {
        return Encoding.UTF8WithoutBom.GetByteCount(text);
    }
}