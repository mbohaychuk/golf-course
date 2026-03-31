// tests/composables/useApi.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { ref } from 'vue'
import { mockNuxtImport } from '@nuxt/test-utils/runtime'

// mockNuxtImport is hoisted like vi.mock, so the factory must not reference
// top-level variables. Use a wrapper object instead so the factory captures
// the container by reference, and we swap .impl after hoisting.
const useFetchMock = { impl: vi.fn() }
const fetchMock = { impl: vi.fn() }

mockNuxtImport('useRuntimeConfig', () => () => ({
  public: { apiBase: 'http://api.test' },
}))

mockNuxtImport('useFetch', () => (...args: unknown[]) => useFetchMock.impl(...args))

// $fetch is a Nuxt global (not an auto-import), so use vi.stubGlobal
vi.stubGlobal('$fetch', (...args: unknown[]) => fetchMock.impl(...args))

// Import AFTER mocking
const { useApi } = await import('~/composables/useApi')

describe('useApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    useFetchMock.impl = vi.fn().mockReturnValue({ data: ref([]), error: ref(null), pending: ref(false) })
    fetchMock.impl = vi.fn()
  })

  it('url() prepends apiBase', () => {
    const api = useApi()
    expect(api.url('/announcements')).toBe('http://api.test/api/announcements')
  })

  it('get() calls useFetch with full URL', () => {
    const api = useApi()
    api.get('/announcements')
    // useFetch may be called with extra internal args by Nuxt; check first two only
    const [calledUrl, calledOpts] = useFetchMock.impl.mock.calls[0]
    expect(calledUrl).toBe('http://api.test/api/announcements')
    expect(calledOpts).toEqual(expect.any(Object))
  })

  it('post() calls $fetch with method POST', async () => {
    fetchMock.impl.mockResolvedValue({ id: '1' })
    const api = useApi()
    await api.post('/bookings', { golferName: 'Test' })
    expect(fetchMock.impl).toHaveBeenCalledWith(
      'http://api.test/api/bookings',
      expect.objectContaining({ method: 'POST', body: { golferName: 'Test' } })
    )
  })

  it('del() calls $fetch with method DELETE', async () => {
    fetchMock.impl.mockResolvedValue(undefined)
    const api = useApi()
    await api.del('/bookings/abc')
    expect(fetchMock.impl).toHaveBeenCalledWith(
      'http://api.test/api/bookings/abc',
      expect.objectContaining({ method: 'DELETE' })
    )
  })
})
