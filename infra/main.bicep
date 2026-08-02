targetScope = 'resourceGroup'

// Parameters
@description('Required. Company name abbreviation used in resource naming (lowercase letters/numbers, short form recommended).')
param companyName string = 'rkt'

@description('Required. Project name abbreviation used in resource naming (lowercase letters/numbers, short form recommended).')
param projectName string = 'securemcp'

@description('Required. Deployment environment abbreviation used in resource naming.')
@allowed([
  'dev'
  'test'
  'qa'
  'stg'
  'prod'
])
param environment string = 'dev'

@description('Optional. Azure location for resources.')
param location string = resourceGroup().location

@description('Optional. Tags applied to all resources.')
param tags object = {
  environment: environment
  project: projectName
  company: companyName
}

// Resource Names
var uniqueSuffix = toLower(substring(uniqueString(subscription().id, resourceGroup().id), 0, 3))
var normalizedCompany = toLower(replace(companyName, '-', ''))
var normalizedProject = toLower(replace(projectName, '-', ''))
var normalizedEnvironment = toLower(replace(environment, '-', ''))

var baseCompact = '${normalizedCompany}${normalizedProject}${normalizedEnvironment}${uniqueSuffix}'
var baseDashed = '${normalizedCompany}-${normalizedProject}-${normalizedEnvironment}-${uniqueSuffix}'

var appServicePlanName = take('${baseDashed}-asp', 60)
var webAppName = take('${baseDashed}-app', 60)
var userAssignedIdentityName = take('${baseDashed}-id', 128)

var keyVaultNameRaw = take('${baseDashed}-kv', 24)
var keyVaultName = endsWith(keyVaultNameRaw, '-') ? '${substring(keyVaultNameRaw, 0, length(keyVaultNameRaw) - 1)}0' : keyVaultNameRaw

var cosmosAccountName = take('${baseCompact}cos', 44)
var cosmosSqlDatabaseName = take('${normalizedEnvironment}-db', 255)
var logAnalyticsWorkspaceName = take('${baseDashed}-law', 63)
var appInsightsName = take('${baseDashed}-appi', 260)

// Variables
var appServicePlanSkuName = 'F1'
var appServicePlanSkuCapacity = 1
var cosmosPublicNetworkAccess = 'Enabled'
var keyVaultPublicNetworkAccess = 'Enabled'
var logAnalyticsSkuName = 'PerGB2018'
var observabilityRetentionDays = 30

// Resources
resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: userAssignedIdentityName
  location: location
  tags: tags
}

module logAnalyticsWorkspace 'br/public:avm/res/operational-insights/workspace:0.16.0' = {
  params: {
    name: logAnalyticsWorkspaceName
    location: location
    skuName: logAnalyticsSkuName
    dataRetention: observabilityRetentionDays
    tags: tags
    enableTelemetry: false
  }
}

module appInsights 'br/public:avm/res/insights/component:0.8.0' = {
  params: {
    name: appInsightsName
    location: location
    workspaceResourceId: logAnalyticsWorkspace.outputs.resourceId
    applicationType: 'web'
    kind: 'web'
    disableLocalAuth: true
    retentionInDays: observabilityRetentionDays
    roleAssignments: [
      {
        principalId: userAssignedIdentity.properties.principalId
        principalType: 'ServicePrincipal'
        roleDefinitionIdOrName: 'Monitoring Metrics Publisher'
      }
    ]
    tags: tags
    enableTelemetry: false
  }
}

module appServicePlan 'br/public:avm/res/web/serverfarm:0.7.0' = {
  params: {
    name: appServicePlanName
    location: location
    kind: 'app'
    skuName: appServicePlanSkuName
    skuCapacity: appServicePlanSkuCapacity
    reserved: false
    zoneRedundant: false
    diagnosticSettings: [
      {
        workspaceResourceId: logAnalyticsWorkspace.outputs.resourceId
      }
    ]
    tags: tags
    enableTelemetry: false
  }
}

module cosmosAccount 'br/public:avm/res/document-db/database-account:0.20.0' = {
  params: {
    name: cosmosAccountName
    location: location
    capabilitiesToAdd: [
      'EnableServerless'
    ]
    enableFreeTier: true
    enableAutomaticFailover: false
    disableLocalAuthentication: true
    disableKeyBasedMetadataWriteAccess: true
    zoneRedundant: false
    sqlDatabases: [
      {
        name: cosmosSqlDatabaseName
      }
    ]
    sqlRoleAssignments: [
      {
        principalId: userAssignedIdentity.properties.principalId
        roleDefinitionId: 'Cosmos DB Built-in Data Contributor'
      }
    ]
    networkRestrictions: {
      publicNetworkAccess: cosmosPublicNetworkAccess
      networkAclBypass: 'AzureServices'
      ipRules: []
      virtualNetworkRules: []
    }
    diagnosticSettings: [
      {
        workspaceResourceId: logAnalyticsWorkspace.outputs.resourceId
      }
    ]
    tags: tags
    enableTelemetry: false
  }
}

