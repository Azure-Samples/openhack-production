#!/bin/bash
parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

businessUnit=$1
appName=$2
env=$3
# Regions are passed in as additional arguments
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

# Convert array to ARM array format
toArmArray() {
  array=("$@")
  value=$(printf "\"%s\"", "${array[@]}")
  value="(${value%?})"
  printf $value
}

regions=()
let counter=0

for region in $@
do
  let counter++
  if [ $counter -gt 3 ]
  then
    echo $region
    regions+=($region)
  fi
done

frontendHostArray=()
backendHostArray=()

# Deploy scale unit per region
for region in ${regions[*]}
do
  frontendHostArray+=("frontend-$scope-$region.azurewebsites.net")
  backendHostArray+=("apim-$scope-$region.azure-api.net")
  bash deploy-region.sh $businessUnit $appName $env $region
done

frontendHosts=$(toArmArray ${frontendHostArray[*]})
backendHosts=$(toArmArray ${backendHostArray[*]})

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
