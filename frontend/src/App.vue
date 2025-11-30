<template>
  <div id="app">
    <NavigationBar />
    <router-view />
    <ToastContainer />
  </div>
</template>

<script setup lang="ts">
import NavigationBar from '@/components/NavigationBar.vue'
import ToastContainer from '@/components/ToastContainer.vue'
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
  margin: 0;
  padding: 0;
  min-height: 100vh;
  background: var(--color-background);
}
</style>
