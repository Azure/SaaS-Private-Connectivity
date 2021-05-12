param aksIdentityObjectId string

var networkContributorRole = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4d97b98b-1d4f-4787-a291-c67834d212e7')

resource aksIdentityNetworkContributor 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid('aksIdentityNetworkContributor', resourceGroup().id)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: networkContributorRole
    principalId: aksIdentityObjectId
    principalType: 'ServicePrincipal'
  }
}
