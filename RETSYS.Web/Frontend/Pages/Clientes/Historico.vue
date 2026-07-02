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
          <span 
            :class="[(os.IsRetroativa || os.isRetroativa) ? 'bg-amber-500 ring-amber-50' : 'bg-teal-600 ring-teal-50']"
            class="absolute -left-10 md:-left-14 top-6 w-4 h-4 rounded-full border-4 border-slate-50 ring-4 flex items-center justify-center"
          ></span>

          <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2 border-b border-slate-100 pb-3">
            <div>
              <div class="flex items-center gap-2">
                <span class="text-xs font-mono font-bold bg-slate-950 text-teal-400 px-2.5 py-0.5 rounded shadow-sm">
                  {{ os.numeroOS || os.NumeroOS }}
                </span>
                <span v-if="os.IsRetroativa || os.isRetroativa" class="text-[9px] font-black uppercase tracking-wider bg-amber-100 text-amber-800 px-2 py-0.5 rounded">
                  Ficha Antiga (CRM)
                </span>
              </div>
              
              <h3 class="text-sm font-bold text-slate-800 mt-1.5">
                Lente: {{ (os.IsRetroativa || os.isRetroativa) ? (os.LenteManual || os.lenteManual) : (os.tipoLente || os.TipoLente || 'Não especificada') }}
              </h3>
              <p v-if="(os.IsRetroativa || os.isRetroativa)" class="text-xs text-slate-500 mt-0.5 font-medium">
                Armação: {{ os.ArmacaoManual || os.armacaoManual }}
              </p>
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
              <p class="text-[10px] font-black uppercase text-slate-400 tracking-wider">Refração Registrada</p>
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

          <div v-if="extrairUrlReceita(os.ObsReceita || os.obsReceita)" class="mt-3 pt-3 border-t border-slate-100 space-y-2">
            <p class="text-[10px] font-black uppercase text-slate-400 tracking-wider">Receita Médica Digitalizada</p>
            <div class="relative w-fit">
              <a 
                :href="extrairUrlReceita(os.ObsReceita || os.obsReceita)" 
                target="_blank" 
                class="block rounded-xl overflow-hidden border border-slate-200 shadow-sm max-w-xs hover:opacity-90 transition relative group"
              >
                <img 
                  :src="extrairUrlReceita(os.ObsReceita || os.obsReceita)" 
                  alt="Receita antiga digitalizada" 
                  class="max-h-32 w-full object-cover" 
                />
                <div class="bg-slate-950/80 text-white text-[10px] font-bold py-1.5 text-center uppercase tracking-wider">
                  🔍 Expandir Receita
                </div>
              </a>
            </div>
          </div>

          <p class="text-[11px] text-slate-400 italic">
            Especialista responsável: {{ (os.medico || os.Medico) ? 'Dr(a). ' + (os.medico || os.Medico) : 'Não Informado' }}
          </p>
        </div>

      </div>
    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { Link } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

defineProps({
  cliente: Object,
  Cliente: Object,
  historico: Array,
  Historico: Array
})

const formatarMoeda = (valor) => {
  if (valor === undefined || valor === null) return '0,00'
  return Number(valor).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const formatarData = (dataRaw) => {
  if (!dataRaw) return '--/--/----'
  return new Date(dataRaw).toLocaleDateString('pt-BR')
}

// Localiza a string do caminho físico da imagem da receita gravada no banco (Áudio 3)
const extrairUrlReceita = (obsText) => {
  if (!obsText) return null
  const match = obsText.match(/\/uploads\/receitas\/[a-zA-Z0-9-]+.[a-zA-Z0-9]+/);
  return match ? match[0] : null
}

const obterBlocoGraus = (os) => {
  return os?.graus || os?.Graus || os?.especificacoes || os?.Especificacoes || os
}

// Extração e cálculo reativo de refração para objetos planos e sub-objetos clínicos do EF
const obterGrau = (os, chave) => {
  const bloco = obterBlocoGraus(os)
  if (!bloco) return '0.00'
  
  // Mapeamento defensivo unificado para suportar o retorno plano do controlador novo
  const mapping = {
    esfericoLongeDireito: bloco.esfericoLongeDireito ?? bloco.EsfericoLongeDireito ?? bloco.odEsferico ?? bloco.OdEsferico ?? 0,
    cilindricoLongeDireito: bloco.cilindricoLongeDireito ?? bloco.CilindricoLongeDireito ?? bloco.odCilindrico ?? bloco.OdCilindrico ?? 0,
    eixoLongeDireito: bloco.eixoLongeDireito ?? bloco.EixoLongeDireito ?? bloco.odEixo ?? bloco.OdEixo ?? 0,
    esfericoLongeEsquerdo: bloco.esfericoLongeEsquerdo ?? bloco.EsfericoLongeEsquerdo ?? bloco.oeEsferico ?? bloco.OeEsferico ?? 0,
    cilindricoLongeEsquerdo: bloco.cilindricoLongeEsquerdo ?? bloco.CilindricoLongeEsquerdo ?? bloco.oeCilindrico ?? bloco.OeCilindrico ?? 0,
    eixoLongeEsquerdo: bloco.eixoLongeEsquerdo ?? bloco.EixoLongeEsquerdo ?? bloco.oeEixo ?? bloco.OeEixo ?? 0,
    adicao: bloco.adicao ?? bloco.Adicao ?? 0,
    esfericoPertoDireito: bloco.esfericoPertoDireito ?? bloco.EsfericoPertoDireito ?? (Number(bloco.odEsferico ?? 0) + Number(bloco.adicao ?? 0)),
    esfericoPertoEsquerdo: bloco.esfericoPertoEsquerdo ?? bloco.EsfericoPertoEsquerdo ?? (Number(bloco.oeEsferico ?? 0) + Number(bloco.adicao ?? 0))
  }
  
  const valor = mapping[chave] ?? 0
  
  // Exibe o Eixo como inteiro e o Grau Clínico com duas casas decimais
  if (chave.toLowerCase().includes('eixo')) {
    return Math.round(valor).toString()
  }
  return Number(valor).toFixed(2)
}
</script>