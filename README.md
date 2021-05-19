# SaaS Private Connectivity Pattern


# Introduction 
This documentation provides guidance on the architecture and deployment of the SaaS Private Connectivity Pattern that supports the following key features:

- Private Connectivity over Azure PrivateLink to expose Service hosted API's
- Deployment and Automated approval of a PrivateLink connection with a Managed Application deployment

The solution is intended to be deployed from either a Service Catalog or Azure MarketPlace deployment.  The Service Catalog setup and deployment limits the deployment scope to the subscription in which the Service Catalog definition is deployed, however use of the Azure MarketPlace allows the solution to be made available as a Managed Application to any consuming customer/subscription given the agreed authentication mechanism is validated.  The solution described can be used to enable private connectivity from a customer subscription/tenant to a Service Providers solution.  To allow connectivity from a customers networks either vnet peering would be required (recommended) or the use of an existing vnet into which the Managed Application is deployed.  In this scenario additional permissions would need to be granted by the customer to allow deployment to the existing vnet scope.

As well as providing an overview of the connectivity pattern there are also three tutorials which provide examples of how to deploy the pattern with a simple application context example.


- [SaaS Private Connectivity Pattern](#saas-private-connectivity-pattern)
- [Introduction](#introduction)
- [Deployment Architecture](#deployment-architecture)
  - [Components](#components)
    - [Private Endpoint](#private-endpoint)
    - [Private DNS Zone](#private-dns-zone)
    - [PrivateLink Service](#privatelink-service)
    - [Internal Loadbalancer](#internal-loadbalancer)
    - [Azure Function App](#azure-function-app)
- [Managed Application Deployment with Automation of Private Link Connectivity](#Managed-Application-Deployment-with-Automation-of-Private-Link-Connectivity)
- [Tutorial](./documentation/tutorial.md)
  - [Function App Webhook](./documentation/tutorials/tutorial1.md)
  - [Example App with Private Link Service](./documentation/tutorials/tutorial2.md)
  - [Managed Application](./documentation/tutorials/tutorial3.md)


# Deployment Architecture

## Components
The Private connectivity pattern leverages standard Azure components to enable Private connectivity between a consumer and a SaaS Provider where public outbound connectivity from a consumers tenant and subscription is not permitted or supported. This is typical of environments such as those used by Azure FSI customers.

The diagram below provides a high level view of the deployment architecture for this pattern:

![Deployment Architecture](./images/deployment_architecture.png)


The key Azure components used to support this pattern are:
- Private Endpoint
- Private DNS Zone
- PrivateLink Service
- Internal Loadbalancer

To support the automated approval of the PrivateLink connection from a MarketPlace ( or Service Catalog ) deployment additionally:
- Webhook Notification Endpoint (using Azure Function App)

The other components depicated in the deployment architecture are used to support the SaaS application deployment which provides the APIs that are being made available over the PrivateLink Service

### Private Endpoint
Private Endpoint is a network interface that connects you privately and securely to a service powered by Azure Private Link. Private Endpoint uses a private IP address from your VNet to provide the connection to the remote service.  In the case of the Private Connectivity pattern this is a Private Link Service exposing a set of service APIs
[Private Endpoint](https://docs.microsoft.com/en-us/azure/private-link/private-endpoint-overview)

### Private DNS Zone
Private DNS Zone is linked to the vnet and allows resolution of the Private Link service names.
[Private DNS Zone](https://docs.microsoft.com/en-us/azure/dns/private-dns-overview)

### PrivateLink Service
Private Link service is the reference to the Provider's service.  The service is running behind an internal loadbalance and is enabled for Private Link access.
[PrivateLink Service](https://docs.microsoft.com/en-us/azure/private-link/private-link-service-overview)

### Internal Loadbalancer
[Internal Loadbalancer](https://docs.microsoft.com/en-us/azure/load-balancer/load-balancer-overview)

### Notification Webhook
In the examples shown in the tutorials an Azure Funtion has been used for the Notification Webhook
[Function App](https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview)
This could be deployed using other capabilities providing the notification endpoint is exposed to allow the MarketPlace (Service Catalog) notifications to be sent to this listening endpoint.


# Managed Application Deployment with Automation of Private Link Connectivity 

The Managed Application Deployment can be done from either a [Service Catalog](./servicecatalog.md) or an [Azure Market Place](./marketplace.md) entry.  By using a [Managed Application](https://docs.microsoft.com/en-us/azure/azure-resource-manager/managed-applications/overview) the deployment into the customer subscription is then managed by the Publisher.  The deployment of all the components required to enable private connectivity are contained within the Managed Application with the exception of the Webhook to handle notifications and any additional vnet peering that is required by the customer to enable access to the Managed Application vnet.

The diagram below illustrates a typical flow when provisioning a Managed Application and automating the approval of the Private Link Connection

![flow](./images/flow.png)

## Elements of Approval Flow

1. Customer will locate the service catalog entry or Azure Marketplace offer and deploy/subscribe to it
2. This action will result in the creation of a Managed Application in the resource group of the customers choosing.
3. Additionally a managed resource group will be created containing the resources defined in the managed application template (mainTemplate.json)
4. Notification sent my the deployment to the webhook will be handled by the function app to authorise and approve the Private Link connection. In order to complete this action the identity used by the Azure Function app will be used to access the Managed Application deployment details and must have permissions to allow this. These are granted within the Marketplace setup or the Service Catalog application definition.

In the case of the Private Connectivity pattern it is recommended that an additional authorization is added to Notification and approval process based on the requirements of the Service Provider.  In the tutorials provided this is demonstrated using a pre-shared key stored in the providers database which is provided at ordering time by the consumer and verified by the function app before the connection is approved.

At the end of the deployment process identities ( Users, Groups or Apps/Service Principals ) with have access to the managed application deployment based on the authorization granted.  The Customer admin will have full control of the managed application however, they will only have read access to the managed resource group unless specific additional authorizations have been granted.  In the case of the Private Connectivity pattern these additional permissions are:
- Ability to peer the managed app vnet
- Ability to link the private DNS zone to a vnet


## Webhook endpoint
The commercial marketplace or Service Catalog calls this endpoint to notify the solution for the events happening on the marketplace/Service catalog side. Those events can be the acceptance of a deployment request, success of the deployment or cancellation of the managed application subscription. A publisher provides the URL for this webhook endpoint when registering the offer.  In the case of an Azure Function App details of the requests received can be easily logged using Application Insights allowing visibility of the requests being sent.  Using a notification webhook also supports notication retries as the Managed Application Notification service expects a 200 OK response from the webhook endpoint to the notification. The notification service will retry if the webhook endpoint returns an HTTP error code greater than or equal to 500, if it returns an error code of 429, or if the endpoint is temporarily unreachable. If the webhook endpoint doesn't become available within 10 hours, the notification message will be dropped and the retries will stop.

## Next steps

Complete the [Tutorial](./tutorials/tutorial.md)