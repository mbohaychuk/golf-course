// MiquelonGolf.Web/tests/composables/useAuth.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { ref } from 'vue'
import { mockNuxtImport } from '@nuxt/test-utils/runtime'

// navigateTo is auto-imported by Nuxt (transformed to import from '#app').
// Must use mockNuxtImport so the stub is hoisted into the module system.
// Container object avoids the hoisting closure trap.
const navigateToMock = { fn: vi.fn() }
mockNuxtImport('navigateTo', () => (...args: any[]) => navigateToMock.fn(...args))

const tokenCookie = ref<string | null>(null)
const roleCookie = ref<string | null>(null)

// useCookie is a Nuxt auto-import — use mockNuxtImport so the stub
// is hoisted into the module system before the composable loads.
// The factory cannot close over top-level variables, so we use a
// container object captured by reference.
const cookieMock = {
  impl: (key: string) => {
    if (key === 'admin-token') return tokenCookie
    if (key === 'admin-role') return roleCookie
    return ref(null)
  },
}
mockNuxtImport('useCookie', () => (key: string) => cookieMock.impl(key))

mockNuxtImport('useRuntimeConfig', () => () => ({
  public: { apiBase: 'http://api.test' },
}))

// $fetch is a true Nuxt global (not an auto-import)
const mockFetch = vi.fn()
vi.stubGlobal('$fetch', mockFetch)

const { useAuth } = await import('~/composables/useAuth')

describe('useAuth', () => {
  beforeEach(() => {
    tokenCookie.value = null
    roleCookie.value = null
    vi.clearAllMocks()
  })

  it('isAdmin is false when role is null', () => {
    const { isAdmin } = useAuth()
    expect(isAdmin.value).toBe(false)
  })

  it('isAdmin is true when role is Admin', () => {
    roleCookie.value = 'Admin'
    const { isAdmin } = useAuth()
    expect(isAdmin.value).toBe(true)
  })

  it('isAdmin is false when role is Member', () => {
    roleCookie.value = 'Member'
    const { isAdmin } = useAuth()
    expect(isAdmin.value).toBe(false)
  })

  it('authHeaders returns Bearer token when token is set', () => {
    tokenCookie.value = 'abc123'
    const { authHeaders } = useAuth()
    expect(authHeaders.value).toEqual({ Authorization: 'Bearer abc123' })
  })

  it('authHeaders returns empty object when token is null', () => {
    const { authHeaders } = useAuth()
    expect(authHeaders.value).toEqual({})
  })

  it('login stores token and role from API response', async () => {
    mockFetch.mockResolvedValue({ token: 'tok123', userId: 'u1', role: 'Admin' })
    const { login, token, role } = useAuth()
    await login('admin@test.com', 'Admin1234!')
    expect(token.value).toBe('tok123')
    expect(role.value).toBe('Admin')
  })

  it('login calls correct endpoint with credentials', async () => {
    mockFetch.mockResolvedValue({ token: 't', userId: 'u', role: 'Admin' })
    const { login } = useAuth()
    await login('admin@test.com', 'pw')
    expect(mockFetch).toHaveBeenCalledWith(
      'http://api.test/api/auth/login',
      expect.objectContaining({ method: 'POST', body: { email: 'admin@test.com', password: 'pw' } })
    )
  })

  it('logout clears token and role', () => {
    tokenCookie.value = 'tok123'
    roleCookie.value = 'Admin'
    const { logout } = useAuth()
    logout()
    expect(tokenCookie.value).toBeNull()
    expect(roleCookie.value).toBeNull()
    expect(navigateToMock.fn).toHaveBeenCalledOnce()
    expect(navigateToMock.fn).toHaveBeenCalledWith('/admin/login')
  })
})
