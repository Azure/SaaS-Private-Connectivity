param appName string
 
var resgpguid = substring(replace(guid(resourceGroup().id), '-', ''), 0, 4)
var uniqueResourceName_var = '${appName}-${resgpguid}'

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2020-10-01' = {
  name: '${uniqueResourceName_var}-la'
  location: resourceGroup().location
  properties:{
    sku:{
      name: 'PerGB2018'
    }
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: '${uniqueResourceName_var}-insights'
  dependsOn: [
    logAnalytics
  ]
  location: resourceGroup().location
  kind: 'web'
  properties:{
    WorkspaceResourceId: logAnalytics.id
    Application_Type: 'web'
  }
}

output laName string = logAnalytics.name
output workspaceId string = logAnalytics.id
output insightsId string = appInsights.id
output insightsAppId string = appInsights.properties.AppId
output insightsKey string = appInsights.properties.InstrumentationKey
