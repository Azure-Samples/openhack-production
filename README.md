---
page_type: sample
languages:
  - csharp
products:
  - dotnet
description: Add 150 character max description
urlFragment: update-this-to-unique-url-stub
---
# Production Fundamentals OpenHack Sample Application

This is a replica of the Urlist website but re-architected to use Azure AppService to host the backend instead of using Azure Functions. The original project repository can be found here <https://github.com/the-urlist>

Please [check here](docs/Infrastructure.md) to view and read about the architecture.

---

## Table Of Contents

<!-- toc -->
- [Glossary](#glossary)
- [Contents](#contents)
- [Prerequisites](#prerequisites)
- [Setup](#setup)
- [Running the sample](#running-the-sample)
- [Contributing](#contributing)

<!-- tocstop -->

## Glossary

- Vanity URL: is a descriptive, human-readable, an easy to remember URL that is typically created to simplify sharing a long and hard-to-remember URLs.
- Link Bundle: a bundle/list of URLs that the user associated with a vanity URL using the Urlist application.

## Contents

Outline the file contents of the repository. It helps users navigate the codebase, build configuration and any related assets.

| File/folder       | Description                                |
|-------------------|--------------------------------------------|
| `api`             | The backend code base implemented using .Net Core. |
| `frontend`        | The frontend code base implemented using VUE and Typescript |
| `deployment`      | This folder holds deployment scripts to setup infrastructure. |
| `pipelines`       | This folder holds YAML files to create Azure pipelines to deploy the project |
| `docs`            | This folder holds relevant documentation about the project |
| `.gitignore`      | Define what to ignore at commit time.      |
| `CHANGELOG.md`    | List of changes to the sample.             |
| `CONTRIBUTING.md` | Guidelines for contributing to the sample. |
| `README.md`       | This README file.                          |
| `LICENSE`         | The license for the sample.                |

## Prerequisites

You need to have the following available

- An Azure subscription
- Check out the [API folder for backend prerequisites](api/README.md)
- Check out the [Frontend folder for frontend prerequisites](frontend/README.md)

## Linting

Please see [the Linting guide](./LINTING.md) to set up your environment.

## Setup

- Check out the [deployment folder](deployment/README.md) to setup the Azure infrastructure automatically.
- Check out the [pipelines folder](pipelines/README.md) to create the needed pipelines to automate deployments and infrastructure updates.

## Running the sample

Once the pipelines are setup and had successfully deployed the project, find your Azure Front Door endpoint and use it to test the website.

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit <https://cla.opensource.microsoft.com>

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
