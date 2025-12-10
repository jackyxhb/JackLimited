import { ApplicationInsights } from '@microsoft/applicationinsights-web'
import type { Router } from 'vue-router'

let insightsClient: ApplicationInsights | null = null
let initialized = false

export const initializeTelemetry = (router: Router): void => {
  if (initialized) {
    return
  }

  const connectionString = import.meta.env.VITE_APPINSIGHTS_CONNECTION_STRING
  if (!connectionString) {
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

  insightsClient.loadAppInsights()
  insightsClient.trackPageView()

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
    return
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
