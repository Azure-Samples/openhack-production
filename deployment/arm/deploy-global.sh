#!/bin/bash
parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

businessUnit=$1
appName=$2
env=$3
frontendHosts=$4
backendHosts=$5
scope="$businessUnit-$appName-$env-gbl"
resourceGroupName="rg-$scope"
frontDoorName="fd-$scope"

echo "Resource Group: $resourceGroupName"
echo "Business Unit: $businessUnit"
echo "App Name: $appName"
echo "Environment: $env"
echo "Front Door: $frontDoorName"
echo "Frontend Hosts: $frontendHosts"
echo "Backend Hosts: $backendHosts"

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
  --template-file global.json \
  --parameters frontDoorName=$frontDoorName frontendHosts=$frontendHosts backendHosts=$backendHosts
