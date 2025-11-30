<template>
  <div class="nps-chart">
    <h3>Net Promoter Score</h3>
    <Doughnut :data="chartData" :options="chartOptions" />
    <p class="nps-value">NPS: {{ nps }}</p>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
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
}

const props = defineProps<Props>()

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
      borderWidth: 1
    }]
  }
})

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'bottom' as const,
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
</script>

<style scoped>
.nps-chart {
  max-width: 400px;
  margin: 2rem auto;
}

.nps-value {
  text-align: center;
  font-size: 1.5rem;
  font-weight: bold;
  margin-top: 1rem;
}
</style>
