<script setup>
import { ref, watch } from 'vue'
import { X, Plus, Minus } from 'lucide-vue-next'

const props = defineProps({
  open: Boolean,
  prefill: { type: Object, default: null },
})
const emit = defineEmits(['close', 'confirm'])

const description = ref('')
const time = ref(0)
const date = ref(today())
const descriptionEl = ref(null)
const submitting = ref(false)

watch(() => props.open, (val) => {
  if (val) {
    description.value = props.prefill?.description ?? ''
    time.value = props.prefill?.time != null ? Number(props.prefill.time) : 0
    date.value = props.prefill?.date
      ? new Date(props.prefill.date).toISOString().slice(0, 10)
      : today()
    submitting.value = false
    setTimeout(() => descriptionEl.value?.focus(), 50)
  }
})

function today() {
  return new Date().toISOString().slice(0, 10)
}

function addTime(delta) {
  time.value = Math.round(Math.max(0, (Number(time.value) || 0) + delta) * 100) / 100
}

const isValid = () => description.value.trim() && Number(time.value) > 0

function submit() {
  if (!isValid() || submitting.value) return
  submitting.value = true
  emit('confirm', {
    description: description.value.trim(),
    time: Number(time.value),
    date: new Date(date.value + 'T00:00:00'),
  })
}

function onKey(e) {
  if (e.key === 'Escape') emit('close')
}
</script>

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="open"
        class="overlay-bg"
        role="dialog"
        aria-modal="true"
        aria-labelledby="modal-title"
        @mousedown.self="emit('close')"
        @keydown="onKey"
      >
        <div class="modal">
          <div class="modal-header">
            <h2 id="modal-title">Novo Registro</h2>
            <button
              class="close-btn"
              @click="emit('close')"
              aria-label="Fechar modal"
            >
              <X :size="16" />
            </button>
          </div>

          <div class="modal-body">
            <div class="field">
              <label for="rec-desc">Descrição</label>
              <textarea
                id="rec-desc"
                ref="descriptionEl"
                v-model="description"
                rows="3"
                placeholder="O que foi feito?"
              />
            </div>

            <div class="field">
              <label for="rec-date">Data</label>
              <input id="rec-date" v-model="date" type="date" />
            </div>

            <div class="field">
              <label for="rec-time">Tempo (horas)</label>
              <div class="time-row">
                <button
                  type="button"
                  class="btn-secondary time-btn"
                  @click="addTime(-0.25)"
                  aria-label="Remover 15 minutos"
                >
                  <Minus :size="14" />
                </button>
                <input
                  id="rec-time"
                  v-model.number="time"
                  type="number"
                  step="0.25"
                  min="0"
                  placeholder="0"
                  style="text-align:center"
                />
                <button
                  type="button"
                  class="btn-secondary time-btn"
                  @click="addTime(0.25)"
                  aria-label="Adicionar 15 minutos"
                >
                  <Plus :size="14" />
                </button>
                <button
                  type="button"
                  class="btn-secondary"
                  @click="addTime(1)"
                  aria-label="Adicionar 1 hora"
                >
                  +1h
                </button>
              </div>
            </div>
          </div>

          <div class="modal-footer">
            <button class="btn-secondary" :disabled="submitting" @click="emit('close')">Cancelar</button>
            <button class="btn-primary" :disabled="!isValid() || submitting" @click="submit">
              <span v-if="submitting" class="modal-spinner" aria-hidden="true"></span>
              {{ submitting ? 'Adicionando…' : 'Adicionar à fila' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal {
  background: var(--white);
  border-radius: 12px;
  width: 100%;
  max-width: 460px;
  margin: 16px;
  display: flex;
  flex-direction: column;
}
.modal-header {
  padding: 18px 20px 14px;
  border-bottom: 1px solid var(--slate-200);
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.modal-header h2 { font-size: 16px; font-weight: 600; color: var(--slate-900); }
.close-btn {
  background: none;
  border: none;
  color: var(--slate-400);
  padding: 6px;
  border-radius: 6px;
  min-height: 32px;
  display: flex; align-items: center; justify-content: center;
  transition: background 0.12s, color 0.12s;
}
.close-btn:hover { background: var(--slate-100); color: var(--slate-700); }

.modal-body  { padding: 20px; display: flex; flex-direction: column; gap: 16px; }
.modal-footer {
  padding: 14px 20px;
  border-top: 1px solid var(--slate-200);
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}

.field { display: flex; flex-direction: column; gap: 6px; }
.field label { font-size: 13px; font-weight: 500; color: var(--slate-700); }

textarea { resize: vertical; min-height: 80px; line-height: 1.5; }

.time-row { display: flex; gap: 8px; align-items: center; }
.time-row input { flex: 1; }
.time-btn { padding: 7px 10px; }

.modal-footer .btn-primary { display: flex; align-items: center; gap: 7px; }

.modal-spinner {
  width: 13px; height: 13px;
  border: 2px solid rgba(255,255,255,0.3);
  border-top-color: #fff;
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
  flex-shrink: 0;
}
@keyframes spin { to { transform: rotate(360deg); } }
@media (prefers-reduced-motion: reduce) { .modal-spinner { animation: none; } }

/* Modal transition */
.modal-enter-active, .modal-leave-active { transition: opacity 0.2s, transform 0.2s; }
.modal-enter-from, .modal-leave-to { opacity: 0; transform: scale(0.96); }
</style>
