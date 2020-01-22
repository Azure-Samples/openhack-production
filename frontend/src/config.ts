export default {
  FRONTEND: process.env.VUE_APP_FRONTEND,
  BACKEND: process.env.VUE_APP_BACKEND,
  openId: {
    authority: process.env.VUE_APP_OIDC_AUTHORITY,
    clientId: process.env.VUE_APP_OIDC_CLIENT_ID,
    scope: process.env.VUE_APP_OIDC_SCOPE
  }
};
