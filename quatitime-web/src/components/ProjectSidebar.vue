<script setup>
import { ref, computed } from 'vue'
import { Search, FolderOpen, Star } from 'lucide-vue-next'
import { useAppStore } from '../stores/app.js'
import { useFavorites } from '../composables/useFavorites.js'

const store = useAppStore()
const emit = defineEmits(['select'])
const { isFavorite, toggleFavorite } = useFavorites()
const filter = ref('')

// ─── Resize ───────────────────────────────────────
const MIN_WIDTH = 160
const MAX_WIDTH = 480
const sidebarEl = ref(null)
const width = ref(parseInt(localStorage.getItem('quatitime_sidebar_width') ?? '240'))

function onResizeStart(e) {
  e.preventDefault()
  const startX = e.clientX
  const startW = width.value

  function onMove(e) {
    const next = Math.min(MAX_WIDTH, Math.max(MIN_WIDTH, startW + e.clientX - startX))
    width.value = next
  }

  function onUp() {
    localStorage.setItem('quatitime_sidebar_width', String(width.value))
    document.removeEventListener('mousemove', onMove)
    document.removeEventListener('mouseup', onUp)
    document.body.style.cursor = ''
    document.body.style.userSelect = ''
  }

  document.body.style.cursor = 'col-resize'
  document.body.style.userSelect = 'none'
  document.addEventListener('mousemove', onMove)
  document.addEventListener('mouseup', onUp)
}

const allFiltered = computed(() =>
  store.projects.filter(p => p.toUpperCase().includes(filter.value.toUpperCase()))
)

const favoriteProjects = computed(() =>
  allFiltered.value.filter(p => isFavorite(p))
)

const otherProjects = computed(() =>
  allFiltered.value.filter(p => !isFavorite(p))
)

function select(project) {
  const next = store.selectedProject === project ? null : project
  store.selectedProject = next
  if (next) emit('select')
}

function onToggleFavorite(e, project) {
  e.stopPropagation()
  toggleFavorite(project)
}
</script>

<template>
  <aside ref="sidebarEl" class="sidebar" :style="{ width: width + 'px', minWidth: width + 'px' }" aria-label="Projetos">
    <div class="sidebar-header">
      <span class="sidebar-title">Projetos</span>
    </div>

    <div class="search-wrap">
      <Search :size="14" class="search-icon" aria-hidden="true" />
      <input
        v-model="filter"
        placeholder="Filtrar projetos…"
        class="sidebar-search"
        aria-label="Filtrar projetos"
      />
    </div>

    <!-- Skeleton while loading -->
    <ul v-if="store.loading" class="project-list" aria-busy="true" aria-label="Carregando projetos">
      <li v-for="i in 6" :key="i" class="skeleton-item">
        <span class="skeleton" style="height:13px; width: 100%;"></span>
      </li>
    </ul>

    <ul v-else class="project-list" role="listbox" aria-label="Lista de projetos">
      <!-- Favorites -->
      <template v-if="favoriteProjects.length">
        <li class="list-label">Favoritos</li>
        <li
          v-for="project in favoriteProjects"
          :key="'fav-' + project"
          :class="['project-item', { active: store.selectedProject === project }]"
          role="option"
          :aria-selected="store.selectedProject === project"
          tabindex="0"
          @click="select(project)"
          @keydown.enter="select(project)"
          @keydown.space.prevent="select(project)"
        >
          <FolderOpen :size="13" class="folder-icon" aria-hidden="true" />
          <span class="project-name">{{ project }}</span>
          <button
            class="star-btn star-active"
            :aria-label="`Remover ${project} dos favoritos`"
            @click="onToggleFavorite($event, project)"
          >
            <Star :size="12" fill="currentColor" />
          </button>
        </li>
        <li v-if="otherProjects.length" class="list-divider" aria-hidden="true"></li>
        <li v-if="otherProjects.length" class="list-label">Projetos</li>
      </template>

      <!-- Others -->
      <li
        v-for="project in otherProjects"
        :key="project"
        :class="['project-item', { active: store.selectedProject === project }]"
        role="option"
        :aria-selected="store.selectedProject === project"
        tabindex="0"
        @click="select(project)"
        @keydown.enter="select(project)"
        @keydown.space.prevent="select(project)"
      >
        <FolderOpen :size="13" class="folder-icon" aria-hidden="true" />
        <span class="project-name">{{ project }}</span>
        <button
          class="star-btn"
          :aria-label="`Adicionar ${project} aos favoritos`"
          @click="onToggleFavorite($event, project)"
        >
          <Star :size="12" />
        </button>
      </li>

      <li v-if="!allFiltered.length" class="empty">Nenhum projeto</li>
    </ul>

    <div class="resize-handle" aria-hidden="true" @mousedown="onResizeStart"></div>
  </aside>
