# Azure Pipelines

The Urlist application needs three pipelines:

- [The infrastructure pipeline](deploy-infrastructure.yml) responsible for setting up the regional and global infrastructures for the application.
- [The backend pipeline](api-pipelines.yml) responsible for the Continuous Deployment (CD) of the Urlist API stack.
- [The frontend pipeline](frontend-pipeline.yml) responsible for the CD of the Urlist website.

The following describes how each pipeline works.

## Variable group set

There are a set of values needed for the pipelines to run, those values are stored and passed to the pipelines via a variables group called **prodoh-urlist-variable-group**.

- businessUnit - Your company's business unit / team name
- appName - The name of your app
- environment - The environment to deploy (dev, test, prod, etc)
- azureSubscription - the Azure subscription to use for hosting the application

## Infrastructure pipeline

This pipeline sets up the Urlist application needed infrastructure ([described here](../docs/infrastructure.md)) by utilizing [Azure Resource Manager (ARM) templates](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/overview) and
shell scripts, you can learn more about the used ARM templates and shell scripts [here](../deployment/README.md).

The pipeline has two stages:

- For each region you need to deploy to, it would call the associated regional ARM template and script to setup the needed resources in those given regions. By default, it deploys to westus2, eastus, and centralus. You would need to modify the YAML file
if you need to deploy to other regions.
- Once done deploying to regions, it would use the global ARM template and deployment scripts to setup the shared global resources.

## Backend pipeline

This pipeline is responsible for deploying the API Backend to the AppService instance created by the infrastructure pipeline. The pipeline has two stages:

- A build stage that compiles the .Net Core backend and stages the build output for deployment.
- A deployment stage that fetches the global CosmosDB instance URI and Key, then for each configured region, it deploys the output from the build stage onto the AppService instance for that region and passes the Cosmos DB URI and Key as app configs.

## Frontend pipeline

This pipeline is responsible for deploying the Urlist website to be hosted as a static-website on the Azure storage account created by the infrastructure pipeline. The pipeline has two stages:

- A build stage that:
  - Generates the frontend configurations ([described here](../frontend/README.md#feconfigs)) based on the infrastructure created.
  - Builds the frontend code base and stages the output for deployment.
- A deployment stage that, for each configured region, publishes the output from the build stage into the configured static website hosting for the respective region.
