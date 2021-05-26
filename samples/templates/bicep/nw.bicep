param appName string
var resgpguid = substring(replace(guid(resourceGroup().id), '-', ''), 0, 4)
var uniqueResourceName_var = '${appName}-${resgpguid}'

param vnetAddressPrefix string = '10.0.0.0/24'
var vnetName = '${uniqueResourceName_var}-vnet'

var subnets = [
  {
    name: 'app-subnet'
    address: '10.0.0.0/24'
  }
]

resource vnet 'Microsoft.Network/virtualNetworks@2020-06-01' = {
  name: vnetName
  location: resourceGroup().location
  properties: {
    addressSpace: {
      addressPrefixes: [
        vnetAddressPrefix
      ]
    }
    subnets: [for subnet in subnets: {
      name: subnet.name
      properties: {
        addressPrefix: subnet.address
        privateLinkServiceNetworkPolicies: 'Disabled'
      }
    }]

    enableDdosProtection: false
    enableVmProtection: false
  }
}

output vnetId string = vnet.id
output appSubnetName string = subnets[0].name
output appSubnetId string = vnet.properties.subnets[0].id
