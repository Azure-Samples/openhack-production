#!/bin/bash
parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

regions=(westus2 eastus centralus)
prefix=wabrez
appName=urlist

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
  frontendHostArray+=("$prefix-$region-$appName-frontend.azurewebsites.net")
  backendHostArray+=("$prefix-$region-$appName-apim.azure-api.net")
  bash deploy-region.sh $prefix $region $appName
done

frontendHosts=$(toArmArray ${frontendHostArray[*]})
backendHosts=$(toArmArray ${backendHostArray[*]})

bash deploy-global.sh wabrez urlist $frontendHosts $backendHosts
