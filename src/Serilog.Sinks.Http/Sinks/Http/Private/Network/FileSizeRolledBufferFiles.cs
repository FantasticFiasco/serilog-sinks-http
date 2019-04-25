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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog.Sinks.Http.Private.IO;

namespace Serilog.Sinks.Http.Private.Network
{
    public class FileSizeRolledBufferFiles : IBufferFiles
    {
        private readonly IDirectoryService directoryService;
        private readonly string logFolder;
        private readonly string candidateSearchPath;
        private readonly Regex fileNameMatcher;

        public FileSizeRolledBufferFiles(IDirectoryService directoryService, string bufferBaseFileName)
        {
            if (bufferBaseFileName == null) throw new ArgumentNullException(nameof(bufferBaseFileName));

            this.directoryService = directoryService ?? throw new ArgumentNullException(nameof(directoryService));

            BookmarkFileName = Path.GetFullPath(bufferBaseFileName + ".bookmark");
            logFolder = Path.GetDirectoryName(BookmarkFileName);
            candidateSearchPath = Path.GetFileName(bufferBaseFileName) + "-*.json";
            fileNameMatcher = new Regex("^" + Regex.Escape(Path.GetFileName(bufferBaseFileName)) + "-(?<date>\\d{8})(?<sequence>_[0-9]{3,}){0,1}\\.json$");
        }

        public string BookmarkFileName { get; }

        public string[] Get()
        {
            return directoryService.GetFiles(logFolder, candidateSearchPath)
                .Select(filePath => new KeyValuePair<string, Match>(filePath, fileNameMatcher.Match(Path.GetFileName(filePath))))
                .Where(pair => pair.Value.Success)
                .OrderBy(pair => pair.Value.Groups["date"].Value, StringComparer.OrdinalIgnoreCase)
                .ThenBy(pair => int.Parse("0" + pair.Value.Groups["sequence"].Value.Replace("_", string.Empty)))
                .Select(pair => pair.Key)
                .ToArray();
        }
    }
}
