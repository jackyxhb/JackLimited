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
}

const props = defineProps<Props>()

const chartData = computed(() => ({
  labels: ['Promoters', 'Passives', 'Detractors'],
  datasets: [{
    data: [
      Math.max(0, props.nps), // Promoters
      0, // Passives (simplified)
      Math.max(0, -props.nps) // Detractors
    ],
    backgroundColor: [
      '#28a745', // Green for promoters
      '#ffc107', // Yellow for passives
      '#dc3545'  // Red for detractors
    ],
    borderWidth: 1
  }]
}))

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'bottom' as const,
    },
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
