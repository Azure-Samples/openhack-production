#!/bin/bash

##########################################################################################################################################################################################
#- Purpose: Script is used to deploy the Urlist app
#- Support deploying to multiple regions as well as required global resources
#- Parameters are:
#- [-s] subscription - The subscription where the resources will reside.
#- [-u] businessUnit - The business unit used for resource naming convention.
#- [-a] appName - The application name used for resource naming convention.
#- [-e] env - The environment to deploy (ex: dev | test | qa | prod).
#- [-r] regions - A comma delimted list of regions to deploy to (ex: westus,eastus,centralus).
#- [-c] cosmosdbThroughput - The cosmos database throughput.
#- [-t] appServicePlanSkuResourceType - The app service plan SKU resource type.
#- [-p] appServicePlanSkuResourceCount - The app service plan SKU resource count.
###########################################################################################################################################################################################

set -eu
parent_path=$(
    cd "$(dirname "${BASH_SOURCE[0]}")"
    pwd -P
)
cd "$parent_path"

#######################################################
#- function used to print out script usage
#######################################################
function usage() {
    echo
    echo "Arguments:"
    echo -e "\t-s\t Sets the subscription"
    echo -e "\t-u \t Sets the business unit (required)"
    echo -e "\t-a \t Sets the app name (required)"
    echo -e "\t-e \t Sets the environment (required)"
    echo -e "\t-r \t Sets the region list (Comma delimited values) (required)"
    echo -e "\t-c \t Sets the cosmos db throughput"
    echo -e "\t-t \t Sets the app service plan SKU resource type"
    echo -e "\t-p \t Sets the app service plan SKU resource count"
    echo
    echo "Example:"
    echo -e "\tbash deploy.sh -u $(whoami) -a urlist -e test -r westus,eastus,centralus -c 400 -t S1 -p 1"
}

##############################################################################
#- function used to join a bash array into a string with specified delimiter
#- $1 - The delimiter
#- $n - The values to join
##############################################################################
function join() {
    local d=$1
    shift
    echo -n "$1"
    shift
    printf "%s" "${@/#/$d}"
}

while getopts "s:u:a:e:r:c:t:p:hq" opt; do
    case $opt in
    s) subscription=$OPTARG ;;
    u) businessUnit=$OPTARG ;;
    a) appName=$OPTARG ;;
    e) env=$OPTARG ;;
    r) regions=(${OPTARG//,/ }) ;;
    c) cosmosdbThroughput=$OPTARG ;;
    t) appServicePlanSkuResourceType=$OPTARG ;;
    p) appServicePlanSkuResourceCount=$OPTARG ;;
    :)
        echo "Error: -${OPTARG} requires a value"
        exit 1
        ;;
    *)
        usage
        exit 1
        ;;
    esac
done

: ${appServicePlanSkuResourceType:="S1"}
: ${appServicePlanSkuResourceCount:=1}
: ${cosmosdbThroughput:=400}

# Validation
if [[ $# -eq 0 || -z $subscription || -z $businessUnit || -z $appName || -z $env || -z $regions ]]; then
    echo "Required parameters are missing"
    usage
    exit 1
fi

# Deploy scale unit per region
for region in ${regions[@]}; do
    bash deploy-region.sh \
        -s $subscription \
        -u $businessUnit \
        -a $appName \
        -e $env \
        -r $region \
        -t $appServicePlanSkuResourceType \
        -p $appServicePlanSkuResourceCount
done

# Deploy global resources
bash deploy-global.sh \
    -s $subscription \
    -u $businessUnit \
    -a $appName \
    -e $env \
    -r $(join , ${regions[*]}) \
    -c $cosmosdbThroughput
