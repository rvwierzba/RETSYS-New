<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-7xl mx-auto">
      
      <!-- Cabeçalho Principal -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <div>
          <div class="flex items-center gap-2 mb-1">
            <span class="text-xs font-mono font-bold uppercase tracking-wider text-teal-700 bg-teal-50 px-2.5 py-0.5 rounded-full border border-teal-200">
              Módulo Comercial Óptico
            </span>
          </div>
          <h1 class="text-2xl font-black text-slate-900 tracking-tight">Histórico de Vendas</h1>
          <p class="text-sm text-slate-500">
            Registro unificado de saídas comerciais: Vendas rápidas de Balcão e Ordens de Serviço faturadas.
          </p>
        </div>

        <div class="flex items-center gap-3">
          <a
            href="/vendas/pdv"
            class="bg-amber-500 hover:bg-amber-600 text-slate-900 font-black py-2.5 px-4 rounded-xl text-xs uppercase tracking-wider transition shadow-sm flex items-center gap-2"
          >
            <span>🛒 Novo PDV Balcão</span>
          </a>
          <a
            href="/ordens-servico/nova"
            class="bg-teal-600 hover:bg-teal-700 text-white font-bold py-2.5 px-4 rounded-xl text-xs uppercase tracking-wider transition shadow-sm flex items-center gap-2"
          >
            <span>+ Nova OS Laboratório</span>
          </a>
        </div>
      </div>

      <!-- Cards de Métricas / KPIs da Loja -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <!-- Faturamento Total -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm flex items-center justify-between">
          <div>
            <p class="text-[11px] font-mono font-bold uppercase text-slate-400">Faturamento Filtrado</p>
            <p class="text-xl font-black text-slate-900 mt-1 font-mono">
              R$ {{ formatMoney(totalFaturamentoCalculado) }}
            </p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-teal-50 text-teal-600 flex items-center justify-center font-bold text-lg">
            💰
          </div>
        </div>

        <!-- Total de Vendas -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm flex items-center justify-between">
          <div>
            <p class="text-[11px] font-mono font-bold uppercase text-slate-400">Total de Operações</p>
            <p class="text-xl font-black text-slate-900 mt-1 font-mono">
              {{ listaVendasFiltradas.length }}
            </p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-blue-50 text-blue-600 flex items-center justify-center font-bold text-lg">
            📋
          </div>
        </div>

        <!-- Vendas Balcão -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm flex items-center justify-between">
          <div>
            <p class="text-[11px] font-mono font-bold uppercase text-slate-400">Vendas Balcão (VB)</p>
            <p class="text-xl font-black text-amber-600 mt-1 font-mono">
              {{ countBalcao }}
            </p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-amber-50 text-amber-600 flex items-center justify-center font-bold text-lg">
            🛒
          </div>
        </div>

        <!-- Vendas de OS -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm flex items-center justify-between">
          <div>
            <p class="text-[11px] font-mono font-bold uppercase text-slate-400">Ordens de Serviço (OS)</p>
            <p class="text-xl font-black text-indigo-600 mt-1 font-mono">
              {{ countOs }}
            </p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-indigo-50 text-indigo-600 flex items-center justify-center font-bold text-lg">
            👓
          </div>
        </div>
      </div>

      <!-- Filtros e Barra de Busca -->
      <div class="bg-white p-4 rounded-2xl border border-slate-200 shadow-sm flex flex-col md:flex-row md:items-center justify-between gap-4">
        <!-- Abas de Filtro de Origem -->
        <div class="flex items-center gap-1.5 bg-slate-100 p-1 rounded-xl">
          <button
            v-for="aba in abas"
            :key="aba.id"
            @click="abaAtiva = aba.id"
            :class="[
              'px-3.5 py-1.5 text-xs font-bold rounded-lg transition',
              abaAtiva === aba.id
                ? 'bg-white text-slate-900 shadow-sm'
                : 'text-slate-500 hover:text-slate-800'
            ]"
          >
            {{ aba.nome }}
          </button>
        </div>

        <!-- Campo de Busca em Tempo Real -->
        <div class="flex items-center gap-2 flex-1 max-w-md">
          <div class="relative w-full">
            <span class="absolute inset-y-0 left-0 flex items-center pl-3 text-slate-400">
              🔍
            </span>
            <input
              v-model="termoBusca"
              type="text"
              placeholder="Buscar por número (VB-..., OS-...), cliente ou atendente..."
              class="w-full pl-9 pr-3 py-2 rounded-xl border border-slate-200 text-xs shadow-sm focus:border-teal-500 focus:ring-1 focus:ring-teal-500 outline-none"
            />
          </div>
          <button
            v-if="termoBusca"
            @click="termoBusca = ''"
            class="text-xs text-slate-400 hover:text-slate-600 font-bold px-2 py-1"
          >
            Limpar
          </button>
        </div>
      </div>

      <!-- Tabela de Vendas -->
      <div class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
        <div v-if="listaVendasFiltradas.length === 0" class="text-center py-16 px-4">
          <div class="w-16 h-16 rounded-full bg-slate-100 text-slate-400 flex items-center justify-center mx-auto mb-3 text-2xl">
            📦
          </div>
          <h3 class="text-sm font-bold text-slate-800">Nenhuma venda encontrada</h3>
          <p class="text-xs text-slate-500 mt-1 max-w-sm mx-auto">
            Não há registros de vendas correspondentes aos filtros selecionados.
          </p>
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-left border-collapse text-xs">
            <thead class="bg-slate-50 text-slate-500 uppercase text-[10px] font-black border-b border-slate-200">
              <tr>
                <th class="py-3.5 px-4">Canal / Origem</th>
                <th class="py-3.5 px-4">Identificador</th>
                <th class="py-3.5 px-4">Data / Hora</th>
                <th class="py-3.5 px-4">Cliente / Paciente</th>
                <th class="py-3.5 px-4">Atendente</th>
                <th class="py-3.5 px-4">Forma Pagto</th>
                <th class="py-3.5 px-4 text-right">Valor Líquido</th>
                <th class="py-3.5 px-4 text-center">Ações</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100 font-medium text-slate-700">
              <tr 
                v-for="v in listaVendasFiltradas" 
                :key="v.id || v.Id" 
                class="hover:bg-slate-50/70 transition"
              >
                <!-- Canal -->
                <td class="py-3 px-4">
                  <span
                    v-if="(v.tipo || v.Tipo) === 'BALCAO'"
                    class="bg-amber-50 text-amber-700 border border-amber-200 text-[10px] font-black px-2 py-0.5 rounded-md uppercase"
                  >
                    PDV Balcão
                  </span>
                  <span
                    v-else
                    class="bg-indigo-50 text-indigo-700 border border-indigo-200 text-[10px] font-black px-2 py-0.5 rounded-md uppercase"
                  >
                    Ordem de Serviço
                  </span>
                </td>

                <!-- Identificador -->
                <td class="py-3 px-4 font-mono font-black text-slate-900">
                  {{ v.numeroVenda || v.NumeroVenda }}
                </td>

                <!-- Data / Hora -->
                <td class="py-3 px-4 text-slate-500 font-mono">
                  {{ v.dataVenda || v.DataVenda }}
                </td>

                <!-- Cliente -->
                <td class="py-3 px-4 font-bold text-slate-900">
                  {{ v.clienteNome || v.ClienteNome }}
                </td>

                <!-- Atendente / Vendedor -->
                <td class="py-3 px-4 text-slate-600">
                  {{ v.vendedorNome || v.VendedorNome }}
                </td>

                <!-- Forma Pagamento -->
                <td class="py-3 px-4">
                  <span class="bg-slate-100 text-slate-700 text-[10px] font-bold px-2 py-1 rounded">
                    {{ v.formaPagamento || v.FormaPagamento || 'A COMBINAR' }}
                  </span>
                </td>

                <!-- Valor Líquido -->
                <td class="py-3 px-4 text-right font-mono font-black text-slate-900">
                  R$ {{ formatMoney(v.valorTotalLiquido || v.ValorTotalLiquido) }}
                </td>

                <!-- Ações -->
                <td class="py-3 px-4 text-center">
                  <a
                    v-if="(v.tipo || v.Tipo) === 'BALCAO'"
                    :href="`/vendas/${v.id || v.Id}/comprovante`"
                    target="_blank"
                    class="inline-flex items-center gap-1 text-teal-600 hover:text-teal-800 font-bold hover:underline"
                  >
                    <span>🖨️ Cupom</span>
                  </a>
                  <a
                    v-else
                    href="/ordens-servico"
                    class="inline-flex items-center gap-1 text-indigo-600 hover:text-indigo-800 font-bold hover:underline"
                  >
                    <span>👁️ Ver OS</span>
                  </a>
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
import { ref, computed } from 'vue'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  Vendas: Array,
  vendas: Array,
  TotalFaturamento: Number,
  totalFaturamento: Number,
  TotalVendasBalcao: Number,
  totalVendasBalcao: Number,
  TotalVendasOs: Number,
  totalVendasOs: Number,
  Busca: String,
  busca: String
})

