<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import SurveyForm from '../components/SurveyForm.vue'
import NpsChart from '../components/NpsChart.vue'
import RatingDistribution from '../components/RatingDistribution.vue'
import ThemeToggle from '../components/ThemeToggle.vue'
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
  <main>
    <div class="container">
      <header class="app-header">
        <h1>Jack Limited Feedback Portal</h1>
        <p class="subtitle">Help us improve by sharing your experience</p>
        <div class="header-actions">
          <ThemeToggle />
        </div>
      </header>

      <SurveyForm />

      <section class="analytics-section">
        <h2>Analytics Dashboard</h2>

        <!-- Global loading indicator -->
        <div v-if="surveyStore.isAnyLoading" class="global-loading">
          <div class="loading-spinner"></div>
          <span>Updating data...</span>
        </div>

        <!-- Global error indicator -->
        <div v-if="surveyStore.hasErrors" class="global-error">
          <div class="error-icon">⚠</div>
          <div class="error-content">
            <h3>Data Loading Issues</h3>
            <p>Some data may not be current. Please check your connection.</p>
            <button @click="loadData" :disabled="surveyStore.isAnyLoading" class="refresh-button">
              Refresh Data
            </button>
          </div>
        </div>

        <div class="charts">
          <div class="chart-wrapper">
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
          <div class="chart-wrapper">
            <RatingDistribution
              v-if="!surveyStore.isAverageLoading && !surveyStore.isDistributionLoading && !surveyStore.averageError && !surveyStore.distributionError"
              :is-loading="false"
              :error="null"
              :on-retry="() => Promise.all([surveyStore.fetchAverage(), surveyStore.fetchDistribution()])"
            />
            <ChartSkeleton v-else />
          </div>
        </div>
      </section>
    </div>
  </main>
</template>

<style scoped>
.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.app-header {
  text-align: center;
  margin-bottom: 3rem;
  padding: 2rem 0;
  border-bottom: 1px solid var(--color-border);
  position: relative;
}

.app-header h1 {
  color: var(--color-heading);
  margin-bottom: 0.5rem;
  font-size: 2.5rem;
  font-weight: 700;
}

.subtitle {
  color: var(--color-text);
  font-size: 1.1rem;
  margin: 0;
}

.header-actions {
  position: absolute;
  top: 1rem;
  right: 1rem;
}

.analytics-section {
  margin-top: 4rem;
}

.analytics-section h2 {
  text-align: center;
  color: #333;
  margin-bottom: 2rem;
  font-size: 1.8rem;
  font-weight: 600;
}

.charts {
  display: flex;
  flex-wrap: wrap;
  gap: 2rem;
  margin-top: 2rem;
}

.chart-wrapper {
  flex: 1;
  min-width: 300px;
}

.global-loading, .global-error {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  padding: 1rem 1.5rem;
  margin-bottom: 2rem;
  border-radius: 8px;
  font-size: 0.95rem;
}

.global-loading {
  background: #e7f3ff;
  border: 1px solid #b3d9ff;
  color: #0066cc;
}

.global-error {
  background: #fff3cd;
  border: 1px solid #ffeaa7;
  color: #856404;
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

.error-content {
  flex: 1;
}

.error-content h3 {
  margin: 0 0 0.25rem 0;
  font-size: 1rem;
  font-weight: 600;
}

.error-content p {
  margin: 0 0 0.5rem 0;
  font-size: 0.9rem;
}

.refresh-button {
  background: #ffc107;
  color: #212529;
  border: none;
  padding: 0.375rem 0.75rem;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.85rem;
  font-weight: 500;
  transition: background-color 0.3s ease;
}

.refresh-button:hover:not(:disabled) {
  background: #e0a800;
}

.refresh-button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Responsive design */
@media (max-width: 768px) {
  .container {
    padding: 15px;
  }

  .app-header h1 {
    font-size: 2rem;
  }

  .charts {
    flex-direction: column;
    gap: 1.5rem;
  }

  .charts > * {
    min-width: unset;
  }
}
</style>
