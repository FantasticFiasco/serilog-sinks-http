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

using System.IO;
using System.Text;
using Serilog.Debugging;

namespace Serilog.Sinks.Http.Private.Durable
{
    public static class BufferFileReader
    {
        private const char CR = '\r';
        private const char LF = '\n';

        private static readonly string CRLFString = $"{CR}{LF}";
        private static readonly string LFString = $"{LF}";

        public static Batch Read(
            string fileName,
            ref long nextLineBeginsAtOffset,
            int batchPostingLimit,
            long batchSizeLimitBytes)
        {
            using var stream = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return Read(stream, ref nextLineBeginsAtOffset, batchPostingLimit, batchSizeLimitBytes);
        }

        public static Batch Read(
            Stream stream,
            ref long nextLineBeginsAtOffset,
            int batchPostingLimit,
            long batchSizeLimitBytes)
        {
            var batch = new Batch();
            long batchSizeBytes = 0;

            while (true)
            {
                if (stream.Length <= nextLineBeginsAtOffset)
                {
                    break;
                }

                stream.Position = nextLineBeginsAtOffset;

                // Read log event
                var line = ReadLine(stream);
                if (string.IsNullOrEmpty(line.Text))
                {
                    break;
                }

                // Calculate the size of the log event
                var lineSizeBytes = ByteSize.From(line.Text);
                var includeLine = true;

                // Respect batch size limit
                if (lineSizeBytes > batchSizeLimitBytes)
                {
                    // This single log event exceeds the batch size limit, let's drop it
                    includeLine = false;

                    SelfLog.WriteLine(
                        "Event exceeds the batch size limit of {0} bytes set for this sink and will be dropped; data: {1}",
                        batchSizeLimitBytes,
                        line);
                }
                else if (batchSizeBytes + lineSizeBytes > batchSizeLimitBytes)
                {
                    // The accumulated size of the batch is exceeding the batch size limit
                    batch.HasReachedLimit = true;
                    break;
                }

                // Update cursor
                nextLineBeginsAtOffset += lineSizeBytes + ByteSize.From(line.NewLine);

                // Add log event
                if (includeLine)
                {
                    batch.LogEvents.Add(line.Text);
                    batchSizeBytes += lineSizeBytes;
                }

                // Respect batch posting limit
                if (batch.LogEvents.Count == batchPostingLimit)
                {
                    batch.HasReachedLimit = true;
                    break;
                }
            }

            return batch;
        }

        private struct Line
        {
            public string Text;
            public string NewLine;
        }

        private static Line ReadLine(Stream stream)
        {
            // Important not to dispose this StreamReader as the stream must remain open
            var reader = new StreamReader(stream, Encoding.UTF8WithoutBom, false, 128);
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
                    return new Line { Text = string.Empty, NewLine = string.Empty };
                }

                // Are we done, have we read the line?
                if (character == CR)
                {
                    return new Line { Text = lineBuilder.ToString(), NewLine = CRLFString };
                }
                if (character == LF)
                {
                    return new Line { Text = lineBuilder.ToString(), NewLine = LFString };
                }

                lineBuilder.Append((char)character);
            }
        }
    }
}
