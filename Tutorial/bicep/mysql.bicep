param appName string
param serverName string = 'fsidemo'
param skuName string = 'GP_Gen5_4'
param skuTier string = 'GeneralPurpose'
param administratorLogin string = 'azureadmin'
@secure()
param administratorLoginPassword string = '2021@Wrekin@2021'
param version string = '5.7'
 
var resgpguid = substring(replace(guid(resourceGroup().id), '-', ''), 0, 4)
var uniqueResourceName_var = '${appName}-${resgpguid}'

resource mysql 'Microsoft.DBforMySQL/servers@2017-12-01' = {
  name: '${uniqueResourceName_var}-mysql'
  location: resourceGroup().location
  properties: {
    version: version
    createMode:'Default'
    administratorLogin:administratorLogin
    administratorLoginPassword:administratorLoginPassword
  }
}
