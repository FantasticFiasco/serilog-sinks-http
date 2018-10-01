using System;
using System.Linq;
using System.Net.Http;

namespace Serilog.Support.Fixtures
{
    public class GitHubWikiFixture
    {
        private string[] rows;

        public void Load(string wikiPage)
        {
            using (var client = new HttpClient())
            {
                rows = client
                    .GetStringAsync($"https://raw.githubusercontent.com/wiki/FantasticFiasco/serilog-sinks-http/{wikiPage}")
                    .Result
                    .Split('\n');
            }
        }

        public string GetDescription(string parameterName)
        {
            var pattern = $"- `{parameterName}` - ";

            var matchingRow = rows.SingleOrDefault(row => row.StartsWith(pattern));
            if (matchingRow == null) throw new Exception($"GitHub wiki does not contain a description of parameter \"{parameterName}\"");

            return matchingRow
                .Substring(pattern.Length)
                .Replace("`", string.Empty);    // Remove code annotation
        }
    }
}
