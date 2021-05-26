# Private Connectivity Market Place

Outlined below are detailed of how the Azure Commercial MarketPlace can be used to support the private connectivity pattern.

## Microsoft Partner Center
Partner center provides access to the Azure Commercial MarketPlace where offers can be created and published.  Details of how to access Partner Center, sign up and create an account are detailed in the Partner Center documentation [Partner Center](https://docs.microsoft.com/en-us/partner-center/).

Once an offer has been created the key elements for the Private Connectivity Pattern are within the Plan Setup

## Plan Listing
Contains overview of the individual plan

## Pricing & Availability
Key section here for validation puposes is whether the Plan Visibility is Public or Private.  With Private visibility Azure subscription IDs can be added to limit the visibility of the Marketplace entry to target subscriptions only

## Technical Configuration
The technical configuration provides the package details. 


|Field |Comment  |
|---------|---------|
| Package file | zip file of the managed application customUIdefinition.json and the mainTemplate.json |
| Deployment Mode | Ensure "Incremental" is selected |
| Notification Endpoint URL | Enter the webhook notification endpoint without /resource as this will be added by the Marketplace notification service eg (https://functionapp.azurewebsites.net/api) |
| Customize Allowed Customer actions | select (see below for details) |
| Azure Active Directory tenant ID | Enter the Publisher tenant id |
| Authorizations | Enter the principal Id of the function App identity and any User/Group required to access the managed app |


### Allowed Control actions:
To enable Private connectivity add the following actions that will allow vnet peering 
Microsoft.Network/virtualNetworks/virtualNetworkPeerings/write
Microsoft.Network/virtualNetworks/virtualNetworkPeerings/delete
Microsoft.Network/virtualNetworks/peer/action
Microsoft.Network/privateDnsZones/virtualNetworkLinks/write
Microsoft.Network/privateDnsZones/virtualNetworkLinks/delete
Microsoft.Network/virtualNetworks/join/action
