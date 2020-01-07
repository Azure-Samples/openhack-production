#!/bin/bash
parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

regions=(westus2 eastus centralus)
# uncomment -$RANDOM to generate unique business unit names. 
businessUnit=prodoh #-$RANDOM 
appName=urlist
env=dev
scope="$businessUnit-$appName-$env"

# Deploy scale unit per region
for region in ${regions[*]}
do
  bash deploy-region.sh $businessUnit $appName $env $region
done

# Deploy global resources
bash deploy-global.sh $businessUnit $appName $env ${regions[*]}
