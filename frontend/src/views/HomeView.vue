<script setup lang="ts">
// No script needed for the home page
</script>

<template>
  <main class="home-page">
    <div class="hero-section">
      <div class="container">
        <div class="hero-content">
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
        </div>
      </div>
    </div>

    <div class="features-section">
      <div class="container">
        <h2 class="section-title">What We Offer</h2>
        <div class="features-grid">
          <div class="feature-card">
            <div class="feature-icon">📝</div>
            <h3>Easy Feedback</h3>
            <p>Share your thoughts quickly and securely with our user-friendly survey form.</p>
            <router-link to="/survey" class="feature-link">Start Survey →</router-link>
          </div>

          <div class="feature-card">
            <div class="feature-icon">📊</div>
            <h3>Real-time Analytics</h3>
            <p>Explore live data visualizations and insights from customer feedback.</p>
            <router-link to="/analytics" class="feature-link">View Dashboard →</router-link>
          </div>

          <div class="feature-card">
            <div class="feature-icon">🌙</div>
            <h3>Dark Mode</h3>
            <p>Switch between light and dark themes for comfortable viewing anytime.</p>
          </div>
        </div>
      </div>
    </div>
  </main>
</template>

<style scoped>
.home-page {
  min-height: 100vh;
  background: var(--color-background);
}

.hero-section {
  background: linear-gradient(135deg, var(--color-primary) 0%, var(--color-primary-hover) 100%);
  color: white;
  padding: 4rem 0;
  text-align: center;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1rem;
}

.hero-content {
  max-width: 600px;
  margin: 0 auto;
}

.hero-title {
  font-size: 3rem;
  font-weight: 700;
  margin-bottom: 1rem;
  line-height: 1.2;
}

.hero-subtitle {
  font-size: 1.25rem;
  margin-bottom: 2rem;
  opacity: 0.9;
  line-height: 1.5;
}

.hero-actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
  flex-wrap: wrap;
}

.btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.875rem 1.5rem;
  border-radius: 8px;
  text-decoration: none;
  font-weight: 600;
  font-size: 1rem;
  transition: all 0.3s ease;
  border: 2px solid transparent;
}

.btn-primary {
  background: white;
  color: var(--color-primary);
  border-color: white;
}

.btn-primary:hover {
  background: rgba(255, 255, 255, 0.9);
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.btn-secondary {
  background: transparent;
  color: white;
  border-color: white;
}

.btn-secondary:hover {
  background: rgba(255, 255, 255, 0.1);
  transform: translateY(-2px);
}

.icon {
  font-size: 1.1em;
}

.features-section {
  padding: 4rem 0;
  background: var(--color-card-background);
}

.section-title {
  text-align: center;
  color: var(--color-heading);
  font-size: 2rem;
  font-weight: 600;
  margin-bottom: 3rem;
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 2rem;
  max-width: 1000px;
  margin: 0 auto;
}

.feature-card {
  background: var(--color-background);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  padding: 2rem;
  text-align: center;
  transition: all 0.3s ease;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.feature-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);
}

.feature-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.feature-card h3 {
  color: var(--color-heading);
  font-size: 1.25rem;
  font-weight: 600;
  margin-bottom: 1rem;
}

.feature-card p {
  color: var(--color-text);
  margin-bottom: 1.5rem;
  line-height: 1.5;
}

.feature-link {
  color: var(--color-primary);
  text-decoration: none;
  font-weight: 600;
  font-size: 0.9rem;
  transition: color 0.3s ease;
}

.feature-link:hover {
  color: var(--color-primary-hover);
}

/* Responsive design */
@media (max-width: 768px) {
  .hero-section {
    padding: 3rem 0;
  }

  .hero-title {
    font-size: 2.25rem;
  }

  .hero-subtitle {
    font-size: 1.1rem;
  }

  .hero-actions {
    flex-direction: column;
    align-items: center;
  }

  .btn {
    width: 100%;
    max-width: 300px;
    justify-content: center;
  }

  .features-section {
    padding: 3rem 0;
  }

  .section-title {
    font-size: 1.75rem;
  }

  .features-grid {
    grid-template-columns: 1fr;
    gap: 1.5rem;
  }

  .feature-card {
    padding: 1.5rem;
  }
}

@media (max-width: 480px) {
  .hero-title {
    font-size: 2rem;
  }

  .hero-subtitle {
    font-size: 1rem;
  }

  .feature-card {
    padding: 1.25rem;
  }

  .feature-icon {
    font-size: 2.5rem;
  }
}
</style>