const termoBusca = ref(props.Busca || props.busca || '')
const abaAtiva = ref('todas')

const abas = [
  { id: 'todas', nome: 'Todas as Vendas' },
  { id: 'balcao', nome: 'Apenas Balcão (VB)' },
  { id: 'os', nome: 'Apenas OSs de Grau' }
]

const todasVendas = computed(() => props.Vendas || props.vendas || [])

const countBalcao = computed(() => {
  return todasVendas.value.filter(v => (v.tipo || v.Tipo) === 'BALCAO').length
})

const countOs = computed(() => {
  return todasVendas.value.filter(v => (v.tipo || v.Tipo) !== 'BALCAO').length
})

const listaVendasFiltradas = computed(() => {
  let lista = todasVendas.value

  // Filtro de aba
  if (abaAtiva.value === 'balcao') {
    lista = lista.filter(v => (v.tipo || v.Tipo) === 'BALCAO')
  } else if (abaAtiva.value === 'os') {
    lista = lista.filter(v => (v.tipo || v.Tipo) !== 'BALCAO')
  }

  // Filtro de termo de busca
  if (termoBusca.value.trim()) {
    const t = termoBusca.value.trim().toLowerCase()
    lista = lista.filter(v => {
      const num = (v.numeroVenda || v.NumeroVenda || '').toLowerCase()
      const cli = (v.clienteNome || v.ClienteNome || '').toLowerCase()
      const vend = (v.vendedorNome || v.VendedorNome || '').toLowerCase()
      const forma = (v.formaPagamento || v.FormaPagamento || '').toLowerCase()
      return num.includes(t) || cli.includes(t) || vend.includes(t) || forma.includes(t)
    })
  }

  return lista
})

const totalFaturamentoCalculado = computed(() => {
  return listaVendasFiltradas.value.reduce((acc, v) => {
    const val = Number(v.valorTotalLiquido || v.ValorTotalLiquido || 0)
    return acc + val
  }, 0)
})

const formatMoney = (val) => {
  if (val === undefined || val === null || isNaN(val)) return '0,00'
  return Number(val).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}
</script>
