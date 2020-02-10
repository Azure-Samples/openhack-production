import Oidc from "oidc-client";
import config from "../config";
import axios from "axios";

const userManager: Oidc.UserManager = new Oidc.UserManager({
    automaticSilentRenew: true,
    loadUserInfo: false,
    authority: `${config.BACKEND}/api/openid`,
    client_id: config.openId.clientId,
    scope: config.openId.scope,
    redirect_uri: `${window.location.origin}/s/auth/login`,
    post_logout_redirect_uri: `${window.location.origin}/s/auth/logout`,
    response_type: "id_token token"
});

Oidc.Log.logger = console;
Oidc.Log.level = Oidc.Log.INFO;

function onUserLoaded(user: Oidc.User) {
    axios.defaults.headers.common["Authorization"] = `bearer ${user.access_token}`;
}

userManager.events.addAccessTokenExpired(() => {
    userManager.signoutRedirect();
});

userManager.events.addSilentRenewError(async () => {
    axios.defaults.headers.common["Authorization"] = "";
    await userManager.signinRedirect();
});

userManager.events.addUserLoaded(onUserLoaded);

userManager.events.addUserSignedOut(() => {
    axios.defaults.headers.common["Authorization"] = "";
});

userManager.getUser().then(user => {
    if (user) {
        onUserLoaded(user);
    }
})

const AuthService = {
    async login() {
        return await userManager.signinRedirect();
    },

    async completeLogin() {
        return await userManager.signinRedirectCallback();
    },

    async logout() {
        return await userManager.signoutRedirect();
    },

    async completeLogout() {
        return await userManager.signoutRedirectCallback();
    },

    async getUser() {
        return await userManager.getUser();
    }
};

export default AuthService;
