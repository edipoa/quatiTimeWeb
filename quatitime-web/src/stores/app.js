import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { tasks as tasksApi, records as recordsApi } from '../api/index.js'

export const useAppStore = defineStore('app', () => {
  const tasks = ref([])
  const records = ref([])
  const selectedProject = ref(null)
  const loading = ref(false)

  const projects = computed(() =>
    [...new Set(tasks.value.map(t => t.projeto))].sort()
  )

  const filteredTasks = computed(() =>
    selectedProject.value
      ? tasks.value.filter(t => t.projeto === selectedProject.value)
      : []
  )

  async function loadTasks(refresh = false) {
    loading.value = true
    try {
      const { data } = await tasksApi.getAll(refresh)
      tasks.value = data
    } finally {
      loading.value = false
    }
  }

  async function loadRecords() {
    const { data } = await recordsApi.getAll()
    records.value = data
  }

  async function addRecord(payload) {
    await recordsApi.create(payload)
    await loadRecords()
  }

  async function updateRecord(id, payload) {
    await recordsApi.update(id, payload)
    await loadRecords()
  }

  async function removeRecord(id) {
    await recordsApi.remove(id)
    records.value = records.value.filter(r => r.id !== id)
  }

  async function syncRecords() {
    const results = await recordsApi.sync()
    await loadRecords()
    return results.data
  }

  async function removeSynced() {
    await recordsApi.removeSynced()
    records.value = records.value.filter(r => !r.synchronized)
  }

  return {
    tasks, records, selectedProject, loading,
    projects, filteredTasks,
    loadTasks, loadRecords, addRecord, updateRecord, removeRecord, syncRecords, removeSynced,
  }
})
