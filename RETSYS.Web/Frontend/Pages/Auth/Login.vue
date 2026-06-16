<template>
  <div class="min-h-screen bg-slate-50 flex items-center justify-center p-4 font-sans selection:bg-teal-500 selection:text-white">
    <div class="w-full max-w-md bg-white rounded-3xl border border-slate-200 shadow-xl p-8 space-y-6">
      
      <div class="text-center space-y-2">
        <span class="text-3xl font-black tracking-wider text-slate-950 font-mono">
          R<span class="text-teal-600">E</span>TSYS<span class="text-teal-600">.</span>
        </span>
        <p class="text-xs font-semibold uppercase text-slate-400 tracking-widest">Gerenciador de Ótica</p>
      </div>

      <div v-if="$page.props.erro" class="p-4 bg-red-50 border border-red-200 text-red-700 rounded-xl text-sm font-medium animate-shake">
        {{ $page.props.erro }}
      </div>

      <form @submit.prevent="executarLogin" class="space-y-4">
        <div>
          <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">E-mail Corporativo</label>
          <input 
            v-model="formulario.Email" 
            type="email" 
            placeholder="nome@otica.com" 
            class="w-full rounded-xl border-slate-200 text-sm py-3 focus:border-teal-500 focus:ring-teal-500 placeholder:text-slate-300"
            required 
          />
        </div>

        <div>
          <div class="flex items-center justify-between mb-2">
            <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider">Senha de Acesso</label>
            <a href="#" class="text-xs font-semibold text-teal-600 hover:text-teal-700 transition">Esqueceu a senha?</a>
          </div>
          <input 
            v-model="formulario.Senha" 
            type="password" 
            placeholder="••••••••" 
            class="w-full rounded-xl border-slate-200 text-sm py-3 focus:border-teal-500 focus:ring-teal-500 placeholder:text-slate-300"
            required 
          />
        </div>

        <div class="flex items-center gap-2 pt-1">
          <input type="checkbox" id="lembrar" class="rounded border-slate-300 text-teal-600 focus:ring-teal-500" />
          <label type="checkbox" for="lembrar" class="text-xs text-slate-500 font-medium cursor-pointer select-none">Lembrar deste dispositivo</label>
        </div>

        <button 
          type="submit" 
          :disabled="carregando"
          class="w-full bg-teal-600 hover:bg-teal-700 text-white font-bold py-3.5 rounded-xl shadow-sm hover:shadow transition text-sm flex items-center justify-center gap-2 disabled:opacity-50"
        >
          <span v-if="carregando">Verificando...</span>
          <span v-else>Entrar no Sistema</span>
        </button>
      </form>

    </div>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { router } from '@inertiajs/vue3'

const carregando = ref(false)

// Payload reativo mapeado para o DtoLogin do C#
const formulario = reactive({
  Email: '',
  Senha: ''
})

const executarLogin = () => {
  carregando.value = true
  
  // Dispara o payload via POST direto para o endpoint do AutenticacaoController
  router.post('/login', formulario, {
    onFinish: () => {
      carregando.value = false
    }
  })
}
</script>