module keyVault 'br/public:avm/res/key-vault/vault:0.14.0' = {
  params: {
    name: keyVaultName
    location: location
    sku: 'standard'
    enableRbacAuthorization: true
    enablePurgeProtection: false
    publicNetworkAccess: keyVaultPublicNetworkAccess
    roleAssignments: [
      {
        principalId: userAssignedIdentity.properties.principalId
        principalType: 'ServicePrincipal'
        roleDefinitionIdOrName: 'Key Vault Secrets User'
      }
    ]
    diagnosticSettings: [
      {
        workspaceResourceId: logAnalyticsWorkspace.outputs.resourceId
      }
    ]
    tags: tags
    enableTelemetry: false
  }
}

module webApp 'br/public:avm/res/web/site:0.24.0' = {
  params: {
    name: webAppName
    location: location
    kind: 'app'
    serverFarmResourceId: appServicePlan.outputs.resourceId
    httpsOnly: true
    managedIdentities: {
      userAssignedResourceIds: [
        userAssignedIdentity.id
      ]
    }
    keyVaultAccessIdentityResourceId: userAssignedIdentity.id
    siteConfig: {
      alwaysOn: false
      minTlsVersion: '1.2'
      ftpsState: 'FtpsOnly'
    }
    configs: [
      {
        name: 'appsettings'
        properties: {
          ASPNETCORE_ENVIRONMENT: environment
          KEYVAULT__URI: keyVault.outputs.uri
          CosmosDatabase__AccountEndpoint: cosmosAccount.outputs.endpoint
          CosmosDatabase__ManagedIdentityClientId: userAssignedIdentity.properties.clientId
          CosmosDatabase__DatabaseName: 'DemoLibrary'
          CosmosDatabase__EndpointActivity: 'EndpointActivity'
          CosmosDatabase__Books: 'Books'
          CosmosDatabase__Loans: 'Loans'
          CosmosDatabase__EnsureCreated: 'true'
          ActivityLogging__CaptureRequestBody: 'true'
          ActivityLogging__CaptureResponseBody: 'true'
          ActivityLogging__MaxBodyLength: '16384'
          APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.outputs.connectionString
          APPLICATIONINSIGHTS_AUTHENTICATION_STRING: 'Authorization=AAD;ClientId=${userAssignedIdentity.properties.clientId}'
          AZURE_CLIENT_ID: userAssignedIdentity.properties.clientId
          MANAGED_IDENTITY_CLIENT_ID: userAssignedIdentity.properties.clientId
        }
      }
    ]
    diagnosticSettings: [
      {
        workspaceResourceId: logAnalyticsWorkspace.outputs.resourceId
      }
    ]
    publicNetworkAccess: 'Enabled'
    tags: tags
    enableTelemetry: false
  }
}

// Outputs
@description('Calculated App Service Plan name.')
output appServicePlanNameOutput string = appServicePlanName

@description('Calculated Web App name.')
output webAppNameOutput string = webAppName

@description('Calculated User Assigned Managed Identity name.')
output managedIdentityNameOutput string = userAssignedIdentityName

@description('Calculated Cosmos DB account name.')
output cosmosAccountNameOutput string = cosmosAccountName

@description('Calculated Cosmos SQL database name.')
output cosmosSqlDatabaseNameOutput string = cosmosSqlDatabaseName

@description('Calculated Key Vault name.')
output keyVaultNameOutput string = keyVaultName

@description('Deployed Web App resource ID.')
output webAppResourceId string = webApp.outputs.resourceId

@description('Deployed App Service Plan resource ID.')
output appServicePlanResourceId string = appServicePlan.outputs.resourceId

@description('Deployed User Assigned Managed Identity resource ID.')
output managedIdentityResourceId string = userAssignedIdentity.id

@description('Managed Identity client ID.')
output managedIdentityClientId string = userAssignedIdentity.properties.clientId

@description('Managed Identity principal ID.')
output managedIdentityPrincipalId string = userAssignedIdentity.properties.principalId

@description('Cosmos DB account endpoint URI.')
output cosmosEndpoint string = cosmosAccount.outputs.endpoint

@description('Cosmos DB resource ID.')
output cosmosResourceId string = cosmosAccount.outputs.resourceId

@description('Calculated Log Analytics workspace name.')
output logAnalyticsWorkspaceNameOutput string = logAnalyticsWorkspaceName

@description('Log Analytics workspace resource ID.')
output logAnalyticsWorkspaceResourceId string = logAnalyticsWorkspace.outputs.resourceId

@description('Calculated Application Insights name.')
output appInsightsNameOutput string = appInsightsName

@description('Application Insights resource ID.')
output appInsightsResourceId string = appInsights.outputs.resourceId

@description('Key Vault URI.')
output keyVaultUri string = keyVault.outputs.uri

@description('Key Vault resource ID.')
output keyVaultResourceId string = keyVault.outputs.resourceId
