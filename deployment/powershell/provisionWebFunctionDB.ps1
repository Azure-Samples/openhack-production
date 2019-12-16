# This IaC script provisions and configures the web stite hosted in azure
# storage, the back end function and the cosmos db
#
[CmdletBinding()]
param(
    [Parameter(Mandatory = $True)]
    [string]
    $servicePrincipal,

    [Parameter(Mandatory = $True)]
    [string]
    $servicePrincipalSecret,

    [Parameter(Mandatory = $True)]
    [string]
    $servicePrincipalTenantId,

    [Parameter(Mandatory = $True)]
    [string]
    $azureSubscriptionName,

    [Parameter(Mandatory = $True)]
    [string]
    $resourceGroupName,

    [Parameter(Mandatory = $True)]
    [string]
    $resourceGroupNameRegion,

    [Parameter(Mandatory = $True)]
    [string]
    $webStorageAccountName,

    [Parameter(Mandatory = $True)]
    [string]
    $webStorageAccountRegion,

    [Parameter(Mandatory = $True)]
    [string]
    $webStorageAccountSku,

    [Parameter(Mandatory = $True)]
    [string]
    $webErrorDocName,

    [Parameter(Mandatory = $True)]
    [string]
    $webIndexPage,

    [Parameter(Mandatory = $True)]
    [string]
    $cosmosAccountName,

    [Parameter(Mandatory = $True)]
    [string]
    $cosmosDBName,

    [Parameter(Mandatory = $True)]
    [string]
    $cosmosContainerName,

    [Parameter(Mandatory = $True)]
    [string]
    $cosmosThroughput,

    [Parameter(Mandatory = $True)]
    [string]
    $functionStorageAccountName,

    [Parameter(Mandatory = $True)]
    [string]
    $functionStorageAccountRegion,

    [Parameter(Mandatory = $True)]
    [string]
    $functionStorageAccountSku,

    [Parameter(Mandatory = $True)]
    [string]
    $functionConsumptionPlanRegion,

    [Parameter(Mandatory = $True)]
    [string]
    $functionName,

    [Parameter(Mandatory = $True)]
    [string]
    $twitterConsumerKey,

    [Parameter(Mandatory = $True)]
    [string]
    $twitterPrivateKey,

    [Parameter(Mandatory = $True)]
    [string]
    $functionAppInsightName,

    [Parameter(Mandatory = $True)]
    [string]
    $functionAppInsightRegion
)


#region Login

# This logs in a service principal
#
Write-Output "Logging in to Azure with a service principal..."
az login `
    --service-principal `
    --username $servicePrincipal `
    --password $servicePrincipalSecret `
    --tenant $servicePrincipalTenantId
Write-Output "Done"
Write-Output ""

