<template>
  <div class="survey-form">
    <div class="form-header">
      <SendIcon class="form-icon" />
      <h2>Submit Your Feedback</h2>
      <p class="form-description">Help us improve by sharing your experience with Jack Limited</p>
    </div>

    <form @submit.prevent="handleSubmit" novalidate class="form-content">
      <div class="form-group" :class="{ 'has-error': validationErrors.likelihoodToRecommend }">
        <label for="rating" class="form-label">
          <StarIcon class="label-icon" />
          How likely are you to recommend us? (0-10) *
        </label>
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
          class="form-input"
          placeholder="Enter a number 0-10"
        />
        <span v-if="validationErrors.likelihoodToRecommend" class="field-error">
          <AlertCircleIcon class="error-icon" />
          {{ validationErrors.likelihoodToRecommend }}
        </span>
      </div>

      <div class="form-group" :class="{ 'has-error': validationErrors.comments }">
        <label for="comments" class="form-label">
          <MessageSquareIcon class="label-icon" />
          Comments (optional)
        </label>
        <textarea
          id="comments"
          v-model="form.comments"
          :class="{ 'error': validationErrors.comments }"
          @input="validateField('comments')"
          @blur="validateField('comments')"
          maxlength="1000"
          rows="4"
          placeholder="Share your thoughts..."
          class="form-textarea"
        ></textarea>
        <div class="field-info">
          <span v-if="validationErrors.comments" class="field-error">
            <AlertCircleIcon class="error-icon" />
            {{ validationErrors.comments }}
          </span>
          <span v-else class="character-count">
            {{ form.comments?.length || 0 }}/1000 characters
          </span>
        </div>
      </div>

      <div class="form-group" :class="{ 'has-error': validationErrors.email }">
        <label for="email" class="form-label">
          <UsersIcon class="label-icon" />
          Email (optional)
        </label>
        <input
          id="email"
          v-model="form.email"
          type="email"
          :class="{ 'error': validationErrors.email }"
          @input="validateField('email')"
          @blur="validateField('email')"
          placeholder="your.email@example.com"
          class="form-input"
        />
        <span v-if="validationErrors.email" class="field-error">
          <AlertCircleIcon class="error-icon" />
          {{ validationErrors.email }}
        </span>
      </div>

      <div class="form-actions">
        <button
          type="submit"
          :disabled="isSubmitting || !isFormValid"
          class="btn btn-primary btn-lg"
        >
          <span v-if="isSubmitting" class="loading-spinner"></span>
          <SendIcon v-else class="icon" />
          {{ isSubmitting ? 'Submitting...' : 'Submit Feedback' }}
        </button>

        <button
          type="button"
          @click="resetForm"
          :disabled="isSubmitting"
          class="btn btn-secondary"
        >
          <RotateCcwIcon class="icon" />
          Reset Form
        </button>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive } from 'vue'
import { useSurveyStore } from '@/stores/survey'
import { useToastStore } from '@/stores/toast'
import type { SurveyRequest } from '@/types/survey'
import { SendIcon, StarIcon, MessageSquareIcon, UsersIcon, AlertCircleIcon, RotateCcwIcon } from 'lucide-vue-next'

interface ValidationErrors {
  likelihoodToRecommend?: string
  comments?: string
  email?: string
}

const surveyStore = useSurveyStore()
const toastStore = useToastStore()
const { submitSurvey } = surveyStore

const form = reactive<SurveyRequest>({
  likelihoodToRecommend: 5,
  comments: '',
  email: '',
})

const validationErrors = reactive<ValidationErrors>({})
const isSubmitting = ref(false)
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

    // Success - reset form and show toast
    resetForm()
    toastStore.success(
      'Feedback Submitted!',
      'Thank you for your valuable feedback.',
      3000
    )

  } catch (error) {
    console.error(`Submission attempt ${attempt} failed:`, error)

    // Determine error type and create user-friendly message
    let errorTitle: string
    let errorMessage: string

    if (error instanceof TypeError && error.message.includes('fetch')) {
      errorTitle = 'Connection Error'
      errorMessage = 'Unable to connect to the server. Please check your internet connection.'
    } else if (error instanceof Error && error.message.includes('400')) {
      errorTitle = 'Invalid Data'
      errorMessage = 'Please check your input and try again.'
    } else if (error instanceof Error && error.message.includes('500')) {
      errorTitle = 'Server Error'
      errorMessage = 'Our servers are experiencing issues. Please try again later.'
    } else {
      errorTitle = 'Submission Failed'
      errorMessage = 'An unexpected error occurred. Please try again.'
    }

    toastStore.error(errorTitle, errorMessage, 5000)

    // Auto-retry for certain errors
    if ((error instanceof TypeError && error.message.includes('fetch')) ||
        (error instanceof Error && error.message.includes('500'))) {
      if (attempt < maxRetries) {
        retryCount.value = attempt
        setTimeout(() => {
          submitWithRetry(attempt + 1)
        }, Math.pow(2, attempt) * 1000) // Exponential backoff
      }
    }
  }
}

