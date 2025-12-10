import { ApplicationInsights } from '@microsoft/applicationinsights-web'
import type { Router } from 'vue-router'

type TelemetryClient = Pick<ApplicationInsights, 'trackPageView' | 'trackEvent'> & {
  loadAppInsights?: () => void
}

let insightsClient: TelemetryClient | null = null
let initialized = false

const createFallbackClient = (): TelemetryClient => {
  const log = (level: 'warn' | 'info', message: string, data?: Record<string, unknown>) => {
    const payload = data ? `${message} ${JSON.stringify(data)}` : message
    const fn = level === 'warn' ? console.warn : console.info
    fn(`[TelemetryFallback] ${payload}`)
  }

  return {
    trackPageView: (data) => log('info', 'Page view', data ?? {}),
    trackEvent: (data) => log('warn', 'Custom event', data?.properties ?? {}),
  }
}

export const initializeTelemetry = (router: Router): void => {
  if (initialized) {
    return
  }

  const connectionString = import.meta.env.VITE_APPINSIGHTS_CONNECTION_STRING
  if (!connectionString) {
    console.warn('[Telemetry] VITE_APPINSIGHTS_CONNECTION_STRING missing; recording events to console fallback.')
    insightsClient = createFallbackClient()
    initialized = true
    return
  }

  insightsClient = new ApplicationInsights({
    config: {
      connectionString,
      enableAutoRouteTracking: true,
      enableUnhandledPromiseRejectionTracking: true,
      disableAjaxTracking: true,
      disableFetchTracking: false,
      samplingPercentage: 100,
    },
  })

  insightsClient.loadAppInsights?.()
  insightsClient.trackPageView({ name: 'initial-page-load' })

  router.afterEach((to) => {
    insightsClient?.trackPageView({
      name: to.name?.toString() ?? to.path,
      uri: to.fullPath,
    })
  })

  initialized = true
}

export const recordFetchTelemetry = (
  name: string,
  method: string,
  statusCode: number,
  durationMs: number,
  success: boolean
): void => {
  if (!insightsClient) {
    insightsClient = createFallbackClient()
  }

  insightsClient.trackEvent({
    name: 'api_call',
    properties: {
      target: name,
      method: method.toUpperCase(),
      statusCode: statusCode.toString(),
      success: success ? 'true' : 'false',
    },
    measurements: {
      durationMs,
    },
  })
}
