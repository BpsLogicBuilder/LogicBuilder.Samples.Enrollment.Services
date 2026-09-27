@description('The region to deploy all resources.')
param location string = resourceGroup().location

@description('Number of CPU cores the container can use. Can be with a maximum of two decimals.')
@allowed([
  '0.25'
  '0.5'
  '0.75'
  '1'
  '1.25'
  '1.5'
  '1.75'
  '2'
])
param cpuCore string = '0.5'

@description('Amount of memory (in gibibytes, GiB) allocated to the container up to 4GiB. Can be with a maximum of two decimals. Ratio with CPU cores must be equal to 2.')
@allowed([
  '0.5'
  '1'
  '1.5'
  '2'
  '3'
  '3.5'
  '4'
])
param memorySize string = '1'

@description('Minimum number of replicas that will be deployed')
@minValue(0)
@maxValue(25)
param minReplicas int = 1

@description('Maximum number of replicas that will be deployed')
@minValue(0)
@maxValue(25)
param maxReplicas int = 3

@description('The naming prefix for all resources.')
param prefix string = 'enroll'

@description('Specifies the container port.')
param targetPort int = 8080

@description('Database connection string.')
@secure()
param dbConnectionString string

@description('The naming prefix for all resources.')
param imageTag string = 'v1.0.0'

@description('Expected cerificate thumbprint')
param expectedCerificateThumbprint string

var imageprefix string = 'enrollment'
var uniqueSubString = uniqueString(resourceGroup().id)
var acrName = '${prefix}acr${uniqueSubString}'
var appInsightsName = '${prefix}-insights-${uniqueSubString}'
var appConfigurationName = '${prefix}-config-${uniqueSubString}'
var containerAppEnvName = '${prefix}-cae-${uniqueSubString}'

resource containerAppEnv 'Microsoft.App/managedEnvironments@2022-06-01-preview' existing = {
  name: containerAppEnvName
}

resource acr 'Microsoft.ContainerRegistry/registries@2025-11-01' existing = {
  name: acrName
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightsName
}

resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2024-05-01' existing = {
  name: appConfigurationName
}

resource bslService 'Microsoft.App/containerApps@2026-01-01' = {
  name: '${prefix}-bsl-${uniqueSubString}'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      ingress: {
        external: false
        targetPort: targetPort
        transport: 'auto'
        clientCertificateMode:'require'
      }
      registries: [
        {
          server: acr.properties.loginServer
          identity: 'system'
        }
      ]
      secrets: [
        {
          name: 'db-connection-string'
          value: dbConnectionString
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'bsl'
          image: '${acr.properties.loginServer}/${imageprefix}bsl:${imageTag}'
          resources: {
            cpu: json(cpuCore)
            memory: '${memorySize}Gi'
          }
          env: [
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: appInsights.properties.ConnectionString
            }
            {
              name: 'APPLICATION_CONFIGURATION_ENDPOINT'
              value: appConfiguration.properties.endpoint
            }
            {
              name: 'ConnectionStrings__DefaultConnection'
              secretRef: 'db-connection-string'
            }
            {
              name: 'ExpectedCerificateThumbprint'
              value: expectedCerificateThumbprint
            }
          ]
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
      }
    }
  }
}
