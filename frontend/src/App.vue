<template>
  <div id="app">
    <router-view />
    <ToastContainer />
  </div>
</template>

<script setup lang="ts">
import ToastContainer from '@/components/ToastContainer.vue'
</script>

<style>
#app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: var(--color-background);
  color: var(--color-text);
  font-family: var(--font-family);
  font-size: var(--font-size-base);
  line-height: var(--line-height-normal);
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}
</style>
