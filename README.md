---
page_type: sample
languages:
- csharp
products:
- dotnet
description: "Add 150 character max description"
urlFragment: "update-this-to-unique-url-stub"
---

# Official Microsoft Sample

<!-- 
Guidelines on README format: https://review.docs.microsoft.com/help/onboard/admin/samples/concepts/readme-template?branch=master

Guidance on onboarding samples to docs.microsoft.com/samples: https://review.docs.microsoft.com/help/onboard/admin/samples/process/onboarding?branch=master

Taxonomies for products and languages: https://review.docs.microsoft.com/new-hope/information-architecture/metadata/taxonomies?branch=master
-->

This is a replica of the Urlist website but re-architected to use server based backend hosted in Azure AppService instead of being serverless. The original project repository can be found here https://github.com/the-urlist

Please [check here](docs/infrastructure.md) to view and read about the new architecture.

## Contents

Outline the file contents of the repository. It helps users navigate the codebase, build configuration and any related assets.

| File/folder       | Description                                |
|-------------------|--------------------------------------------|
| `api`             | The backend code base implemented using .Net Core. |
| `frontend`        | The frontend code base implemented using VUE and Typescript |
| `deployment`      | This folder holds deployment scripts to setup infrastructure. |
| `pipelines`       | This folder holds YAML files to create Azure pipelines to deploy the project |
| `docs`            | This folder holds relevant documenation about the project |
| `.gitignore`      | Define what to ignore at commit time.      |
| `CHANGELOG.md`    | List of changes to the sample.             |
| `CONTRIBUTING.md` | Guidelines for contributing to the sample. |
| `README.md`       | This README file.                          |
| `LICENSE`         | The license for the sample.                |

## Prerequisites

You need to have the following available

* An Azure subscription
* Check out the [API folder for backend prerequisites](api/README.md)
* Check out the [Frontend folder for frontend prerequisites](frontend/README.md)

## Setup

* Check out the [deployment folder](deployment/README.md) to setup the Azure infrastructure automatically.
* Check out the [pipelines folder](pipelines/README.md) to create the needed pipelines to automate deployments and infrastructure updates.

## Running the sample

Once the pipelines are setup and had successfully deployed the project, find your Azure Frontdoor endpoint and use it to test the website.

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
