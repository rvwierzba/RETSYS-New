<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-7xl mx-auto">
      
      <!-- Cabeçalho -->
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 bg-white p-6 rounded-3xl border border-slate-200 shadow-sm">
        <div>
          <div class="flex items-center gap-3">
            <span class="w-10 h-10 rounded-2xl bg-teal-500/10 text-teal-600 flex items-center justify-center text-xl font-black border border-teal-500/20">
              🔬
            </span>
            <div>
              <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight">Esteira Especialista de Laboratório &amp; Montagem</h1>
              <p class="text-xs text-slate-500">Gestão do ciclo óptico: pedidos a laboratórios terceirizados (Essilor, Hoya), montagem, lensometria e controle de qualidade.</p>
            </div>
          </div>
        </div>

        <div class="flex items-center gap-2">
          <Link 
            href="/ordens/kanban" 
            class="bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold py-2.5 px-4 rounded-xl text-xs transition border border-slate-200 flex items-center gap-1.5 font-mono"
          >
            <span>📊</span> Kanban Geral
          </Link>
          <span class="bg-teal-50 text-teal-800 border border-teal-200 px-3.5 py-2 rounded-xl text-xs font-mono font-bold">
            {{ ordensFiltradas.length }} Ordens no Fluxo
          </span>
        </div>
      </div>

      <!-- Filtros por Etapa da Esteira -->
      <div class="flex flex-wrap items-center gap-2 bg-slate-100/70 p-1.5 rounded-2xl border border-slate-200 text-xs font-mono">
        <button 
          v-for="aba in abasEtapa" 
          :key="aba.id"
          @click="filtroEtapa = aba.id"
          :class="filtroEtapa === aba.id ? 'bg-slate-950 text-white font-black shadow-sm' : 'text-slate-600 hover:bg-white hover:text-slate-900 font-bold'"
          class="px-4 py-2 rounded-xl transition flex items-center gap-2"
        >
          <span>{{ aba.icone }}</span>
          <span>{{ aba.nome }}</span>
          <span 
            :class="filtroEtapa === aba.id ? 'bg-teal-500 text-slate-950' : 'bg-slate-200 text-slate-700'"
            class="text-[10px] px-1.5 py-0.2 rounded-full font-bold ml-1"
          >
            {{ contarOrdensPorEtapa(aba.id) }}
          </span>
        </button>
      </div>

      <!-- Lista Vazia -->
      <div v-if="ordensFiltradas.length === 0" class="text-center py-16 bg-white rounded-3xl border border-slate-200 shadow-sm space-y-3">
        <span class="text-4xl block">✨</span>
        <h3 class="text-base font-bold text-slate-800 font-mono">Nenhuma ordem nesta etapa no momento</h3>
        <p class="text-xs text-slate-400 max-w-md mx-auto">
          Todas as ordens de serviço laboratoriais para este filtro já foram concluídas ou aguardam novas vendas no balcão.
        </p>
      </div>

      <!-- Grid de Cards de Laboratório -->
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div 
          v-for="os in ordensFiltradas" 
          :key="os.id || os.Id"
          class="bg-white rounded-3xl border border-slate-200 shadow-sm overflow-hidden flex flex-col justify-between hover:border-slate-300 transition hover:shadow-md"
        >
          <!-- Topo do Card -->
          <div class="bg-slate-950 text-white p-4 space-y-2">
            <div class="flex items-center justify-between">
              <span class="font-mono text-xs font-black bg-teal-500/20 text-teal-300 border border-teal-500/30 px-2.5 py-0.5 rounded-lg">
                OS #{{ os.numeroOS || os.NumeroOS }}
              </span>
              <span 
                :class="obterCorEtapa(os.etapaLaboratorio || os.EtapaLaboratorio)"
                class="text-[10px] uppercase font-bold tracking-wider px-2 py-0.5 rounded-full border font-mono"
              >
                {{ formatarEtapa(os.etapaLaboratorio || os.EtapaLaboratorio) }}
              </span>
            </div>

            <div class="flex items-center justify-between text-xs">
              <p class="font-bold text-slate-200 truncate max-w-[200px]" :title="os.clienteNome || os.ClienteNome">
                👤 {{ os.clienteNome || os.ClienteNome }}
              </p>
              <span class="text-[10px] text-slate-400 font-mono">
                📅 Entrega: {{ os.dataPrevistaEntrega || os.DataPrevistaEntrega }}
              </span>
            </div>
          </div>

          <!-- Informações de Armação e Lente -->
          <div class="p-4 space-y-4 flex-1 text-xs">
            
            <div class="p-3 bg-slate-50 rounded-2xl border border-slate-150 space-y-1.5 font-mono">
              <div class="flex items-center justify-between">
                <span class="text-[10px] font-bold text-slate-400 uppercase">Armação:</span>
                <span class="font-bold text-slate-800 truncate max-w-[180px]">{{ os.armacaoNome || os.ArmacaoNome || 'Armação Própria' }}</span>
              </div>
              <div class="flex items-center justify-between">
                <span class="text-[10px] font-bold text-slate-400 uppercase">Lente:</span>
                <span class="font-bold text-teal-800 truncate max-w-[180px]">{{ os.tipoLente || os.TipoLente }}</span>
              </div>
              <div v-if="os.laboratorioParceiro || os.LaboratorioParceiro" class="flex items-center justify-between pt-1 border-t border-slate-200/60 text-[11px]">
                <span class="text-[10px] font-bold text-purple-700 uppercase">Lab Externo:</span>
                <span class="font-bold text-purple-900 font-mono">{{ os.laboratorioParceiro || os.LaboratorioParceiro }} (Ped: {{ os.numeroPedidoLaboratorio || os.NumeroPedidoLaboratorio || 'S/N' }})</span>
              </div>
            </div>

            <!-- Tabela de Dioptrias e Refração -->
            <div class="space-y-1.5">
              <div class="flex items-center justify-between">
                <span class="text-[10px] font-black uppercase tracking-wider text-slate-500 font-mono">🔬 Refração Clínica</span>
                <span v-if="(os.especificacoes || os.Especificacoes)?.adicao" class="text-[10px] bg-teal-50 text-teal-800 font-bold px-2 py-0.5 rounded-md font-mono border border-teal-200">
                  AD: +{{ formatGrau((os.especificacoes || os.Especificacoes)?.adicao) }}
                </span>
              </div>

              <!-- Tabela Longe -->
              <table class="w-full text-center border-collapse font-mono text-[11px] bg-white rounded-xl border border-slate-200 overflow-hidden">
                <thead class="bg-slate-100 text-slate-600 text-[9px] uppercase font-bold">
                  <tr>
                    <th class="py-1">Olho</th>
                    <th class="py-1">Esf.</th>
                    <th class="py-1">Cil.</th>
                    <th class="py-1">Eixo</th>
                    <th class="py-1">DNP</th>
                    <th class="py-1">Alt.</th>
                  </tr>
                </thead>
                <tbody>
                  <tr class="border-b border-slate-100">
                    <td class="py-1.5 font-bold text-slate-900">OD</td>
                    <td class="py-1.5 font-bold text-slate-800">{{ formatGrau((os.especificacoes || os.Especificacoes)?.esfericoLongeDireito) }}</td>
                    <td class="py-1.5 text-amber-700 font-bold">{{ formatGrau((os.especificacoes || os.Especificacoes)?.cilindricoLongeDireito) }}</td>
                    <td class="py-1.5">{{ (os.especificacoes || os.Especificacoes)?.eixoLongeDireito }}°</td>
                    <td class="py-1.5 text-slate-600">{{ (os.especificacoes || os.Especificacoes)?.dnpOd || '--' }}</td>
                    <td class="py-1.5 font-bold text-indigo-700">{{ (os.especificacoes || os.Especificacoes)?.alturaOd || '--' }}</td>
                  </tr>
                  <tr>
                    <td class="py-1.5 font-bold text-slate-900">OE</td>
                    <td class="py-1.5 font-bold text-slate-800">{{ formatGrau((os.especificacoes || os.Especificacoes)?.esfericoLongeEsquerdo) }}</td>
                    <td class="py-1.5 text-amber-700 font-bold">{{ formatGrau((os.especificacoes || os.Especificacoes)?.cilindricoLongeEsquerdo) }}</td>
                    <td class="py-1.5">{{ (os.especificacoes || os.Especificacoes)?.eixoLongeEsquerdo }}°</td>
                    <td class="py-1.5 text-slate-600">{{ (os.especificacoes || os.Especificacoes)?.dnpOe || '--' }}</td>
                    <td class="py-1.5 font-bold text-indigo-700">{{ (os.especificacoes || os.Especificacoes)?.alturaOe || '--' }}</td>
                  </tr>
                </tbody>
              </table>

              <!-- Linha Perto se houver adição -->
              <div v-if="(os.especificacoes || os.Especificacoes)?.adicao" class="p-2 bg-teal-50/50 rounded-xl border border-teal-100 text-[10px] font-mono flex items-center justify-between text-teal-900">
                <span>🔍 Perto OD: <b>{{ formatGrau((os.especificacoes || os.Especificacoes)?.esfericoPertoDireito) }}</b></span>
                <span>OE: <b>{{ formatGrau((os.especificacoes || os.Especificacoes)?.esfericoPertoEsquerdo) }}</b></span>
              </div>
            </div>

            <!-- Selo de Lensometria e Controle de Qualidade -->
            <div class="p-2.5 rounded-2xl border flex items-center justify-between" :class="(os.conferidoLensometro || os.ConferidoLensometro) ? 'bg-emerald-50 border-emerald-200 text-emerald-900' : 'bg-slate-50 border-slate-200 text-slate-500'">
              <div class="flex items-center gap-2">
                <span class="text-sm">{{ (os.conferidoLensometro || os.ConferidoLensometro) ? '✅' : '⏳' }}</span>
                <div>
                  <span class="text-[10px] font-black uppercase block tracking-wider">Lensometria &amp; Qualidade</span>
                  <span class="text-[9px] font-mono">{{ (os.conferidoLensometro || os.ConferidoLensometro) ? 'Conferido no Lensômetro' : 'Aguardando Conferência Técnica' }}</span>
                </div>
              </div>
              <button 
                v-if="!(os.conferidoLensometro || os.ConferidoLensometro)"
                @click="marcarLensometriaOk(os.id || os.Id)"
                class="bg-emerald-600 hover:bg-emerald-700 text-white text-[10px] font-bold px-2.5 py-1 rounded-lg transition font-mono"
              >
                Validar OK
              </button>
            </div>

          </div>

          <!-- Ações Rápidas de Esteira -->
          <div class="p-4 bg-slate-50 border-t border-slate-100 space-y-2">
            
            <div class="grid grid-cols-2 gap-2">
              <button 
                @click="abrirModalLabExterno(os)"
                class="bg-purple-100 hover:bg-purple-200 text-purple-900 font-bold py-2 px-3 rounded-xl text-[11px] transition font-mono border border-purple-200 flex items-center justify-center gap-1"
                title="Informar pedido ao laboratório externo (Essilor, Hoya, etc.)"
              >
                <span>📦</span> Lab Externo
              </button>
              
              <button 
                @click="avancarEtapa(os.id || os.Id, 'EM_MONTAGEM')"
                class="bg-indigo-100 hover:bg-indigo-200 text-indigo-900 font-bold py-2 px-3 rounded-xl text-[11px] transition font-mono border border-indigo-200 flex items-center justify-center gap-1"
                title="Marcar lente como recebida e em montagem"
              >
                <span>⚙️</span> Montagem
              </button>
            </div>

            <button 
              @click="avancarEtapa(os.id || os.Id, 'PRONTO_RETIRADA', true)"
              class="w-full bg-slate-950 hover:bg-teal-600 text-white font-black py-2.5 rounded-xl text-xs uppercase tracking-wider transition shadow-sm flex items-center justify-center gap-1.5 font-mono"
            >
              <span>🚚</span> Liberar Óculos p/ Balcão
            </button>

            <div class="flex items-center justify-between pt-1 text-[11px] font-mono">
              <Link :href="`/ordens?busca=${os.numeroOS || os.NumeroOS}`" class="text-teal-700 hover:underline">
                Ver OS Completa ↗
              </Link>
              <a :href="`/ordens/${os.id || os.Id}/carne`" target="_blank" class="text-amber-800 hover:underline">
                📜 Carnê
              </a>
            </div>

          </div>
        </div>
      </div>

      <!-- MODAL DE PEDIDO AO LABORATÓRIO EXTERNO -->
      <div v-if="modalLabAberto && osSelecionadaLab" class="fixed inset-0 bg-slate-950/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
        <div class="bg-white rounded-3xl border border-slate-200 shadow-2xl max-w-md w-full p-6 space-y-5 animate-fadeIn">
          <div class="flex items-center justify-between border-b border-slate-100 pb-3">
            <div>
              <span class="text-[10px] font-mono font-bold bg-purple-100 text-purple-800 px-2.5 py-0.5 rounded-full uppercase">Surfaçagem / Tratamento Externo</span>
              <h3 class="text-base font-black text-slate-950 font-mono mt-1">OS {{ osSelecionadaLab.numeroOS || osSelecionadaLab.NumeroOS }}</h3>
            </div>
            <button @click="modalLabAberto = false" class="text-slate-400 hover:text-slate-800 font-bold text-lg">✕</button>
          </div>

          <div class="space-y-4 text-xs font-mono">
            <div>
              <label class="block font-bold uppercase text-slate-500 mb-1">Laboratório Fabricante *</label>
              <select v-model="formLab.laboratorio" class="w-full p-2.5 rounded-xl border border-slate-200 font-bold text-slate-800 focus:ring-2 focus:ring-purple-500">
                <option value="Essilor">Essilor (Varilux / Crizal)</option>
                <option value="Hoya">Hoya (MiyoSmart / BlueControl)</option>
                <option value="Zeiss">Carl Zeiss Vision</option>
                <option value="Rodenstock">Rodenstock</option>
                <option value="Laboratório Regional / Local">Laboratório Regional / Local</option>
                <option value="Laboratório Próprio">Laboratório Próprio</option>
              </select>
            </div>

            <div>
              <label class="block font-bold uppercase text-slate-500 mb-1">Nº do Pedido no Laboratório (Rastreio) *</label>
              <input 
                v-model="formLab.numeroPedido" 
                type="text" 
                placeholder="Ex: 8847291 / Pedido Essilor" 
                class="w-full p-2.5 rounded-xl border border-slate-200 font-bold text-slate-800 focus:ring-2 focus:ring-purple-500" 
              />
            </div>
          </div>

          <div class="flex justify-end gap-2 border-t border-slate-100 pt-4">
            <button @click="modalLabAberto = false" class="px-4 py-2 text-xs font-bold text-slate-600 hover:bg-slate-100 rounded-xl transition">
              Cancelar
            </button>
            <button 
              @click="salvarPedidoLabExterno" 
              class="bg-purple-700 hover:bg-purple-800 text-white font-bold px-5 py-2.5 rounded-xl text-xs uppercase tracking-wider transition shadow-sm font-mono"
            >
              Confirmar Envio ao Lab
            </button>
          </div>
        </div>
      </div>

    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { ref, computed } from 'vue'
