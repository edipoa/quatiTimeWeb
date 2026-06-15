<script setup>
import { ref, computed, reactive } from 'vue'
import { RefreshCw, Trash2, CheckCheck, ClipboardList } from 'lucide-vue-next'
import { useAppStore } from '../stores/app.js'
import { useConfirm } from '../composables/useConfirm.js'
import { useToast } from '../composables/useToast.js'

const store = useAppStore()
const { confirm } = useConfirm()
const { toast } = useToast()
const selected = ref(new Set())
const syncing = ref(false)

// Estado local de projeto selecionado por linha (para dropdowns encadeados)
const rowProject = reactive({}) // { [recordId]: projectName }

function resolveProject(rec) {
  // Prioridade: 1) seleção local em andamento, 2) lookup no store.tasks pelo taskId, 3) campo do servidor
  if (rowProject[rec.id] !== undefined) return rowProject[rec.id]
  if (rec.taskId) {
    const task = store.tasks.find(t => t.id === rec.taskId)
    if (task?.projeto) return task.projeto
  }
  return rec.project !== 'Não localizado' ? rec.project : ''
}

function getRowProject(rec) {
  return resolveProject(rec)
}

const allProjects = computed(() =>
  [...new Set(store.tasks.map(t => t.projeto))].filter(Boolean).sort()
)

function tasksForProject(project) {
  return store.tasks.filter(t => t.projeto === project)
}

function clearTaskName(nome) {
  if (!nome) return ''
  const parts = nome.split('-')
  return parts[parts.length - 1].trim()
}

// ─── Selection ────────────────────────────────────
const totalSelected = computed(() => {
  if (!selected.value.size) return null
  return store.records
    .filter(r => selected.value.has(r.id))
    .reduce((sum, r) => sum + r.time, 0)
    .toFixed(2)
})

function toggleSelect(id) {
  const next = new Set(selected.value)
  next.has(id) ? next.delete(id) : next.add(id)
  selected.value = next
}

// ─── Actions ──────────────────────────────────────
async function syncAll() {
  const ok = await confirm('Deseja realmente sincronizar os registros pendentes?', {
    title: 'Sincronizar registros',
    confirmLabel: 'Sincronizar',
  })
  if (!ok) return
  syncing.value = true
  try {
    const results = await store.syncRecords()
    const synced = results.filter(r => r.success).length
    const err    = results.filter(r => !r.success).length
    if (err === 0) toast(`${synced} registro(s) sincronizado(s) com sucesso.`)
    else toast(`${synced} sincronizado(s), ${err} com erro.`, 'error')
  } finally {
    syncing.value = false
  }
}

async function removeSelected() {
  if (!selected.value.size) return
  const count = selected.value.size
  const ok = await confirm(`Apagar ${count} registro(s) selecionado(s)?`, {
    title: 'Remover registros',
    confirmLabel: 'Remover',
    variant: 'danger',
  })
  if (!ok) return
  for (const id of selected.value) await store.removeRecord(id)
  selected.value = new Set()
  toast(`${count} registro(s) removido(s).`)
}

async function removeSynced() {
  const ok = await confirm('Remover todos os registros já sincronizados da fila?', {
    title: 'Limpar sincronizados',
    confirmLabel: 'Limpar',
    variant: 'danger',
  })
  if (!ok) return
  await store.removeSynced()
  toast('Registros sincronizados removidos.')
}

// ─── Field updates ────────────────────────────────
function buildPayload(rec, overrides = {}) {
  return {
    taskId: overrides.taskId ?? rec.taskId,
    date: overrides.date instanceof Date ? overrides.date : (overrides.date ? new Date(overrides.date + 'T00:00:00') : new Date(rec.date)),
    description: overrides.description ?? rec.description,
    time: overrides.time !== undefined ? parseFloat(overrides.time) || 0 : rec.time,
  }
}

async function updateField(rec, field, rawValue) {
  await store.updateRecord(rec.id, buildPayload(rec, { [field]: rawValue }))
}

async function onProjectChange(rec, project) {
  rowProject[rec.id] = project
  // Limpa tarefa — usuário deve escolher nova
}

