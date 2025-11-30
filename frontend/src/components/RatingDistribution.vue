<template>
  <div class="rating-distribution">
    <h3>Rating Distribution</h3>

    <!-- Loading State -->
    <div v-if="isLoading" class="loading-container">
      <div class="loading-spinner"></div>
      <p>Loading distribution data...</p>
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
        <div class="average-display">
          <span class="average-label">Average Rating:</span>
          <span class="average-value">{{ average.toFixed(1) }}</span>
          <span class="total-responses">({{ totalResponses }} responses)</span>
        </div>
        <button @click="exportChart" class="export-button" title="Download chart as image">
          📥 Download
        </button>
      </div>
      <canvas ref="chartCanvas"></canvas>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, onUnmounted, computed } from 'vue'
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend } from 'chart.js'
import { useSurveyStore } from '@/stores/survey'

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend)

interface Props {
  isLoading?: boolean
  error?: string | null
  onRetry?: () => void
}

const props = withDefaults(defineProps<Props>(), {
  isLoading: false,
  error: null,
  onRetry: () => {}
})

const chartCanvas = ref<HTMLCanvasElement>()
let chart: ChartJS | null = null

const surveyStore = useSurveyStore()
const average = surveyStore.average
const distribution = surveyStore.distribution

const totalResponses = computed(() => {
  return Object.values(distribution).reduce((sum, count) => sum + count, 0)
})

const createChart = () => {
  if (!chartCanvas.value) return

  const ctx = chartCanvas.value.getContext('2d')
  if (!ctx) return

  const labels = Object.keys(distribution).map(key => `Rating ${key}`)
  const data = Object.values(distribution)

  chart = new ChartJS(ctx, {
    type: 'bar',
    data: {
      labels,
      datasets: [{
        label: 'Number of Responses',
        data,
        backgroundColor: 'rgba(54, 162, 235, 0.6)',
        borderColor: 'rgba(54, 162, 235, 1)',
        borderWidth: 1,
        borderRadius: 4,
        borderSkipped: false,
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: 'top',
          labels: {
            usePointStyle: true,
            padding: 20
          }
        },
        title: {
          display: false
        },
        tooltip: {
          callbacks: {
            label: (context) => {
              const value = context.parsed.y as number
              const total = (context.dataset.data as number[]).reduce((sum, val) => sum + val, 0)
              const percentage = total > 0 ? Math.round((value / total) * 100) : 0
              return `${value} responses (${percentage}%)`
            }
          }
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: {
            stepSize: 1,
            precision: 0
          },
          grid: {
            color: 'rgba(0, 0, 0, 0.1)'
          }
        },
        x: {
          grid: {
            display: false
          }
        }
      },
      animation: {
        duration: 1000,
        easing: 'easeOutQuart'
      }
    }
  })
}

const updateChart = () => {
  if (!chart || !chart.data.datasets[0]) return

  const labels = Object.keys(distribution).map(key => `Rating ${key}`)
  const data = Object.values(distribution)

  chart.data.labels = labels
  chart.data.datasets[0].data = data
  chart.update('active')
}

const destroyChart = () => {
  if (chart) {
    chart.destroy()
    chart = null
  }
}

onMounted(() => {
  if (!props.isLoading && !props.error) {
    createChart()
  }
})

watch([distribution, () => props.isLoading, () => props.error], () => {
  if (props.isLoading || props.error) {
    destroyChart()
  } else {
    if (chart) {
      updateChart()
    } else {
      createChart()
    }
  }
})

onUnmounted(() => {
  destroyChart()
})

const exportChart = () => {
  if (chart && chartCanvas.value) {
    const link = document.createElement('a')
    link.download = `rating-distribution-${new Date().toISOString().split('T')[0]}.png`
    link.href = chart.toBase64Image()
    link.click()
  }
}
</script>

<style scoped>
.rating-distribution {
  margin: 2rem 0;
  padding: 1.5rem;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

.chart-container {
  position: relative;
  height: 300px;
}

.chart-header {
  position: relative;
}

.average-display {
  text-align: center;
  margin-bottom: 1.5rem;
  padding: 1rem;
  background: linear-gradient(135deg, #f8f9fa, #e9ecef);
  border-radius: 8px;
  border: 1px solid #dee2e6;
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

.average-label {
  font-size: 0.9rem;
  color: #6c757d;
  margin-right: 0.5rem;
}

.average-value {
  font-size: 2rem;
  font-weight: bold;
  color: #007bff;
  margin-right: 0.5rem;
}

.total-responses {
  font-size: 0.85rem;
  color: #6c757d;
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
  .rating-distribution {
    margin: 1rem 0;
    padding: 1rem;
  }

  .chart-container {
    height: 250px;
  }

  .average-value {
    font-size: 1.5rem;
  }
}
</style>
