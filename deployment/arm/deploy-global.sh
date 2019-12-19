#!/bin/bash
parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

prefix=$1
appName=$2
frontendHosts=$3
backendHosts=$4
resourceGroupName="$prefix-gbl-$appName-rg"
frontDoorName="$prefix-gbl-$appName-fd"

echo "Resource Group: $resourceGroupName"
echo "Prefix: $prefix"
echo "App Name: $appName"
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
  --parameters "frontDoorName=$frontDoorName"
