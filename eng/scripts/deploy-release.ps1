param([string]$PackageDirectory = 'artifacts/release', [switch]$Rollback)
$ErrorActionPreference = 'Stop'
function Azure {
    $value = & az @args --only-show-errors -o json
    if ($LASTEXITCODE -ne 0) { throw "Azure command failed: $($args[0..1] -join ' ')" }
    if ($value) { $value | ConvertFrom-Json }
}
$manifest = Get-Content "$PackageDirectory/release.json" -Raw | ConvertFrom-Json
if ($manifest.revision -notmatch '^[a-f0-9]{40}$') { throw 'Invalid release identity.' }
$hash = (Get-FileHash "$PackageDirectory/web.zip" -Algorithm SHA256).Hash.ToLowerInvariant()
if ($hash -ne $manifest.sha256) { throw 'Release package hash mismatch.' }
if (-not $Rollback -and $manifest.revision -ne $env:GITHUB_SHA) { throw 'Release does not match this push.' }
$app = Azure webapp show -g $env:AZURE_RESOURCE_GROUP -n $env:AZURE_APP_NAME
if (-not $Rollback -and [long]$app.tags.releaseRunNumber -ge [long]$env:GITHUB_RUN_NUMBER) {
    throw 'This release is older than or equal to an already successful deployment.'
}
if (-not $Rollback) {
    if (-not $env:MIGRATION_CONNECTION_STRING -or -not $env:PRODUCTION_DIGEST_KEY) { throw 'Missing migration configuration.' }
    $address = (Invoke-RestMethod 'https://api.ipify.org').Trim()
    if (-not [Net.IPAddress]::TryParse($address, [ref]([Net.IPAddress]$null))) { throw 'Invalid runner IP.' }
    $rule = "github-$env:GITHUB_RUN_ID-$env:GITHUB_RUN_ATTEMPT"
    try {
        $null = Azure sql server firewall-rule create -g $env:AZURE_RESOURCE_GROUP -s $env:AZURE_SQL_SERVER -n $rule --start-ip-address $address --end-ip-address $address
        $env:ConnectionStrings__EventDatabase = $env:MIGRATION_CONNECTION_STRING
        $env:Security__DigestKey = $env:PRODUCTION_DIGEST_KEY
        $env:DOTNET_ENVIRONMENT = 'Production'
        & dotnet "$PackageDirectory/provisioning/FaithTechTorontoAiBuildEvent.Provisioning.dll" migrate
        if ($LASTEXITCODE -ne 0) { throw 'Migration failed; package has not been deployed.' }
    } finally {
        Remove-Item Env:ConnectionStrings__EventDatabase,Env:Security__DigestKey,Env:DOTNET_ENVIRONMENT -ErrorAction SilentlyContinue
        $null = Azure sql server firewall-rule delete -g $env:AZURE_RESOURCE_GROUP -s $env:AZURE_SQL_SERVER -n $rule
    }
}
$null = Azure webapp deploy -g $env:AZURE_RESOURCE_GROUP -n $env:AZURE_APP_NAME --src-path "$PackageDirectory/web.zip" --type zip --timeout 600000 --track-status false
& "$PSScriptRoot/smoke-release.ps1" -Url $env:AZURE_WEBAPP_URL -Revision $manifest.revision
$tag = if ($Rollback) { $app.tags.releaseRunNumber } else { $env:GITHUB_RUN_NUMBER }
$null = Azure webapp update -g $env:AZURE_RESOURCE_GROUP -n $env:AZURE_APP_NAME --set "tags.releaseCommit=$($manifest.revision)" "tags.releaseRunNumber=$tag"
"Deployed $($manifest.revision) to $env:AZURE_WEBAPP_URL" | Out-File -FilePath $env:GITHUB_STEP_SUMMARY -Append
