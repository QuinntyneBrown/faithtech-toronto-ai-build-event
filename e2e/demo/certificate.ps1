param([string]$Destination)
$ErrorActionPreference = 'Stop'
$store = New-Object System.Security.Cryptography.X509Certificates.X509Store('My', 'CurrentUser')
$store.Open('ReadOnly')
$certificate = $store.Certificates |
    Where-Object { $_.Subject -eq 'CN=localhost' -and $_.HasPrivateKey -and $_.NotAfter -gt (Get-Date) } |
    Sort-Object NotAfter -Descending | Select-Object -First 1
if (-not $certificate) { throw 'Install a local HTTPS development certificate before recording.' }
[IO.File]::WriteAllBytes($Destination, $certificate.Export('Pfx', $env:FAITHTECH_DEMO_CERT_PASSWORD))
$store.Close()
