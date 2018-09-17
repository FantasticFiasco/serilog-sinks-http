﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Serilog.Sinks.Http.Private.Network
{
    internal static class PayloadReader
    {
        private const char CR = '\r';
        private const char LF = '\n';

        public static string Read(
            string fileName,
            ref long nextLineBeginsAtOffset,
            ref int count,
            IBatchFormatter batchFormatter,
            int batchPostingLimit)
        {
            using (var stream = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return Read(stream, ref nextLineBeginsAtOffset, ref count, batchFormatter, batchPostingLimit);
            }
        }

        public static string Read(
            Stream stream,
            ref long nextLineBeginsAtOffset,
            ref int count,
            IBatchFormatter batchFormatter,
            int batchPostingLimit)
        {
            var events = new List<string>();

            stream.Position = nextLineBeginsAtOffset;

            while (count < batchPostingLimit &&
                    TryReadLine(stream, ref nextLineBeginsAtOffset, out var nextLine))
            {
                // Count is the indicator that work was done, so advances even in the (rare) case an
                // oversized event is dropped
                count++;

                events.Add(nextLine);
            }

            var payload = new StringWriter();

            batchFormatter.Format(events, payload);

            return payload.ToString();
        }

        private static bool TryReadLine(Stream current, ref long nextStart, out string nextLine)
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
                return false;

            nextStart += Encoding.UTF8.GetByteCount(nextLine) + Encoding.UTF8.GetByteCount(Environment.NewLine);

            if (includesBom)
            {
                nextStart += 3;
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
                var character = (char)reader.Read();

                // Is this the end of the stream? In that case abort since all log events are
                // terminated using a new line, and this would mean that either:
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

                lineBuilder.Append(character);
            }
        }
    }
}
