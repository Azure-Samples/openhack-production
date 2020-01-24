#!/bin/bash

##############################################################################
#- function used to create a AzDO matrix from region list
#- $n - The regoins to join
##############################################################################
function toRegionMatrix() {
    regions=(${@//,/ })

    for region in ${regions[@]}
    do
        output+="\"$region\": { \"region\": \"$region\" },"
    done

    result=$(printf %s ${output})
    result="{${result%?}}"

    printf $result
}

##############################################################################
#- function used to create a AzDO matrix from environment list
#- $n - The regoins to join
##############################################################################
function toEnvironmentMatrix() {
    environments=(${@//,/ })

    for env in ${environments[@]}
    do
        output+="\"$env\": { \"environment\": \"$env\" },"
    done

    result=$(printf %s ${output})
    result="{${result%?}}"

    printf $result
}
