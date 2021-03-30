using Serilog.Support.Fixtures;
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

        [Fact(Skip = "Skip until v8 is released")]
        // [TheoryOnMasterBranch]
        // [InlineData("HTTP-sink.md", "Http")]
        // [InlineData("Durable-file-size-rolled-HTTP-sink.md", "DurableHttpUsingFileSizeRolledBuffers")]
        // [InlineData("Durable-time-rolled-HTTP-sink.md", "DurableHttpUsingTimeRolledBuffers")]
        // public async Task MatchCode(string wikiPage, string extensionName)
        public void MatchCode()
        {
            // // Arrange
            // await gitHubWikiFixture.LoadAsync(wikiPage);

            // var parameterNames = typeof(LoggerSinkConfigurationExtensions)
            //     .GetMethod(extensionName)
            //     .GetParameters()
            //     .Where(parameter => parameter.ParameterType != typeof(LoggerSinkConfiguration))
            //     .Select(parameter => parameter.Name);

            // foreach (var parameterName in parameterNames)
            // {
            //     // Act
            //     var got = gitHubWikiFixture.GetDescription(parameterName);
            //     var want = xmlDocumentationFixture.GetDescription(extensionName, parameterName);

            //     // Assert
            //     got.ShouldBe(want);
            // }
        }
    }
}
