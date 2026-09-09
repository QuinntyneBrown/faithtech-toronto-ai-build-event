param([string]$ResourceGroup = 'rg-faithtech-prod', [string]$Repository = 'QuinntyneBrown/faithtech-toronto-ai-build-event')
. "$PSScriptRoot/common.ps1"
$resources = (Invoke-Azure deployment group show -g $ResourceGroup -n faithtech-platform).properties.outputs
$sqlServer = $resources.sqlServer.value
$address = (Invoke-RestMethod 'https://api.ipify.org').Trim()
if (-not [Net.IPAddress]::TryParse($address, [ref]([Net.IPAddress]$null))) { throw 'Invalid operator IP.' }
$rule = "bootstrap-$([guid]::NewGuid().ToString('N'))"
$null = Invoke-Azure sql server firewall-rule create -g $ResourceGroup -s $sqlServer -n $rule --start-ip-address $address --end-ip-address $address
try {
    $builder = [System.Data.Common.DbConnectionStringBuilder]::new()
    $builder['Server'] = "tcp:$($resources.sqlHostname.value),1433"
    $builder['Database'] = 'FaithTech'
    $builder['User ID'] = 'faithtech_provisioner'
    $builder['Password'] = Get-DeploymentSecret 'sql-administrator'
    $builder['Encrypt'] = 'True'
    $builder['TrustServerCertificate'] = 'False'
    $builder['MultipleActiveResultSets'] = 'False'
    $env:ConnectionStrings__EventDatabase = $builder.ConnectionString
    # Use a base64 digest with at least 32 bytes; preserve it across reruns.
    $digest = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes((Get-DeploymentSecret 'digest')))
    $env:Security__DigestKey = $digest
    $env:DOTNET_ENVIRONMENT = 'Production'
    & "$PSScriptRoot/../scripts/Invoke-ReviewedMigration.ps1" -ToolPath artifacts/release/provisioning/FaithTechTorontoAiBuildEvent.Provisioning.dll
    $env:SQLCMDPASSWORD = Get-DeploymentSecret 'sql-administrator'
    $sqlArguments = @('-S', $resources.sqlHostname.value, '-d', 'FaithTech', '-U', 'faithtech_provisioner', '-b', '-l', '30')
    foreach ($role in @('runtime', 'migration')) {
        $password = Get-DeploymentSecret "sql-$role"
        $permissions = if ($role -eq 'runtime') { 'db_datareader', 'db_datawriter' } else { 'db_owner' }
        $sql = "IF DATABASE_PRINCIPAL_ID('faithtech_$role') IS NULL CREATE USER [faithtech_$role] WITH PASSWORD = '$($password.Replace("'", "''"))';"
        foreach ($permission in $permissions) { $sql += " ALTER ROLE [$permission] ADD MEMBER [faithtech_$role];" }
        $sql | & sqlcmd @sqlArguments
        if ($LASTEXITCODE -ne 0) { throw 'Database identity setup failed.' }
    }
    $count = & sqlcmd @sqlArguments -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM AspNetUsers WHERE NormalizedUserName = 'DEPLOYMENT-SMOKE'"
    if ($LASTEXITCODE -ne 0) { throw 'Could not check smoke account.' }
    if ([int]($count.Trim()) -eq 0) {
        Get-DeploymentSecret 'smoke-password' | & dotnet artifacts/release/provisioning/FaithTechTorontoAiBuildEvent.Provisioning.dll create-admin deployment-smoke
        if ($LASTEXITCODE -ne 0) { throw 'Smoke account provisioning failed.' }
    }
    $builder['User ID'] = 'faithtech_migration'; $builder['Password'] = Get-DeploymentSecret 'sql-migration'
    $secrets = @{MIGRATION_CONNECTION_STRING = $builder.ConnectionString; PRODUCTION_DIGEST_KEY = $digest;
        SMOKE_USERNAME = 'deployment-smoke'; SMOKE_PASSWORD = (Get-DeploymentSecret 'smoke-password')}
    foreach ($entry in $secrets.GetEnumerator()) {
        $entry.Value | gh secret set $entry.Key --env production --repo $Repository
        if ($LASTEXITCODE -ne 0) { throw "Failed to store $($entry.Key)" }
    }
    Write-Output 'Database migrated; runtime, migration, and smoke identities configured.'
} finally {
    Remove-Item Env:SQLCMDPASSWORD,Env:ConnectionStrings__EventDatabase,Env:Security__DigestKey,Env:DOTNET_ENVIRONMENT -ErrorAction SilentlyContinue
    $null = Invoke-Azure sql server firewall-rule delete -g $ResourceGroup -s $sqlServer -n $rule
}
