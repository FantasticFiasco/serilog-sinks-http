$logo = (Invoke-WebRequest "https://raw.githubusercontent.com/FantasticFiasco/logo/master/logo.raw").toString();
Write-Host "$logo" -ForegroundColor Green

Write-Host "build: Build started"
Write-Host "build: dotnet cli v$(dotnet --version)"

Push-Location $PSScriptRoot\..

if (Test-Path .\artifacts) {
    Write-Host "build: Cleaning .\artifacts"
    Remove-Item .\artifacts -Force -Recurse
}

Write-Host "build: Install NuGet packages"
& dotnet restore --no-cache

$branch = @{ $true = $env:APPVEYOR_REPO_BRANCH; $false = $(git symbolic-ref --short -q HEAD) }[$env:APPVEYOR_REPO_BRANCH -ne $NULL];
if ($branch -like "*/*") { $branch = $branch.Substring($branch.LastIndexOf("/") + 1) }
$revision = @{ $true = "{0:00000}" -f [convert]::ToInt32("0" + $env:APPVEYOR_BUILD_NUMBER, 10); $false = "local" }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
$suffix = @{ $true = ""; $false = "$branch-$revision"}[$branch -eq "master" -and $revision -ne "local"]

Write-Host "build: Version suffix is '$suffix'"

foreach ($src in Get-ChildItem src/*) {
    Push-Location $src

    Write-Host "build: Packaging project in $src"

    if ($suffix -eq "")
    {
        & dotnet build -c Release
        & dotnet pack -c Release --include-symbols -o ..\..\artifacts --no-build
    }
    else {
        & dotnet build -c Release --version-suffix=$suffix
        & dotnet pack -c Release --include-symbols -o ..\..\artifacts --version-suffix=$suffix --no-build
    }
    if ($LASTEXITCODE -ne 0) { exit 1 }    

    Pop-Location
}

foreach ($test in Get-ChildItem test/*Tests) {
    Push-Location $test

    Write-Host "build: Testing project in $test"

    & dotnet test -c Release
    if ($LASTEXITCODE -ne 0) { exit 2 }

    Pop-Location
}

Pop-Location
