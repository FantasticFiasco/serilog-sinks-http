$logo = (Invoke-WebRequest "https://raw.githubusercontent.com/FantasticFiasco/logo/master/logo.raw").toString();
Write-Host "$logo" -ForegroundColor Green

Write-Host "build: Build started"
Write-Host "build: dotnet cli v$(dotnet --version)"

Push-Location $PSScriptRoot\..

# Clean artifacts
if (Test-Path .\artifacts)
{
    Write-Host "build: Cleaning .\artifacts"
    Remove-Item .\artifacts -Force -Recurse
}

$tagged_build = if ($env:APPVEYOR_REPO_TAG -eq "true") { $true } else { $false }
Write-Host "build: Triggered by git tag: $tagged_build"

$git_sha = $env:APPVEYOR_REPO_COMMIT.Substring(0, 7)
Write-Host "build: Git SHA: $git_sha"

# Build and pack
foreach ($source in Get-ChildItem .\src\*)
{
    Push-Location $source

    Write-Host "build: Packaging project in $source"

    if ($tagged_build)
    {
        & dotnet build -c Release
        & dotnet pack -c Release --include-symbols -o ..\..\artifacts --no-build
    }
    else
    {
        & dotnet build -c Release --version-suffix=$git_sha
        & dotnet pack -c Release --include-symbols -o ..\..\artifacts --version-suffix=$git_sha --no-build
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

    Write-Host "build: Testing project in $test"

    & dotnet test -c Release
    if ($LASTEXITCODE -ne 0) { exit 2 }

    Pop-Location
}

# Push
if ($tagged_build)
{
    Write-Host "build: push package to www.nuget.org"

    Push-Location .\artifacts

    foreach ($package in Get-ChildItem *.nupkg -Exclude *.symbols.nupkg)
    {
        & dotnet nuget push $package --source "https://www.nuget.org/api/v2/package" --api-key "$env:NUGET_API_KEY"
    }

    Pop-Location
}

Pop-Location
