export default {
  FRONTEND: process.env.VUE_APP_FRONTEND,
  BACKEND: process.env.VUE_APP_BACKEND,
  AUTH_URL: function(provider: string) {
    return `${
      process.env.VUE_APP_BACKEND
    }/.auth/login/${provider}?post_login_redirect_url=${
      process.env.VUE_APP_FRONTEND
    }`;
  },
  FUNCTION_KEY: process.env.VUE_APP_FUNCTION_KEY
};
