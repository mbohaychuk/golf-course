// composables/useApi.ts
export function useApi() {
  const config = useRuntimeConfig()
  const base = config.public.apiBase as string

  function url(path: string): string {
    return `${base}/api${path}`
  }

  function get<T>(path: string, opts?: object) {
    return useFetch<T>(url(path), { ...opts })
  }

  async function post<T>(path: string, body: unknown, headers?: Record<string, string>): Promise<T> {
    return $fetch<T>(url(path), { method: 'POST', body, headers })
  }

  async function put<T>(path: string, body: unknown, headers?: Record<string, string>): Promise<T> {
    return $fetch<T>(url(path), { method: 'PUT', body, headers })
  }

  async function del(path: string, headers?: Record<string, string>): Promise<void> {
    return $fetch(url(path), { method: 'DELETE', headers })
  }

  return { url, get, post, put, del }
}
