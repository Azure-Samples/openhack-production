#!/bin/bash
az extension add \
    --name application-insights

az extension add \
    --name front-door

resourceGroupName="wabrez-az-cli-test-rg"
location="westus2"
appServicePlanName="wabrez-az-cli-test-asp"

echo "Creating Resource Group..."
az group create \
    --name $resourceGroupName \
    --location $location

echo "Creating Application Insights..."
az monitor app-insights component create\
    --resource-group $resourceGroupName \
    --app wabrez-az-cli-test-ai \
    --location $location

echo "Creating App Service Plan..."
az appservice plan create \
    --resource-group $resourceGroupName \
    --location $location \
    --name $appServicePlanName \
    --sku S1

echo "Creating Frontend..."
az webapp create \
    --resource-group $resourceGroupName \
    --plan $appServicePlanName \
    --name wabrez-az-cli-test-frontend

echo "Creating Backend..."
az webapp create \
    --resource-group $resourceGroupName \
    --plan $appServicePlanName \
    --name wabrez-az-cli-test-backend

frontDoorName=wabrez-az-cli-test-fd
frontendLoadBalancer="frontend-loadbalancer"
frontendProbe="frontend-health-probe"

echo "Creating Frontdoor..."
az network front-door create \
    --resource-group $resourceGroupName \
    --name $frontDoorName \
    --backend-address "wabrez-az-cli-test-fd.azurefd.net"

echo "Creating Frontdoor frontent endpoint..."
az network front-door frontend-endpoint create \
    --resource-group $resourceGroupName \
    --front-door-name $frontDoorName \
    --name "endpoint" \
    --host-name "wabrez-az-cli-test-fd.azurefd.net"

echo "Creating Frontdoor load balancer..."
az network front-door load-balancing create \
    --resource-group $resourceGroupName \
    --front-door-name $frontDoorName \
    --name $frontendLoadBalancer \
    --sample-size 4 \
    --successful-samples-required 4

echo "Creating Frontdoor health probe..."
az network front-door probe create \
    --resource-group $resourceGroupName \
    --front-door-name $frontDoorName \
    --name $frontendProbe \
    --interval 30 \
    --path "/" \
    --enabled Enabled \
    --probeMethod GET \
    --protocol Https

echo "Creating Frontdoor backend pool..."
az network front-door backend-pool create \
    --resource-group $resourceGroupName \
    --front-door-name $frontDoorName \
    --load-balancing $frontendLoadBalancer \
    --probe $frontendProbe \
    --name Frontend \
    --address "wabrez-az-cli-test-frontend.azurewebsites.net"

echo "Creating API Management..."
az apim create \
    --resource-group $resourceGroupName \
    --location $location \
    --name wabrez-az-cli-test-apim \
    --sku-name Consumption \
    --publisher-email wabrez@microsoft.com \
    --publisher-name Microsoft