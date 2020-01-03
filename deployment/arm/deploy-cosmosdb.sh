#!/bin/bash
parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

businessUnit=$1
appName=$2
env=$3
primaryRegion=$4
secondaryRegion=$5
tertiaryRegion=$6


scope="$businessUnit-$appName-$env-gbl"
resourceGroupName="rg-$scope"

echo "Resource Group: $resourceGroupName"
echo "Business Unit: $businessUnit"
echo "App Name: $appName"
echo "Environment: $env"

timestamp() {
  date +"%Y%m%dZ%H%M%S"
}

echo "Creating global resource Group: $resourceGroupName"
az group create \
  --name $resourceGroupName \
  --location centralus

echo "Deploying global resources to $resourceGroupName"
az group deployment create \
  --name "Urlist-global-$(timestamp)" \
  --resource-group $resourceGroupName \
  --template-file cosmosDB.json \
  --parameters primaryRegion=$primaryRegion secondaryRegion=$secondaryRegion tertiaryRegion=$tertiaryRegion
