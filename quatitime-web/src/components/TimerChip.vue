<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { Square } from 'lucide-vue-next'
import { useTimersStore } from '../stores/timers.js'

const props = defineProps({
  timer: { type: Object, required: true },
})
const emit = defineEmits(['stop'])

const timers = useTimersStore()
const now = ref(Date.now())
const description = ref(props.timer.description ?? '')
let interval

onMounted(() => { interval = setInterval(() => { now.value = Date.now() }, 1000) })
onUnmounted(() => clearInterval(interval))

const isPaused = computed(() => !!props.timer.pausedAt)

const elapsed = computed(() => {
  const end = props.timer.pausedAt ?? now.value
  const ms = Math.max(0, end - props.timer.startedAt)
  const totalSec = Math.floor(ms / 1000)
  const h = Math.floor(totalSec / 3600)
  const m = Math.floor((totalSec % 3600) / 60)
  const s = totalSec % 60
  return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
})

const isLong = computed(() => (Date.now() - props.timer.startedAt) > 8 * 3_600_000)

function clearName(nome) {
  if (!nome) return ''
  const parts = nome.split('-')
  return parts[parts.length - 1].trim()
}

function onDescriptionBlur() {
  timers.updateDescription(props.timer.id, description.value)
}

function onStop() {
  if (isPaused.value) {
    // Timer já está pausado (ex: página foi recarregada com modal aberto).
    // Reabre o modal de confirmação com o tempo pausado.
    const elapsed = Math.max(0.25, Math.round((props.timer.pausedAt - props.timer.startedAt) / 3_600_000 * 4) / 4)
    emit('stop', { ...props.timer, elapsed })
    return
  }
  const result = timers.pauseTimer(props.timer.id)
  if (result) emit('stop', result)
}
</script>

<template>
  <div
    :class="['timer-chip', { 'chip-warn': isLong, 'chip-paused': isPaused }]"
    role="status"
    :aria-label="`Timer ${isPaused ? 'pausado' : 'ativo'}: ${clearName(timer.taskName)}, tempo: ${elapsed}`"
  >
    <span class="chip-dot" aria-hidden="true"></span>
    <span class="chip-name">{{ clearName(timer.taskName) }}</span>
    <input
      v-model="description"
      class="chip-desc"
      placeholder="O que está fazendo?"
      :aria-label="`Descrição do timer: ${clearName(timer.taskName)}`"
      @blur="onDescriptionBlur"
      @click.stop
    />
    <span class="chip-sep" aria-hidden="true"></span>
    <span class="chip-time">{{ elapsed }}</span>
    <button
      class="chip-stop"
      :aria-label="isPaused ? `Aguardando confirmação` : `Parar timer: ${clearName(timer.taskName)}`"
      @click="onStop"
    >
      <Square :size="10" fill="currentColor" aria-hidden="true" />
    </button>
  </div>
</template>

<style scoped>
.timer-chip {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  background: rgba(255,255,255,0.1);
  border: 1px solid rgba(255,255,255,0.18);
  border-radius: 999px;
  padding: 4px 6px 4px 10px;
  font-size: 12px;
  font-weight: 500;
  color: var(--white);
  flex-shrink: 0;
}

.chip-warn {
  background: rgba(245,158,11,0.2);
  border-color: rgba(245,158,11,0.4);
  color: #fbbf24;
}
.chip-paused {
  background: rgba(255,255,255,0.05);
  border-color: rgba(255,255,255,0.1);
  opacity: 0.7;
}

.chip-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: #4ade80;
  flex-shrink: 0;
  animation: pulse 1.5s ease-in-out infinite;
}
.chip-warn   .chip-dot { background: #fbbf24; }
.chip-paused .chip-dot { background: #94a3b8; animation: none; }

.chip-name {
  max-width: 130px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  color: rgba(255,255,255,0.9);
  flex-shrink: 0;
}

.chip-desc {
  background: rgba(255,255,255,0.08);
  border: 1px solid rgba(255,255,255,0.15);
  border-radius: 4px;
  color: rgba(255,255,255,0.85);
  font-size: 11px;
  padding: 2px 7px;
  width: 160px;
  min-height: unset;
  outline: none;
  transition: background 0.15s, border-color 0.15s;
}
.chip-desc::placeholder { color: rgba(255,255,255,0.3); }
.chip-desc:focus {
  background: rgba(255,255,255,0.14);
  border-color: rgba(255,255,255,0.35);
}
.chip-warn .chip-desc {
  background: rgba(245,158,11,0.1);
  border-color: rgba(245,158,11,0.3);
  color: #fbbf24;
}
.chip-warn .chip-desc::placeholder { color: rgba(245,158,11,0.4); }

.chip-sep {
  width: 1px;
  height: 14px;
  background: rgba(255,255,255,0.2);
  flex-shrink: 0;
}

.chip-time {
  font-variant-numeric: tabular-nums;
  color: rgba(255,255,255,0.7);
  font-size: 11px;
  white-space: nowrap;
  flex-shrink: 0;
}
.chip-warn .chip-time { color: #fbbf24; }

.chip-stop {
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(255,255,255,0.12);
  border: none;
  color: rgba(255,255,255,0.7);
  border-radius: 50%;
  width: 22px;
  height: 22px;
  min-height: unset;
  padding: 0;
  cursor: pointer;
  transition: background 0.15s, color 0.15s;
  flex-shrink: 0;
}
.chip-stop:hover {
  background: rgba(239,68,68,0.6);
  color: #fff;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.4; }
}
@media (prefers-reduced-motion: reduce) { .chip-dot { animation: none; } }
</style>
