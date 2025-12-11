import { describe, it, beforeEach, afterEach, expect, vi } from 'vitest'
import type { Router } from 'vue-router'

const loadAppInsightsSpy = vi.fn()
const trackPageViewSpy = vi.fn()
const trackEventSpy = vi.fn()

const ApplicationInsightsCtor = vi.hoisted(() => vi.fn(function ApplicationInsightsStub() {
  this.loadAppInsights = loadAppInsightsSpy
  this.trackPageView = trackPageViewSpy
  this.trackEvent = trackEventSpy
}))

vi.mock('@microsoft/applicationinsights-web', () => ({
  ApplicationInsights: ApplicationInsightsCtor,
}))

describe('appInsights telemetry helpers', () => {
  const routerStub = {
    afterEach: vi.fn(),
  } as unknown as Router

  let warnSpy: ReturnType<typeof vi.spyOn>
  let infoSpy: ReturnType<typeof vi.spyOn>

  const loadModule = async () => {
    const mod = await import('../appInsights')
    return {
      initializeTelemetry: mod.initializeTelemetry,
      recordFetchTelemetry: mod.recordFetchTelemetry,
    }
  }

  beforeEach(() => {
    vi.resetModules()
    vi.unstubAllEnvs()
    routerStub.afterEach.mockReset()
    loadAppInsightsSpy.mockReset()
    trackPageViewSpy.mockReset()
    trackEventSpy.mockReset()
    warnSpy = vi.spyOn(console, 'warn').mockImplementation(() => {})
    infoSpy = vi.spyOn(console, 'info').mockImplementation(() => {})
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('falls back to console logging when connection string is missing', async () => {
    const { initializeTelemetry, recordFetchTelemetry } = await loadModule()

    await initializeTelemetry(routerStub)

    expect(warnSpy).toHaveBeenCalledWith('[Telemetry] VITE_APPINSIGHTS_CONNECTION_STRING missing; recording events to console fallback.')
    expect(routerStub.afterEach).not.toHaveBeenCalled()

    recordFetchTelemetry('/api/example', 'get', 200, 42, true)
    expect(warnSpy).toHaveBeenCalledWith(expect.stringContaining('[TelemetryFallback] Custom event'))
  })

  it('wires up Application Insights when connection string is provided', async () => {
    vi.stubEnv('VITE_APPINSIGHTS_CONNECTION_STRING', 'InstrumentationKey=abc')
    const { initializeTelemetry, recordFetchTelemetry } = await loadModule()

    await initializeTelemetry(routerStub)

    expect(loadAppInsightsSpy).toHaveBeenCalled()
    expect(trackPageViewSpy).toHaveBeenCalledWith({ name: 'initial-page-load' })
    expect(routerStub.afterEach).toHaveBeenCalledTimes(1)

    recordFetchTelemetry('/api/survey', 'POST', 201, 99, true)
    expect(trackEventSpy).toHaveBeenCalledWith({
      name: 'api_call',
      properties: {
        target: '/api/survey',
        method: 'POST',
        statusCode: '201',
        success: 'true',
      },
      measurements: {
        durationMs: 99,
      },
    })
  })
})
