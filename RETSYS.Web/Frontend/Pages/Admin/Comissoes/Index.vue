<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-6xl mx-auto">
      
      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight">Fechamentos de Comissão</h1>
          <p class="text-xs text-slate-500 mt-1">Audite, consolide e realize a baixa financeira do pagamento de comissões da equipe.</p>
        </div>
        
        <div v-if="$page.props.flash?.erro" class="p-3 bg-red-50 border border-red-200 text-red-800 rounded-xl text-xs font-bold no-print">
          ⚠️ {{ $page.props.flash.erro }}
        </div>
      </div>

      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono">Extratos de Fechamentos Consolidados</h3>
        
        <div v-if="fechamentosMapeados.length === 0" class="text-center py-8 text-slate-400 text-xs border border-dashed border-slate-100 rounded-xl">
          Nenhum fechamento de período localizado no histórico do sistema.
        </div>

        <div class="overflow-x-auto" v-else>
          <table class="w-full text-left text-xs border-collapse">
            <thead>
              <tr class="border-b border-slate-100 text-slate-400 font-bold uppercase tracking-wider">
                <th class="pb-3">Vendedor</th>
                <th class="pb-3 text-center">Período</th>
                <th class="pb-3 text-center">Ordens (Qtd)</th>
                <th class="pb-3 text-right">Vendas Brutas</th>
                <th class="pb-3 text-right">Comissão Devida</th>
                <th class="pb-3 text-center">Status</th>
                <th class="pb-3 text-center">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="f in fechamentosMapeados" :key="f.id" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
                <td class="py-3 font-semibold text-slate-800">{{ f.vendedorNome }}</td>
                <td class="py-3 text-center font-mono text-slate-600">{{ f.periodoReferencia }}</td>
                <td class="py-3 text-right font-mono text-slate-700">R$ {{ formatMoeda(f.totalVendasBrutas) }}</td>
                <td class="py-3 text-right font-black text-teal-600 font-mono">R$ {{ formatMoeda(f.totalComissao) }}</td>
                <td class="py-3 text-center text-slate-500 font-semibold">{{ f.qtdOs }} un</td>
                <td class="py-3 text-center">
                  <span 
                    :class="[
                      f.status === 'PAGO' ? 'bg-emerald-50 text-emerald-700 border-emerald-100' :
                      f.status === 'FECHADO' ? 'bg-amber-50 text-amber-700 border-amber-100' :
                      'bg-slate-50 text-slate-600 border-slate-100'
                    ]"
                    class="px-2 py-0.5 rounded-full font-bold text-[10px] border"
                  >
                    {{ f.status }}
                  </span>
                </td>
                <td class="py-3 text-center text-slate-400 font-mono">
                  {{ f.dataFechamento }}
                </td>
                <td class="py-3 text-right">
                  <button 
                    v-if="f.status === 'FECHADO'"
                    @click="efetuarBaixaPagamento(f.id)"
                    class="text-xs font-bold bg-teal-600 hover:bg-teal-700 text-white px-4 py-2 rounded-xl transition shadow-sm active:scale-95 whitespace-nowrap"
                  >
                    Confirmar Pagamento
                  </button>
                  <span v-else-if="f.status === 'PAGO'" class="text-xs font-semibold text-slate-400 font-mono bg-slate-50 px-2.5 py-1 rounded-lg border border-slate-100">
                    ✓ Liquidado: {{ f.dataPagamento }}
                  </span>
                  <span v-else class="text-xs font-medium text-slate-400 font-mono">
                    Aguardando Fechamento
                  </span>
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
import { computed } from 'vue'
import { router } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  Fechamentos: Array,
  fechamentos: Array
})

// Normalização defensiva contra oscilações de PascalCase/camelCase da API
const fechamentosMapeados = computed(() => {
  const lista = props.Fechamentos ?? props.fechamentos ?? []
  return lista.map(item => ({
    id: item.Id ?? item.id,
    vendedorNome: item.VendedorNome ?? item.vendedorNome ?? 'Não Informado',
    periodoReferencia: item.PeriodoReferencia ?? item.periodoReferencia,
    totalVendasBrutas: item.TotalVendasBrutas ?? item.totalVendasBrutas ?? 0,
    totalComissao: item.TotalComissao ?? item.totalComissao ?? 0,
    qtdOs: item.QtdOs ?? item.qtdOs ?? 0,
    status: item.Status ?? item.status,
    dataFechamento: formatarData(item.DataFechamento ?? item.dataFechamento),
    dataPagamento: formatarData(item.DataPagamento ?? item.dataPagamento)
  }))
})

// ✅ SEÇÃO 2: Dispara o gatilho POST de liquidação financeira e baixa em lote
const WhiteListPost = (id) => {
  router.post(`/admin/comissoes/pagar/${id}`, {}, { preserveScroll: true })
}

const efectuarBaixaPagamento = (id) => {
  if (!id) return
  if (confirm('Confirmar o pagamento físico da comissão e dar baixa definitiva no sistema?')) {
    WhiteListPost(id)
  }
}

const formatarMoeda = (valor) => {
  if (valor === undefined || valor === null) return '0,00'
  return Number(valor).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const formatarData = (dataRaw) => {
  if (!dataRaw) return null
  return new Date(dataRaw).toLocaleDateString('pt-BR')
}
</script>