<script setup>
import { ref } from 'vue'
import { chat } from '../api/index.js'

const emit = defineEmits(['parsed'])
const message = ref('')
const loading = ref(false)

async function send() {
  const text = message.value.trim()
  if (!text) return
  loading.value = true
  try {
    const { data } = await chat.parse(text)
    emit('parsed', data)
    message.value = ''
  } finally {
    loading.value = false
  }
}

function onKey(e) {
  if (e.key === 'Enter' && !e.shiftKey) {
    e.preventDefault()
    send()
  }
}
</script>

<template>
  <div class="chat-bar">
    <input
      v-model="message"
      placeholder='Ex: "trabalhei 4h na correção monetária"'
      :disabled="loading"
      @keydown="onKey"
    />
    <button class="btn-primary" :disabled="loading || !message.trim()" @click="send">
      {{ loading ? '…' : '↵' }}
    </button>
  </div>
</template>

<style scoped>
.chat-bar {
  display: flex;
  gap: 8px;
  padding: 10px 16px;
  border-top: 1px solid var(--slate-200);
  background: var(--white);
}
.chat-bar input { flex: 1; }
.chat-bar button { min-width: 44px; font-size: 16px; }
</style>
