$logo = (Invoke-WebRequest "https://raw.githubusercontent.com/FantasticFiasco/logo/master/logo.raw").toString();
Write-Host "$logo" -ForegroundColor Green

Push-Location $PSScriptRoot\..

Write-Host "[info] dotnet cli v$(dotnet --version)"

$tagged_build = if ($env:APPVEYOR_REPO_TAG -eq "true") { $true } else { $false }
Write-Host "[info] triggered by git tag: $tagged_build"

$git_sha = $env:APPVEYOR_REPO_COMMIT.Substring(0, 7)
Write-Host "[info] git sha: $git_sha"

$is_pull_request = if ("$env:APPVEYOR_PULL_REQUEST_NUMBER" -eq "") { $false } else { $true }
Write-Host "[info] is pull request: $is_pull_request"

[xml]$build_props = Get-Content -Path .\Directory.Build.props
$version_prefix = $build_props.Project.PropertyGroup.VersionPrefix
Write-Host "[info] build props version prefix: $version_prefix"
$version_suffix = $build_props.Project.PropertyGroup.VersionSuffix
Write-Host "[info] build props version suffix: $version_suffix"

# Clean artifacts
if (Test-Path .\artifacts)
{
    Write-Host "[build] cleaning .\artifacts"
    Remove-Item .\artifacts -Force -Recurse
}

# Build and pack
if ($tagged_build)
{
    Write-Host "[build] build"
    & dotnet build -c Release

    Write-Host "[build] pack"
    & dotnet pack -c Release -o ..\..\artifacts --no-build
}
else
{
    # Use git tag if version suffix isn't specified
    if ($version_suffix -eq "")
    {
        $version_suffix = $git_sha
    }

    Write-Host "[build] build"
    & dotnet build -c Release --version-suffix=$version_suffix

    Write-Host "[build] pack"
    & dotnet pack -c Release -o ..\..\artifacts --version-suffix=$version_suffix --no-build
}

if ($LASTEXITCODE -ne 0)
{
    exit 1
}

# Test
Write-Host "[test] test"
& dotnet test -c Release --no-build --collect:"XPlat Code Coverage"
if ($LASTEXITCODE -ne 0)
{
    exit 1
}

If ($is_pull_request -eq $false)
{
    Write-Host "[test] upload coverage report"
    Invoke-WebRequest -Uri "https://codecov.io/bash" -OutFile codecov.sh

    foreach ($testResult in Get-ChildItem .\test\Serilog.Sinks.HttpTests\TestResults\*)
    {
        Push-Location $testResult

        bash codecov.sh -f "coverage.cobertura.xml"

        if ($LASTEXITCODE -ne 0)
        {
            exit 1
        }

        Pop-Location
    }
}
