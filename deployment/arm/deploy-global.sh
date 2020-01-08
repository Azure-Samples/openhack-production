#!/bin/bash
set -eu

parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

businessUnit=$1
appName=$2
env=$3

if [[ "$#" < 4 ]]; then
    echo "Illegal number of arguments. BusinessUnit, AppName, Environment and atleast one Region must be provided"
    exit 1
fi

# Regions are passed in as additional arguments
scope="$businessUnit-$appName-$env-gbl"
regionScope="$businessUnit-$appName-$env"
resourceGroupName="rg-$scope"
frontDoorName="fd-$scope"
cosmosdbName="db-$scope"

echo "Resource Group: $resourceGroupName"
echo "Business Unit: $businessUnit"
echo "App Name: $appName"
echo "Environment: $env"
echo "Front Door: $frontDoorName"
echo "Cosmos DB Name: $cosmosdbName"

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

# set -e exists here if just let counter=0 is specified. Workaround is to add || true to the expression
let counter=0 || true

frontendHostArray=()
backendHostArray=()
cosmosdbRegionArray=()

for region in $@
do
  let "counter=counter + 1"
  if [ $counter -gt 3 ]
  then
    frontendHostArray+=("frontend-$regionScope-$region.azurewebsites.net")
    backendHostArray+=("apim-$regionScope-$region.azure-api.net")
    cosmosdbRegionArray+=("$region")
  fi
done

frontendHosts=$(toArmArray ${frontendHostArray[*]})
backendHosts=$(toArmArray ${backendHostArray[*]})
cosmosdbRegions=$(toArmArray ${cosmosdbRegionArray[*]})

echo "Creating global resource Group: $resourceGroupName"
az group create \
  --name $resourceGroupName \
  --location ${cosmosdbRegionArray[0]}

echo "Deploying global resources to $resourceGroupName"
az group deployment create \
  --name "Urlist-global-$(timestamp)" \
  --resource-group $resourceGroupName \
  --template-file global.json \
  --parameters frontDoorName=$frontDoorName frontendHosts=$frontendHosts backendHosts=$backendHosts \
      cosmosdbName=$cosmosdbName cosmosdbRegions=$cosmosdbRegions
