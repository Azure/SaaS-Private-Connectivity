# SaaS Private Connectivity Pattern


# Introduction 
This documentation provides detailed guidance on the architecture and deployment of the SaaS Private Connectivity Pattern that supports the following key features:

- Private Connectivity over Azure PrivateLink
- Automated Approval and deployment of PrivateLink connection from Azure MarketPlace
- Exposure of SaaS API's over the PrivateLink

The solution is intended to be deployed from either a Service Catalog or Azure MarketPlace deployment.  Service Catalog setup and deployment limits the deployment scope to the subscription in which the Service Catalog definition is deployed, however use of the Azure MarketPlace allows the solution to be made available as a Managed Application to any consuming customer/subscription given the agreed authentication mechanism is validated.


- [SaaS Private Connectivity Pattern](#saas-private-connectivity-pattern)
- [Introduction](#introduction)
- [Deployment Architecture](#deployment-architecture)
  - [Components](#components)
    - [Private Endpoint](#private-endpoint)
    - [Private DNS Zone](#private-dns-zone)
    - [PrivateLink Service](#privatelink-service)
    - [Internal Loadbalancer](#internal-loadbalancer)
    - [Azure Function App](#azure-function-app)
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
- Webhook Notification Endpoint 

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