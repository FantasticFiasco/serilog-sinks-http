using System;

// ReSharper disable once CheckNamespace
namespace Xunit
{
    public class TheoryOnMasterBranch : TheoryAttribute
    {
        public override string Skip { get; set; } =
            Environment.GetEnvironmentVariable("APPVEYOR_REPO_BRANCH") == "master" &&
            Environment.GetEnvironmentVariable("APPVEYOR_PULL_REQUEST_NUMBER") == null
                ? null
                : "Only run test on master branch";
    }
}
