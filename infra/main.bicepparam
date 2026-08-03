using './main.bicep'

param companyName = 'rkt'
param projectName = 'securemcp'
param environment = 'dev'

param tags = {
  owner: 'platform-team'
  workload: 'demo-library-api'
}

param entraTenantId = '<your-tenant-id>'
param entraAudienceClientId = '<your-api-app-client-id>'

param aiFoundryModelName = 'gpt-4.1-mini'
param aiFoundryModelFormat = 'OpenAI'
