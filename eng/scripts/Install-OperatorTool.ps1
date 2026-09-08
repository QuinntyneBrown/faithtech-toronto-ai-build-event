[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$dotnet = Join-Path $env:LOCALAPPDATA 'FaithTech/dotnet/dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnet)) { $dotnet = 'dotnet' }
else { $env:DOTNET_ROOT = Split-Path $dotnet }
$project = Join-Path $repositoryRoot 'backend/src/FaithTechTorontoAiBuildEvent.Provisioning/FaithTechTorontoAiBuildEvent.Provisioning.csproj'
$packageDirectory = Join-Path $repositoryRoot '.local/packages/operator'
$version = "1.0.0-local.$(Get-Date -Format 'yyyyMMddHHmmss')"

New-Item -ItemType Directory -Force -Path $packageDirectory | Out-Null
& $dotnet restore $project --locked-mode
if ($LASTEXITCODE -ne 0) { throw 'Restore failed; the installed tool was not changed.' }
& $dotnet build $project --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Build failed; the installed tool was not changed.' }
& $dotnet pack $project --configuration Release --no-build --output $packageDirectory "/p:Version=$version"
if ($LASTEXITCODE -ne 0) { throw 'Packaging failed; the installed tool was not changed.' }

$installed = & $dotnet tool list --global | Select-String '^faithtechtorontoaibuildevent\.provisioning\s'
if ($LASTEXITCODE -ne 0) { throw 'Unable to inspect installed tools.' }
if ($installed) {
    & $dotnet tool update --global FaithTechTorontoAiBuildEvent.Provisioning --version $version --add-source $packageDirectory --ignore-failed-sources
}
else {
    & $dotnet tool install --global FaithTechTorontoAiBuildEvent.Provisioning --version $version --add-source $packageDirectory --ignore-failed-sources
}
if ($LASTEXITCODE -ne 0) { throw 'Tool installation/update failed.' }

$tool = Join-Path $env:USERPROFILE '.dotnet/tools/faithtech-admin.exe'
& $tool --version
if ($LASTEXITCODE -ne 0) { throw 'Installed tool verification failed.' }
