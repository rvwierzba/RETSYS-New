<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-7xl mx-auto">
      
      <!-- Cabeçalho e Seletor de Loja / Filtros -->
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <div>
          <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight">Caixa e Fluxo de Recebimentos</h1>
          <p class="text-xs text-slate-500 mt-1">Monitore o faturamento em linha única por venda, acompanhe o caixa físico e a quitação de saldos.</p>
        </div>

        <div class="flex flex-wrap items-center gap-3">
          <!-- Filtro de Loja -->
          <div>
            <label class="block text-[9px] font-bold uppercase text-slate-400 mb-0.5">Loja / Unidade</label>
            <select v-model="filtros.loja" @change="filtrarCaixa" class="rounded-xl border-slate-200 text-xs font-bold text-indigo-700 bg-indigo-50/50 focus:ring-indigo-500">
              <option value="Consolidado">🏢 Todas as Lojas (Consolidado)</option>
              <option value="Matriz">Matriz</option>
              <option value="Travessa Itália">Travessa Itália</option>
              <option value="Parque">Parque</option>
            </select>
          </div>

          <!-- Filtro de Situação -->
          <div>
            <label class="block text-[9px] font-bold uppercase text-slate-400 mb-0.5">Situação</label>
            <select v-model="filtros.situacao" @change="filtrarCaixa" class="rounded-xl border-slate-200 text-xs font-bold text-slate-700 bg-slate-50 focus:ring-teal-500">
              <option value="todas">Todas as Situações</option>
              <option value="1/1">1/1 (Quitada)</option>
              <option value="1/2">1/2 (Entrada + Saldo Pendente)</option>
            </select>
          </div>
        </div>
      </div>

      <!-- QUADROS TOTALIZADORES DUPLOS -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        
        <!-- Total Vendido (Volume de Vendas) -->
        <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm flex items-center justify-between">
          <div>
            <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider block">Total Vendido (Volume)</span>
            <p class="text-2xl font-black text-slate-950 mt-1 font-mono">
              R$ {{ formatarMoeda(totais.totalVendido) }}
            </p>
            <span class="text-[10px] text-slate-400 block mt-0.5">Soma líquida das vendas do período</span>
          </div>
          <div class="w-12 h-12 rounded-2xl bg-slate-100 text-slate-700 flex items-center justify-center text-lg font-bold">
            📊
          </div>
        </div>

        <!-- Total Recebido (Caixa Físico / Gaveta) -->
        <div class="bg-emerald-900 text-white p-6 rounded-2xl border border-emerald-800 shadow-sm flex items-center justify-between ring-2 ring-emerald-500/20">
          <div>
            <div class="flex items-center gap-2">
              <span class="text-[10px] font-bold uppercase text-emerald-300 tracking-wider">Total Recebido (Caixa Físico)</span>
              <span class="w-2 h-2 rounded-full bg-emerald-400 animate-pulse"></span>
            </div>
            <p class="text-3xl font-black text-emerald-400 mt-1 font-mono">
              R$ {{ formatarMoeda(totais.totalRecebido) }}
            </p>
            <span class="text-[10px] text-emerald-200/80 block mt-0.5">Entradas + Saldos quitados no período (Bate com a gaveta)</span>
          </div>
          <div class="w-12 h-12 rounded-2xl bg-emerald-800 text-emerald-300 flex items-center justify-center text-lg font-bold">
            💵
          </div>
        </div>

        <!-- Total a Receber -->
        <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm flex items-center justify-between">
          <div>
            <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider block">Total a Receber</span>
            <p class="text-2xl font-black text-amber-600 mt-1 font-mono">
              R$ {{ formatarMoeda(totais.totalAReceber) }}
            </p>
            <span class="text-[10px] text-slate-400 block mt-0.5">Soma dos saldos restantes (1/2) pendentes</span>
          </div>
          <div class="w-12 h-12 rounded-2xl bg-amber-50 text-amber-600 flex items-center justify-center text-lg font-bold">
            ⏳
          </div>
        </div>

      </div>

      <!-- QUEBRA POR FORMA DE PAGAMENTO -->
      <div v-if="totais.quebraFormas && totais.quebraFormas.length > 0" class="bg-white p-4 rounded-2xl border border-slate-200 shadow-sm">
        <h4 class="text-[10px] font-bold uppercase tracking-wider text-slate-400 mb-3">Detalhamento do Caixa por Forma de Pagamento</h4>
        <div class="grid grid-cols-2 sm:grid-cols-5 gap-3">
          <div v-for="item in totais.quebraFormas" :key="item.forma || item.Forma" class="bg-slate-50 p-3 rounded-xl border border-slate-100">
            <span class="text-[9px] font-bold uppercase text-slate-400 block">{{ formatarNomeForma(item.forma || item.Forma) }}</span>
            <p class="text-sm font-black text-slate-900 font-mono mt-0.5">R$ {{ formatarMoeda(item.totalForma ?? item.TotalForma) }}</p>
          </div>
        </div>
      </div>

      <!-- TABELA DE CAIXA EM LINHA ÚNICA POR VENDA -->
      <div class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden space-y-4 p-6">
        <div class="flex items-center justify-between border-b pb-4">
          <h3 class="text-base font-bold text-slate-900 font-mono">Vendas e Lançamentos de Caixa</h3>
          <span class="text-xs text-slate-400 font-mono">{{ listaVendas.length }} registro(s) encontrado(s)</span>
        </div>

        <div v-if="listaVendas.length === 0" class="text-center py-12 text-slate-400 text-sm">
          Nenhuma venda financeira registrada com os filtros selecionados.
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-xs border-collapse">
            <thead>
              <tr class="border-b border-slate-200 text-slate-400 font-bold uppercase tracking-wider">
                <th class="pb-3 text-left">Cliente / Origem</th>
                <th class="pb-3 text-center">Data Compra</th>
                <th class="pb-3 text-left">Armação</th>
                <th class="pb-3 text-right">Valor Compra</th>
                <th class="pb-3 text-right">Valor Entrada</th>
                <th class="pb-3 text-right">Saldo Restante</th>
                <th class="pb-3 text-center">Situação</th>
                <th class="pb-3 text-left">Forma de Pagamento</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="v in listaVendas" :key="v.id || v.Id" class="border-b border-slate-100 hover:bg-slate-50/60 transition">
                <!-- Cliente / Nº OS (Alinhado à Esquerda) -->
                <td class="py-4 text-left">
                  <p class="font-bold text-slate-900">{{ v.clienteNome || v.ClienteNome }}</p>
                  <p class="text-[10px] text-slate-400 font-mono">
                    OS nº {{ v.numeroOS || v.NumeroOS }} • <span class="text-indigo-600 font-bold">{{ v.lojaVenda || v.LojaVenda || 'Matriz' }}</span>
                  </p>
                </td>

                <!-- Data da Compra (Centralizado) -->
                <td class="py-4 text-center text-slate-600 font-mono">
                  {{ formatarData(v.dataEntrada || v.DataEntrada) }}
                </td>

                <!-- Armação (Alinhado à Esquerda) -->
                <td class="py-4 text-left text-slate-700">
                  {{ v.armacaoDescricao || v.ArmacaoDescricao || '—' }}
                </td>

                <!-- Valor da Compra (Alinhado à Direita) -->
                <td class="py-4 text-right font-black text-slate-950 font-mono">
                  R$ {{ formatarMoeda(v.valorTotalLiquido ?? v.ValorTotalLiquido) }}
                </td>

                <!-- Valor Entrada (Alinhado à Direita) -->
                <td class="py-4 text-right font-bold text-emerald-700 font-mono">
                  R$ {{ formatarMoeda(v.valorEntrada ?? v.ValorEntrada) }}
                </td>

                <!-- Saldo Restante (Alinhado à Direita) -->
                <td class="py-4 text-right font-bold font-mono" :class="(v.valorRestante ?? v.ValorRestante) > 0 ? 'text-amber-700' : 'text-slate-400'">
                  R$ {{ formatarMoeda(v.valorRestante ?? v.ValorRestante) }}
                </td>

                <!-- Situação (Centralizado - 1/1 ou 1/2) -->
                <td class="py-4 text-center">
                  <span 
                    v-if="(v.situacao || v.Situacao) === '1/1'" 
                    class="bg-emerald-50 text-emerald-700 border border-emerald-200 px-2.5 py-1 rounded-full font-bold font-mono text-[11px]"
                  >
                    1/1 (Quitada)
                  </span>
                  <span 
                    v-else 
                    class="px-2.5 py-1 rounded-full font-bold font-mono text-[11px] border"
                    :class="(v.isAtrasada12 || v.IsAtrasada12) ? 'bg-rose-50 text-rose-700 border-rose-200 animate-pulse' : 'bg-amber-50 text-amber-700 border-amber-200'"
                    :title="(v.isAtrasada12 || v.IsAtrasada12) ? 'Atenção: Data de entrega estourada com saldo pendente!' : 'Entrada paga, saldo a receber na retirada'"
                  >
                    1/2 {{ (v.isAtrasada12 || v.IsAtrasada12) ? '⚠️ (Atrasada)' : '(Pendente)' }}
                  </span>
                </td>

                <!-- Forma de Pagamento (Alinhado à Esquerda) -->
                <td class="py-4 text-left font-semibold text-slate-800">
                  {{ v.formaPagamentoFormatada || v.FormaPagamentoFormatada }}
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
import { ref, reactive, computed, onUnmounted } from 'vue'
import { router } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  vendas: Array, Vendas: Array,
  totais: Object, Totais: Object,
  lojaFiltro: String, LojaFiltro: String,
  situacaoFiltro: String, SituacaoFiltro: String,
  formaPagamentoFiltro: String, FormaPagamentoFiltro: String
})

