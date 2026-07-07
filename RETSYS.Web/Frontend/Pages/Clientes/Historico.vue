<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-6xl mx-auto">

      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <div>
          <span class="text-[10px] font-bold uppercase bg-slate-100 text-slate-600 px-2.5 py-1 rounded-md font-mono border">Prontuário Clínico Integrado</span>
          <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight mt-2">
            {{ infoCliente.nome }}
          </h1>
          <p class="text-xs text-slate-500 mt-1">
            <strong>CPF:</strong> {{ infoCliente.cpf }} | <strong>WhatsApp:</strong> {{ infoCliente.telefone || 'Não informado' }} | <strong>Convênio:</strong> {{ infoCliente.convenio || 'Particular' }}
          </p>
        </div>
        
        <div class="bg-teal-50 border border-teal-200 rounded-2xl p-4 min-w-[220px] text-center md:text-right">
          <span class="text-[10px] font-bold text-teal-800 uppercase tracking-wider block">Faturamento Total Acumulado</span>
          <p class="text-xl font-black text-teal-700 font-mono mt-0.5">
            R$ {{ formatarMoeda(props.TotalGasto) }}
          </p>
          <span class="text-[9px] text-teal-600 block mt-0.5 leading-none">Saldo Legado + Ordens do Sistema</span>
        </div>
      </div>

      <div v-if="infoCliente.dataUltimaCompra || infoCliente.valorGasto" class="bg-amber-50/40 border border-amber-200 rounded-2xl p-6 space-y-4">
        <div class="flex items-center gap-2 text-amber-900 font-bold font-mono text-xs uppercase border-b border-amber-200/60 pb-2">
          <span>📝 Registro Informativo de Ficha Física Antiga (Migração CRM)</span>
        </div>
        
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4 text-xs">
          <div class="bg-white p-3 rounded-xl border border-amber-200/40">
            <p class="text-slate-400 font-bold text-[10px] uppercase">Último Produto Adquirido</p>
            <p class="font-bold text-slate-800 mt-0.5">{{ infoCliente.produtoAdquirido || 'Não especificado na ficha' }}</p>
          </div>
          <div class="bg-white p-3 rounded-xl border border-amber-200/60">
            <p class="text-slate-400 font-bold uppercase text-[10px]">Gasto na Ficha Antiga</p>
            <p class="font-black text-amber-950 font-mono mt-0.5">R$ {{ formatarMoeda(infoCliente.valorGasto) }}</p>
          </div>
          <div class="bg-white p-3 rounded-xl border border-amber-200/60">
            <p class="text-slate-400 font-bold uppercase text-[10px]">Data Última Compra (Papel)</p>
            <p class="font-bold text-slate-700 font-mono mt-0.5">{{ formatarData(infoCliente.dataUltimaCompra) }}</p>
          </div>
        </div>

        <div class="bg-white p-4 rounded-xl border border-amber-200/60 space-y-3">
          <p class="font-black text-amber-950 uppercase text-[10px] tracking-wider">Último Grau Clínico Conhecido no Arquivo Morto</p>
          <div class="grid grid-cols-2 gap-4 font-mono text-xs">
            <div class="bg-slate-50/60 p-3 rounded-lg border border-slate-100">
              <p class="font-sans font-bold text-slate-400 text-[10px] uppercase mb-1">Olho Direito (OD)</p>
              <p>Esférico: <strong class="text-slate-800">{{ infoCliente.ultimaOdEsferico?.toFixed(2) ?? '0.00' }}</strong></p>
              <p>Cilíndrico: <strong class="text-slate-800">{{ infoCliente.ultimaOdCilindrico?.toFixed(2) ?? '0.00' }}</strong></p>
              <p>Eixo: <strong class="text-slate-800">{{ infoCliente.ultimaOdEixo ?? '0' }}°</strong></p>
              <p class="text-[10px] text-slate-400 font-sans mt-1">DNP OD: {{ infoCliente.ultimaDnpOd ?? 0 }}mm</p>
            </div>
            <div class="bg-slate-50/60 p-3 rounded-lg border border-slate-100">
              <p class="font-sans font-bold text-slate-400 text-[10px] uppercase mb-1">Olho Esquerdo (OE)</p>
              <p>Esférico: <strong class="text-slate-800">{{ infoCliente.ultimaOeEsferico?.toFixed(2) ?? '0.00' }}</strong></p>
              <p>Cilíndrico: <strong class="text-slate-800">{{ infoCliente.ultimaOeCilindrico?.toFixed(2) ?? '0.00' }}</strong></p>
              <p>Eixo: <strong class="text-slate-800">{{ infoCliente.ultimaOeEixo ?? '0' }}°</strong></p>
              <p class="text-[10px] text-slate-400 font-sans mt-1">DNP OE: {{ infoCliente.ultimaDnpOe ?? 0 }}mm</p>
            </div>
          </div>
          <p v-if="infoCliente.ultimaAdicao" class="text-[11px] font-bold text-slate-600 font-mono">
            Adição da Ficha: +{{ infoCliente.ultimaAdicao.toFixed(2) }}
          </p>
        </div>
      </div>

      <div class="space-y-4">
        <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono">Linha do Tempo de Atendimentos no Sistema</h3>

        <div v-if="listaOsMapeada.length === 0" class="text-center py-12 bg-white rounded-2xl border border-slate-200 shadow-sm border-dashed text-slate-400 text-xs">
          Nenhuma ordem de serviço real emitida ou faturada no balcão digital para este paciente.
        </div>

        <div v-else class="space-y-6 relative before:absolute before:inset-0 before:left-4 md:before:left-6 before:w-0.5 before:bg-slate-200 before:block pl-10 md:pl-14">
          <div 
            v-for="os in listaOsMapeada" 
            :key="os.numeroOS" 
            class="relative bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4"
          >
            <span class="bg-teal-600 ring-teal-50 absolute -left-10 md:-left-14 top-6 w-4 h-4 rounded-full border-4 border-slate-50 ring-4 flex items-center justify-center"></span>

            <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2 border-b border-slate-100 pb-3">
              <div>
                <span class="text-xs font-mono font-bold bg-slate-950 text-teal-400 px-2.5 py-0.5 rounded shadow-sm">
                  {{ os.numeroOS }}
                </span>
                <h3 class="text-sm font-bold text-slate-800 mt-1.5">
                  Lente Catalográfica: {{ os.tipoLente || 'Configuração Padrão do Sistema' }}
                </h3>
              </div>
              
              <div class="text-sm sm:text-right">
                <p class="text-xs font-medium text-slate-400 font-mono">{{ os.dataVenda }}</p>
                <p class="font-black text-slate-950 font-mono text-sm mt-0.5">R$ {{ formatarMoeda(os.valorTotal) }}</p>
              </div>
            </div>

            <div class="space-y-2">
              <div class="flex items-center justify-between">
                <p class="text-[10px] font-black uppercase text-slate-400 tracking-wider">Refração Registrada na OS</p>
                <span class="text-[10px] bg-slate-100 text-slate-700 px-2 py-0.5 rounded font-bold uppercase border">
                  Status: {{ os.status }}
                </span>
              </div>
              
              <div class="grid grid-cols-1 md:grid-cols-2 gap-3 text-xs font-mono">
                <div class="bg-slate-50 p-3 rounded-xl border border-slate-150 space-y-1">
                  <p class="font-sans font-bold text-slate-400 text-[10px] uppercase">Visão de Longe</p>
                  <p>OD (Esf/Cil): <span class="font-bold text-slate-800">{{ os.odEsferico }} / {{ os.odCilindrico }}</span></p>
                  <p>OE (Esf/Cil): <span class="font-bold text-slate-800">{{ os.oeEsferico }} / {{ os.oeCilindrico }}</span></p>
                </div>

                <div class="bg-teal-50/20 p-3 rounded-xl border border-teal-100/40 space-y-1">
                  <div class="flex justify-between items-center">
                    <p class="font-sans font-bold text-teal-600 text-[10px] uppercase">Visão de Perto</p>
                    <span class="text-[9px] font-bold text-teal-700 bg-teal-50 px-1 rounded">
                      Adição: +{{ os.adicao }}
                    </span>
                  </div>
                  <p>OD (Esférico): <span class="font-bold text-slate-800">{{ os.esfericoPertoDireito }}</span></p>
                  <p>OE (Esférico): <span class="font-bold text-slate-800">{{ os.esfericoPertoEsquerdo }}</span></p>
                </div>
              </div>
            </div>

            <p class="text-[11px] text-slate-400 italic font-mono pt-1">
              Especialista responsável técnico: {{ os.medico ? 'Dr(a). ' + os.medico : 'Não Informado' }}
            </p>
          </div>
        </div>
      </div>

    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { computed } from 'vue'
