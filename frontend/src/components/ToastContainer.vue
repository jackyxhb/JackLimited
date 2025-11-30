<template>
  <Teleport to="body">
    <TransitionGroup name="toast" tag="div" class="toast-container">
      <div
        v-for="toast in toasts"
        :key="toast.id"
        :class="['toast', `toast--${toast.type}`]"
        @click="handleToastClick(toast)"
      >
        <div class="toast__content">
          <div class="toast__icon">
            <span v-if="toast.type === 'success'">✓</span>
            <span v-else-if="toast.type === 'error'">✕</span>
            <span v-else-if="toast.type === 'warning'">⚠</span>
            <span v-else-if="toast.type === 'info'">ℹ</span>
          </div>
          <div class="toast__text">
            <div class="toast__title">{{ toast.title }}</div>
            <div v-if="toast.message" class="toast__message">{{ toast.message }}</div>
          </div>
          <button
            v-if="toast.action"
            @click.stop="handleAction(toast)"
            class="toast__action"
          >
            {{ toast.action.label }}
          </button>
        </div>
        <button @click.stop="removeToast(toast.id)" class="toast__close">
          ×
        </button>
        <div v-if="toast.duration && toast.duration > 0" class="toast__progress">
          <div
            class="toast__progress-bar"
            :style="{ animationDuration: `${toast.duration}ms` }"
          ></div>
        </div>
      </div>
    </TransitionGroup>
  </Teleport>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useToastStore, type Toast } from '@/stores/toast'

const toastStore = useToastStore()
const { toasts, removeToast } = toastStore

const handleToastClick = (toast: Toast) => {
  if (!toast.action) {
    removeToast(toast.id)
  }
}

const handleAction = (toast: Toast) => {
  if (toast.action) {
    toast.action.handler()
  }
  removeToast(toast.id)
}
</script>

<style scoped>
.toast-container {
  position: fixed;
  top: 1rem;
  right: 1rem;
  z-index: 1000;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  max-width: 400px;
  pointer-events: none;
}

.toast {
  background: var(--color-background);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  padding: 0;
  pointer-events: auto;
  overflow: hidden;
  transition: all 0.3s ease;
  cursor: pointer;
}

.toast:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.2);
}

.toast--success {
  border-color: #10b981;
}

.toast--success .toast__icon {
  color: #10b981;
}

.toast--error {
  border-color: #ef4444;
}

.toast--error .toast__icon {
  color: #ef4444;
}

.toast--warning {
  border-color: #f59e0b;
}

.toast--warning .toast__icon {
  color: #f59e0b;
}

.toast--info {
  border-color: #3b82f6;
}

.toast--info .toast__icon {
  color: #3b82f6;
}

.toast__content {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  padding: 1rem;
}

.toast__icon {
  font-size: 1.25rem;
  font-weight: bold;
  flex-shrink: 0;
  margin-top: 0.125rem;
}

.toast__text {
  flex: 1;
  min-width: 0;
}

.toast__title {
  font-weight: 600;
  font-size: 0.875rem;
  color: var(--color-heading);
  margin-bottom: 0.25rem;
}

.toast__message {
  font-size: 0.8125rem;
  color: var(--color-text);
  line-height: 1.4;
}

.toast__action {
  background: transparent;
  border: 1px solid var(--color-border);
  color: var(--color-text);
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.75rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  flex-shrink: 0;
}

.toast__action:hover {
  background: var(--color-background-soft);
}

.toast__close {
  position: absolute;
  top: 0.5rem;
  right: 0.5rem;
  background: transparent;
  border: none;
  color: var(--color-text);
  font-size: 1.25rem;
  line-height: 1;
  cursor: pointer;
  padding: 0;
  width: 1.5rem;
  height: 1.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  transition: background-color 0.2s ease;
}

.toast__close:hover {
  background: var(--color-background-soft);
}

.toast__progress {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 3px;
  background: var(--color-background-soft);
}

.toast__progress-bar {
  height: 100%;
  background: currentColor;
  animation: progress linear;
  transform-origin: left;
}

@keyframes progress {
  0% {
    transform: scaleX(1);
  }
  100% {
    transform: scaleX(0);
  }
}

/* Toast animations */
.toast-enter-active,
.toast-leave-active {
  transition: all 0.3s ease;
}

.toast-enter-from {
  opacity: 0;
  transform: translateX(100%);
}

.toast-leave-to {
  opacity: 0;
  transform: translateX(100%);
}

.toast-move {
  transition: transform 0.3s ease;
}

/* Dark mode adjustments */
:global(.dark) .toast {
  background: var(--color-background-soft);
}

/* Mobile responsiveness */
@media (max-width: 480px) {
  .toast-container {
    left: 1rem;
    right: 1rem;
    max-width: none;
  }

  .toast__content {
    padding: 0.75rem;
  }

  .toast__title {
    font-size: 0.8125rem;
  }

  .toast__message {
    font-size: 0.75rem;
  }
}
</style>
