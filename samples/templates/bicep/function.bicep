param appName string
param appInsightsKey string
param storageConnectionstring string
param mysqldb string
@secure()
param mysqlpassword string
param mysqluser string
param mysqlurl string

var resgpguid = substring(replace(guid(resourceGroup().id), '-', ''), 0, 4)
var uniqueResourceName_var = '${appName}-${resgpguid}'

resource appsvc 'Microsoft.Web/serverfarms@2020-12-01' = {
  name: '${uniqueResourceName_var}-appsvc'
  kind: 'linux'
  location: resourceGroup().location
  properties: {
    reserved: true
  }
  sku: {
    name: 'P1v2'
    tier: 'PremiumV2'
    size: 'P1v2'
    family: 'Pv2'
    capacity: 1
  }
}

resource func 'Microsoft.Web/sites@2018-11-01' = {
  name: '${uniqueResourceName_var}-func'
  kind: 'functionapp,linux'
  location: resourceGroup().location

  properties: {
    serverFarmId: appsvc.id
    siteConfig: {
      appSettings: [
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'AzureWebJobsStorage'
          value: storageConnectionstring
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsightsKey
        }
        {
          name: 'MySqlDatabase'
          value: mysqldb
        }
        {
          name: 'MySqlPassword'
          value: mysqlpassword
        }
        {
          name: 'MySqlServer'
          value: mysqlurl
        }
        {
          name: 'MySqlUserId'
          value: mysqluser
        }
      ]
    }
  }
}

output name string = func.name
