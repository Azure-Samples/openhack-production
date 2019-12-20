#!/bin/bash
parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

regions=(westus2 eastus centralus)
businessUnit=prodoh
appName=urlist
env=dev
scope="$businessUnit-$appName-$env"

# Convert array to ARM array format
toArmArray() {
  array=("$@")
  value=$(printf "\"%s\"", "${array[@]}")
  value="(${value%?})"
  printf $value
}

frontendHostArray=()
backendHostArray=()

# Deploy scale unit per region
for region in ${regions[*]}
do
  frontendHostArray+=("frontend-$scope-$region.azurewebsites.net")
  backendHostArray+=("apim-$scope-$region.azure-api.net")
  bash deploy-region.sh $businessUnit $appName $env $region
done

frontendHosts=$(toArmArray ${frontendHostArray[*]})
backendHosts=$(toArmArray ${backendHostArray[*]})

bash deploy-global.sh $businessUnit $appName $env $frontendHosts $backendHosts
