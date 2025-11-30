<template>
  <div class="chart-skeleton">
    <div class="chart-skeleton__header">
      <SkeletonLoader variant="text" width="120px" height="24px" />
    </div>
    <div class="chart-skeleton__content">
      <div class="chart-skeleton__chart">
        <SkeletonLoader variant="circular" width="200px" height="200px" />
      </div>
      <div class="chart-skeleton__info">
        <SkeletonLoader variant="text" width="80px" height="20px" />
        <SkeletonLoader variant="text" width="120px" height="16px" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import SkeletonLoader from './SkeletonLoader.vue'
</script>

<style scoped>
.chart-skeleton {
  padding: 1.5rem;
  background: var(--color-background);
  border-radius: 8px;
  border: 1px solid var(--color-border);
}

.chart-skeleton__header {
  margin-bottom: 1rem;
}

.chart-skeleton__content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}

.chart-skeleton__chart {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 250px;
  height: 250px;
}

.chart-skeleton__info {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
}
</style>
