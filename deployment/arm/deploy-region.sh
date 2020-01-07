#!/bin/bash

##########################################################################################################################################################################################
#- Purpose: Script is used to deploy the regional resources for the Urlist app
#- Parameters are:
#- [-u] businessUnit - The business unit used for resource naming convention.
#- [-a] appName - The application name used for resource naming convention.
#- [-e] env - The environment to deploy (ex: dev | test | qa | prod).
#- [-r] region - The Azure region to deploy to (ex: westus | eastus | centralus | etc).
###########################################################################################################################################################################################

set -eu
parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

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

##############################################################
#- function to create a timestamp string
##############################################################
function timestamp() {
    date +"%Y%m%dZ%H%M%S"
}

while getopts "u:a:e:r:hq" opt
do
    case $opt in
        u) businessUnit=$OPTARG;;
        a) appName=$OPTARG;;
        e) env=$OPTARG;;
        r) region=$OPTARG;;
        :) echo "Error: -${OPTARG} requires a value"; exit 1;;
        *) usage;exit 1;;
    esac
done

# Validation
if [[ $# -eq 0 || -z $businessUnit || -z $appName || -z $env || -z $region ]]
then
    echo "Required parameters are missing"
    usage
    exit 1
fi

scope="$businessUnit-$appName-$env-$region"
resourceGroupName="rg-$scope"
# Storage account names can only contain lowercase letters and numbers
# they need to be unique globally!
storageActName=$(echo "${businessUnit//-}$location" | tr "[:upper:]" "[:lower:]")

echo "Resource Group: $resourceGroupName"
echo "Region Scope: $scope"
echo "Business Unit: $businessUnit"
echo "App Name: $appName"
echo "Environment: $env"
echo "Region: $region"
echo

apimName="apim-$scope"
appServicePlanName="asp-$scope"
frontendAppName="frontend-$scope"
backendAppName="backend-$scope"
appInsightsName="ai-$scope"

echo "Creating Regional Resource Group: $resourceGroupName"
az group create \
--name $resourceGroupName \
--location $region

echo "Deploying regional resources to $resourceGroupName"
az group deployment create \
  --name "Urlist-$location-$(timestamp)" \
  --resource-group $resourceGroupName \
  --template-file region.json \

  --parameters location=$location apimName=$apimName appServicePlanName=$appServicePlanName frontendAppName=$frontendAppName backendAppName=$backendAppName appInsightsName=$appInsightsName storageActName=$storageActName

echo "Configuring blob storage for static website hosting"
az storage blob service-properties update \
  --account-name $storageActName \
  --static-website \
  --index-document index.html \
  --404-document index.html
