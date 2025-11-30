<template>
  <button
    @click="toggleTheme"
    class="theme-toggle"
    :title="isDark ? 'Switch to light mode' : 'Switch to dark mode'"
  >
    <div class="theme-toggle__icon">
      <span v-if="isDark">☀️</span>
      <span v-else>🌙</span>
    </div>
    <span class="theme-toggle__text">
      {{ isDark ? 'Light' : 'Dark' }}
    </span>
  </button>
</template>

<script setup lang="ts">
import { useThemeStore } from '@/stores/theme'

const { isDark, toggleTheme } = useThemeStore()
</script>

<style scoped>
.theme-toggle {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 0.75rem;
  background: var(--color-background);
  border: 1px solid var(--color-border);
  border-radius: 6px;
  color: var(--color-text);
  cursor: pointer;
  font-size: 0.875rem;
  font-weight: 500;
  transition: all 0.2s ease;
}

.theme-toggle:hover {
  background: var(--color-background-soft);
  border-color: var(--color-border-hover);
}

.theme-toggle__icon {
  font-size: 1rem;
}

.theme-toggle__text {
  font-size: 0.8125rem;
}
</style>
