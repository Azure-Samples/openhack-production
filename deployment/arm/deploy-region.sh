#!/bin/bash
parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

prefix=$1
subscriptionId="d36d0808-a967-4f73-9fdc-32ea232fc81d"
location=$2
appName=$3
fullPrefix="$1-$2-$3"
regionalResourceGroupName="$fullPrefix-rg"

echo "Resource Group: $regionalResourceGroupName"
echo "Prefix: $prefix"
echo "Location: $location"
echo "App Name: $appName"
echo "Prefix: $fullPrefix"

frontDoorName="$prefix-gbl-$appName-fd"
apimName="$fullPrefix-apim"
appServicePlanName="$fullPrefix-asp"
frontendAppName="$fullPrefix-frontend"
backendAppName="$fullPrefix-backend"
appInsightsName="$fullPrefix-ai"

timestamp() {
  date +"%Y%m%dZ%H%M%S"
}

echo "Creating Regional Resource Group: $regionalResourceGroupName"
az group create \
  --name $regionalResourceGroupName \
  --location $location

echo "Deploying regional resources to $regionalResourceGroupName"
az group deployment create \
  --name "Urlist-$location-$(timestamp)" \
  --resource-group $regionalResourceGroupName \
  --template-file region.json \
  --parameters subscriptionId=$subscriptionId location=$location apimName=$apimName appServicePlanName=$appServicePlanName frontendAppName=$frontendAppName backendAppName=$backendAppName appInsightsName=$appInsightsName
