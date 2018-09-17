using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Serilog.Sinks.Http.Private.Network
{
    internal static class PayloadReader
    {
        public static string Read(
            string currentFile,
            ref long nextLineBeginsAtOffset,
            ref int count,
            IBatchFormatter batchFormatter,
            int batchPostingLimit)
        {
            var events = new List<string>();

            using (var current = System.IO.File.Open(currentFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                current.Position = nextLineBeginsAtOffset;

                while (count < batchPostingLimit &&
                       TryReadLine(current, ref nextLineBeginsAtOffset, out var nextLine))
                {
                    // Count is the indicator that work was done, so advances even in the (rare) case an
                    // oversized event is dropped
                    count++;

                    events.Add(nextLine);
                }
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

        private static string ReadLine(Stream current)
        {
            // Important not to dispose this StreamReader as the stream must remain open.
            var reader = new StreamReader(current, Encoding.UTF8, false, 128);

            var stringBuilder = new StringBuilder();

            while (true)
            {
                var x = reader.Read();

                if (x == -1)
                {
                    return null;
                }

                if (x == '\r' || x == '\n')
                {
                    return stringBuilder.ToString();
                    
                }

                stringBuilder.Append((char)x);
            }
        }
    }
}
