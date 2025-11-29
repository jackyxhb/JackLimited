<script setup lang="ts">
import { onMounted } from 'vue'
import SurveyForm from './components/SurveyForm.vue'
import NpsChart from './components/NpsChart.vue'
import { useSurveyStore } from './stores/survey'

const surveyStore = useSurveyStore()

onMounted(() => {
  surveyStore.fetchNps()
})
</script>

<template>
  <div id="app">
    <header>
      <h1>Jack Limited Feedback Portal</h1>
    </header>

    <main>
      <SurveyForm />
      <NpsChart :nps="surveyStore.nps" />
    </main>
  </div>
</template>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
  margin: 0;
  padding: 0;
}

header {
  background-color: #007bff;
  color: white;
  text-align: center;
  padding: 2rem 0;
}

header h1 {
  margin: 0;
}

main {
  padding: 2rem;
}
</style>
