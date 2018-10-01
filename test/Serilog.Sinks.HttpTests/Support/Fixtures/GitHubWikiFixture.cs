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

            return rows
                .Single(row => row.StartsWith(pattern))
                .Substring(pattern.Length);
        }
    }
}