const handleSubmit = async () => {
  if (!validateForm()) {
    return
  }

  isSubmitting.value = true

  try {
    await submitWithRetry()
  } finally {
    isSubmitting.value = false
  }
}

const resetForm = () => {
  form.likelihoodToRecommend = 5
  form.comments = ''
  form.email = ''
  Object.keys(validationErrors).forEach(key => {
    delete validationErrors[key as keyof ValidationErrors]
  })
  retryCount.value = 0
}
</script>

<style scoped>
.survey-form {
  max-width: 600px;
  margin: 0 auto;
  background: var(--color-surface);
  border-radius: var(--border-radius-lg);
  box-shadow: 0 8px 32px var(--color-shadow);
  overflow: hidden;
  border: 1px solid var(--color-border);
}

.form-header {
  background: var(--gradient-primary);
  color: var(--color-text-inverse);
  padding: var(--spacing-2xl);
  text-align: center;
  position: relative;
}

.form-header::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
}

.form-icon {
  width: 48px;
  height: 48px;
  margin-bottom: var(--spacing-md);
  color: var(--color-text-inverse);
}

.form-header h2 {
  margin: 0 0 var(--spacing-sm) 0;
  font-size: var(--font-size-2xl);
  font-weight: var(--font-weight-bold);
  position: relative;
  z-index: 1;
}

.form-description {
  margin: 0;
  font-size: var(--font-size-base);
  opacity: 0.9;
  position: relative;
  z-index: 1;
}

.form-content {
  padding: var(--spacing-2xl);
}

.form-group {
  margin-bottom: var(--spacing-xl);
  transition: all var(--transition-fast);
}

.form-group.has-error {
  margin-bottom: var(--spacing-2xl);
}

.form-label {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  margin-bottom: var(--spacing-md);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text);
  font-size: var(--font-size-base);
}

.label-icon {
  width: 20px;
  height: 20px;
  color: var(--color-primary);
}

.form-input, .form-textarea {
  width: 100%;
  padding: var(--spacing-md) var(--spacing-lg);
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius);
  font-size: var(--font-size-base);
  transition: all var(--transition-fast);
  background: var(--color-background);
  color: var(--color-text);
  font-family: inherit;
}

.form-input:focus, .form-textarea:focus {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
}

.form-input.error, .form-textarea.error {
  border-color: var(--color-error);
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1);
}

.form-textarea {
  resize: vertical;
  min-height: 120px;
  line-height: var(--line-height-relaxed);
}

.field-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: var(--spacing-xs);
  font-size: var(--font-size-sm);
}

.field-error {
  color: var(--color-error);
  font-weight: var(--font-weight-medium);
  display: flex;
  align-items: center;
  gap: var(--spacing-xs);
}

.error-icon {
  width: 16px;
  height: 16px;
}

.character-count {
  color: var(--color-text-muted);
}

.form-actions {
  display: flex;
  gap: var(--spacing-lg);
  margin-top: var(--spacing-2xl);
  justify-content: center;
  flex-wrap: wrap;
}

.loading-spinner {
  width: 20px;
  height: 20px;
  border: 2px solid currentColor;
  border-top: 2px solid transparent;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* Responsive design */
@media (max-width: 600px) {
  .survey-form {
    margin: 0 var(--spacing-lg);
    border-radius: var(--border-radius);
  }

  .form-header {
    padding: var(--spacing-xl);
  }

  .form-content {
    padding: var(--spacing-xl);
  }

  .form-actions {
    flex-direction: column;
  }

  .form-icon {
    width: 40px;
    height: 40px;
  }

  .form-header h2 {
    font-size: var(--font-size-xl);
  }
}
</style>
