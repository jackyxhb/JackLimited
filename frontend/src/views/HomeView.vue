<script setup lang="ts">
import { onMounted } from 'vue'
import SurveyForm from '../components/SurveyForm.vue'
import NpsChart from '../components/NpsChart.vue'
import RatingDistribution from '../components/RatingDistribution.vue'
import { useSurveyStore } from '@/stores/survey'

const surveyStore = useSurveyStore()

onMounted(async () => {
  await Promise.all([
    surveyStore.fetchNps(),
    surveyStore.fetchAverage(),
    surveyStore.fetchDistribution()
  ])
})
</script>

<template>
  <main>
    <div class="container">
      <h1>Jack Limited Feedback Portal</h1>
      <SurveyForm />
      <div class="charts">
        <NpsChart :nps="surveyStore.nps" />
        <RatingDistribution />
      </div>
    </div>
  </main>
</template>

<style scoped>
.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.charts {
  display: flex;
  flex-wrap: wrap;
  gap: 20px;
  margin-top: 40px;
}

.charts > * {
  flex: 1;
  min-width: 300px;
}
</style>
