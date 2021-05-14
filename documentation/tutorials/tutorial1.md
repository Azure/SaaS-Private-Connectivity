# Tutorial: Deploy an Example Function App as a Notification Endpoint

This tutorial is part one of a three part tutorial series that will configure and deploy and example of the Private Connectivity pattern.

In later tutorials, an example Private Link Service will be created using an AKS cluster and internal Load Balancer and a Managed application will be deployed from the Service Catalog.

## Before you begin

This tutorial assumes a basic understanding of azure cli and Visual Studio Code and Azure Functions

To support deployment ensure the functions core tools are available [Core Tool](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=linux%2Ccsharp%2Cbash) and dotnet sdks are installed [dotnet](https://docs.microsoft.com/en-gb/dotnet/core/install/)

To complete this tutorial you will need access to an Azure subscription with the Azure cli configured to use that subscription and have the appropriate dotnet SDK installed.


## Get application code

The [sample function app][sample-application] used in this tutorial is a simple function app consisting of a http trigger to allow interaction with of a front-end web component and a back-end Redis instance. The web component is packaged into a custom container image. The Redis instance uses an unmodified image from Docker Hub.

Use git to clone the sample application to your development environment:

```
git clone https://github.com/Azure/SaaS-Private-Connectivity.git

```

Change into the cloned directory.

```
cd tutorials

```

In this tutorial, you learn how to:

* Create a resource group 
* Deploy the Azure components required to support your Function App
* Deploy your Function App
* Configure the Function App for use with Azure App Insights and Azure MySQL 
* Create a database table using MySQL Workbench for use with the example app


## Create a resource group

In Azure, you allocate related resources to a resource group. Create a resource group by using [az group create](/cli/azure/group#az_group_create). The following example creates a resource group named *demoResourceGroup* in the *northeurope* location (region). 

```
az group create --name rg-tutorial --location northeurope

```

## Deploy needed Azure Components

You'll now deploy the components needed to support the Notification Webhook.

- App Service Plan
- Azure MySql
- Storage Account
- Log Analytics
- Application Insights
- Virtual Network (not required directly but will be used in later tutorial)

The templates to deploy these component have been provided as an ARM template or Bicep templates



### Bicep deployment

This tutorial assumes you have bicep installed [bicep](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/bicep-install?tabs=azure-powershell)

```
cd /templates/bicep

az deployment group create -g rg-tutorial -f ./main.bicep

```

Once deployed there are some values that will be required in subsequent steps which can be found in the outputs from the template deployments for example:

```
    "outputs": {
      "appSvcResourceId": {
        "type": "String",
        "value": "/subscriptions/<subscriptionId>/resourceGroups/rg-tutorial/providers/Microsoft.Web/serverfarms/fsidemo-88dc-appsvc"
      },
      "insightsKey": {
        "type": "String",
        "value": "<key value>"
      },
      "insightsName": {
        "type": "String",
        "value": "fsidemo-88dc-insights"
      },
      "resourceGroup": {
        "type": "String",
        "value": "rg-tutorial"
      },
      "storageAccountName": {
        "type": "String",
        "value": "fsidemo88dcsa"
      }

```


## Deploy the Function App
The Function App will be deployed to the App Service Plan created in the last step.  The http trigger based function to listen for webhook notifications will then be deployed to this function app.

```
cd /Tutorial/ManagedAppWebHook

```
In order to deploy the function app use the following:

```
resourceGroup=<resouregroup from outputs>
storageAccount=<storageAccount Name from outputs>
plan=<appSvcResourceId from outputs>
insights=<insightsName from outputs>
functionApp=fsidemo

az functionapp create --name $functionApp -g $resoruceGroup -s $storageAccount --app-insights $insights --os-type Linux --runtime dotnet --plan $plan --functions-version 3

```
The function app will be created and can be viewed in the Azure portal 

![functionApp](../../images/function-publish13.png)


Deploy the function

```
func azure functionapp publish $functionApp


```

The package file will be created and deployed to your function app:

![functionApp](../../images/function-publish15.png)


## Check that the function is reachable

Now that the Function has been deployed it can be verified using the health url 

```
https://<azure website host>/api/health
```

Once the function has been deployed you can additionally connect to the Azuze MySql using your chosen [connection method](https://docs.microsoft.com/en-us/azure/mysql/how-to-connect-overview-single-server)

When you have connected you will be able to create the required database and table and insert a record.

```
-- Create a database
DROP DATABASE IF EXISTS tutorialdb;
CREATE DATABASE tutorialdb;
USE tutorialdb;

-- Create a table and insert rows
DROP TABLE IF EXISTS customer;
CREATE TABLE customer (id serial PRIMARY KEY, CompanyName VARCHAR(50), SharedKey VARCHAR(50));

```

The tutorial uses a SharedKey to validate the request for private link connection approval. To generate a SharedKey you will need to create an entry in the customer table

```
-- insert sample row
INSERT INTO customer ( CompanyName, SharedKey ) VALUES ('ExampleCustomer',uuid());

select * from customer;
```

The result will return a value for the ExampleCustomer and SharedKey.  This SharedKey can be used in the subsequent steps in [tutorial3](./tutorial3.md)



## Next steps

Deploy a sample application [Tutorial2](./tutorial-sampleapp.md)