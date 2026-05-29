<script setup>
import { ref, onMounted } from 'vue'
import { chart } from '../api/index.js'

const imgUrl = ref('')
const loading = ref(false)
const error = ref('')

async function load() {
  loading.value = true
  error.value = ''
  try {
    // Append timestamp to bust cache
    imgUrl.value = chart.getUrl() + '?t=' + Date.now()
  } finally {
    loading.value = false
  }
}

function onError() {
  error.value = 'Não foi possível carregar o gráfico.'
  imgUrl.value = ''
}

onMounted(load)
</script>

<template>
  <div class="chart-panel">
    <div class="chart-header">
      <span class="chart-title">Gráfico de Horas</span>
      <button class="btn-secondary" :disabled="loading" @click="load">
        {{ loading ? '…' : '↻ Atualizar' }}
      </button>
    </div>
    <div class="chart-body">
      <img v-if="imgUrl" :src="imgUrl" alt="Gráfico de horas" @error="onError" />
      <p v-if="error" class="chart-error">{{ error }}</p>
      <p v-if="!imgUrl && !error" class="chart-empty">Carregando gráfico…</p>
    </div>
  </div>
</template>

<style scoped>
.chart-panel {
  border-top: 1px solid var(--slate-200);
  background: var(--white);
}
.chart-header {
  padding: 10px 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid var(--slate-200);
}
.chart-title { font-size: 12px; font-weight: 600; color: var(--slate-700); text-transform: uppercase; letter-spacing: 0.06em; }
.chart-body { padding: 16px; display: flex; justify-content: center; }
.chart-body img { max-width: 100%; border-radius: 6px; }
.chart-error, .chart-empty { font-size: 13px; color: var(--slate-500); }
</style>
