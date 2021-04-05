$logo = (Invoke-WebRequest "https://raw.githubusercontent.com/FantasticFiasco/logo/master/logo.raw").toString();
Write-Host "$logo" -ForegroundColor Green

Write-Host "build: build started"
Write-Host "build: dotnet cli v$(dotnet --version)"

Push-Location $PSScriptRoot\..

# Clean artifacts
if (Test-Path .\artifacts)
{
    Write-Host "build: cleaning .\artifacts"
    Remove-Item .\artifacts -Force -Recurse
}

$tagged_build = if ($env:APPVEYOR_REPO_TAG -eq "true") { $true } else { $false }
Write-Host "build: triggered by git tag: $tagged_build"

$git_sha = $env:APPVEYOR_REPO_COMMIT.Substring(0, 7)
Write-Host "build: git sha: $git_sha"

[xml]$build_props = Get-Content -Path .\Directory.Build.props
$version_prefix = $build_props.Project.PropertyGroup.VersionPrefix
Write-Host "build: build props version prefix: $version_prefix"
$version_suffix = $build_props.Project.PropertyGroup.VersionSuffix
Write-Host "build: build props version suffix: $version_suffix"

# Build and pack
foreach ($source in Get-ChildItem .\src\*)
{
    Push-Location $source

    Write-Host "build: packaging project in $source"

    if ($tagged_build)
    {
        & dotnet build -c Release
        & dotnet pack -c Release -o ..\..\artifacts --no-build
    }
    else
    {
        # Use git tag if version suffix isn't specified
        if ($version_suffix -eq "")
        {
            $version_suffix = $git_sha
        }

        & dotnet build -c Release --version-suffix=$version_suffix
        & dotnet pack -c Release -o ..\..\artifacts --version-suffix=$version_suffix --no-build
    }

    if ($LASTEXITCODE -ne 0)
    {
        exit 1
    }

    Pop-Location
}

# Test
foreach ($test in Get-ChildItem test/*Tests)
{
    Push-Location $test

    Write-Host "build: testing project in $test"

    dotnet test -c Release --no-build --collect:"XPlat Code Coverage"
    if ($LASTEXITCODE -ne 0) { exit 2 }

    Pop-Location
}

Pop-Location
