<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-6xl mx-auto">

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

      <div class="grid grid-cols-1 lg:grid-cols-4 gap-4 bg-white p-4 rounded-2xl border border-slate-200 shadow-sm items-center">
        <div class="lg:col-span-3 flex flex-wrap gap-1.5">
          <button 
            @click="filtrarPorComposicao('total')"
            :class="[filtroAtivoComp === 'total' ? 'bg-slate-950 text-white font-black' : 'bg-slate-50 text-slate-600 hover:bg-slate-100 font-semibold']"
            class="px-4 py-2 rounded-xl text-xs uppercase tracking-wider transition border border-transparent"
          >
            📋 Total de Vendas
          </button>
          <button 
            @click="filtrarPorComposicao('armacao')"
            :class="[filtroAtivoComp === 'armacao' ? 'bg-slate-950 text-white font-black' : 'bg-slate-50 text-slate-600 hover:bg-slate-100 font-semibold']"
            class="px-4 py-2 rounded-xl text-xs uppercase tracking-wider transition border"
          >
            👓 Receita de Armações
          </button>
          <button 
            @click="irParaFiltro('lente')"
            :class="[filtroAtivoComp === 'lente' ? 'bg-slate-950 text-white font-bold' : 'bg-slate-50 text-slate-600 font-medium']"
            class="px-4 py-2 rounded-xl text-xs border border-transparent hover:bg-slate-100 transition"
          >
            🔬 Receita de Lentes
          </button>
          <button 
            @click="filtrarPorComposicao('completo')"
            :class="[filtroAtivoComp === 'completo' ? 'bg-slate-950 text-white font-black' : 'bg-slate-50 text-slate-600 hover:bg-slate-100 font-semibold']"
            class="px-4 py-2 rounded-xl text-xs uppercase tracking-wider transition border"
          >
            💎 Óculos Completo
          </button>
        </div>

        <div class="bg-teal-50 border border-teal-200 rounded-xl p-3 text-center lg:text-right">
          <span class="text-[10px] font-bold text-teal-800 uppercase tracking-wider block">Faturamento do Filtro</span>
          <p class="text-lg font-black text-teal-700 font-mono mt-0.5">
            R$ {{ formatarMoeda(totalExibido) }}
          </p>
        </div>
      </div>

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
                <th class="pb-3">Especificação Lente</th>
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
                <td class="py-4 text-slate-600 text-xs">
                  <p class="font-medium text-slate-800">{{ os.tipoLente || os.TipoLente }}</p>
                  <p class="text-[11px] text-slate-400">Dr(a). {{ os.medico || os.Medico }}</p>
                </td>
                <td class="py-4 text-right font-black font-mono text-slate-950">
                  R$ {{ formatarMoeda(os.valorTotal ?? os.ValorTotal) }}
                </td>
                <td class="py-4 text-center">
                  <button 
                    @click="abrirPranchetaClinica(os)"
                    class="bg-slate-950 hover:bg-slate-800 text-white text-xs font-bold px-3 py-1.5 rounded-lg transition shadow-sm font-mono"
                  >
                    Ver Receita
                  </button>
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
  Ordens: Array, ordens: Array,
  FiltroAtivo: String, filtroAtivo: String,
  TotalFiltroAtivo: Number, totalFiltroAtivo: Number
})

const osSelecionada = ref(null)

const filtroAtivoComp = computed(() => props.FiltroAtivo ?? props.filtroAtivo ?? 'total')
const totalExibido = computed(() => props.TotalFiltroAtivo ?? props.totalFiltroAtivo ?? 0)

const irParaNovaOrdem = () => router.get('/ordens/nova')
const filtrarPorComposicao = (tipo) => router.get('/ordens', { filtroComposicao: tipo }, { preserveState: true })
const irParaFiltro = (tipo) => filtrarPorComposicao(tipo)
const abrirPranchetaClinica = (ordem) => { osSelecionada.value = { ...ordem } }

const formatarMoeda = (valor) => {
  if (valor === undefined || valor === null) return '0,00'
  return Number(valor).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const formatarData = (dataRaw) => {
  if (!dataRaw) return '--/--/----'
  return new Date(dataRaw).toLocaleDateString('pt-BR')
}
</script>