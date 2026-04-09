import { ref } from 'vue'

interface ToastMessage {
  id: number
  text: string
  type: 'success' | 'error' | 'info'
}

const toasts = ref<ToastMessage[]>([])
let nextId = 0

export function useToast() {
  function show(text: string, type: ToastMessage['type'] = 'success', duration = 4000) {
    const id = nextId++
    toasts.value.push({ id, text, type })
    setTimeout(() => {
      toasts.value = toasts.value.filter(t => t.id !== id)
    }, duration)
  }

  function success(text: string) { show(text, 'success') }
  function error(text: string) { show(text, 'error', 6000) }
  function info(text: string) { show(text, 'info') }

  return { toasts, show, success, error, info }
}
