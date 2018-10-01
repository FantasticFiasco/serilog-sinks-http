using System.Linq;
using Serilog.Configuration;
using Serilog.Support.Fixtures;
using Shouldly;
using Xunit;

namespace Serilog
{
    public class DocumentationShould
        : IClassFixture<XmlDocumentationFixture>, IClassFixture<GitHubWikiFixture>
    {
        private readonly XmlDocumentationFixture xmlDocumentationFixture;
        private readonly GitHubWikiFixture gitHubWikiFixture;
        
        public DocumentationShould(XmlDocumentationFixture xmlDocumentationFixture, GitHubWikiFixture gitHubWikiFixture)
        {
            this.xmlDocumentationFixture = xmlDocumentationFixture;
            this.gitHubWikiFixture = gitHubWikiFixture;
        }

        [Theory]
        [InlineData("Http", "HTTP-sink.md")]
        [InlineData("DurableHttp", "Durable-HTTP-sink.md")]
        public void MatchWikiPage(string extensionName, string wikiPage)
        {
            // Arrange
            gitHubWikiFixture.Load(wikiPage);

            var parameterNames = typeof(LoggerSinkConfigurationExtensions)
                .GetMethod(extensionName)
                .GetParameters()
                .Where(parameter => parameter.ParameterType != typeof(LoggerSinkConfiguration))
                .Select(parameter => parameter.Name);

            foreach (var parameterName in parameterNames)
            {
                // Act
                var codeDescription = xmlDocumentationFixture.GetDescription(extensionName, parameterName);
                var wikiDescription = gitHubWikiFixture.GetDescription(parameterName);

                // Assert
                wikiDescription.ShouldBe(codeDescription);
            }
        }
    }
}
