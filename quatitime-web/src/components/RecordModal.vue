<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  open: Boolean,
  prefill: { type: Object, default: null },
})
const emit = defineEmits(['close', 'confirm'])

const description = ref('')
const time = ref('')
const date = ref(today())

watch(() => props.open, (val) => {
  if (val) {
    description.value = props.prefill?.description ?? ''
    time.value = props.prefill?.time != null ? String(props.prefill.time) : ''
    date.value = props.prefill?.date
      ? new Date(props.prefill.date).toISOString().slice(0, 10)
      : today()
  }
})

function today() {
  return new Date().toISOString().slice(0, 10)
}

function addTime(delta) {
  const current = parseFloat(time.value.replace(',', '.')) || 0
  time.value = String(Math.round((current + delta) * 100) / 100)
}

function submit() {
  if (!description.value.trim() || !time.value) return
  const t = parseFloat(time.value.replace(',', '.'))
  if (isNaN(t) || t <= 0) return
  emit('confirm', {
    description: description.value.trim(),
    time: t,
    date: new Date(date.value + 'T00:00:00'),
  })
}

function close() { emit('close') }
</script>

<template>
  <Teleport to="body">
    <div v-if="open" class="overlay" @mousedown.self="close">
      <div class="modal" role="dialog" aria-modal="true">
        <div class="modal-header">
          <span>Novo Registro</span>
          <button class="close-btn" @click="close">✕</button>
        </div>

        <div class="modal-body">
          <div class="field">
            <label>Descrição</label>
            <textarea v-model="description" rows="3" placeholder="O que foi feito?" autofocus />
          </div>

          <div class="field">
            <label>Data</label>
            <input v-model="date" type="date" />
          </div>

          <div class="field">
            <label>Tempo (horas)</label>
            <div class="time-row">
              <input v-model="time" placeholder="Ex: 1.5" style="flex:1" />
              <button type="button" class="btn-secondary" @click="addTime(0.25)">+¼h</button>
              <button type="button" class="btn-secondary" @click="addTime(1)">+1h</button>
            </div>
          </div>
        </div>

        <div class="modal-footer">
          <button class="btn-secondary" @click="close">Cancelar</button>
          <button class="btn-primary" @click="submit">Adicionar à fila</button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<style scoped>
.overlay {
  position: fixed; inset: 0;
  background: rgba(15, 23, 42, 0.4);
  display: flex; align-items: center; justify-content: center;
  z-index: 100;
}
.modal {
  background: var(--white);
  border-radius: 10px;
  width: 480px;
  box-shadow: 0 10px 40px rgba(0,0,0,0.12);
  display: flex; flex-direction: column;
}
.modal-header {
  padding: 16px 20px;
  border-bottom: 1px solid var(--slate-200);
  font-weight: 600;
  font-size: 15px;
  display: flex; justify-content: space-between; align-items: center;
}
.close-btn {
  background: none; border: none; color: var(--slate-500);
  font-size: 14px; padding: 4px 6px; line-height: 1;
}
.close-btn:hover { color: var(--slate-900); }
.modal-body { padding: 20px; display: flex; flex-direction: column; gap: 14px; }
.modal-footer {
  padding: 14px 20px;
  border-top: 1px solid var(--slate-200);
  display: flex; justify-content: flex-end; gap: 8px;
}
.field { display: flex; flex-direction: column; gap: 5px; }
.field label { font-size: 12px; font-weight: 500; color: var(--slate-700); }
.time-row { display: flex; gap: 8px; align-items: center; }
</style>
