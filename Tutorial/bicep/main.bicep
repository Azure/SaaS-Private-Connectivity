targetScope = 'resourceGroup'

@description('appName used to make this deployment unique')
param appName string = 'fsidemo'

var resgpguid_var = substring(replace(guid(resourceGroup().id), '-', ''), 0, 4)
var uniqueResourceName_var = '${resgpguid_var}'

module analytics './la.bicep' = {
  name: '${appName}-la'
  params:{
    appName: appName
  }
}

module network './nw.bicep' = {
  name: '${appName}-vnet'
  params:{
    appName: appName
  }
}

module mysql './mysql.bicep' = {
  name: '${appName}-mysql'
  params:{
    appName: appName
  }
}

module appsvc './appsvc.bicep' = {
  name: '${appName}-appsvc'
  params:{
    appName: appName
  }
}

module storage './storage.bicep' = {
  name: '${appName}-sa'
  params:{
    appName: appName
  }
}

output laName string = analytics.outputs.laName
output laId string = analytics.outputs.workspaceId
output vnetId string = network.outputs.vnetId
output appSubnetId string = network.outputs.appSubnetId
output insightsId string = analytics.outputs.insightsId
output insightsAppId string = analytics.outputs.insightsAppId
output insightsKey string = analytics.outputs.insightsKey

