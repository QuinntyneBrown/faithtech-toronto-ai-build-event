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
    $env:ConnectionStrings__Companion = $builder.ConnectionString
    $env:ASPNETCORE_ENVIRONMENT = 'Production'
    & dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw 'Could not restore the migration tool.' }
    & dotnet ef database update --project backend/src/FaithTechTorontoAiBuildEvent.Infrastructure --startup-project backend/src/FaithTechTorontoAiBuildEvent.Api
    if ($LASTEXITCODE -ne 0) { throw 'Initial database migration failed.' }
    $env:SQLCMDPASSWORD = Get-DeploymentSecret 'sql-administrator'
    $sqlArguments = @('-S', $resources.sqlHostname.value, '-d', 'FaithTech', '-U', 'faithtech_provisioner', '-b', '-l', '30')
    $runtimePassword = Get-DeploymentSecret 'sql-runtime'
    $sql = "IF DATABASE_PRINCIPAL_ID('faithtech_runtime') IS NULL CREATE USER [faithtech_runtime] WITH PASSWORD = '$($runtimePassword.Replace("'", "''"))'; IF IS_ROLEMEMBER('db_owner', 'faithtech_runtime') <> 1 ALTER ROLE [db_owner] ADD MEMBER [faithtech_runtime];"
    $sql | & sqlcmd @sqlArguments
    if ($LASTEXITCODE -ne 0) { throw 'Runtime database identity setup failed.' }
    $smokePasscode = Get-DeploymentSecret 'smoke-passcode'
    if ($smokePasscode -notmatch '^\d{4}$') { throw 'The smoke passcode must contain four digits.' }
    "EXEC dbo.ReplaceAdminPasscode @Passcode = N'$smokePasscode';" | & sqlcmd @sqlArguments
    if ($LASTEXITCODE -ne 0) { throw 'Smoke passcode provisioning failed.' }
    $secrets = @{SMOKE_PASSCODE = $smokePasscode}
    foreach ($entry in $secrets.GetEnumerator()) {
        $entry.Value | gh secret set $entry.Key --env production --repo $Repository
        if ($LASTEXITCODE -ne 0) { throw "Failed to store $($entry.Key)" }
    }
    Write-Output 'Database migrated; runtime identity and smoke passcode configured.'
} finally {
    Remove-Item Env:SQLCMDPASSWORD,Env:ConnectionStrings__Companion,Env:ASPNETCORE_ENVIRONMENT -ErrorAction SilentlyContinue
    $null = Invoke-Azure sql server firewall-rule delete -g $ResourceGroup -s $sqlServer -n $rule
}
