param appName string
param administratorLogin string = 'azureadmin'
@secure()
param administratorLoginPassword string
param version string = '5.7'

var resgpguid = substring(replace(guid(resourceGroup().id), '-', ''), 0, 4)
var uniqueResourceName_var = '${appName}-${resgpguid}'

resource mysql 'Microsoft.DBforMySQL/servers@2017-12-01' = {
  name: '${uniqueResourceName_var}-mysql'
  location: resourceGroup().location
  properties: {
    version: version
    createMode: 'Default'
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    sslEnforcement: 'Disabled'
  }
}

resource mysqlfwrule 'Microsoft.DBforMySQL/servers/firewallRules@2017-12-01' = {
  parent: mysql
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}
