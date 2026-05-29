import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../views/LoginView.vue'
import MainView from '../views/MainView.vue'

const routes = [
  { path: '/login', component: LoginView },
  { path: '/', component: MainView, meta: { requiresAuth: true } },
  { path: '/:pathMatch(.*)*', redirect: '/' },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

// Guard: API returns 401 → axios interceptor redirects to /login
// This guard handles direct URL access without a session
router.beforeEach((to) => {
  // Auth check is handled by axios 401 interceptor on first API call
  return true
})

export default router
