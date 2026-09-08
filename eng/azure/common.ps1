$ErrorActionPreference = 'Stop'
function Invoke-Azure {
    $output = & az @args --only-show-errors -o json
    if ($LASTEXITCODE -ne 0) { throw "Azure command failed: $($args[0..1] -join ' ')" }
    if ($output) { $output | ConvertFrom-Json }
}
function Invoke-GitHub {
    & gh @args
    if ($LASTEXITCODE -ne 0) { throw "GitHub command failed: $($args[0])" }
}
function Get-DeploymentSecret([string]$Name) {
    if (-not $IsWindows) { throw 'One-time bootstrap uses Windows DPAPI for local secret recovery.' }
    $directory = Join-Path $env:LOCALAPPDATA 'FaithTech/production-secrets'
    New-Item -ItemType Directory -Force $directory | Out-Null
    $path = Join-Path $directory "$Name.dpapi"
    if (-not (Test-Path -LiteralPath $path)) {
        $value = 'Ft9!' + [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(36))
        ConvertTo-SecureString $value -AsPlainText -Force | ConvertFrom-SecureString | Set-Content -LiteralPath $path
    }
    $secret = (Get-Content -LiteralPath $path -Raw).Trim() | ConvertTo-SecureString
    [Net.NetworkCredential]::new('', $secret).Password
}
