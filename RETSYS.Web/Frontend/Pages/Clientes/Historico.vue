<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-4xl mx-auto">
      
      <div class="flex items-center justify-between">
        <Link 
          href="/clientes" 
          class="text-xs font-bold text-teal-600 hover:text-teal-700 uppercase tracking-wider transition inline-flex items-center gap-1"
        >
          &larr; Voltar para Clientes
        </Link>
        <span class="text-[10px] font-mono text-slate-400 uppercase tracking-widest">Prontuário Histórico</span>
      </div>

      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm flex flex-col sm:flex-row justify-between sm:items-center gap-4">
        <div>
          <h1 class="text-2xl font-black text-slate-950 tracking-tight">
            {{ (cliente ?? Cliente)?.nome || (cliente ?? Cliente)?.Nome }}
          </h1>
          <p class="text-xs text-slate-400 font-mono mt-1">
            CPF: {{ (cliente ?? Cliente)?.cpf || (cliente ?? Cliente)?.CPF }} • 
            Celular: {{ (cliente ?? Cliente)?.celular || (cliente ?? Cliente)?.Celular }}
          </p>
        </div>
        <div class="bg-slate-50 px-5 py-2.5 rounded-xl border border-slate-150 text-center shrink-0">
          <p class="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Ordens Geradas</p>
          <p class="text-xl font-black text-slate-950 font-mono">
            {{ (Historico ?? historico)?.length || 0 }}
          </p>
        </div>
      </div>

      <div v-if="!(Historico ?? historico) || (Historico ?? historico).length === 0" class="text-center py-16 bg-white rounded-2xl border border-slate-200 shadow-sm">
        <p class="text-slate-400 text-sm font-medium">Este paciente ainda não possui ordens de serviço ou receitas registradas.</p>
      </div>

      <div v-else class="space-y-8 relative before:absolute before:inset-0 before:left-4 md:before:left-6 before:w-0.5 before:bg-slate-200 before:block pl-10 md:pl-14">
        
        <div 
          v-for="(os, index) in (Historico ?? historico)" 
          :key="index" 
          class="relative bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4"
        >
          <span class="absolute -left-10 md:-left-14 top-6 w-4 h-4 rounded-full bg-teal-600 border-4 border-slate-50 ring-4 ring-teal-50 flex items-center justify-center"></span>

          <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2 border-b border-slate-100 pb-3">
            <div>
              <span class="text-xs font-mono font-bold bg-slate-950 text-teal-400 px-2.5 py-0.5 rounded shadow-sm">
                {{ os.numeroOS || os.NumeroOS }}
              </span>
              <h3 class="text-sm font-bold text-slate-800 mt-1.5">Lente: {{ os.tipoLente || os.TipoLente }}</h3>
            </div>
            <div class="text-sm sm:text-right">
              <p class="text-xs font-medium text-slate-400">
                {{ formatarData(os.dataVenda || os.DataVenda) }}
              </p>
              <p class="font-black text-slate-950 font-mono text-sm mt-0.5">
                R$ {{ formatarMoeda(os.valorTotal ?? os.ValorTotal) }}
              </p>
            </div>
          </div>

          <div class="space-y-2">
            <div class="flex items-center justify-between">
              <p class="text-[10px] font-black uppercase text-slate-400 tracking-wider">Refração Cadastrada</p>
              <span class="text-[10px] bg-slate-100 text-slate-700 px-2 py-0.5 rounded font-medium">
                Status: {{ os.status || os.Status }}
              </span>
            </div>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-3 text-xs font-mono">
              <div class="bg-slate-50 p-3 rounded-xl border border-slate-150 space-y-1">
                <p class="font-sans font-bold text-slate-400 text-[10px] uppercase">Visão de Longe</p>
                <p>OD (Esf/Cil): <span class="font-bold text-slate-800">{{ obterGrau(os, 'esfericoLongeDireito') }} / {{ obterGrau(os, 'cilindricoLongeDireito') }}</span></p>
                <p>OE (Esf/Cil): <span class="font-bold text-slate-800">{{ obterGrau(os, 'esfericoLongeEsquerdo') }} / {{ obterGrau(os, 'cilindricoLongeEsquerdo') }}</span></p>
              </div>

              <div class="bg-teal-50/20 p-3 rounded-xl border border-teal-100/40 space-y-1">
                <div class="flex justify-between items-center">
                  <p class="font-sans font-bold text-teal-600 text-[10px] uppercase">Visão de Perto</p>
                  <span class="text-[9px] font-bold text-teal-700 bg-teal-50 px-1 rounded">
                    Adição: +{{ obterGrau(os, 'adicao') }}
                  </span>
                </div>
                <p>OD (Esférico): <span class="font-bold text-slate-800">{{ obterGrau(os, 'esfericoPertoDireito') }}</span></p>
                <p>OE (Esférico): <span class="font-bold text-slate-800">{{ obterGrau(os, 'esfericoPertoEsquerdo') }}</span></p>
              </div>
            </div>
          </div>

          <p class="text-[11px] text-slate-400 italic">Especialista responsável: Dr(a). {{ os.medico || os.Medico }}</p>
        </div>

      </div>
    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { Link } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

// Suporte formal para as chaves em PascalCase enviadas pelo C#
defineProps({
  cliente: Object,
  Cliente: Object,
  historico: Array,
  Historico: Array
})

// Funções de auxílio defensivo contra propriedades ausentes ou nulas
const formatarMoeda = (valor) => {
  if (valor === undefined || valor === null) return '0,00'
  return Number(valor).toFixed(2)
}

const formatarData = (dataRaw) => {
  if (!dataRaw) return '--/--/----'
  return new Date(dataRaw).toLocaleDateString('pt-BR')
}

// Extrai dinamicamente os sub-blocos clínicos independentemente do padrão de serialização
const obterBlocoGraus = (os) => {
  return os?.graus || os?.Graus || os?.especificacoes || os?.Especificacoes || os
}

const obterGrau = (os, chave) => {
  const bloco = obterBlocoGraus(os)
  if (!bloco) return '0.00'
  
  const chavePascal = chave.charAt(0).toUpperCase() + chave.slice(1)
  const valor = bloco[chave] ?? bloco[chavePascal] ?? 0
  return Number(valor).toFixed(2)
}
</script>