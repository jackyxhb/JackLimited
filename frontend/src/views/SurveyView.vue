<script setup lang="ts">
import ViewContainer from '@/components/ViewContainer.vue'
import SurveyForm from '../components/SurveyForm.vue'
</script>

<template>
  <ViewContainer>
    <div class="content-wrapper">
      <header class="page-header">
        <h1>Share Your Feedback</h1>
        <p class="subtitle">Help us improve by sharing your experience with Jack Limited</p>
      </header>

      <div class="card">
        <SurveyForm />
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
  width: 100%;
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 var(--spacing-lg);
}

@media (max-width: 768px) {
  .content-wrapper {
    padding: 0 var(--spacing-md);
  }
}
</style>
