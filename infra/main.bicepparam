using './main.bicep'

param companyName = 'rkt'
param projectName = 'securemcp'
param environment = 'dev'

param tags = {
  owner: 'platform-team'
  workload: 'demo-library-api'
}
