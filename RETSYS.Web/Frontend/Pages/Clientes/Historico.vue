<template>
  <div class="min-h-screen bg-slate-50 p-4 md:p-8 font-sans text-slate-900">
    <div class="max-w-4xl mx-auto space-y-6">
      
      <div class="flex items-center justify-between">
        <Link 
          href="/clientes" 
          class="text-xs font-bold text-teal-600 hover:text-teal-700 uppercase tracking-wider transition"
        >
          &larr; Voltar para Clientes
        </Link>
        <span class="text-[10px] font-mono text-slate-400 uppercase tracking-widest">Prontuário Histórico</span>
      </div>

      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm flex flex-col sm:flex-row justify-between sm:items-center gap-4">
        <div>
          <h1 class="text-2xl font-black text-slate-950 tracking-tight">{{ cliente.nome }}</h1>
          <p class="text-xs text-slate-400 font-mono mt-1">
            CPF: {{ cliente.cpf }} • Celular: {{ cliente.celular }}
          </p>
        </div>
        <div class="bg-slate-50 px-5 py-2.5 rounded-xl border border-slate-150 text-center shrink-0">
          <p class="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Ordens Geradas</p>
          <p class="text-xl font-black text-slate-950 font-mono">{{ historico.length }}</p>
        </div>
      </div>

      <div v-if="historico.length === 0" class="text-center py-16 bg-white rounded-2xl border border-slate-200 shadow-sm">
        <p class="text-slate-400 text-sm font-medium">Este paciente ainda não possui ordens de serviço ou receitas registradas.</p>
      </div>

      <div v-else class="space-y-8 relative before:absolute before:inset-0 before:left-4 md:before:left-6 before:w-0.5 before:bg-slate-200 before:block pl-10 md:pl-14">
        
        <div 
          v-for="(os, index) in historico" 
          :key="index" 
          class="relative bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4"
        >
          <span class="absolute -left-10 md:-left-14 top-6 w-4 h-4 rounded-full bg-teal-600 border-4 border-slate-50 ring-4 ring-teal-50 flex items-center justify-center"></span>

          <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2 border-b border-slate-100 pb-3">
            <div>
              <span class="text-xs font-mono font-bold bg-slate-950 text-teal-400 px-2.5 py-0.5 rounded shadow-sm">
                {{ os.numeroOS }}
              </span>
              <h3 class="text-sm font-bold text-slate-800 mt-1.5">Lente: {{ os.tipoLente }}</h3>
            </div>
            <div class="text-sm sm:text-right">
              <p class="text-xs font-medium text-slate-400">{{ new Date(os.dataVenda).toLocaleDateString('pt-BR') }}</p>
              <p class="font-black text-slate-950 font-mono text-sm mt-0.5">R$ {{ os.valorTotal.toFixed(2) }}</p>
            </div>
          </div>

          <div class="space-y-2">
            <div class="flex items-center justify-between">
              <p class="text-[10px] font-black uppercase text-slate-400 tracking-wider">Refração Cadastrada</p>
              <span class="text-[10px] bg-slate-100 text-slate-700 px-2 py-0.5 rounded font-medium">
                Status: {{ os.status }}
              </span>
            </div>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-3 text-xs font-mono">
              <div class="bg-slate-50 p-3 rounded-xl border border-slate-150 space-y-1">
                <p class="font-sans font-bold text-slate-400 text-[10px] uppercase">Visão de Longe</p>
                <p>OD (Esf/Cil): <span class="font-bold text-slate-800">{{ os.graus.esfericoLongeDireito.toFixed(2) }} / {{ os.graus.cilindricoLongeDireito.toFixed(2) }}</span></p>
                <p>OE (Esf/Cil): <span class="font-bold text-slate-800">{{ os.graus.esfericoLongeEsquerdo.toFixed(2) }} / {{ os.graus.cilindricoLongeEsquerdo.toFixed(2) }}</span></p>
              </div>

              <div class="bg-teal-50/20 p-3 rounded-xl border border-teal-100/40 space-y-1">
                <div class="flex justify-between items-center">
                  <p class="font-sans font-bold text-teal-600 text-[10px] uppercase">Visão de Perto</p>
                  <span class="text-[9px] font-bold text-teal-700 bg-teal-50 px-1 rounded">Adição: +{{ os.graus.adicao.toFixed(2) }}</span>
                </div>
                <p>OD (Esférico): <span class="font-bold text-slate-800">{{ os.graus.esfericoPertoDireito.toFixed(2) }}</span></p>
                <p>OE (Esférico): <span class="font-bold text-slate-800">{{ os.graus.esfericoPertoEsquerdo.toFixed(2) }}</span></p>
              </div>
            </div>
          </div>

          <p class="text-[11px] text-slate-400 italic">Especialista responsável: Dr(a). {{ os.medico }}</p>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { Link } from '@inertiajs/vue3'

defineProps({
  cliente: Object,
  historico: Array
})
</script>