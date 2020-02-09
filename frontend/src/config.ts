export default {
  FRONTEND: process.env.VUE_APP_FRONTEND,
  BACKEND: process.env.VUE_APP_BACKEND,
  openId: {
    tenantId: process.env.VUE_APP_OIDC_TENANT_ID,
    clientId: process.env.VUE_APP_OIDC_CLIENT_ID,
    domain: process.env.VUE_APP_OIDC_DOMAIN,
    scope: process.env.VUE_APP_OIDC_SCOPE,
    policy: process.env.VUE_APP_OIDC_POLICY
  },
  appInsightsInstrumentationKey: process.env.VUE_APP_APPINSIGHTS_INSTRUMENTATIONKEY
};
