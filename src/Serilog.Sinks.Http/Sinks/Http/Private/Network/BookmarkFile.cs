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

using System;
using System.IO;
using System.Text;

namespace Serilog.Sinks.Http.Private.Network
{
    public class BookmarkFile : IDisposable
    {
        private readonly FileStream fileStream;

        public BookmarkFile(string bookmarkFileName)
        {
            if (bookmarkFileName == null) throw new ArgumentNullException(nameof(bookmarkFileName));

            fileStream = System.IO.File.Open(
                bookmarkFileName,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.Read);
        }

        public void TryReadBookmark(out long nextLineBeginsAtOffset, out string currentFile)
        {
            nextLineBeginsAtOffset = 0;
            currentFile = null;

            if (fileStream.Length != 0)
            {
                // Important not to dispose this StreamReader as the stream must remain open
                var reader = new StreamReader(fileStream, Encoding.UTF8, false, 128);
                var bookmark = reader.ReadLine();

                if (bookmark != null)
                {
                    fileStream.Position = 0;
                    var parts = bookmark.Split(
                        new[] { ":::" },
                        StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length == 2)
                    {
                        nextLineBeginsAtOffset = long.Parse(parts[0]);
                        currentFile = parts[1];
                    }
                }
            }
        }

        public void WriteBookmark(long nextLineBeginsAtOffset, string currentFile)
        {
            using var writer = new StreamWriter(fileStream);
            writer.WriteLine("{0}:::{1}", nextLineBeginsAtOffset, currentFile);
        }

        public void Dispose() =>
            fileStream?.Dispose();
    }
}
