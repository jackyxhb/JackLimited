<template>
  <div>
    <NavigationBar />
    <main class="page-container">
      <div class="container">
        <slot />
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import NavigationBar from '@/components/NavigationBar.vue'
</script>

<style scoped>
/* ViewContainer specific styles can be added here if needed */
</style>
