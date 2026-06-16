<template>
  <div class="min-h-screen bg-slate-50 p-4 md:p-8 font-sans text-slate-900">
    <div class="max-w-6xl mx-auto space-y-6">

      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <h1 class="text-2xl font-black text-slate-950">Painel de Ordens de Serviço</h1>
          <p class="text-sm text-slate-500">Consulte receitas, especificações de lentes e envie ordens para o laboratório.</p>
        </div>
        <button 
          @click="irParaNovaOrdem"
          class="bg-teal-600 hover:bg-teal-700 text-white font-bold py-2.5 px-6 rounded-xl text-xs transition shadow-sm uppercase tracking-wider"
        >
          + Emitir Nova OS
        </button>
      </div>

      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <div v-if="ordens.length === 0" class="text-center py-12 border-2 border-dashed border-slate-100 rounded-xl text-slate-400 text-sm">
          Nenhuma ordem de serviço emitida no sistema até o momento.
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
              <tr v-for="os in ordens" :key="os.id" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
                <td class="py-4">
                  <p class="font-mono font-bold text-slate-900 text-xs bg-slate-100 px-2 py-0.5 rounded w-fit mb-1">{{ os.numeroOS }}</p>
                  <p class="text-xs text-slate-400">{{ new Date(os.dataVenda).toLocaleDateString('pt-BR') }}</p>
                </td>
                <td class="py-4 font-semibold text-slate-800">{{ os.clienteNome }}</td>
                <td class="py-4 text-slate-600 text-xs">
                  <p class="font-medium text-slate-800">{{ os.tipoLente }}</p>
                  <p class="text-[11px] text-slate-400">Dr(a). {{ os.medico }}</p>
                </td>
                <td class="py-4 text-right font-black font-mono text-slate-950">
                  R$ {{ os.valorTotal.toFixed(2) }}
                </td>
                <td class="py-4 text-center">
                  <button 
                    @click="abrirPranchetaClinica(os)"
                    class="bg-slate-950 hover:bg-slate-800 text-white text-xs font-bold px-3 py-1.5 rounded-lg transition shadow-sm"
                  >
                    Ver Receita
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div v-if="osSelecionada" class="fixed inset-0 bg-slate-950/60 backdrop-blur-sm flex items-center justify-center p-4 z-50">
        <div class="bg-white rounded-3xl border border-slate-200 shadow-2xl w-full max-w-3xl overflow-hidden animate-fadeIn">
          
          <div class="bg-slate-950 text-white p-6 flex items-center justify-between">
            <div>
              <h2 class="text-base font-black uppercase tracking-wider">Prancheta Clínica: {{ osSelecionada.numeroOS }}</h2>
              <p class="text-xs text-slate-400">Paciente: {{ osSelecionada.clienteNome }}</p>
            </div>
            <button @click="osSelecionada = null" class="text-slate-400 hover:text-white text-sm font-bold">&times; Fechar</button>
          </div>

          <div class="p-6 space-y-6">
            
            <div class="space-y-3">
              <h4 class="text-xs font-bold text-teal-600 uppercase tracking-widest border-b border-slate-100 pb-1">Refração de Longe</h4>
              <div class="grid grid-cols-2 gap-4 text-xs font-mono">
                <div class="bg-slate-50 p-3 rounded-xl border border-slate-150">
                  <p class="font-sans font-bold text-slate-400 mb-1">Olho Direito (OD)</p>
                  <p>Esférico: <span class="font-bold text-slate-800">{{ osSelecionada.refracao.esfericoLongeDireito.toFixed(2) }}</span></p>
                  <p>Cilíndrico: <span class="font-bold text-slate-800">{{ osSelecionada.refracao.cilindricoLongeDireito.toFixed(2) }}</span></p>
                  <p>Eixo: <span class="font-bold text-slate-800">{{ osSelecionada.refracao.eixoLongeDireito }}°</span></p>
                </div>
                <div class="bg-slate-50 p-3 rounded-xl border border-slate-150">
                  <p class="font-sans font-bold text-slate-400 mb-1">Olho Esquerdo (OE)</p>
                  <p>Esférico: <span class="font-bold text-slate-800">{{ osSelecionada.refracao.esfericoLongeEsquerdo.toFixed(2) }}</span></p>
                  <p>Cilíndrico: <span class="font-bold text-slate-800">{{ osSelecionada.refracao.cilindricoLongeEsquerdo.toFixed(2) }}</span></p>
                  <p>Eixo: <span class="font-bold text-slate-800">{{ osSelecionada.refracao.eixoLongeEsquerdo }}°</span></p>
                </div>
              </div>
            </div>

            <div class="space-y-3">
              <div class="flex items-center justify-between border-b border-slate-100 pb-1">
                <h4 class="text-xs font-bold text-indigo-600 uppercase tracking-widest">Refração de Perto</h4>
                <span class="text-[10px] bg-indigo-50 text-indigo-700 font-bold font-mono px-2 py-0.5 rounded">Adição: +{{ osSelecionada.refracao.adicao.toFixed(2) }}</span>
              </div>
              <div class="grid grid-cols-2 gap-4 text-xs font-mono">
                <div class="bg-indigo-50/40 p-3 rounded-xl border border-indigo-100">
                  <p class="font-sans font-bold text-indigo-400 mb-1">Olho Direito (OD)</p>
                  <p>Esférico: <span class="font-bold text-indigo-900">{{ osSelecionada.refracao.esfericoPertoDireito.toFixed(2) }}</span></p>
                  <p>Cilíndrico: <span class="font-bold text-indigo-900">{{ osSelecionada.refracao.cilindricoPertoDireito.toFixed(2) }}</span></p>
                  <p>Eixo: <span class="font-bold text-indigo-900">{{ osSelecionada.refracao.eixoPertoDireito }}°</span></p>
                </div>
                <div class="bg-indigo-50/40 p-3 rounded-xl border border-indigo-100">
                  <p class="font-sans font-bold text-indigo-400 mb-1">Olho Esquerdo (OE)</p>
                  <p>Esférico: <span class="font-bold text-indigo-900">{{ osSelecionada.refracao.esfericoPertoEsquerdo.toFixed(2) }}</span></p>
                  <p>Cilíndrico: <span class="font-bold text-indigo-900">{{ osSelecionada.refracao.cilindricoPertoEsquerdo.toFixed(2) }}</span></p>
                  <p>Eixo: <span class="font-bold text-indigo-900">{{ osSelecionada.refracao.eixoPertoEsquerdo }}°</span></p>
                </div>
              </div>
            </div>

          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { router } from '@inertiajs/vue3'

defineProps({
  ordens: Array
})

const osSelecionada = ref(null)

const irParaNovaOrdem = () => {
  router.get('/ordens/nova')
}

const abrirPranchetaClinica = (ordem) => {
  osSelecionada.value = ordem
}
</script>