const parcelaSelecionada = ref(null)
const intervaloChecagem = ref(null)

const filtros = reactive({
  loja: props.LojaFiltro ?? props.lojaFiltro ?? 'Consolidado',
  situacao: props.SituacaoFiltro ?? props.situacaoFiltro ?? 'todas',
  formaPagamento: props.FormaPagamentoFiltro ?? props.formaPagamentoFiltro ?? ''
})

const listaVendas = computed(() => props.Vendas ?? props.vendas ?? [])
const totais = computed(() => props.Totais ?? props.totais ?? { totalVendido: 0, totalRecebido: 0, totalAReceber: 0, quebraFormas: [] })

const filtrarCaixa = () => {
  router.get('/caixa', {
    loja: filtros.loja,
    situacao: filtros.situacao,
    formaPagamento: filtros.formaPagamento
  }, { preserveState: true })
}

const solicitarPixProducao = (parcela) => {
  parcelaSelecionada.value = { ...parcela }
  const idReal = parcela.id ?? parcela.Id
  
  router.get('/caixa', { gerarPixParaId: idReal }, { 
    preserveState: true, 
    replace: true,
    onSuccess: () => {
      iniciarMonitoramentoPix(idReal)
    }
  })
}

const iniciarMonitoramentoPix = (id) => {
  pararMonitoramentoPix()

  intervaloChecagem.value = setInterval(async () => {
    try {
      const resposta = await fetch(`/caixa/status/${id}`)
      if (resposta.ok) {
        const dados = await resposta.json()
              
        if (dados.pago || dados.Pago) {
          pararMonitoramentoPix()
          alert('Pagamento PIX confirmado com sucesso na OpenPix!')
          
          parcelaSelecionada.value = null
          router.get('/caixa', {}, { preserveState: false })
        }
      }
    } catch (erro) {
      console.error("Erro na checagem automática do PIX:", erro)
    }
  }, 3000)
}

