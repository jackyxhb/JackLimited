<template>
  <div class="survey-form">
    <h2>Submit Your Feedback</h2>
    <form @submit.prevent="submitForm">
      <div class="form-group">
        <label for="rating">How likely are you to recommend us? (0-10)</label>
        <input
          id="rating"
          v-model.number="form.likelihoodToRecommend"
          type="number"
          min="0"
          max="10"
          required
        />
      </div>

      <div class="form-group">
        <label for="comments">Comments (optional)</label>
        <textarea
          id="comments"
          v-model="form.comments"
          maxlength="1000"
          rows="4"
        ></textarea>
      </div>

      <div class="form-group">
        <label for="email">Email (optional)</label>
        <input
          id="email"
          v-model="form.email"
          type="email"
        />
      </div>

      <button type="submit" :disabled="isLoading">
        {{ isLoading ? 'Submitting...' : 'Submit Feedback' }}
      </button>

      <p v-if="error" class="error">{{ error }}</p>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useSurveyStore } from '@/stores/survey'
import type { SurveyRequest } from '@/types/survey'

const surveyStore = useSurveyStore()
const { submitSurvey, isLoading, error } = surveyStore

const form = ref<SurveyRequest>({
  likelihoodToRecommend: 5,
  comments: '',
  email: '',
})

const submitForm = async () => {
  await submitSurvey(form.value)
  if (!error.value) {
    // Reset form on success
    form.value = {
      likelihoodToRecommend: 5,
      comments: '',
      email: '',
    }
  }
}
</script>

<style scoped>
.survey-form {
  max-width: 500px;
  margin: 0 auto;
  padding: 2rem;
}

.form-group {
  margin-bottom: 1rem;
}

label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: bold;
}

input, textarea {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid #ccc;
  border-radius: 4px;
}

button {
  background-color: #007bff;
  color: white;
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 1rem;
}

button:disabled {
  background-color: #ccc;
  cursor: not-allowed;
}

.error {
  color: red;
  margin-top: 1rem;
}
</style>
