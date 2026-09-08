param([Parameter(Mandatory)][string]$Url, [Parameter(Mandatory)][string]$Revision,
    [switch]$Restart, [string]$AppName, [string]$ResourceGroup)
$ErrorActionPreference = 'Stop'
if (-not $env:SMOKE_USERNAME -or -not $env:SMOKE_PASSWORD) { throw 'Configure smoke credentials.' }
$session = [Microsoft.PowerShell.Commands.WebRequestSession]::new()
function Request([string]$Path, [string]$Method = 'GET', $Body = $null, $Headers = @{}) {
    $parameters = @{Uri = "$Url$Path"; Method = $Method; WebSession = $session; TimeoutSec = 15;
        SkipHttpErrorCheck = $true; Headers = $Headers}
    if ($null -ne $Body) { $parameters.Body = $Body | ConvertTo-Json; $parameters.ContentType = 'application/json' }
    Invoke-WebRequest @parameters
}
function ExpectStatus($Response, [int]$Expected) {
    if ([int]$Response.StatusCode -ne $Expected) { throw "Unexpected HTTP status: $($Response.StatusCode), expected $Expected" }
}
for ($attempt = 0; $attempt -lt 36; $attempt++) {
    try { $response = Request '/'; if ($response.StatusCode -eq 200) { break } } catch { }
    if ($attempt -eq 35) { throw 'Application startup deadline exceeded.' }
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
$csrfResponse = Request '/api/admin/antiforgery'; ExpectStatus $csrfResponse 200
$token = ($csrfResponse.Content | ConvertFrom-Json).requestToken
$headers = @{'X-CSRF-TOKEN' = $token}
ExpectStatus (Request '/api/admin/session' 'POST' @{username = $env:SMOKE_USERNAME; password = $env:SMOKE_PASSWORD} $headers) 204
try {
    if ($Restart) {
        & az webapp restart -g $ResourceGroup -n $AppName --only-show-errors
        if ($LASTEXITCODE -ne 0) { throw 'Restart failed.' }
    }
    for ($attempt = 0; $attempt -lt 36; $attempt++) {
        try {
            $response = Request '/api/admin/readiness'
            if ($response.StatusCode -eq 200) {
                $state = $response.Content | ConvertFrom-Json
                if ($state.ready -and $state.revision.EndsWith($Revision)) { break }
            }
        } catch { }
        if ($attempt -eq 35) { throw 'Authenticated SQL/revision readiness deadline exceeded.' }
        Start-Sleep -Seconds 5
    }
} finally {
    $token = ((Request '/api/admin/antiforgery').Content | ConvertFrom-Json).requestToken
    ExpectStatus (Request '/api/admin/session' 'DELETE' $null @{'X-CSRF-TOKEN' = $token}) 204
}
ExpectStatus (Request '/api/admin/readiness') 401
Write-Output "Release $Revision passed HTTPS, assets, authentication, SQL, and sign-out checks."
