<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import NavigationBar from '@/components/NavigationBar.vue'
import NpsChart from '../components/NpsChart.vue'
import RatingDistribution from '../components/RatingDistribution.vue'
import ChartSkeleton from '../components/ChartSkeleton.vue'
import { useSurveyStore } from '@/stores/survey'

const surveyStore = useSurveyStore()
let refreshInterval: number | null = null

// Load initial data with error handling
const loadData = async () => {
  try {
    await Promise.allSettled([
      surveyStore.fetchNps(),
      surveyStore.fetchAverage(),
      surveyStore.fetchDistribution()
    ])
  } catch (error) {
    console.error('Failed to load initial data:', error)
  }
}

// Auto-refresh data every 30 seconds
const startAutoRefresh = () => {
  refreshInterval = setInterval(async () => {
    if (!surveyStore.isAnyLoading) {
      await loadData()
    }
  }, 30000) // 30 seconds
}

const stopAutoRefresh = () => {
  if (refreshInterval) {
    clearInterval(refreshInterval)
    refreshInterval = null
  }
}

onMounted(async () => {
  await loadData()
  startAutoRefresh()
})

onUnmounted(() => {
  stopAutoRefresh()
})
</script>

<template>
  <div>
    <NavigationBar />
    <main class="page-content">
      <div class="container">
        <header class="page-header">
          <h1>Analytics Dashboard</h1>
          <p class="subtitle">Real-time insights into customer feedback and satisfaction</p>
        </header>

        <!-- Global loading indicator -->
        <div v-if="surveyStore.isAnyLoading" class="loading-container">
          <div class="loading-spinner"></div>
          <span>Updating data...</span>
        </div>

        <!-- Global error indicator -->
        <div v-if="surveyStore.hasErrors" class="error-container">
          <div class="error-icon">⚠</div>
          <div class="error-content">
            <h3>Data Loading Issues</h3>
            <p>Some data may not be current. Please check your connection.</p>
            <button @click="loadData" :disabled="surveyStore.isAnyLoading" class="btn btn-sm">
              Refresh Data
            </button>
          </div>
        </div>

        <div class="grid grid-charts">
          <div class="card">
            <NpsChart
              v-if="!surveyStore.isNpsLoading && !surveyStore.isDistributionLoading && !surveyStore.npsError && !surveyStore.distributionError"
              :nps="surveyStore.nps"
              :distribution="surveyStore.distribution"
              :is-loading="false"
              :error="null"
              :on-retry="() => Promise.all([surveyStore.fetchNps(), surveyStore.fetchDistribution()])"
            />
            <ChartSkeleton v-else />
          </div>

          <div class="card">
            <RatingDistribution
              v-if="!surveyStore.isAverageLoading && !surveyStore.isDistributionLoading && !surveyStore.averageError && !surveyStore.distributionError"
              :is-loading="false"
              :error="null"
              :on-retry="() => Promise.all([surveyStore.fetchAverage(), surveyStore.fetchDistribution()])"
            />
            <ChartSkeleton v-else />
          </div>
        </div>
      </div>
    </main>
</div>
</template>

<style scoped>
/* Analytics page specific styles */
</style>
