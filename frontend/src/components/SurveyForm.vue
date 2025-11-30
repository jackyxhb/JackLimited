<template>
  <div class="survey-form">
    <h2>Submit Your Feedback</h2>
    <form @submit.prevent="handleSubmit" novalidate>
      <div class="form-group" :class="{ 'has-error': validationErrors.likelihoodToRecommend }">
        <label for="rating">How likely are you to recommend us? (0-10) *</label>
        <input
          id="rating"
          v-model.number="form.likelihoodToRecommend"
          type="number"
          min="0"
          max="10"
          step="1"
          :class="{ 'error': validationErrors.likelihoodToRecommend }"
          @input="validateField('likelihoodToRecommend')"
          @blur="validateField('likelihoodToRecommend')"
          required
        />
        <span v-if="validationErrors.likelihoodToRecommend" class="field-error">
          {{ validationErrors.likelihoodToRecommend }}
        </span>
      </div>

      <div class="form-group" :class="{ 'has-error': validationErrors.comments }">
        <label for="comments">Comments (optional)</label>
        <textarea
          id="comments"
          v-model="form.comments"
          :class="{ 'error': validationErrors.comments }"
          @input="validateField('comments')"
          @blur="validateField('comments')"
          maxlength="1000"
          rows="4"
          placeholder="Share your thoughts..."
        ></textarea>
        <div class="field-info">
          <span v-if="validationErrors.comments" class="field-error">
            {{ validationErrors.comments }}
          </span>
          <span v-else class="character-count">
            {{ form.comments?.length || 0 }}/1000 characters
          </span>
        </div>
      </div>

      <div class="form-group" :class="{ 'has-error': validationErrors.email }">
        <label for="email">Email (optional)</label>
        <input
          id="email"
          v-model="form.email"
          type="email"
          :class="{ 'error': validationErrors.email }"
          @input="validateField('email')"
          @blur="validateField('email')"
          placeholder="your.email@example.com"
        />
        <span v-if="validationErrors.email" class="field-error">
          {{ validationErrors.email }}
        </span>
      </div>

      <div class="form-actions">
        <button
          type="submit"
          :disabled="isSubmitting || !isFormValid"
          class="submit-button"
        >
          <span v-if="isSubmitting" class="loading-spinner"></span>
          {{ isSubmitting ? 'Submitting...' : 'Submit Feedback' }}
        </button>

        <button
          type="button"
          @click="resetForm"
          :disabled="isSubmitting"
          class="reset-button"
        >
          Reset Form
        </button>
      </div>

      <!-- Success Message -->
      <div v-if="submitSuccess" class="success-message">
        <div class="success-icon">✓</div>
        <div class="success-content">
          <h3>Thank you for your feedback!</h3>
          <p>Your response has been submitted successfully.</p>
        </div>
      </div>

      <!-- Error Message -->
      <div v-if="submitError" class="error-message">
        <div class="error-icon">⚠</div>
        <div class="error-content">
          <h3>{{ submitError.title }}</h3>
          <p>{{ submitError.message }}</p>
          <button
            v-if="submitError.canRetry"
            @click="retrySubmit"
            :disabled="isSubmitting"
            class="retry-button"
          >
            Try Again
          </button>
        </div>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, watch } from 'vue'
import { useSurveyStore } from '@/stores/survey'
import type { SurveyRequest } from '@/types/survey'

interface ValidationErrors {
  likelihoodToRecommend?: string
  comments?: string
  email?: string
}

interface SubmitError {
  title: string
  message: string
  canRetry: boolean
}

const surveyStore = useSurveyStore()
const { submitSurvey } = surveyStore

const form = reactive<SurveyRequest>({
  likelihoodToRecommend: 5,
  comments: '',
  email: '',
})

const validationErrors = reactive<ValidationErrors>({})
const isSubmitting = ref(false)
const submitSuccess = ref(false)
const submitError = ref<SubmitError | null>(null)
const retryCount = ref(0)
const maxRetries = 3

