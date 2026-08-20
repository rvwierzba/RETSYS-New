<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-6xl mx-auto">
      
      <!-- Cabeçalho do Painel -->
      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight">Gestão de Comissões da Equipe</h1>
          <p class="text-xs text-slate-500 mt-1">Defina porcentagens de comissão individuais, consolide fechamentos mensais e liquide pagamentos.</p>
        </div>
        
        <div v-if="$page.props.flash?.erro" class="p-3 bg-red-50 border border-red-200 text-red-800 rounded-xl text-xs font-bold">
          ⚠️ {{ $page.props.flash.erro }}
        </div>
      </div>

      <!-- CARD 1: Configuração de Taxas Individuais por Vendedora (NOVO) -->
      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono flex items-center gap-2">
            <span>⚙️ Taxas de Comissão por Vendedora</span>
          </h3>
          <span class="text-[11px] text-slate-400">Defina o % de comissão individual de cada funcionário</span>
        </div>

        <div v-if="vendedoresMapeados.length === 0" class="text-center py-6 text-slate-400 text-xs border border-dashed border-slate-100 rounded-xl">
          Nenhum vendedor ativo localizado no sistema.
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div v-for="vendedor in vendedoresMapeados" :key="vendedor.id" class="p-4 rounded-xl border border-slate-200 bg-slate-50/50 space-y-3">
            <div class="flex justify-between items-start">
              <div>
                <p class="font-bold text-slate-900 text-sm">{{ vendedor.nome }}</p>
                <p class="text-[11px] text-slate-400 font-mono">{{ vendedor.email }}</p>
                <span class="inline-block mt-1 px-2 py-0.5 rounded bg-slate-200 text-slate-700 text-[10px] font-bold">
                  📍 {{ vendedor.filialLoja || 'Matriz' }}
                </span>
              </div>
            </div>

            <!-- Ajuste de Porcentagem Individual -->
            <div class="flex items-center gap-2 pt-2 border-t border-slate-200/80">
              <div class="flex-1">
                <label class="block text-[10px] font-bold uppercase text-slate-400 mb-1">Comissão (%)</label>
                <input 
                  type="number" 
                  step="0.1" 
                  min="0" 
                  max="100" 
                  v-model.number="vendedor.taxaEdicao" 
                  class="w-full rounded-xl border-slate-200 text-xs font-mono font-bold text-teal-700 focus:border-teal-500 py-1.5 px-2.5 bg-white" 
                />
              </div>
              <button 
                @click="salvarTaxaIndividual(vendedor.id, vendedor.taxaEdicao)"
                class="self-end bg-teal-600 hover:bg-teal-700 text-white font-bold text-xs px-3 py-2 rounded-xl transition shadow-sm"
                title="Salvar % de comissão"
              >
                Salvar
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- CARD 2: Fechamentos Consolidados -->
      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
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
                <th class="pb-3 text-center">Data Fechamento</th>
                <th class="pb-3 text-right">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="f in fechamentosMapeados" :key="f.id" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
                <td class="py-3 font-semibold text-slate-800">{{ f.vendedorNome }}</td>
                <td class="py-3 text-center font-mono text-slate-600">{{ f.periodoReferencia }}</td>
                <td class="py-3 text-center text-slate-500 font-semibold">{{ f.qtdOs }} un</td>
                <td class="py-3 text-right font-mono text-slate-700">R$ {{ formatarMoeda(f.totalVendasBrutas) }}</td>
                <td class="py-3 text-right font-black text-teal-600 font-mono">R$ {{ formatarMoeda(f.totalComissao) }}</td>
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
import { ref, computed, watch } from 'vue'
import { router } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  Fechamentos: Array, fechamentos: Array,
  Vendedores: Array, vendedores: Array
})

// Mapeamento dos vendedores e cópia reativa do % para edição
const listaVendedoresRaw = computed(() => props.Vendedores ?? props.vendedores ?? [])
const vendedoresMapeados = ref([])

watch(listaVendedoresRaw, (novos) => {
  vendedoresMapeados.value = novos.map(v => ({
    id: v.id || v.Id,
    nome: v.nome || v.Nome,
    email: v.email || v.Email,
    filialLoja: v.filialLoja || v.FilialLoja,
    percentualComissao: v.percentualComissao ?? v.PercentualComissao ?? 3.00,
    taxaEdicao: v.percentualComissao ?? v.PercentualComissao ?? 3.00
  }))
}, { immediate: true })

// Mapeamento dos fechamentos
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

// Salva a alteração da comissão da vendedora diretamente
const salvarTaxaIndividual = (vendedorId, novaTaxa) => {
  if (!vendedorId || novaTaxa === undefined) return
  router.post(`/admin/comissoes/atualizar-taxa/${vendedorId}`, { PercentualComissao: novaTaxa }, {
    preserveScroll: true,
    onSuccess: () => alert('Porcentagem de comissão atualizada com sucesso!')
  })
}

// Dispara o pagamento do fechamento
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

const formatarData = (dataRaw) => {
  if (!dataRaw) return null
  return new Date(dataRaw).toLocaleDateString('pt-BR')
}
</script>