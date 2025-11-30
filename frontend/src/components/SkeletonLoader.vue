<template>
  <div class="skeleton" :class="classes" :style="styles">
    <slot />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  width?: string | number
  height?: string | number
  borderRadius?: string | number
  variant?: 'text' | 'rectangular' | 'circular'
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'rectangular'
})

const classes = computed(() => ({
  [`skeleton--${props.variant}`]: true
}))

const styles = computed(() => ({
  width: typeof props.width === 'number' ? `${props.width}px` : props.width,
  height: typeof props.height === 'number' ? `${props.height}px` : props.height,
  borderRadius: typeof props.borderRadius === 'number' ? `${props.borderRadius}px` : props.borderRadius
}))
</script>

<style scoped>
.skeleton {
  background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
  background-size: 200% 100%;
  animation: loading 1.5s infinite;
  display: inline-block;
}

.skeleton--text {
  height: 1em;
  border-radius: 4px;
}

.skeleton--rectangular {
  border-radius: 4px;
}

.skeleton--circular {
  border-radius: 50%;
}

@keyframes loading {
  0% {
    background-position: 200% 0;
  }
  100% {
    background-position: -200% 0;
  }
}

/* Dark mode support */
:global(.dark) .skeleton {
  background: linear-gradient(90deg, #2a2a2a 25%, #3a3a3a 50%, #2a2a2a 75%);
}
</style>
