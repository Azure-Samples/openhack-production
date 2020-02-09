import Oidc from "oidc-client";
import config from "../config";
import axios from "axios";

const userManager: Oidc.UserManager = new Oidc.UserManager({
    automaticSilentRenew: true,
    loadUserInfo: false,
    authority: `https://${config.openId.domain}.b2clogin.com/${config.openId.domain}.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=${config.openId.policy}`,
    client_id: config.openId.clientId,
    scope: config.openId.scope,
    redirect_uri: `${window.location.origin}/s/auth/login`,
    post_logout_redirect_uri: `${window.location.origin}/s/auth/logout`,
    response_type: "id_token token",
    metadata: {
        issuer: `https://${config.openId.domain}.b2clogin.com/${config.openId.tenantId}/v2.0/`,
        authorization_endpoint: `https://${config.openId.domain}.b2clogin.com/${config.openId.domain}.onmicrosoft.com/oauth2/v2.0/authorize?p=${config.openId.policy}`,
        token_endpoint: `https://${config.openId.domain}.b2clogin.com/${config.openId.domain}.onmicrosoft.com/oauth2/v2.0/token?p=${config.openId.policy}`,
        end_session_endpoint: `https://${config.openId.domain}.b2clogin.com/${config.openId.domain}.onmicrosoft.com/oauth2/v2.0/logout?p=${config.openId.policy}`,
        jwks_uri: `https://${config.openId.domain}.b2clogin.com/${config.openId.domain}.onmicrosoft.com/discovery/v2.0/keys?p=${config.openId.policy}`
    },
    signingKeys: [
        {
            "kid": "X5eXk4xyojNFum1kl2Ytv8dlNP4-c57dO6QGTVBwaNk",
            "nbf": 1493763266,
            "use": "sig",
            "kty": "RSA",
            "e": "AQAB",
            "n": "tVKUtcx_n9rt5afY_2WFNvU6PlFMggCatsZ3l4RjKxH0jgdLq6CScb0P3ZGXYbPzXvmmLiWZizpb-h0qup5jznOvOr-Dhw9908584BSgC83YacjWNqEK3urxhyE2jWjwRm2N95WGgb5mzE5XmZIvkvyXnn7X8dvgFPF5QwIngGsDG8LyHuJWlaDhr_EPLMW4wHvH0zZCuRMARIJmmqiMy3VD4ftq4nS5s8vJL0pVSrkuNojtokp84AtkADCDU_BUhrc2sIgfnvZ03koCQRoZmWiHu86SuJZYkDFstVTVSR0hiXudFlfQ2rOhPlpObmku68lXw-7V-P7jwrQRFfQVXw"
        }
    ]
});

Oidc.Log.logger = console;
Oidc.Log.level = Oidc.Log.INFO;

function onUserLoaded(user: Oidc.User) {
    axios.defaults.headers.common["Authorization"] = `bearer ${user.access_token}`;
}

userManager.events.addUserSessionChanged(() => {
    debugger;
    console.log("session changed");
})

userManager.events.addAccessTokenExpired(() => {
    debugger;
    userManager.signoutRedirect();
});

userManager.events.addSilentRenewError(async () => {
    debugger;
    axios.defaults.headers.common["Authorization"] = "";
    await userManager.signinRedirect();
});

userManager.events.addUserLoaded(onUserLoaded);

userManager.events.addUserSignedOut(() => {
    axios.defaults.headers.common["Authorization"] = "";
});

userManager.events.addAccessTokenExpiring(() => {
    debugger;
    console.log("expiring");
})

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
