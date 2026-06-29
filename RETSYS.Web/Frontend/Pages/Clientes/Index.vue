<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-6xl mx-auto">
      
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <h1 class="text-2xl font-black text-slate-950">Carteira de Clientes (CRM)</h1>
          <p class="text-sm text-slate-500">Consulte históricos ópticos, prontuários clínicos e faturamento consolidado por paciente.</p>
        </div>
      </div>

      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm flex flex-col sm:flex-row items-center gap-4 justify-between">
        <div class="w-full max-w-xl">
          <input 
            v-model="termoBusca" 
            @input="executarBusca"
            type="text" 
            placeholder="Pesquisa rápida por nome parcial ou CPF do cliente..." 
            class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 shadow-sm placeholder:text-slate-400"
          />
        </div>
        
        <button 
          @click="exibirFormNovoCliente = !exibirFormNovoCliente" 
          class="w-full sm:w-auto text-xs bg-slate-100 border border-slate-200 hover:bg-slate-200 text-slate-700 px-4 py-2.5 rounded-xl font-bold transition flex items-center justify-center gap-2"
        >
          <span>{{ exibirFormNovoCliente ? '✕ Ocultar Cadastro' : '＋ Criar Cadastro Prévio' }}</span>
        </button>
      </div>

      <div v-if="exibirFormNovoCliente" class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm transition duration-300">
        <h3 class="text-base font-bold text-slate-950 mb-3">Pré-cadastro Auxiliar de Cliente</h3>
        <p class="text-xs text-slate-400 mb-4">Utilize este formulário apenas se desejar registrar dados cadastrais isolados sem emitir uma Ordem de Serviço na hora.</p>
        
        <form @submit.prevent="cadastrarCliente" class="space-y-4">
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div class="md:col-span-2">
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Nome Completo *</label>
              <input v-model="form.Nome" type="text" placeholder="Ex: João Silva" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>
            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Convênio</label>
              <input v-model="form.Convenio" type="text" placeholder="Plano Óptico" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
            </div>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">CPF *</label>
              <input v-model="form.CPF" type="text" placeholder="000.000.000-00" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>
            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">WhatsApp / Telefone *</label>
              <input v-model="form.Telefone" type="text" placeholder="(11) 99999-0000" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>
            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">E-mail</label>
              <input v-model="form.Email" type="email" placeholder="cliente@email.com" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
            </div>
          </div>

          <div class="border-t border-slate-100 pt-4 grid grid-cols-1 sm:grid-cols-4 gap-4">
            <div>
              <label class="block text-[11px] font-bold uppercase text-teal-600 tracking-wider mb-1.5">CEP Residencia *</label>
              <input v-model="form.Cep" @blur="buscarCepAutomático" type="text" placeholder="00000-000" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" required />
            </div>
            <div class="sm:col-span-2">
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Logradouro *</label>
              <input v-model="form.Logradouro" type="text" placeholder="Rua, Avenida..." class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>
            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Número *</label>
              <input v-model="form.Numero" type="text" placeholder="123" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono text-center" required />
            </div>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Bairro *</label>
              <input v-model="form.Bairro" type="text" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>
            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Cidade *</label>
              <input v-model="form.Cidade" type="text" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>
            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">UF *</label>
              <input v-model="form.Estado" type="text" placeholder="SP" maxlength="2" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 uppercase font-mono text-center" required />
            </div>
          </div>

          <div class="flex justify-end gap-2">
            <button type="submit" :disabled="form.processing" class="bg-teal-600 hover:bg-teal-700 disabled:bg-slate-200 text-white font-bold py-2.5 px-6 rounded-xl text-xs uppercase tracking-wider transition shadow-sm">
              <span v-if="form.processing">Processando...</span>
              <span v-else>Salvar Ficha Cadastral</span>
            </button>
          </div>
        </form>
      </div>

      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <h3 class="text-base font-bold text-slate-950 mb-4">Registros Encontrados</h3>

        <div v-if="!Clientes || Clientes.length === 0" class="text-center py-12 border-2 border-dashed border-slate-100 rounded-xl text-slate-400 text-sm">
          Nenhum cliente atende aos critérios de busca informados.
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-left text-sm border-collapse">
            <thead>
              <tr class="border-b border-slate-100 text-slate-400 text-xs font-bold uppercase tracking-wider">
                <th class="pb-3">Nome do Cliente</th>
                <th class="pb-3 text-center">CPF</th>
                <th class="pb-3 text-center">Contato (WhatsApp)</th>
                <th class="pb-3 text-center">Última OS</th>
                <th class="pb-3 text-right">Total Gasto</th>
                <th class="pb-3 text-center">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="c in Clientes" :key="c.Id" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
                <td class="py-4 font-bold text-slate-800">{{ c.Nome }}</td>
                <td class="py-4 text-center font-mono text-slate-600 text-xs">{{ c.CPF }}</td>
                
                <td class="py-4 text-center font-mono text-xs">
                  <a 
                    v-if="c.Telefone" 
                    :href="gerarLinkSampleWhatsapp(c.Telefone)" 
                    target="_blank"
                    class="text-teal-600 hover:text-teal-700 font-bold inline-flex items-center gap-1 bg-teal-50 px-2.5 py-1 rounded-lg border border-teal-100/80 transition"
                  >
                    <span>💬</span> {{ c.Telefone }}
                  </a>
                  <span v-else class="text-slate-400">--</span>
                </td>

                <td class="py-4 text-center font-mono text-xs text-teal-600 font-bold">
                  {{ c.UltimaOs }}
                </td>

                <td class="py-4 text-right font-black text-slate-950 font-mono text-xs">
                  R$ {{ c.TotalGasto.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }}
                </td>
                
                <td class="py-4 text-center">
                  <Link 
                    :href="`/clientes/${c.Id}/historico`"
                    class="bg-slate-950 hover:bg-slate-800 text-white text-xs font-bold px-3 py-1.5 rounded-lg transition shadow-sm"
                  >
                    Ficha Completa
                  </Link>
                </td>
              </tr>
            </tbody>
          </table>
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
  FiltroBusca: String
})

