<script setup>
import { MousePointerClick, Play } from 'lucide-vue-next'
import { useAppStore } from '../stores/app.js'
import { useTimersStore } from '../stores/timers.js'

const store = useAppStore()
const timers = useTimersStore()
const emit = defineEmits(['add-record'])

function clearName(name) {
  if (!name) return ''
  const parts = name.split('-')
  return parts[parts.length - 1].trim()
}

function runningCount(task) {
  return timers.timers.filter(t => t.taskId === task.id).length
}

function startTimer(task) {
  timers.startTimer(task)
}
</script>

<template>
  <div class="tasks-grid">
    <!-- Empty state: no project selected -->
    <div v-if="!store.selectedProject && !store.loading" class="empty-state">
      <MousePointerClick :size="32" class="empty-icon" aria-hidden="true" />
      <p>Selecione um projeto na lista lateral</p>
    </div>

    <!-- Skeleton: loading -->
    <div v-else-if="store.loading" class="table-scroll" aria-busy="true">
      <table aria-label="Carregando tarefas">
        <thead>
          <tr>
            <th>Id</th><th>Tarefa</th><th>Horas</th><th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="i in 5" :key="i" class="skeleton-row">
            <td><span class="skeleton" style="width:36px;height:12px;display:block"></span></td>
            <td><span class="skeleton" style="width:180px;height:12px;display:block"></span></td>
            <td><span class="skeleton" style="width:40px;height:12px;display:block"></span></td>
            <td></td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Tasks table -->
    <div v-else class="table-scroll">
      <table>
        <thead>
          <tr>
            <th>Id</th>
            <th>Tarefa</th>
            <th class="num-col">Horas</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="task in store.filteredTasks"
            :key="task.id"
            class="task-row"
            tabindex="0"
            :aria-label="`Tarefa ${task.nome}, clique para adicionar registro`"
            @click="emit('add-record', task)"
            @keydown.enter="emit('add-record', task)"
          >
            <td class="id-col">{{ task.id }}</td>
            <td class="name-col">{{ clearName(task.nome) }}</td>
            <td class="num-col hours-col">{{ task.totalHorasRealizadas }}</td>
            <td class="action-col">
              <div class="action-wrap">
                <button
                  :class="['btn-start', { 'btn-running': runningCount(task) > 0 }]"
                  @click.stop="startTimer(task)"
                  :aria-label="`Iniciar timer para ${task.nome}`"
                >
                  <Play :size="10" aria-hidden="true" />
                  Start
                  <span v-if="runningCount(task)" class="running-badge">{{ runningCount(task) }}</span>
                </button>
                <button
                  class="btn-add"
                  @click.stop="emit('add-record', task)"
                  :aria-label="`Adicionar registro para ${task.nome}`"
                >
                  + Registro
                </button>
              </div>
            </td>
          </tr>
          <tr v-if="!store.filteredTasks.length">
            <td colspan="4" class="empty-row">Nenhuma tarefa neste projeto</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.tasks-grid { flex: 1; display: flex; flex-direction: column; overflow: hidden; }

.empty-state {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  color: var(--slate-400);
  font-size: 14px;
}
.empty-icon { color: var(--slate-300); }

.table-scroll { flex: 1; overflow-x: auto; overflow-y: auto; }

table { width: 100%; border-collapse: collapse; min-width: 400px; }
thead { position: sticky; top: 0; background: var(--slate-50); z-index: 1; }
th {
  padding: 9px 12px;
  text-align: left;
  font-size: 11px;
  font-weight: 600;
  color: var(--slate-500);
  border-bottom: 1px solid var(--slate-200);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  white-space: nowrap;
}
td { padding: 9px 12px; border-bottom: 1px solid var(--slate-100); font-size: 13px; }

.task-row { cursor: pointer; transition: background 0.12s; }
.task-row:hover { background: var(--accent-light); }
.task-row:focus-visible { outline: 2px solid var(--accent); outline-offset: -2px; }
.task-row:hover .btn-add  { opacity: 1; }
.task-row:hover .btn-start { opacity: 1; }

.id-col    { color: var(--slate-400); width: 60px; font-size: 12px; font-variant-numeric: tabular-nums; }
.num-col   { text-align: right; }
.hours-col { color: var(--slate-500); width: 80px; }
.name-col  { max-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.action-col { width: 190px; padding-right: 8px; }
.action-wrap { display: flex; gap: 6px; justify-content: flex-end; align-items: center; }

.btn-add {
  opacity: 0;
  background: var(--accent);
  color: #fff;
  border: none;
  border-radius: 5px;
  padding: 4px 10px;
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
  transition: opacity 0.15s, background 0.15s;
  min-height: unset;
  flex-shrink: 0;
}
.btn-add:hover { background: var(--accent-hover); }

.btn-start {
  opacity: 0;
  display: inline-flex;
  align-items: center;
  gap: 4px;
  background: transparent;
  color: var(--slate-500);
  border: 1px solid var(--slate-300);
  border-radius: 5px;
  padding: 4px 8px;
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
  transition: opacity 0.15s, background 0.15s, color 0.15s;
  min-height: unset;
  flex-shrink: 0;
  white-space: nowrap;
}
.btn-start:hover:not(:disabled) {
  background: #dcfce7;
  color: #16a34a;
  border-color: #86efac;
}
.btn-start.btn-running {
  opacity: 1;
  color: #16a34a;
  border-color: #86efac;
  background: #dcfce7;
}
.task-row:hover .btn-start { opacity: 1; }

.running-badge {
  background: #16a34a;
  color: #fff;
  border-radius: 999px;
  font-size: 10px;
  font-weight: 700;
  padding: 0 5px;
  min-width: 16px;
  text-align: center;
  line-height: 16px;
  height: 16px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

/* Always show buttons on mobile (no hover) */
@media (max-width: 767px) {
  .btn-add { opacity: 1; }
  .btn-start { opacity: 1; }
}

.skeleton-row td { padding: 12px; }
.empty-row { text-align: center; color: var(--slate-400); padding: 40px; font-size: 14px; }
</style>
