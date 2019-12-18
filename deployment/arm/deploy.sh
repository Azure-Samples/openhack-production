#!/bin/bash
parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

prefix=$1
location=$2
appName=$3
fullPrefix="$1-$2-$3"

echo "Prefix: $prefix"
echo "Location: $location"
echo "App Name: $appName"
echo "Prefix: $fullPrefix"

frontDoorName="$fullPrefix-fd"
apimName="$fullPrefix-apim"
appServicePlanName="$fullPrefix-asp"
frontendAppName="$fullPrefix-frontend"
backendAppName="$fullPrefix-backend"
appInsightsName="$fullPrefix-ai"

az group deployment create \
  --resource-group wabrez-westus2-urlist-rg \
  --template-file template.json \
  --parameters \
    location=$location \
    frontDoorName=$frontdoorName \
    apimName=$apimName \
    appServicePlanName=$appServicePlanName \
    frontendAppName=$frontendAppName \
    backendAppName=$backendAppName \
    appInsightsName=$appInsightsName