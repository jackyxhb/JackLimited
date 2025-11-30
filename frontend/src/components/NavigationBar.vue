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
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 4rem;
}

.navbar-brand {
  font-weight: 700;
  font-size: 1.25rem;
}

.brand-link {
  color: var(--color-heading);
  text-decoration: none;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.brand-text {
  font-weight: 700;
  font-size: 1.25rem;
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
    padding: 0 1rem;
  }

  .navbar-menu {
    position: absolute;
    top: 100%;
    left: 0;
    right: 0;
    background: var(--color-background);
    border-bottom: 1px solid var(--color-border);
    flex-direction: column;
    padding: 1rem 0;
    transform: translateY(-100%);
    opacity: 0;
    visibility: hidden;
    transition: all 0.3s ease;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  }

  .navbar-menu.is-active {
    transform: translateY(0);
    opacity: 1;
    visibility: visible;
  }

  .navbar-item {
    width: 100%;
    justify-content: center;
    padding: 1rem;
    margin: 0.25rem 1rem;
    border-radius: 8px;
  }

  .navbar-burger {
    display: flex;
  }

  .brand-text {
    font-size: 1.1rem;
  }
}

@media (max-width: 480px) {
  .navbar-container {
    height: 3.5rem;
  }

  .navbar-item {
    padding: 0.875rem 1rem;
    font-size: 0.9rem;
  }

  .icon {
    font-size: 1.1em;
  }
}
</style>
