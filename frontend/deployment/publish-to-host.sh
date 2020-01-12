#!/bin/bash
set -eu

##########################################################################################################################################################################################
#- Purpose: Script is used to publish the frontend to the static website hosting 
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
    echo -e "\tbash publish-to-host.sh -u $(whoami) -a urlist -e test -r westus"
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

# include common script to populate shared variables
common-script.sh

storageActName=$(generateStorageAccountName -u $businessUnit -a $appName -e $env -r $region)

# upload to host
az storage blob upload-batch -s dist -d \$web --account-name $storageActName