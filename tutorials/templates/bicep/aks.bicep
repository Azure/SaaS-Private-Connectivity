param clusterName string
param nodeResourceGroup string
param aksIdentityResourceId string
param dnsPrefix string = toLower(clusterName)
param kubernetesVersion string = '1.18.14'
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

resource aks 'Microsoft.ContainerService/managedClusters@2020-12-01' = {
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
    kubernetesVersion: kubernetesVersion
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
        nodeTaints: [
          'CriticalAddonsOnly=true:NoSchedule'
        ]
      }
      {
        name: 'user'
        count: 3
        enableAutoScaling: true
        maxCount: 10
        minCount: 3
        vmSize: 'Standard_DS3_v2'
        osType: 'Linux'
        mode: 'User'
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
      // Note: Disabling the appgw addon until there is a solution to upgrade it.
      // Until then, AGIC will be installed via Helm as documented in the docs.
      // https://github.com/Azure/application-gateway-kubernetes-ingress/blob/master/docs/how-tos/helm-upgrade.md
      //
      // ingressApplicationGateway: {
      //   enabled: true
      //   config: {
      //     applicationGatewayId: applicationGatewayId
      //   }
      // }
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
      // networkPolicy: 'calico' // There is a known issue with Flux and the Azure network policy: https://github.com/fluxcd/flux2/issues/703
      podCidr: podCidr
      serviceCidr: serviceCidr
      dnsServiceIP: dnsServiceIP
      dockerBridgeCidr: dockerBridgeCidr
      loadBalancerSku: 'standard'
    }
    // apiServerAccessProfile: {
    //   enablePrivateCluster: true
    // }
  }
}

output aksClusterName string = aks.name
output aksNodeResourceGroup string = aks.properties.nodeResourceGroup
output aksKubeletIdentityObjectId string = aks.properties.identityProfile.kubeletidentity.objectId
// output aksAppgwIdentityObjectId string = aks.properties.addonProfiles.ingressApplicationGateway.identity.objectId
