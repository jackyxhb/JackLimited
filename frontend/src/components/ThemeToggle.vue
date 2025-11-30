<template>
  <button
    @click="toggleTheme"
    class="theme-toggle"
    :title="isDark ? 'Switch to light mode' : 'Switch to dark mode'"
  >
    <SunIcon v-if="isDark" class="theme-toggle__icon" />
    <MoonIcon v-else class="theme-toggle__icon" />
    <span class="theme-toggle__text">
      {{ isDark ? 'Light' : 'Dark' }}
    </span>
  </button>
</template>

<script setup lang="ts">
import { useThemeStore } from '@/stores/theme'
import { MoonIcon, SunIcon } from 'lucide-vue-next'

const { isDark, toggleTheme } = useThemeStore()
</script>

<style scoped>
.theme-toggle {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--border-radius);
  color: var(--color-text);
  cursor: pointer;
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-medium);
  transition: all var(--transition-fast);
  position: relative;
  overflow: hidden;
}

.theme-toggle::before {
  content: '';
  position: absolute;
  top: 50%;
  left: 50%;
  width: 0;
  height: 0;
  background: var(--color-primary);
  border-radius: 50%;
  transform: translate(-50%, -50%);
  transition: width var(--transition-normal), height var(--transition-normal);
  opacity: 0.1;
}

.theme-toggle:hover::before {
  width: 100px;
  height: 100px;
}

.theme-toggle:hover {
  border-color: var(--color-border-hover);
  transform: translateY(-1px);
  box-shadow: 0 4px 12px var(--color-shadow);
}

.theme-toggle__icon {
  width: 18px;
  height: 18px;
  color: var(--color-primary);
  transition: transform var(--transition-fast);
}

.theme-toggle:hover .theme-toggle__icon {
  transform: scale(1.1);
}

.theme-toggle__text {
  font-size: var(--font-size-sm);
  position: relative;
  z-index: 1;
}
</style>
