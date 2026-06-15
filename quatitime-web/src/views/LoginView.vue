<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { LogIn, AlertCircle } from 'lucide-vue-next'
import { auth, storeToken } from '../api/index.js'
import quatiLogo from '../assets/quati.png'

const router = useRouter()
const username = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

async function submit() {
  if (!username.value || !password.value) return
  loading.value = true
  error.value = ''
  try {
    const { data } = await auth.login(username.value, password.value)
    storeToken(data.token)
    router.push('/')
  } catch {
    error.value = 'Credenciais inválidas ou portal inacessível.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-wrap">
    <form class="login-card" @submit.prevent="submit" novalidate>
      <div class="brand">
        <div class="brand-icon">
          <img :src="quatiLogo" alt="Quati" width="52" height="52" />
        </div>
        <div>
          <h1>QuatiTime</h1>
          <p class="subtitle">Lançamento de horas</p>
        </div>
      </div>

      <div class="field">
        <label for="username">Utilizador</label>
        <input
          id="username"
          v-model="username"
          autocomplete="username"
          placeholder="seu.utilizador"
          :disabled="loading"
        />
      </div>
      <div class="field">
        <label for="password">Senha</label>
        <input
          id="password"
          v-model="password"
          type="password"
          autocomplete="current-password"
          placeholder="••••••••"
          :disabled="loading"
        />
      </div>

      <div v-if="error" class="error-box" role="alert">
        <AlertCircle :size="15" />
        {{ error }}
      </div>

      <button type="submit" class="btn-primary submit-btn" :disabled="loading">
        <LogIn v-if="!loading" :size="16" />
        <span class="spinner" v-if="loading" aria-hidden="true"></span>
        {{ loading ? 'Entrando…' : 'Entrar' }}
      </button>
    </form>
  </div>
</template>

<style scoped>
.login-wrap {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--slate-50);
  padding: 16px;
}
.login-card {
  background: var(--white);
  border: 1px solid var(--slate-200);
  border-radius: 12px;
  padding: 32px;
  width: 100%;
  max-width: 380px;
  display: flex;
  flex-direction: column;
  gap: 18px;
}
.brand { display: flex; align-items: center; gap: 12px; margin-bottom: 4px; }
.brand-icon {
  width: 64px; height: 64px;
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0;
}
.brand-icon img {
  width: 100%; height: 100%;
  object-fit: contain;
}
h1 { font-size: 20px; font-weight: 700; color: var(--slate-900); }
.subtitle { font-size: 13px; color: var(--slate-500); margin-top: 1px; }
.field { display: flex; flex-direction: column; gap: 6px; }
.field label { font-size: 13px; font-weight: 500; color: var(--slate-700); }
.error-box {
  display: flex; align-items: center; gap: 7px;
  font-size: 13px; color: var(--red);
  background: var(--red-bg);
  border: 1px solid #FECACA;
  border-radius: 6px;
  padding: 10px 12px;
}
.submit-btn { width: 100%; justify-content: center; padding: 11px; font-size: 14px; }
.spinner {
  width: 14px; height: 14px;
  border: 2px solid rgba(255,255,255,0.3);
  border-top-color: #fff;
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }
</style>
