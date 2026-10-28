<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-7xl mx-auto">
      
      <!-- Cabeçalho -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <div>
          <h1 class="text-2xl font-black text-slate-900">Histórico de Vendas Balcão</h1>
          <p class="text-sm text-slate-500">Registro de todas as saídas de armações solares, produtos e acessórios de pronta-entrega.</p>
        </div>

        <div class="flex items-center gap-3">
          <a
            href="/vendas/pdv"
            class="bg-teal-600 hover:bg-teal-700 text-white font-bold py-2.5 px-5 rounded-xl text-xs uppercase tracking-wider transition shadow-sm flex items-center gap-2"
          >
            <span>+ Nova Venda no PDV</span>
          </a>
        </div>
      </div>

      <!-- Barra de Filtro / Busca -->
      <div class="bg-white p-4 rounded-2xl border border-slate-200 shadow-sm flex items-center gap-3">
        <input
          v-model="termoBusca"
          @keyup.enter="buscar"
          type="text"
          placeholder="Buscar por número da venda (VB-...) ou nome do cliente..."
          class="flex-1 p-2.5 rounded-xl border border-slate-300 text-xs shadow-sm focus:border-teal-500"
        />
        <button
          @click="buscar"
          class="bg-slate-900 hover:bg-slate-800 text-white font-bold px-5 py-2.5 rounded-xl text-xs"
        >
          Filtrar
        </button>
      </div>

      <!-- Tabela de Vendas -->
      <div class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
        <div v-if="listaVendas.length === 0" class="text-center py-16 text-slate-400 text-sm">
          Nenhuma venda encontrada no histórico.
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-left border-collapse text-xs">
            <thead class="bg-slate-50 text-slate-500 uppercase text-[10px] font-black border-b border-slate-200">
              <tr>
                <th class="py-3.5 px-4">Número</th>
                <th class="py-3.5 px-4">Data / Hora</th>
                <th class="py-3.5 px-4">Cliente</th>
                <th class="py-3.5 px-4">Vendedor</th>
                <th class="py-3.5 px-4">Pagamento</th>
                <th class="py-3.5 px-4 text-right">Valor Líquido</th>
                <th class="py-3.5 px-4 text-center">Ações</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100 font-medium text-slate-700">
              <tr v-for="v in listaVendas" :key="v.id || v.Id" class="hover:bg-slate-50/60 transition">
                <td class="py-3 px-4 font-mono font-bold text-teal-700">
                  {{ v.numeroVenda || v.NumeroVenda }}
                </td>
                <td class="py-3 px-4 text-slate-500">
                  {{ v.dataVenda || v.DataVenda }}
                </td>
                <td class="py-3 px-4 font-bold text-slate-900">
                  {{ v.clienteNome || v.ClienteNome }}
                </td>
                <td class="py-3 px-4 text-slate-600">
                  {{ v.vendedorNome || v.VendedorNome }}
                </td>
                <td class="py-3 px-4">
                  <span class="bg-slate-100 text-slate-700 text-[10px] font-bold px-2 py-1 rounded">
                    {{ v.formaPagamento || v.FormaPagamento }}
                  </span>
                </td>
                <td class="py-3 px-4 text-right font-mono font-bold text-slate-900">
                  R$ {{ formatMoney(v.valorTotalLiquido || v.ValorTotalLiquido) }}
                </td>
                <td class="py-3 px-4 text-center">
                  <a
                    :href="`/vendas/${v.id || v.Id}/comprovante`"
                    class="text-teal-600 hover:text-teal-800 font-bold hover:underline"
                  >
                    🖨️ Cupom
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
import { router } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  Vendas: Array,
  vendas: Array,
  Busca: String,
  busca: String
})

const termoBusca = ref(props.Busca || props.busca || '')
const listaVendas = computed(() => props.Vendas || props.vendas || [])

const buscar = () => {
  router.get('/vendas', { busca: termoBusca.value }, { preserveState: true })
}

const formatMoney = (val) => {
  if (val === undefined || val === null) return '0,00'
  return Number(val).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}
</script>

