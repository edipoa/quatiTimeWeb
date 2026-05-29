<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAppStore } from '../stores/app.js'
import { auth } from '../api/index.js'
import ProjectSidebar from '../components/ProjectSidebar.vue'
import TasksGrid from '../components/TasksGrid.vue'
import RecordsQueue from '../components/RecordsQueue.vue'
import RecordModal from '../components/RecordModal.vue'
import ChatInput from '../components/ChatInput.vue'
import ChartPanel from '../components/ChartPanel.vue'

const router = useRouter()
const store = useAppStore()

const activeTab = ref('tasks')
const modalOpen = ref(false)
const pendingTaskId = ref(null)
const prefill = ref(null)

onMounted(async () => {
  await Promise.all([store.loadTasks(), store.loadRecords()])
})

async function refresh() {
  await store.loadTasks(true)
}

async function logout() {
  await auth.logout()
  router.push('/login')
}

function openModalForTask(task) {
  pendingTaskId.value = task.id
  prefill.value = null
  modalOpen.value = true
}

function onChatParsed(data) {
  pendingTaskId.value = data.taskId ?? null
  prefill.value = {
    description: data.description,
    time: data.time,
    date: data.date,
  }
  modalOpen.value = true
}

async function onModalConfirm(payload) {
  if (!pendingTaskId.value) {
    // Chat parsed but no task matched — user needs to pick
    // For now add with taskId 0 so user can edit in queue
  }
  await store.addRecord({
    taskId: pendingTaskId.value ?? 0,
    date: payload.date,
    description: payload.description,
    time: payload.time,
  })
  modalOpen.value = false
  activeTab.value = 'records'
}
</script>

<template>
  <div class="layout">
    <!-- Topbar -->
    <header class="topbar">
      <span class="app-name">QuatiTime</span>
      <div class="topbar-actions">
        <button class="btn-secondary topbar-btn" :disabled="store.loading" @click="refresh">
          {{ store.loading ? '…' : '↻ Atualizar' }}
        </button>
        <button class="btn-secondary topbar-btn" @click="logout">Sair</button>
      </div>
    </header>

    <!-- Body -->
    <div class="body">
      <ProjectSidebar />

      <div class="main">
        <!-- Tabs -->
        <div class="tabs">
          <button
            :class="['tab', { active: activeTab === 'tasks' }]"
            @click="activeTab = 'tasks'"
          >Tarefas</button>
          <button
            :class="['tab', { active: activeTab === 'records' }]"
            @click="activeTab = 'records'"
          >
            Registros
            <span v-if="store.records.filter(r => !r.synchronized).length" class="tab-count">
              {{ store.records.filter(r => !r.synchronized).length }}
            </span>
          </button>
          <button
            :class="['tab', { active: activeTab === 'chart' }]"
            @click="activeTab = 'chart'"
          >Gráfico</button>
        </div>

        <!-- Tab content -->
        <div class="tab-content">
          <TasksGrid v-if="activeTab === 'tasks'" @add-record="openModalForTask" />
          <RecordsQueue v-else-if="activeTab === 'records'" />
          <ChartPanel v-else-if="activeTab === 'chart'" />
        </div>

        <!-- Chat -->
        <ChatInput @parsed="onChatParsed" />
      </div>
    </div>

    <!-- Record Modal -->
    <RecordModal
      :open="modalOpen"
      :prefill="prefill"
      @close="modalOpen = false"
      @confirm="onModalConfirm"
    />
  </div>
</template>

<style scoped>
.layout { display: flex; flex-direction: column; height: 100vh; overflow: hidden; }

.topbar {
  height: 48px;
  background: var(--slate-800);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
  flex-shrink: 0;
}
.app-name { color: var(--white); font-size: 15px; font-weight: 600; }
.topbar-actions { display: flex; gap: 8px; }
.topbar-btn { font-size: 12px; padding: 4px 12px; }

.body { display: flex; flex: 1; overflow: hidden; }

.main { flex: 1; display: flex; flex-direction: column; overflow: hidden; }

.tabs {
  display: flex;
  gap: 0;
  border-bottom: 1px solid var(--slate-200);
  background: var(--white);
  flex-shrink: 0;
}
.tab {
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  border-radius: 0;
  padding: 10px 20px;
  font-size: 13px;
  font-weight: 500;
  color: var(--slate-500);
  display: flex;
  align-items: center;
  gap: 6px;
}
.tab:hover { color: var(--slate-900); }
.tab.active { color: var(--accent); border-bottom-color: var(--accent); }

.tab-count {
  background: var(--amber-bg);
  color: var(--amber);
  border-radius: 999px;
  font-size: 11px;
  padding: 1px 6px;
}

.tab-content { flex: 1; overflow: hidden; display: flex; flex-direction: column; }
</style>
