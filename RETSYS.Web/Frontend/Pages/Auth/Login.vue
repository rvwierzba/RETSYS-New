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
            v-model="form.Email" 
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
            v-model="form.Senha" 
            type="password" 
            placeholder="••••••••" 
            class="w-full rounded-xl border-slate-200 text-sm py-3 focus:border-teal-500 focus:ring-teal-500 placeholder:text-slate-300"
            required 
          />
        </div>

        <div class="flex items-center gap-2 pt-1">
          <input type="checkbox" id="lembrar" class="rounded border-slate-300 text-teal-600 focus:ring-teal-500" />
          <label for="lembrar" class="text-xs text-slate-500 font-medium cursor-pointer select-none">Lembrar deste dispositivo</label>
        </div>

        <button 
          type="submit" 
          :disabled="form.processing"
          class="w-full bg-teal-600 hover:bg-teal-700 text-white font-bold py-3.5 rounded-xl shadow-sm hover:shadow transition text-sm flex items-center justify-center gap-2 disabled:opacity-50"
        >
          <span v-if="form.processing">Verificando...</span>
          <span v-else>Entrar no Sistema</span>
        </button>

        <p class="text-center text-xs text-slate-400 pt-2">
          Não possui uma conta? 
          <Link href="/cadastro" class="font-bold text-slate-950 hover:text-slate-700 ml-1 transition">
            Criar Conta
          </Link>
        </p>
      </form>

    </div>
  </div>
</template>

<script setup>
import { Link, useForm } from '@inertiajs/vue3'

// CORRIGIDO: Utilizando useForm para gerenciar dados e estados de envio automaticamente
const form = useForm({
  Email: '',
  Senha: ''
})

// CORRIGIDO: De 'ejecutarLogin' para 'executarLogin' (com X)
const executarLogin = () => {
  // Dispara o formulário reativo direto para o endpoint do C#
  form.post('/login', {
    onError: () => {
      form.reset('Senha')
    }
  })
}
</script>