<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-6xl mx-auto">
      
      <!-- CABEÇALHO GERAL DO PAINEL COM FILTRAGEM TEMPORAL -->
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <div>
          <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight flex items-center gap-2">
            <span class="w-2.5 h-2.5 rounded-full" :class="[eAdmin ? 'bg-indigo-600' : 'bg-teal-500']"></span>
            {{ eAdmin ? 'Painel de Performance Administrativo' : 'Meu Painel de Performance' }}
          </h1>
          <p class="text-xs text-slate-500 mt-1">
            {{ eAdmin ? 'Acompanhamento integrado de metas de filiais, faturamentos, estoque e desempenho.' : 'Acompanhe seus atendimentos, faturamentos diários e estimativa de comissões.' }}
          </p>
        </div>
        
        <div class="flex items-center gap-2">
          <select v-model="filtros.mes" @change="atualizarDashboard" class="rounded-xl border-slate-200 text-xs font-bold text-slate-700 focus:border-teal-500 focus:ring-teal-500 bg-slate-50">
            <option v-for="(nome, index) in meses" :key="index + 1" :value="index + 1">{{ nome }}</option>
          </select>
          <select v-model="filtros.ano" @change="atualizarDashboard" class="rounded-xl border-slate-200 text-xs font-bold text-slate-700 focus:border-teal-500 focus:ring-teal-500 bg-slate-50">
            <option v-for="ano in anos" :key="ano" :value="ano">{{ ano }}</option>
          </select>
        </div>
      </div>

      <!-- CARDS DE RESUMO DO DIA (EXIGÊNCIA 01/07 - TOPO DO PAINEL) -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        
        <!-- OS EMITIDAS HOJE -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm flex items-center justify-between">
          <div>
            <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider">OS Emitidas Hoje</span>
            <p class="text-2xl font-black text-slate-950 mt-1 font-mono">
              {{ kpisHoje.osHoje ?? kpisHoje.OsHoje ?? 0 }}
            </p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-teal-50 text-teal-600 flex items-center justify-center text-sm font-bold">✓</div>
        </div>

        <!-- VALOR FATURADO HOJE -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm flex items-center justify-between">
          <div>
            <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider">Faturado Hoje</span>
            <p class="text-2xl font-black text-teal-600 mt-1 font-mono">
              R$ {{ formatMoeda(kpisHoje.faturadoHoje ?? kpisHoje.FaturadoHoje ?? 0) }}
            </p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-teal-50 text-teal-600 flex items-center justify-center text-sm font-bold">$</div>
        </div>

        <!-- OS PRONTAS EM ABERTO -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm flex items-center justify-between">
          <div>
            <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider">Aguardando Retirada</span>
            <p class="text-2xl font-black text-indigo-600 mt-1 font-mono">
              {{ kpisHoje.osProntas ?? kpisHoje.OsProntas ?? 0 }} <span class="text-xs text-slate-400 font-normal">OS</span>
            </p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-indigo-50 text-indigo-600 flex items-center justify-center text-sm font-bold">📦</div>
        </div>

        <!-- OS COM ENTREGA ATRASADA (BADGE VERMELHO SE FOR MAIOR QUE ZERO) -->
        <div 
          :class="[(kpisHoje.osAtrasadas ?? kpisHoje.OsAtrasadas ?? 0) > 0 ? 'bg-red-50/60 border-red-200 ring-2 ring-red-100' : 'bg-white border-slate-200']"
          class="p-5 rounded-2xl border shadow-sm flex items-center justify-between transition duration-200"
        >
          <div>
            <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider">Entregas Vencidas</span>
            <p class="text-2xl font-black mt-1 font-mono" :class="[(kpisHoje.osAtrasadas ?? kpisHoje.OsAtrasadas ?? 0) > 0 ? 'text-red-700 animate-pulse' : 'text-slate-900']">
              {{ kpisHoje.osAtrasadas ?? kpisHoje.OsAtrasadas ?? 0 }}
            </p>
          </div>
          <div class="w-10 h-10 rounded-xl flex items-center justify-center text-sm font-bold" :class="[(kpisHoje.osAtrasadas ?? kpisHoje.OsAtrasadas ?? 0) > 0 ? 'bg-red-100 text-red-700' : 'bg-slate-100 text-slate-700']">🛑</div>
        </div>
      </div>

      <!-- SEÇÃO CENTRAL: GRÁFICO DE 30 DIAS, ÚLTIMAS OS & ALERTAS DE ESTOQUE -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        <!-- COLUNA 1 & 2: FATURAMENTO E ATENDIMENTOS -->
        <div class="lg:col-span-2 space-y-6">
          
          <!-- GRÁFICO DE FATURAMENTO DOS ÚLTIMOS 30 DIAS (SVG INTERATIVO AUTOSUSTENTÁVEL) -->
          <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
            <div>
              <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono">Curva de Faturamento (Últimos 30 Dias)</h3>
              <p class="text-xs text-slate-400">Evolução diária dos recebimentos em reais de faturamentos de balcão.</p>
            </div>

            <!-- Gráfico de Linha em SVG Dinâmico -->
            <div class="relative w-full h-52 pt-4 bg-slate-50/50 rounded-xl border border-slate-100 overflow-hidden">
              <div v-if="graficoDados.length === 0" class="absolute inset-0 flex items-center justify-center text-slate-400 text-xs">
                Nenhum faturamento registrado no intervalo dos últimos 30 dias.
              </div>
              <svg v-else class="w-full h-full" viewBox="0 0 500 200" preserveAspectRatio="none">
                <defs>
                  <linearGradient id="chart-grad" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stop-color="#0d9488" stop-opacity="0.25" />
                    <stop offset="100%" stop-color="#0d9488" stop-opacity="0.0" />
                  </linearGradient>
                </defs>

                <!-- Malha de Grade Traseira -->
                <line x1="20" y1="30" x2="480" y2="30" stroke="#f1f5f9" stroke-width="1" />
                <line x1="20" y1="100" x2="480" y2="100" stroke="#f1f5f9" stroke-width="1" />
                <line x1="20" y1="170" x2="480" y2="170" stroke="#e2e8f0" stroke-width="1.5" />

                <!-- Preenchimento Sob a Linha -->
                <path :d="chartAreaPath" fill="url(#chart-grad)" />

                <!-- Linha Principal do Gráfico -->
                <path :d="chartPath" fill="none" stroke="#0d9488" stroke-width="3" stroke-linecap="round" stroke-linejoin="round" />

                <!-- Pontos Interativos com Tooltips implícitos -->
                <circle 
                  v-for="(p, idx) in chartPoints" 
                  :key="idx" 
                  :cx="p.x" 
                  :cy="p.y" 
                  r="4" 
                  fill="#ffffff" 
                  stroke="#0d9488" 
                  stroke-width="2" 
                />
              </svg>
            </div>
          </div>

          <!-- TABELA: ÚLTIMAS 5 OS EMITIDAS EM TEMPO REAL -->
          <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
            <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono">Últimos Atendimentos Registrados</h3>
            
            <div v-if="ultimasOrdens.length === 0" class="text-center py-8 text-slate-400 text-xs border border-dashed border-slate-100 rounded-xl">
              Nenhuma OS em circulação cadastrada no sistema.
            </div>

            <div v-else class="overflow-x-auto">
              <table class="w-full text-left text-xs border-collapse">
                <thead>
                  <tr class="border-b border-slate-100 text-slate-400 font-bold uppercase tracking-wider">
                    <th class="pb-3">Número OS</th>
                    <th class="pb-3">Cliente</th>
                    <th class="pb-3 text-center">Status de Esteira</th>
                    <th class="pb-3 text-center">Data Entrada</th>
                    <th class="pb-3 text-right">Valor Líquido</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="os in ultimasOrdens" :key="os.NumeroOS" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
                    <td class="py-3 font-mono font-bold text-slate-800">{{ os.NumeroOS }}</td>
                    <td class="py-3 font-semibold text-slate-700 truncate max-w-[150px]">{{ os.ClienteNome }}</td>
                    <td class="py-3 text-center">
                      <span 
                        :class="[
                          os.Status === 'ENTREGUE' ? 'bg-emerald-50 text-emerald-700 border-emerald-100' :
                          os.Status === 'CANCELADA' ? 'bg-red-50 text-red-700 border-red-100' :
                          os.Status === 'PRONTO' ? 'bg-indigo-50 text-indigo-700 border-indigo-100' :
                          'bg-amber-50 text-amber-700 border-amber-100'
                        ]"
                        class="px-2 py-0.5 rounded-full font-bold text-[10px] border"
                      >
                        {{ os.Status }}
                      </span>
                    </td>
                    <td class="py-3 text-center text-slate-400 font-mono">{{ os.DataEntrada }}</td>
                    <td class="py-3 text-right font-black text-slate-950 font-mono">R$ {{ formatMoeda(os.Valor) }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <!-- COLUNA 3: ALERTAS (REQUISITO 01/07 - ADMIN VÊ TUDO, VENDEDOR SEM ALERTA DE ESTOQUE) -->
        <div class="space-y-6">
          
          <!-- SEÇÃO DE ENTREGAS VENCIDAS (ALERTA DE SEGURANÇA COMUM) -->
          <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
            <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono flex items-center gap-1.5 text-red-700">
              <span class="w-2.5 h-2.5 rounded-full bg-red-600 animate-pulse"></span>
              Entregas Atrasadas
            </h3>
            
            <div v-if="entregasAtrasadas.length === 0" class="p-4 bg-emerald-50 text-emerald-800 rounded-xl text-xs font-bold flex items-center gap-2 border border-emerald-100">
              <span>✓ Todas as ordens de serviço estão rigorosamente em dia!</span>
            </div>

            <div v-else class="space-y-3">
              <div v-for="alerta in entregasAtrasadas" :key="alerta.NumeroOS" class="p-3.5 bg-red-50/60 rounded-xl border border-red-100/60 flex items-center justify-between text-xs transition hover:bg-red-50">
                <div>
                  <p class="font-bold text-slate-900 font-mono">{{ alerta.NumeroOS }}</p>
                  <p class="text-[10px] text-slate-500 truncate max-w-[120px]">{{ alerta.ClienteNome }}</p>
                </div>
                <span class="font-black text-red-700 bg-red-100/60 px-2 py-1 rounded text-[10px] font-mono">
                  +{{ alerta.DiasAtraso }} dias
                </span>
              </div>
            </div>
          </div>

          <!-- SEÇÃO DE PRODUTOS ABAIXO DO MÍNIMO (EXCLUSIVO PARA ADMIN - EXIGÊNCIA 01/07) -->
          <div v-if="eAdmin" class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
            <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono flex items-center gap-1.5 text-amber-700">
              <span class="w-2.5 h-2.5 rounded-full bg-amber-500"></span>
              Alerta de Reposição (Estoque)
            </h3>

            <div v-if="estoqueVencendo.length === 0" class="p-4 bg-emerald-50 text-emerald-800 rounded-xl text-xs font-bold flex items-center gap-2 border border-emerald-100">
              <span>✓ Todo o estoque de armações está acima do mínimo estipulado.</span>
            </div>

            <div v-else class="space-y-3">
              <div v-for="produto in estoqueVencendo" :key="produto.ModeloReferencia" class="p-3 bg-amber-50/60 rounded-xl border border-amber-100/60 flex items-center justify-between text-xs">
                <div>
                  <p class="font-black text-slate-900">{{ produto.ModeloReferencia }}</p>
                  <p class="text-[10px] text-slate-400">Modelo Armação</p>
                </div>
                <span class="font-bold text-amber-800 bg-amber-100 px-2.5 py-0.5 rounded-full font-mono text-[10px]">
                  {{ produto.QuantidadeEstoque }} un
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- SEÇÃO HISTÓRICA MENSAL: RANKING E METAS GLOBAIS (ADMINISTRATIVO / SUPORTE) -->
      <div v-if="eAdmin" class="grid grid-cols-1 md:grid-cols-3 gap-6 pt-2 border-t border-slate-200/60">
        
        <!-- CARD FATURAMENTO DO MÊS -->
        <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm flex items-center justify-between">
          <div>
            <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider">Faturamento do Mês</span>
            <p class="text-2xl font-black text-teal-600 mt-1 font-mono">
              R$ {{ formatMoeda(TotalFaturadoMensal ?? totalFaturadoMensal ?? 0) }}
            </p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-teal-50 text-teal-600 flex items-center justify-center text-sm font-bold">$</div>
        </div>

        <!-- CARD TOTAL DE OS DO MÊS -->
        <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm flex items-center justify-between">
          <div>
            <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider">OS Emitidas no Mês</span>
            <p class="text-2xl font-black text-slate-950 mt-1 font-mono">
              {{ TotalOSMensal ?? totalOSMensal ?? 0 }} <span class="text-xs text-slate-400 font-normal">OS</span>
            </p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-slate-50 text-slate-700 flex items-center justify-center text-sm font-bold">OS</div>
        </div>

        <!-- STATUS CORPORATIVO -->
        <div class="bg-slate-950 p-6 rounded-2xl text-white flex flex-col justify-between">
          <div>
            <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider">Status Corporativo</span>
            <p class="text-sm font-bold text-teal-400 mt-0.5">Visão Integrada Ativa</p>
          </div>
          <div class="flex items-center justify-between text-[10px] text-slate-400 pt-2 border-t border-slate-800 mt-2">
            <span>Sincronização Ativa</span>
            <span class="w-2 h-2 rounded-full bg-emerald-400 animate-pulse"></span>
          </div>
        </div>
      </div>

      <!-- RANKING DE COLABORADORES & DIVISÃO POR LOJAS (EXCLUSIVO PARA ADMIN/GERENTE) -->
      <div v-if="eAdmin" class="grid grid-cols-1 lg:grid-cols-3 gap-6 pt-4">
        
        <!-- RANKING DE VENDEDORES -->
        <div class="lg:col-span-2 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
          <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono">Ranking de Desempenho dos Vendedores</h3>
          
          <div v-if="ranking.length === 0" class="text-center py-8 text-slate-400 text-xs">
            Nenhuma venda faturada cadastrada por colaboradores neste período.
          </div>

          <div v-else class="overflow-x-auto">
            <table class="w-full text-left text-xs">
              <thead>
                <tr class="border-b border-slate-100 text-slate-400 font-bold uppercase tracking-wider">
                  <th class="pb-3">Vendedor</th>
                  <th class="pb-3 text-center">Atendimentos (OS)</th>
                  <th class="pb-3 text-right">Total Gerado</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="vendedor in ranking" :key="vendedor.VendedorNome" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
                  <td class="py-3 font-semibold text-slate-800">{{ vendedor.VendedorNome }}</td>
                  <td class="py-3 text-center text-slate-600 font-bold">{{ vendedor.QuantidadeOS }} un</td>
                  <td class="py-3 text-right font-black text-slate-950 font-mono">R$ {{ formatMoeda(vendedor.TotalVendas) }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- DIVISÃO POR FILIAIS -->
        <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
          <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono">Faturamento consolidado por Loja</h3>
          
          <div v-if="faturamentoLojas.length === 0" class="text-center py-8 text-slate-400 text-xs">
            Sem registros de faturamentos de filiais no período.
          </div>

          <div v-else class="space-y-3">
            <div v-for="loja in faturamentoLojas" :key="loja.Loja" class="p-3.5 bg-slate-50 rounded-xl border border-slate-100 flex items-center justify-between text-xs">
              <div>
                <p class="font-bold text-slate-800">{{ loja.Loja }}</p>
                <p class="text-[10px] text-slate-400">Unidade Operacional</p>
              </div>
              <p class="font-black text-slate-950 font-mono">R$ {{ formatMoeda(loja.Total) }}</p>
            </div>
          </div>
        </div>
      </div>

    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { reactive, computed } from 'vue'
import { router, usePage } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const page = usePage()

const props = defineProps({
  // Controle de Permissões e Perfil
  PerfilUsuario: String, perfilUsuario: String,
  IsAdmin: Boolean, isAdmin: Boolean,
  
  // KPIs Diários do Topo (01/07)
  ResumoHoje: Object, resumoHoje: Object,
  
  // Dados Gráficos e Histórico Central
  FaturamentoGrafico: Array, faturamentoGrafico: Array,
  UltimasOS: Array, ultimasOS: Array,

  // Alertas (01/07)
  AlertasEstoque: Array, alertasEstoque: Array,
  AlertasEntregasVencidas: Array, alertasEntregasVencidas: Array,

  // Metas Administrativas de apoio
  MesFiltro: Number, mesFiltro: Number,
  AnoFiltro: Number, anoFiltro: Number,
  TotalFaturadoMensal: Number, totalFaturadoMensal: Number,
  TotalOSMensal: Number, totalOSMensal: Number,
  RankingVendedores: Array, rankingVendedores: Array,
  FaturamentoPorLoja: Array, faturamentoPorLoja: Array
})

// Definição de filtros reativos para Inertia
const filtros = reactive({
  mes: props.MesFiltro ?? props.mesFiltro ?? new Date().getMonth() + 1,
  ano: props.AnoFiltro ?? props.anoFiltro ?? new Date().getFullYear()
})

// Mapeamentos Defensivos dos payloads (Evita incompatibilidades de caixa alta/baixa do serializer JSON)
const eAdmin = computed(() => props.IsAdmin ?? props.isAdmin ?? (props.PerfilUsuario ?? props.perfilUsuario ?? '').toLowerCase() === 'admin')
const kpisHoje = computed(() => props.ResumoHoje ?? props.resumoHoje ?? { osHoje: 0, faturadoHoje: 0, osProntas: 0, osAtrasadas: 0 })
const graficoDados = computed(() => props.FaturamentoGrafico ?? props.faturamentoGrafico ?? [])
const ultimasOrdens = computed(() => props.UltimasOS ?? props.ultimasOS ?? [])
const estoqueVencendo = computed(() => props.AlertasEstoque ?? props.alertasEstoque ?? [])
const entregasAtrasadas = computed(() => props.AlertasEntregasVencidas ?? props.alertasEntregasVencidas ?? [])
const ranking = computed(() => props.RankingVendedores ?? props.rankingVendedores ?? [])
const faturamentoLojas = computed(() => props.FaturamentoPorLoja ?? props.faturamentoPorLoja ?? [])

const meses = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
]

const anos = [2025, 2026, 2027, 2028]

const atualizarDashboard = () => {
  router.get('/dashboard', { mes: filtros.mes, ano: filtros.ano }, { preserveState: true })
}

const formatMoeda = (valor) => {
  if (valor === undefined || valor === null) return '0,00'
  return Number(valor).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

// =========================================================================
// MOTOR DE CÁLCULO DO GRÁFICO SVG INTEGRADO PARA MÁXIMA PORTABILIDADE (01/07)
// =========================================================================
const chartPoints = computed(() => {
  const dados = graficoDados.value
  if (dados.length === 0) return []
  
  // Encontra o valor máximo para dimensionar a proporção de altura do gráfico
  const maxVal = Math.max(...dados.map(d => d.Valor ?? d.valor ?? 0), 100)
  
  return dados.map((d, index) => {
    const x = dados.length > 1 ? (index / (dados.length - 1)) * 460 + 20 : 250
    const val = d.Valor ?? d.valor ?? 0
    const y = 170 - (val / maxVal) * 140
    return { x, y, val }
  })
})

const chartPath = computed(() => {
  const pts = chartPoints.value
  if (pts.length === 0) return ''
  return pts.map((p, i) => `${i === 0 ? 'M' : 'L'} ${p.x} ${p.y}`).join(' ')
})

const chartAreaPath = computed(() => {
  const pts = chartPoints.value
  if (pts.length === 0) return ''
  const first = pts[0]
  const last = pts[pts.length - 1]
  return `${chartPath.value} L ${last.x} 170 L ${first.x} 170 Z`
})
</script>