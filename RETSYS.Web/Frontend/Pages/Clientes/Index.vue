<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-6xl mx-auto">
      
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight">Carteira de Clientes & CRM Óptico</h1>
          <p class="text-xs text-slate-500">Consulte históricos ópticos, prontuários clínicos, filtre por períodos de faturamento ou registre fichas históricas antigas.</p>
        </div>
      </div>

      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm flex flex-col lg:flex-row items-end gap-4 justify-between">
        
        <div class="w-full lg:max-w-md space-y-1">
          <label class="block text-[10px] font-bold uppercase text-slate-400 tracking-wider">Busca por Nome ou Identificador</label>
          <input 
            v-model="termoBusca" 
            @input="executarFiltroCombinado"
            type="text" 
            placeholder="Pesquisa rápida por nome parcial ou CPF do cliente..." 
            class="w-full rounded-xl border-slate-200 text-xs focus:border-teal-500 focus:ring-teal-500 shadow-sm placeholder:text-slate-400"
          />
        </div>

        <div class="flex flex-wrap sm:flex-nowrap gap-3 items-end w-full lg:w-auto">
          <div class="w-full sm:w-36 space-y-1">
            <label class="block text-[10px] font-bold uppercase text-slate-400 tracking-wider">Mês de Compra</label>
            <select v-model="filtroPeriodo.mes" @change="executarFiltroCombinado" class="w-full rounded-xl border-slate-200 text-xs focus:border-teal-500 font-bold text-slate-700 bg-slate-50">
              <option :value="null">Todos os Meses</option>
              <option v-for="(nome, index) in meses" :key="index + 1" :value="index + 1">{{ nome }}</option>
            </select>
          </div>

          <div class="w-full sm:w-28 space-y-1">
            <label class="block text-[10px] font-bold uppercase text-slate-400 tracking-wider">Ano de Compra</label>
            <select v-model="filtroPeriodo.ano" @change="executarFiltroCombinado" class="w-full rounded-xl border-slate-200 text-xs focus:border-teal-500 font-bold text-slate-700 bg-slate-50">
              <option :value="null">Todos os Anos</option>
              <option v-for="ano in anos" :key="ano" :value="ano">{{ ano }}</option>
            </select>
          </div>

          <button 
            @click="exibirFormNovoCliente = !exibirFormNovoCliente" 
            class="w-full sm:w-auto text-xs bg-slate-950 hover:bg-slate-800 text-white px-5 py-3 rounded-xl font-bold transition flex items-center justify-center gap-2 shadow-sm"
          >
            <span>{{ exibirFormNovoCliente ? '✕ Ocultar Painel' : '＋ Cadastrar Cliente / Histórico' }}</span>
          </button>
        </div>
      </div>

      <div v-if="exibirFormNovoCliente" class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm transition duration-300">
        <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono border-b pb-2 mb-4">Ficha Cadastral Auxiliar</h3>
        
        <form @submit.prevent="cadastrarCliente" class="space-y-6 text-xs" enctype="multipart/form-data">
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div class="md:col-span-2">
              <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1.5">Nome Completo *</label>
              <input v-model="form.Nome" type="text" placeholder="Ex: João Silva" class="w-full rounded-xl border-slate-200 text-xs focus:border-teal-500" required />
            </div>
            <div>
              <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1.5">Convênio</label>
              <input v-model="form.Convenio" type="text" placeholder="Plano Óptico" class="w-full rounded-xl border-slate-200 text-xs focus:border-teal-500" />
            </div>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div>
              <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1.5">CPF *</label>
              <input v-model="form.CPF" type="text" placeholder="000.000.000-00" class="w-full rounded-xl border-slate-200 text-xs focus:border-teal-500" required />
            </div>
            <div>
              <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1.5">WhatsApp / Telefone *</label>
              <input v-model="form.Telefone" type="text" placeholder="(11) 99999-0000" class="w-full rounded-xl border-slate-200 text-xs focus:border-teal-500" required />
            </div>
            <div>
              <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1.5">E-mail</label>
              <input v-model="form.Email" type="email" placeholder="cliente@email.com" class="w-full rounded-xl border-slate-200 text-xs focus:border-teal-500" />
            </div>
          </div>

          <div class="border-t border-slate-100 pt-4 grid grid-cols-1 sm:grid-cols-4 gap-4">
            <div>
              <label class="block font-bold text-teal-600 uppercase tracking-wider mb-1.5">CEP Residência *</label>
              <input v-model="form.Cep" @blur="buscarCepAutomático" type="text" placeholder="00000-000" class="w-full rounded-xl border-slate-200 text-xs focus:border-teal-500 font-mono" required />
            </div>
            <div class="sm:col-span-2">
              <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1.5">Logradouro *</label>
              <input v-model="form.Logradouro" type="text" placeholder="Rua, Avenida..." class="w-full rounded-xl border-slate-200 text-xs focus:border-teal-500" required />
            </div>
            <div>
              <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1.5">Número *</label>
              <input v-model="form.Numero" type="text" placeholder="123" class="w-full rounded-xl border-slate-200 text-xs focus:border-teal-500 font-mono text-center" required />
            </div>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div>
              <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1.5">Bairro *</label>
              <input v-model="form.Bairro" type="text" class="w-full rounded-xl border-slate-200 text-xs focus:border-teal-500" required />
            </div>
            <div>
              <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1.5">Cidade *</label>
              <input v-model="form.Cidade" type="text" class="w-full rounded-xl border-slate-200 text-xs focus:border-teal-500" required />
            </div>
            <div>
              <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1.5">UF *</label>
              <input v-model="form.Estado" type="text" placeholder="SP" maxlength="2" class="w-full rounded-xl border-slate-200 text-xs focus:border-teal-500 uppercase font-mono text-center" required />
            </div>
          </div>

          <div class="p-5 bg-amber-50/60 rounded-2xl border border-amber-200 space-y-4">
            <div class="flex items-center justify-between">
              <div>
                <h4 class="font-bold text-amber-950 uppercase tracking-wider text-[10px] flex items-center gap-2">
                  <span class="w-2 h-2 rounded-full bg-amber-500 animate-pulse"></span>
                  Vincular Registro Histórico de Compra Antiga para este Cliente
                </h4>
                <p class="text-[10px] text-amber-600 mt-0.5">Marque se deseja importar o histórico de faturamento e lentes sem alterar o estoque físico ativo de hoje.</p>
              </div>
              <input type="checkbox" v-model="form.RegistrarHistorico" class="rounded border-amber-300 text-amber-600 focus:ring-amber-500" />
            </div>

            <div v-if="form.RegistrarHistorico" class="grid grid-cols-1 md:grid-cols-4 gap-4 pt-3 border-t border-amber-200/50 animate-fadeIn">
              <div>
                <label class="block font-bold text-amber-900 uppercase mb-1.5">Data da Compra *</label>
                <input v-model="form.HistoricoData" type="date" class="w-full rounded-xl border-amber-200 bg-white" :required="form.RegistrarHistorico" />
              </div>
              <div>
                <label class="block font-bold text-amber-900 uppercase mb-1.5">Valor do Pedido (R$) *</label>
                <input v-model.number="form.HistoricoValor" type="number" step="0.01" placeholder="R$ 0,00" class="w-full rounded-xl border-amber-200 bg-white font-mono font-bold" :required="form.RegistrarHistorico" />
              </div>
              <div class="md:col-span-2">
                <label class="block font-bold text-amber-900 uppercase mb-1.5">Identificação / Texto da Lente *</label>
                <input v-model="form.HistoricoLente" type="text" placeholder="Escreva a lente comprada na época (Ex: Progressiva com AR)" class="w-full rounded-xl border-amber-200 bg-white" :required="form.RegistrarHistorico" />
              </div>
            </div>

            <div v-if="form.RegistrarHistorico" class="pt-2">
              <label class="block font-bold text-amber-900 uppercase mb-1.5">Anexo da Receita Antiga (Foto/Scan)</label>
              <div class="flex items-center gap-3">
                <input type="file" id="arquivoReceita" accept="image/*" @change="vincularArquivoUpload" class="hidden" />
                <label for="arquivoReceita" class="bg-amber-100 hover:bg-amber-200 text-amber-900 text-[10px] font-bold px-3 py-2 rounded-xl transition cursor-pointer select-none border border-amber-300 shadow-sm">
                  {{ form.HistoricoFotoReceita ? 'Alterar Imagem' : '📸 Selecionar Arquivo Receita' }}
                </label>
                <span class="text-[10px] font-mono text-amber-700 truncate max-w-xs block">
                  {{ form.HistoricoFotoReceita ? form.HistoricoFotoReceita.name : 'Nenhuma foto anexada no momento' }}
                </span>
              </div>
            </div>
          </div>

          <div class="flex justify-end gap-2 border-t border-slate-100 pt-4">
            <button type="submit" :disabled="form.processing" class="bg-teal-600 hover:bg-teal-700 disabled:bg-slate-200 text-white font-bold py-2.5 px-6 rounded-xl text-xs uppercase tracking-wider transition shadow-sm">
              <span v-if="form.processing">Processando Gravação...</span>
              <span v-else>Salvar Ficha Cadastral</span>
            </button>
          </div>
        </form>
      </div>

      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono mb-4">Registros Encontrados</h3>

        <div v-if="!Clientes || Clientes.length === 0" class="text-center py-12 border-2 border-dashed border-slate-100 rounded-xl text-slate-400 text-sm">
          Nenhum cliente atende aos critérios de busca ou ao período de compra informado.
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
                    :href="generarLinkSampleWhatsapp(c.Telefone)" 
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
import { ref, reactive } from 'vue'
import { useForm, Link, router } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  Clientes: Array,
  FiltroBusca: String,
  MesFiltro: Number,
  AnoFiltro: Number
})

