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
#- [-s] subscription - The subscription where the resources will reside.
#- [-u] businessUnit - The business unit used for resource naming convention.
#- [-a] appName - The application name used for resource naming convention.
#- [-e] env - The environment to deploy (ex: dev | test | qa | prod).
#- [-r] region - The Azure region to deploy to (ex: westus | eastus | centralus | etc).
#- [-t] appServicePlanSkuResourceType - The app service plan SKU resource type.
#- [-p] appServicePlanSkuResourceCount - The app service plan SKU resource count.
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
    echo -e "\t-r\t Sets the region"
    echo -e "\t-t\t Sets the app service plan SKU resource type"
    echo -e "\t-p\t Sets the app service plan SKU resource count"
    echo
    echo "Example:"
    echo -e "\tbash deploy.sh -s c77dad45-b62f-467d-bad4-8e00a807c0a2 -u $(whoami) -a urlist -e test -r westus -t S1 -p 1"
}

while getopts "s:u:a:e:r:t:p:hq" opt; do
    case $opt in
    s) subscription=$OPTARG ;;
    u) businessUnit=$OPTARG ;;
    a) appName=$OPTARG ;;
    e) env=$OPTARG ;;
    r) region=$OPTARG ;;
    t) appServicePlanSkuResourceType=$OPTARG ;;
    p) appServicePlanSkuResourceCount=$OPTARG ;;
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

: ${appServicePlanSkuResourceType:="S1"}
: ${appServicePlanSkuResourceCount:=1}

# Validation
if [[ $# -eq 0 || -z $subscription || -z $businessUnit || -z $appName || -z $env || -z $region ]]; then
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
echo "App Service Plan SKU Resource Type: $appServicePlanSkuResourceType"
echo "App Service Plan SKU Resource Count: $appServicePlanSkuResourceCount"
echo

echo "Creating Regional Resource Group: $resourceGroupName"
echo
az group create \
    --subscription $subscription \
    --name $resourceGroupName \
    --location $region \
    --output table
    
echo
echo "Deploying regional resources to $resourceGroupName"
echo
az group deployment create \
    --name "Urlist-$region-$(timestamp)" \
    --resource-group $resourceGroupName \
    --subscription $subscription \
    --template-file region.json \
    --output table \
    --parameters location=$region \
    apimName=$apimName \
    appServicePlanName=$appServicePlanName \
    appServicePlanSkuResourceType=$appServicePlanSkuResourceType \
    appServicePlanSkuResourceCount=$appServicePlanSkuResourceCount \
    backendAppName=$backendAppName \
    storageActName=$storageActName

echo
echo "Configuring blob storage for static website hosting"
az storage blob service-properties update \
    --subscription $subscription \
    --account-name $storageActName \
    --static-website \
    --index-document index.html \
    --404-document index.html \
    --output jsonc
