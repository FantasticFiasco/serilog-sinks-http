using System.IO;
using System.Linq;

namespace Serilog.Support.IO
{
    public static class Files
    {
        public static void DeleteBufferFiles()
        {
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "Buffer*")
                .ToArray();

            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }
}
