<script setup>
import { useAppStore } from '../stores/app.js'

const store = useAppStore()
const emit = defineEmits(['add-record'])

function clearName(name) {
  if (!name) return ''
  const parts = name.split('-')
  return parts[parts.length - 1].trim()
}
</script>

<template>
  <div class="tasks-grid">
    <div v-if="!store.selectedProject" class="empty-state">
      Selecione um projeto na sidebar para ver as tarefas
    </div>
    <template v-else>
      <table>
        <thead>
          <tr>
            <th>Id</th>
            <th>Tarefa</th>
            <th>Horas</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="task in store.filteredTasks"
            :key="task.id"
            @dblclick="emit('add-record', task)"
          >
            <td class="id-col">{{ task.id }}</td>
            <td>{{ clearName(task.nome) }}</td>
            <td class="hours-col">{{ task.totalHorasRealizadas }}</td>
            <td class="action-col">
              <button class="btn-secondary" @click="emit('add-record', task)">
                + Registro
              </button>
            </td>
          </tr>
          <tr v-if="!store.filteredTasks.length">
            <td colspan="4" class="empty-row">Nenhuma tarefa encontrada</td>
          </tr>
        </tbody>
      </table>
    </template>
  </div>
</template>

<style scoped>
.tasks-grid { flex: 1; overflow-y: auto; }
.empty-state { padding: 40px; text-align: center; color: var(--slate-500); font-size: 13px; }

table { width: 100%; border-collapse: collapse; }
thead { position: sticky; top: 0; background: var(--slate-50); z-index: 1; }
th {
  padding: 8px 12px;
  text-align: left;
  font-size: 12px;
  font-weight: 500;
  color: var(--slate-500);
  border-bottom: 1px solid var(--slate-200);
}
td {
  padding: 8px 12px;
  border-bottom: 1px solid var(--slate-100);
  font-size: 13px;
}
tbody tr:hover { background: var(--slate-50); cursor: pointer; }
tbody tr:hover .action-col button { opacity: 1; }

.id-col { color: var(--slate-500); width: 60px; }
.hours-col { color: var(--slate-500); width: 80px; text-align: right; }
.action-col { width: 100px; text-align: right; }
.action-col button { opacity: 0; transition: opacity 0.15s; font-size: 12px; padding: 4px 10px; }
.empty-row { text-align: center; color: var(--slate-500); padding: 24px; }
</style>
