#!/bin/bash
set -eu
parent_path=$(
    cd "$(dirname "${BASH_SOURCE[0]}")"
    pwd -P
)
cd "$parent_path"

##########################################################################################################################################################################################
#- Purpose: Script is used to deploy the regional resources for the Urlist app
#- Parameters are:
#- [-u] businessUnit - The business unit used for resource naming convention.
#- [-a] appName - The application name used for resource naming convention.
#- [-e] env - The environment to deploy (ex: dev | test | qa | prod).
#- [-r] region - The Azure region to deploy to (ex: westus | eastus | centralus | etc).
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
    echo -e "\t-r\t Sets the region"
    echo
    echo "Example:"
    echo -e "\tbash deploy.sh -u $(whoami) -a urlist -e test -r westus"
}

while getopts "u:a:e:r:hq" opt; do
    case $opt in
    u) businessUnit=$OPTARG ;;
    a) appName=$OPTARG ;;
    e) env=$OPTARG ;;
    r) region=$OPTARG ;;
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
if [[ $# -eq 0 || -z $businessUnit || -z $appName || -z $env || -z $region ]]; then
    echo "Required parameters are missing"
    usage
    exit 1
fi

echo
echo "Deploying region: $region..."

# include common script to populate shared variables
source utils.sh
source common-script.sh

resourceGroupName=$(createResourceName -p "rg" -u $businessUnit -a $appName -e $env -r $region)
storageActName=$(generateStorageAccountName -u $businessUnit -a $appName -e $env -r $region)

echo "Resource Group: $resourceGroupName"
echo "Region Scope: $regionScope"
echo "Business Unit: $businessUnit"
echo "App Name: $appName"
echo "Storage Account Name: $storageActName"
echo "Environment: $env"
echo "Region: $region"
echo

echo "Creating Regional Resource Group: $resourceGroupName"
az group create \
    --name $resourceGroupName \
    --location $region
echo
echo "Deploying regional resources to $resourceGroupName"
az group deployment create \
    --name "Urlist-$region-$(timestamp)" \
    --resource-group $resourceGroupName \
    --template-file region.json \
    --parameters location=$region apimName=$apimName appServicePlanName=$appServicePlanName \
    backendAppName=$backendAppName appInsightsName=$appInsightsName storageActName=$storageActName

echo
echo "Configuring blob storage for static website hosting"
az storage blob service-properties update \
    --account-name $storageActName \
    --static-website \
    --index-document index.html \
    --404-document index.html
