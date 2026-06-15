<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  RefreshCw, LogOut, Menu, X,
  LayoutList, ClipboardList, BarChart2
} from 'lucide-vue-next'
import quatiLogo from '../assets/quati.jpg'
import { useAppStore } from '../stores/app.js'
import { auth, clearToken } from '../api/index.js'
import { useTimersStore } from '../stores/timers.js'
import ProjectSidebar from '../components/ProjectSidebar.vue'
import TasksGrid from '../components/TasksGrid.vue'
import RecordsQueue from '../components/RecordsQueue.vue'
import RecordModal from '../components/RecordModal.vue'
import ChatInput from '../components/ChatInput.vue'
import ChartPanel from '../components/ChartPanel.vue'
import TimerBar from '../components/TimerBar.vue'
import ConfirmDialog from '../components/ConfirmDialog.vue'
import ToastStack from '../components/ToastStack.vue'
import { useToast } from '../composables/useToast.js'

const router  = useRouter()
const store   = useAppStore()
const timers  = useTimersStore()
const { toast } = useToast()

const activeTab     = ref('tasks')
const modalOpen     = ref(false)
const pendingTaskId = ref(null)
const pendingTimerId = ref(null)
const prefill       = ref(null)
const sidebarOpen   = ref(false)   // mobile drawer

onMounted(async () => {
  await Promise.all([store.loadTasks(), store.loadRecords()])
})

async function refresh() {
  await store.loadTasks(true)
}

async function logout() {
  await auth.logout()
  clearToken()
  router.push('/login')
}