// Validation rules
const validateField = (field: keyof SurveyRequest) => {
  const value = form[field]

  switch (field) {
    case 'likelihoodToRecommend':
      if (value === null || value === undefined || value === '') {
        validationErrors.likelihoodToRecommend = 'Rating is required'
      } else if (typeof value !== 'number' || isNaN(value)) {
        validationErrors.likelihoodToRecommend = 'Please enter a valid number'
      } else if (value < 0 || value > 10) {
        validationErrors.likelihoodToRecommend = 'Rating must be between 0 and 10'
      } else if (!Number.isInteger(value)) {
        validationErrors.likelihoodToRecommend = 'Rating must be a whole number'
      } else {
        delete validationErrors.likelihoodToRecommend
      }
      break

    case 'comments':
      if (value && typeof value === 'string' && value.length > 1000) {
        validationErrors.comments = 'Comments must not exceed 1000 characters'
      } else {
        delete validationErrors.comments
      }
      break

    case 'email':
      if (value && typeof value === 'string') {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
        if (!emailRegex.test(value.trim())) {
          validationErrors.email = 'Please enter a valid email address'
        } else {
          delete validationErrors.email
        }
      } else {
        delete validationErrors.email
      }
      break
  }
}

// Input sanitization
const sanitizeInput = (input: string): string => {
  return input
    .trim()
    .replace(/[<>]/g, '') // Remove potential HTML tags
    .replace(/\s+/g, ' ') // Normalize whitespace
    .slice(0, 1000) // Limit length
}

// Form validation
const isFormValid = computed(() => {
  return !validationErrors.likelihoodToRecommend &&
         !validationErrors.comments &&
         !validationErrors.email &&
         form.likelihoodToRecommend >= 0 &&
         form.likelihoodToRecommend <= 10
})

// Validate all fields
const validateForm = (): boolean => {
  validateField('likelihoodToRecommend')
  validateField('comments')
  validateField('email')
  return isFormValid.value
}

// Submit with retry logic
const submitWithRetry = async (attempt: number = 1): Promise<void> => {
  try {
    // Sanitize inputs
    const sanitizedForm: SurveyRequest = {
      likelihoodToRecommend: form.likelihoodToRecommend,
      comments: form.comments ? sanitizeInput(form.comments) : undefined,
      email: form.email ? sanitizeInput(form.email) : undefined,
    }

    await submitSurvey(sanitizedForm)

    // Success - reset form
    resetForm()
    submitSuccess.value = true
    submitError.value = null
    retryCount.value = 0

    // Auto-hide success message after 5 seconds
    setTimeout(() => {
      submitSuccess.value = false
    }, 5000)

  } catch (error) {
    console.error(`Submission attempt ${attempt} failed:`, error)

    // Determine error type and create user-friendly message
    let errorInfo: SubmitError

    if (error instanceof TypeError && error.message.includes('fetch')) {
      errorInfo = {
        title: 'Connection Error',
        message: 'Unable to connect to the server. Please check your internet connection.',
        canRetry: true
      }
    } else if (error instanceof Error && error.message.includes('400')) {
      errorInfo = {
        title: 'Invalid Data',
        message: 'Please check your input and try again.',
        canRetry: false
      }
    } else if (error instanceof Error && error.message.includes('500')) {
      errorInfo = {
        title: 'Server Error',
        message: 'Our servers are experiencing issues. Please try again later.',
        canRetry: true
      }
    } else {
      errorInfo = {
        title: 'Submission Failed',
        message: 'An unexpected error occurred. Please try again.',
        canRetry: true
      }
    }

    submitError.value = errorInfo

    // Auto-retry for certain errors
    if (errorInfo.canRetry && attempt < maxRetries) {
      retryCount.value = attempt
      setTimeout(() => {
        submitWithRetry(attempt + 1)
      }, Math.pow(2, attempt) * 1000) // Exponential backoff
    }
  }
}