const termoBusca = ref(props.FiltroBusca || '')
const exibirFormNovoCliente = ref(false) // Controla o estado colapsado do cadastro prévio

const form = useForm({
  Nome: '',
  CPF: '',
  Telefone: '', // Corrigido Celular -> Telefone
  Cep: '',      // Sincronizado minúsculas/maiúsculas estruturais do C#
  Logradouro: '',
  Numero: '',
  Bairro: '',
  Cidade: '',
  Estado: '',
  Convenio: '',
  Email: ''
})

const registrarTimeout = ref(null)
const executarBusca = () => {
  clearTimeout(registrarTimeout.value)
  registrarTimeout.value = setTimeout(() => {
    router.get('/clientes', { busca: termoBusca.value }, { preserveState: true, replace: true })
  }, 300)
}

const buscarCepAutomático = async () => {
  const cepLimpo = form.Cep.replace(/\D/g, '')
  if (cepLimpo.length !== 8) return

  try {
    const resposta = await fetch(`https://viacep.com.br/ws/${cepLimpo}/json/`)
    if (resposta.ok) {
      const dados = await resposta.json()
      if (!dados.erro) {
        form.Logradouro = dados.logradouro || ''
        form.Bairro = dados.bairro || ''
        form.Cidade = dados.localidade || ''
        form.Estado = dados.uf || ''
      }
    }
  } catch (err) {
    console.error("Falha ao consultar o webservice do ViaCEP:", err)
  }
}

const generarLinkSampleWhatsapp = (telefoneRaw) => {
  if (!telefoneRaw) return '#'
  const numeroLimpo = telefoneRaw.replace(/\D/g, '')
  return `https://wa.me/55${numeroLimpo}?text=Olá!%20Aqui%20é%20da%20óptica%20RETSYS.`
}

const cadastrarCliente = () => {
  form.post('/clientes', {
    preserveScroll: true,
    onSuccess: () => {
      form.reset()
      exibirFormNovoCliente.value = false
      alert('Cadastro realizado com sucesso!')
    }
  })
}
</script>