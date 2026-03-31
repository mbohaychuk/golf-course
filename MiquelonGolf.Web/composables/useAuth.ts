// MiquelonGolf.Web/composables/useAuth.ts
export function useAuth() {
  const config = useRuntimeConfig()
  const base = config.public.apiBase as string

  const token = useCookie<string | null>('admin-token', { default: () => null })
  const role  = useCookie<string | null>('admin-role',  { default: () => null })

  const isAdmin = computed(() => role.value === 'Admin')

  const authHeaders = computed<Record<string, string>>(() =>
    token.value ? { Authorization: `Bearer ${token.value}` } : {}
  )

  async function login(email: string, password: string) {
    const result = await $fetch<{ token: string; userId: string; role: string }>(
      `${base}/api/auth/login`,
      { method: 'POST', body: { email, password } }
    )
    token.value = result.token
    role.value = result.role
    return result
  }

  function logout() {
    token.value = null
    role.value = null
    navigateTo('/admin/login')
  }

  return { token, role, isAdmin, authHeaders, login, logout }
}
