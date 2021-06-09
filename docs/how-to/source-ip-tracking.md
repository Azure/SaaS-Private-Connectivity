# Track client source IP address over Private Link connection

## Introduction

When using Private Link service, the source IP address of the packets coming from private endpoint is network address translated (NAT) on the service provider side using the NAT IP allocated from provider's virtual network. Hence the applications receive the allocated NAT IP address instead of actual source IP address of the service consumers. In many cases the service providers required the actual SourceIp Address of the clients for audit purposes.

## Before you begin

This guide assumes a basic understanding of azure cli and Nginx Ingress Controller configuration.

To complete this guide you will need access to an Azure subscription with the Azure cli configured and the ability to update the Nginx ingress controller. The guide does not cover how to deploy the Nginx ingress controller, details can be found [here](https://docs.microsoft.com/en-us/azure/aks/ingress-basic).

## Steps

### Enable proxy-protocol setting on the Private Link service

Using the Azure CLI:

```
az network private-link-service update -g resource-group-name -n private-link-service --enable-proxy-protocol true
```

Note: This can also be achieved by setting enable-proxy-protocal attribute in an ARM template or Terraform template

### Enable proxy-protocol on the nginx ingress controller

Update the settings applied to with the Helm chart to include the required configuration in a file internal-ingress.yaml

```
controller:
  config:
    use-proxy-protocol: "true"
    real-ip-header: "proxy_protocol"
  service:
    enableHttp: "false"
  publishService:
    enabled: true
  service:
    loadBalancerIP: 10.240.0.42
    annotations:
      service.beta.kubernetes.io/azure-load-balancer-internal: "true"
```

Now deploy the nginx-ingress chart with Helm or update an existing deployment. To use the manifest file created in the previous step, add the -f internal-ingress.yaml parameter.

### New deployment

```
# Create a namespace for your ingress resources
kubectl create namespace ingress-basic

# Add the ingress-nginx repository
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx

# Use Helm to deploy an NGINX ingress controller
helm install nginx-ingress ingress-nginx/ingress-nginx \
    --namespace ingress-basic \
    -f internal-ingress.yaml \
    --set controller.replicaCount=2 \
    --set controller.nodeSelector."beta\.kubernetes\.io/os"=linux \
    --set defaultBackend.nodeSelector."beta\.kubernetes\.io/os"=linux \
    --set controller.admissionWebhooks.patch.nodeSelector."beta\.kubernetes\.io/os"=linux
```

Once the helm chart has been deployed successfully the nginx controller logs will now track the sourceIp address of the client invoking the Private Link Service.

For example, check nginx controller logs. Requests should now be logged as originating from a client:

```
10.3.0.5 - - [23/Apr/2021:12:59:38 +0000] "GET /WeatherForecast HTTP/2.0" 200 508 "-" "curl/7.58.0" 70 0.001 [default-helloservice-80] [] 10.244.0.37:80 520 0.000 200 f4a0e8af30821cda341c04cdc45ed9a8
```

where "10.3.0.5" is the private IP of the client used to connect via the Private Endpoint connection.
