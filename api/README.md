# The Urlist - Backend

[![License](https://img.shields.io/badge/license-MIT-orange.svg)](https://raw.githubusercontent.com/Azure-Samples/openhack-production/master/LICENSE)

The backend for this project is built as a .Net Core API using .NET Core. All the data is stored in a Cosmos DB collection using the SQL API.

- [.NET Core](https://dotnet.microsoft.com)
- [Azure Cosmos DB](https://azure.microsoft.com/services/cosmos-db)

---

## Table Of Contents

<!-- toc -->

- [Build and run the ASP.NET Core backend locally](#build-and-run-the-aspnet-core-backend-locally)
- [Using the API](#using-the-api)
- [Testing](#testing)
- [Docker local development](#docker-local-development)
- [Troubleshooting local development certificate issues](#troubleshooting-local-development-certificate-issues)

<!-- tocstop -->

## Build and run the ASP.NET Core backend locally

### Prerequisites

- Install the [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download). This repo is pinned to use version 3.1.x of the SDK.
- Install [Visual Studio](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio Community edition](https://visualstudio.microsoft.com/vs)
- Install the [C# extension for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp)
- The infrastructure is setup and available as described [here](../docs/Infrastructure.md)

#### Optional

- Install [Windows Subsystem for Linux](https://docs.microsoft.com/en-us/windows/wsl/install-win10)
- Install [Postman](https://www.getpostman.com/)
- Install [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)

Copy the contents of the `appsettings.sample.json` into the `appsettings.Development.json` file:

```bash
# Navigate to the project directory
cd api/src/LinkyLink

# Copy sample app settings
cp appsettings.sample.json appsettings.Development.json
```

### Setup Database

Update the `appsettings.Development.json` file with your Cosmos DB Uri and Primary Key in the `ServiceEndpoint` & `AuthKey` settings respectively. You dont need to worry about setting up the database as it will be created automatically for you when you create your first link-bundle through the frontend.

You can find the the value for `ServiceEndpoint` & `AuthKey` from the Cosmos DB resource in Azure portal or from Azure DevOps variable group (Pipelines/Library/Variable groups).

### Setup Azure AD B2C Configuration

This application requires [Azure Active Directory B2C](../docs/AzureADB2C.md) for authentication/authorization. Each API request performs JWT validation from your Azure B2C tenant.

Update `appsettings.Development.json` file with your tenants configuration:

```json
"AzureAdB2C": {
  "Instance": "https://[YourTenantName].b2clogin.com/tfp/",
  "ClientId": "[The Azure B2C application client ID]",
  "Name": "[YourTenantName]",
  "Domain": "[YourTenantName].onmicrosoft.com",
  "SignUpSignInPolicyId": "[Your Azure B2C policy name]"
},
```

### Run from your favorite terminal

Set the `ASPNETCORE_ENVIRONMENT` environment variable. To

```bash
export ASPNETCORE_ENVIRONMENT=Development
```

Navigate into backend folder

```bash
cd /api/src/LinkyLink
```

Build the project

```bash
dotnet build
```

Start the API via the command line

```bash
dotnet run
```

> If you receive SSL errors in your browser review the [Troubleshooting local development certificate issues](#Troubleshooting-local-development-certificate-issues) section

![func start](docs/api_start.png)

Alternatively, start a debugging session in `Visual Studio` or `Visual Studio Code`.

> To debug from VS Code ensure you are opening the project folder @ `./api/`

### Run from Visual Studio

From Visual studio choose the startup project as `LinkyLink`. Then from the debug start menu choose one of the following:

#### IIS Express

VS will start the Rest API from an IIS Express instance on a random port.

#### LinkLink

Will start the Rest API from a console app on port `5001` similar to calling `dotnet run` from your your terminal.

## Using the API

To test out the API locally you can use your favorite tooling or try out some of our favorites.

### Try out the API with Postman

<details>
  <summary>View Postman setup</summary>

- Start up Postman and import the `theurlist_collection.json` file that's in the `api` folder
- Next import the `theurlist_localhost_env.json` file. That includes the Localhost environment settings.
- Set your environment to `Localhost`
- Turn off Postman SSL Verification

![postman](docs/postman-disable-ssl-verification.png)

- Run `Save Bundle` to add some data to Cosmos DB. The structure (collection, documents, etc.) in the database will be created for you if it does not exist yet. Next run `Get bundle for vanity url` to retrieve the entry you just created.

> Note: Change the vanityUrl value to a unique name before running 'Save Bundle' and use the same vanityUrl name to run `Get bundle for vanity url`.

![postman](docs/postman_localhost.png)

If everything was setup correctly, you should see a response that resembles the following.

![postman](docs/postman_response.png)

</details>

### Try out the API using the REST Client extension

<details>
  <summary>View REST Client setup</summary>

- Open the `validate-api.http` file located in the `openhack-production/api` directory
- Follow the instructions in the `Run the ASP.Net Core Web API backend` section of this README to start the backend
- Select `Send Request` for any of the endpoints

![REST Client](docs/rest_client.png)

</details>

### Try out the API using the Swagger UI

<details>
<summary>View Swagger setup</summary>

### Swagger API Documentation

- The API uses [Swagger](https://swagger.io/) for API Documentation. You can view the swagger documentation by navigating to: `https://localhost:<port>/swagger`

![swagger](docs/swagger.png)

#### Misc. Notes

- [Swagger](https://swagger.io/) XML Comments have been enabled to provide better API Documentation. This means that warnings will be generated for public undocumented public types and members. By default, this project disables warnings. Documentation to enable warnings can be found [here](https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-3.1&tabs=visual-studio-code)

</details>

## Testing

### Running unit tests

Unit tests validate the individual components of the API.

```bash
dotnet test api/tests/LinkyLink.Tests/LinkyLink.Tests.csproj
```

### Running Integration tests

Integration tests validate the APIs against a running system.

The integration tests require a special configuration that uses a [resource owner password credentials (ROPC) flow in Azure AD B2C](https://docs.microsoft.com/en-us/azure/active-directory-b2c/configure-ropc?tabs=applications). The ROPC client application only works with local user accounts. Social logins are not supported.

Before running integration tests you must update the settings within the `appsettings.json` of the integration test project or set environment variables for the following:

### Set Environment Variables

```bash
# The host address of the deployed environment (dev/staging/prod) to test
export INTTEST_BaseAddress=
# The client ID of your test application within Azure B2C
export INTTEST_AzureAdB2C__ClientId=
# The Azure B2C authority endpoint
export INTTEST_AzureAdB2C__Authority=
# The scope to set on your access token
export INTTEST_AzureAdB2C__Scope=
# The username of your test user account
export INTTEST_AzureAdB2C__Username=
# The password of your test user account
export INTTEST_AzureAdB2C__Password=
```

### Run tests with .Net CLI

```bash
dotnet test api/tests/LinkyLink.Integration.Tests/LinkyLink.Integration.Tests.csproj
```

## Docker local development

This is an alternative local development option. The container sets the ASPNETCORE environment to `Development`, so make sure you have a `appsettings.Development.json` created and configured before building the container image.

```bash
docker build -t linkylink .
docker run -p 5000:80 -it linkylink
curl <http://localhost:5000/api/links/postman-test>

```

### Setup certificate for HTTPS

If you want to get everything working with HTTPS, you can follow the instructions in this [doc](https://docs.microsoft.com/en-us/aspnet/core/security/docker-https?view=aspnetcore-3.1). In essence, you need to mount a drive containing your development certificate into the container.

## Troubleshooting local development certificate issues

By default the application runs under HTTPS. If you have not previously trusted a .NET Core localhost development certificate you will need to generate and trust your a local development certificate otherwise you will receive warnings from your browser.

For detailed instructions you can review the [Troubleshooting Guide](https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-2.2&tabs=visual-studio#trust-the-aspnet-core-https-development-certificate-on-windows-and-macos) or follow the steps below.

### Windows & MacOS

In Windows and MacOS the process is straight forward and you can run a single command.

Generates and trust the self signed local development certificate

```bash
# Generate and trust development certificate
dotnet dev-certs https --trust
```

### Windows with WSL

If you are running the Windows Subsystem for Linux (WSL) you have a little more work to do:

#### Export existing certificate

Generate and export the certificate created within WSL.

```bash
# Run from your Windows HOST OS setting your own password for the certificate
dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\aspnet.pfx -p <PASSWORD>
```

#### Install Certificate into your Trust Root Certificates Store

Install the certificate with the command below. When prompted enter the password you used in the previous step

```bash
# Run from your Windows HOST OS in an elevated command prompt
certutil -importPFX "Root" "%USERPROFILE%\.aspnet\https\aspnet.pfx"
```

> You will be required to run this command from an elevated command prompt

#### Set environment variables for .NET Core runtime

Tell the .NET Core runtime what certificate to use. User the same password you created in the previous steps and point to the location on your windows host where the certificate was exported to.

```bash
# Run from your WSL Terminal
export ASPNETCORE_Kestrel__Certificates__Default__Password=<PASSWORD>
export ASPNETCORE_Kestrel__Certificates__Default__Path=/mnt/c/Users/<USERNAME>/.aspnet/https/aspnet.pfx
```

> These environment variables will only be available in your current terminal session.  
> To set them globally add them to your WSL `~/.bashrc` file
> You may need to close all tabs and restart your browser for the certificate changes to take effect
