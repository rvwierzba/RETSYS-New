<template>
  <div class="min-h-screen bg-slate-50 p-4 md:p-8 font-sans text-slate-900">
    <div class="max-w-6xl mx-auto space-y-6">
      
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-black text-slate-950">Controle de Equipe</h1>
          <p class="text-sm text-slate-500">Gerencie os acessos de vendedores, gerentes e administradores de todas as filiais.</p>
        </div>
        <Link href="/" class="text-xs font-bold uppercase tracking-wider bg-white border border-slate-200 text-slate-600 hover:bg-slate-50 px-4 py-2.5 rounded-xl shadow-sm transition">
          Voltar para o Início
        </Link>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm h-fit space-y-4">
          <h3 class="text-base font-bold text-slate-950">Novo Integrante</h3>
          
          <form @submit.prevent="cadastrarColaborador" class="space-y-4">
            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Nome Completo *</label>
              <input v-model="form.Nome" type="text" placeholder="Ex: Carlos Souza" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>

            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">E-mail Corporativo *</label>
              <input v-model="form.Email" type="email" placeholder="carlos@otica.com" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>

            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Unidade / Filial Loja *</label>
              <input v-model="form.FilialLoja" type="text" placeholder="Ex: Filial Centro" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>

            <button type="submit" class="w-full bg-teal-600 hover:bg-teal-700 text-white font-bold py-3 rounded-xl text-xs transition shadow-sm uppercase tracking-wider">
              Registrar Colaborador
            </button>
          </form>
        </div>

        <div class="lg:col-span-2 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
          <h3 class="text-base font-bold text-slate-950 mb-4">Funcionários Cadastrados</h3>

          <div v-if="!Equipe || Equipe.length === 0" class="text-center py-12 border-2 border-dashed border-slate-100 rounded-xl text-slate-400 text-sm">
            Nenhum colaborador registrado no sistema.
          </div>

          <div v-else class="overflow-x-auto">
            <table class="w-full text-left text-sm border-collapse">
              <thead>
                <tr class="border-b border-slate-100 text-slate-400 text-xs font-bold uppercase tracking-wider">
                  <th class="pb-3">Colaborador / Acesso</th>
                  <th class="pb-3 text-center">Filial Alocada</th>
                  <th class="pb-3 text-center">Status</th>
                  <th class="pb-3 text-center">Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="user in Equipe" :key="user.id || user.Id" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
                  <td class="py-4">
                    <p class="font-bold text-slate-800">{{ user.nome || user.Nome }}</p>
                    <p class="text-xs text-slate-400 font-mono">{{ user.email || user.Email }}</p>
                  </td>
                  <td class="py-4 text-center text-sm font-medium text-slate-700">
                    {{ user.filialLoja || user.FilialLoja || 'Não Informada' }}
                  </td>
                  <td class="py-4 text-center">
                    <span :class="(user.ativo ?? user.Ativo) ? 'bg-emerald-50 text-emerald-700 border-emerald-100' : 'bg-red-50 text-red-700 border-red-100'" class="px-2.5 py-0.5 rounded-full text-xs font-bold border">
                      {{ (user.ativo ?? user.Ativo) ? 'Ativo' : 'Inativo' }}
                    </span>
                  </td>
                  <td class="py-4 text-center">
                    <button 
                      @click="alterarStatusUsuario(user.id || user.Id)"
                      :class="(user.ativo ?? user.Ativo) ? 'bg-slate-950 hover:bg-slate-800' : 'bg-teal-600 hover:bg-teal-700'"
                      class="text-white text-xs font-bold px-3 py-1.5 rounded-lg transition shadow-sm whitespace-nowrap"
                    >
                      {{ (user.ativo ?? user.Ativo) ? 'Desativar' : 'Reativar' }}
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { reactive } from 'vue'
import { Link, router } from '@inertiajs/vue3'

// CORRIGIDO: Prop em PascalCase combinando com o retorno do UsuariosController
defineProps({
  Equipe: Array
})

const form = reactive({
  Nome: '',
  Email: '',
  FilialLoja: ''
})

const cadastrarColaborador = () => {
  router.post('/equipe', form, {
    onSuccess: () => {
      form.Nome = ''
      form.Email = ''
      form.FilialLoja = ''
    }
  })
}

const alterarStatusUsuario = (id) => {
  router.post(`/equipe/alternar-status/${id}`)
}
</script>