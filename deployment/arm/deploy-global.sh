#!/bin/bash
set -e
parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

businessUnit=$1
appName=$2
env=$3
primaryRegion=$4
secondaryRegion=$5
tertiaryRegion=$6

if [[ -z $businessUnit || -z $appName || -z $env || -z $primaryRegion || -z $secondaryRegion || -z $tertiaryRegion ]]; then
  echo 'One or more variables are undefined'
  exit 1
fi

# Regions are passed in as additional arguments
scope="$businessUnit-$appName-$env-glb"
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
echo "Primary Region: $primaryRegion"
echo "Secondary Region: $secondaryRegion"
echo "Tertiary Region: $tertiaryRegion"

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

let counter=0 || true

frontendHostArray=()
backendHostArray=()

for region in $@
do
  let "counter=counter + 1"
  if [ $counter -gt 3 ]
  then
    frontendHostArray+=("frontend-$regionScope-$region.azurewebsites.net")
    backendHostArray+=("apim-$regionScope-$region.azure-api.net")
  fi
done

frontendHosts=$(toArmArray ${frontendHostArray[*]})
backendHosts=$(toArmArray ${backendHostArray[*]})

echo "Creating global resource Group: $resourceGroupName"
az group create \
  --name $resourceGroupName \
  --location $primaryRegion

echo "Deploying global resources to $resourceGroupName"
az group deployment create \
  --name "Urlist-global-$(timestamp)" \
  --resource-group $resourceGroupName \
  --template-file global.json \
  --parameters frontDoorName=$frontDoorName frontendHosts=$frontendHosts backendHosts=$backendHosts \
      cosmosdbName=$cosmosdbName cosmosdbPrimaryRegion=$primaryRegion \
      cosmosdbSecondaryRegion=$secondaryRegion cosmosdbTertiaryRegion=$tertiaryRegion
