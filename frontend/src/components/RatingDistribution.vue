<template>
  <div class="rating-distribution">
    <h3>Average Rating: {{ average }}</h3>
    <canvas ref="chartCanvas"></canvas>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, onUnmounted } from 'vue'
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend } from 'chart.js'
import { Bar } from 'vue-chartjs'
import { useSurveyStore } from '@/stores/survey'

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend)

const chartCanvas = ref<HTMLCanvasElement>()
let chart: ChartJS | null = null

const surveyStore = useSurveyStore()

const average = surveyStore.average
const distribution = surveyStore.distribution

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
        borderWidth: 1
      }]
    },
    options: {
      responsive: true,
      plugins: {
        legend: {
          position: 'top',
        },
        title: {
          display: true,
          text: 'Rating Distribution'
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: {
            stepSize: 1
          }
        }
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
  chart.update()
}

const destroyChart = () => {
  if (chart) {
    chart.destroy()
    chart = null
  }
}

onMounted(() => {
  createChart()
})

watch(distribution, () => {
  if (chart) {
    updateChart()
  } else {
    createChart()
  }
})

onUnmounted(() => {
  destroyChart()
})
</script>

<style scoped>
.rating-distribution {
  margin: 20px 0;
}

.rating-distribution h3 {
  text-align: center;
  margin-bottom: 20px;
}
</style>
