#!/bin/bash
set -eu

parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

function usage() {
    echo
    echo "Arguments:"
    echo -e "\t-u\t Sets the business unit"
    echo -e "\t-a\t Sets the app name"
    echo -e "\t-e\t Sets the environment"
    echo -e "\t-r\t Sets the regions (Comma delimited values)"
    echo
    echo "Example:"
    echo -e "\tbash deploy.sh -u $(whoami) -a urlist -e test -r westus,eastus,centralus"
}

while getopts "u:a:e:r:hq" opt
do
    case $opt in
        u) businessUnit=$OPTARG;;
        a) appName=$OPTARG;;
        e) env=$OPTARG;;
        r) regions=(${OPTARG//,/ });;
        :) echo "Error: -${OPTARG} requires a value"; exit 2;;
        *) usage;exit 2;;
    esac
done

# Validation
if [ -z $businessUnit ]
then
    echo "Business Unit is required"
    usage
    exit 2
fi

if [ -z $appName ]
then
    echo "App name is required"
    usage
    exit 2
fi

if [ -z $env ]
then
    echo "Environment is required"
    usage
    exit 2
fi

if [ -z $regions ]
then
    echo "Regions is required"
    usage
    exit 2
fi

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
    value="[${value%?}]"
    printf $value
}

# set -e fails and exit here if just let counter=0 is specified. Workaround is to add || true to the expression
let counter=0 || true

frontendHostArray=()
backendHostArray=()
cosmosdbRegionArray=()

for region in ${regions[@]}
do
    frontendHostArray+=("frontend-$regionScope-$region.azurewebsites.net")
    backendHostArray+=("apim-$regionScope-$region.azure-api.net")
    cosmosdbRegionArray+=("$region")
done

frontendHosts=$(toArmArray ${frontendHostArray[*]})
backendHosts=$(toArmArray ${backendHostArray[*]})
cosmosdbRegions=$(toArmArray ${cosmosdbRegionArray[*]})
echo "Frontend Hosts: $frontendHosts"
echo "Backend Hosts: $backendHosts"

echo "Creating global resource Group: $resourceGroupName"
az group create \
--name $resourceGroupName \
--location ${regions[0]}

echo "Deploying global resources to $resourceGroupName"
az group deployment create \
--name "Urlist-global-$(timestamp)" \
--resource-group $resourceGroupName \
--template-file global.json \
--parameters frontDoorName=$frontDoorName frontendHosts=$frontendHosts backendHosts=$backendHosts \
  cosmosdbName=$cosmosdbName cosmosdbRegions=$cosmosdbRegions