const confirmarBaixaManual = (idParcela) => {
  if (confirm('Confirmar recebimento manual via Dinheiro ou Cartão Físico?')) {
    pararMonitoramentoPix()
    
    router.post(`/caixa/baixar/${idParcela}`, {}, {
      onSuccess: () => {
        parcelaSelecionada.value = null
      }
    })
  }
}

const formatarMoeda = (valor) => {
  if (valor === undefined || valor === null) return '0,00'
  return Number(valor).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const formatarData = (dataRaw) => {
  if (!dataRaw) return '--/--/----'
  return new Date(dataRaw).toLocaleDateString('pt-BR')
}

const copiarCopiaECola = (texto) => {
  if (!texto) return
  navigator.clipboard.writeText(texto)
  alert('Código PIX Copia e Cola copiado para a área de transferência!')
}

const cancelarOperacaoPix = () => {
  pararMonitoramentoPix()
  if (router.page?.props) {
    router.page.props.DadosPixAtivo = null
  }
  parcelaSelecionada.value = null
  router.get('/caixa', {}, { preserveState: false })
}

const formatarNomeForma = (forma) => {
  if (!forma) return 'OUTRO'
  switch (forma.toUpperCase()) {
    case 'CARTAO_CREDITO': return 'Cartão Crédito'
    case 'CARTAO_DEBITO': return 'Cartão Débito'
    case 'DINHEIRO': return 'Dinheiro'
    case 'PIX': return 'PIX'
    case 'BOLETO': return 'Boleto'
    default: return forma
  }
}

const pararMonitoramentoPix = () => {
  if (intervaloChecagem.value) {
    clearInterval(intervaloChecagem.value)
    intervaloChecagem.value = null
  }
}

onUnmounted(() => {
  pararMonitoramentoPix()
})
</script>