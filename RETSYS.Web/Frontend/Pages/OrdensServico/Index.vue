<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-6xl mx-auto">

      <!-- Cabeçalho do Painel -->
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <div>
          <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight">Painel de Ordens de Serviço</h1>
          <p class="text-xs text-slate-500 mt-1">Consulte receitas, especificações de lentes, gerencie a esteira comercial e aplique filtros de faturamento.</p>
        </div>
        <button 
          @click="irParaNovaOrdem"
          class="bg-teal-600 hover:bg-teal-700 text-white font-bold py-2.5 px-6 rounded-xl text-xs transition shadow-sm uppercase tracking-wider h-fit"
        >
          + Emitir Nova OS
        </button>
      </div>

      <!-- Barra de Filtros e Faturamento (PONTO 4: Filtro por Vendedora) -->
      <div class="grid grid-cols-1 lg:grid-cols-4 gap-4 bg-white p-4 rounded-2xl border border-slate-200 shadow-sm items-center">
        
        <div class="lg:col-span-3 flex flex-wrap items-center gap-3">
          <!-- Filtro por Composição da Venda -->
          <div class="flex flex-wrap gap-1.5">
            <button 
              @click="filtrarPorComposicao('total')"
              :class="[filtroAtivoComp === 'total' ? 'bg-slate-950 text-white font-black' : 'bg-slate-50 text-slate-600 hover:bg-slate-100 font-semibold']"
              class="px-3.5 py-2 rounded-xl text-xs uppercase tracking-wider transition border border-transparent"
            >
              📋 Total
            </button>
            <button 
              @click="filtrarPorComposicao('armacao')"
              :class="[filtroAtivoComp === 'armacao' ? 'bg-slate-950 text-white font-black' : 'bg-slate-50 text-slate-600 hover:bg-slate-100 font-semibold']"
              class="px-3.5 py-2 rounded-xl text-xs uppercase tracking-wider transition border"
            >
              👓 Armações
            </button>
            <button 
              @click="filtrarPorComposicao('lente')"
              :class="[filtroAtivoComp === 'lente' ? 'bg-slate-950 text-white font-bold' : 'bg-slate-50 text-slate-600 font-medium']"
              class="px-3.5 py-2 rounded-xl text-xs border border-transparent hover:bg-slate-100 transition"
            >
              🔬 Lentes
            </button>
            <button 
              @click="filtrarPorComposicao('completo')"
              :class="[filtroAtivoComp === 'completo' ? 'bg-slate-950 text-white font-black' : 'bg-slate-50 text-slate-600 hover:bg-slate-100 font-semibold']"
              class="px-3.5 py-2 rounded-xl text-xs uppercase tracking-wider transition border"
            >
              💎 Completo
            </button>
          </div>

          <!-- PONTO 4: Select de Filtro por Vendedora -->
          <div class="w-full sm:w-auto min-w-[180px]">
            <select 
              v-model="vendedorSelecionado" 
              @change="filtrarPorVendedor"
              class="w-full rounded-xl border-slate-200 text-xs focus:border-teal-500 font-bold text-slate-700 bg-slate-50 py-2 px-3"
            >
              <option value="">(Todas as Vendedoras)</option>
              <option v-for="v in (Vendedores ?? vendedores)" :key="v.id || v.Id" :value="v.id || v.Id">
                👩‍💼 {{ v.nome || v.Nome }}
              </option>
            </select>
          </div>
        </div>

        <!-- Faturamento Calculado -->
        <div class="bg-teal-50 border border-teal-200 rounded-xl p-3 text-center lg:text-right">
          <span class="text-[10px] font-bold text-teal-800 uppercase tracking-wider block">Faturamento do Filtro</span>
          <p class="text-lg font-black text-teal-700 font-mono mt-0.5">
            {{ formatarMoeda(totalExibido) }}
          </p>
        </div>
      </div>

      <!-- Tabela de OSs -->
      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <div v-if="!(Ordens ?? ordens) || (Ordens ?? ordens).length === 0" class="text-center py-12 border-2 border-dashed border-slate-100 rounded-xl text-slate-400 text-sm">
          Nenhuma ordem de serviço localizada para os filtros e parâmetros indicados.
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-left text-sm border-collapse">
            <thead>
              <tr class="border-b border-slate-100 text-slate-400 text-xs font-bold uppercase tracking-wider">
                <th class="pb-3">Nº Documento / Data</th>
                <th class="pb-3">Cliente / Paciente</th>
                <th class="pb-3">Atendente / Vendedora</th>
                <th class="pb-3 text-right">Valor Total</th>
                <th class="pb-3 text-center">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="os in (Ordens ?? ordens)" :key="os.id || os.Id" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
                <td class="py-4">
                  <p class="font-mono font-bold text-slate-900 text-xs bg-slate-100 px-2 py-0.5 rounded w-fit mb-1">
                    {{ os.numeroOS || os.NumeroOS }}
                  </p>
                  <p class="text-xs text-slate-400">
                    {{ formatarData(os.dataVenda || os.DataVenda || os.dataEntrada || os.DataEntrada) }}
                  </p>
                </td>
                <td class="py-4 font-semibold text-slate-800">
                  {{ os.clienteNome || os.ClienteNome }}
                </td>
                <td class="py-4 text-slate-600 text-xs font-medium">
                  {{ os.vendedorNome || os.VendedorNome || 'Não atribuído' }}
                </td>
                <td class="py-4 text-right font-black font-mono text-slate-950">
                  {{ formatarMoeda(os.valorTotal ?? os.ValorTotal) }}
                </td>
                <td class="py-4 text-center flex items-center justify-center gap-2">
                  <!-- PONTO 2: Botão para abrir modal de detalhes completos da OS -->
                  <button 
                    @click="abrirPranchetaClinica(os)"
                    class="bg-slate-950 hover:bg-slate-800 text-white text-xs font-bold px-3 py-1.5 rounded-lg transition shadow-sm font-mono flex items-center gap-1"
                  >
                    <span>👁️</span> Ver Receita / OS
                  </button>
                  <button 
                    @click="excluirOS(os.id || os.Id, os.numeroOS || os.NumeroOS)"
                    class="bg-rose-50 hover:bg-rose-100 text-rose-700 border border-rose-200 text-xs font-bold px-3 py-1.5 rounded-lg transition shadow-sm font-mono"
                    title="Excluir Ordem de Serviço"
                  >
                    🗑️
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- PONTO 2: MODAL / PRANCHETA COMPLETA DA ORDEM DE SERVIÇO -->
      <div v-if="osSelecionada" class="fixed inset-0 bg-slate-950/60 backdrop-blur-sm z-50 flex items-center justify-center p-4 overflow-y-auto">
        <div class="bg-white rounded-3xl border border-slate-200 shadow-2xl max-w-3xl w-full max-h-[90vh] overflow-y-auto p-6 md:p-8 space-y-6">
          
          <!-- Cabeçalho da Modal -->
          <div class="flex items-center justify-between border-b border-slate-100 pb-4">
            <div>
              <span class="text-[10px] font-mono font-bold bg-teal-100 text-teal-800 px-2.5 py-0.5 rounded-full uppercase">
                Ordem de Serviço
              </span>
              <h2 class="text-xl font-black font-mono text-slate-950 mt-1">
                {{ osSelecionada.numeroOS || osSelecionada.NumeroOS }}
              </h2>
            </div>
            <button 
              @click="osSelecionada = null" 
              class="w-9 h-9 rounded-full bg-slate-100 hover:bg-slate-200 font-bold text-slate-500 hover:text-slate-900 transition flex items-center justify-center"
            >
              ✕
            </button>
          </div>

          <!-- Bloco 1: Cliente e Atendimento -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4 bg-slate-50 p-4 rounded-2xl border border-slate-200/80 text-xs">
            <div>
              <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider block mb-1">Cliente / Paciente</span>
              <p class="font-black text-slate-800 text-sm">{{ osSelecionada.clienteNome || osSelecionada.ClienteNome }}</p>
              <p class="text-slate-500 font-mono mt-0.5">CPF: {{ osSelecionada.cliente?.cpf || osSelecionada.Cliente?.CPF || 'Não informado' }}</p>
              <p class="text-slate-500 mt-0.5">Tel: {{ osSelecionada.cliente?.telefone || osSelecionada.Cliente?.Telefone || 'Sem telefone' }}</p>
            </div>
            <div>
              <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider block mb-1">Dados da Emissão</span>
              <p class="text-slate-700 font-semibold">Data Entrada: {{ formatarData(osSelecionada.dataEntrada || osSelecionada.DataEntrada) }}</p>
              <p class="text-slate-700 font-semibold">Data Entrega: {{ formatarData(osSelecionada.dataPrevistaEntrega || osSelecionada.DataPrevistaEntrega) }}</p>
              <p class="text-slate-700 font-semibold">Vendedora: {{ osSelecionada.vendedorNome || osSelecionada.VendedorNome || 'Não informado' }}</p>
            </div>
          </div>

          <!-- Bloco 2: Receita Médica / Grau Clínico -->
          <div v-if="osSelecionada.receita || osSelecionada.Receita" class="space-y-3">
            <h4 class="text-xs font-black uppercase tracking-wider text-slate-950 font-mono flex items-center gap-2">
              <span>🩺 Receita Oftalmológica / Refração</span>
              <span class="text-[10px] text-slate-400 font-normal">
                (Dr(a). {{ osSelecionada.medico || osSelecionada.MedicoNome || 'Não informado' }})
              </span>
            </h4>

            <div class="bg-white rounded-2xl border border-slate-200 overflow-hidden text-xs">
              <table class="w-full text-center border-collapse">
                <thead>
                  <tr class="bg-slate-100 text-slate-500 text-[10px] font-bold uppercase tracking-wider border-b border-slate-200">
                    <th class="py-2">Olho</th>
                    <th class="py-2">Esférico</th>
                    <th class="py-2">Cilíndrico</th>
                    <th class="py-2">Eixo</th>
                    <th class="py-2">DNP</th>
                    <th class="py-2">Altura Mont.</th>
                  </tr>
                </thead>
                <tbody class="font-mono">
                  <!-- OD -->
                  <tr class="border-b border-slate-100">
                    <td class="py-2.5 font-black text-slate-800">OD</td>
                    <td class="py-2.5 font-bold text-slate-900">{{ formatarGrau(receitaObj.odEsferico) }}</td>
                    <td class="py-2.5 font-bold text-amber-700">{{ formatarGrau(receitaObj.odCilindrico) }}</td>
                    <td class="py-2.5 text-slate-700">{{ receitaObj.odEixo }}°</td>
                    <td class="py-2.5 text-slate-700">{{ receitaObj.dnpOd || '--' }} mm</td>
                    <!-- PONTO 3: Exibição da Altura de Montagem OD -->
                    <td class="py-2.5 font-bold text-indigo-700">{{ receitaObj.alturaMontagemOd || receitaObj.alturaMontagem || '--' }} mm</td>
                  </tr>
                  <!-- OE -->
                  <tr>
                    <td class="py-2.5 font-black text-slate-800">OE</td>
                    <td class="py-2.5 font-bold text-slate-900">{{ formatarGrau(receitaObj.oeEsferico) }}</td>
                    <td class="py-2.5 font-bold text-amber-700">{{ formatarGrau(receitaObj.oeCilindrico) }}</td>
                    <td class="py-2.5 text-slate-700">{{ receitaObj.oeEixo }}°</td>
                    <td class="py-2.5 text-slate-700">{{ receitaObj.dnpOe || '--' }} mm</td>
                    <!-- PONTO 3: Exibição da Altura de Montagem OE -->
                    <td class="py-2.5 font-bold text-indigo-700">{{ receitaObj.alturaMontagemOe || receitaObj.alturaMontagem || '--' }} mm</td>
                  </tr>
                </tbody>
              </table>
            </div>

            <div v-if="receitaObj.adicao" class="p-3 bg-teal-50 border border-teal-100 rounded-xl text-xs font-semibold text-teal-900 flex justify-between">
              <span>Adição (AD): +{{ formatarGrau(receitaObj.adicao) }}</span>
              <span>Esférico Perto OD: {{ formatarGrau(receitaObj.esfericoPertoDireito) }} | OE: {{ formatarGrau(receitaObj.esfericoPertoEsquerdo) }}</span>
            </div>
          </div>

          <!-- Bloco 3: Medidas da Armação -->
          <div v-if="receitaObj && (receitaObj.aro || receitaObj.dm || receitaObj.vert)" class="p-4 bg-indigo-50/50 rounded-2xl border border-indigo-100 space-y-2">
            <span class="text-[10px] font-black uppercase text-indigo-900 tracking-wider block">📐 Medidas Físicas da Armação / Laboratório</span>
            <div class="grid grid-cols-3 md:grid-cols-6 gap-2 text-center text-xs font-mono">
              <div class="bg-white p-2 rounded-xl border border-indigo-100"><span class="block text-[9px] text-slate-400">ARO</span><b>{{ receitaObj.aro || '--' }}</b></div>
              <div class="bg-white p-2 rounded-xl border border-indigo-100"><span class="block text-[9px] text-slate-400">DM</span><b>{{ receitaObj.dm || '--' }}</b></div>
              <div class="bg-white p-2 rounded-xl border border-indigo-100"><span class="block text-[9px] text-slate-400">VERT</span><b>{{ receitaObj.vert || '--' }}</b></div>
              <div class="bg-white p-2 rounded-xl border border-indigo-100"><span class="block text-[9px] text-slate-400">PO</span><b>{{ receitaObj.po || '--' }}</b></div>
              <div class="bg-white p-2 rounded-xl border border-indigo-100"><span class="block text-[9px] text-slate-400">C.O OD</span><b>{{ receitaObj.coOd || '--' }}</b></div>
              <div class="bg-white p-2 rounded-xl border border-indigo-100"><span class="block text-[9px] text-slate-400">C.O OE</span><b>{{ receitaObj.coOe || '--' }}</b></div>
            </div>
          </div>

          <!-- Bloco 4: Financeiro e Condições de Pagamento -->
          <div v-if="osSelecionada.financeiro || osSelecionada.Financeiro" class="bg-slate-50 p-4 rounded-2xl border border-slate-200 space-y-3 text-xs">
            <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider block">Resumo Financeiro & Condições</span>
            
            <div class="grid grid-cols-2 md:grid-cols-4 gap-3 font-mono">
              <div>
                <span class="text-[10px] text-slate-400 block">Total Bruto</span>
                <span class="font-bold text-slate-700">{{ formatarMoeda(financeiroObj.valorTotalBruto) }}</span>
              </div>
              <div>
                <span class="text-[10px] text-slate-400 block">Desconto</span>
                <span class="font-bold text-amber-700">{{ formatarMoeda(financeiroObj.descontoReais) }}</span>
              </div>
              <div>
                <span class="text-[10px] text-slate-400 block">Forma Pagamento</span>
                <span class="font-bold text-slate-800">{{ financeiroObj.formaPagamento }}</span>
              </div>
              <div>
                <span class="text-[10px] text-slate-400 block">Total Líquido</span>
                <span class="font-black text-teal-700 text-sm">{{ formatarMoeda(financeiroObj.valorTotalLiquido || osSelecionada.valorTotal) }}</span>
              </div>
            </div>

            <!-- Tabela de Parcelas -->
            <div v-if="parcelasLista.length > 0" class="pt-2 border-t border-slate-200">
              <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider block mb-1">Cronograma de Parcelas</span>
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-2">
                <div v-for="p in parcelasLista" :key="p.numeroParcela" class="bg-white p-2 rounded-xl border border-slate-200 flex justify-between font-mono text-[11px]">
                  <span>Parcela {{ p.numeroParcela }}</span>
                  <span class="text-slate-500">{{ formatarData(p.dataVencimento) }}</span>
                  <b class="text-slate-900">{{ formatarMoeda(p.valor) }}</b>
                </div>
              </div>
            </div>
          </div>

          <div class="flex justify-end border-t border-slate-100 pt-4">
            <button 
              @click="osSelecionada = null" 
              class="bg-slate-950 hover:bg-slate-800 text-white font-bold py-2.5 px-6 rounded-xl text-xs uppercase transition shadow-sm"
            >
              Fechar Prancheta
            </button>
          </div>

        </div>
      </div>

    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { ref, computed } from 'vue'
