param([Parameter(Mandatory)][string]$ToolPath, [ValidateSet('test','production')][string]$Environment = 'production')
$ErrorActionPreference = 'Stop'
if (-not $env:ConnectionStrings__EventDatabase) { throw 'Missing protected migration connection.' }
$connection = [System.Data.Common.DbConnectionStringBuilder]::new()
$connection.set_ConnectionString($env:ConnectionStrings__EventDatabase)
$server = if ($connection.ContainsKey('Server')) { $connection['Server'] } else { $connection['Data Source'] }
$database = if ($connection.ContainsKey('Database')) { $connection['Database'] } else { $connection['Initial Catalog'] }
$directory = Join-Path ([IO.Path]::GetTempPath()) "faithtech-migration-$([guid]::NewGuid().ToString('N'))"
$config = Join-Path $directory 'targets.json'
# Artifacts stay private and available for reconciliation after interrupted deployment.
& dotnet $ToolPath target configure --target release --config $config --server $server --database $database --environment $Environment --connection-env ConnectionStrings__EventDatabase
if ($LASTEXITCODE -ne 0) { throw 'Migration target configuration failed.' }
$previewJson = & dotnet $ToolPath migrate --target release --config $config --preview --json
if ($LASTEXITCODE -ne 0) { throw 'Migration preview failed.' }
$preview = $previewJson | ConvertFrom-Json
Write-Output "Migration operation $($preview.operationId); config $config"
Write-Output "Pending migrations: $($preview.result.pendingMigrations -join ', ')"
& dotnet $ToolPath operations apply $preview.previewId --approve $preview.previewId --target release --config $config --json
if ($LASTEXITCODE -ne 0) { throw "Migration did not confirm success; reconcile $($preview.operationId) using $config." }
