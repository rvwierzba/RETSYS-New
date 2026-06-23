<template>
  <div class="min-h-screen bg-slate-50 p-4 md:p-8 font-sans text-slate-900">
    <div class="max-w-7xl mx-auto space-y-8">
      
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <div>
          <h1 class="text-2xl font-black text-slate-950">Painel Gerencial</h1>
          <p class="text-sm text-slate-500">Acompanhamento de metas, filiais e performance da equipe.</p>
        </div>
        
        <div class="flex items-center gap-2">
          <select v-model="filtros.mes" @change="atualizarDashboard" class="rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500">
            <option v-for="(nome, index) in meses" :key="index + 1" :value="index + 1">{{ nome }}</option>
          </select>
          <select v-model="filtros.ano" @change="atualizarDashboard" class="rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500">
            <option v-for="ano in anos" :key="ano" :value="ano">{{ ano }}</option>
          </select>
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        
        <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm flex items-center justify-between">
          <div>
            <span class="text-xs font-bold uppercase text-slate-400 tracking-wider">Faturamento do Período</span>
            <p class="text-3xl font-black text-teal-600 mt-1">
              R$ {{ formatMoeda(TotalFaturado ?? totalFaturado) }}
            </p>
          </div>
          <div class="w-12 h-12 rounded-xl bg-teal-50 flex items-center justify-center text-teal-600 text-xl font-bold">$</div>
        </div>

        <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm flex items-center justify-between">
          <div>
            <span class="text-xs font-bold uppercase text-slate-400 tracking-wider">Ordens Emitidas</span>
            <p class="text-3xl font-black text-slate-950 mt-1">
              {{ TotalOS ?? totalOS ?? 0 }} OS
            </p>
          </div>
          <div class="w-12 h-12 rounded-xl bg-slate-100 flex items-center justify-center text-slate-700 text-sm font-bold">OS</div>
        </div>

        <div class="bg-slate-950 p-6 rounded-2xl text-white flex flex-col justify-between md:col-span-2 lg:col-span-1">
          <div>
            <span class="text-xs font-bold uppercase text-slate-400 tracking-wider">Status do Caixa</span>
            <p class="text-lg font-bold text-teal-400 mt-1">Integração PIX Ativa</p>
          </div>
          <div class="flex items-center justify-between text-xs text-slate-400 pt-4 border-t border-slate-800 mt-4">
            <span>Ambiente Produtivo</span>
            <span class="w-2 h-2 rounded-full bg-emerald-400 animate-pulse"></span>
          </div>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        <div class="lg:col-span-2 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
          <h3 class="text-lg font-bold text-slate-950">Desempenho por Vendedor</h3>
          
          <div v-if="!(RankingVendedores ?? rankingVendedores) || (RankingVendedores ?? rankingVendedores).length === 0" class="text-center py-12 text-slate-400 text-sm">
            Nenhuma venda registrada por colaboradores neste mês.
          </div>
          
          <div v-else class="overflow-x-auto">
            <table class="w-full text-left text-sm">
              <thead>
                <tr class="border-b border-slate-100 text-slate-400 text-xs font-bold uppercase tracking-wider">
                  <th class="pb-3">Vendedor</th>
                  <th class="pb-3 text-center">Atendimentos (OS)</th>
                  <th class="pb-3 text-right">Total Gerado</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="vendedor in (RankingVendedores ?? rankingVendedores)" :key="vendedor.vendedorNome || vendedor.VendedorNome" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
                  <td class="py-4 font-semibold text-slate-800">
                    {{ vendedor.vendedorNome || vendedor.VendedorNome }}
                  </td>
                  <td class="py-4 text-center text-slate-600">
                    {{ vendedor.quantidadeOS ?? vendedor.QuantidadeOS ?? 0 }}
                  </td>
                  <td class="py-4 text-right font-bold text-slate-950">
                    R$ {{ formatMoeda(vendedor.totalVendas ?? vendedor.TotalVendas) }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
          <h3 class="text-lg font-bold text-slate-950">Faturamento por Loja</h3>
          
          <div v-if="!(FaturamentoPorLoja ?? faturamentoPorLoja) || (FaturamentoPorLoja ?? faturamentoPorLoja).length === 0" class="text-center py-12 text-slate-400 text-sm">
            Sem registros de filiais no período.
          </div>
          
          <div v-else class="space-y-4">
            <div v-for="loja in (FaturamentoPorLoja ?? faturamentoPorLoja)" :key="loja.loja || loja.Loja" class="p-4 bg-slate-50 rounded-xl border border-slate-100 flex items-center justify-between">
              <div>
                <p class="font-bold text-slate-800 text-sm">
                  {{ loja.loja || loja.Loja }}
                </p>
                <p class="text-xs text-slate-400">Filial Ativa</p>
              </div>
              <p class="font-black text-slate-950 text-base">
                R$ {{ formatMoeda(loja.total ?? loja.Total) }}
              </p>
            </div>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { reactive } from 'vue'
import { router } from '@inertiajs/vue3'

const props = defineProps({
  MesFiltro: Number,
  mesFiltro: Number,
  AnoFiltro: Number,
  anoFiltro: Number,
  TotalFaturado: Number,
  totalFaturado: Number,
  TotalOS: Number,
  totalOS: Number,
  RankingVendedores: Array,
  rankingVendedores: Array,
  FaturamentoPorLoja: Array,
  faturamentoPorLoja: Array
})

const filtros = reactive({
  mes: props.MesFiltro ?? props.mesFiltro,
  ano: props.AnoFiltro ?? props.anoFiltro
})

const meses = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
]

const anos = [2025, 2026, 2027, 2028]

const atualizarDashboard = () => {
  router.get('/dashboard', { mes: filtros.mes, ano: filtros.ano }, { preserveState: true })
}

// Auxiliar para formatação de moeda limpa sem estourar erros de undefined
const formatMoeda = (valor) => {
  if (valor === undefined || valor === null) return '0,00'
  return Number(valor).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}
</script>