const termoBusca = ref(props.FiltroBusca || '')
const exibirFormNovoCliente = ref(false) 

// Estrutura de filtros reativos temporais combinados
const filtroPeriodo = reactive({
  mes: props.MesFiltro || null,
  ano: props.AnoFiltro || null
})

const meses = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
]

const anos = [2023, 2024, 2025, 2026, 2027, 2028]

const form = useForm({
  Nome: '',
  CPF: '',
  Telefone: '', 
  Cep: '',      
  Logradouro: '',
  Numero: '',
  Bairro: '',
  Cidade: '',
  Estado: '',
  Convenio: '',
  Email: '',
  
  // Mapeamento dos campos históricos adicionais
  RegistrarHistorico: false,
  HistoricoData: '',
  HistoricoValor: 0,
  HistoricoLente: '',
  HistoricoFotoReceita: null
})

const registrarTimeout = ref(null)
const executarFiltroCombinado = () => {
  clearTimeout(registrarTimeout.value)
  registrarTimeout.value = setTimeout(() => {
    router.get('/clientes', { 
      busca: termoBusca.value,
      mes: filtroPeriodo.mes,
      ano: filtroPeriodo.ano
    }, { preserveState: true, replace: true })
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
    console.error(err)
  }
}

const vincularArquivoUpload = (event) => {
  const arquivos = event.target.files
  if (arquivos.length > 0) {
    form.HistoricoFotoReceita = archivos[0]
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
      alert('Cadastro e dados históricos consolidados!')
    }
  })
}
</script>