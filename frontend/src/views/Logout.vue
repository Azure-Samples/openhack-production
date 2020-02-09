<template>
  <h1>Logout</h1>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

@Component({
  components: {}
})
export default class Logout extends Vue {
  async created() {
    await this.$store.dispatch("completeLogout");
    this.$router.push("/");
  }
}
</script>
