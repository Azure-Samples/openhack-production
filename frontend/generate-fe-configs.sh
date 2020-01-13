#!/bin/bash

set -eu
parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

######################################################################
#- Purpose: This script will produce the configuration file for this frontend
#- the config file is used to pass the endpoints hosting the frontend
#- website and the backend stack supporting it!
#- 
#- It should populate two configs as key & value pairs:
#- VUE_APP_FRONTEND: which contains the URL to frontend after hosting
#- VUE_APP_BACKEND: which contains the URL for the backend stack
#-
#- the configs need to be formated as "key=value" per line.
#-
#- Parameters are:
#- [-u] businessUnit - The business unit used for resource naming convention.
#- [-a] appName - The application name used for resource naming convention.
#- [-e] env - The environment to deploy (ex: dev | test | qa | prod).
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
    echo
    echo "Example:"
    echo -e "\tbash generate-fe-configs.sh -u $(whoami) -a urlist -e test"
}

while getopts "u:a:e:r:hq" opt
do
    case $opt in
        u) businessUnit=$OPTARG;;
        a) appName=$OPTARG;;
        e) env=$OPTARG;;
        :) echo "Error: -${OPTARG} requires a value"; exit 1;;
        *) usage;exit 1;;
    esac
done

# Validation
if [[ $# -eq 0 || -z $businessUnit || -z $appName || -z $env ]]
then
    echo "Required parameters are missing"
    usage
    exit 1
fi

# include common script to populate shared variables
source ../deployment/arm/common-script.sh


echo "VUE_APP_FRONTEND=https://$frontDoorEndpoint" > .env.production
echo "VUE_APP_BACKEND=https://$frontDoorEndpoint" >> .env.production 

