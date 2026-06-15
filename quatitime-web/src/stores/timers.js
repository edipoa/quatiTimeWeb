import { defineStore } from 'pinia'
import { ref } from 'vue'

const LS_KEY = 'quatitime_active_timers'

export const useTimersStore = defineStore('timers', () => {
  const raw = JSON.parse(localStorage.getItem(LS_KEY) ?? '[]')
  const timers = ref(raw.map(t => {
    const timer = { description: '', ...t }
    // Se o timer ficou pausado ao fechar a página, retoma automaticamente
    if (timer.pausedAt) {
      timer.startedAt += Date.now() - timer.pausedAt
      timer.pausedAt = null
    }
    return timer
  }))

  function persist() {
    localStorage.setItem(LS_KEY, JSON.stringify(timers.value))
  }

  function startTimer(task) {
    timers.value.push({
      id: `timer_${task.id}_${Date.now()}`,
      taskId: task.id,
      taskName: task.nome,
      projectName: task.projeto,
      startedAt: Date.now(),
      pausedAt: null,
      description: '',
    })
    persist()
  }

  function updateDescription(id, description) {
    const timer = timers.value.find(t => t.id === id)
    if (timer) { timer.description = description; persist() }
  }

  function pauseTimer(id) {
    const timer = timers.value.find(t => t.id === id)
    if (!timer || timer.pausedAt) return null
    timer.pausedAt = Date.now()
    persist()
    const elapsed = Math.max(0.25, Math.round((timer.pausedAt - timer.startedAt) / 3_600_000 * 4) / 4)
    return { ...timer, elapsed }
  }

  function resumeTimer(id) {
    const timer = timers.value.find(t => t.id === id)
    if (!timer || !timer.pausedAt) return
    // Shift startedAt forward by the paused duration so elapsed stays correct
    timer.startedAt += Date.now() - timer.pausedAt
    timer.pausedAt = null
    persist()
  }

  function finalizeTimer(id) {
    const idx = timers.value.findIndex(t => t.id === id)
    if (idx === -1) return
    timers.value.splice(idx, 1)
    persist()
  }

  return { timers, startTimer, pauseTimer, resumeTimer, finalizeTimer, updateDescription }
})
