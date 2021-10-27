# Private Connectivity - Service Catalog setup

Outlined below are detailed of how the Azure Service Catalog can be used to deploy the private connectivity pattern. Service catalog based deployments are only available to be deployed by internal users as the service catalog is an internal catalog of approved solutions for users in an organization.

![service catalog](../../images/manage_app_options.png)

## Setup

Details of how to create and publish the service catalog Managed Application definition can be found [here](https://docs.microsoft.com/en-gb/azure/azure-resource-manager/managed-applications/publish-service-catalog-app).

In order to create a service catalog definition, the following elements will be required:

- _customUIDefinition.json_ (defines the users portal experience)
- _mainTemplate.json_ (ARM template definition of the resources to deploy)

In addition when the service catalog definition is created authorization details (users or AD group) and details of the notification webhook (if used) will be required.

### customUIDefinition.json

The CustomUIDefinition will contain details of the fields and information that needs to be captured from the end user at ordering time. For the private connectivity pattern, as a minimum this would contain the resource group to which the Managed Application will be deployed and details of the virtual network configuration required, either existing or new VNet.

### mainTemplate.json

The _mainTemplate.json_ for the private connectivity pattern contains the following infrastructure elements to deploy:

- Virtual Network (if required)
- Private DNS zone
- Virtual Network link
- Private Endpoint
- Deployment (to add a DNS record)