const handleSubmit = async () => {
  if (!validateForm()) {
    return
  }

  isSubmitting.value = true
  submitSuccess.value = false
  submitError.value = null

  try {
    await submitWithRetry()
  } finally {
    isSubmitting.value = false
  }
}

const retrySubmit = async () => {
  await handleSubmit()
}

const resetForm = () => {
  form.likelihoodToRecommend = 5
  form.comments = ''
  form.email = ''
  Object.keys(validationErrors).forEach(key => {
    delete validationErrors[key as keyof ValidationErrors]
  })
  submitSuccess.value = false
  submitError.value = null
  retryCount.value = 0
}

// Watch for form changes to clear success/error states
watch(() => [form.likelihoodToRecommend, form.comments, form.email], () => {
  if (submitSuccess.value || submitError.value) {
    submitSuccess.value = false
    submitError.value = null
  }
})
</script>

<style scoped>
.survey-form {
  max-width: 500px;
  margin: 0 auto;
  padding: 2rem;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

.form-group {
  margin-bottom: 1.5rem;
  transition: all 0.3s ease;
}

.form-group.has-error {
  margin-bottom: 2rem;
}

label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 600;
  color: #333;
}

input, textarea {
  width: 100%;
  padding: 0.75rem;
  border: 2px solid #e1e5e9;
  border-radius: 6px;
  font-size: 1rem;
  transition: all 0.3s ease;
  background: #fff;
}

input:focus, textarea:focus {
  outline: none;
  border-color: #007bff;
  box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.1);
}

input.error, textarea.error {
  border-color: #dc3545;
  box-shadow: 0 0 0 3px rgba(220, 53, 69, 0.1);
}

.field-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 0.25rem;
  font-size: 0.875rem;
}

.field-error {
  color: #dc3545;
  font-weight: 500;
}

.character-count {
  color: #6c757d;
}

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 2rem;
}

.submit-button, .reset-button, .retry-button {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 6px;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.submit-button {
  background: linear-gradient(135deg, #007bff, #0056b3);
  color: white;
  flex: 1;
}

.submit-button:hover:not(:disabled) {
  background: linear-gradient(135deg, #0056b3, #004085);
  transform: translateY(-1px);
}

.submit-button:disabled {
  background: #6c757d;
  cursor: not-allowed;
  transform: none;
}

.reset-button {
  background: #f8f9fa;
  color: #495057;
  border: 1px solid #dee2e6;
}

.reset-button:hover:not(:disabled) {
  background: #e9ecef;
}

.reset-button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.loading-spinner {
  width: 16px;
  height: 16px;
  border: 2px solid transparent;
  border-top: 2px solid currentColor;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.success-message, .error-message {
  margin-top: 2rem;
  padding: 1.5rem;
  border-radius: 8px;
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  animation: slideIn 0.3s ease-out;
}

.success-message {
  background: #d4edda;
  border: 1px solid #c3e6cb;
  color: #155724;
}

.error-message {
  background: #f8d7da;
  border: 1px solid #f5c6cb;
  color: #721c24;
}

.success-icon, .error-icon {
  font-size: 1.5rem;
  font-weight: bold;
  flex-shrink: 0;
}

.success-content h3, .error-content h3 {
  margin: 0 0 0.5rem 0;
  font-size: 1.1rem;
  font-weight: 600;
}

.success-content p, .error-content p {
  margin: 0;
  font-size: 0.95rem;
}

.retry-button {
  margin-top: 1rem;
  background: #dc3545;
  color: white;
  padding: 0.5rem 1rem;
  font-size: 0.9rem;
}

.retry-button:hover:not(:disabled) {
  background: #c82333;
}

@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Responsive design */
@media (max-width: 600px) {
  .survey-form {
    padding: 1rem;
    margin: 0 1rem;
  }

  .form-actions {
    flex-direction: column;
  }

  .submit-button, .reset-button {
    width: 100%;
  }
}
</style>
