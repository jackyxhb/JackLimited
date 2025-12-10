import './assets/main.css'
import './assets/global.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import { useThemeStore } from '@/stores/theme'
import { initializeTelemetry } from './telemetry/appInsights'

const app = createApp(App)

initializeTelemetry(router)

app.use(createPinia())
app.use(router)

app.mount('#app')

// Initialize theme after app is mounted
const themeStore = useThemeStore()
themeStore.initTheme()
themeStore.watchSystemTheme()