import { router, Link } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  Ordens: Array,
  ordens: Array,
  FilaMontagem: Array,
  filaMontagem: Array
})

const listaTotalOrdens = computed(() => {
  return props.Ordens ?? props.ordens ?? props.FilaMontagem ?? props.filaMontagem ?? []
})

const filtroEtapa = ref('TODAS')

const abasEtapa = [
  { id: 'TODAS', nome: 'Todas', icone: '📋' },
  { id: 'BALCAO', nome: 'Aguardando', icone: '⏳' },
  { id: 'LABORATORIO_EXTERNO', nome: 'Lab Externo', icone: '📦' },
  { id: 'EM_MONTAGEM', nome: 'Em Montagem', icone: '⚙️' },
  { id: 'PRONTO_RETIRADA', nome: 'Pronto Balcão', icone: '✅' }
]

const contarOrdensPorEtapa = (etapaId) => {
  if (etapaId === 'TODAS') return listaTotalOrdens.value.length
  return listaTotalOrdens.value.filter(o => {
    const e = (o.etapaLaboratorio || o.EtapaLaboratorio || 'BALCAO').toUpperCase()
    if (etapaId === 'PRONTO_RETIRADA') return e === 'PRONTO_RETIRADA' || e === 'PRONTA'
    return e === etapaId
  }).length
}

