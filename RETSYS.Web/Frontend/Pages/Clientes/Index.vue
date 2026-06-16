<template>
  <div class="min-h-screen bg-slate-50 p-4 md:p-8 font-sans text-slate-900">
    
    <div class="max-w-6xl mx-auto mb-6 flex flex-col md:flex-row md:items-center md:justify-between gap-4">
      <div>
        <h1 class="text-2xl font-black text-slate-950">Carteira de Clientes</h1>
        <p class="text-sm text-slate-500">Cadastre novos clientes e consulte históricos de receitas.</p>
      </div>

      <div class="w-full md:w-80">
        <input 
          v-model="termoBusca" 
          @input="executarBusca"
          type="text" 
          placeholder="Buscar por Nome ou CPF..." 
          class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 shadow-sm placeholder:text-slate-400"
        />
      </div>
    </div>

    <div class="max-w-6xl mx-auto grid grid-cols-1 lg:grid-cols-3 gap-8">
      
      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm h-fit space-y-4">
        <h3 class="text-base font-bold text-slate-950">Novo Cadastro</h3>
        
        <form @submit.prevent="cadastrarCliente" class="space-y-4">
          <div>
            <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Nome Completo *</label>
            <input v-model="form.Nome" type="text" placeholder="Ex: João Silva" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
          </div>

          <div>
            <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">CPF *</label>
            <input v-model="form.CPF" type="text" placeholder="000.000.000-00" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
          </div>

          <div>
            <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Celular / WhatsApp *</label>
            <input v-model="form.Celular" type="text" placeholder="(11) 99999-0000" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
          </div>

          <button type="submit" class="w-full bg-teal-600 hover:bg-teal-700 text-white font-bold py-3 rounded-xl text-xs transition shadow-sm uppercase tracking-wider">
            Salvar Registro
          </button>
        </form>
      </div>

      <div class="lg:col-span-2 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <h3 class="text-base font-bold text-slate-950 mb-4">Registros Encontrados</h3>

        <div v-if="clientes.length === 0" class="text-center py-12 border-2 border-dashed border-slate-100 rounded-xl text-slate-400 text-sm">
          Nenhum cliente atende aos critérios de busca.
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-left text-sm border-collapse">
            <thead>
              <tr class="border-b border-slate-100 text-slate-400 text-xs font-bold uppercase tracking-wider">
                <th class="pb-3">Nome do Cliente</th>
                <th class="pb-3 text-center">CPF</th>
                <th class="pb-3 text-center">Contato</th>
                <th class="pb-3 text-center">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="c in clientes" :key="c.id" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
                <td class="py-4 font-bold text-slate-800">{{ c.nome }}</td>
                <td class="py-4 text-center font-mono text-slate-600 text-xs">{{ c.cpf }}</td>
                <td class="py-4 text-center text-slate-500 font-mono text-xs">{{ c.celular }}</td>
                <td class="py-4 text-center">
                  <Link 
                    :href="`/clientes/${c.id}/historico`"
                    class="bg-slate-950 hover:bg-slate-800 text-white text-xs font-bold px-3 py-1.5 rounded-lg transition shadow-sm"
                  >
                    Histórico
                  </Link>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { router, Link } from '@inertiajs/vue3'

const props = defineProps({
  clientes: Array,
  filtroBusca: String
})

const termoBusca = ref(props.filtroBusca || '')

const form = reactive({
  Nome: '',
  CPF: '',
  Celular: ''
})

const registrarTimeout = ref(null)
const ejecutarBusca = () => {
  clearTimeout(registrarTimeout.value)
  registrarTimeout.value = setTimeout(() => {
    router.get('/clientes', { busca: termoBusca.value }, { preserveState: true, replace: true })
  }, 300)
}

const cadastrarCliente = () => {
  router.post('/clientes', form, {
    onSuccess: () => {
      form.Nome = ''
      form.CPF = ''
      form.Celular = ''
    }
  })
}
</script>