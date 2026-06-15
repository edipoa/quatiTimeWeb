import axios from 'axios'

const TOKEN_KEY = 'qts_token'

export function getStoredToken() {
  return localStorage.getItem(TOKEN_KEY)
}

export function storeToken(token) {
  localStorage.setItem(TOKEN_KEY, token)
}

export function clearToken() {
  localStorage.removeItem(TOKEN_KEY)
}

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? '',
  withCredentials: true,
})

// Injeta o token como header em toda requisição
api.interceptors.request.use((config) => {
  const token = getStoredToken()
  if (token) config.headers['X-Session-Token'] = token
  return config
})

api.interceptors.response.use(
  r => r,
  err => {
    const status = err.response?.status
    if (status === 401) {
      clearToken()
      window.location.href = '/login'
    }
    return Promise.reject(err)
  }
)

export const auth = {
  login: (username, password) => api.post('/api/auth/login', { username, password }),
  logout: () => api.delete('/api/auth/logout'),
}

export const tasks = {
  getAll: (refresh = false) => api.get('/api/tasks', { params: { refresh } }),
}

export const records = {
  getAll:      ()         => api.get('/api/records'),
  create:      (data)     => api.post('/api/records', data),
  update:      (id, data) => api.put(`/api/records/${id}`, data),
  remove:      (id)       => api.delete(`/api/records/${id}`),
  sync:        ()         => api.post('/api/records/sync'),
  removeSynced: ()        => api.delete('/api/records/synced'),
}

export { api }

export const chart = {
  getUrl: () => `${import.meta.env.VITE_API_URL ?? ''}/api/chart`,
}

export const chat = {
  parse: (message) => api.post('/api/chat/parse', { message }),
}
