# The Urlist - Frontend

## Table Of Contents

<!-- toc -->
- [UI Views](#ui-views)
- [Dependencies](#dependencies)
- [Frontend configurations](#frontend-configurations)
- [Build and run the frontend locally](#build-and-run-the-frontend-locally)
- [Debugging the application](#debugging-the-application)
- [Docker local development](#docker-local-development)
<!-- tocstop -->

## UI Views

The frontend has the following views:

### Homepage view

This is the first page you would see when visiting the frontend. You can start the process of creating a link-bundles from here.

  ![Homepage picture](docs/Images/Homepage.png)

### Edit view

This is the page where you would create your lists.

  ![Edit picture](docs/Images/Edit_page.png)

### User view

This page would list all list-bundles created for a signed-in user.

  ![Manage lists picture](docs/Images/Manage_lists_page.png)

### List view

This page is used to view the list-bundle for a given vanity URL.

  ![View list picture](docs/Images/View_page.png)

---

## Dependencies

The frontend for this project is built with the following libraries and frameworks:

- [TypeScript](https://www.typescriptlang.org/)
- [Vue.js](https://github.com/vuejs/vue) / [Vue CLI](https://github.com/vuejs/vue-cli)
- [Vuelidate](https://github.com/vuelidate/vuelidate)
- [Axios](https://github.com/axios/axios)

### Other useful tools

- [Visual Studio Code](https://code.visualstudio.com/?WT.mc_id=theurlist-github-buhollan)
- [Vetur](https://marketplace.visualstudio.com/items?itemName=octref.vetur&WT.mc_id=theurlist-github-buhollan)
- [VS Code Debugger for Chrome](https://marketplace.visualstudio.com/items?itemName=msjsdiag.debugger-for-chrome&WT.mc_id=theurlist-github-buhollan)
- [Vue VS Code Extension Pack](https://marketplace.visualstudio.com/items?itemName=sdras.vue-vscode-extensionpack&WT.mc_id=theurlist-github-buhollan)
- [Vue browser devtools](https://github.com/vuejs/vue-devtools)

## Frontend configurations

There are a few configurations needed for the frontend app to run, those are passed as [VUE environment configs](https://cli.vuejs.org/guide/mode-and-env.html)

- `VUE_APP_BACKEND`: this has the URL pointing to the backend endpoint.
- `VUE_APP_FRONTEND`: this has the URL point to the frontend endpoint.
- `VUE_APP_OIDC_CLIENT_ID`: the configured Azure B2C Client Application ID.
- `VUE_APP_OIDC_SCOPE`: the Azure B2C Client Scopes.
- `VUE_APP_APPINSIGHTS_INSTRUMENTATIONKEY`: the configured Azure Application Insights instrumentation key.

## Build and run the frontend locally

### Modify VUE environment configs

- Follow the guide on how to run the Backend locally. You can find the README [here](../api/README.md)

- Once the Backend has started, you will want to get the port the Backend uses _(local port may change depending upon the IDE being used)_.
- Follow the authentication setup guide, [here](../docs/AzureADB2C.md), to identify the needed value for the OpenID Connect (OIDC) configurations needed.
- Create a local configuration file the root of the `frontend` folder called `.env.development.local` and add the needed configs as described [above](#Frontend-configurations), something like the following:

```bash
# .env.development.local
VUE_APP_BACKEND=http://localhost:[backend_port] (default is 5000)
VUE_APP_FRONTEND=http://localhost:[frontend_port] (default is 8080)
VUE_APP_OIDC_CLIENT_ID=[Azure B2C Client Application ID]
VUE_APP_OIDC_SCOPE=[Azure B2C Client Scopes]
VUE_APP_APPINSIGHTS_INSTRUMENTATIONKEY=[Azure Application Insights Instrumentation Key]
```

### Setup NPM for the frontend

```bash
# Ensure you are in the frontend folder
cd frontend

# Install Vue CLI globally
npm install -g @vue/cli

# Install npm packages for frontend project
npm install
```

### Serve development build

```bash
# Serves a local application on port '8080'
npm run serve
```

![localhost serve](docs/Images/localhost_serve.png)

### Create production optimized build

```bash
npm run build
```

> This creates a dist folder under frontend

### Lints and fixes files

```bash
npm run lint
```

## Debugging the application

- Follow the instructions in the [Build and run the frontend locally](##-build-and-run-the-frontend-locally) to start application
- In Chrome, open the Chrome Developer Tools
- Select `Source` then expand `webpack` then expand the `.` folder then expand `src` and find the TypeScript file that you would like to debug and `double-click` the line in the TypeScript file that you are interested in. See the screenshot below as an example:

![localhost serve](docs/Images/localhost_debugging.png)

## Docker local development

By default the front-end will be running in `development mode`, consequently make sure you setup the environment files as described [here](###-Modify-Vue-Environment-Configs).

```bash
docker build -t linkylink-fe .
docker run -it --rm -p 8080:8080 --name frontend linkylink-fe
```

Alternatively, you can pass the environment variables to override any settings from the `.env.[mode]` files

```bash
docker build -t linkylink-fe .
docker run -it --rm -p 8080:8080 --name frontend \
-e VUE_APP_BACKEND="http://localhost:5000" \
-e VUE_APP_OIDC_AUTHORITY="https://<b2c login subdomain>.b2clogin.com/testprodoh.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1_SignUp_SignIn" \
-e VUE_APP_OIDC_CLIENT_ID="<client id>" \
-e VUE_APP_OIDC_SCOPE="openid https://<b2c login subdomain>.onmicrosoft.com/api/UrlBundle.ReadWrite" \
-e "VUE_APP_APPINSIGHTS_INSTRUMENTATIONKEY=[Azure Application Insights Instrumentation Key]" \
linkylink-fe
```
