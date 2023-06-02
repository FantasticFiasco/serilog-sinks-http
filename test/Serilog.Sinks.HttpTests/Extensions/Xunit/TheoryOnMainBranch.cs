using System;

// ReSharper disable once CheckNamespace
namespace Xunit
{
    public class TheoryOnMainBranch : TheoryAttribute
    {
        private string skip;

        public TheoryOnMainBranch()
        {
            var branch = Environment.GetEnvironmentVariable("APPVEYOR_REPO_BRANCH");
            var pullRequestNumber = Environment.GetEnvironmentVariable("APPVEYOR_PULL_REQUEST_NUMBER");

            skip = branch == "main" && pullRequestNumber == null
                ? null
                : "Only run test on main branch";
        }

        public override string Skip
        {
            get => skip;
            set => skip = value;
        }
    }
}
