using System.IO;

namespace Serilog.Support
{
    public static class BufferFiles
    {
        public static void Delete()
        {
            var files = Directory.GetFiles(Directory.GetCurrentDirectory());

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);

                var delete = fileName.EndsWith(".bookmark")
                             || (fileName.Contains("Buffer") && fileName.EndsWith(".txt"));

                if (delete)
                {
                    File.Delete(file);
                }
            }
        }
    }
}
