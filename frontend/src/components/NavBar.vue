<template>
  <header id="navbar" class="header">
    <nav class="navbar container" role="navigation" aria-label="main navigation">
      <div class="navbar-brand">
        <a class="navbar-item" href="/">
          <img width="100" height="60" src="@/assets/logo-beta.svg" alt="urlist logo" />
        </a>
        <a
          id="hamburger"
          role="button"
          class="beta-bump navbar-burger burger"
          :class="{ 'is-active': showMenu }"
          aria-label="menu"
          aria-expanded="false"
          data-target="navbarBasicExample"
          @click="showMenu = !showMenu"
        >
          <img src="@/assets/burger.svg" alt="toggle menu" width="60" height="60" />
        </a>
      </div>
      <div class="beta-bump navbar-menu" :class="{ 'is-active': showMenu }">
        <div class="navbar-start">
          <a class="navbar-item" @click.prevent="newList()">
            <span class="icon is-large navbar-icon">
              <i class="fas fa-lg fa-plus-circle"></i>
            </span>
            New
          </a>
          <router-link v-if="currentUser.loggedIn" class="navbar-item" to="/s/user">
            <span class="icon is-large navbar-icon">
              <i class="fas fa-lg fa-user-circle"></i>
            </span>
            My Lists
          </router-link>
          <a href="https://aka.ms/theurlist" class="navbar-item" target="_new">
            <span class="icon is-large navbar-icon">
              <i class="fas fa-lg fa-question-circle"></i>
            </span>
            About
          </a>
        </div>

        <div class="navbar-end">
          <div class="navbar-item has-dropdown is-hoverable" v-if="currentUser.loggedIn">
            <a class="navbar-link">
              <div class="columns is-gapless is-mobile">
                <div class="column is-narrow">
                  <figure id="profileImage" class="image">
                    <img class="is-rounded" :src="currentUser.profileImage" alt />
                  </figure>
                </div>
                <div class="column">
                  <span>{{ currentUser.name }}</span>
                </div>
              </div>
            </a>
            <div class="navbar-dropdown">
              <a class="navbar-item" @click.prevent="logout()">
                <span class="icon is-medium navbar-icon">
                  <i class="fas fa-sign-out-alt"></i>
                </span>
                Log Out
              </a>
            </div>
          </div>
          <a v-else class="navbar-item" @click.prevent="login()">
            <span class="icon is-large navbar-icon">
              <i class="fas fa-lg fa-sign-in-alt"></i>
            </span>
            Login
          </a>
        </div>
      </div>
    </nav>
  </header>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";
import ProgressBar from "@/components/ProgressBar.vue";
import UserMenu from "@/components/UserMenu.vue";
import { UserManager } from "oidc-client";
import User from "@/models/User";
import config from "@/config";

@Component({
  components: {
    ProgressBar,
    UserMenu
  }
})
export default class extends Vue {
  showMenu: boolean = false;
  showLoginModal: boolean = false;
  userManager: UserManager;

  constructor() {
    super();
    this.userManager = new UserManager({
      loadUserInfo: false,
      authority: config.openId.authority,
      client_id: config.openId.clientId,
      scope: config.openId.scope,
      prompt: "login",
      redirect_uri: `${window.location.origin}/s/auth/openid`,
      post_logout_redirect_uri: `${window.location.origin}/s/auth/openid`,
      response_type: "id_token token"
    });
  }

  get currentUser(): User {
    return this.$store.getters.currentUser;
  }

  newList() {
    this.$store.dispatch("resetCurrentList");
    this.$router.push("/s/edit");
  }

  async login() {
    await this.userManager.signinRedirect();
  }

  async logout() {
    await this.userManager.signoutRedirect();
  }

  async created() {
    // check to see if the user is logged in
    try {
      const user = this.$store.dispatch("getUser");
    } catch (err) {
      // TODO
    }
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped lang="scss">
.beta-bump {
  margin-top: 20px;
}

#navbar {
  top: 0;
  width: 100%;
  background: #fff;
  box-shadow: 0 5px 40px 1px #e8e8e8;
  padding-bottom: 20px;
}

a.navbar-item {
  color: #71777e;
  img {
    max-height: inherit;
  }
  &:hover,
  &:focus,
  &:focus-within {
    background-color: inherit;
    color: inherit;
  }
}

.navbar-menu {
  margin-left: 20px;
  border: none;
  box-shadow: none;
}

#profileImage {
  margin-left: 5px;
  margin-right: 10px;
  > img {
    width: 28px;
    height: 28px;
  }
}
</style>