function openModalForTask(task) {
  pendingTaskId.value = task.id
  prefill.value = null
  modalOpen.value = true
  sidebarOpen.value = false   // close mobile drawer if open
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

function onChatStartTimer(data) {
  if (!data.taskId) return
  const task = store.tasks.find(t => t.id === data.taskId)
  if (task) timers.startTimer(task)
}

function clearName(nome) {
  if (!nome) return ''
  const parts = nome.split('-')
  return parts[parts.length - 1].trim()
}

function onTimerStopped(result) {
  pendingTaskId.value = result.taskId
  pendingTimerId.value = result.id
  prefill.value = {
    description: result.description || clearName(result.taskName) || '',
    time: result.elapsed,
    date: new Date(),
  }
  modalOpen.value = true
}

function onModalClose() {
  if (pendingTimerId.value) {
    timers.resumeTimer(pendingTimerId.value)
    pendingTimerId.value = null
  }
  modalOpen.value = false
}

async function onModalConfirm(payload) {
  try {
    await store.addRecord({
      taskId: pendingTaskId.value ?? 0,
      date: payload.date,
      description: payload.description,
      time: payload.time,
    })
  } catch {
    toast('Erro ao adicionar registro. Tente novamente.', 'error')
    return
  }
  if (pendingTimerId.value) {
    timers.finalizeTimer(pendingTimerId.value)
    pendingTimerId.value = null
  }
  modalOpen.value = false
  activeTab.value = 'records'
  toast('Registro adicionado à fila.')
}

const pendingCount = () => store.records.filter(r => !r.synchronized).length
</script>

<template>
  <!-- Skip link for keyboard users -->
  <a href="#main-content" class="skip-link">Ir para o conteúdo principal</a>

  <div class="layout">
    <!-- Topbar -->
    <header class="topbar" role="banner">
      <div class="topbar-left">
        <!-- Hamburger (mobile only) -->
        <button
          class="hamburger"
          aria-label="Abrir lista de projetos"
          :aria-expanded="sidebarOpen"
          @click="sidebarOpen = true"
        >
          <Menu :size="20" />
        </button>
        <div class="brand-mark" aria-hidden="true">
          <img :src="quatiLogo" alt="" width="28" height="28" />
        </div>
        <span class="app-name">QuatiTime</span>
      </div>

      <div class="topbar-actions">
        <button
          class="topbar-btn"
          :disabled="store.loading"
          aria-label="Atualizar tarefas"
          @click="refresh"
        >
          <RefreshCw :size="15" :class="store.loading ? 'spin' : ''" />
          <span class="btn-label-desktop">Atualizar</span>
        </button>
        <button class="topbar-btn" aria-label="Sair" @click="logout">
          <LogOut :size="15" />
          <span class="btn-label-desktop">Sair</span>
        </button>
      </div>
    </header>

    <!-- Timer bar -->
    <TimerBar @timer-stopped="onTimerStopped" />

    <!-- Body -->
    <div class="body">
      <!-- Desktop sidebar -->
      <div class="sidebar-desktop">
        <ProjectSidebar @select="activeTab = 'tasks'" />
      </div>

      <!-- Mobile sidebar drawer -->
      <Transition name="drawer">
        <div v-if="sidebarOpen" class="mobile-drawer-overlay" @click.self="sidebarOpen = false">
          <div class="mobile-drawer" role="dialog" aria-label="Lista de projetos" aria-modal="true">
            <div class="drawer-header">
              <span class="drawer-title">Projetos</span>
              <button class="close-btn" aria-label="Fechar" @click="sidebarOpen = false">
                <X :size="18" />
              </button>
            </div>
            <ProjectSidebar @select="activeTab = 'tasks'; sidebarOpen = false" />
          </div>
        </div>
      </Transition>

      <!-- Main content -->
      <main id="main-content" class="main">
        <!-- Tabs -->
        <nav class="tabs" role="tablist" aria-label="Seções do app">
          <button
            id="tab-tasks"
            role="tab"
            :aria-selected="activeTab === 'tasks'"
            :class="['tab', { active: activeTab === 'tasks' }]"
            @click="activeTab = 'tasks'"
          >
            <LayoutList :size="15" aria-hidden="true" />
            Tarefas
          </button>
          <button
            id="tab-records"
            role="tab"
            :aria-selected="activeTab === 'records'"
            :class="['tab', { active: activeTab === 'records' }]"
            @click="activeTab = 'records'"
          >
            <ClipboardList :size="15" aria-hidden="true" />
            Registros
            <span v-if="pendingCount()" class="pending-badge" :aria-label="`${pendingCount()} pendentes`">
              {{ pendingCount() }}
            </span>
          </button>
          <button
            id="tab-chart"
            role="tab"
            :aria-selected="activeTab === 'chart'"
            :class="['tab', { active: activeTab === 'chart' }]"
            @click="activeTab = 'chart'"
          >
            <BarChart2 :size="15" aria-hidden="true" />
            Gráfico
          </button>
        </nav>

        <!-- Tab panels -->
        <div
          class="tab-content"
          role="tabpanel"
          :aria-labelledby="`tab-${activeTab}`"
        >
          <TasksGrid v-if="activeTab === 'tasks'" @add-record="openModalForTask" />
          <RecordsQueue v-else-if="activeTab === 'records'" />
          <ChartPanel v-else-if="activeTab === 'chart'" />
        </div>

        <ChatInput @parsed="onChatParsed" @start-timer="onChatStartTimer" />
      </main>
    </div>

    <ConfirmDialog />
    <ToastStack />

    <!-- Record Modal -->
    <RecordModal
      :open="modalOpen"
      :prefill="prefill"
      @close="onModalClose"
      @confirm="onModalConfirm"
    />
  </div>
</template>

<style scoped>
/* ─── Skip link ─── */
.skip-link {
  position: absolute;
  top: -999px; left: 16px;
  background: var(--accent); color: #fff;
  padding: 8px 14px; border-radius: 0 0 6px 6px;
  font-size: 13px; font-weight: 500;
  z-index: 999; text-decoration: none;
}
.skip-link:focus { top: 0; }

/* ─── Layout ─── */
.layout { display: flex; flex-direction: column; height: 100vh; overflow: hidden; }

/* ─── Topbar ─── */
.topbar {
  height: 52px;
  background: var(--slate-800);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
  flex-shrink: 0;
  gap: 12px;
}
.topbar-left { display: flex; align-items: center; gap: 10px; }
.brand-mark {
  width: 32px; height: 32px;
  background: var(--white);
  border-radius: 8px;
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0;
  padding: 3px;
}
.brand-mark img { width: 100%; height: 100%; object-fit: contain; }
.app-name { color: var(--white); font-size: 16px; font-weight: 700; letter-spacing: -0.3px; }
.topbar-actions { display: flex; gap: 6px; }

.topbar-btn {
  background: rgba(255,255,255,0.08);
  color: var(--slate-300);
  border: 1px solid rgba(255,255,255,0.1);
  padding: 6px 12px;
  font-size: 13px;
  min-height: 36px;
  border-radius: 6px;
  transition: background 0.15s, color 0.15s;
}
.topbar-btn:hover:not(:disabled) { background: rgba(255,255,255,0.14); color: var(--white); }
.topbar-btn:disabled { opacity: 0.5; }

/* Hamburger: only visible on mobile */
.hamburger {
  display: none;
  background: rgba(255,255,255,0.08);
  border: 1px solid rgba(255,255,255,0.1);
  color: var(--slate-300);
  padding: 7px;
  border-radius: 6px;
  min-height: 40px; min-width: 40px;
  justify-content: center;
}
@media (max-width: 767px) {
  .hamburger { display: flex; }
  .btn-label-desktop { display: none; }
  .topbar-btn { padding: 7px; min-width: 40px; justify-content: center; }
}

/* ─── Body ─── */
.body { display: flex; flex: 1; overflow: hidden; }

.sidebar-desktop { display: flex; flex-shrink: 0; }
@media (max-width: 767px) { .sidebar-desktop { display: none; } }

/* ─── Mobile drawer ─── */
.mobile-drawer-overlay {
  position: fixed; inset: 0;
  background: rgba(15,23,42,0.5);
  z-index: 40;
  display: flex;
}
.mobile-drawer {
  width: 280px;
  background: var(--slate-800);
  display: flex;
  flex-direction: column;
  height: 100%;
  overflow: hidden;
}
.drawer-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px;
  border-bottom: 1px solid var(--slate-700);
  flex-shrink: 0;
}
.drawer-title { color: var(--white); font-weight: 600; font-size: 15px; }
.close-btn {
  background: rgba(255,255,255,0.08);
  border: none;
  color: var(--slate-400);
  padding: 6px; border-radius: 6px;
  min-height: 36px; min-width: 36px;
  display: flex; align-items: center; justify-content: center;
  cursor: pointer;
}
.close-btn:hover { color: var(--white); background: rgba(255,255,255,0.14); }