</template>

<style scoped>
.sidebar {
  background: var(--slate-800);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  position: relative;
  flex-shrink: 0;
}
.sidebar-header {
  padding: 16px 16px 10px;
  flex-shrink: 0;
}
.sidebar-title {
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--slate-400);
}

.search-wrap {
  position: relative;
  padding: 0 12px 10px;
  flex-shrink: 0;
}
.search-icon {
  position: absolute;
  left: 22px;
  top: 50%;
  transform: translateY(-60%);
  color: var(--slate-500);
  pointer-events: none;
}
.sidebar-search {
  background: var(--slate-700);
  border-color: transparent;
  color: var(--white);
  font-size: 13px;
  padding: 7px 10px 7px 30px;
  width: 100%;
}
.sidebar-search::placeholder { color: var(--slate-500); }
.sidebar-search:focus { border-color: var(--accent); box-shadow: 0 0 0 2px rgba(59,130,246,0.25); }

.project-list { list-style: none; overflow-y: auto; flex: 1; padding-bottom: 12px; }

.project-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 9px 16px;
  color: var(--slate-400);
  font-size: 13px;
  cursor: pointer;
  border-left: 3px solid transparent;
  transition: background 0.12s, color 0.12s;
  min-height: 40px;
  user-select: none;
}
.project-item:hover { background: var(--slate-700); color: var(--white); }
.project-item:focus-visible { outline: 2px solid var(--accent); outline-offset: -2px; }
.project-item.active {
  background: rgba(59, 130, 246, 0.12);
  color: #93C5FD;
  border-left-color: var(--accent);
  font-weight: 500;
}
.folder-icon { flex-shrink: 0; }
.project-name { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; flex: 1; }

.star-btn {
  flex-shrink: 0;
  background: none;
  border: none;
  color: var(--slate-600);
  padding: 2px;
  min-height: unset;
  border-radius: 3px;
  cursor: pointer;
  opacity: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: opacity 0.15s, color 0.15s;
}
.project-item:hover .star-btn { opacity: 1; }
.star-btn:hover { color: #fbbf24; }
.star-btn.star-active {
  opacity: 1;
  color: #fbbf24;
}

.list-label {
  font-size: 10px;
  font-weight: 600;
  letter-spacing: 0.07em;
  text-transform: uppercase;
  color: var(--slate-500);
  padding: 8px 16px 4px;
  pointer-events: none;
}

.list-divider {
  height: 1px;
  background: var(--slate-700);
  margin: 6px 12px;
}

.skeleton-item {
  padding: 8px 16px;
  min-height: 40px;
  display: flex;
  align-items: center;
}

.empty { padding: 12px 16px; color: var(--slate-500); font-size: 13px; }

.resize-handle {
  position: absolute;
  top: 0;
  right: 0;
  width: 4px;
  height: 100%;
  cursor: col-resize;
  background: transparent;
  transition: background 0.15s;
  z-index: 10;
}
.resize-handle:hover,
.resize-handle:active {
  background: var(--accent);
}
</style>
