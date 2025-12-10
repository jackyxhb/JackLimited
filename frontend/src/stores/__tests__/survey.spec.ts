import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useSurveyStore } from '../survey'

const recordFetchTelemetryMock = vi.hoisted(() => vi.fn())
const nativeFetch = globalThis.fetch
type GlobalWithFetch = typeof globalThis & { fetch?: typeof globalThis.fetch }

vi.mock('@/telemetry/appInsights', () => ({
  recordFetchTelemetry: recordFetchTelemetryMock,
}))

let timeoutSpy: ReturnType<typeof vi.spyOn> | null = null

describe('useSurveyStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.stubEnv('VITE_API_BASE_URL', 'http://localhost:5264')
    recordFetchTelemetryMock.mockReset()
    timeoutSpy = vi.spyOn(globalThis, 'setTimeout').mockImplementation((cb: Parameters<typeof setTimeout>[0]) => {
      if (typeof cb === 'function') {
        cb()
      }
      return 0 as unknown as ReturnType<typeof setTimeout>
    })
  })

  afterEach(() => {
    vi.resetModules()
    vi.unstubAllEnvs()
    timeoutSpy?.mockRestore()
    timeoutSpy = null
    vi.restoreAllMocks()
    if (nativeFetch) {
      globalThis.fetch = nativeFetch
    } else {
      Reflect.deleteProperty(globalThis as GlobalWithFetch, 'fetch')
    }
  })

  it('fetchNps updates cached metrics and clears previous errors', async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      new Response(JSON.stringify({ nps: 42 }), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      })
    )
    globalThis.fetch = fetchMock as typeof globalThis.fetch

    const store = useSurveyStore()
    await store.fetchNps()

    expect(store.nps).toBe(42)
    expect(store.npsError).toBeNull()
    expect(recordFetchTelemetryMock).toHaveBeenCalledWith(
      '/api/survey/nps',
      'GET',
      200,
      expect.any(Number),
      true
    )
  })

  it('submitSurvey captures server-side failures and exposes friendly messages', async () => {
    const fetchMock = vi.fn().mockRejectedValue(new Error('500: Boom'))
    globalThis.fetch = fetchMock as typeof globalThis.fetch

    const store = useSurveyStore()

    await expect(store.submitSurvey({ likelihoodToRecommend: 7 })).rejects.toThrow('500')
    expect(store.submitError).toBe('Server error occurred. Our team has been notified. Please try again later.')
  })
})
