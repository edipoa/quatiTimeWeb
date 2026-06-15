<script setup>
import { ref } from 'vue'
import { SendHorizonal, Sparkles, Play, ClipboardList } from 'lucide-vue-next'
import { chat } from '../api/index.js'

const emit = defineEmits(['parsed', 'start-timer'])
const message = ref('')
const loading = ref(false)
const error = ref('')
const parsedResult = ref(null)

async function send() {
  const text = message.value.trim()
  if (!text || loading.value) return
  loading.value = true
  error.value = ''
  parsedResult.value = null
  try {
    const { data } = await chat.parse(text)
    parsedResult.value = data
    message.value = ''
  } catch {
    error.value = 'Não foi possível interpretar a mensagem.'
  } finally {
    loading.value = false
  }
}

function launchNow() {
  emit('parsed', parsedResult.value)
  parsedResult.value = null
}

function startTimer() {
  emit('start-timer', parsedResult.value)
  parsedResult.value = null
}

function dismiss() {
  parsedResult.value = null
}

function onKey(e) {
  if (e.key === 'Enter' && !e.shiftKey) {
    e.preventDefault()
    send()
  }
}

function clearTaskName(nome) {
  if (!nome) return ''
  const parts = nome.split('-')
  return parts[parts.length - 1].trim()
}
</script>

<template>
  <div class="chat-wrap">
    <div class="chat-hint">
      <Sparkles :size="12" aria-hidden="true" />
      <span>Lançamento rápido por linguagem natural</span>
    </div>
    <div class="chat-bar">
      <input
        v-model="message"
        placeholder='Ex: "trabalhei 4h na PBI XYZ ontem"'
        :disabled="loading || !!parsedResult"
        aria-label="Descreva o registro em linguagem natural"
        @keydown="onKey"
      />
      <button
        class="btn-primary send-btn"
        :disabled="loading || !message.trim() || !!parsedResult"
        aria-label="Interpretar e criar rascunho"
        @click="send"
      >
        <SendHorizonal v-if="!loading" :size="16" />
        <span v-else class="spinner" aria-hidden="true"></span>
      </button>
    </div>

    <Transition name="choice">
      <div v-if="parsedResult" class="chat-choice" role="group" aria-label="O que deseja fazer?">
        <span class="choice-task">
          {{ parsedResult.taskName ? clearTaskName(parsedResult.taskName) : 'Tarefa não identificada' }}
          <span v-if="parsedResult.time" class="choice-time">· {{ parsedResult.time }}h</span>
        </span>
        <button
          class="choice-btn choice-record"
          @click="launchNow"
          aria-label="Lançar registro agora"
        >
          <ClipboardList :size="13" aria-hidden="true" />
          Lançar agora
        </button>
        <button
          class="choice-btn choice-timer"
          :disabled="!parsedResult.taskId"
          :title="!parsedResult.taskId ? 'Tarefa não identificada' : undefined"
          @click="startTimer"
          aria-label="Iniciar timer para esta tarefa"
        >
          <Play :size="13" aria-hidden="true" />
          Iniciar timer
        </button>
        <button class="choice-dismiss" @click="dismiss" aria-label="Descartar">✕</button>
      </div>
    </Transition>

    <p v-if="error" class="chat-error" role="alert">{{ error }}</p>
  </div>
</template>

<style scoped>
.chat-wrap {
  border-top: 1px solid var(--slate-200);
  background: var(--white);
  padding: 10px 14px 12px;
  flex-shrink: 0;
}
.chat-hint {
  display: flex;
  align-items: center;
  gap: 5px;
  font-size: 11px;
  color: var(--slate-400);
  margin-bottom: 7px;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}
.chat-hint svg { color: var(--accent); }
.chat-bar { display: flex; gap: 8px; align-items: center; }
.chat-bar input { flex: 1; }
.send-btn {
  min-width: 44px;
  min-height: 44px;
  padding: 0;
  border-radius: 8px;
  flex-shrink: 0;
  justify-content: center;
}
.chat-error { font-size: 12px; color: var(--red); margin-top: 6px; }

.chat-choice {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 8px;
  padding: 8px 10px;
  background: var(--slate-50);
  border: 1px solid var(--slate-200);
  border-radius: 8px;
  flex-wrap: wrap;
}

.choice-task {
  font-size: 12px;
  font-weight: 500;
  color: var(--slate-700);
  flex: 1;
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.choice-time { color: var(--slate-400); font-weight: 400; }

.choice-btn {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  font-size: 12px;
  font-weight: 500;
  padding: 5px 10px;
  border-radius: 6px;
  border: 1px solid transparent;
  cursor: pointer;
  min-height: unset;
  transition: background 0.15s, color 0.15s;
  flex-shrink: 0;
}
.choice-record {
  background: var(--accent);
  color: #fff;
}
.choice-record:hover { background: var(--accent-hover); }

.choice-timer {
  background: #dcfce7;
  color: #16a34a;
  border-color: #86efac;
}
.choice-timer:hover:not(:disabled) { background: #bbf7d0; }
.choice-timer:disabled { opacity: 0.4; cursor: not-allowed; }

.choice-dismiss {
  background: none;
  border: none;
  color: var(--slate-400);
  font-size: 13px;
  padding: 4px 6px;
  min-height: unset;
  cursor: pointer;
  border-radius: 4px;
  line-height: 1;
  flex-shrink: 0;
}
.choice-dismiss:hover { color: var(--slate-700); background: var(--slate-200); }

.choice-enter-active, .choice-leave-active {
  transition: opacity 0.15s, transform 0.15s;
}
.choice-enter-from, .choice-leave-to {
  opacity: 0;
  transform: translateY(-4px);
}

.spinner {
  width: 14px; height: 14px;
  border: 2px solid rgba(255,255,255,0.3);
  border-top-color: #fff;
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }
@media (prefers-reduced-motion: reduce) { .spinner { animation-duration: 0s; } }
</style>
