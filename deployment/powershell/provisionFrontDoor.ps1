# This IaC script provisions and configures the front door instance needed by 
# The Urlist.
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
    $frontDoorName,

    [Parameter(Mandatory = $True)]
    [string]
    $resourceGroupName,

    [Parameter(Mandatory = $True)]
    [string]
    $agentReleaseDirectory,

    [Parameter(Mandatory = $True)]
    [string]
    $urlistWebStorageName,

    [Parameter(Mandatory = $True)]
    [string]
    $urlistBackendFunctionName,

    [Parameter(Mandatory = $True)]
    [string]
    $dnsName,

    [Parameter(Mandatory = $True)]
    [string]
    $friendlyDnsName
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
    --subscription "ca-abewan-demo-test"
Write-Output "Done"
Write-Output ""
#endregion

# this defines my time 1 up function which will deploy and configure the infrastructure 
# for Front Door using an ARM template
#
function 1_Up {
    # this creates front door from an arm template. Ironically, there are some stuff in front door
    # that can't be configured by the Azure CLI at this moment. 
    # 
    Write-Output "creating Front Door: $frontDoorName..."
    az group deployment create `
        --name azuredeployfd `
        --resource-group $resourceGroupName `
        --template-file "$agentReleaseDirectory/AbelIaCBuild/drop/AzureCLI/powershell/frontdoorazuredeploy.json" `
        --parameters frontDoorName=$frontDoorName `
                    backendPool1Address1="$urlistWebStorageName.z21.web.core.windows.net" `
                    backendPool2Address1="$urlistBackendFunctionName.azurewebsites.net" `
                    customDomainName="$dnsName"
    Write-Output "Done creating Front Door"
    Write-Output ""

    # this addes the front door extension to the azure cli. It's currently in preview
    # hopefully i can remove this soon
    #
    az extension add `
        --name front-door

    # this enables https for the custom domain front end host using the Azure CLI.
    # Ironically, there are some stuff in front door that can't be configured using 
    # ARM templates at this moment.
    #
    Write-Output "enabling https for front end host $friendlyDnsName..."
    az network front-door frontend-endpoint enable-https `
        --front-door-name $frontDoorName `
        --name $friendlyDnsName `
        --resource-group $resourceGroupName
    Write-Output "Done enabling https"
    Write-Output ""
}

Install-Module -Name VersionInfrastructure -Force -Scope CurrentUser
Update-InfrastructureVersion `
    -infraToolsFunctionName $Env:IAC_EXCLUSIVE_INFRATOOLSFUNCTIONNAME `
    -infraToolsTableName $Env:IAC_INFRATABLENAME `
    -deploymentStage $Env:IAC_DEPLOYMENTSTAGE