const ordensFiltradas = computed(() => {
  if (filtroEtapa.value === 'TODAS') return listaTotalOrdens.value
  return listaTotalOrdens.value.filter(o => {
    const e = (o.etapaLaboratorio || o.EtapaLaboratorio || 'BALCAO').toUpperCase()
    if (filtroEtapa.value === 'PRONTO_RETIRADA') return e === 'PRONTO_RETIRADA' || e === 'PRONTA'
    return e === filtroEtapa.value
  })
})

const formatGrau = (valor) => {
  if (valor === undefined || valor === null || valor === '') return '0.00'
  const num = Number(valor)
  return isNaN(num) ? '0.00' : num.toFixed(2)
}

const formatarEtapa = (etapa) => {
  const e = (etapa || 'BALCAO').toUpperCase()
  if (e === 'BALCAO') return 'Aguardando Início'
  if (e === 'LABORATORIO_EXTERNO') return 'Lab Externo'
  if (e === 'EM_MONTAGEM') return 'Em Montagem'
  if (e === 'PRONTO_RETIRADA' || e === 'PRONTA') return 'Pronto Retirada'
  return etapa
}

const obterCorEtapa = (etapa) => {
  const e = (etapa || 'BALCAO').toUpperCase()
  if (e === 'LABORATORIO_EXTERNO') return 'bg-purple-900 text-purple-200 border-purple-700'
  if (e === 'EM_MONTAGEM') return 'bg-indigo-900 text-indigo-200 border-indigo-700'
  if (e === 'PRONTO_RETIRADA' || e === 'PRONTA') return 'bg-emerald-900 text-emerald-200 border-emerald-700'
  return 'bg-amber-900 text-amber-200 border-amber-700'
}

