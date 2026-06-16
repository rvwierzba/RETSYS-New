<template>
  <div class="min-h-screen bg-slate-50 p-4 md:p-8 font-sans text-slate-900">
    <div class="max-w-6xl mx-auto space-y-6">
      
      <div class="flex items-center justify-between bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <div>
          <h1 class="text-2xl font-black text-slate-950">Esteira do Laboratório</h1>
          <p class="text-sm text-slate-500">Ordem de trabalho para surfaçagem, montagem e conferência de lentes.</p>
        </div>
        <span class="bg-amber-50 text-amber-700 border border-amber-200 px-3 py-1 rounded-full text-xs font-bold animate-pulse">
          {{ filaMontagem.length }} ODS Pendentes
        </span>
      </div>

      <div v-if="filaMontagem.length === 0" class="text-center py-16 bg-white rounded-2xl border border-slate-200 shadow-sm max-w-6xl mx-auto">
        <p class="text-slate-400 text-sm font-medium">Todas as lentes foram montadas! Fila de ordens vazia.</p>
      </div>

      <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div 
          v-for="os in filaMontagem" 
          :key="os.id" 
          class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden flex flex-col justify-between"
        >
          <div class="bg-slate-950 text-white p-4 flex items-center justify-between">
            <div>
              <span class="font-mono text-xs font-bold bg-slate-800 px-2.5 py-1 rounded text-teal-400">{{ os.numeroOS }}</span>
              <p class="text-xs text-slate-400 mt-1 truncate">Paciente: {{ os.clienteNome }}</p>
            </div>
            <span class="text-[10px] uppercase font-bold tracking-wider text-slate-400 bg-slate-900 px-2 py-1 rounded">
              {{ os.tipoLente }}
            </span>
          </div>

          <div class="p-4 space-y-4 flex-1">
            
            <div class="space-y-1.5">
              <p class="text-[10px] font-black uppercase text-slate-400 tracking-wider">Refração de Longe</p>
              <div class="grid grid-cols-2 gap-2 text-xs font-mono">
                <div class="bg-slate-50 p-2.5 rounded-xl border border-slate-150">
                  <p class="font-sans font-bold text-slate-400 text-[10px] mb-1">OD (Direito)</p>
                  <p>ESF: <span class="font-bold text-slate-800">{{ os.especificacoes.esfericoLongeDireito.toFixed(2) }}</span></p>
                  <p>CIL: <span class="font-bold text-slate-800">{{ os.especificacoes.cilindricoLongeDireito.toFixed(2) }}</span></p>
                  <p>EIXO: <span class="font-bold text-slate-800">{{ os.especificacoes.eixoLongeDireito }}°</span></p>
                </div>
                <div class="bg-slate-50 p-2.5 rounded-xl border border-slate-150">
                  <p class="font-sans font-bold text-slate-400 text-[10px] mb-1">OE (Esquerdo)</p>
                  <p>ESF: <span class="font-bold text-slate-800">{{ os.especificacoes.esfericoLongeEsquerdo.toFixed(2) }}</span></p>
                  <p>CIL: <span class="font-bold text-slate-800">{{ os.especificacoes.cilindricoLongeEsquerdo.toFixed(2) }}</span></p>
                  <p>EIXO: <span class="font-bold text-slate-800">{{ os.especificacoes.eixoLongeEsquerdo }}°</span></p>
                </div>
              </div>
            </div>

            <div class="space-y-1.5">
              <div class="flex justify-between items-center">
                <p class="text-[10px] font-black uppercase text-slate-400 tracking-wider">Refração de Perto</p>
                <span class="text-[9px] bg-teal-50 text-teal-700 font-bold px-1.5 py-0.5 rounded font-mono">AD: +{{ os.especificacoes.adicao.toFixed(2) }}</span>
              </div>
              <div class="grid grid-cols-2 gap-2 text-xs font-mono">
                <div class="bg-teal-50/20 p-2.5 rounded-xl border border-teal-100/50">
                  <p class="font-sans font-bold text-teal-600 text-[10px] mb-1">OD (Direito)</p>
                  <p>ESF: <span class="font-bold text-slate-800">{{ os.especificacoes.esfericoPertoDireito.toFixed(2) }}</span></p>
                  <p>CIL: <span class="font-bold text-slate-800">{{ os.especificacoes.cilindricoPertoDireito.toFixed(2) }}</span></p>
                  <p>EIXO: <span class="font-bold text-slate-800">{{ os.especificacoes.eixoPertoDireito }}°</span></p>
                </div>
                <div class="bg-teal-50/20 p-2.5 rounded-xl border border-teal-100/50">
                  <p class="font-sans font-bold text-teal-600 text-[10px] mb-1">OE (Esquerdo)</p>
                  <p>ESF: <span class="font-bold text-slate-800">{{ os.especificacoes.esfericoPertoEsquerdo.toFixed(2) }}</span></p>
                  <p>CIL: <span class="font-bold text-slate-800">{{ os.especificacoes.cilindricoPertoEsquerdo.toFixed(2) }}</span></p>
                  <p>EIXO: <span class="font-bold text-slate-800">{{ os.especificacoes.eixoPertoEsquerdo }}°</span></p>
                </div>
              </div>
            </div>

          </div>

          <div class="p-4 bg-slate-50 border-t border-slate-100">
            <button 
              @click="concluirMontagemLente(os.id, os.numeroOS)"
              class="w-full bg-slate-950 hover:bg-teal-600 text-white font-bold py-2.5 rounded-xl text-xs uppercase tracking-wider transition shadow-sm flex items-center justify-center gap-1"
            >
              Concluir Montagem &amp; Liberar Óculos
            </button>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { router } from '@inertiajs/vue3'

defineProps({
  filaMontagem: Array
})

// Reutiliza com maestria a rota de alteração de status que criamos anteriormente no OrdensServicoController
const concluirMontagemLente = (id, numeroOS) => {
  if (confirm(`Confirmar que as lentes da ${numeroOS} foram conferidas no lensômetro e montadas com sucesso?`)) {
    router.post(`/ordens/alterar-status/${id}?novoStatus=Pronto`, {}, {
      onSuccess: () => {
        alert('Ordem de serviço movida para a gaveta de [Pronto para Entrega] no balcão!')
      }
    })
  }
}
</script>