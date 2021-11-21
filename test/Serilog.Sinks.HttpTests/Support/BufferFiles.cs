using System;
using System.IO;

namespace Serilog.Support
{
    public static class BufferFiles
    {
        public static void Delete()
        {
            var filePaths = Directory.GetFiles(Directory.GetCurrentDirectory());

            foreach (var filePath in filePaths)
            {
                var fileName = Path.GetFileName(filePath);

                var delete = fileName.EndsWith(".bookmark")
                             || (fileName.Contains("Buffer") && fileName.EndsWith(".txt"));

                if (delete)
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Unable to delete file {fileName}.", e);
                    }
                }
            }
        }
    }
}