async function onTaskChange(rec, taskId) {
  if (!taskId) return
  await store.updateRecord(rec.id, buildPayload(rec, { taskId: Number(taskId) }))
  // Limpa o estado local após o loadRecords completar — agora resolveProject
  // usa o store.tasks para recuperar o projeto a partir do taskId salvo
  delete rowProject[rec.id]
}
</script>

<template>
  <div class="queue-wrap">
    <!-- Toolbar -->
    <div class="toolbar" role="toolbar" aria-label="Ações da fila">
      <button class="btn-primary" :disabled="syncing" @click="syncAll">
        <RefreshCw :size="14" :class="syncing ? 'spin' : ''" aria-hidden="true" />
        {{ syncing ? 'Sincronizando…' : 'Sincronizar' }}
      </button>
      <button class="btn-danger" :disabled="!selected.size" @click="removeSelected"
              :aria-label="`Remover ${selected.size} selecionados`">
        <Trash2 :size="14" aria-hidden="true" />
        <span class="btn-label">Remover</span>
      </button>
      <button class="btn-secondary" @click="removeSynced">
        <CheckCheck :size="14" aria-hidden="true" />
        <span class="btn-label">Limpar sincronizados</span>
      </button>
    </div>

    <!-- Table -->
    <div class="table-wrap">
      <table aria-label="Fila de registros">
        <thead>
          <tr>
            <th style="width:36px"></th>
            <th>Projeto</th>
            <th>Tarefa</th>
            <th>Data</th>
            <th>Descrição</th>
            <th class="num-col">Horas</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="rec in store.records"
            :key="rec.id"
            :class="['rec-row', { 'row-selected': selected.has(rec.id) }]"
            @click="toggleSelect(rec.id)"
          >
            <td>
              <input type="checkbox" :checked="selected.has(rec.id)"
                     :aria-label="`Selecionar registro ${rec.id}`"
                     @click.stop="toggleSelect(rec.id)" />
            </td>

            <!-- Projeto (select sempre visível) -->
            <td @click.stop>
              <select
                class="inline-select"
                :value="getRowProject(rec)"
                :class="{ unset: !getRowProject(rec) }"
                :aria-label="`Projeto do registro ${rec.id}`"
                @change="onProjectChange(rec, $event.target.value)"
              >
                <option value="" disabled>— Projeto —</option>
                <option v-for="p in allProjects" :key="p" :value="p">{{ p }}</option>
              </select>
            </td>

            <!-- Tarefa (select filtrado pelo projeto escolhido) -->
            <td @click.stop>
              <select
                class="inline-select"
                :value="rec.taskId || ''"
                :class="{ unset: !rec.taskId }"
                :aria-label="`Tarefa do registro ${rec.id}`"
                :disabled="!getRowProject(rec)"
                @change="onTaskChange(rec, $event.target.value)"
              >
                <option value="" disabled>— Tarefa —</option>
                <option
                  v-for="t in tasksForProject(getRowProject(rec))"
                  :key="t.id"
                  :value="t.id"
                >{{ clearTaskName(t.nome) }}</option>
              </select>
            </td>

            <!-- Data -->
            <td @click.stop>
              <input
                type="date"
                :value="rec.date?.slice(0, 10)"
                class="inline-input"
                :aria-label="`Data do registro ${rec.id}`"
                @change="updateField(rec, 'date', $event.target.value)"
              />
            </td>

            <!-- Descrição -->
            <td @click.stop>
              <input
                :value="rec.description"
                class="inline-input"
                :aria-label="`Descrição do registro ${rec.id}`"
                @change="updateField(rec, 'description', $event.target.value)"
              />
            </td>

            <!-- Horas -->
            <td class="num-col" @click.stop>
              <input
                type="number" step="0.25" min="0"
                :value="rec.time"
                class="inline-input hours-input"
                :aria-label="`Horas do registro ${rec.id}`"
                @change="updateField(rec, 'time', $event.target.value)"
              />
            </td>

            <!-- Status -->
            <td>
              <span :class="['badge', rec.synchronized ? 'badge-green' : 'badge-amber']">
                {{ rec.synchronized ? 'Sincronizado' : 'Pendente' }}
              </span>
            </td>
          </tr>

          <tr v-if="!store.records.length">
            <td colspan="7" class="empty-row">
              <ClipboardList :size="28" class="empty-icon" aria-hidden="true" />
              <p>Nenhum registro na fila</p>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Footer -->
    <div class="queue-footer" aria-live="polite">
      <span v-if="totalSelected !== null" class="total">
        Total selecionado: <strong>{{ totalSelected }}h</strong>
      </span>
      <span v-else class="total-hint">
        {{ store.records.filter(r => !r.synchronized).length }} pendente(s)
      </span>
    </div>
  </div>
