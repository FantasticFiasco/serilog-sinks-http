using System;
using System.IO;
using System.Text;

namespace Serilog.Sinks.Http.Private.Network
{
    internal class BookmarkFile : IDisposable
    {
        private readonly FileStream fileStream;

        public BookmarkFile(string bookmarkFilename)
        {
            if (bookmarkFilename == null) throw new ArgumentNullException(nameof(bookmarkFilename));

            fileStream = System.IO.File.Open(
                bookmarkFilename,
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
                // Important not to dispose this StreamReader as the stream must remain open.
                var reader = new StreamReader(fileStream, Encoding.UTF8, false, 128);
                var current = reader.ReadLine();

                if (current != null)
                {
                    fileStream.Position = 0;
                    var parts = current.Split(
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
            using (var writer = new StreamWriter(fileStream))
            {
                writer.WriteLine("{0}:::{1}", nextLineBeginsAtOffset, currentFile);
            }
        }

        public void Dispose() => fileStream?.Dispose();
    }
}
