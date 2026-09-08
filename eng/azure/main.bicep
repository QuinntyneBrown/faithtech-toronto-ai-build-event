param location string = 'canadacentral'
@secure()
param sqlAdminPassword string
param sqlAdminName string = 'faithtech_provisioner'
var suffix = uniqueString(subscription().subscriptionId, resourceGroup().id)

resource plan 'Microsoft.Web/serverfarms@2024-11-01' = {
  name: 'faithtech-plan'
  location: location
  kind: 'linux'
  sku: { name: 'B1', tier: 'Basic', capacity: 1 }
  properties: { reserved: true }
}
resource app 'Microsoft.Web/sites@2024-11-01' = {
  name: 'faithtech-${suffix}'
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: true
      minTlsVersion: '1.2'
      scmMinTlsVersion: '1.2'
      ftpsState: 'Disabled'
      appCommandLine: 'dotnet /home/site/wwwroot/FaithTechTorontoAiBuildEvent.Api.dll'
    }
  }
}
resource sql 'Microsoft.Sql/servers@2023-08-01' = {
  name: 'faithtech-sql-${suffix}'
  location: location
  properties: {
    administratorLogin: sqlAdminName
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}
resource database 'Microsoft.Sql/servers/databases@2023-08-01' = {
  parent: sql
  name: 'FaithTech'
  location: location
  sku: { name: 'Basic', tier: 'Basic', capacity: 5 }
  properties: {
    maxSizeBytes: 2147483648
    requestedBackupStorageRedundancy: 'Local'
    zoneRedundant: false
  }
}
resource retention 'Microsoft.Sql/servers/databases/backupShortTermRetentionPolicies@2023-08-01' = {
  parent: database
  name: 'default'
  properties: { retentionDays: 7 }
}
output appName string = app.name
output appId string = app.id
output hostname string = app.properties.defaultHostName
output sqlServer string = sql.name
output sqlId string = sql.id
output sqlHostname string = sql.properties.fullyQualifiedDomainName
