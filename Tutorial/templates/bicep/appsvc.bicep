param appName string

var resgpguid = substring(replace(guid(resourceGroup().id), '-', ''), 0, 4)
var uniqueResourceName_var = '${appName}-${resgpguid}'

resource appsvc 'Microsoft.Web/serverfarms@2020-12-01' = {
  name: '${uniqueResourceName_var}-appsvc'
  kind: 'linux'
  location: resourceGroup().location
  properties:{
    reserved:true
  }
  sku:{
     name: 'P1v2'
     tier: 'PremiumV2'
     size: 'P1v2'
     family: 'Pv2'
     capacity: 1
    }
}

output appsvcName string = appsvc.name
