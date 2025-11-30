<script setup lang="ts">
import ViewContainer from '@/components/ViewContainer.vue'
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
            <span class="icon">📝</span>
            Give Feedback
          </router-link>
          <router-link to="/analytics" class="btn btn-secondary">
            <span class="icon">📊</span>
            View Analytics
          </router-link>
        </div>
      </header>

      <div class="card">
        <div class="features-section">
          <h2 class="section-title">What We Offer</h2>
          <div class="features-grid">
            <div class="feature-card">
              <div class="feature-icon icon-xl">📝</div>
              <h3>Easy Feedback</h3>
              <p>Share your thoughts quickly and securely with our user-friendly survey form.</p>
              <router-link to="/survey" class="feature-link">Start Survey →</router-link>
            </div>

            <div class="feature-card">
              <div class="feature-icon icon-xl">📊</div>
              <h3>Real-time Analytics</h3>
              <p>Explore live data visualizations and insights from customer feedback.</p>
              <router-link to="/analytics" class="feature-link">View Dashboard →</router-link>
            </div>

            <div class="feature-card">
              <div class="feature-icon icon-xl">🌙</div>
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
  max-width: 1000px;
  margin: 0 auto;
}
</style>
