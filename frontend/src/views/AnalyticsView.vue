<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import ViewContainer from '@/components/ViewContainer.vue'
import NpsChart from '../components/NpsChart.vue'
import RatingDistribution from '../components/RatingDistribution.vue'
import ChartSkeleton from '../components/ChartSkeleton.vue'
import { useSurveyStore } from '@/stores/survey'
import { AlertCircleIcon } from 'lucide-vue-next'

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

const retryNps = async () => {
  await Promise.all([surveyStore.fetchNps(), surveyStore.fetchDistribution()])
}

const retryAverage = async () => {
  await Promise.all([surveyStore.fetchAverage(), surveyStore.fetchDistribution()])
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
  <ViewContainer>
    <div class="content-wrapper">
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
        <AlertCircleIcon class="error-icon" />
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
            :on-retry="retryNps"
          />
          <ChartSkeleton v-else />
        </div>

        <div class="card">
          <RatingDistribution
            v-if="!surveyStore.isAverageLoading && !surveyStore.isDistributionLoading && !surveyStore.averageError && !surveyStore.distributionError"
            :is-loading="false"
            :error="null"
            :on-retry="retryAverage"
          />
          <ChartSkeleton v-else />
        </div>
      </div>
    </div>
  </ViewContainer>
</template><style scoped>
.content-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
  max-width: 1000px;
  margin: 0 auto;
}
</style>
