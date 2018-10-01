using System.Linq;
using System.Xml.Linq;

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

            return parameter.Value;
        }
    }
}
