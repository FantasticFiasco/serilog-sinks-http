// Copyright 2015-2018 Serilog Contributors
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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog.Sinks.Http.Private.IO;

namespace Serilog.Sinks.Http.Private.Network
{
    public class TimeRolledBufferFiles : IBufferFiles
    {
        private static readonly Regex BufferPathFormatRegex = new Regex(
            $"(?<prefix>.+)(?:{string.Join("|", Enum.GetNames(typeof(DateFormats)).Select(x => $"{{{x}}}"))})(?<postfix>.+)");

        private readonly IDirectoryService directoryService;
        private readonly string logFolder;
        private readonly string candidateSearchPath;

        public TimeRolledBufferFiles(IDirectoryService directoryService, string bufferPathFormat)
        {
            if (bufferPathFormat == null) throw new ArgumentNullException(nameof(bufferPathFormat));
            if (bufferPathFormat != bufferPathFormat.Trim()) throw new ArgumentException("bufferPathFormat must not contain any leading or trailing whitespaces", nameof(bufferPathFormat));

            this.directoryService = directoryService ?? throw new ArgumentNullException(nameof(directoryService));

            var bufferPathFormatMatch = BufferPathFormatRegex.Match(bufferPathFormat);
            if (!bufferPathFormatMatch.Success)
            {
                throw new ArgumentException($"Must include one of the date formats [{string.Join(", ", Enum.GetNames(typeof(DateFormats)))}]", nameof(bufferPathFormat));
            }

            var prefix = bufferPathFormatMatch.Groups["prefix"];
            var postfix = bufferPathFormatMatch.Groups["postfix"];

            BookmarkFileName = Path.GetFullPath(prefix.Value.TrimEnd('-') + ".bookmark");
            logFolder = Path.GetDirectoryName(BookmarkFileName);
            candidateSearchPath = $"{Path.GetFileName(prefix.Value)}*{postfix.Value}";
        }

        public string BookmarkFileName { get; }

        public string[] Get()
        {
            return directoryService.GetFiles(logFolder, candidateSearchPath)
                .OrderBy(filePath => filePath)
                .ToArray();
        }
    }
}
