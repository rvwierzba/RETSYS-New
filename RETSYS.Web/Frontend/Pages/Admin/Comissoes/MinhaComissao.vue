<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-6xl mx-auto">
      
      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight flex items-center gap-2">
            <span class="w-2.5 h-2.5 rounded-full bg-teal-500"></span>
            Extrato de Comissões e Performance
          </h1>
          <p class="text-xs text-slate-500 mt-1">
            Consulte seus lançamentos de vendas em tempo real, acompanhe comissões acumuladas e confira o histórico de fechamentos.
          </p>
        </div>
        
        <div class="flex items-center gap-2">
          <label class="text-xs font-bold text-slate-400 uppercase font-mono">Competência:</label>
          <input 
            type="month" 
            v-model="periodoSelecionado" 
            @change="mudarPeriodoCompetencia" 
            class="rounded-xl border-slate-200 text-xs font-bold text-slate-700 focus:border-teal-500 focus:ring-teal-500 bg-slate-50 font-mono"
          />
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        
        <div class="bg-slate-950 p-6 rounded-2xl text-white flex flex-col justify-between shadow-md ring-4 ring-slate-900/10">
          <div>
            <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider font-mono">Comissão Estimada (Mês Atual)</span>
            <p class="text-3xl font-black text-teal-400 mt-1 font-mono">
              R$ {{ formatarMoeda(props.ComissaoAcumulada) }}
            </p>
          </div>
          <p class="text-[10px] text-slate-400 mt-4 leading-tight border-t border-slate-800 pt-2 flex items-center gap-1.5">
            <span class="w-1.5 h-1.5 rounded-full bg-emerald-400 animate-pulse"></span>
            Atualizado em tempo real com base nos faturamentos válidos do balcão.
          </p>
        </div>

        <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm flex flex-col justify-between">
          <div>
            <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider font-mono">Competência Ativa</span>
            <p class="text-lg font-black text-slate-800 font-mono mt-1 uppercase">
              {{ formatarPeriodoExtenso(props.PeriodoFiltro) }}
            </p>
          </div>
          <div class="text-[10px] text-slate-500 font-medium bg-slate-50 p-2 rounded-xl border border-slate-100">
            ⚠ Vendas estornadas ou canceladas são excluídas automaticamente deste cálculo.
          </div>
        </div>

        <div class="bg-slate-50 border border-slate-200/60 p-6 rounded-2xl flex flex-col justify-between">
          <div>
            <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider font-mono">Acesso Operacional</span>
            <p class="text-xs font-semibold text-slate-600 mt-1">Precisa lançar um novo pedido e vincular um cliente?</p>
          </div>
          <button 
            @click="irParaNovaVenda" 
            class="w-full bg-slate-950 hover:bg-slate-800 text-white font-bold py-2.5 px-4 rounded-xl text-xs uppercase tracking-wider transition active:scale-95 shadow-sm"
          >
            Acessar Frente de Balcão →
          </button>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        <div class="lg:col-span-2 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
          <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono">Extrato de Vendas do Período</h3>
          
          <div v-if="!props.Extrato || props.Extrato.length === 0" class="text-center py-12 border-2 border-dashed border-slate-100 rounded-xl text-slate-400 text-sm">
            Nenhuma comissão ou venda localizada para a competência selecionada.
          </div>

          <div class="overflow-x-auto" v-else>
            <table class="w-full text-left text-xs border-collapse">
              <thead>
                <tr class="border-b border-slate-100 text-slate-400 font-bold uppercase tracking-wider">
                  <th class="pb-3">Data</th>
                  <th class="pb-3">Ordem de Serviço</th>
                  <th class="pb-3 text-right">Valor Bruto OS</th>
                  <th class="pb-3 text-right">Minha Comissão</th>
                  <th class="pb-3 text-center">Status</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in props.Extrato" :key="item.Id" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
                  <td class="py-3 font-mono text-slate-500">{{ item.DataLançamento }}</td>
                  <td class="py-3 font-mono font-bold text-slate-900">{{ item.NumeroOS }}</td>
                  <td class="py-3 text-right font-mono text-slate-600">R$ {{ formatarMoeda(item.ValorBrutoVenda) }}</td>
                  <td class="py-3 text-right font-black text-teal-600 font-mono">R$ {{ formatarMoeda(item.ComissaoGerada) }}</td>
                  <td class="py-3 text-center">
                    <span 
                      :class="[
                        item.Status === 'PAGO' ? 'bg-emerald-50 text-emerald-700 border-emerald-100' :
                        item.Status === 'PENDENTE' ? 'bg-amber-50 text-amber-700 border-amber-100' :
                        'bg-red-50 text-red-700 border-red-100'
                      ]"
                      class="px-2 py-0.5 rounded-full font-bold text-[10px] border uppercase"
                    >
                      {{ item.Status }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
          <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono">Fechamentos Mensais</h3>
          
          <div v-if="!props.Historico || props.Historico.length === 0" class="text-center py-8 text-slate-400 text-xs border border-dashed border-slate-100 rounded-xl">
            Nenhum encerramento de competência registrado para seu perfil.
          </div>

          <div class="space-y-3" v-else>
            <div v-for="f in props.Historico" :key="f.Id" class="p-3.5 bg-slate-50 rounded-xl border border-slate-100 flex flex-col justify-between gap-2 transition hover:bg-slate-100/60">
              <div class="flex items-center justify-between">
                <span class="font-mono font-bold text-slate-900 text-xs">{{ f.PeriodoReferencia }}</span>
                <span 
                  :class="[
                    f.Status === 'PAGO' ? 'bg-emerald-100 text-emerald-800' : 
                    f.Status === 'FECHADO' ? 'bg-amber-100 text-amber-800' : 
                    'bg-slate-200 text-slate-700'
                  ]"
                  class="px-2 py-0.5 rounded font-black text-[9px] uppercase font-mono"
                >
                  {{ f.Status }}
                </span>
              </div>
              
              <div class="flex justify-between items-center text-[11px] pt-1 text-slate-500">
                <span>Vendas: <strong class="text-slate-700 font-mono">R$ {{ formatarMoeda(f.TotalVendasBrutas) }}</strong></span>
                <span>Comissão: <strong class="text-teal-600 font-mono">R$ {{ formatarMoeda(f.TotalComissao) }}</strong></span>
              </div>

              <div v-if="f.DataLiquidacao" class="text-[9px] text-slate-400 italic font-mono pt-1 border-t border-slate-200/60 flex items-center justify-between">
                <span>Pago via caixa/banco</span>
                <span>{{ f.DataLiquidacao }}</span>
              </div>
            </div>
          </div>
        </div>

      </div>

    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { ref } from 'vue'
import { router } from '@inertiajs/vue3'
// Correção do nível de pasta efetuada aqui:
import AuthenticatedLayout from '../../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  Extrato: Array,
  Historico: Array,
  ComissaoAcumulada: Number,
  PeriodoFiltro: String
})

const periodoSelecionado = ref(props.PeriodoFiltro)

const irParaNovaVenda = () => {
  router.visit('/ordens/nova')
}

const mudarPeriodoCompetencia = () => {
  router.get('/minhas-comissoes', { periodo: periodoSelecionado.value }, { preserveState: true })
}

const formatarMoeda = (valor) => {
  if (valor === undefined || valor === null) return '0,00'
  return Number(valor).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const formatarPeriodoExtenso = (periodoRaw) => {
  if (!periodoRaw || periodoRaw.length !== 7) return 'Período Inválido'
  const partes = periodoRaw.split('-')
  const meses = [
    'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
    'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
  ]
  const indexMes = parseInt(partes[1], 10) - 1
  return `${meses[indexMes]} de ${partes[0]}`
}
</script>