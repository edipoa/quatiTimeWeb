import { reactive } from 'vue'

const state = reactive({ toasts: [] })
let nextId = 0

export function useToast() {
  function toast(msg, type = 'success', duration = 3500) {
    const id = ++nextId
    state.toasts.push({ id, msg, type })
    setTimeout(() => dismiss(id), duration)
  }

  function dismiss(id) {
    const idx = state.toasts.findIndex(t => t.id === id)
    if (idx !== -1) state.toasts.splice(idx, 1)
  }

  return { state, toast, dismiss }
}
