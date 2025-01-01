// Copyright 2015-2025 Serilog Contributors
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

namespace Serilog.Sinks.Http.Private.Durable;

public class TimeRolledBufferFiles : IBufferFiles
{
    private readonly DirectoryService directoryService;
    private readonly string logFolder;
    private readonly string candidateSearchPath;
    private readonly Regex fileNameMatcher;

    public TimeRolledBufferFiles(DirectoryService directoryService, string bufferBaseFilePath)
    {
        if (bufferBaseFilePath == null) throw new ArgumentNullException(nameof(bufferBaseFilePath));

        this.directoryService = directoryService ?? throw new ArgumentNullException(nameof(directoryService));

        var bufferBaseFullPath = Path.GetFullPath(bufferBaseFilePath);
        var bufferBaseFileName = Path.GetFileName(bufferBaseFilePath) ?? throw new Exception("Cannot get file name from buffer base file path");

        logFolder = Path.GetDirectoryName(bufferBaseFullPath) ?? throw new Exception("Cannot get directory of buffer base file path");
        candidateSearchPath = $"{bufferBaseFileName}-*.*";
        fileNameMatcher = new Regex(
            "^" +                            // Start of string
            Regex.Escape(bufferBaseFileName) +      // Base file name
            "-" +
            "(?<timestamp>[0-9]{4,12})" +           // Timestamp in format YYYY(MM(DD(HH(MM))))
            "\\." +
            "(?<extension>json|txt)" +              // File extension
            "$");                                   // End of string

        BookmarkFileName = $"{bufferBaseFullPath}.bookmark";
    }

    public string BookmarkFileName { get; }

    public string[] Get()
    {
        return directoryService.GetFiles(logFolder, candidateSearchPath)
            .Select(filePath => new KeyValuePair<string, Match>(filePath, fileNameMatcher.Match(Path.GetFileName(filePath))))
            .Where(pair => pair.Value.Success)
            .OrderBy(pair => pair.Value.Groups["extension"].Value == "txt" ? 1 : 0)
            .ThenBy(pair => pair.Value.Groups["timestamp"].Value, StringComparer.OrdinalIgnoreCase)
            .Select(pair => pair.Key)
            .ToArray();
    }
}