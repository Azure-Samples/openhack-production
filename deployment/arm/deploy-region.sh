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
    echo -e "\t-r\t Sets the region"
    echo
    echo "Example:"
    echo -e "\tbash deploy.sh -u $(whoami) -a urlist -e test -r westus"
}

while getopts "u:a:e:r:hq" opt
do
    case $opt in
        u) businessUnit=$OPTARG;;
        a) appName=$OPTARG;;
        e) env=$OPTARG;;
        r) region=$OPTARG;;
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

if [ -z $region ]
then
    echo "Region is required"
    usage
    exit 2
fi

scope="$businessUnit-$appName-$env-$region"
resourceGroupName="rg-$scope"

echo "Resource Group: $resourceGroupName"
echo "Region Scope: $scope"
echo "Business Unit: $businessUnit"
echo "App Name: $appName"
echo "Environment: $env"
echo "Region: $region"

apimName="apim-$scope"
appServicePlanName="asp-$scope"
frontendAppName="frontend-$scope"
backendAppName="backend-$scope"
appInsightsName="ai-$scope"

timestamp() {
    date +"%Y%m%dZ%H%M%S"
}

echo "Creating Regional Resource Group: $resourceGroupName"
az group create \
--name $resourceGroupName \
--location $region

echo "Deploying regional resources to $resourceGroupName"
az group deployment create \
--name "Urlist-$region-$(timestamp)" \
--resource-group $resourceGroupName \
--template-file region.json \
--parameters location=$region apimName=$apimName appServicePlanName=$appServicePlanName frontendAppName=$frontendAppName backendAppName=$backendAppName appInsightsName=$appInsightsName
