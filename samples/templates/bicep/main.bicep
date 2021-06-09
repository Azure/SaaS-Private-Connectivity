targetScope = 'resourceGroup'

@description('appName used to make this deployment unique')
param appName string = 'fsidemo'
param aksNodeResourceGroup string = '${resourceGroup().name}-aks'

@secure()
param administratorLoginPassword string

module analytics './la.bicep' = {
  name: '${appName}-la'
  params: {
    appName: appName
  }
}

module network './nw.bicep' = {
  name: '${appName}-vnet'
  params: {
    appName: appName
  }
}

module identity 'identity.bicep' = {
  name: '${appName}-Identity'
  params: {
    appName: appName
  }
}

module mysql './mysql.bicep' = {
  name: '${appName}-mysql'
  params: {
    appName: appName
    administratorLoginPassword: administratorLoginPassword
  }
}

module function './function.bicep' = {
  name: '${appName}-function'
  params: {
    appName: appName
    appInsightsKey: analytics.outputs.insightsKey
    storageConnectionstring: storage.outputs.connectionstring
  }
}

module storage './storage.bicep' = {
  name: '${appName}-sa'
  params: {
    appName: appName
  }
}

module aks 'aks.bicep' = {
  name: '${appName}-Aks'
  params: {
    clusterName: '${appName}Aks'
    aksIdentityResourceId: identity.outputs.aksIdentityResourceId
    nodeResourceGroup: aksNodeResourceGroup
    aksSubnetId: network.outputs.appSubnetId
    logAnalyticsWorkspaceId: analytics.outputs.workspaceId
  }
}

module roleassignment_aks 'role-assignment-aks.bicep' = {
  name: '${appName}-RoleAssignment-Aks'
  params: {
    aksIdentityObjectId: identity.outputs.aksIdentityObjectId
  }
}

output laName string = analytics.outputs.laName
output laId string = analytics.outputs.workspaceId
output vnetId string = network.outputs.vnetId
output appSubnetId string = network.outputs.appSubnetId
output insightsId string = analytics.outputs.insightsId
output insightsName string = analytics.outputs.insightsName
output insightsAppId string = analytics.outputs.insightsAppId
output insightsKey string = analytics.outputs.insightsKey
output storageAccountName string = storage.outputs.storageAccountName
output functionName string = function.outputs.name