import { router } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  Ordens: Array, ordens: Array,
  Vendedores: Array, vendedores: Array,
  FiltroAtivo: String, filtroAtivo: String,
  VendedorFiltro: String, vendedorFiltro: String,
  TotalFiltroAtivo: Number, totalFiltroAtivo: Number
})

const osSelecionada = ref(null)
const vendedorSelecionado = ref(props.VendedorFiltro ?? props.vendedorFiltro ?? '')

const filtroAtivoComp = computed(() => props.FiltroAtivo ?? props.filtroAtivo ?? 'total')
const totalExibido = computed(() => props.TotalFiltroAtivo ?? props.totalFiltroAtivo ?? 0)

const receitaObj = computed(() => osSelecionada.value?.receita || osSelecionada.value?.Receita || {})
const financeiroObj = computed(() => osSelecionada.value?.financeiro || osSelecionada.value?.Financeiro || {})
const parcelasLista = computed(() => osSelecionada.value?.parcelas || osSelecionada.value?.Parcelas || [])

const irParaNovaOrdem = () => router.get('/ordens/nova')

const aplicarFiltros = (tipoComposicao = null) => {
  const comp = tipoComposicao !== null ? tipoComposicao : filtroAtivoComp.value
  const params = {}
  if (comp && comp !== 'total') params.filtroComposicao = comp
  if (vendedorSelecionado.value) params.vendedorId = vendedorSelecionado.value

  router.get('/ordens', params, { preserveState: true, replace: true })
}

const filtrarPorComposicao = (tipo) => aplicarFiltros(tipo)
const filtrarPorVendedor = () => aplicarFiltros()
const irParaFiltro = (tipo) => filtrarPorComposicao(tipo)

const abrirPranchetaClinica = (ordem) => { 
  osSelecionada.value = { ...ordem } 
}

const excluirOS = (id, numero) => {
  if (confirm(`Deseja realmente excluir a OS ${numero}? O estoque da armação será devolvido automaticamente.`)) {
    router.post(`/ordens/excluir/${id}`, {}, {
      preserveScroll: true,
      onSuccess: () => alert(`OS ${numero} excluída com sucesso!`)
    })
  }
}

const formatarMoeda = (valor) => {
  if (valor === undefined || valor === null) return 'R$ 0,00'
  return Number(valor).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

const formatarGrau = (valor) => {
  if (valor === undefined || valor === null) return '0,00'
  const num = Number(valor)
  return (num > 0 ? '+' : '') + num.toFixed(2)
}

const formatarData = (dataRaw) => {
  if (!dataRaw) return '--/--/----'
  return new Date(dataRaw).toLocaleDateString('pt-BR')
}
</script>