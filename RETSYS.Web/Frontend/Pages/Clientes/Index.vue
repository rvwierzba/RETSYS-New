<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6">
      
      <div class="max-w-6xl mx-auto flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <h1 class="text-2xl font-black text-slate-950">Carteira de Clientes</h1>
          <p class="text-sm text-slate-500">Cadastre novos clientes, consulte históricos de receitas e abra chats diretos.</p>
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

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">CPF *</label>
                <input v-model="form.CPF" type="text" placeholder="000.000.000-00" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Celular / WhatsApp *</label>
                <input v-model="form.Celular" type="text" placeholder="(11) 99999-0000" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
              </div>
            </div>

            <div class="border-t border-slate-100 pt-4 space-y-4">
              <div>
                <label class="block text-[11px] font-bold uppercase text-teal-600 tracking-wider mb-1.5">CEP (Auto-Preenchimento)</label>
                <input 
                  v-model="form.CEP" 
                  @input="buscarCepAutomático" 
                  type="text" 
                  placeholder="00000-000" 
                  class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" 
                />
              </div>

              <div class="grid grid-cols-3 gap-2">
                <div class="col-span-2">
                  <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Logradouro (Rua / Av.) *</label>
                  <input v-model="form.Rua" type="text" placeholder="Rua, Av, Travessa..." class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
                </div>
                <div>
                  <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Número *</label>
                  <input v-model="form.Numero" type="text" placeholder="123" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono text-center" required />
                </div>
              </div>

              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Bairro</label>
                <input v-model="form.Bairro" type="text" placeholder="Nome do Bairro" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>

              <div class="grid grid-cols-3 gap-2">
                <div class="col-span-2">
                  <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Cidade</label>
                  <input v-model="form.Cidade" type="text" placeholder="Cidade" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
                </div>
                <div>
                  <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">UF</label>
                  <input v-model="form.Estado" type="text" placeholder="SP" max-length="2" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 uppercase font-mono text-center" />
                </div>
              </div>
            </div>

            <button 
              type="submit" 
              :disabled="form.processing"
              class="w-full bg-teal-600 hover:bg-teal-700 disabled:bg-slate-200 disabled:text-slate-400 text-white font-bold py-3 rounded-xl text-xs transition shadow-sm uppercase tracking-wider flex items-center justify-center min-h-[42px]"
            >
              <span v-if="form.processing">Processando...</span>
              <span v-else>Salvar Registro</span>
            </button>
          </form>
        </div>

        <div class="lg:col-span-2 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
          <h3 class="text-base font-bold text-slate-950 mb-4">Registros Encontrados</h3>

          <div v-if="!(Clientes ?? clientes) || (Clientes ?? clientes).length === 0" class="text-center py-12 border-2 border-dashed border-slate-100 rounded-xl text-slate-400 text-sm">
            Nenhum cliente atende aos critérios de busca.
          </div>

          <div v-else class="overflow-x-auto">
            <table class="w-full text-left text-sm border-collapse">
              <thead>
                <tr class="border-b border-slate-100 text-slate-400 text-xs font-bold uppercase tracking-wider">
                  <th class="pb-3">Nome do Cliente</th>
                  <th class="pb-3 text-center">CPF</th>
                  <th class="pb-3 text-center">Contato (WhatsApp)</th>
                  <th class="pb-3 text-center">Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="c in (Clientes ?? clientes)" :key="c.id || c.Id" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
                  <td class="py-4 font-bold text-slate-800">{{ c.nome || c.Nome }}</td>
                  <td class="py-4 text-center font-mono text-slate-600 text-xs">{{ c.cpf || c.CPF }}</td>
                  
                  <td class="py-4 text-center font-mono text-xs">
                    <a 
                      v-if="c.celular || c.Celular" 
                      :href="gerarLinkWhatsapp(c.celular || c.Celular)" 
                      target="_blank"
                      title="Clique para iniciar conversa no WhatsApp"
                      class="text-teal-600 hover:text-teal-700 font-bold hover:underline inline-flex items-center gap-1 bg-teal-50 px-2.5 py-1 rounded-lg border border-teal-100/80 transition"
                    >
                      <span>💬</span> {{ c.celular || c.Celular }}
                    </a>
                    <span v-else class="text-slate-400">--</span>
                  </td>
                  
                  <td class="py-4 text-center">
                    <Link 
                      :href="`/clientes/${c.id || c.Id}/historico`"
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
  </AuthenticatedLayout>
</template>

<script setup>
import { ref } from 'vue'
import { useForm, Link, router } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  Clientes: Array,
  FiltroBusca: String, filtroBusca: String
})

const termoBusca = ref(props.FiltroBusca || props.filtroBusca || '')

// CORRIGIDO: Propriedades mapeadas de forma idêntica à classe de Entidade Cliente.cs do C#
const form = useForm({
  Nome: '',
  CPF: '',
  Celular: '',
  CEP: '',
  Rua: '',
  Numero: '',
  Bairro: '',
  Cidade: '',
  Estado: ''
})

const registrarTimeout = ref(null)
const executarBusca = () => {
  clearTimeout(registrarTimeout.value)
  registrarTimeout.value = setTimeout(() => {
    router.get('/clientes', { busca: termoBusca.value }, { preserveState: true, replace: true })
  }, 300)
}

// CORRIGIDO: Atribuição do ViaCEP alterada para a propriedade "Rua"
const buscarCepAutomático = async () => {
  const cepLimpo = form.CEP.replace(/\D/g, '')
  if (cepLimpo.length !== 8) return

  try {
    const resposta = await fetch(`https://viacep.com.br/ws/${cepLimpo}/json/`)
    if (resposta.ok) {
      const dados = await resposta.json()
      if (!dados.erro) {
        form.Rua = dados.logradouro || ''
        form.Bairro = dados.bairro || ''
        form.Cidade = dados.localidade || ''
        form.Estado = dados.uf || ''
      }
    }
  } catch (err) {
    console.error("Falha ao consultar o webservice do ViaCEP:", err)
  }
}

const gerarLinkWhatsapp = (telefoneRaw) => {
  if (!telefoneRaw) return '#'
  const numeroLimpo = telefoneRaw.replace(/\D/g, '')
  return `https://wa.me/55${numeroLimpo}?text=Olá!%20Aqui%20é%20da%20óptica%20RETSYS.`
}

const cadastrarCliente = () => {
  form.post('/clientes', {
    preserveScroll: true,
    onSuccess: () => {
      form.reset()
    }
  })
}
</script>