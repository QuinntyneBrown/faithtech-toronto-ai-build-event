$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath('artifacts/release')
$database = "FaithTechPackage_$([guid]::NewGuid().ToString('N'))"
$password = 'Ft9!' + [Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(24))
Write-Output "::add-mask::$password"
$certificate = Join-Path $root 'local.pfx'
$process = $null
try {
    & dotnet dev-certs https --export-path $certificate --password $password
    if ($LASTEXITCODE -ne 0) { throw 'Cannot create local HTTPS certificate.' }
    $connection = [System.Data.Common.DbConnectionStringBuilder]::new()
    $connection.ConnectionString = $env:FAITHTECH_TEST_SQL
    $connection['Database'] = $database
    $env:ConnectionStrings__EventDatabase = $connection.ConnectionString
    $env:Security__DigestKey = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
    $env:DOTNET_ENVIRONMENT = 'Production'
    & dotnet "$root/provisioning/FaithTechTorontoAiBuildEvent.Provisioning.dll" migrate
    if ($LASTEXITCODE -ne 0) { throw 'Package database migration failed.' }
    $password | & dotnet "$root/provisioning/FaithTechTorontoAiBuildEvent.Provisioning.dll" create-admin package-smoke
    if ($LASTEXITCODE -ne 0) { throw 'Package account provisioning failed.' }
    $env:ASPNETCORE_ENVIRONMENT = 'Production'
    $env:ASPNETCORE_URLS = 'https://127.0.0.1:5043'
    $env:ASPNETCORE_Kestrel__Certificates__Default__Path = $certificate
    $env:ASPNETCORE_Kestrel__Certificates__Default__Password = $password
    $launch = @{FilePath = 'dotnet'; ArgumentList = "`"$root/api/FaithTechTorontoAiBuildEvent.Api.dll`""; PassThru = $true;
        RedirectStandardOutput = 'artifacts/package.stdout.log'; RedirectStandardError = 'artifacts/package.stderr.log'}
    if ($IsWindows) { $launch.WindowStyle = 'Hidden' }
    $process = Start-Process @launch
    $env:SMOKE_USERNAME = 'package-smoke'; $env:SMOKE_PASSWORD = $password
    $revision = (Get-Content "$root/release.json" -Raw | ConvertFrom-Json).revision
    & "$PSScriptRoot/smoke-release.ps1" -Url 'https://127.0.0.1:5043' -Revision $revision -TrustLocalCertificate
} finally {
    if ($process -and -not $process.HasExited) { Stop-Process -Id $process.Id; $process.WaitForExit() }
    if ($database -notmatch '^FaithTechPackage_[a-f0-9]{32}$') { throw 'Unsafe cleanup database.' }
    & docker exec -e "SQLCMDPASSWORD=$env:MSSQL_SA_PASSWORD" faithtech-ci-sql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -C -b -Q "IF DB_ID('$database') IS NOT NULL BEGIN ALTER DATABASE [$database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$database]; END"
    if ($LASTEXITCODE -ne 0) { throw 'Package database cleanup failed.' }
    Remove-Item -LiteralPath $certificate -ErrorAction SilentlyContinue
    Remove-Item Env:ConnectionStrings__EventDatabase,Env:Security__DigestKey,Env:DOTNET_ENVIRONMENT,Env:ASPNETCORE_ENVIRONMENT,Env:ASPNETCORE_URLS,Env:ASPNETCORE_Kestrel__Certificates__Default__Path,Env:ASPNETCORE_Kestrel__Certificates__Default__Password,Env:SMOKE_USERNAME,Env:SMOKE_PASSWORD -ErrorAction SilentlyContinue
}
