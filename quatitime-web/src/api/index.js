import axios from 'axios'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5000',
  withCredentials: true,
})

api.interceptors.response.use(
  r => r,
  err => {
    if (err.response?.status === 401) {
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
  getAll: () => api.get('/api/records'),
  create: (data) => api.post('/api/records', data),
  update: (id, data) => api.put(`/api/records/${id}`, data),
  remove: (id) => api.delete(`/api/records/${id}`),
  sync: () => api.post('/api/records/sync'),
  removeSynced: () => api.delete('/api/records/synced'),
}

export const chart = {
  getUrl: () => `${import.meta.env.VITE_API_URL ?? 'http://localhost:5000'}/api/chart`,
}

export const chat = {
  parse: (message) => api.post('/api/chat/parse', { message }),
}
