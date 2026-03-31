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

  async function post<T>(path: string, body: unknown): Promise<T> {
    return $fetch<T>(url(path), { method: 'POST', body })
  }

  async function put<T>(path: string, body: unknown): Promise<T> {
    return $fetch<T>(url(path), { method: 'PUT', body })
  }

  async function del(path: string): Promise<void> {
    return $fetch(url(path), { method: 'DELETE' })
  }

  return { url, get, post, put, del }
}
