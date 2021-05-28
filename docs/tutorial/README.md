# Private Connectivity Pattern - Tutorial

The tutorial is designed to provide and example deployment that demonstrated the private connectivity pattern, automated authorization of the Private Link connection based on a Pre-Shared Key using a notification webhook and identity to interact with the Managed Application deployment. In this example an Azure Function app has been used to support the webhook, the example of which can be found in the [repo](../../samples/ManagedAppWebHook/).

The tutorial architecture is shown below:

![tutorial-architecture](../../images/tutorial1.png)

The deployment includes the deployment of an Azure MySQL DB for storing a pre-shared key and Azure Kubernetes Service for hosting the example application which will be used to validate that the Private Link connection is working correctly.

## Next steps

Get started with [part 1 of the tutorial](part1.md) and deploy the notification webhook and Azure infrastructure required.
