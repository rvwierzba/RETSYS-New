<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-7xl mx-auto">

      <!-- CABEÇALHO DA PÁGINA E CONTROLES DE FILTRO -->
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm no-print">
        <div>
          <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight flex items-center gap-2">
            <span>📊 Fechamento do Gerente</span>
          </h1>
          <p class="text-xs text-slate-500 mt-1">Consolidação automática do caixa, conferência de recebimentos de balcão e conferência auditável.</p>
        </div>

        <div class="flex flex-wrap items-center gap-2">
          <!-- Botões Rápidos de Período -->
          <button 
            @click="filtrarAtalho('hoje')" 
            :class="filtros.tipoPeriodo === 'hoje' ? 'bg-slate-950 text-white font-bold' : 'bg-slate-100 text-slate-700 hover:bg-slate-200'"
            class="px-3 py-2 rounded-xl text-xs font-mono transition"
          >
            Hoje
          </button>
          <button 
            @click="filtrarAtalho('semana')" 
            :class="filtros.tipoPeriodo === 'semana' ? 'bg-slate-950 text-white font-bold' : 'bg-slate-100 text-slate-700 hover:bg-slate-200'"
            class="px-3 py-2 rounded-xl text-xs font-mono transition"
          >
            Esta Semana
          </button>
          <button 
            @click="filtrarAtalho('mes')" 
            :class="filtros.tipoPeriodo === 'mes' ? 'bg-slate-950 text-white font-bold' : 'bg-slate-100 text-slate-700 hover:bg-slate-200'"
            class="px-3 py-2 rounded-xl text-xs font-mono transition"
          >
            Este Mês
          </button>

          <!-- Filtro de Datas Customizado -->
          <div class="flex items-center gap-1 bg-slate-50 p-1 rounded-xl border border-slate-200">
            <input v-model="filtros.dataInicio" type="date" class="border-0 bg-transparent text-xs font-mono font-bold focus:ring-0 p-1" />
            <span class="text-xs text-slate-400 font-bold">até</span>
            <input v-model="filtros.dataFim" type="date" class="border-0 bg-transparent text-xs font-mono font-bold focus:ring-0 p-1" />
            <button @click="filtrarCustomizado" class="bg-teal-600 text-white font-bold px-3 py-1.5 rounded-lg text-xs hover:bg-teal-700 transition">
              Filtrar
            </button>
          </div>

          <!-- Ação de Impressão -->
          <button @click="imprimirRelatorio" class="bg-indigo-600 hover:bg-indigo-700 text-white font-bold px-4 py-2 rounded-xl text-xs uppercase tracking-wider transition shadow-sm flex items-center gap-1.5">
            🖨️ Imprimir Fechamento (A4)
          </button>
        </div>
      </div>

      <!-- CARDS DE RESUMO DO PERÍODO -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 no-print">
        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm space-y-1">
          <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider">Total Vendido (Líquido)</span>
          <p class="text-2xl font-black text-slate-950 font-mono">{{ formatarMoeda(totais.totalVendidoLiquido) }}</p>
          <p class="text-[10px] text-slate-400 font-mono">{{ totais.qtdOS }} Ordens de Serviço emitidas</p>
        </div>

        <div class="bg-white p-5 rounded-2xl border border-emerald-200 bg-emerald-50/20 shadow-sm space-y-1">
          <span class="text-[10px] font-bold uppercase text-emerald-800 tracking-wider">Total Recebido no Caixa</span>
          <p class="text-2xl font-black text-emerald-700 font-mono">{{ formatarMoeda(totais.totalRecebidoCaixa) }}</p>
          <p class="text-[10px] text-emerald-600 font-mono">Entradas: {{ formatarMoeda(totais.totalEntradasRecebidas) }} | Retiradas: {{ formatarMoeda(totais.totalRetiradasRecebidas) }}</p>
        </div>

        <div class="bg-white p-5 rounded-2xl border border-rose-200 bg-rose-50/20 shadow-sm space-y-1">
          <span class="text-[10px] font-bold uppercase text-rose-800 tracking-wider">Saldo Pendente (A Receber)</span>
          <p class="text-2xl font-black text-rose-700 font-mono">{{ formatarMoeda(totais.totalAReceberRestante) }}</p>
          <p class="text-[10px] text-rose-600 font-mono">A ser quitado na retirada dos óculos</p>
        </div>

        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm space-y-1">
          <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider">Ticket Médio / Descontos</span>
          <p class="text-2xl font-black text-teal-700 font-mono">{{ formatarMoeda(totais.ticketMedio) }}</p>
          <p class="text-[10px] text-amber-700 font-mono font-bold">Total Descontos Concedidos: {{ formatarMoeda(totais.totalDescontosReais) }}</p>
        </div>
      </div>

      <!-- PAINEL DE CONFERÊNCIA DE CAIXA -->
      <div class="bg-slate-900 text-white p-4 rounded-2xl flex flex-col sm:flex-row sm:items-center justify-between gap-4 font-mono text-xs no-print">
        <div class="flex items-center gap-3">
          <span class="text-lg">🛡️</span>
          <div>
            <p class="font-bold">Status da Conferência do Gerente</p>
            <p class="text-[11px] text-slate-400">Verificação auditada de cada lançamento contra o extrato bancário/gaveta</p>
          </div>
        </div>
        <div class="flex items-center gap-4 text-center">
          <div class="bg-emerald-500/20 px-3 py-1.5 rounded-xl border border-emerald-500/30">
            <span class="text-[10px] text-emerald-400 block uppercase font-bold">Conferido</span>
            <b class="text-emerald-300 text-sm">{{ formatarMoeda(totais.totalConferido) }}</b>
          </div>
          <div class="bg-amber-500/20 px-3 py-1.5 rounded-xl border border-amber-500/30">
            <span class="text-[10px] text-amber-400 block uppercase font-bold">Pendente Conferência</span>
            <b class="text-amber-300 text-sm">{{ formatarMoeda(totais.totalPendenteConferencia) }}</b>
          </div>
        </div>
      </div>

      <!-- DETALHAMENTOS DAS QUEBRAS -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6 no-print">
        
        <!-- Quebra por Forma de Pagamento -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm space-y-3">
          <h3 class="text-xs font-black text-slate-900 uppercase font-mono tracking-wider border-b pb-2">💳 Por Forma de Pagamento</h3>
          <div class="space-y-2">
            <div v-for="forma in resumoFormas" :key="forma.Forma" class="flex items-center justify-between text-xs p-2 rounded-xl bg-slate-50 border border-slate-100">
              <div>
                <b class="text-slate-800 font-mono">{{ forma.Forma }}</b>
                <span class="text-[10px] text-slate-400 block">{{ forma.QtdVendas }} operação(ões)</span>
              </div>
              <b class="font-mono text-teal-700 text-sm">{{ formatarMoeda(forma.Total) }}</b>
            </div>
          </div>
        </div>

        <!-- Quebra por Tipo de Venda -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm space-y-3">
          <h3 class="text-xs font-black text-slate-900 uppercase font-mono tracking-wider border-b pb-2">📦 Por Tipo de Venda</h3>
          <div class="space-y-2">
            <div v-for="tipo in resumoTipos" :key="tipo.Tipo" class="flex items-center justify-between text-xs p-2 rounded-xl bg-slate-50 border border-slate-100">
              <div>
                <b class="text-slate-800">{{ tipo.Tipo }}</b>
                <span class="text-[10px] text-slate-400 block">{{ tipo.Qtd }} OS(s)</span>
              </div>
              <b class="font-mono text-slate-900 text-sm">{{ formatarMoeda(tipo.Total) }}</b>
            </div>
          </div>
        </div>

        <!-- Quebra por Vendedora -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm space-y-3">
          <h3 class="text-xs font-black text-slate-900 uppercase font-mono tracking-wider border-b pb-2">👩‍💼 Por Vendedora</h3>
          <div class="space-y-2">
            <div v-for="vendedor in resumoVendedoresList" :key="vendedor.VendedorNome" class="p-2 rounded-xl bg-slate-50 border border-slate-100 text-xs space-y-1">
              <div class="flex justify-between items-center">
                <b class="text-slate-900">{{ vendedor.VendedorNome }}</b>
                <span class="text-[10px] text-slate-500 font-mono font-bold">{{ vendedor.QtdOS }} OS(s)</span>
              </div>
              <div class="flex justify-between text-[11px] font-mono text-slate-600">
                <span>Vendido: <b>{{ formatarMoeda(vendedor.TotalVendido) }}</b></span>
                <span class="text-teal-700">Comissão: <b>{{ formatarMoeda(vendedor.ComissaoGerada) }}</b></span>
              </div>
            </div>
          </div>
        </div>

      </div>

      <!-- TABELA DE VENDAS E AUDITORIA DE CONFERÊNCIA -->
      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4 no-print">
        <h3 class="text-xs font-black text-slate-900 uppercase font-mono tracking-wider">
          📋 Relação de Vendas do Período para Conferência
        </h3>

        <div v-if="vendasList.length === 0" class="text-center py-8 text-slate-400 text-xs border border-dashed border-slate-200 rounded-xl">
          Nenhuma Ordem de Serviço registrada no período selecionado.
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-left text-xs border-collapse">
            <thead>
              <tr class="border-b border-slate-200 text-slate-400 font-bold uppercase tracking-wider">
                <th class="pb-3">OS / Data</th>
                <th class="pb-3">Cliente</th>
                <th class="pb-3">Vendedora</th>
                <th class="pb-3 text-right">Bruto</th>
                <th class="pb-3 text-right">Desconto</th>
                <th class="pb-3 text-right">Líquido</th>
                <th class="pb-3 text-center">Forma Pag.</th>
                <th class="pb-3 text-center">Conferência</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in vendasList" :key="item.Id" class="border-b border-slate-100 hover:bg-slate-50/80 transition">
                <td class="py-3 font-mono">
                  <b>{{ item.NumeroOS }}</b>
                  <span class="block text-[10px] text-slate-400">{{ formatarData(item.DataEntrada) }}</span>
                </td>
                <td class="py-3 font-semibold text-slate-800">{{ item.ClienteNome }}</td>
                <td class="py-3 text-slate-600">{{ item.VendedorNome }}</td>
                <td class="py-3 text-right font-mono text-slate-600">{{ formatarMoeda(item.ValorBruto) }}</td>
                <td class="py-3 text-right font-mono text-amber-700">{{ formatarMoeda(item.DescontoReais) }}</td>
                <td class="py-3 text-right font-mono font-black text-slate-900">{{ formatarMoeda(item.ValorLiquido) }}</td>
                <td class="py-3 text-center font-mono font-bold text-slate-700">{{ item.FormaPagamento }}</td>
                <td class="py-3 text-center">
                  <button 
                    @click="alternarConferencia(item.Id)" 
                    :class="item.PagamentoConferido ? 'bg-emerald-100 text-emerald-800 border-emerald-300' : 'bg-slate-100 text-slate-600 hover:bg-amber-100 hover:text-amber-900 border-slate-200'"
                    class="px-2.5 py-1 rounded-lg text-[10px] font-bold border transition shadow-sm"
                  >
                    {{ item.PagamentoConferido ? '✓ Conferido' : '⏳ Conferir' }}
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- SEÇÃO SEPARADA: ORDENS DE SERVIÇO CANCELADAS -->
      <div v-if="canceladasList.length > 0" class="bg-rose-50/50 p-6 rounded-2xl border border-rose-200 space-y-3 no-print">
        <h3 class="text-xs font-black text-rose-900 uppercase font-mono tracking-wider flex items-center gap-1.5">
          <span>🚫 Ordens de Serviço Canceladas no Período (Auditoria)</span>
        </h3>
        <p class="text-[11px] text-rose-700">Estes lançamentos foram desconsiderados do faturamento líquido, porém mantidos para controle da gerência.</p>
        
        <div class="overflow-x-auto">
          <table class="w-full text-left text-xs border-collapse">
            <thead>
              <tr class="border-b border-rose-200 text-rose-800 font-bold uppercase tracking-wider">
                <th class="pb-2">OS</th>
                <th class="pb-2">Data Emissão</th>
                <th class="pb-2">Cliente</th>
                <th class="pb-2">Vendedora</th>
                <th class="pb-2 text-right">Valor Cancelado</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="c in canceladasList" :key="c.Id" class="border-b border-rose-100 text-slate-700">
                <td class="py-2 font-mono font-bold text-rose-900">{{ c.NumeroOS }}</td>
                <td class="py-2 font-mono">{{ formatarData(c.DataEntrada) }}</td>
                <td class="py-2">{{ c.ClienteNome }}</td>
                <td class="py-2">{{ c.VendedorNome }}</td>
                <td class="py-2 text-right font-mono font-bold text-rose-800">{{ formatarMoeda(c.ValorTotalLiquido) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- RELATÓRIO IMPRESSO COMPACTO (MODELO A4) -->
      <div class="only-print p-8 space-y-6 text-slate-900 font-sans">
        <div class="border-b-2 border-slate-950 pb-4 flex justify-between items-end">
          <div>
            <h1 class="text-2xl font-black uppercase tracking-wider">RETSYS ÓTICA</h1>
            <p class="text-sm font-bold uppercase text-slate-600">Relatório de Fechamento do Gerente</p>
          </div>
          <div class="text-right text-xs font-mono">
            <p><b>Período:</b> {{ formatarDataBR(filtros.dataInicio) }} até {{ formatarDataBR(filtros.dataFim) }}</p>
            <p><b>Emissão:</b> {{ new Date().toLocaleString('pt-BR') }}</p>
          </div>
        </div>

        <div class="grid grid-cols-4 gap-4 p-4 bg-slate-100 rounded-xl font-mono text-xs">
          <div><span class="block text-[10px] text-slate-500">TOTAL VENDIDO:</span><b>{{ formatarMoeda(totais.totalVendidoLiquido) }}</b></div>
          <div><span class="block text-[10px] text-slate-500">TOTAL RECEBIDO:</span><b>{{ formatarMoeda(totais.totalRecebidoCaixa) }}</b></div>
          <div><span class="block text-[10px] text-slate-500">A RECEBER:</span><b>{{ formatarMoeda(totais.totalAReceberRestante) }}</b></div>
          <div><span class="block text-[10px] text-slate-500">CONFERIDO:</span><b>{{ formatarMoeda(totais.totalConferido) }}</b></div>
        </div>

        <div class="space-y-2">
          <h3 class="text-xs font-bold uppercase border-b pb-1">Vendas Registradas no Período</h3>
          <table class="w-full text-left text-[11px] font-mono border-collapse">
            <thead>
              <tr class="border-b text-slate-500">
                <th class="pb-1">OS</th>
                <th class="pb-1">Data</th>
                <th class="pb-1">Cliente</th>
                <th class="pb-1">Vendedora</th>
                <th class="pb-1 text-right">Líquido</th>
                <th class="pb-1 text-center">Forma</th>
                <th class="pb-1 text-center">Conferido</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in vendasList" :key="item.Id" class="border-b border-slate-200">
                <td class="py-1.5 font-bold">{{ item.NumeroOS }}</td>
                <td class="py-1.5">{{ formatarData(item.DataEntrada) }}</td>
                <td class="py-1.5 truncate max-w-[150px]">{{ item.ClienteNome }}</td>
                <td class="py-1.5">{{ item.VendedorNome }}</td>
                <td class="py-1.5 text-right font-bold">{{ formatarMoeda(item.ValorLiquido) }}</td>
                <td class="py-1.5 text-center">{{ item.FormaPagamento }}</td>
                <td class="py-1.5 text-center">{{ item.PagamentoConferido ? 'SIM' : 'NÃO' }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="pt-16 grid grid-cols-2 gap-12 text-center text-xs font-mono">
          <div class="border-t border-slate-950 pt-2">
            Assinatura do Gerente
          </div>
          <div class="border-t border-slate-950 pt-2">
            Visto da Administração
          </div>
        </div>
      </div>

    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { reactive, computed } from 'vue'
import { router } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  DataInicio: String, dataInicio: String,
  DataFim: String, dataFim: String,
  TipoPeriodo: String, tipoPeriodo: String,
  Totais: Object, totais: Object,
  ResumoFormasPagamento: Array, resumoFormasPagamento: Array,
  ResumoTiposVenda: Array, resumoTiposVenda: Array,
  ResumoVendedores: Array, resumoVendedores: Array,
  ListaVendas: Array, listaVendas: Array,
  OrdensCanceladas: Array, ordensCanceladas: Array
})

const filtros = reactive({
  dataInicio: props.DataInicio ?? props.dataInicio ?? new Date().toISOString().split('T')[0],
  dataFim: props.DataFim ?? props.dataFim ?? new Date().toISOString().split('T')[0],
  tipoPeriodo: props.TipoPeriodo ?? props.tipoPeriodo ?? 'hoje'
})

const totais = computed(() => props.Totais ?? props.totais ?? {})
const resumoFormas = computed(() => props.ResumoFormasPagamento ?? props.resumoFormasPagamento ?? [])
const resumoTipos = computed(() => props.ResumoTiposVenda ?? props.resumoTiposVenda ?? [])
const resumoVendedoresList = computed(() => props.ResumoVendedores ?? props.resumoVendedores ?? [])
const vendasList = computed(() => props.ListaVendas ?? props.listaVendas ?? [])
const canceladasList = computed(() => props.OrdensCanceladas ?? props.ordensCanceladas ?? [])

const filtrarAtalho = (tipo) => {
  filtros.tipoPeriodo = tipo
  router.get('/caixa/fechamento', { tipoPeriodo: tipo }, { preserveState: true })
}

const filtrarCustomizado = () => {
  filtros.tipoPeriodo = 'custom'
  router.get('/caixa/fechamento', { dataInicio: filtros.dataInicio, dataFim: filtros.dataFim }, { preserveState: true })
}

const alternarConferencia = (osId) => {
  router.post(`/caixa/conferir-pagamento/${osId}`, {}, { preserveScroll: true })
}

const imprimirRelatorio = () => {
  window.print()
}

const formatarMoeda = (valor) => {
  return Number(valor || 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

const formatarData = (dataRaw) => {
  if (!dataRaw) return '--/--/----'
  return new Date(dataRaw).toLocaleDateString('pt-BR')
}

const formatarDataBR = (dataRaw) => {
  if (!dataRaw) return '--/--/----'
  const partes = dataRaw.split('-')
  if (partes.length === 3) return `${partes[2]}/${partes[1]}/${partes[0]}`
  return new Date(dataRaw).toLocaleDateString('pt-BR')
}
</script>

<style>
@media print {
  .no-print {
    display: none !important;
  }
  .only-print {
    display: block !important;
  }
}
@media screen {
  .only-print {
    display: none !important;
  }
}
</style>