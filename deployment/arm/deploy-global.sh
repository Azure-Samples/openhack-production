#!/bin/bash
set -eu
parent_path=$(
    cd "$(dirname "${BASH_SOURCE[0]}")"
    pwd -P
)
cd "$parent_path"

##########################################################################################################################################################################################
#- Purpose: Script is used to deploy the global resources for the Urlist app
#- Parameters are:
#- [-s] subscription - The subscription where the resources will reside.
#- [-u] businessUnit - The business unit used for resource naming convention.
#- [-a] appName - The application name used for resource naming convention.
#- [-e] env - The environment to deploy (ex: dev | test | qa | prod).
#- [-r] regions - A comma delimted list of regions to deploy to (ex: westus,eastus,centralus).
###########################################################################################################################################################################################

#######################################################
#- function used to print out script usage
#######################################################
function usage() {
    echo
    echo "Arguments:"
    echo -e "\t-s\t Sets the subscription"
    echo -e "\t-u\t Sets the business unit"
    echo -e "\t-a\t Sets the app name"
    echo -e "\t-e\t Sets the environment"
    echo -e "\t-r\t Sets the regions (Comma delimited values)"
    echo
    echo "Example:"
    echo -e "\tbash deploy.sh -s c77dad45-b62f-467d-bad4-8e00a807c0a2  -u $(whoami) -a urlist -e test -r westus,eastus,centralus"
}

while getopts "s:u:a:e:r:hq" opt; do
    case $opt in
    s) subscription=$OPTARG ;;
    u) businessUnit=$OPTARG ;;
    a) appName=$OPTARG ;;
    e) env=$OPTARG ;;
    r) regions=(${OPTARG//,/ }) ;;
    :)
        echo "Error: -${OPTARG} requires a value"
        exit 1
        ;;
    *)
        usage
        exit 1
        ;;
    esac
done

# Validation
if [[ $# -eq 0 || -z $businessUnit || -z $appName || -z $env || -z $regions ]]; then
    echo "Required parameters are missing"
    usage
    exit 1
fi

# include common script to populate shared variables
source utils.sh
source common-script.sh

# set global resource group name
resourceGroupName=$(createResourceName -p rg -u $businessUnit -a $appName -e $env -r gbl)

echo "Resource Group: $resourceGroupName"
echo "Business Unit: $businessUnit"
echo "App Name: $appName"
echo "Environment: $env"
echo "Front Door: $frontDoorName"
echo "Cosmos DB Name: $cosmosdbName"
echo "Application Insights Name: $appInsightsName"
echo

# set -e fails and exit here if just let counter=0 is specified. Workaround is to add || true to the expression
let counter=0 || true

frontendHostArray=()
backendHostArray=()
cosmosdbRegionArray=()
resourceGroupRegionArray=()
apimNameArray=()

for region in ${regions[@]}; do
    # include common script to populate shared variables per region
    source common-script.sh

    storageActName="$(generateStorageAccountName -u $businessUnit -a $appName -e $env -r $region)"

    # fetch the regional primary endpoint for the static website hosted in blob storage
    primaryEndpoint=$(az storage account show --subscription $subscription --name $storageActName --query 'primaryEndpoints.web')
    # the frontendHost needs to be the domain name, using sed to extract that from the URL
    # also removing the additional quotes az returns with the URL
    primaryEndpoint=$(echo $primaryEndpoint | sed -e 's|^[^/]*//||' -e 's|/.*$||' -e 's/"//g')
    frontendHostArray+=("$primaryEndpoint")

    backendHostArray+=("apim-$regionScope.azure-api.net")
    apimNameArray+=("apim-$regionScope")

    cosmosdbRegionArray+=("$region")
    resourceGroupRegionArray+=("rg-$regionScope")
done

frontendHosts=$(toArmArray ${frontendHostArray[*]})
backendHosts=$(toArmArray ${backendHostArray[*]})
# replicate cosmosdb data to one region only
cosmosdbRegions=$(toArmArray ${cosmosdbRegionArray[0]})
resourceGroupRegions=$(toArmArray ${resourceGroupRegionArray[*]})
apimNames=$(toArmArray ${apimNameArray[*]})
echo "Frontend Hosts: $frontendHosts"
echo "Backend Hosts: $backendHosts"
echo "Regional Resource Groups: $resourceGroupRegions"
echo "Regional APIM Names: $apimNames"
echo 

echo "Creating global resource Group: $resourceGroupName"
az group create \
    --subscription $subscription \
    --name $resourceGroupName \
    --location ${regions[0]} \
    --output jsonc

echo
echo "Deploying global resources to $resourceGroupName"
az group deployment create \
    --name "Urlist-global-$(timestamp)" \
    --resource-group $resourceGroupName \
    --subscription $subscription \
    --template-file global.json \
    --output table \
    --parameters \
    appInsightsName=$appInsightsName apimNames=$apimNames \
    frontDoorName=$frontDoorName frontDoorEndpoint=$frontDoorEndpoint \
    frontendHosts=$frontendHosts backendHosts=$backendHosts \
    cosmosdbName=$cosmosdbName cosmosdbRegions=$cosmosdbRegions \
    resourceGroupRegions=$resourceGroupRegions
