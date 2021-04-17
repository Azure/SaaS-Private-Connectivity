# SaaS Private Connectivity Pattern


# Introduction 
This documentation provides detailed guidance on the architecture and deployment of the SaaS Private Connectivity Pattern that supports the following key features:

- Private Connectivity over Azure PrivateLink
- Automated Approval and deployment of PrivateLink connection from Azure MarketPlace
- Exposure of SaaS API's over the PrivateLink


### Table of Contents
- [Introduction](#introduction)
    - [Table of Contents](#table-of-contents)
- [Deployment Architecture](#deployment-architecture)
  - [Components](#components)
    - [Internal LoadBalancer](#internal-loadbalancer)
    - [Private Link Service](#private-link-service)
    - [Private Endpoint](#private-endpoint)
    - [Private DNS Zone](#private-dns-zone)
    - [Function App](#function-app)
    - 


# Deployment Architecture

## Components
The Private connectivity pattern leverages standard Azure components to enable Private connectivity between a consumer and a SaaS Provider where public outbound connectivity from a consumers tenant and subscription is not permitted or supported. This is typical of environments such as those used by Azure FSI customers.

The diagram below provides a high level view of the deployment architecture for this pattern:

![Deployment Architecture](./images/deployment_architecture.png)
