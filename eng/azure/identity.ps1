param([string]$Repository = 'QuinntyneBrown/faithtech-toronto-ai-build-event', [string]$ResourceGroup = 'rg-faithtech-prod')
. "$PSScriptRoot/common.ps1"
$account = Invoke-Azure account show
$resources = (Invoke-Azure deployment group show -g $ResourceGroup -n faithtech-platform).properties.outputs
$name = 'faithtech-github-production'
$apps = @(Invoke-Azure ad app list --display-name $name)
if ($apps.Count -gt 1) { throw 'Multiple deployment identities match.' }
$identity = if ($apps.Count) { $apps[0] } else { Invoke-Azure ad app create --display-name $name }
$principals = @(Invoke-Azure ad sp list --filter "appId eq '$($identity.appId)'")
$principal = if ($principals.Count) { $principals[0] } else { Invoke-Azure ad sp create --id $identity.appId }
$directory = Join-Path $env:LOCALAPPDATA 'FaithTech/production-secrets'
$federationFile = Join-Path $directory 'federation.json'
$oidc = Invoke-GitHub api "repos/$Repository/actions/oidc/customization/sub" | ConvertFrom-Json
if (-not $oidc.use_default -or -not $oidc.sub_claim_prefix) { throw 'Configure the repository default OIDC subject before bootstrap.' }
@{name = 'github-production'; issuer = 'https://token.actions.githubusercontent.com';
  subject = "$($oidc.sub_claim_prefix):environment:production"; audiences = @('api://AzureADTokenExchange')} |
    ConvertTo-Json | Set-Content -LiteralPath $federationFile
$federations = @(Invoke-Azure ad app federated-credential list --id $identity.id)
$federation = $federations | Where-Object name -eq 'github-production'
if ($federation) {
    $null = Invoke-Azure ad app federated-credential update --id $identity.id --federated-credential-id $federation.id --parameters "@$federationFile"
} else {
    $null = Invoke-Azure ad app federated-credential create --id $identity.id --parameters "@$federationFile"
}
$null = Invoke-Azure role assignment create --assignee-object-id $principal.id --assignee-principal-type ServicePrincipal --role 'Website Contributor' --scope $resources.appId.value
$roleName = 'FaithTech deployment SQL firewall'
$roleFile = Join-Path $directory 'firewall-role.json'
@{Name = $roleName; IsCustom = $true; Description = 'Manage temporary runner firewall rules for FaithTech migrations';
  Actions = @('Microsoft.Sql/servers/read', 'Microsoft.Sql/servers/firewallRules/read', 'Microsoft.Sql/servers/firewallRules/write', 'Microsoft.Sql/servers/firewallRules/delete');
  NotActions = @(); AssignableScopes = @("/subscriptions/$($account.id)/resourceGroups/$ResourceGroup")} |
    ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $roleFile
$roles = @(Invoke-Azure role definition list --name $roleName)
if (-not $roles.Count) { $null = Invoke-Azure role definition create --role-definition $roleFile }
$null = Invoke-Azure role assignment create --assignee-object-id $principal.id --assignee-principal-type ServicePrincipal --role $roleName --scope $resources.sqlId.value
$environmentFile = Join-Path $directory 'environment.json'
@{deployment_branch_policy = @{protected_branches = $false; custom_branch_policies = $true}} |
    ConvertTo-Json | Set-Content -LiteralPath $environmentFile
$null = Invoke-GitHub api --method PUT "repos/$Repository/environments/production" --input $environmentFile
$policies = gh api "repos/$Repository/environments/production/deployment-branch-policies" | ConvertFrom-Json
if ($LASTEXITCODE -ne 0) { throw 'Cannot read deployment branch policies.' }
if (-not ($policies.branch_policies | Where-Object name -eq 'main')) {
    $null = Invoke-GitHub api --method POST "repos/$Repository/environments/production/deployment-branch-policies" -f name=main -f type=branch
}
$variables = @{AZURE_CLIENT_ID = $identity.appId; AZURE_TENANT_ID = $account.tenantId;
  AZURE_SUBSCRIPTION_ID = $account.id; AZURE_APP_NAME = $resources.appName.value;
  AZURE_RESOURCE_GROUP = $ResourceGroup; AZURE_SQL_SERVER = $resources.sqlServer.value;
  AZURE_WEBAPP_URL = "https://$($resources.hostname.value)"}
foreach ($entry in $variables.GetEnumerator()) {
    Invoke-GitHub variable set $entry.Key --env production --repo $Repository --body $entry.Value
}
Write-Output 'Production federation, scoped roles, and GitHub variables configured.'