</template>

<style scoped>
.queue-wrap { display: flex; flex-direction: column; flex: 1; overflow: hidden; position: relative; }

.toolbar {
  padding: 10px 14px;
  display: flex; gap: 8px; flex-wrap: wrap;
  border-bottom: 1px solid var(--slate-200);
  background: var(--white);
  flex-shrink: 0;
}
@media (max-width: 480px) { .btn-label { display: none; } }

/* Table */
.table-wrap { flex: 1; overflow-x: auto; overflow-y: auto; }
table { width: 100%; border-collapse: collapse; min-width: 700px; }
thead { position: sticky; top: 0; background: var(--slate-50); z-index: 1; }
th {
  padding: 9px 8px; text-align: left;
  font-size: 11px; font-weight: 600; color: var(--slate-500);
  border-bottom: 1px solid var(--slate-200);
  text-transform: uppercase; letter-spacing: 0.05em; white-space: nowrap;
}
td { padding: 6px 8px; border-bottom: 1px solid var(--slate-100); font-size: 13px; }
.num-col { text-align: right; }

.rec-row { cursor: pointer; transition: background 0.1s; }
.rec-row:hover { background: var(--slate-50); }
.row-selected { background: var(--accent-light) !important; }

input[type="checkbox"] {
  width: 16px; height: 16px; min-height: unset;
  cursor: pointer; accent-color: var(--accent);
}

/* Inline inputs */
.inline-input {
  border: 1px solid transparent; border-radius: 4px;
  padding: 4px 6px; background: transparent;
  font-size: 13px; width: 100%; min-height: unset;
}
.inline-input:hover  { border-color: var(--slate-200); background: var(--white); }
.inline-input:focus  { border-color: var(--accent); box-shadow: 0 0 0 2px var(--accent-light); background: var(--white); }
.hours-input { width: 70px; }

/* Inline selects */
.inline-select {
  border: 1px solid transparent; border-radius: 4px;
  padding: 4px 6px; background: transparent;
  font-size: 13px; width: 100%; min-height: unset;
  color: var(--slate-900); cursor: pointer;
  font-family: inherit;
  max-width: 160px;
  overflow: hidden; text-overflow: ellipsis;
}
.inline-select:hover  { border-color: var(--slate-200); background: var(--white); }
.inline-select:focus  { border-color: var(--accent); box-shadow: 0 0 0 2px var(--accent-light); background: var(--white); outline: none; }
.inline-select:disabled { opacity: 0.4; cursor: not-allowed; }

/* Highlight selects that need to be filled */
.inline-select.unset {
  color: var(--amber);
  border-color: var(--amber-bg);
  background: var(--amber-bg);
}
.inline-select.unset:hover { border-color: var(--amber); }

/* Footer */
.queue-footer {
  padding: 8px 16px; border-top: 1px solid var(--slate-200);
  font-size: 13px; color: var(--slate-500); min-height: 36px;
  display: flex; align-items: center; flex-shrink: 0; background: var(--white);
}
.total strong { color: var(--slate-900); }
.total-hint { color: var(--slate-400); }

.empty-row { text-align: center; color: var(--slate-400); padding: 48px; }
.empty-row p { margin-top: 10px; font-size: 14px; }
.empty-icon { margin: 0 auto; color: var(--slate-300); }

.spin { animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
@media (prefers-reduced-motion: reduce) { .spin { animation: none; } }
</style>
