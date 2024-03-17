# -------------------------------------------------------------------------------------------------
# COMMON FUNCTIONS
# -------------------------------------------------------------------------------------------------
function Print {
    param (
        [string]$Category,
        [string]$Message
    )

    if ($Category) {
        Write-Host "[$Category] $Message" -ForegroundColor Green
    } else {
        Write-Host "$Message" -ForegroundColor Green
    }
}

function AssertLastExitCode {
    if ($LASTEXITCODE -ne 0) {
        exit 1
    }
}

# -------------------------------------------------------------------------------------------------
# LOGO
# -------------------------------------------------------------------------------------------------
$logo = (Invoke-WebRequest "https://raw.githubusercontent.com/FantasticFiasco/logo/main/logo.raw").toString();
Print -Message $logo

# -------------------------------------------------------------------------------------------------
# VARIABLES
# -------------------------------------------------------------------------------------------------
$git_sha = "$env:GITHUB_SHA".TrimStart("0").substring(0, 7)
$is_tagged_build = If ("$env:GITHUB_REF".StartsWith("refs/tags/")) { $true } Else { $false }
Print "info" "git sha: $git_sha"
Print "info" "is git tag: $is_tagged_build"

# -------------------------------------------------------------------------------------------------
# BUILD
# -------------------------------------------------------------------------------------------------
Print "build" "build started"
Print "build" "dotnet cli v$(dotnet --version)"

[xml]$build_props = Get-Content -Path .\Directory.Build.props
$version_prefix = $build_props.Project.PropertyGroup.VersionPrefix
Print "info" "build props version prefix: $version_prefix"
$version_suffix = $build_props.Project.PropertyGroup.VersionSuffix
Print "info" "build props version suffix: $version_suffix"

if ($is_tagged_build) {
    Print "build" "build"
    dotnet build -c Release
    AssertLastExitCode

    Print "build" "pack"
    New-Item -ItemType Directory -Path .\artifacts
    dotnet pack -c Release --no-build
    AssertLastExitCode
    Move-Item -Path .\src\Serilog.Sinks.Http\bin\Release\*.nupkg -Destination .\artifacts
    Move-Item -Path .\src\Serilog.Sinks.Http\bin\Release\*.snupkg -Destination .\artifacts
} else {
    # Use git tag if version suffix isn't specified
    if ($version_suffix -eq "") {
        $version_suffix = $git_sha
    }

    Print "build" "build"
    dotnet build -c Release --version-suffix=$version_suffix
    AssertLastExitCode

    Print "build" "pack"
    New-Item -ItemType Directory -Path .\artifacts
    dotnet pack -c Release --version-suffix=$version_suffix --no-build
    AssertLastExitCode
    Move-Item -Path .\src\Serilog.Sinks.Http\bin\Release\*.nupkg -Destination .\artifacts
    Move-Item -Path .\src\Serilog.Sinks.Http\bin\Release\*.snupkg -Destination .\artifacts
}

# -------------------------------------------------------------------------------------------------
# TEST
# -------------------------------------------------------------------------------------------------
Print "test" "test started"

dotnet test -c Release --no-build --collect:"XPlat Code Coverage" --settings coverlet.runsettings
AssertLastExitCode

Print "test" "download codecov uploader"
Invoke-WebRequest -Uri https://uploader.codecov.io/latest/codecov.exe -Outfile codecov.exe

foreach ($test_result in Get-ChildItem .\test\Serilog.Sinks.HttpTests\TestResults\*\coverage.cobertura.xml) {
    $relative_test_result = $test_result | Resolve-Path -Relative

    # CodeCode uploader cant handle "\", thus we have to replace these with "/"
    $relative_test_result = $relative_test_result -Replace "\\", "/"

    Print "test" "upload coverage report $relative_test_result"

    .\codecov.exe -f $relative_test_result
    AssertLastExitCode
}
