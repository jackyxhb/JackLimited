<script setup lang="ts">
import ViewContainer from '@/components/ViewContainer.vue'
import { MessageSquareIcon, BarChart3Icon, SunIcon, ArrowRightIcon } from 'lucide-vue-next'
</script>

<template>
  <ViewContainer>
    <div class="content-wrapper">
      <header class="page-header">
        <h1 class="hero-title">Welcome to Jack Limited</h1>
        <p class="hero-subtitle">
          Your feedback helps us build better products and services.
          Share your experience and explore our analytics dashboard.
        </p>
        <div class="hero-actions">
          <router-link to="/survey" class="btn btn-primary">
            <MessageSquareIcon class="icon" />
            Give Feedback
          </router-link>
          <router-link to="/analytics" class="btn btn-secondary">
            <BarChart3Icon class="icon" />
            View Analytics
          </router-link>
        </div>
      </header>

      <div class="card">
        <div class="features-section">
          <h2 class="section-title">What We Offer</h2>
          <div class="features-grid">
            <div class="feature-card">
              <MessageSquareIcon class="feature-icon" />
              <h3>Easy Feedback</h3>
              <p>Share your thoughts quickly and securely with our user-friendly survey form.</p>
              <router-link to="/survey" class="feature-link">Start Survey <ArrowRightIcon class="icon" /></router-link>
            </div>

            <div class="feature-card">
              <BarChart3Icon class="feature-icon" />
              <h3>Real-time Analytics</h3>
              <p>Explore live data visualizations and insights from customer feedback.</p>
              <router-link to="/analytics" class="feature-link">View Dashboard <ArrowRightIcon class="icon" /></router-link>
            </div>

            <div class="feature-card">
              <SunIcon class="feature-icon" />
              <h3>Dark Mode</h3>
              <p>Switch between light and dark themes for comfortable viewing anytime.</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </ViewContainer>
</template>

<style scoped>
.content-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
  width: 100%;
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 var(--spacing-lg);
}

@media (max-width: 768px) {
  .content-wrapper {
    padding: 0 var(--spacing-md);
  }
}
</style>
