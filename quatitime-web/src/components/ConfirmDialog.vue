<script setup>
import { AlertTriangle, HelpCircle } from 'lucide-vue-next'
import { useConfirm } from '../composables/useConfirm.js'

const { state, accept, cancel } = useConfirm()

function onKey(e) {
  if (!state.open) return
  if (e.key === 'Enter') accept()
  if (e.key === 'Escape') cancel()
}
</script>

<template>
  <Teleport to="body">
    <Transition name="confirm">
      <div
        v-if="state.open"
        class="confirm-overlay"
        role="alertdialog"
        aria-modal="true"
        :aria-labelledby="'confirm-title'"
        :aria-describedby="'confirm-msg'"
        @mousedown.self="cancel"
        @keydown="onKey"
      >
        <div class="confirm-box">
          <div :class="['confirm-icon-wrap', `icon-${state.variant}`]" aria-hidden="true">
            <AlertTriangle v-if="state.variant === 'danger'" :size="22" />
            <HelpCircle v-else :size="22" />
          </div>

          <div class="confirm-content">
            <p id="confirm-title" class="confirm-title">{{ state.title }}</p>
            <p id="confirm-msg" class="confirm-msg">{{ state.message }}</p>
          </div>

          <div class="confirm-actions">
            <button class="btn-secondary" autofocus @click="cancel">Cancelar</button>
            <button :class="['btn-confirm', `btn-confirm-${state.variant}`]" @click="accept">
              {{ state.confirmLabel }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.confirm-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.55);
  backdrop-filter: blur(2px);
  z-index: 100;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;
}

.confirm-box {
  background: var(--white);
  border-radius: 14px;
  box-shadow: 0 20px 60px rgba(0,0,0,0.18), 0 4px 16px rgba(0,0,0,0.08);
  width: 100%;
  max-width: 400px;
  padding: 28px 24px 20px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  text-align: center;
}

.confirm-icon-wrap {
  width: 52px;
  height: 52px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.icon-danger  { background: #fef2f2; color: #dc2626; }
.icon-default { background: #f0f9ff; color: #0284c7; }

.confirm-content { display: flex; flex-direction: column; gap: 6px; }

.confirm-title {
  font-size: 16px;
  font-weight: 600;
  color: var(--slate-900);
  margin: 0;
}

.confirm-msg {
  font-size: 14px;
  color: var(--slate-500);
  line-height: 1.5;
  margin: 0;
}

.confirm-actions {
  display: flex;
  gap: 10px;
  margin-top: 8px;
  width: 100%;
}
.confirm-actions button {
  flex: 1;
  padding: 9px 16px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  min-height: 40px;
}

.btn-confirm {
  border: none;
  cursor: pointer;
  transition: background 0.15s;
}
.btn-confirm-danger  { background: #dc2626; color: #fff; }
.btn-confirm-danger:hover  { background: #b91c1c; }
.btn-confirm-default { background: var(--accent); color: #fff; }
.btn-confirm-default:hover { background: var(--accent-hover); }

/* Transition */
.confirm-enter-active, .confirm-leave-active {
  transition: opacity 0.18s, transform 0.18s;
}
.confirm-enter-from, .confirm-leave-to {
  opacity: 0;
  transform: scale(0.95);
}

@media (prefers-reduced-motion: reduce) {
  .confirm-enter-active, .confirm-leave-active { transition: none; }
}
</style>
