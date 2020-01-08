#!/bin/bash
parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

IFS=','

usage() {
  echo "Arguments:"
  echo -e "\t-u\t Sets the business unit"
  echo -e "\t-a\t Sets the app name"
  echo -e "\t-e\t Sets the environment"
  echo -e "\t-r\t Sets the region list (Comma delimited values)"
  echo
  echo "Example:"
  echo -e "\tbash deploy.sh -u $(whoami) -a urlist -e test -r westus,eastus,centralus"
}

while getopts "u:a:e:r:hq" opt
do
    case $opt in
        u) businessUnit=$OPTARG;;
        a) appName=$OPTARG;;
        e) env=$OPTARG;;
        r) regions=($OPTARG);;
        :) echo "Error: -${OPTARG} requires a value"; exit 2;;
        *) usage;exit 2;;
    esac
done

# Validation
if [ -z $businessUnit ]
then
  echo "Business Unit is required"
  usage
  exit 2
fi

if [ -z $appName ]
then
  echo "App name is required"
  usage
  exit 2
fi

if [ -z $env ]
then
  echo "Environment is required"
  usage
  exit 2
fi

if [ -z $regions ]
then
  echo "Regions is required"
  usage
  exit 2
fi

# Deploy scale unit per region
for region in ${regions[@]}
do
    bash deploy-region.sh $businessUnit $appName $env $region
done

# Deploy global resources
bash deploy-global.sh $businessUnit $appName $env ${regions[*]}
