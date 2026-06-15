<script setup>
import { ref, onMounted } from 'vue'
import { RefreshCw, BarChart2 } from 'lucide-vue-next'
import { api, clearToken } from '../api/index.js'

const imgUrl  = ref('')
const loading = ref(false)
const error   = ref('')
const sessionExpired = ref(false)

function defaultStartDate() {
  const d = new Date()
  d.setDate(1)
  return d.toISOString().slice(0, 10)
}
function defaultEndDate() {
  return new Date().toISOString().slice(0, 10)
}

const startDate = ref(defaultStartDate())
const endDate   = ref(defaultEndDate())

async function load() {
  loading.value = true
  error.value   = ''
  imgUrl.value  = ''
  sessionExpired.value = false
  try {
    const params = { t: Date.now() }
    if (startDate.value) params.startDate = startDate.value
    if (endDate.value)   params.endDate   = endDate.value
    const res = await api.get('/api/chart', { params, responseType: 'blob' })
    imgUrl.value = URL.createObjectURL(res.data)
  } catch (err) {
    const status = err.response?.status
    if (status === 502 || status === 401) {
      sessionExpired.value = true
      error.value = 'Sessão expirada. Faça login novamente.'
    } else {
      error.value = 'Não foi possível carregar o gráfico.'
    }
  } finally {
    loading.value = false
  }
}

function relogin() {
  clearToken()
  window.location.href = '/login'
}

function onError() {
  error.value  = 'Não foi possível carregar o gráfico do portal.'
  imgUrl.value = ''
  loading.value = false
}

onMounted(load)
</script>

<template>
  <div class="chart-panel">
    <div class="chart-header">
      <div class="chart-title-row">
        <BarChart2 :size="15" class="title-icon" aria-hidden="true" />
        <span class="chart-title">Gráfico de Horas</span>
      </div>
      <div class="chart-controls">
        <div class="date-range">
          <label class="date-label" for="chart-start">De</label>
          <input id="chart-start" type="date" class="date-input" v-model="startDate" :disabled="loading" />
          <label class="date-label" for="chart-end">Até</label>
          <input id="chart-end" type="date" class="date-input" v-model="endDate" :disabled="loading" />
        </div>
        <button class="btn-secondary" :disabled="loading" @click="load" aria-label="Recarregar gráfico">
          <RefreshCw :size="14" :class="loading ? 'spin' : ''" aria-hidden="true" />
          Buscar
        </button>
      </div>
    </div>

    <div class="chart-body">
      <div v-if="loading" class="chart-loading" role="status" aria-live="polite" aria-label="Carregando gráfico">
        <div class="chart-loading-spinner"></div>
        <p class="chart-loading-text">Buscando gráfico do portal…</p>
      </div>

      <img
        v-else-if="imgUrl"
        :src="imgUrl"
        alt="Gráfico de horas lançadas no portal"
        class="chart-img"
        loading="lazy"
        @error="onError"
      />

      <div v-else class="chart-empty">
        <BarChart2 :size="36" class="empty-icon" aria-hidden="true" />
        <p>{{ error || 'Clique em Atualizar para carregar o gráfico' }}</p>
        <button v-if="sessionExpired" class="btn-primary" @click="relogin">
          Fazer login novamente
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.chart-panel {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
.chart-header {
  padding: 10px 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid var(--slate-200);
  flex-shrink: 0;
  background: var(--white);
  flex-wrap: wrap;
  gap: 8px;
}
.chart-controls {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}
.date-range {
  display: flex;
  align-items: center;
  gap: 6px;
}
.date-label {
  font-size: 12px;
  color: var(--slate-500);
  white-space: nowrap;
}
.date-input {
  font-size: 12px;
  padding: 4px 6px;
  border: 1px solid var(--slate-200);
  border-radius: 6px;
  background: var(--white);
  color: var(--slate-700);
  cursor: pointer;
  outline: none;
  transition: border-color 0.15s;
}
.date-input:focus {
  border-color: var(--accent);
}
.date-input:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
.chart-title-row { display: flex; align-items: center; gap: 7px; }
.title-icon { color: var(--accent); }
.chart-title {
  font-size: 13px;
  font-weight: 600;
  color: var(--slate-700);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.chart-body {
  flex: 1;
  padding: 20px;
  display: flex;
  align-items: flex-start;
  justify-content: center;
  overflow-y: auto;
}

.chart-img {
  max-width: 100%;
  border-radius: 8px;
  border: 1px solid var(--slate-200);
}

.chart-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 14px;
  padding: 60px 40px;
}
.chart-loading-spinner {
  width: 36px;
  height: 36px;
  border: 3px solid var(--slate-200);
  border-top-color: var(--accent);
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}
.chart-loading-text {
  font-size: 13px;
  color: var(--slate-500);
}
@media (prefers-reduced-motion: reduce) {
  .chart-loading-spinner { animation: none; border-top-color: var(--accent); }
}

.chart-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  color: var(--slate-400);
  text-align: center;
  padding: 40px;
}
.empty-icon { color: var(--slate-300); }
.chart-empty p { font-size: 14px; max-width: 260px; line-height: 1.5; }

.spin { animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
@media (prefers-reduced-motion: reduce) { .spin { animation: none; } }
</style>
