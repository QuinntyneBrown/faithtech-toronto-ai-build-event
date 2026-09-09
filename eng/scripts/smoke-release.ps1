param([Parameter(Mandatory)][string]$Url, [Parameter(Mandatory)][string]$Revision,
    [switch]$Restart, [string]$AppName, [string]$ResourceGroup, [switch]$TrustLocalCertificate)
$ErrorActionPreference = 'Stop'
Import-Module Microsoft.PowerShell.Utility
if ($env:SMOKE_PASSCODE -notmatch '^\d{4}$') { throw 'Configure the four-digit smoke passcode.' }
if ($TrustLocalCertificate -and ([uri]$Url).Host -notin @('127.0.0.1', 'localhost')) { throw 'Certificate bypass is restricted to loopback.' }
$session = [Microsoft.PowerShell.Commands.WebRequestSession]::new()
function Request([string]$Path, [string]$Method = 'GET', $Body = $null, $Headers = @{}) {
    Write-Host "$Method $Path"
    $parameters = @{Uri = "$Url$Path"; Method = $Method; WebSession = $session; TimeoutSec = 30;
        SkipHttpErrorCheck = $true; Headers = $Headers}
    if ($TrustLocalCertificate) { $parameters.SkipCertificateCheck = $true }
    if ($null -ne $Body) { $parameters.Body = $Body | ConvertTo-Json; $parameters.ContentType = 'application/json' }
    Invoke-WebRequest @parameters
}
function ExpectStatus($Response, [int]$Expected) {
    if ([int]$Response.StatusCode -ne $Expected) { throw "Unexpected HTTP status: $($Response.StatusCode), expected $Expected" }
}
$deadline = [DateTimeOffset]::UtcNow.AddMinutes(3)
for ($attempt = 0; $attempt -lt 36; $attempt++) {
    try { $response = Request '/'; if ($response.StatusCode -eq 200) { break } } catch { }
    if ($attempt -eq 35 -or [DateTimeOffset]::UtcNow -ge $deadline) { throw 'Application startup deadline exceeded.' }
    Start-Sleep -Seconds 5
}
foreach ($path in @('/', '/countdown', '/projects', '/teams', '/raffle')) {
    $response = Request $path
    ExpectStatus $response 200
    if ($response.Content -notmatch '<base href=') { throw "Missing application shell at $path" }
    foreach ($asset in [regex]::Matches($response.Content, '(?:src|href)="([^"/]+\.(?:js|css))"')) {
        ExpectStatus (Request ('/' + $asset.Groups[1].Value)) 200
    }
}
foreach ($path in @('/api/missing', '/missing.js')) { ExpectStatus (Request $path) 404 }
ExpectStatus (Request '/api/health/live') 200
$deadline = [DateTimeOffset]::UtcNow.AddMinutes(3)
for ($attempt = 0; $attempt -lt 36; $attempt++) {
    $response = $null
    try {
        $response = Request '/api/health/ready'
    } catch { }
    if ($response -and $response.StatusCode -eq 200) { break }
    if ($response -and $response.StatusCode -notin @(502, 503, 504)) { ExpectStatus $response 200 }
    if ($attempt -eq 35 -or [DateTimeOffset]::UtcNow -ge $deadline) { throw 'Database readiness deadline exceeded.' }
    Start-Sleep -Seconds 5
}
if ($Restart) {
    & az webapp restart -g $ResourceGroup -n $AppName --only-show-errors
    if ($LASTEXITCODE -ne 0) { throw 'Restart failed.' }
    Start-Sleep -Seconds 5
    ExpectStatus (Request '/api/health/ready') 200
}
$deadline = [DateTimeOffset]::UtcNow.AddMinutes(3)
for ($attempt = 0; $attempt -lt 36; $attempt++) {
    $response = Request '/api/admin/session' 'POST' @{passcode = $env:SMOKE_PASSCODE}
    if ($response.StatusCode -eq 200) { break }
    if ($response.StatusCode -notin @(502, 503, 504)) { ExpectStatus $response 200 }
    if ($attempt -eq 35 -or [DateTimeOffset]::UtcNow -ge $deadline) { throw 'Passcode sign-in deadline exceeded.' }
    Start-Sleep -Seconds 5
}
ExpectStatus (Request '/api/admin/session') 200
ExpectStatus (Request '/api/admin/session' 'DELETE') 204
ExpectStatus (Request '/api/admin/session') 401
Write-Output "Release $Revision passed shell, assets, readiness, passcode, and sign-out checks."
