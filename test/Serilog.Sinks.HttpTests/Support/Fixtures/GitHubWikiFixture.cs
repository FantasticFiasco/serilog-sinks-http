using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Serilog.Support.Fixtures
{
    public class GitHubWikiFixture
    {
        private const string WikiUrl = "https://raw.githubusercontent.com/wiki/FantasticFiasco/serilog-sinks-http/{0}";
        private const string DescriptionRegexFormat = "- `{0}` - (?<description>.*)$";

        private string pageContent;
        
        public async Task LoadAsync(string wikiPage)
        {
            using (var httpClient = new HttpClient())
            {
                pageContent = await httpClient.GetStringAsync(string.Format(WikiUrl, wikiPage));
            }
        }

        public string GetDescription(string parameterName)
        {
            var descriptionRegex = new Regex(string.Format(DescriptionRegexFormat, parameterName), RegexOptions.Multiline);

            var match = descriptionRegex.Match(pageContent);
            if (!match.Success) throw new Exception($"GitHub wiki does not contain a description of parameter \"{parameterName}\"");

            return match.Groups["description"].Value
                .Replace("`", string.Empty);    // Remove code indicator
        }
    }
}