import { Link } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  cliente: Object, Cliente: Object,
  historico: Array, Historico: Array,
  TotalGasto: Number, totalGasto: Number // Prop consolidada oficial enviada pelo controlador
})

// Normalizações defensivas contra flutuações de propriedades no payload JSON
const infoCliente = computed(() => props.Cliente ?? props.cliente ?? {})
const listaOsMapeada = computed(() => {
  const bruta = props.Historico ?? props.historico ?? []
  return bruta.map(os => ({
    numeroOS: os.NumeroOS ?? os.numeroOS,
    dataVenda: formatarData(os.DataVenda ?? os.dataVenda),
    medico: os.Medico ?? os.medico,
    valorTotal: os.ValorTotal ?? os.valorTotal ?? 0,
    status: os.Status ?? os.status,
    tipoLente: os.TipoLente ?? os.tipoLente,
    odEsferico: Number(os.odEsferico ?? 0).toFixed(2),
    odCilindrico: Number(os.odCilindrico ?? 0).toFixed(2),
    oeEsferico: Number(os.oeEsferico ?? 0).toFixed(2),
    oeCilindrico: Number(os.oeCilindrico ?? 0).toFixed(2),
    adicao: Number(os.adicao ?? 0).toFixed(2),
    esfericoPertoDireito: Number(os.odEsfericoPerto ?? (Number(os.odEsferico ?? 0) + Number(os.adicao ?? 0))).toFixed(2),
    esfericoPertoEsquerdo: Number(os.oeEsfericoPerto ?? (Number(os.oeEsferico ?? 0) + Number(os.adicao ?? 0))).toFixed(2)
  }))
})

const formatarMoeda = (valor) => {
  if (valor === undefined || valor === null) return '0,00'
  return Number(valor).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const formatarData = (dataRaw) => {
  if (!dataRaw) return '--/--/----'
  return new Date(dataRaw).toLocaleDateString('pt-BR')
}
</script>