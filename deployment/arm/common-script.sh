#!/bin/bash

##############################################################
#- This file holds the script commonly used across the
#- the solutions deployment scripts.
##############################################################

parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

##############################################################
#- function to create a timestamp string
##############################################################
function timestamp() {
    date +"%Y%m%dZ%H%M%S"
}

##############################################################
#- function used to convert a bash array to ARM array format
#- $n - The values to join
##############################################################
function toArmArray() {
    array=("$@")
    value=$(printf "\"%s\"", "${array[@]}")
    value="[${value%?}]"
    printf $value
}

function generateStorageAccountName() {
    #######################################################
    #- function used to print out script usage
    #######################################################
    function genStorageActNameUsage() {
        echo
        echo "Arguments:"
        echo -e "\t-u\t Sets the business unit"
        echo -e "\t-a\t Sets the app name"
        echo -e "\t-e\t Sets the environment"
        echo -e "\t-r\t Sets the region"
        echo
        echo "Example:"
        echo -e "\t generateStorageAccountName -u $(whoami) -a urlist -e test -r westus"
    }
    
    # reset the index for the arguments locally for this function.
    local OPTIND u a e r 
    while getopts ":u:a:e:r:" opt
    do
        case $opt in
            u) local businessUnit=${OPTARG//-};;
            a) local appName=${OPTARG//-};;
            e) local env=${OPTARG//-};;
            r) local region=${OPTARG//-};;
            :) echo "Error: -${OPTARG} requires a value" >& 2; exit 1;;
            *) genStorageActNameUsage;exit 1;;
        esac
    done
    shift $((OPTIND-1))

    # Validation
    if [[ -z $businessUnit || -z $appName || -z $env || -z $region ]]
    then
        echo "Required parameters are missing"
        genStorageActNameUsage
        exit 1
    fi

    # Storage account names can only contain lowercase letters and numbers
    # they need to be unique globally! Another restriction is they need
    # to be between 3-24 in length, the approach we follow here
    # is to truncate each part of the name to 5 characters totaling to 22 
    # characters in length.
    storageActName="sa${businessUnit:0:10}${appName:0:5}${env:0:2}${region:0:3}"
    storageActName=$(echo "$storageActName" | tr "[:upper:]" "[:lower:]")
    
    # Return storage account name
    echo $storageActName
}

# Deployment scopes
globalScope="$businessUnit-$appName-$env-gbl"
regionScope="$businessUnit-$appName-$env"

# Global configs
frontDoorName="fd-$globalScope"
frontDoorEndpoint="$frontDoorName.azurefd.net"
cosmosdbName="db-$globalScope"

# Regional configs
apimName="apim-$regionScope"
appServicePlanName="asp-$regionScope"
frontendAppName="frontend-$regionScope"
backendAppName="backend-$regionScope"
appInsightsName="ai-$regionScope"
