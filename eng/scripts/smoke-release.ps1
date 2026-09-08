param([Parameter(Mandatory)][string]$Url, [Parameter(Mandatory)][string]$Revision,
    [switch]$Restart, [string]$AppName, [string]$ResourceGroup, [switch]$TrustLocalCertificate)
$ErrorActionPreference = 'Stop'
Import-Module Microsoft.PowerShell.Utility
if (-not $env:SMOKE_USERNAME -or -not $env:SMOKE_PASSWORD) { throw 'Configure smoke credentials.' }
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
foreach ($path in @('/', '/admin/sign-in', '/events/00000000-0000-0000-0000-000000000001/schedule')) {
    $response = Request $path
    ExpectStatus $response 200
    if ($response.Content -notmatch '<base href=') { throw "Missing application shell at $path" }
    $prefix = if ($path.StartsWith('/admin')) { '/admin/' } else { '/' }
    foreach ($asset in [regex]::Matches($response.Content, '(?:src|href)="([^"/]+\.(?:js|css))"')) {
        ExpectStatus (Request ($prefix + $asset.Groups[1].Value)) 200
    }
}
foreach ($path in @('/api/missing', '/missing.js', '/admin/missing.js')) { ExpectStatus (Request $path) 404 }
ExpectStatus (Request '/api/admin/readiness') 401
$deadline = [DateTimeOffset]::UtcNow.AddMinutes(3)
for ($attempt = 0; $attempt -lt 36; $attempt++) {
    $csrfResponse = $null; $response = $null
    try {
        $csrfResponse = Request '/api/admin/antiforgery'
        if ($csrfResponse.StatusCode -eq 200) {
            $token = ($csrfResponse.Content | ConvertFrom-Json).requestToken
            $response = Request '/api/admin/session' 'POST' @{username = $env:SMOKE_USERNAME; password = $env:SMOKE_PASSWORD} @{'X-CSRF-TOKEN' = $token}
        }
    } catch { }
    if ($response -and $response.StatusCode -eq 204) { break }
    if ($csrfResponse -and $csrfResponse.StatusCode -notin @(200, 502, 503, 504)) { ExpectStatus $csrfResponse 200 }
    if ($response -and $response.StatusCode -notin @(502, 503, 504)) { ExpectStatus $response 204 }
    if ($attempt -eq 35 -or [DateTimeOffset]::UtcNow -ge $deadline) { throw 'Sign-in startup deadline exceeded.' }
    Start-Sleep -Seconds 5
}
try {
    if ($Restart) {
        $previousInstance = ((Request '/api/admin/readiness').Content | ConvertFrom-Json).instance
        if (-not $previousInstance) { throw 'Restart verification requires a process instance identifier.' }
        & az webapp restart -g $ResourceGroup -n $AppName --only-show-errors
        if ($LASTEXITCODE -ne 0) { throw 'Restart failed.' }
    }
    $deadline = [DateTimeOffset]::UtcNow.AddMinutes(3)
    for ($attempt = 0; $attempt -lt 36; $attempt++) {
        try {
            $response = Request '/api/admin/readiness'
            if ($response.StatusCode -eq 200) {
                $state = $response.Content | ConvertFrom-Json
                if ($state.ready -and $state.revision.EndsWith($Revision) -and (!$Restart -or $state.instance -ne $previousInstance)) { break }
            }
        } catch { }
        if ($attempt -eq 35 -or [DateTimeOffset]::UtcNow -ge $deadline) { throw 'Authenticated SQL/revision readiness deadline exceeded.' }
        Start-Sleep -Seconds 5
    }
} finally {
    $token = ((Request '/api/admin/antiforgery').Content | ConvertFrom-Json).requestToken
    ExpectStatus (Request '/api/admin/session' 'DELETE' $null @{'X-CSRF-TOKEN' = $token}) 204
}
ExpectStatus (Request '/api/admin/readiness') 401
Write-Output "Release $Revision passed HTTPS, assets, authentication, SQL, and sign-out checks."
