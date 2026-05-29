<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { auth } from '../api/index.js'

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
    await auth.login(username.value, password.value)
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
    <form class="login-card" @submit.prevent="submit">
      <h1>QuatiTime</h1>
      <p class="subtitle">Lançamento de horas</p>

      <div class="field">
        <label>Utilizador</label>
        <input v-model="username" autocomplete="username" placeholder="seu.utilizador" />
      </div>
      <div class="field">
        <label>Senha</label>
        <input v-model="password" type="password" autocomplete="current-password" placeholder="••••••••" />
      </div>

      <p v-if="error" class="error">{{ error }}</p>

      <button type="submit" class="btn-primary" :disabled="loading" style="width:100%;padding:10px">
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
}
.login-card {
  background: var(--white);
  border: 1px solid var(--slate-200);
  border-radius: 10px;
  padding: 36px;
  width: 360px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  box-shadow: 0 10px 40px rgba(0,0,0,0.08);
}
h1 { font-size: 22px; font-weight: 600; color: var(--slate-900); }
.subtitle { font-size: 13px; color: var(--slate-500); margin-top: -10px; }
.field { display: flex; flex-direction: column; gap: 5px; }
.field label { font-size: 12px; font-weight: 500; color: var(--slate-700); }
.error { font-size: 13px; color: var(--red); }
</style>
