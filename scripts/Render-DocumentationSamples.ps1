[CmdletBinding()]
param(
    [string] $Configuration,
    [switch] $NoRestore,
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]] $DotnetTestArguments
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$environmentVariableName = "PAPERCRAFT_UPDATE_DOCUMENTATION_SAMPLE_ASSETS"
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).ProviderPath
$testProject = Join-Path $repositoryRoot "test/X39.Solutions.PdfTemplate.Test/X39.Solutions.PdfTemplate.Test.csproj"
$filter = "FullyQualifiedName~DocumentationSamples"
$previousEnvironmentValue = [Environment]::GetEnvironmentVariable($environmentVariableName, "Process")

$arguments = @(
    "test",
    $testProject,
    "--filter",
    $filter
)

if (-not [string]::IsNullOrWhiteSpace($Configuration)) {
    $arguments += @("--configuration", $Configuration)
}

if ($NoRestore) {
    $arguments += "--no-restore"
}

if ($DotnetTestArguments) {
    $arguments += $DotnetTestArguments
}

try {
    [Environment]::SetEnvironmentVariable($environmentVariableName, "true", "Process")
    Write-Host "Regenerating documentation sample assets in docs/assets/samples..."
    & dotnet @arguments
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}
finally {
    [Environment]::SetEnvironmentVariable($environmentVariableName, $previousEnvironmentValue, "Process")
}
