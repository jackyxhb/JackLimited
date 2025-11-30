<script setup lang="ts">
import ViewContainer from '@/components/ViewContainer.vue'
</script>

<template>
  <ViewContainer>
    <div class="content-wrapper">
      <header class="page-header">
        <h1>About Jack Limited</h1>
        <p class="subtitle">Learn more about our company and mission</p>
      </header>

      <div class="card">
        <h1>This is an about page</h1>
        <p>Welcome to Jack Limited. We are committed to providing excellent products and services to our customers.</p>
      </div>
    </div>
  </ViewContainer>
</template>

<style scoped>
.content-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
  max-width: 1000px;
  margin: 0 auto;
}
</style>
