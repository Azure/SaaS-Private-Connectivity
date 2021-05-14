param appName string
 
var resgpguid = substring(replace(guid(resourceGroup().id), '-', ''), 0, 4)
var uniqueResourceName_var = '${appName}${resgpguid}'


resource aksIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: '${uniqueResourceName_var}aksIdentity'
  location: resourceGroup().location
}


output aksIdentityObjectId string = aksIdentity.properties.principalId
output aksIdentityTenantId string = aksIdentity.properties.tenantId
output aksIdentityResourceId string = aksIdentity.id