// Modal Lab Externo
const modalLabAberto = ref(false)
const osSelecionadaLab = ref(null)
const formLab = ref({
  laboratorio: 'Essilor',
  numeroPedido: ''
})

const abrirModalLabExterno = (os) => {
  osSelecionadaLab.value = os
  formLab.value.laboratorio = os.laboratorioParceiro || os.LaboratorioParceiro || 'Essilor'
  formLab.value.numeroPedido = os.numeroPedidoLaboratorio || os.NumeroPedidoLaboratorio || ''
  modalLabAberto.value = true
}

const salvarPedidoLabExterno = async () => {
  if (!osSelecionadaLab.value) return
  const id = osSelecionadaLab.value.id || osSelecionadaLab.value.Id
  
  try {
    const res = await fetch(`/laboratorio/${id}/avancar-etapa`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        novaEtapa: 'LABORATORIO_EXTERNO',
        laboratorioParceiro: formLab.value.laboratorio,
        numeroPedidoLab: formLab.value.numeroPedido
      })
    })
    if (res.ok) {
      modalLabAberto.value = false
      router.reload({ preserveScroll: true })
    }
  } catch (e) {
    alert('Erro ao registrar envio ao laboratório: ' + e)
  }
}

const avancarEtapa = async (id, novaEtapa, conferirLensometro = false) => {
  try {
    const res = await fetch(`/laboratorio/${id}/avancar-etapa`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        novaEtapa,
        conferidoLensometro: conferirLensometro
      })
    })
    if (res.ok) {
      router.reload({ preserveScroll: true })
    }
  } catch (e) {
    alert('Erro ao atualizar etapa: ' + e)
  }
}

const marcarLensometriaOk = async (id) => {
  try {
    const res = await fetch(`/laboratorio/${id}/avancar-etapa`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        novaEtapa: 'EM_MONTAGEM',
        conferidoLensometro: true
      })
    })
    if (res.ok) {
      router.reload({ preserveScroll: true })
    }
  } catch (e) {
    alert('Erro ao validar lensometria: ' + e)
  }
}
</script>
