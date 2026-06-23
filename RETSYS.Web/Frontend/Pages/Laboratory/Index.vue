<template>
  <div class="min-h-screen bg-slate-50 p-4 md:p-8 font-sans text-slate-900">
    <div class="max-w-6xl mx-auto space-y-6">
      
      <div class="flex items-center justify-between bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <div>
          <h1 class="text-2xl font-black text-slate-950">Esteira do Laboratório</h1>
          <p class="text-sm text-slate-500">Ordem de trabalho para surfaçagem, montagem e conferência de lentes.</p>
        </div>
        <span class="bg-amber-50 text-amber-700 border border-amber-200 px-3 py-1 rounded-full text-xs font-bold animate-pulse">
          {{ FilaMontagem?.length || 0 }} ODS Pendentes
        </span>
      </div>

      <div v-if="!FilaMontagem || FilaMontagem.length === 0" class="text-center py-16 bg-white rounded-2xl border border-slate-200 shadow-sm max-w-6xl mx-auto">
        <p class="text-slate-400 text-sm font-medium">Todas as lentes foram montadas! Fila de ordens vazia.</p>
      </div>

      <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div 
          v-for="os in FilaMontagem" 
          :key="os.id || os.Id" 
          class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden flex flex-col justify-between"
        >
          <div class="bg-slate-950 text-white p-4 flex items-center justify-between">
            <div>
              <span class="font-mono text-xs font-bold bg-slate-800 px-2.5 py-1 rounded text-teal-400">
                {{ os.numeroOS || os.NumeroOS }}
              </span>
              <p class="text-xs text-slate-400 mt-1 truncate">
                Paciente: {{ os.clienteNome || os.ClienteNome }}
              </p>
            </div>
            <span class="text-[10px] uppercase font-bold tracking-wider text-slate-400 bg-slate-900 px-2 py-1 rounded">
              {{ os.tipoLente || os.TipoLente }}
            </span>
          </div>

          <div class="p-4 space-y-4 flex-1">
            
            <div class="space-y-1.5">
              <p class="text-[10px] font-black uppercase text-slate-400 tracking-wider">Refração de Longe</p>
              <div class="grid grid-cols-2 gap-2 text-xs font-mono">
                
                <div class="bg-slate-50 p-2.5 rounded-xl border border-slate-150">
                  <p class="font-sans font-bold text-slate-400 text-[10px] mb-1">OD (Direito)</p>
                  <p>ESF: <span class="font-bold text-slate-800">{{ formatGrau((os.especificacoes || os.Especificacoes)?.esfericoLongeDireito ?? (os.especificacoes || os.Especificacoes)?.EsfericoLongeDireito) }}</span></p>
                  <p>CIL: <span class="font-bold text-slate-800">{{ formatGrau((os.especificacoes || os.Especificacoes)?.cilindricoLongeDireito ?? (os.especificacoes || os.Especificacoes)?.CilindricoLongeDireito) }}</span></p>
                  <p>EIXO: <span class="font-bold text-slate-800">{{ (os.especificacoes || os.Especificacoes)?.eixoLongeDireito ?? (os.especificacoes || os.Especificacoes)?.EixoLongeDireito }}°</span></p>
                </div>
                
                <div class="bg-slate-50 p-2.5 rounded-xl border border-slate-150">
                  <p class="font-sans font-bold text-slate-400 text-[10px] mb-1">OE (Esquerdo)</p>
                  <p>ESF: <span class="font-bold text-slate-800">{{ formatGrau((os.especificacoes || os.Especificacoes)?.esfericoLongeEsquerdo ?? (os.especificacoes || os.Especificacoes)?.EsfericoLongeEsquerdo) }}</span></p>
                  <p>CIL: <span class="font-bold text-slate-800">{{ formatGrau((os.especificacoes || os.Especificacoes)?.cilindricoLongeEsquerdo ?? (os.especificacoes || os.Especificacoes)?.CilindricoLongeEsquerdo) }}</span></p>
                  <p>EIXO: <span class="font-bold text-slate-800">{{ (os.especificacoes || os.Especificacoes)?.eixoLongeEsquerdo ?? (os.especificacoes || os.Especificacoes)?.EixoLongeEsquerdo }}°</span></p>
                </div>

              </div>
            </div>

            <div class="space-y-1.5">
              <div class="flex justify-between items-center">
                <p class="text-[10px] font-black uppercase text-slate-400 tracking-wider">Refração de Perto</p>
                <span class="text-[9px] bg-teal-50 text-teal-700 font-bold px-1.5 py-0.5 rounded font-mono">
                  AD: +{{ formatGrau((os.especificacoes || os.Especificacoes)?.adicao ?? (os.especificacoes || os.Especificacoes)?.Adicao) }}
                </span>
              </div>
              <div class="grid grid-cols-2 gap-2 text-xs font-mono">
                
                <div class="bg-teal-50/20 p-2.5 rounded-xl border border-teal-100/50">
                  <p class="font-sans font-bold text-teal-600 text-[10px] mb-1">OD (Direito)</p>
                  <p>ESF: <span class="font-bold text-slate-800">{{ formatGrau((os.especificacoes || os.Especificacoes)?.esfericoPertoDireito ?? (os.especificacoes || os.Especificacoes)?.EsfericoPertoDireito) }}</span></p>
                  <p>CIL: <span class="font-bold text-slate-800">{{ formatGrau((os.especificacoes || os.Especificacoes)?.cilindricoPertoDireito ?? (os.especificacoes || os.Especificacoes)?.CilindricoPertoDireito) }}</span></p>
                  <p>EIXO: <span class="font-bold text-slate-800">{{ (os.especificacoes || os.Especificacoes)?.eixoPertoDireito ?? (os.especificacoes || os.Especificacoes)?.EixoPertoDireito }}°</span></p>
                </div>
                
                <div class="bg-teal-50/20 p-2.5 rounded-xl border border-teal-100/50">
                  <p class="font-sans font-bold text-teal-600 text-[10px] mb-1">OE (Esquerdo)</p>
                  <p>ESF: <span class="font-bold text-slate-800">{{ formatGrau((os.especificacoes || os.Especificacoes)?.esfericoPertoEsquerdo ?? (os.especificacoes || os.Especificacoes)?.EsfericoPertoEsquerdo) }}</span></p>
                  <p>CIL: <span class="font-bold text-slate-800">{{ formatGrau((os.especificacoes || os.Especificacoes)?.cilindricoPertoEsquerdo ?? (os.especificacoes || os.Especificacoes)?.CilindricoPertoEsquerdo) }}</span></p>
                  <p>EIXO: <span class="font-bold text-slate-800">{{ (os.especificacoes || os.Especificacoes)?.eixoPertoEsquerdo ?? (os.especificacoes || os.Especificacoes)?.EixoPertoEsquerdo }}°</span></p>
                </div>

              </div>
            </div>

          </div>

          <div class="p-4 bg-slate-50 border-t border-slate-100">
            <button 
              @click="concluirMontagemLente(os.id || os.Id, os.numeroOS || os.NumeroOS)"
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
  FilaMontagem: Array
})

const concluirMontagemLente = (id, numeroOS) => {
  if (confirm(`Confirmar que as lentes da ${numeroOS} foram conferidas no lensômetro e montadas com sucesso?`)) {
    router.post(`/ordens/alterar-status/${id}?novoStatus=Pronto`, {}, {
      onSuccess: () => {
        alert('Ordem de serviço movida para a gaveta de [Pronto para Entrega] no balcão!')
      }
    })
  }
}

// Auxiliar defensivo para evitar quebras de renderização caso o grau venha nulo ou indefinido
const formatGrau = (valor) => {
  if (valor === undefined || valor === null) return '0.00'
  return Number(valor).toFixed(2)
}
</script>