/* Drawer animation */
.drawer-enter-active, .drawer-leave-active { transition: opacity 0.2s; }
.drawer-enter-from, .drawer-leave-to { opacity: 0; }
.drawer-enter-active .mobile-drawer, .drawer-leave-active .mobile-drawer { transition: transform 0.2s ease; }
.drawer-enter-from .mobile-drawer, .drawer-leave-to .mobile-drawer { transform: translateX(-100%); }

/* ─── Main ─── */
.main { flex: 1; display: flex; flex-direction: column; overflow: hidden; min-width: 0; }

/* ─── Tabs ─── */
.tabs {
  display: flex;
  border-bottom: 1px solid var(--slate-200);
  background: var(--white);
  flex-shrink: 0;
  overflow-x: auto;
}
.tab {
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  border-radius: 0;
  padding: 11px 18px;
  font-size: 13px;
  font-weight: 500;
  color: var(--slate-500);
  display: flex;
  align-items: center;
  gap: 6px;
  white-space: nowrap;
  min-height: 46px;
  transition: color 0.15s, border-color 0.15s;
  flex-shrink: 0;
}
.tab:hover { color: var(--slate-900); background: var(--slate-50); }
.tab.active { color: var(--accent); border-bottom-color: var(--accent); }

.pending-badge {
  background: var(--amber-bg);
  color: var(--amber);
  border-radius: 999px;
  font-size: 11px;
  font-weight: 600;
  padding: 1px 7px;
  min-width: 20px;
  text-align: center;
}

/* ─── Tab content ─── */
.tab-content { flex: 1; overflow: hidden; display: flex; flex-direction: column; min-height: 0; }

/* ─── Spin ─── */
.spin { animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
@media (prefers-reduced-motion: reduce) { .spin { animation: none; } }
</style>
