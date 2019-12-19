#!/bin/bash
parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

prefix=$1
subscriptionId="d36d0808-a967-4f73-9fdc-32ea232fc81d"
location=$2
appName=$3
fullPrefix="$1-$2-$3"
regionalResourceGroupName="$fullPrefix-rg"
globalResourceGroupName="$prefix-gbl-$appName-rg"

echo "Resource Group: $regionalResourceGroupName"
echo "Prefix: $prefix"
echo "Location: $location"
echo "App Name: $appName"
echo "Prefix: $fullPrefix"

frontDoorName="$prefix-gbl-$appName-fd"
apimName="$fullPrefix-apim"
frontendAppName="$fullPrefix-frontend"
backendAppName="$fullPrefix-backend"

timestamp() {
  date +"%Y%m%dZ%H%M%S"
}

echo "Creating global resource Group: $globalResourceGroupName"
az group create \
  --name $globalResourceGroupName \
  --location centralus

echo "Deploying global resources to $globalResourceGroupName"
az group deployment create \
  --name "Urlist-global-$(timestamp)" \
  --resource-group $globalResourceGroupName \
  --template-file global.json \
  --parameters "frontDoorName=$frontDoorName apimName=$apimName frontendAppName=$frontendAppName backendAppName=$backendAppName"
