<template>
  <div id="app">
    <ComparisonForm />
  </div>
</template>

<script>
import ComparisonForm from './components/ComparisonForm.vue'

export default {
  name: 'App',
  components: {
    ComparisonForm
  }
}
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  margin-top: 20px;
}
</style>
