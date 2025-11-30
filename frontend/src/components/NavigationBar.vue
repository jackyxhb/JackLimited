<template>
  <nav class="navbar">
    <div class="navbar-container">
      <!-- Logo/Brand -->
      <div class="navbar-brand">
        <router-link to="/" class="brand-link">
          <span class="brand-text">Jack Limited</span>
        </router-link>
      </div>

      <!-- Desktop Navigation -->
      <div class="navbar-menu" :class="{ 'is-active': isMobileMenuOpen }">
        <router-link
          to="/"
          class="navbar-item"
          @click="closeMobileMenu"
          :class="{ 'is-active': $route.name === 'home' }"
        >
          <HomeIcon class="icon" />
          <span>Home</span>
        </router-link>

        <router-link
          to="/survey"
          class="navbar-item"
          @click="closeMobileMenu"
          :class="{ 'is-active': $route.name === 'survey' }"
        >
          <MessageSquareIcon class="icon" />
          <span>Feedback</span>
        </router-link>

        <router-link
          to="/analytics"
          class="navbar-item"
          @click="closeMobileMenu"
          :class="{ 'is-active': $route.name === 'analytics' }"
        >
          <BarChart3Icon class="icon" />
          <span>Analytics</span>
        </router-link>

        <router-link
          to="/about"
          class="navbar-item"
          @click="closeMobileMenu"
          :class="{ 'is-active': $route.name === 'about' }"
        >
          <InfoIcon class="icon" />
          <span>About</span>
        </router-link>
      </div>

      <!-- Theme Toggle -->
      <div class="navbar-end">
        <ThemeToggle />
      </div>

      <!-- Mobile Menu Toggle -->
      <button
        class="navbar-burger"
        @click="toggleMobileMenu"
        :class="{ 'is-active': isMobileMenuOpen }"
        aria-label="menu"
        aria-expanded="false"
      >
        <span aria-hidden="true"></span>
        <span aria-hidden="true"></span>
        <span aria-hidden="true"></span>
      </button>
    </div>
  </nav>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import ThemeToggle from '@/components/ThemeToggle.vue'
import { HomeIcon, MessageSquareIcon, BarChart3Icon, InfoIcon } from 'lucide-vue-next'

const isMobileMenuOpen = ref(false)

const toggleMobileMenu = () => {
  isMobileMenuOpen.value = !isMobileMenuOpen.value
}

const closeMobileMenu = () => {
  isMobileMenuOpen.value = false
}
</script>

<style>
.navbar {
  background: var(--color-background);
  border-bottom: 1px solid var(--color-border);
  position: sticky;
  top: 0;
  z-index: 1000;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.navbar-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 0 var(--spacing-lg);
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 4.5rem;
  min-height: 4.5rem;
}

.navbar-brand {
  flex-shrink: 0;
  font-weight: var(--font-weight-bold);
  font-size: var(--font-size-lg);
}

.brand-link {
  color: var(--color-text);
  text-decoration: none;
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  white-space: nowrap;
  transition: color var(--transition-fast);
}

.brand-link:hover {
  color: var(--color-primary);
}

.brand-text {
  font-weight: var(--font-weight-bold);
  font-size: var(--font-size-lg);
  letter-spacing: -0.025em;
}

.navbar-menu {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  flex-shrink: 0;
}

.navbar-item {
  color: var(--color-text);
  text-decoration: none;
  padding: var(--spacing-md) var(--spacing-lg);
  border-radius: var(--border-radius);
  transition: all var(--transition-fast);
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  font-weight: var(--font-weight-medium);
  font-size: var(--font-size-sm);
  position: relative;
  white-space: nowrap;
}

.navbar-menu {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.navbar-item {
  color: var(--color-text);
  text-decoration: none;
  padding: 0.75rem 1rem;
  border-radius: 6px;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 500;
  position: relative;
}

.navbar-item:hover {
  background: var(--color-hover);
  color: var(--color-heading);
}

.navbar-item.is-active {
  background: var(--color-primary);
  color: white;
}

.navbar-item.is-active:hover {
  background: var(--color-primary-hover);
}

.navbar-end {
  display: flex;
  align-items: center;
}

.navbar-burger {
  display: none;
  flex-direction: column;
  justify-content: space-around;
  width: 2rem;
  height: 2rem;
  background: transparent;
  border: none;
  cursor: pointer;
  padding: 0;
  margin-left: 1rem;
}

.navbar-burger span {
  width: 100%;
  height: 2px;
  background: var(--color-text);
  border-radius: 1px;
  transition: all 0.3s ease;
  transform-origin: center;
}

.navbar-burger.is-active span:nth-child(1) {
  transform: rotate(45deg) translate(6px, 6px);
}

.navbar-burger.is-active span:nth-child(2) {
  opacity: 0;
}

.navbar-burger.is-active span:nth-child(3) {
  transform: rotate(-45deg) translate(6px, -6px);
}

/* Mobile Styles */
@media (max-width: 768px) {
  .navbar-container {
    padding: 0 var(--spacing-md);
    height: 4rem;
    min-height: 4rem;
  }

  .navbar-menu {
    position: absolute;
    top: 100%;
    left: 0;
    right: 0;
    background: var(--color-surface);
    border-bottom: 1px solid var(--color-border);
    flex-direction: column;
    padding: var(--spacing-lg) 0;
    transform: translateY(-100%);
    opacity: 0;
    visibility: hidden;
    transition: all var(--transition-normal);
    box-shadow: 0 8px 16px var(--color-shadow);
    border-radius: 0 0 var(--border-radius) var(--border-radius);
  }

  .navbar-menu.is-active {
    transform: translateY(0);
    opacity: 1;
    visibility: visible;
  }

  .navbar-item {
    width: 100%;
    justify-content: center;
    padding: var(--spacing-lg);
    margin: var(--spacing-xs) var(--spacing-lg);
    border-radius: var(--border-radius);
    font-size: var(--font-size-base);
  }

  .navbar-burger {
    display: flex;
  }

  .brand-text {
    font-size: var(--font-size-base);
  }
}

@media (max-width: 480px) {
  .navbar-container {
    height: 3.5rem;
    min-height: 3.5rem;
    padding: 0 var(--spacing-sm);
  }

  .navbar-item {
    padding: var(--spacing-md) var(--spacing-lg);
    font-size: var(--font-size-sm);
    margin: var(--spacing-xs) var(--spacing-md);
  }

  .brand-text {
    font-size: var(--font-size-sm);
  }
}
</style>
