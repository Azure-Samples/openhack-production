<template>
  <h1>Login</h1>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

@Component({
  components: {}
})
export default class Login extends Vue {
  async created() {
    await this.$store.dispatch("completeLogin");
    this.$router.push("/");
  }
}
</script>
