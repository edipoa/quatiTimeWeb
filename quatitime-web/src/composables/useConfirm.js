import { reactive } from 'vue'

const state = reactive({
  open: false,
  title: '',
  message: '',
  confirmLabel: 'Confirmar',
  variant: 'default', // 'default' | 'danger'
  resolve: null,
})

export function useConfirm() {
  function confirm(message, { title = 'Confirmar', confirmLabel = 'Confirmar', variant = 'default' } = {}) {
    return new Promise((resolve) => {
      state.open = true
      state.message = message
      state.title = title
      state.confirmLabel = confirmLabel
      state.variant = variant
      state.resolve = resolve
    })
  }

  function accept() {
    state.open = false
    state.resolve?.(true)
    state.resolve = null
  }

  function cancel() {
    state.open = false
    state.resolve?.(false)
    state.resolve = null
  }

  return { state, confirm, accept, cancel }
}
