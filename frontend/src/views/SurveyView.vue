<script setup lang="ts">
import SurveyForm from '../components/SurveyForm.vue'
</script>

<template>
  <main class="survey-page">
    <div class="container">
      <header class="page-header">
        <h1>Share Your Feedback</h1>
        <p class="subtitle">Help us improve by sharing your experience with Jack Limited</p>
      </header>

      <div class="survey-content">
        <SurveyForm />
      </div>
    </div>
  </main>
</template>

<style scoped>
.survey-page {
  min-height: 100vh;
  background: var(--color-background);
  padding: 2rem 0;
}

.container {
  max-width: 800px;
  margin: 0 auto;
  padding: 0 1rem;
}

.page-header {
  text-align: center;
  margin-bottom: 3rem;
}

.page-header h1 {
  color: var(--color-heading);
  font-size: 2.5rem;
  font-weight: 700;
  margin-bottom: 0.5rem;
}

.subtitle {
  color: var(--color-text);
  font-size: 1.1rem;
  margin: 0;
}

.survey-content {
  background: var(--color-card-background);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  padding: 2rem;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
}

/* Responsive design */
@media (max-width: 768px) {
  .survey-page {
    padding: 1rem 0;
  }

  .container {
    padding: 0 1rem;
  }

  .page-header {
    margin-bottom: 2rem;
  }

  .page-header h1 {
    font-size: 2rem;
  }

  .survey-content {
    padding: 1.5rem;
    border-radius: 8px;
  }
}

@media (max-width: 480px) {
  .page-header h1 {
    font-size: 1.75rem;
  }

  .subtitle {
    font-size: 1rem;
  }

  .survey-content {
    padding: 1rem;
  }
}
</style>
