export default {
  FRONTEND: process.env.VUE_APP_FRONTEND,
  BACKEND: process.env.VUE_APP_BACKEND,
  openId: {
    clientId: process.env.VUE_APP_OIDC_CLIENT_ID,
    scope: process.env.VUE_APP_OIDC_SCOPE
  },
  appInsightsInstrumentationKey: process.env.VUE_APP_APPINSIGHTS_INSTRUMENTATIONKEY
};
