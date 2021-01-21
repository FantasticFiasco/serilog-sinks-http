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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Serilog.Sinks.Http.Private.Network
{
    public class Batch
    {
        public List<string> LogEvents { get; } = new();

        public bool HasReachedLimit { get; set; }
    }

    public static class BufferFileReader
    {
        private const char CR = '\r';
        private const char LF = '\n';

        /// <summary>
        /// The length of the Byte Order Marks (BOM).
        /// </summary>
        public const int BomLength = 3;

        public static Batch Read(
            string fileName,
            ref long nextLineBeginsAtOffset,
            int batchPostingLimit,
            long batchSizeLimit)
        {
            using var stream = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return Read(stream, ref nextLineBeginsAtOffset, batchPostingLimit, batchSizeLimit);
        }

        public static Batch Read(
            Stream stream,
            ref long nextLineBeginsAtOffset,
            int batchPostingLimit,
            long batchSizeLimit)
        {
            var batch = new Batch();
            long batchSize = 0;

            while (true)
            {
                if (stream.Length <= nextLineBeginsAtOffset)
                {
                    break;
                }
                
                stream.Position = nextLineBeginsAtOffset;

                // Read next log event
                var nextLine = ReadLine(stream);
                if (nextLine == null)
                {
                    break;
                }

                // Respect batch size limit
                batchSize += ByteSize.From(nextLine);
                if (batchSize > batchSizeLimit)
                {
                    batch.HasReachedLimit = true;
                    break;
                }

                // Update cursor
                var includesBom = nextLineBeginsAtOffset == 0;
                nextLineBeginsAtOffset += ByteSize.From(nextLine) + ByteSize.From(Environment.NewLine);

                if (includesBom)
                {
                    nextLineBeginsAtOffset += BomLength;
                }

                // Add log event
                batch.LogEvents.Add(nextLine);

                // Respect batch posting limit
                if (batch.LogEvents.Count == batchPostingLimit)
                {
                    batch.HasReachedLimit = true;
                    break;
                }
            }

            return batch;
        }

        private static string ReadLine(Stream stream)
        {
            // Important not to dispose this StreamReader as the stream must remain open
            var reader = new StreamReader(stream, Encoding.UTF8, false, 128);
            var lineBuilder = new StringBuilder();

            while (true)
            {
                var character = reader.Read();

                // Is this the end of the stream? In that case abort since all log events should be
                // terminated using a new line, and a line without a new line would mean that
                // either:
                //   - There are no new log events
                //   - The current log event hasn't yet been completely flushed to disk
                if (character == -1)
                {
                    return null;
                }

                // Are we done, have we read the line?
                if (character == CR || character == LF)
                {
                    return lineBuilder.ToString();
                }

                lineBuilder.Append((char)character);
            }
        }
    }
}
