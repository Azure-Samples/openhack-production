#!/bin/bash
parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

bash deploy-region.sh wabrez westus2 urlist
bash deploy-region.sh wabrez eastus urlist
bash deploy-global.sh wabrez westus2 urlist