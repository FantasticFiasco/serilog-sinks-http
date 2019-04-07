using System.Linq;
using System.Xml.Linq;
using Serilog.Sinks.Http;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.TextFormatters;

namespace Serilog.Support.Fixtures
{
    public class XmlDocumentationFixture
    {
        private readonly XDocument document;

        public XmlDocumentationFixture()
        {
            document = XDocument.Load("Serilog.Sinks.Http.xml");
        }

        public string GetDescription(string extensionName, string parameterName)
        {
            var member = document
                .Descendants("member")
                .Single(descendant => descendant.Attribute("name").Value.StartsWith($"M:Serilog.LoggerSinkConfigurationExtensions.{extensionName}"));

            var parameter = member
                .Descendants("param")
                .Single(descendant => descendant.Attribute("name").Value == parameterName);

            var description = GetValue(parameter)
                .Split("\n")
                .Select(row => row.Trim())
                .Where(row => row.Length > 0)
                .Select(RemoveLinks);

            return string.Join(" ", description);
        }

        private static string RemoveLinks(string description)
        {
            return description
                .Replace($"<see cref=\"T:{typeof(NormalRenderedTextFormatter).FullName}\" />", nameof(NormalRenderedTextFormatter))
                .Replace($"<see cref=\"T:{typeof(DefaultBatchFormatter).FullName}\" />", nameof(DefaultBatchFormatter))
                .Replace($"<see cref=\"T:{typeof(IHttpClient).FullName}\" />", nameof(IHttpClient))
                .Replace("<see cref=\"F:Serilog.Events.LevelAlias.Minimum\" />", "LevelAlias.Minimum")
                .Replace("<see cref=\"T:System.Net.Http.HttpClient\" />", "HttpClient")
                .Replace("<paramref name=\"retainedBufferFileCountLimit\" />", "retainedBufferFileCountLimit");
        }

        private static string GetValue(XNode node)
        {
            using (var reader = node.CreateReader())
            {
                reader.MoveToContent();

                return reader.ReadInnerXml();
            }
        }
    }
}
