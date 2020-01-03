#!/bin/bash
parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

businessUnit=$1
appName=$2
env=$3
location=$4
scope="$businessUnit-$appName-$env-$location"
globalScope="$businessUnit-$appName-$env-glb"
resourceGroupName="rg-$scope"


echo "Resource Group: $resourceGroupName"
echo "Region Scope: $scope"
echo "Business Unit: $businessUnit"
echo "App Name: $appName"
echo "Environment: $env"
echo "Location: $location"

apimName="apim-$scope"
appServicePlanName="asp-$scope"
frontendAppName="frontend-$scope"
backendAppName="backend-$scope"
appInsightsName="ai-$scope"

timestamp() {
  date +"%Y%m%dZ%H%M%S"
}

echo "Creating Regional Resource Group: $resourceGroupName"
az group create \
  --name $resourceGroupName \
  --location $location

echo "Deploying regional resources to $resourceGroupName"
az group deployment create \
  --name "Urlist-$location-$(timestamp)" \
  --resource-group $resourceGroupName \
  --template-file region.json \
  --parameters location=$location apimName=$apimName appServicePlanName=$appServicePlanName frontendAppName=$frontendAppName backendAppName=$backendAppName appInsightsName=$appInsightsName

cosmosEndPoint=`az group deployment show -g  rg-$globalScope -n Urlist-global-cosmosdb --query properties.outputs.documentEndpoint.value -o tsv`
cosmosKey=`az group deployment show -g  rg-$globalScope -n Urlist-global-cosmosdb --query properties.outputs.accountKey.value -o tsv`

echo "Configuring App service settings for $backendAppName"
az webapp config appsettings set \
-g $resourceGroupName \
-n $backendAppName \
--settings CosmosSettings:ServiceEndpoint=$cosmosEndPoint CosmosSettings:AuthKey=$cosmosKey  
