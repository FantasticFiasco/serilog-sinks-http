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
    public static class BufferFileReader
    {
        private const char CR = '\r';
        private const char LF = '\n';

        /// <summary>
        /// The length of the Byte Order Marks (BOM).
        /// </summary>
        public const int BomLength = 3;

        public static string[] Read(
            string fileName,
            ref long nextLineBeginsAtOffset,
            ref int count,
            int batchPostingLimit,
            long batchSizeLimit)
        {
            using var stream = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return Read(stream, ref nextLineBeginsAtOffset, ref count, batchPostingLimit, batchSizeLimit);
        }

        public static string[] Read(
            Stream stream,
            ref long nextLineBeginsAtOffset,
            ref int count,
            int batchPostingLimit,
            long batchSizeLimit)
        {
            var logEvents = new List<string>();

            stream.Position = nextLineBeginsAtOffset;
            long batchSize = 0;

            while (count < batchPostingLimit
                   && TryReadLine(stream, ref nextLineBeginsAtOffset, out var nextLine, ref batchSize, batchSizeLimit))
            {
                logEvents.Add(nextLine);
                count++;
            }

            return logEvents.ToArray();
        }

        private static bool TryReadLine(Stream current, ref long nextStart, out string nextLine, ref long batchSize, long batchSizeLimit)
        {
            var includesBom = nextStart == 0;

            if (current.Length <= nextStart)
            {
                nextLine = null;
                return false;
            }

            current.Position = nextStart;

            nextLine = ReadLine(current);
            if (nextLine == null)
            {
                return false;
            }

            batchSize += nextLine.Length;
            if (batchSize > batchSizeLimit)
            {
                return false;
            }

            nextStart += ByteSize.From(nextLine) + ByteSize.From(Environment.NewLine);

            if (includesBom)
            {
                nextStart += BomLength;
            }

            return true;
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
