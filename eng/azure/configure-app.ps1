param([string]$ResourceGroup = 'rg-faithtech-prod')
. "$PSScriptRoot/common.ps1"
$resources = (Invoke-Azure deployment group show -g $ResourceGroup -n faithtech-platform).properties.outputs
$appName = $resources.appName.value
$directory = Join-Path $env:LOCALAPPDATA 'FaithTech/production-secrets'
$certificateFile = Join-Path $directory 'data-protection.pfx'
$certificatePassword = Get-DeploymentSecret 'certificate-password'
if (-not (Test-Path -LiteralPath $certificateFile)) {
    $rsa = [Security.Cryptography.RSA]::Create(3072)
    try {
        $request = [Security.Cryptography.X509Certificates.CertificateRequest]::new('CN=FaithTech Data Protection', $rsa,
            [Security.Cryptography.HashAlgorithmName]::SHA256, [Security.Cryptography.RSASignaturePadding]::Pkcs1)
        $certificate = $request.CreateSelfSigned([DateTimeOffset]::UtcNow.AddMinutes(-5), [DateTimeOffset]::UtcNow.AddYears(3))
        [IO.File]::WriteAllBytes($certificateFile, $certificate.Export([Security.Cryptography.X509Certificates.X509ContentType]::Pfx, $certificatePassword))
        $certificate.Dispose()
    } finally { $rsa.Dispose() }
}
# Upload through the management API: private material stays in a restricted request file.
$certificate = [Security.Cryptography.X509Certificates.X509CertificateLoader]::LoadPkcs12FromFile($certificateFile, $certificatePassword)
$thumbprint = $certificate.Thumbprint
$requestFile = Join-Path $directory 'certificate-request.json'
$settingsFile = Join-Path $directory 'settings.json'
try {
    @{location = 'canadacentral'; properties = @{pfxBlob = [Convert]::ToBase64String([IO.File]::ReadAllBytes($certificateFile));
        password = $certificatePassword; serverFarmId = "$($resources.appId.value.Substring(0, $resources.appId.value.IndexOf('/providers/')))/providers/Microsoft.Web/serverfarms/faithtech-plan"}} |
        ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $requestFile
    & icacls $requestFile /inheritance:r /grant:r "$([Security.Principal.WindowsIdentity]::GetCurrent().Name):(F)" | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Cannot restrict certificate request.' }
    $certificateId = "$($resources.appId.value.Substring(0, $resources.appId.value.IndexOf('/providers/')))/providers/Microsoft.Web/certificates/faithtech-data-protection"
    $null = Invoke-Azure rest --method PUT --url "https://management.azure.com${certificateId}?api-version=2024-11-01" --body "@$requestFile"
    $connection = [System.Data.Common.DbConnectionStringBuilder]::new()
    $connection['Server'] = "tcp:$($resources.sqlHostname.value),1433"; $connection['Database'] = 'FaithTech'
    $connection['User ID'] = 'faithtech_runtime'; $connection['Password'] = Get-DeploymentSecret 'sql-runtime'
    $connection['Encrypt'] = 'True'; $connection['TrustServerCertificate'] = 'False'; $connection['MultipleActiveResultSets'] = 'False'
    $settings = @{ASPNETCORE_ENVIRONMENT = 'Production'; ConnectionStrings__Companion = $connection.ConnectionString;
        Security__DigestKey = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes((Get-DeploymentSecret 'digest')));
        AllowedHosts = $resources.hostname.value; SCM_DO_BUILD_DURING_DEPLOYMENT = 'false'; WEBSITE_RUN_FROM_PACKAGE = '1';
        WEBSITE_LOAD_CERTIFICATES = $thumbprint; Hosting__KeyDirectory = '/home/data/faithtech-keys';
        Hosting__KeyCertificatePath = "/var/ssl/private/$thumbprint.p12";
        Hosting__KnownNetworks__0 = '169.254.0.0/16';
        Logging__LogLevel__Default = 'Information'; Logging__LogLevel__Microsoft_AspNetCore = 'Warning'}
    $settings.Remove('Logging__LogLevel__Microsoft_AspNetCore')
    $settings['Logging__LogLevel__Microsoft.AspNetCore'] = 'Warning'
    $settings | ConvertTo-Json | Set-Content -LiteralPath $settingsFile
    & icacls $settingsFile /inheritance:r /grant:r "$([Security.Principal.WindowsIdentity]::GetCurrent().Name):(F)" | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Cannot restrict runtime settings.' }
    $null = Invoke-Azure webapp config appsettings set -g $ResourceGroup -n $appName --settings "@$settingsFile"
    $webapp = Invoke-Azure webapp show -g $ResourceGroup -n $appName
    foreach ($ip in $webapp.possibleOutboundIpAddresses.Split(',')) {
        $null = Invoke-Azure sql server firewall-rule create -g $ResourceGroup -s $resources.sqlServer.value -n "app-$($ip.Replace('.', '-'))" --start-ip-address $ip --end-ip-address $ip
    }
    $null = Invoke-Azure webapp log config -g $ResourceGroup -n $appName --docker-container-logging filesystem
    Write-Output 'Persistent keys, runtime settings, and restricted SQL access configured.'
} finally {
    $certificate.Dispose()
    Remove-Item -LiteralPath $requestFile,$settingsFile -ErrorAction SilentlyContinue
}
