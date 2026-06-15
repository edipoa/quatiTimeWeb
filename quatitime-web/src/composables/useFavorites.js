import { ref } from 'vue'

const LS_KEY = 'quatitime_favorites'
const favorites = ref(new Set(JSON.parse(localStorage.getItem(LS_KEY) ?? '[]')))

function persist() {
  localStorage.setItem(LS_KEY, JSON.stringify([...favorites.value]))
}

export function useFavorites() {
  function isFavorite(project) {
    return favorites.value.has(project)
  }

  function toggleFavorite(project) {
    if (favorites.value.has(project)) {
      favorites.value.delete(project)
    } else {
      favorites.value.add(project)
    }
    favorites.value = new Set(favorites.value) // trigger reactivity
    persist()
  }

  return { favorites, isFavorite, toggleFavorite }
}
