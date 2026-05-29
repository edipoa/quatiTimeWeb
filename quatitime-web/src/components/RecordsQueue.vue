<script setup>
import { ref, computed } from 'vue'
import { useAppStore } from '../stores/app.js'

const store = useAppStore()
const selected = ref(new Set())
const syncing = ref(false)
const syncResults = ref([])

const totalSelected = computed(() => {
  if (!selected.value.size) return null
  return store.records
    .filter(r => selected.value.has(r.id))
    .reduce((sum, r) => sum + r.time, 0)
    .toFixed(2)
})

function toggleSelect(id) {
  if (selected.value.has(id)) selected.value.delete(id)
  else selected.value.add(id)
  selected.value = new Set(selected.value)
}

async function syncAll() {
  if (!confirm('Deseja realmente sincronizar?')) return
  syncing.value = true
  try {
    syncResults.value = await store.syncRecords()
  } finally {
    syncing.value = false
  }
}

async function removeSelected() {
  if (!selected.value.size) return
  if (!confirm('Apagar os registros selecionados?')) return
  for (const id of selected.value) {
    await store.removeRecord(id)
  }
  selected.value = new Set()
}

async function removeSynced() {
  if (!confirm('Remover todos os registros já sincronizados?')) return
  await store.removeSynced()
}

async function updateField(record, field, value) {
  const payload = {
    date: record.date,
    description: record.description,
    time: record.time,
    [field]: value,
  }
  await store.updateRecord(record.id, payload)
}
</script>

<template>
  <div class="queue-wrap">
    <div class="queue-toolbar">
      <button class="btn-primary" :disabled="syncing" @click="syncAll">
        {{ syncing ? 'Sincronizando…' : 'Sincronizar' }}
      </button>
      <button class="btn-danger" :disabled="!selected.size" @click="removeSelected">
        Remover selecionados
      </button>
      <button class="btn-secondary" @click="removeSynced">
        Limpar sincronizados
      </button>
    </div>

    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            <th style="width:32px"></th>
            <th>Projeto</th>
            <th>Tarefa</th>
            <th>Data</th>
            <th>Descrição</th>
            <th>Horas</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="rec in store.records"
            :key="rec.id"
            :class="{ 'row-selected': selected.has(rec.id) }"
            @click="toggleSelect(rec.id)"
          >
            <td><input type="checkbox" :checked="selected.has(rec.id)" @click.stop="toggleSelect(rec.id)" /></td>
            <td class="text-muted">{{ rec.project }}</td>
            <td>{{ rec.taskName }}</td>
            <td>
              <input
                type="date"
                :value="rec.date?.slice(0, 10)"
                class="inline-input"
                @click.stop
                @change="updateField(rec, 'date', new Date($event.target.value + 'T00:00:00'))"
              />
            </td>
            <td>
              <input
                :value="rec.description"
                class="inline-input"
                @click.stop
                @change="updateField(rec, 'description', $event.target.value)"
              />
            </td>
            <td>
              <input
                type="number"
                step="0.25"
                :value="rec.time"
                class="inline-input hours-input"
                @click.stop
                @change="updateField(rec, 'time', parseFloat($event.target.value))"
              />
            </td>
            <td>
              <span :class="['badge', rec.synchronized ? 'badge-green' : 'badge-amber']">
                {{ rec.synchronized ? 'Sincronizado' : 'Pendente' }}
              </span>
            </td>
          </tr>
          <tr v-if="!store.records.length">
            <td colspan="7" class="empty-row">Nenhum registro na fila</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="queue-footer">
      <span v-if="totalSelected !== null" class="total">
        Total selecionado: <strong>{{ totalSelected }}h</strong>
      </span>
    </div>
  </div>
</template>

<style scoped>
.queue-wrap { display: flex; flex-direction: column; flex: 1; overflow: hidden; }

.queue-toolbar {
  padding: 10px 16px;
  display: flex;
  gap: 8px;
  border-bottom: 1px solid var(--slate-200);
  background: var(--white);
}

.table-wrap { flex: 1; overflow-y: auto; }

table { width: 100%; border-collapse: collapse; }
thead { position: sticky; top: 0; background: var(--slate-50); z-index: 1; }
th {
  padding: 8px 10px;
  text-align: left;
  font-size: 12px;
  font-weight: 500;
  color: var(--slate-500);
  border-bottom: 1px solid var(--slate-200);
}
td {
  padding: 6px 10px;
  border-bottom: 1px solid var(--slate-100);
  font-size: 13px;
}
tr:hover { background: var(--slate-50); }
.row-selected { background: var(--accent-light) !important; }
.text-muted { color: var(--slate-500); font-size: 12px; }

.inline-input {
  border: 1px solid transparent;
  border-radius: 4px;
  padding: 3px 6px;
  background: transparent;
  font-size: 13px;
  width: 100%;
}
.inline-input:hover { border-color: var(--slate-200); }
.inline-input:focus { border-color: var(--accent); box-shadow: 0 0 0 2px var(--accent-light); background: var(--white); }
.hours-input { width: 70px; }

.queue-footer {
  padding: 8px 16px;
  border-top: 1px solid var(--slate-200);
  font-size: 13px;
  color: var(--slate-500);
  min-height: 36px;
}
.total strong { color: var(--slate-900); }
.empty-row { text-align: center; color: var(--slate-500); padding: 32px; }
</style>
