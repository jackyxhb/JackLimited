import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import type { SurveyRequest, NpsResponse, AverageResponse, DistributionResponse } from '@/types/survey'
import { recordFetchTelemetry } from '@/telemetry/appInsights'

interface LoadingState {
  submit: boolean
  nps: boolean
  average: boolean
  distribution: boolean
}

interface ErrorState {
  submit: string | null
  nps: string | null
  average: string | null
  distribution: string | null
}

export const useSurveyStore = defineStore('survey', () => {
  const nps = ref<number>(0)
  const average = ref<number>(0)
  const distribution = ref<Record<number, number>>({})

  const loading = ref<LoadingState>({
    submit: false,
    nps: false,
    average: false,
    distribution: false,
  })

  const errors = ref<ErrorState>({
    submit: null,
    nps: null,
    average: null,
    distribution: null,
  })

  // Retry configuration
  const maxRetries = 3
  const baseDelay = 1000 // 1 second

  // Utility function for API calls with retry logic
  const apiCallWithRetry = async <T>(
    apiCall: () => Promise<T>,
    operation: keyof LoadingState,
    maxRetriesCount: number = maxRetries
  ): Promise<T> => {
    let lastError: Error = new Error('Unknown error')

    for (let attempt = 1; attempt <= maxRetriesCount; attempt++) {
      try {
        loading.value[operation] = true
        errors.value[operation] = null

        const result = await apiCall()
        return result

      } catch (error) {
        lastError = error instanceof Error ? error : new Error('Unknown error')

        console.warn(`${operation} attempt ${attempt} failed:`, lastError.message)

        // Don't retry on client errors (4xx)
        if (lastError.message.includes('400') || lastError.message.includes('422')) {
          break
        }

        // Wait before retry (exponential backoff)
        if (attempt < maxRetriesCount) {
          const delay = baseDelay * Math.pow(2, attempt - 1)
          await new Promise(resolve => setTimeout(resolve, delay))
        }
      } finally {
        loading.value[operation] = false
      }
    }

    // Set error state
    errors.value[operation] = getUserFriendlyErrorMessage(lastError!)
    throw lastError
  }

  // Convert technical errors to user-friendly messages
  const getUserFriendlyErrorMessage = (error: Error): string => {
    if (error.message.includes('fetch') || error.message.includes('network')) {
      return 'Unable to connect to the server. Please check your internet connection and try again.'
    }

    if (error.message.includes('400')) {
      return 'Invalid data submitted. Please check your input and try again.'
    }

    if (error.message.includes('422')) {
      return 'Validation failed. Please check your input and try again.'
    }

    if (error.message.includes('500')) {
      return 'Server error occurred. Our team has been notified. Please try again later.'
    }

    if (error.message.includes('403')) {
      return 'Access denied. Please contact support if this persists.'
    }

    if (error.message.includes('404')) {
      return 'Service not found. Please try again later.'
    }

    return 'An unexpected error occurred. Please try again.'
  }

  const submitSurvey = async (survey: SurveyRequest) => {
    return apiCallWithRetry(async () => {
      const startedAt = performance.now()
      let response: Response

      try {
        response = await fetch('/api/survey', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(survey),
        })
      } catch (error) {
        recordFetchTelemetry('/api/survey', 'POST', 0, performance.now() - startedAt, false)
        throw error instanceof Error ? error : new Error('Unknown error')
      }

      const duration = performance.now() - startedAt
      recordFetchTelemetry('/api/survey', 'POST', response.status, duration, response.ok)

      if (!response.ok) {
        const errorText = await response.text()
        throw new Error(`${response.status}: ${errorText || response.statusText}`)
      }

      await Promise.allSettled([
        fetchNps(),
        fetchAverage(),
        fetchDistribution()
      ])

      return response
    }, 'submit')
  }

  const fetchNps = async () => {
    return apiCallWithRetry(async () => {
      const startedAt = performance.now()
      let response: Response

      try {
        response = await fetch('/api/survey/nps')
      } catch (error) {
        recordFetchTelemetry('/api/survey/nps', 'GET', 0, performance.now() - startedAt, false)
        throw error instanceof Error ? error : new Error('Unknown error')
      }

      const duration = performance.now() - startedAt
      recordFetchTelemetry('/api/survey/nps', 'GET', response.status, duration, response.ok)

      if (!response.ok) {
        throw new Error(`${response.status}: ${response.statusText}`)
      }

      const data: NpsResponse = await response.json()
      nps.value = data.nps
      return data
    }, 'nps')
  }

  const fetchAverage = async () => {
    return apiCallWithRetry(async () => {
      const startedAt = performance.now()
      let response: Response

      try {
        response = await fetch('/api/survey/average')
      } catch (error) {
        recordFetchTelemetry('/api/survey/average', 'GET', 0, performance.now() - startedAt, false)
        throw error instanceof Error ? error : new Error('Unknown error')
      }

      const duration = performance.now() - startedAt
      recordFetchTelemetry('/api/survey/average', 'GET', response.status, duration, response.ok)

      if (!response.ok) {
        throw new Error(`${response.status}: ${response.statusText}`)
      }

      const data: AverageResponse = await response.json()
      average.value = data.average
      return data
    }, 'average')
  }

  const fetchDistribution = async () => {
    return apiCallWithRetry(async () => {
      const startedAt = performance.now()
      let response: Response

      try {
        response = await fetch('/api/survey/distribution')
      } catch (error) {
        recordFetchTelemetry('/api/survey/distribution', 'GET', 0, performance.now() - startedAt, false)
        throw error instanceof Error ? error : new Error('Unknown error')
      }

      const duration = performance.now() - startedAt
      recordFetchTelemetry('/api/survey/distribution', 'GET', response.status, duration, response.ok)

      if (!response.ok) {
        throw new Error(`${response.status}: ${response.statusText}`)
      }

      const data: DistributionResponse = await response.json()
      distribution.value = data
      return data
    }, 'distribution')
  }

  // Clear all errors
  const clearErrors = () => {
    errors.value = {
      submit: null,
      nps: null,
      average: null,
      distribution: null,
    }
  }

  // Check if any operation is loading
  const isAnyLoading = computed(() => {
    return Object.values(loading.value).some(loading => loading)
  })

  // Check if there are any errors
  const hasErrors = computed(() => {
    return Object.values(errors.value).some(error => error !== null)
  })

  return {
    // State
    nps: computed(() => nps.value),
    average: computed(() => average.value),
    distribution: computed(() => distribution.value),

    // Loading states
    isLoading: computed(() => loading.value),
    isAnyLoading,
    isSubmitLoading: computed(() => loading.value.submit),
    isNpsLoading: computed(() => loading.value.nps),
    isAverageLoading: computed(() => loading.value.average),
    isDistributionLoading: computed(() => loading.value.distribution),

    // Error states
    errors: computed(() => errors.value),
    hasErrors,
    submitError: computed(() => errors.value.submit),
    npsError: computed(() => errors.value.nps),
    averageError: computed(() => errors.value.average),
    distributionError: computed(() => errors.value.distribution),

    // Actions
    submitSurvey,
    fetchNps,
    fetchAverage,
    fetchDistribution,
    clearErrors,
  }
})
