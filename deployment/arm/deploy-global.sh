#!/bin/bash

##########################################################################################################################################################################################
#- Purpose: Script is used to deploy the global resources for the Urlist app
#- Parameters are:
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
        :) echo "Error: -${OPTARG} requires a value"; exit 1;;
        *) usage;exit 1;;
    esac
done

# Validation
if [[ $# -eq 0 || -z $businessUnit || -z $appName || -z $env || -z $regions ]]
then
    echo "Required parameters are missing"
    usage
    exit 1
fi

# include common script to populate shared variables
source common-script.sh

# set global resource group name
resourceGroupName="rg-$globalScope"

echo "Resource Group: $resourceGroupName"
echo "Business Unit: $businessUnit"
echo "App Name: $appName"
echo "Environment: $env"
echo "Front Door: $frontDoorName"
echo "Cosmos DB Name: $cosmosdbName"
echo "Storage Account Name: $storageActName"
echo 

# set -e fails and exit here if just let counter=0 is specified. Workaround is to add || true to the expression
let counter=0 || true

frontendHostArray=()
backendHostArray=()
cosmosdbRegionArray=()

for region in ${regions[@]}
do
    storageActName="$(generateStorageAccountName -u $businessUnit -a $appName -e $env -r $region)"
    
    # fetch the regional primary endpoint for the static website hosted in blob storage
    primaryEP=$(az storage account show --name $storageActName --query 'primaryEndpoints.web')
    # the frontendHost needs to be the domain name, using sed to extract that from the URL
    # also removing the additional quotes az returns with the URL
    primaryEP=$(echo $primaryEP | sed -e 's|^[^/]*//||' -e 's|/.*$||' -e 's/"//g')
    frontendHostArray+=("$primaryEP")

    backendHostArray+=("apim-$regionScope-$region.azure-api.net")
    cosmosdbRegionArray+=("$region")
done

frontendHosts=$(toArmArray ${frontendHostArray[*]})
backendHosts=$(toArmArray ${backendHostArray[*]})
cosmosdbRegions=$(toArmArray ${cosmosdbRegionArray[*]})
echo "Frontend Hosts: $frontendHosts"
echo "Backend Hosts: $backendHosts"
echo 

echo "Creating global resource Group: $resourceGroupName"
az group create \
--name $resourceGroupName \
--location ${regions[0]}

echo
echo "Deploying global resources to $resourceGroupName"
az group deployment create \
--name "Urlist-global-$(timestamp)" \
--resource-group $resourceGroupName \
--template-file global.json \
--parameters \
frontDoorName=$frontDoorName frontDoorEndpoint=$frontDoorEndpoint \
frontendHosts=$frontendHosts backendHosts=$backendHosts \
cosmosdbName=$cosmosdbName cosmosdbRegions=$cosmosdbRegions
