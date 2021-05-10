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

To support the automated approval of the PrivateLink connection from a MarketPlace deployment additionally:
- Azure Function App

The other components depicated in the deployment architecture are used to support the SaaS application deployment which provides the APIs that are being made available over the PrivateLink Service

### Private Endpoint
[Private Endpoint](https://docs.microsoft.com/en-us/azure/private-link/private-endpoint-overview)

### Private DNS Zone
[Private DNS Zone](https://docs.microsoft.com/en-us/azure/dns/private-dns-overview)

### PrivateLink Service
[PrivateLink Service](https://docs.microsoft.com/en-us/azure/private-link/private-link-service-overview)

### Internal Loadbalancer
[Internal Loadbalancer](https://docs.microsoft.com/en-us/azure/load-balancer/load-balancer-overview)

### Azure Function App
[Function App](https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview)