<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-7xl mx-auto">
      
      <!-- CABEÇALHO DO DASHBOARD (ESTILO SSÓTICA / PWI) -->
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 bg-white p-6 rounded-3xl border border-slate-200 shadow-sm">
        <div>
          <div class="flex items-center gap-3">
            <span class="w-10 h-10 rounded-2xl bg-teal-500/10 text-teal-600 flex items-center justify-center text-xl font-black border border-teal-500/20">
              📊
            </span>
            <div>
              <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight flex items-center gap-2">
                Painel de Gestão &amp; Performance Óptica
              </h1>
              <p class="text-xs text-slate-500 mt-0.5">
                Visão consolidada da loja ativa: vendas diárias, faturamento, produção laboratorial e metas.
              </p>
            </div>
          </div>
        </div>
        
        <div class="flex flex-wrap items-center gap-2 font-mono text-xs">
          <select 
            v-model="filtros.mes" 
            @change="atualizarDashboard" 
            class="rounded-2xl border-slate-200 text-xs font-bold text-slate-700 focus:border-teal-500 bg-slate-50 py-2 px-3"
          >
            <option v-for="(nome, index) in meses" :key="index + 1" :value="index + 1">{{ nome }}</option>
          </select>
          <select 
            v-model="filtros.ano" 
            @change="atualizarDashboard" 
            class="rounded-2xl border-slate-200 text-xs font-bold text-slate-700 focus:border-teal-500 bg-slate-50 py-2 px-3"
          >
            <option v-for="a in [2024, 2025, 2026, 2027, 2028]" :key="a" :value="a">{{ a }}</option>
          </select>
        </div>
      </div>

      <!-- CARDS DE RESUMO DO DIA (KPIS DA ÓTICA) -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-3.5">
        
        <!-- 1. OS Emitidas Hoje -->
        <Link 
          href="/ordens" 
          class="bg-white p-4.5 rounded-3xl border border-slate-200 shadow-xs hover:border-slate-300 transition flex items-center justify-between group"
        >
          <div>
            <span class="text-[10px] font-mono font-bold uppercase text-slate-400">OS Emitidas Hoje</span>
            <p class="text-2xl font-black text-slate-950 mt-1 font-mono group-hover:text-teal-600 transition">
              {{ kpisHoje.osHoje }}
            </p>
            <span class="text-[9px] text-teal-700 font-bold block mt-0.5">Hoje no Balcão</span>
          </div>
          <div class="w-10 h-10 rounded-2xl bg-teal-50 text-teal-600 flex items-center justify-center text-sm font-bold border border-teal-100">
            ✓
          </div>
        </Link>

        <!-- 2. Faturado Hoje -->
        <div class="bg-white p-4.5 rounded-3xl border border-slate-200 shadow-xs flex items-center justify-between">
          <div>
            <span class="text-[10px] font-mono font-bold uppercase text-slate-400">Faturado Hoje</span>
            <p class="text-xl font-black text-teal-600 mt-1 font-mono">
              R$ {{ formatMoeda(kpisHoje.faturadoHoje) }}
            </p>
            <span class="text-[9px] text-slate-400 font-mono block mt-0.5">Entradas &amp; Vendas</span>
          </div>
          <div class="w-10 h-10 rounded-2xl bg-emerald-50 text-emerald-600 flex items-center justify-center text-sm font-bold border border-emerald-100">
            $
          </div>
        </div>

        <!-- 3. Lentes Não Pedidas ao Lab -->
        <Link 
          href="/laboratorio"
          :class="kpisHoje.lentesNaoPedidas > 0 ? 'bg-rose-50/80 border-rose-300 ring-2 ring-rose-100' : 'bg-white border-slate-200'"
          class="p-4.5 rounded-3xl border shadow-xs hover:shadow-sm transition flex items-center justify-between"
          title="Clique para ver lentes aguardando pedido ao laboratório"
        >
          <div>
            <div class="flex items-center gap-1.5">
              <span class="text-[10px] font-mono font-bold uppercase" :class="kpisHoje.lentesNaoPedidas > 0 ? 'text-rose-900' : 'text-slate-400'">
                Lentes Não Pedidas
              </span>
              <span v-if="kpisHoje.lentesNaoPedidas > 0" class="w-2 h-2 rounded-full bg-rose-600 animate-ping"></span>
            </div>
            <p class="text-2xl font-black mt-1 font-mono" :class="kpisHoje.lentesNaoPedidas > 0 ? 'text-rose-700' : 'text-slate-900'">
              {{ kpisHoje.lentesNaoPedidas }}
            </p>
            <span class="text-[9px] block mt-0.5 font-bold" :class="kpisHoje.lentesNaoPedidasCriticas > 0 ? 'text-rose-800 underline' : 'text-slate-400'">
              {{ kpisHoje.lentesNaoPedidasCriticas > 0 ? `⚠️ ${kpisHoje.lentesNaoPedidasCriticas} há +1 dia` : 'Pendentes ao Lab' }}
            </span>
          </div>
          <div class="w-10 h-10 rounded-2xl flex items-center justify-center text-sm font-bold" :class="kpisHoje.lentesNaoPedidas > 0 ? 'bg-rose-100 text-rose-800' : 'bg-slate-100 text-slate-600'">
            📦
          </div>
        </Link>

        <!-- 4. Serviços Atrasados -->
        <Link 
          href="/ordens?filtroAtraso=true"
          :class="(kpisHoje.servicosAtrasados ?? kpisHoje.osVencidas) > 0 ? 'bg-amber-50/80 border-amber-300 ring-2 ring-amber-100' : 'bg-white border-slate-200'"
          class="p-4.5 rounded-3xl border shadow-xs hover:shadow-sm transition flex items-center justify-between"
          title="Clique para ver serviços com prazo vencido"
        >
          <div>
            <div class="flex items-center gap-1.5">
              <span class="text-[10px] font-mono font-bold uppercase" :class="(kpisHoje.servicosAtrasados ?? kpisHoje.osVencidas) > 0 ? 'text-amber-900' : 'text-slate-400'">
                Atrasados
              </span>
              <span v-if="(kpisHoje.servicosAtrasados ?? kpisHoje.osVencidas) > 0" class="w-2 h-2 rounded-full bg-amber-600 animate-ping"></span>
            </div>
            <p class="text-2xl font-black mt-1 font-mono" :class="(kpisHoje.servicosAtrasados ?? kpisHoje.osVencidas) > 0 ? 'text-amber-800' : 'text-slate-900'">
              {{ kpisHoje.servicosAtrasados ?? kpisHoje.osVencidas }}
            </p>
            <span class="text-[9px] text-slate-400 block mt-0.5">Prazo Estourado</span>
          </div>
          <div class="w-10 h-10 rounded-2xl flex items-center justify-center text-sm font-bold" :class="(kpisHoje.servicosAtrasados ?? kpisHoje.osVencidas) > 0 ? 'bg-amber-100 text-amber-800' : 'bg-slate-100 text-slate-600'">
            ⏰
          </div>
        </Link>

        <!-- 5. Óculos Prontos / Aguardando Retirada -->
        <Link 
          href="/ordens?filtroStatus=PRONTA"
          class="bg-white p-4.5 rounded-3xl border border-slate-200 shadow-xs hover:border-slate-300 transition flex items-center justify-between"
          title="Clique para ver óculos prontos no balcão"
        >
          <div>
            <span class="text-[10px] font-mono font-bold uppercase text-slate-400">Prontos no Balcão</span>
            <p class="text-2xl font-black text-slate-900 mt-1 font-mono">
              {{ kpisHoje.osProntas }}
            </p>
            <span class="text-[9px] text-emerald-600 font-bold block mt-0.5">Aguardando Retirada</span>
          </div>
          <div class="w-10 h-10 rounded-2xl bg-blue-50 text-blue-600 flex items-center justify-center text-sm font-bold border border-blue-100">
            👓
          </div>
        </Link>

        <!-- 6. Comissões do Mês -->
        <Link 
          :href="eAdmin ? '/admin/comissoes' : '/minhas-comissoes'"
          class="bg-white p-4.5 rounded-3xl border border-slate-200 shadow-xs hover:border-slate-300 transition flex items-center justify-between"
        >
          <div>
            <span class="text-[10px] font-mono font-bold uppercase text-slate-400">
              {{ eAdmin ? 'Comissões Equipe' : 'Minha Comissão' }}
            </span>
            <p class="text-xl font-black mt-1 font-mono" :class="eAdmin ? 'text-indigo-600' : 'text-teal-600'">
              R$ {{ formatMoeda(comissaoMes) }}
            </p>
            <span class="text-[9px] text-slate-400 block mt-0.5">Acumulado do Mês</span>
          </div>
          <div class="w-10 h-10 rounded-2xl flex items-center justify-center text-sm font-bold" :class="eAdmin ? 'bg-indigo-50 text-indigo-600' : 'bg-teal-50 text-teal-600'">
            💰
          </div>
        </Link>

      </div>

      <!-- PAINEL CENTRAL: GRÁFICO E LISTA DAS ÚLTIMAS ORDENS -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        <!-- Coluna Esquerda: Gráfico & Últimas OSs (2 Colunas) -->
        <div class="lg:col-span-2 space-y-6">
          
          <!-- Curva de Faturamento dos Últimos 30 Dias -->
          <div class="bg-white p-6 rounded-3xl border border-slate-200 shadow-sm space-y-4">
            <div class="flex items-center justify-between">
              <div>
                <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono">Curva de Faturamento Diário</h3>
                <p class="text-xs text-slate-400">Evolução dos recebimentos da loja nos últimos 30 dias.</p>
              </div>
              <span class="text-xs font-mono font-bold bg-teal-50 text-teal-800 px-3 py-1 rounded-xl border border-teal-200">
                Total Mês: R$ {{ formatMoeda(TotalFaturadoMensal ?? totalFaturadoMensal ?? 0) }}
              </span>
            </div>

            <div class="relative w-full h-48 pt-4 bg-slate-50/50 rounded-2xl border border-slate-100 overflow-hidden flex items-end px-4 gap-1.5">
              <div v-if="graficoDados.length === 0" class="absolute inset-0 flex items-center justify-center text-slate-400 text-xs">
                Nenhum faturamento registrado no intervalo.
              </div>
              <div 
                v-else
                v-for="(dia, idx) in graficoDados" 
                :key="idx" 
                class="flex-1 flex flex-col items-center group relative h-full justify-end"
              >
                <!-- Barra com tooltip -->
                <div 
                  class="w-full bg-teal-500/80 hover:bg-teal-600 rounded-t-md transition-all duration-200"
                  :style="{ height: `${Math.max(8, (dia.Valor / valorMaximoGrafico) * 100)}%` }"
                ></div>
                <div class="opacity-0 group-hover:opacity-100 absolute bottom-full mb-2 bg-slate-950 text-white text-[10px] font-mono px-2 py-1 rounded-lg pointer-events-none transition z-10 whitespace-nowrap shadow-lg">
                  {{ dia.Data }}: R$ {{ formatMoeda(dia.Valor) }}
                </div>
              </div>
            </div>
          </div>

          <!-- Tabela de Últimas OSs Emitidas -->
          <div class="bg-white p-6 rounded-3xl border border-slate-200 shadow-sm space-y-4">
            <div class="flex items-center justify-between">
              <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono">
                Últimas Ordens de Serviço Emitidas
              </h3>
              <Link href="/ordens" class="text-xs font-bold text-teal-600 hover:text-teal-700 font-mono">
                Ver Todas ({{ TotalOSMensal ?? totalOSMensal ?? 0 }}) ➔
              </Link>
            </div>

            <div v-if="ultimasOrdens.length === 0" class="text-center py-8 text-slate-400 text-xs">
              Nenhuma ordem de serviço emitida no período selecionado.
            </div>

            <div v-else class="overflow-x-auto">
              <table class="w-full text-left text-xs border-collapse">
                <thead>
                  <tr class="border-b border-slate-100 text-slate-400 font-mono font-bold uppercase text-[10px]">
                    <th class="pb-2.5">Nº OS</th>
                    <th class="pb-2.5">Cliente</th>
                    <th class="pb-2.5 text-center">Status</th>
                    <th class="pb-2.5 text-center">Data</th>
                    <th class="pb-2.5 text-right">Valor Líquido</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-slate-100">
                  <tr v-for="os in ultimasOrdens" :key="os.NumeroOS" class="hover:bg-slate-50/60 transition">
                    <td class="py-3 font-mono font-bold text-slate-900">#{{ os.NumeroOS }}</td>
                    <td class="py-3 font-semibold text-slate-800 truncate max-w-[150px]">{{ os.ClienteNome }}</td>
                    <td class="py-3 text-center">
                      <span 
                        :class="obterCorStatus(os.Status)"
                        class="px-2 py-0.5 rounded-full font-bold text-[9px] font-mono border"
                      >
                        {{ os.Status }}
                      </span>
                    </td>
                    <td class="py-3 text-center text-slate-400 font-mono">{{ os.DataEntrada }}</td>
                    <td class="py-3 text-right font-black text-slate-950 font-mono">
                      R$ {{ formatMoeda(os.Valor) }}
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

        </div>

        <!-- Coluna Direita: Alertas Rápidos & Ações da Loja (1 Coluna) -->
        <div class="space-y-6">
          
          <!-- Ações Rápidas de Balcão -->
          <div class="bg-white p-6 rounded-3xl border border-slate-200 shadow-sm space-y-3">
            <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono">Ações Rápidas de Balcão</h3>
            <div class="grid grid-cols-1 gap-2 text-xs font-mono font-bold">
              <Link 
                href="/ordens/nova" 
                class="w-full p-3 rounded-2xl bg-teal-600 hover:bg-teal-700 text-white flex items-center justify-between transition shadow-sm"
              >
                <span>+ Emitir Nova O.S.</span>
                <span>➔</span>
              </Link>
              <Link 
                href="/vendas/pdv" 
                class="w-full p-3 rounded-2xl bg-amber-500 hover:bg-amber-600 text-white flex items-center justify-between transition shadow-sm"
              >
                <span>🛒 Abrir PDV Balcão</span>
                <span>➔</span>
              </Link>
              <Link 
                href="/crm" 
                class="w-full p-3 rounded-2xl bg-slate-900 hover:bg-slate-800 text-white flex items-center justify-between transition shadow-sm"
              >
                <span>📢 Central de Recall (1 Ano)</span>
                <span>➔</span>
              </Link>
            </div>
          </div>

          <!-- Alerta de Entregas Atrasadas -->
          <div class="bg-white p-6 rounded-3xl border border-slate-200 shadow-sm space-y-3">
            <h3 class="text-sm font-black text-rose-700 uppercase tracking-wider font-mono flex items-center gap-2">
              <span class="w-2 h-2 rounded-full bg-rose-600 animate-pulse"></span>
              Entregas Atrasadas
            </h3>

            <div v-if="entregasAtrasadas.length === 0" class="p-4 bg-emerald-50 text-emerald-800 rounded-2xl text-xs font-bold border border-emerald-100 flex items-center gap-2">
              <span>✓ Todas as ordens da loja estão dentro do prazo previsto!</span>
            </div>

            <div v-else class="space-y-2 max-h-56 overflow-y-auto pr-1">
              <div 
                v-for="alerta in entregasAtrasadas" 
                :key="alerta.NumeroOS" 
                class="p-3 bg-rose-50/70 rounded-2xl border border-rose-200/80 flex items-center justify-between text-xs"
              >
                <div>
                  <p class="font-mono font-bold text-slate-900">#{{ alerta.NumeroOS }}</p>
                  <p class="text-[11px] text-slate-500 truncate max-w-[130px]">{{ alerta.ClienteNome }}</p>
                </div>
                <span class="font-mono font-black text-rose-700 bg-rose-100 px-2 py-0.5 rounded-lg text-[10px]">
                  +{{ alerta.DiasAtraso }}d
                </span>
              </div>
            </div>
          </div>

          <!-- Alerta de Estoque Baixo de Armações -->
          <div v-if="eAdmin" class="bg-white p-6 rounded-3xl border border-slate-200 shadow-sm space-y-3">
            <h3 class="text-sm font-black text-amber-700 uppercase tracking-wider font-mono flex items-center gap-2">
              <span>👓</span> Estoque Baixo (Armações)
            </h3>

            <div v-if="estoqueVencendo.length === 0" class="p-4 bg-emerald-50 text-emerald-800 rounded-2xl text-xs font-bold border border-emerald-100">
              <span>✓ Estoque de armações devidamente abastecido.</span>
            </div>

            <div v-else class="space-y-2 max-h-56 overflow-y-auto pr-1">
              <div 
                v-for="prod in estoqueVencendo" 
                :key="prod.ModeloReferencia" 
                class="p-3 bg-amber-50/70 rounded-2xl border border-amber-200/80 flex items-center justify-between text-xs"
              >
                <div>
                  <p class="font-bold text-slate-900">{{ prod.ModeloReferencia }}</p>
                  <p class="text-[10px] text-slate-400">Armação</p>
                </div>
                <span class="font-mono font-bold text-amber-800 bg-amber-100 px-2 py-0.5 rounded-lg text-[10px]">
                  {{ prod.QuantidadeEstoque }} un
                </span>
              </div>
            </div>
          </div>

        </div>

      </div>

    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { ref, computed } from 'vue'
import { router, Link, usePage } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  PerfilUsuario: String, perfilUsuario: String,
  IsAdmin: Boolean, isAdmin: Boolean,
  ResumoHoje: Object, resumoHoje: Object,
  MinhaComissaoMes: Number, minhaComissaoMes: Number,
  FaturamentoGrafico: Array, faturamentoGrafico: Array,
  UltimasOS: Array, ultimasOS: Array,
  AlertasEstoque: Array, alertasEstoque: Array,
  AlertasEntregasVencidas: Array, alertasEntregasVencidas: Array,
  MesFiltro: Number, mesFiltro: Number,
  AnoFiltro: Number, anoFiltro: Number,
  TotalFaturadoMensal: Number, totalFaturadoMensal: Number,
  TotalOSMensal: Number, totalOSMensal: Number,
  RankingVendedores: Array, rankingVendedores: Array
})

const page = usePage()
const eAdmin = computed(() => {
  const p = (props.PerfilUsuario || props.perfilUsuario || page.props.auth?.usuarioPerfil || '').toLowerCase()
  return ['admin', 'sistema', 'gerente', 'dono'].includes(p)
})

const meses = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
]

const filtros = ref({
  mes: props.MesFiltro ?? props.mesFiltro ?? new Date().getMonth() + 1,
  ano: props.AnoFiltro ?? props.anoFiltro ?? new Date().getFullYear()
})

const kpisHoje = computed(() => {
  const r = props.ResumoHoje || props.resumoHoje || {}
  return {
    osHoje: r.osHoje ?? r.OsHoje ?? 0,
    faturadoHoje: r.faturadoHoje ?? r.FaturadoHoje ?? 0,
    osProntas: r.osProntas ?? r.OsProntas ?? 0,
    servicosAtrasados: r.servicosAtrasados ?? r.ServicosAtrasados ?? 0,
    osVencidas: r.osVencidas ?? r.OsVencidas ?? 0,
    lentesNaoPedidas: r.lentesNaoPedidas ?? r.LentesNaoPedidas ?? 0,
    lentesNaoPedidasCriticas: r.lentesNaoPedidasCriticas ?? r.LentesNaoPedidasCriticas ?? 0
  }
})

const comissaoMes = computed(() => {
  return props.MinhaComissaoMes ?? props.minhaComissaoMes ?? (props.ResumoHoje?.comissaoMesTotal ?? 0)
})

const graficoDados = computed(() => props.FaturamentoGrafico ?? props.faturamentoGrafico ?? [])
const valorMaximoGrafico = computed(() => {
  const valores = graficoDados.value.map(d => d.Valor || 0)
  return Math.max(...valores, 100)
})

const ultimasOrdens = computed(() => props.UltimasOS ?? props.ultimasOS ?? [])
const entregasAtrasadas = computed(() => props.AlertasEntregasVencidas ?? props.alertasEntregasVencidas ?? [])
const estoqueVencendo = computed(() => props.AlertasEstoque ?? props.alertasEstoque ?? [])

const atualizarDashboard = () => {
  router.get('/dashboard', { mes: filtros.value.mes, ano: filtros.value.ano }, { preserveState: true })
}

const formatMoeda = (val) => {
  return Number(val || 0).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const obterCorStatus = (st) => {
  switch (st) {
    case 'ENTREGUE': return 'bg-emerald-50 text-emerald-800 border-emerald-200'
    case 'PRONTA':
    case 'PRONTO': return 'bg-blue-50 text-blue-800 border-blue-200'
    case 'EM_MONTAGEM':
    case 'EM_LABORATORIO': return 'bg-indigo-50 text-indigo-800 border-indigo-200'
    case 'AGUARDANDO_LENTE': return 'bg-amber-50 text-amber-800 border-amber-200'
    default: return 'bg-slate-100 text-slate-800 border-slate-200'
  }
}
</script>
