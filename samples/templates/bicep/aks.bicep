param clusterName string
param nodeResourceGroup string
param aksIdentityResourceId string
param dnsPrefix string = toLower(clusterName)
param logAnalyticsWorkspaceId string
param aksSubnetId string
@description('Specifies the CIDR notation IP range from which to assign pod IPs when kubenet is used.')
param podCidr string = '10.244.0.0/16'
@description('A CIDR notation IP range from which to assign service cluster IPs. It must not overlap with any Subnet IP ranges.')
param serviceCidr string = '10.2.0.0/16'
@description('Specifies the IP address assigned to the Kubernetes DNS service. It must be within the Kubernetes service address range specified in serviceCidr.')
param dnsServiceIP string = '10.2.0.10'
@description('Specifies the CIDR notation IP range assigned to the Docker bridge network. It must not overlap with any Subnet IP ranges or the Kubernetes service address range.')
param dockerBridgeCidr string = '172.17.0.1/16'

resource aks 'Microsoft.ContainerService/managedClusters@2020-09-01' = {
  name: clusterName
  location: resourceGroup().location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${aksIdentityResourceId}': {}
    }
  }
  properties: {
    nodeResourceGroup: nodeResourceGroup
    dnsPrefix: dnsPrefix
    agentPoolProfiles: [
      {
        name: 'system'
        count: 3
        enableAutoScaling: true
        maxCount: 10
        minCount: 3
        vmSize: 'Standard_DS3_v2'
        osType: 'Linux'
        mode: 'System'
        type: 'VirtualMachineScaleSets'
        availabilityZones: [
          '1'
          '2'
          '3'
        ]
        vnetSubnetID: aksSubnetId
      }
    ]
    addonProfiles: {
      omsagent: {
        enabled: true
        config: {
          logAnalyticsWorkspaceResourceID: logAnalyticsWorkspaceId
        }
      }
    }
    enableRBAC: true
    networkProfile: {
      networkPlugin: 'azure'
      podCidr: podCidr
      serviceCidr: serviceCidr
      dnsServiceIP: dnsServiceIP
      dockerBridgeCidr: dockerBridgeCidr
      loadBalancerSku: 'standard'
    }
  }
}

output aksClusterName string = aks.name
output aksNodeResourceGroup string = aks.properties.nodeResourceGroup
output aksKubeletIdentityObjectId string = aks.properties.identityProfile.kubeletidentity.objectId
