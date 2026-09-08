param([string]$ResourceGroup = 'rg-faithtech-prod')
. "$PSScriptRoot/common.ps1"
$account = Invoke-Azure account show
if ($account.id -ne '4a1b5113-89f9-4d27-acfe-581493385536') { throw 'Select the FaithTech deployment subscription first.' }
$null = Invoke-Azure group create --name $ResourceGroup --location canadacentral
$directory = Join-Path $env:LOCALAPPDATA 'FaithTech/production-secrets'
$password = Get-DeploymentSecret 'sql-administrator'
$parameterFile = Join-Path $directory 'parameters.json'
try {
    @{sqlAdminPassword = @{value = $password}} | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $parameterFile
    & icacls $parameterFile /inheritance:r /grant:r "$([Security.Principal.WindowsIdentity]::GetCurrent().Name):(F)" | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Could not restrict provisioning parameters.' }
    $deployment = Invoke-Azure deployment group create --resource-group $ResourceGroup --name faithtech-platform --template-file "$PSScriptRoot/main.bicep" --parameters "@$parameterFile"
    $deployment.properties.outputs | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $directory 'resources.json')
    $deployment.properties.outputs
} finally { Remove-Item -LiteralPath $parameterFile -ErrorAction SilentlyContinue }
