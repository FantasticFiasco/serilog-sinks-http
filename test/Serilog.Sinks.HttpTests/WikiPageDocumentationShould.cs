using System.Linq;
using System.Threading.Tasks;
using Serilog.Configuration;
using Serilog.Support.Fixtures;
using Shouldly;
using Xunit;

namespace Serilog
{
    public class WikiPageDocumentationShould
        : IClassFixture<XmlDocumentationFixture>, IClassFixture<GitHubWikiFixture>
    {
        private readonly GitHubWikiFixture gitHubWikiFixture;
        private readonly XmlDocumentationFixture xmlDocumentationFixture;

        public WikiPageDocumentationShould(GitHubWikiFixture gitHubWikiFixture, XmlDocumentationFixture xmlDocumentationFixture)
        {
            this.gitHubWikiFixture = gitHubWikiFixture;
            this.xmlDocumentationFixture = xmlDocumentationFixture;
        }

        [TheoryOnMasterBranch]
        [InlineData("HTTP-sink.md", "Http")]
        [InlineData("Durable-file-size-rolled-HTTP-sink.md", "DurableHttpUsingFileSizeRolledBuffers")]
        [InlineData("Durable-time-rolled-HTTP-sink.md", "DurableHttpUsingTimeRolledBuffers")]
        public async Task MatchCode(string wikiPage, string extensionName)
        {
            // Arrange
            await gitHubWikiFixture.LoadAsync(wikiPage);

            var parameterNames = typeof(LoggerSinkConfigurationExtensions)
                .GetMethod(extensionName)
                .GetParameters()
                .Where(parameter => parameter.ParameterType != typeof(LoggerSinkConfiguration))
                .Select(parameter => parameter.Name);

            foreach (var parameterName in parameterNames)
            {
                // Act
                var wikiDescription = gitHubWikiFixture.GetDescription(parameterName);
                var codeDescription = xmlDocumentationFixture.GetDescription(extensionName, parameterName);

                // Assert
                wikiDescription.ShouldBe(codeDescription);
            }
        }
    }
}
