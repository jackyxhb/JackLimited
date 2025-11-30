<template>
  <div class="nps-chart">
    <h3>Net Promoter Score</h3>

    <!-- Loading State -->
    <div v-if="isLoading" class="loading-container">
      <div class="loading-spinner"></div>
      <p>Loading NPS data...</p>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="error-container">
      <div class="error-icon">⚠</div>
      <p class="error-message">{{ error }}</p>
      <button @click="onRetry" class="retry-button">
        Try Again
      </button>
    </div>

    <!-- Chart Content -->
    <div v-else class="chart-container">
      <div class="chart-header">
        <Doughnut ref="chartRef" :data="chartData" :options="chartOptions" />
        <button @click="exportChart" class="export-button" title="Download chart as image">
          📥 Download
        </button>
      </div>
      <p class="nps-value">NPS: {{ nps }}</p>
      <p class="response-count">
        Total Responses: {{ totalResponses }}
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { Doughnut } from 'vue-chartjs'
import {
  Chart as ChartJS,
  ArcElement,
  Tooltip,
  Legend
} from 'chart.js'

ChartJS.register(ArcElement, Tooltip, Legend)

interface Props {
  nps: number
  distribution: Record<number, number>
  isLoading?: boolean
  error?: string | null
  onRetry?: () => void
}

const props = withDefaults(defineProps<Props>(), {
  isLoading: false,
  error: null,
  onRetry: () => {}
})

const chartRef = ref()

const totalResponses = computed(() => {
  return Object.values(props.distribution).reduce((sum, count) => sum + count, 0)
})

const chartData = computed(() => {
  // Calculate NPS categories from distribution
  let promoters = 0
  let passives = 0
  let detractors = 0

  // Count responses in each NPS category
  Object.entries(props.distribution).forEach(([rating, count]) => {
    const ratingNum = parseInt(rating)
    if (ratingNum >= 9 && ratingNum <= 10) {
      promoters += count
    } else if (ratingNum >= 7 && ratingNum <= 8) {
      passives += count
    } else if (ratingNum >= 0 && ratingNum <= 6) {
      detractors += count
    }
  })

  return {
    labels: ['Promoters (9-10)', 'Passives (7-8)', 'Detractors (0-6)'],
    datasets: [{
      data: [promoters, passives, detractors],
      backgroundColor: [
        '#28a745', // Green for promoters
        '#ffc107', // Yellow for passives
        '#dc3545'  // Red for detractors
      ],
      borderWidth: 1,
      borderColor: '#fff'
    }]
  }
})

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'bottom' as const,
      labels: {
        padding: 20,
        usePointStyle: true
      }
    },
    tooltip: {
      callbacks: {
        label: (context: { label?: string; parsed: number; dataset: { data: number[] } }) => {
          const label = context.label || ''
          const value = context.parsed
          const total = context.dataset.data.reduce((a: number, b: number) => a + b, 0)
          const percentage = total > 0 ? Math.round((value / total) * 100) : 0
          return `${label}: ${value} (${percentage}%)`
        }
      }
    }
  },
}

const exportChart = () => {
  if (chartRef.value && chartRef.value.chart) {
    const link = document.createElement('a')
    link.download = `nps-chart-${new Date().toISOString().split('T')[0]}.png`
    link.href = chartRef.value.chart.toBase64Image()
    link.click()
  }
}
</script>

<style scoped>
.nps-chart {
  width: 100%;
  margin: 0;
  padding: 0;
  overflow: hidden;
}

.chart-container {
  position: relative;
  height: 300px;
  overflow: hidden;
}

.chart-header {
  position: relative;
  height: 100%;
}

.export-button {
  position: absolute;
  top: 10px;
  right: 10px;
  background: var(--color-background);
  border: 1px solid var(--color-border);
  border-radius: 6px;
  padding: 0.5rem;
  cursor: pointer;
  font-size: 0.8rem;
  color: var(--color-text);
  transition: all 0.2s ease;
  z-index: 10;
}

.export-button:hover {
  background: var(--color-background-soft);
  border-color: var(--color-border-hover);
}

.nps-value {
  text-align: center;
  font-size: 1.5rem;
  font-weight: bold;
  margin: 1rem 0 0.5rem 0;
  color: #333;
}

.response-count {
  text-align: center;
  font-size: 0.9rem;
  color: #6c757d;
  margin: 0;
}

.loading-container, .error-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 300px;
  text-align: center;
  padding: 2rem;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #007bff;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 1rem;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error-container {
  color: #721c24;
  background: #f8d7da;
  border: 1px solid #f5c6cb;
  border-radius: 8px;
}

.error-icon {
  font-size: 2rem;
  margin-bottom: 1rem;
}

.error-message {
  margin: 0 0 1rem 0;
  font-size: 0.95rem;
  line-height: 1.4;
}

.retry-button {
  background: #dc3545;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
  transition: background-color 0.3s ease;
}

.retry-button:hover {
  background: #c82333;
}

/* Responsive design */
@media (max-width: 600px) {
  .nps-chart {
    margin: 0;
    padding: 0;
  }

  .chart-container {
    height: 250px;
  }
}
</style>
