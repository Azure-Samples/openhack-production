#!/bin/bash

##############################################################################
#- function used to create a AzDO matrix from region list
#- $n - The regoins to join
##############################################################################
function toRegionMatrix() {
    regions=(${@//,/ })

    for region in ${regions[@]}; do
        output+="\"$region\": { \"region\": \"$region\" },"
    done

    result=$(printf %s ${output})
    result="{${result%?}}"

    printf $result
}

##############################################################################
#- function used to create resource names from well known parameters
#- -p Prefix ex) rg, webapp, backned, ai
#- -u Business Unit
#- -a App Name
#- -e Environment ex) prod, staging, dev
#- -r Region ex) westus, centralus, eastus
##############################################################################
function createResourceName() {
    # reset the index for the arguments locally for this function.
    local OPTIND p u a e r
    while getopts ":p:u:a:e:r:" opt; do
        case $opt in
        p) local prefix=${OPTARG//-/} ;;
        u) local businessUnit=${OPTARG//-/} ;;
        a) local appName=${OPTARG//-/} ;;
        e) local env=${OPTARG//-/} ;;
        r) local region=${OPTARG//-/} ;;
        :)
            echo "Error: -${OPTARG} requires a value" >&2
            exit 1
            ;;
        esac
    done
    shift $((OPTIND - 1))

    # Validation
    if [[ -z $prefix || -z $businessUnit || -z $appName || -z $env || -z $region ]]; then
        echo "Required parameters are missing"
        exit 1
    fi

    echo "$prefix-$businessUnit-$appName-$env-$region"
}
