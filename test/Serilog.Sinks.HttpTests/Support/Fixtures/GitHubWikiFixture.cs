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

        private static readonly Regex LinkRegex = new Regex(@"\[(?<link>[\w\s.]+)\]\((?<url>[\w.:/]+)\)");

        private string pageContent;
        
        public async Task LoadAsync(string wikiPage)
        {
            using var httpClient = new HttpClient();
            pageContent = await httpClient.GetStringAsync(string.Format(WikiUrl, wikiPage));
        }

        public string GetDescription(string parameterName)
        {
            var descriptionRegex = new Regex(string.Format(DescriptionRegexFormat, parameterName), RegexOptions.Multiline);

            var match = descriptionRegex.Match(pageContent);
            if (!match.Success) throw new Exception($"GitHub wiki does not contain a description of parameter \"{parameterName}\"");

            return
                RemoveLinks(
                    RemoveCodeIndicator(
                        match.Groups["description"].Value));
        }

        private static string RemoveCodeIndicator(string description)
        {
            return description.Replace("`", string.Empty);
        }

        private static string RemoveLinks(string description)
        {
            var matches = LinkRegex.Matches(description);

            foreach (Match match in matches)
            {
                description = description.Replace(
                    match.Groups[0].Value,
                    match.Groups["link"].Value);
            }

            return description;
        }
    }
}
