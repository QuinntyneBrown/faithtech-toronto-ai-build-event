param([Parameter(Mandatory)][string]$Revision, [string]$Output = 'artifacts/release')
$ErrorActionPreference = 'Stop'
if ($Revision -notmatch '^[a-f0-9]{40}$') { throw 'A complete source commit is required.' }
$outputPath = [IO.Path]::GetFullPath($Output)
if (Test-Path -LiteralPath $outputPath) { throw 'Use a fresh release output directory.' }
New-Item -ItemType Directory $outputPath | Out-Null
foreach ($project in @('Api', 'Provisioning')) {
    $destination = if ($project -eq 'Api') { 'api' } else { 'provisioning' }
    & dotnet publish "backend/src/FaithTechTorontoAiBuildEvent.$project" -c Release --no-restore -p:UseAppHost=false "-p:SourceRevisionId=$Revision" -o "$outputPath/$destination"
    if ($LASTEXITCODE -ne 0) { throw "Publish failed: $project" }
}
foreach ($file in @('FaithTechTorontoAiBuildEvent.Api.dll', 'wwwroot/index.html', 'wwwroot/admin/index.html')) {
    if (-not (Test-Path -LiteralPath "$outputPath/api/$file")) { throw "Missing publish output: $file" }
}
Compress-Archive -Path "$outputPath/api/*" -DestinationPath "$outputPath/web.zip"
@{revision = $Revision; sha256 = (Get-FileHash "$outputPath/web.zip" -Algorithm SHA256).Hash.ToLowerInvariant()} |
    ConvertTo-Json | Set-Content -LiteralPath "$outputPath/release.json"
Write-Output "Packaged release $Revision"
