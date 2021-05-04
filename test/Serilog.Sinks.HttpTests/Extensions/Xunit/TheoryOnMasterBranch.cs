using System;

// ReSharper disable once CheckNamespace
namespace Xunit
{
    public class TheoryOnMasterBranch : TheoryAttribute
    {
        private string skip;

        public TheoryOnMasterBranch()
        {
            var branch = Environment.GetEnvironmentVariable("APPVEYOR_REPO_BRANCH");
            var pullRequestNumber = Environment.GetEnvironmentVariable("APPVEYOR_PULL_REQUEST_NUMBER");

            skip = branch == "master" && pullRequestNumber == null
                ? null
                : "Only run test on master branch";
        }

        public override string Skip
        {
            get => skip;
            set => skip = value;
        }
    }
}
