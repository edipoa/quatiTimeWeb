<script setup>
import { useTimersStore } from '../stores/timers.js'
import TimerChip from './TimerChip.vue'

const timers = useTimersStore()
const emit = defineEmits(['timer-stopped'])

function onStop(result) {
  emit('timer-stopped', result)
}
</script>

<template>
  <Transition name="bar">
    <div
      v-if="timers.timers.length"
      class="timer-bar"
      role="region"
      aria-label="Timers ativos"
    >
      <TimerChip
        v-for="t in timers.timers"
        :key="t.id"
        :timer="t"
        @stop="onStop"
      />
    </div>
  </Transition>
</template>

<style scoped>
.timer-bar {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 16px;
  background: var(--slate-700);
  border-bottom: 1px solid var(--slate-600);
  flex-shrink: 0;
  overflow-x: auto;
  scrollbar-width: none;
}
.timer-bar::-webkit-scrollbar { display: none; }

.bar-enter-active, .bar-leave-active {
  transition: max-height 0.2s ease, opacity 0.2s ease, padding 0.2s ease;
  overflow: hidden;
}
.bar-enter-from, .bar-leave-to {
  max-height: 0;
  opacity: 0;
  padding-top: 0;
  padding-bottom: 0;
}
.bar-enter-to, .bar-leave-from { max-height: 60px; opacity: 1; }

@media (prefers-reduced-motion: reduce) {
  .bar-enter-active, .bar-leave-active { transition: none; }
}
</style>