# This sets the subscription to the subscription I need all my apps to
# run in
#
Write-Output "Setting default azure subscription..."
az account set `
    --subscription $azureSubscriptionName
Write-Output "Done"
Write-Output ""
#endregion

# this defines my time 1 up function which will deploy and configure the infrastructure 
# for my static web site in azure storage, back end function and cosmos DB
#
function 1_Up {
    Write-Output "In function 1_Up"
    # # This creates the resource group used to house all of the URList application
    # #
    Write-Output "Creating resource group $resourceGroupName in region $resourceGroupNameRegion..."
    az group create `
        --name $resourceGroupName `
        --location $resourceGroupNameRegion
    Write-Output "Done creating resource group"
    Write-Output ""

    # This creates a storage account to host our static web site
    #
    Write-Output "Creating storage account $webStorageAccountName in resource group $resourceGroupName..."
    az storage account create `
        --location $webStorageAccountRegion `
        --name $webStorageAccountName `
        --resource-group $resourceGroupName `
        --sku "$webStorageAccountSku" `
        --kind StorageV2
    Write-Output "Done creating storage account"
    Write-Output ""

    # This sets the storage account so it can host a static website
    #
    Write-Output "adding storage preview to CLI..."
    az extension add `
        --name storage-preview
    Write-Output "Done adding storage preview to CLI"
    Write-Output "Enabling static website hosting in storage account $webStorageAccountName..."
    az storage blob service-properties update `
        --account-name $webStorageAccountName `
        --static-website `
        --404-document $webErrorDocName `
        --index-document $webIndexPage
    Write-Output "Done enabling static website hosting in storage account"
    Write-Output ""

    # this create a SQL API Cosmos DB account 
    #
    Write-Output "Creating cosmos db account..."
    az cosmosdb create `
        --name $cosmosAccountName `
        --resource-grou $resourceGroupName
    Write-Output "Done creating cosmos db account"
    Write-Output ""

    # This ccreates a database for urlist
    #
    Write-Output "create the db $cosmosDBName for urlist in cosmos..."
    az cosmosdb database create `
        --name $cosmosAccountName `
        --db-name $cosmosDBName `
        --resource-group $resourceGroupName
    Write-Output "Done creating db for urlist in cosmos"
    Write-Output ""

    # this creates a fixed-size container and 400 RU/s
    #
    Write-Output "create a fixed size container collection and 400 RU/s..."
        az cosmosdb collection create `
            --resource-group $resourceGroupName `
            --collection-name $cosmosContainerName `
            --name $cosmosAccountName `
            --db-name $cosmosDBName `
            --throughput $cosmosThroughput `
            --partition-key-path /vanityUrl
    Write-Output "done creating container collection"
    Write-Output ""

    # this creates a storage account for our back end azure function to maintain
    # state and other info for the function
    # 
    Write-Output "create a storage account for function to maintain state and other info for the function..."
    az storage account create `
        --name $functionStorageAccountName `
        --location $functionStorageAccountRegion `
        --resource-group $resourceGroupName `
        --sku $webStorageAccountSku
    Write-Output "done creating storage account for function"
    Write-Output ""

    # this creates the function app used to host the back end function
    #
    Write-Output "create the function app for the back end..."
    az functionapp create `
        --resource-group $resourceGroupName `
        --consumption-plan-location $functionConsumptionPlanRegion `
        --name $functionName `
        --storage-account $functionStorageAccountName `
        --runtime dotnet
    Write-Output "done creating function app"
    Write-Output ""

    # this grabs the static website storage primary endpoint to be used when
    # setting the authentication of the function
    Write-Output "getting the static website storage's primary endpoint..."
    $staticWebsiteUrl=$(az storage account show -n $webStorageAccountName -g $resourceGroupName --query "primaryEndpoints.web" --output tsv)
    Write-Output "done getting static websites endpoint: $staticWebsiteUrl"
    Write-Output ""

    # this sets authentication to be on and to use twitter for the back end
    # function, also sets the allowed external redirect urls to be the
    # static website storage primary endpoint
    #
    Write-Output "setting authentication for the azure function app back end..."
    az webapp auth update `
        --name $functionName `
        --resource-group $resourceGroupName `
        --enabled true `
        --action LoginWithTwitter `
        --twitter-consumer-key $twitterConsumerKey `
        --twitter-consumer-secret $twitterPrivateKey `
        --allowed-external-redirect-urls $staticWebsiteUrl
    Write-Output "done setting authentication to the azure function"
    Write-Output ""

    # this creates an instance of appliction insight
    #
    Write-Output "creating application insight for the function..."
    $appInsightCreateResponse=$(az resource create `
        --resource-group $resourceGroupName `
        --resource-type "Microsoft.Insights/components" `
        --name $functionAppInsightName `
        --location $functionAppInsightRegion `
        --properties '{\"Application_Type\":\"web\"}') | ConvertFrom-Json
    Write-Output "done creating app insight, response: $appInsightCreateResponse"
    Write-Output ""

    # this gets the instrumentation key from the create response
    #
    Write-Output "getting instrumentation key from the create response..."
    $instrumentationKey = $appInsightCreateResponse.properties.InstrumentationKey
    Write-Output "cone getting instrumentation key"
    Write-Output ""

    # this wires up application insights to the function
    # echo "wiring up app insight to function"
    #
    Write-Output "setting application insight to the function..."
    az functionapp config appsettings set `
        --name $functionName `
        --resource-group $resourceGroupName `
        --settings "APPINSIGHTS_INSTRUMENTATIONKEY = $instrumentationKey"
    Write-Output "done setting application insight to the function"
    Write-Output ""
}

Install-Module -Name VersionInfrastructure -Force -Scope CurrentUser
Update-InfrastructureVersion `
    -infraToolsFunctionName $Env:IAC_EXCLUSIVE_INFRATOOLSFUNCTIONNAME `
    -infraToolsTableName $Env:IAC_INFRATABLENAME `
    -deploymentStage $Env:IAC_DEPLOYMENTSTAGE