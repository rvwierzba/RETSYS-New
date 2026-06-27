<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-6xl mx-auto">
      
      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <h1 class="text-2xl font-black text-slate-950">Fechamentos de Comissão</h1>
          <p class="text-sm text-slate-500">Audite, consolide e realize a baixa financeira do pagamento de comissões da equipe.</p>
        </div>
        
        <div v-if="$page.props.flash?.erro" class="p-3 bg-red-50 border border-red-200 text-red-800 rounded-xl text-xs font-bold">
          ⚠️ {{ $page.props.flash.erro }}
        </div>
      </div>

      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <h3 class="text-base font-bold text-slate-950 mb-4">Extratos Consolidados</h3>

        <div v-if="!Fechamentos || Fechamentos.length === 0" class="text-center py-12 border-2 border-dashed border-slate-100 rounded-xl text-slate-400 text-sm">
          Nenhum histórico de fechamento mensal gerado no sistema.
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-left text-sm border-collapse">
            <thead>
              <tr class="border-b border-slate-100 text-slate-400 text-xs font-bold uppercase tracking-wider">
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
              <tr v-for="f in Fechamentos" :key="f.Id" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
                <td class="py-4 font-bold text-slate-800">{{ f.VendedorNome }}</td>
                <td class="py-4 text-center font-mono text-xs text-slate-600">{{ f.PeriodoReferencia }}</td>
                <td class="py-4 text-center font-semibold text-slate-700">{{ f.QtdOs }} OS</td>
                <td class="py-4 text-right font-mono text-slate-600">R$ {{ formatarMoeda(f.TotalVendasBrutas) }}</td>
                <td class="py-4 text-right font-black text-teal-600 font-mono">R$ {{ formatarMoeda(f.TotalComissao) }}</td>
                <td class="py-4 text-center">
                  <span 
                    :class="f.Status === 'PAGO' ? 'bg-emerald-50 text-emerald-700 border-emerald-200' : 'bg-amber-50 text-amber-700 border-amber-200'"
                    class="px-2.5 py-0.5 rounded-full text-[11px] font-bold border uppercase tracking-wider"
                  >
                    {{ f.Status }}
                  </span>
                </td>
                <td class="py-4 text-center">
                  <button
                    v-if="f.Status !== 'PAGO'"
                    @click="efetuarBaixaPagamento(f.Id)"
                    class="text-xs font-bold bg-teal-600 hover:bg-teal-700 text-white px-3 py-1.5 rounded-xl transition shadow-sm active:scale-95"
                  >
                    Quitar Comissão
                  </button>
                  <span v-else class="text-xs font-medium text-slate-400 font-mono">
                    Pago em: {{ new Date(f.DataPagamento).toLocaleDateString('pt-BR') }}
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
import { router } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../../Shared/AuthenticatedLayout.vue'

defineProps({
  Fechamentos: Array
})

const efetuarBaixaPagamento = (id) => {
  if (!id) return
  if (confirm('Confirmar o pagamento físico da comissão e dar baixa definitiva no sistema?')) {
    router.post(`/admin/comissoes/pagar/${id}`, {}, { preserveScroll: true })
  }
}

const formatarMoeda = (valor) => {
  if (valor === undefined || valor === null) return '0,00'
  return Number(valor).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}
</script>