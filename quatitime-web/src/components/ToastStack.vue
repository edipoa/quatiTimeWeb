<script setup>
import { CheckCircle, XCircle, AlertCircle, X } from 'lucide-vue-next'
import { useToast } from '../composables/useToast.js'

const { state, dismiss } = useToast()

const icons = { success: CheckCircle, error: XCircle, info: AlertCircle }
</script>

<template>
  <Teleport to="body">
    <div class="toast-stack" role="region" aria-label="Notificações" aria-live="polite">
      <TransitionGroup name="toast">
        <div
          v-for="t in state.toasts"
          :key="t.id"
          :class="['toast-item', `toast-${t.type}`]"
          role="status"
        >
          <component :is="icons[t.type] ?? icons.info" :size="16" class="toast-icon" aria-hidden="true" />
          <span class="toast-msg">{{ t.msg }}</span>
          <button class="toast-close" :aria-label="`Fechar notificação`" @click="dismiss(t.id)">
            <X :size="13" />
          </button>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<style scoped>
.toast-stack {
  position: fixed;
  bottom: 24px;
  right: 20px;
  z-index: 200;
  display: flex;
  flex-direction: column;
  gap: 8px;
  pointer-events: none;
  max-width: 340px;
  width: calc(100vw - 40px);
}

.toast-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 11px 12px 11px 14px;
  border-radius: 10px;
  font-size: 13px;
  font-weight: 500;
  box-shadow: 0 4px 20px rgba(0,0,0,0.12), 0 1px 4px rgba(0,0,0,0.06);
  pointer-events: all;
  border: 1px solid transparent;
}

.toast-success {
  background: #f0fdf4;
  color: #15803d;
  border-color: #bbf7d0;
}
.toast-error {
  background: #fef2f2;
  color: #dc2626;
  border-color: #fecaca;
}
.toast-info {
  background: #f0f9ff;
  color: #0369a1;
  border-color: #bae6fd;
}

.toast-icon { flex-shrink: 0; }

.toast-msg { flex: 1; line-height: 1.4; }

.toast-close {
  background: none;
  border: none;
  color: currentColor;
  opacity: 0.5;
  padding: 2px;
  min-height: unset;
  border-radius: 4px;
  cursor: pointer;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: opacity 0.15s;
}
.toast-close:hover { opacity: 1; }

/* TransitionGroup */
.toast-enter-active { transition: opacity 0.2s, transform 0.2s; }
.toast-leave-active { transition: opacity 0.18s, transform 0.18s; }
.toast-enter-from   { opacity: 0; transform: translateY(10px); }
.toast-leave-to     { opacity: 0; transform: translateX(10px); }
.toast-move         { transition: transform 0.2s; }

@media (prefers-reduced-motion: reduce) {
  .toast-enter-active, .toast-leave-active { transition: none; }
}
</style>
