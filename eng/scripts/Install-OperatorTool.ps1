param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$project = Join-Path $root 'backend/src/FaithTechTorontoAiBuildEvent.Provisioning/FaithTechTorontoAiBuildEvent.Provisioning.csproj'
$packages = Join-Path ([System.IO.Path]::GetTempPath()) ('faithtech-tool-' + [Guid]::NewGuid())
New-Item -ItemType Directory -Force -Path $packages | Out-Null

try {
    dotnet pack $project --configuration Release --output $packages
    $package = Get-ChildItem -LiteralPath $packages -Filter 'FaithTechTorontoAiBuildEvent.Provisioning.*.nupkg' | Select-Object -First 1
    if ($null -eq $package) { throw 'The operator package was not created.' }
    $version = ($package.BaseName -replace '^FaithTechTorontoAiBuildEvent.Provisioning\.', '')
    $installed = dotnet tool list --global | Select-String -SimpleMatch 'faithtech-admin'
    if ($null -eq $installed) {
        dotnet tool install --global FaithTechTorontoAiBuildEvent.Provisioning --version $version --add-source $packages
    } else {
        dotnet tool update --global FaithTechTorontoAiBuildEvent.Provisioning --version $version --add-source $packages --allow-downgrade
    }
    faithtech-admin --help
} finally {
    Remove-Item -LiteralPath $packages -Recurse -Force -ErrorAction SilentlyContinue
}
