<script setup lang="ts">
import NavigationBar from '@/components/NavigationBar.vue'
import SurveyForm from '../components/SurveyForm.vue'
</script>

<template>
  <div>
    <NavigationBar />
    <main class="page-content">
    <div class="container-sm">
      <header class="page-header">
        <h1>Share Your Feedback</h1>
        <p class="subtitle">Help us improve by sharing your experience with Jack Limited</p>
      </header>

      <div class="card">
        <SurveyForm />
      </div>
    </div>
  </main>
</div>
</template>

<style scoped>
/* Survey page specific styles */
</style>
