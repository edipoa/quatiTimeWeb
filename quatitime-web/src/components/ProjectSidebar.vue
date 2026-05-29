<script setup>
import { ref, computed } from 'vue'
import { useAppStore } from '../stores/app.js'

const store = useAppStore()
const filter = ref('')

const filtered = computed(() =>
  store.projects.filter(p => p.toUpperCase().includes(filter.value.toUpperCase()))
)

function select(project) {
  store.selectedProject = store.selectedProject === project ? null : project
}
</script>

<template>
  <aside class="sidebar">
    <div class="sidebar-header">
      <span class="sidebar-title">Projetos</span>
      <span v-if="store.loading" class="loading-dot">●</span>
    </div>

    <input
      v-model="filter"
      placeholder="Filtrar projetos…"
      class="sidebar-search"
    />

    <ul class="project-list">
      <li
        v-for="project in filtered"
        :key="project"
        :class="['project-item', { active: store.selectedProject === project }]"
        @click="select(project)"
      >
        {{ project }}
      </li>
      <li v-if="!filtered.length && !store.loading" class="empty">
        Nenhum projeto
      </li>
    </ul>
  </aside>
</template>

<style scoped>
.sidebar {
  width: 240px;
  min-width: 240px;
  background: var(--slate-800);
  display: flex;
  flex-direction: column;
  gap: 0;
  overflow: hidden;
}
.sidebar-header {
  padding: 14px 16px 10px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.sidebar-title {
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--slate-300);
}
.loading-dot { color: var(--amber); font-size: 10px; animation: blink 1s infinite; }
@keyframes blink { 0%,100%{opacity:1} 50%{opacity:0.2} }

.sidebar-search {
  margin: 0 12px 8px;
  width: calc(100% - 24px);
  background: var(--slate-700);
  border-color: transparent;
  color: var(--white);
  font-size: 13px;
  padding: 6px 10px;
}
.sidebar-search::placeholder { color: var(--slate-500); }
.sidebar-search:focus { border-color: var(--accent); }

.project-list {
  list-style: none;
  overflow-y: auto;
  flex: 1;
  padding-bottom: 12px;
}
.project-item {
  padding: 8px 16px;
  color: var(--slate-300);
  font-size: 13px;
  cursor: pointer;
  border-left: 3px solid transparent;
  transition: background 0.12s, color 0.12s;
}
.project-item:hover { background: var(--slate-700); color: var(--white); }
.project-item.active {
  background: var(--accent-light);
  color: var(--accent);
  border-left-color: var(--accent);
  font-weight: 500;
}
.empty { padding: 12px 16px; color: var(--slate-500); font-size: 12px; }
</style>
