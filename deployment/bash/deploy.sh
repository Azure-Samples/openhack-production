#!/bin/bash
resourceGroupName="wabrez-az-cli-test-rg"
location="westus2"
appServicePlanName="wabrez-az-cli-test-asp"

echo "Creating Resource Group..."
az.cmd group create \
    --name $resourceGroupName \
    --location $location

echo "Creating Application Insights..."
az.cmd monitor app-insights component create\
    --resource-group $resourceGroupName \
    --app wabrez-az-cli-test-ai \
    --location $location

echo "Creating App Service Plan..."
az.cmd appservice plan create \
    --resource-group $resourceGroupName \
    --location $location \
    --name $appServicePlanName \
    --sku S1

echo "Creating Frontend..."
az.cmd webapp create \
    --resource-group $resourceGroupName \
    --plan $appServicePlanName \
    --name wabrez-az-cli-test-frontend

echo "Creating Backend..."
az.cmd webapp create \
    --resource-group $resourceGroupName \
    --plan $appServicePlanName \
    --name wabrez-az-cli-test-backend

frontDoorName=wabrez-az-cli-test-fd
frontendLoadBalancer="frontend-loadbalancer"
frontendProbe="frontend-health-probe"

echo "Creating Frontdoor..."
az.cmd network front-door create \
    --resource-group $resourceGroupName \
    --name $frontDoorName \
    --backend-address "wabrez-az-cli-test-fd.azurefd.net"

echo "Creating Frontdoor frontent endpoint..."
az.cmd network front-door frontend-endpoint create \
    --resource-group $resourceGroupName \
    --front-door-name $frontDoorName \
    --name "Public Endpoint" \
    --host-name "wabrez-az-cli-test-fd.azurefd.net"

echo "Creating Frontdoor load balancer..."
az.cmd network front-door load-balancing create \
    --resource-group $resourceGroupName \
    --front-door-name $frontDoorName \
    --name $frontendLoadBalancer \
    --sample-size 4 \
    --successful-samples-required 4

echo "Creating Frontdoor health probe..."
az.cmd network front-door probe create \
    --resource-group $resourceGroupName \
    --front-door-name $frontDoorName \
    --name $frontendProbe \
    --interval 30 \
    --path "/" \
    --enabled Enabled \
    --probeMethod GET \
    --protocol Https

echo "Creating Frontdoor backend pool..."
az.cmd network front-door backend-pool create \
    --resource-group $resourceGroupName \
    --front-door-name $frontDoorName \
    --load-balancing $frontendLoadBalancer \
    --probe $frontendProbe \
    --name Frontend \
    --address "https://wabrez-az-cli-test-frontend.azurewebsites.net"

# echo "Creating API Management..."
# az.cmd apim create \
#     --resource-group $resourceGroupName \
#     --location $location \
#     --name wabrez-az-cli-test-apim \
#     --sku-name Consumption \
#     --publisher-email wabrez@microsoft.com \
#     --publisher-name Microsoft