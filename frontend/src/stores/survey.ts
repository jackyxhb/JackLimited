import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import type { SurveyRequest, NpsResponse, AverageResponse, DistributionResponse } from '@/types/survey'

export const useSurveyStore = defineStore('survey', () => {
  const nps = ref<number>(0)
  const average = ref<number>(0)
  const distribution = ref<Record<number, number>>({})
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const submitSurvey = async (survey: SurveyRequest) => {
    isLoading.value = true
    error.value = null
    try {
      const response = await fetch('/api/survey', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(survey),
      })
      if (!response.ok) {
        throw new Error('Failed to submit survey')
      }
      await Promise.all([fetchNps(), fetchAverage(), fetchDistribution()]) // Refresh all analytics after submission
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Unknown error'
    } finally {
      isLoading.value = false
    }
  }

  const fetchNps = async () => {
    try {
      const response = await fetch('/api/survey/nps')
      if (!response.ok) {
        throw new Error('Failed to fetch NPS')
      }
      const data: NpsResponse = await response.json()
      nps.value = data.nps
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Unknown error'
    }
  }

  const fetchAverage = async () => {
    try {
      const response = await fetch('/api/survey/average')
      if (!response.ok) {
        throw new Error('Failed to fetch average')
      }
      const data: AverageResponse = await response.json()
      average.value = data.average
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Unknown error'
    }
  }

  const fetchDistribution = async () => {
    try {
      const response = await fetch('/api/survey/distribution')
      if (!response.ok) {
        throw new Error('Failed to fetch distribution')
      }
      const data: DistributionResponse = await response.json()
      distribution.value = data
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Unknown error'
    }
  }

  return {
    nps: computed(() => nps.value),
    average: computed(() => average.value),
    distribution: computed(() => distribution.value),
    isLoading: computed(() => isLoading.value),
    error: computed(() => error.value),
    submitSurvey,
    fetchNps,
    fetchAverage,
    fetchDistribution,
  }
})
