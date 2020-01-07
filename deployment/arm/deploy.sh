#!/bin/bash
parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

businessUnit=$1
appName=$2
env=$3
regions=()

# Validation
if [ $# -lt 4 ]
then
  echo Location list is required
  exit
fi

# Deploy scale unit per region
for region in $@
do
  let counter++
  if [ $counter -gt 3 ]
  then
    bash deploy-region.sh $businessUnit $appName $env $region
    regions+=($region)
  fi
done

# Deploy global resources
bash deploy-global.sh $businessUnit $appName $env ${regions[*]}
