using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Serilog.Sinks.Http.Private.IO
{
    public class DirectoryServiceMock : DirectoryService
    {
        public DirectoryServiceMock()
        {
            Files = Array.Empty<string>();
        }

        public string[] Files { get; set; }

        public override string[] GetFiles(string path, string searchPattern)
        {
            // Turn the pattern into a regular expression
            var filter = new Regex(searchPattern.Replace("*", ".*"));

            return Files
                .Where(file => filter.IsMatch(file))
                .ToArray();
        }
    }
}
