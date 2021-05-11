param appName string
 
var resgpguid = substring(replace(guid(resourceGroup().id), '-', ''), 0, 4)
var uniqueResourceName_var = '${appName}-${resgpguid}'

resource storage 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: '${appName}${resgpguid}sa'
  location: resourceGroup().location
  kind: 'Storage'
  sku: {
    name:'Standard_LRS'
    tier:'Standard'
  }
  properties: {
    
  }
}

