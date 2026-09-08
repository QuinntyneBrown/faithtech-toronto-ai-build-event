[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$dotnet = Join-Path $env:LOCALAPPDATA 'FaithTech/dotnet/dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnet)) { $dotnet = 'dotnet' }
$project = Join-Path $repositoryRoot 'backend/src/FaithTechTorontoAiBuildEvent.Provisioning/FaithTechTorontoAiBuildEvent.Provisioning.csproj'
$packageDirectory = Join-Path $repositoryRoot '.local/packages/operator'
$version = "1.0.0-local.$(Get-Date -Format 'yyyyMMddHHmmss')"

New-Item -ItemType Directory -Force -Path $packageDirectory | Out-Null
& $dotnet restore $project --locked-mode
& $dotnet build $project --configuration Release --no-restore
& $dotnet pack $project --configuration Release --no-build --output $packageDirectory "/p:Version=$version"

$installed = & $dotnet tool list --global | Select-String '^faithtechtorontoaibuildevent\.provisioning\s'
if ($installed) {
    & $dotnet tool update --global FaithTechTorontoAiBuildEvent.Provisioning --version $version --add-source $packageDirectory --ignore-failed-sources
}
else {
    & $dotnet tool install --global FaithTechTorontoAiBuildEvent.Provisioning --version $version --add-source $packageDirectory --ignore-failed-sources
}

& faithtech-admin --version
