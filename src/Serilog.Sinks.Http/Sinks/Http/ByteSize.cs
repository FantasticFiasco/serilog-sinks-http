// Copyright 2015-2020 Serilog Contributors
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

using System.Text;

namespace Serilog.Sinks.Http
{
    /// <summary>
    /// Class 
    /// </summary>
    public static class ByteSize
    {
        public const long B = 1;

        public const long KB = 1024 * B;

        public const long MB = 1024 * KB;

        public const long GB = 1024 * MB;

        public static long From(string text) => Encoding.UTF8.GetByteCount(text);
    }
}
