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

@description('Required. Entra tenant ID for authentication.')
param entraTenantId string

@description('Required. Entra app registration client ID used as the API audience (api://<client-id>).')
param entraAudienceClientId string

@description('Optional. Azure AI model name to deploy. Choose a currently available low-cost model for your region (for example gpt-4.1-mini).')
param aiFoundryModelName string = 'gpt-4.1-mini'

@description('Optional. Model format for Azure AI model deployment.')
param aiFoundryModelFormat string = 'OpenAI'

// Resource Names
var uniqueSuffix = toLower(substring(uniqueString(location, subscription().id, resourceGroup().id), 0, 3))
var normalizedCompany = toLower(replace(companyName, '-', ''))
var normalizedProject = toLower(replace(projectName, '-', ''))
var normalizedEnvironment = toLower(replace(environment, '-', ''))

var baseDashed = '${normalizedCompany}-${normalizedProject}${uniqueSuffix}-${normalizedEnvironment}'
var baseCompact = replace(baseDashed, '-', '')

var appServicePlanName = take('${baseDashed}-asp', 60)
var webAppName = take('${baseDashed}-app', 60)
var userAssignedIdentityName = take('${baseDashed}-id', 128)

var keyVaultNameRaw = take('${baseDashed}-kv', 24)
var keyVaultName = endsWith(keyVaultNameRaw, '-') ? '${substring(keyVaultNameRaw, 0, length(keyVaultNameRaw) - 1)}0' : keyVaultNameRaw

var cosmosAccountName = take('${baseCompact}cos', 44)
var cosmosSqlDatabaseName = 'DemoLibrary'
var logAnalyticsWorkspaceName = take('${baseDashed}-law', 63)
var appInsightsName = take('${baseDashed}-appi', 260)
var aiFoundryAccountName = take('${baseCompact}ai', 64)
var aiFoundryProjectName = take('${normalizedProject}-${normalizedEnvironment}-project', 64)
var aiFoundryDeploymentName = take(replace('${aiFoundryModelName}-${normalizedEnvironment}', '.', '-'), 64)

// Variables
var appServicePlanSkuName = 'B1'
var appServicePlanSkuCapacity = 1
var cosmosPublicNetworkAccess = 'Enabled'
var keyVaultPublicNetworkAccess = 'Enabled'
var logAnalyticsSkuName = 'PerGB2018'
var observabilityRetentionDays = 30
var entraLoginInstance = az.environment().authentication.loginEndpoint

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

resource aiFoundryAccount 'Microsoft.CognitiveServices/accounts@2025-09-01' = {
  name: aiFoundryAccountName
  location: location
  kind: 'AIServices'
  identity: {
    type: 'SystemAssigned, UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentity.id}': {}
    }
  }
  sku: {
    name: 'S0'
  }
  properties: {
    allowProjectManagement: true
    customSubDomainName: aiFoundryAccountName
    disableLocalAuth: true
    publicNetworkAccess: 'Enabled'
  }
  tags: tags
}

resource aiFoundryProject 'Microsoft.CognitiveServices/accounts/projects@2025-12-01' = {
  name: aiFoundryProjectName
  parent: aiFoundryAccount
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentity.id}': {}
    }
  }
  properties: {
    displayName: aiFoundryProjectName
    description: 'Azure AI Foundry project for ${projectName} (${environment}).'
  }
  tags: tags
}

resource aiFoundryModelDeployment 'Microsoft.CognitiveServices/accounts/deployments@2025-12-01' = {
  name: aiFoundryDeploymentName
  parent: aiFoundryAccount
  sku: {
    name: 'GlobalStandard'
    capacity: 1
  }
  properties: {
    model: {
      format: aiFoundryModelFormat
      name: aiFoundryModelName
    }
    versionUpgradeOption: 'OnceNewDefaultVersionAvailable'
  }
  tags: tags
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
          CosmosDatabase__DatabaseName: cosmosSqlDatabaseName
          CosmosDatabase__EndpointActivity: 'EndpointActivity'
          CosmosDatabase__Books: 'Books'
          CosmosDatabase__Loans: 'Loans'
          CosmosDatabase__AccountClosureRequests: 'AccountClosureRequests'
          CosmosDatabase__EnsureCreated: 'true'
          ActivityLogging__CaptureRequestBody: 'true'
          ActivityLogging__CaptureResponseBody: 'true'
          ActivityLogging__MaxBodyLength: '16384'
          APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.outputs.connectionString
          APPLICATIONINSIGHTS_AUTHENTICATION_STRING: 'Authorization=AAD;ClientId=${userAssignedIdentity.properties.clientId}'
          AZURE_CLIENT_ID: userAssignedIdentity.properties.clientId
          MANAGED_IDENTITY_CLIENT_ID: userAssignedIdentity.properties.clientId
          EntraAuthentication__Instance: entraLoginInstance
          EntraAuthentication__TenantId: entraTenantId
          EntraAuthentication__Audience: 'api://${entraAudienceClientId}'
          EntraAuthentication__RequiredApiScope: 'api.library.account'
          EntraAuthentication__RequiredMcpScope: 'mcp.library.account'
          EntraAuthentication__ApiResourceDocumentationUrl: 'https://docs.example.com/api/library-rest'
          EntraAuthentication__JwtResourceMetadataPath: '/.well-known/oauth-protected-resource/api'
          Mcp__ServerName: 'demo-library-mcp'
          Mcp__ServerVersion: '1.0.0'
          Mcp__StatelessTransport: 'true'
          Mcp__ResourceDocumentationUrl: 'https://docs.example.com/api/library-mcp'
          Mcp__ResourceMetadataPath: '/.well-known/oauth-protected-resource/mcp'
          Mcp__AuthorizationMode: 'ToolLevel'
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

@description('Azure AI Foundry account name when enabled.')
output aiFoundryAccountNameOutput string = aiFoundryAccount.name

@description('Azure AI Foundry account resource ID when enabled.')
output aiFoundryAccountResourceId string = aiFoundryAccount.id

@description('Azure AI Foundry project resource ID when enabled.')
output aiFoundryProjectResourceId string = aiFoundryProject.id

@description('Azure AI model deployment name when enabled.')
output aiFoundryModelDeploymentNameOutput string = aiFoundryModelDeployment.name
