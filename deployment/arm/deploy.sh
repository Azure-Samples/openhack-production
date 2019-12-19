#!/bin/bash
parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

frontendHosts='("wabrez-westus2-frontend.azurewebsites.net","wabrez-eastus-frontend.azurewebsites.net")'
backendendHosts='("wabrez-westus2-apim.azure-api.net","wabrez-eastus-apim.azure-api.net")'

bash deploy-region.sh wabrez westus2 urlist
bash deploy-region.sh wabrez eastus urlist
bash deploy-global.sh wabrez urlist $frontendHosts $backendendHosts