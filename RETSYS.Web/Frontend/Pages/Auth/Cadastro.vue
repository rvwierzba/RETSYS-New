<template>
  <div class="min-h-screen bg-slate-50 flex items-center justify-center p-4 font-sans antialiased selection:bg-teal-500 selection:text-white">
    <div class="w-full max-w-md bg-white rounded-3xl border border-slate-200 shadow-xl p-8 space-y-6">
      
      <div class="text-center space-y-2">
        <span class="text-3xl font-black tracking-wider text-slate-950 font-mono">
          R<span class="text-teal-600">E</span>TSYS<span class="text-teal-600">.</span>
        </span>
        <h2 class="text-xl font-bold text-slate-900 tracking-tight">Comece a gerenciar sua ótica</h2>
        <p class="text-xs text-slate-400">Crie sua conta administradora gratuitamente.</p>
      </div>

      <div v-if="$page.props.erro" class="p-4 bg-red-50 border border-red-200 text-red-700 rounded-xl text-sm font-medium">
        {{ $page.props.erro }}
      </div>

      <form @submit.prevent="executarCadastro" class="space-y-4">
        <div>
          <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-1.5">Seu Nome Completo</label>
          <input v-model="form.Nome" type="text" placeholder="Ex: Carlos Silva" class="w-full rounded-xl border-slate-200 text-sm py-3 focus:border-teal-500 focus:ring-teal-500" required />
        </div>

        <div>
          <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-1.5">Nome da Ótica / Empresa</label>
          <input v-model="form.NomeDaOtica" type="text" placeholder="Ex: Ótica Visão Central" class="w-full rounded-xl border-slate-200 text-sm py-3 focus:border-teal-500 focus:ring-teal-500" required />
        </div>

        <div>
          <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-1.5">E-mail de Acesso</label>
          <input v-model="form.Email" type="email" placeholder="gestao@suaotica.com" class="w-full rounded-xl border-slate-200 text-sm py-3 focus:border-teal-500 focus:ring-teal-500" required />
        </div>

        <div>
          <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-1.5">Senha de Segurança</label>
          <input v-model="form.Senha" type="password" placeholder="Mínimo 6 caracteres" class="w-full rounded-xl border-slate-200 text-sm py-3 focus:border-teal-500 focus:ring-teal-500" required />
        </div>

        <button 
          type="submit" 
          :disabled="form.processing"
          class="w-full bg-teal-600 hover:bg-teal-700 text-white font-bold py-3.5 rounded-xl shadow-sm transition text-sm flex items-center justify-center disabled:opacity-50"
        >
          <span v-if="form.processing">Criando sua conta...</span>
          <span v-else>Concluir Cadastro</span>
        </button>
      </form>

      <div class="text-center pt-2 border-t border-slate-100">
        <p class="text-xs text-slate-500">
          Já tem uma conta? 
          <Link href="/login" class="font-bold text-slate-950 hover:text-slate-700 ml-1 transition">
            Fazer Login
          </Link>
        </p>
      </div>

    </div>
  </div>
</template>

<script setup>
import { Link, useForm } from '@inertiajs/vue3'

// Mantido em PascalCase para conversar perfeitamente com o DtoRegistro do C#
const form = useForm({
  Nome: '',
  NomeDaOtica: '',
  Email: '',
  Senha: ''
})

// CORRIGIDO: De 'ejecutarCadastro' para 'executarCadastro' (com X)
const executarCadastro = () => {
  form.post('/cadastro', {
    // CORRIGIDO: Evento correto do useForm para capturar falhas de validação
    onError: () => {
      form.reset('Senha')
    }
  })
}
</script>