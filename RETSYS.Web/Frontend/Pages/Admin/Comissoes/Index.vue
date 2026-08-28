<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-6xl mx-auto">
      <!-- Cabeçalho do Painel -->
      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight">
            Gestão de Comissões da Equipe
          </h1>

          <p class="text-xs text-slate-500 mt-1">
            Defina taxas individuais, feche períodos mensais e liquide pagamentos de comissão.
          </p>
        </div>

        <div
          v-if="$page.props.flash?.erro"
          class="p-3 bg-rose-50 border border-rose-200 text-rose-800 rounded-xl text-xs font-bold"
        >
          ⚠️ {{ $page.props.flash.erro }}
        </div>
      </div>

      <!-- CARD 1: Fechamento Mensal -->
      <div class="bg-slate-950 p-6 rounded-2xl shadow-sm border border-slate-800 space-y-5">
        <div class="flex flex-col md:flex-row md:items-start md:justify-between gap-3 border-b border-slate-800 pb-4">
          <div>
            <h3 class="text-sm font-black text-white uppercase tracking-wider font-mono flex items-center gap-2">
              <span class="w-2 h-2 rounded-full bg-teal-400"></span>
              Fechamento Mensal de Comissões
            </h3>

            <p class="text-xs text-slate-400 mt-1">
              Consolida as comissões pendentes da vendedora no período selecionado.
            </p>
          </div>

          <span class="text-[10px] font-bold uppercase tracking-wider bg-amber-400/10 text-amber-300 border border-amber-400/20 px-3 py-1.5 rounded-full">
            Ação financeira auditável
          </span>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-3 gap-4 items-end">
          <div>
            <label class="block text-[10px] font-bold uppercase tracking-wider text-slate-400 mb-1.5">
              Vendedora *
            </label>

            <select
              v-model="vendedorFechamentoId"
              class="w-full rounded-xl border-slate-700 bg-slate-900 text-white text-sm focus:border-teal-500 focus:ring-teal-500"
            >
              <option value="">Selecione a vendedora</option>

              <option
                v-for="vendedor in vendedoresMapeados"
                :key="vendedor.id"
                :value="vendedor.id"
              >
                {{ vendedor.nome }} — {{ vendedor.taxaEdicao }}%
              </option>
            </select>
          </div>

          <div>
            <label class="block text-[10px] font-bold uppercase tracking-wider text-slate-400 mb-1.5">
              Período de Referência *
            </label>

            <input
              v-model="periodoFechamento"
              type="month"
              class="w-full rounded-xl border-slate-700 bg-slate-900 text-white text-sm font-mono focus:border-teal-500 focus:ring-teal-500"
            />
          </div>

          <button
            type="button"
            :disabled="fechandoMes || !vendedorFechamentoId || !periodoFechamento"
            @click="fecharMes"
            class="bg-teal-600 hover:bg-teal-700 disabled:bg-slate-700 disabled:text-slate-400 text-white font-black text-xs uppercase tracking-wider px-5 py-3 rounded-xl transition shadow-sm active:scale-95"
          >
            <span v-if="fechandoMes">Consolidando...</span>
            <span v-else>Fechar Comissão do Mês</span>
          </button>
        </div>

        <div class="p-3 bg-slate-900/70 border border-slate-800 rounded-xl text-[11px] leading-relaxed text-slate-400">
          <strong class="text-amber-300">Atenção:</strong>
          o fechamento considera apenas comissões com status
          <span class="font-mono text-white">PENDENTE</span>.
          Após fechar, o período ficará disponível para a confirmação de pagamento.
        </div>
      </div>

      <!-- CARD 2: Configuração de Taxas Individuais -->
      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono flex items-center gap-2">
            <span>⚙️ Taxas de Comissão por Vendedora</span>
          </h3>

          <span class="text-[11px] text-slate-400">
            Defina a porcentagem individual de cada funcionário.
          </span>
        </div>

        <div
          v-if="vendedoresMapeados.length === 0"
          class="text-center py-6 text-slate-400 text-xs border border-dashed border-slate-100 rounded-xl"
        >
          Nenhum vendedor ativo localizado no sistema.
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div
            v-for="vendedor in vendedoresMapeados"
            :key="vendedor.id"
            class="p-4 rounded-xl border border-slate-200 bg-slate-50/50 space-y-3"
          >
            <div class="flex justify-between items-start">
              <div>
                <p class="font-bold text-slate-900 text-sm">
                  {{ vendedor.nome }}
                </p>

                <p class="text-[11px] text-slate-400 font-mono">
                  {{ vendedor.email }}
                </p>

                <span class="inline-block mt-1 px-2 py-0.5 rounded bg-slate-200 text-slate-700 text-[10px] font-bold">
                  📍 {{ vendedor.filialLoja || 'Matriz' }}
                </span>
              </div>
            </div>

            <div class="flex items-center gap-2 pt-2 border-t border-slate-200/80">
              <div class="flex-1">
                <label class="block text-[10px] font-bold uppercase text-slate-400 mb-1">
                  Comissão (%)
                </label>

                <input
                  v-model.number="vendedor.taxaEdicao"
                  type="number"
                  step="0.1"
                  min="0"
                  max="100"
                  class="w-full rounded-xl border-slate-200 text-xs font-mono font-bold text-teal-700 focus:border-teal-500 py-1.5 px-2.5 bg-white"
                />
              </div>

              <button
                type="button"
                @click="salvarTaxaIndividual(vendedor.id, vendedor.taxaEdicao)"
                class="self-end bg-teal-600 hover:bg-teal-700 text-white font-bold text-xs px-3 py-2 rounded-xl transition shadow-sm active:scale-95"
                title="Salvar porcentagem de comissão"
              >
                Salvar
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- CARD 3: Fechamentos Consolidados -->
      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2">
          <div>
            <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono">
              Extratos de Fechamentos Consolidados
            </h3>

            <p class="text-xs text-slate-400 mt-1">
              Histórico financeiro dos períodos já processados.
            </p>
          </div>

          <span class="text-[10px] font-bold uppercase tracking-wider text-slate-500 bg-slate-100 px-3 py-1.5 rounded-full">
            {{ fechamentosMapeados.length }} registro(s)
          </span>
        </div>

        <div
          v-if="fechamentosMapeados.length === 0"
          class="text-center py-8 text-slate-400 text-xs border border-dashed border-slate-100 rounded-xl"
        >
          Nenhum fechamento de período localizado no histórico do sistema.
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-left text-xs border-collapse min-w-[920px]">
            <thead>
              <tr class="border-b border-slate-100 text-slate-400 font-bold uppercase tracking-wider">
                <th class="pb-3">Vendedor</th>
                <th class="pb-3 text-center">Período</th>
                <th class="pb-3 text-center">Ordens</th>
                <th class="pb-3 text-right">Vendas Líquidas</th>
                <th class="pb-3 text-right">Comissão Devida</th>
                <th class="pb-3 text-center">Status</th>
                <th class="pb-3 text-center">Fechamento</th>
                <th class="pb-3 text-right">Ações</th>
              </tr>
            </thead>

            <tbody>
              <tr
                v-for="fechamento in fechamentosMapeados"
                :key="fechamento.id"
                class="border-b border-slate-50 hover:bg-slate-50/50 transition"
              >
                <td class="py-3 font-semibold text-slate-800">
                  {{ fechamento.vendedorNome }}
                </td>

                <td class="py-3 text-center font-mono text-slate-600">
                  {{ fechamento.periodoReferencia }}
                </td>

                <td class="py-3 text-center text-slate-500 font-semibold">
                  {{ fechamento.qtdOs }} un.
                </td>

                <td class="py-3 text-right font-mono text-slate-700">
                  R$ {{ formatarMoeda(fechamento.totalVendasBrutas) }}
                </td>

                <td class="py-3 text-right font-black text-teal-600 font-mono">
                  R$ {{ formatarMoeda(fechamento.totalComissao) }}
                </td>

                <td class="py-3 text-center">
                  <span
                    :class="[
                      fechamento.status === 'PAGO'
                        ? 'bg-emerald-50 text-emerald-700 border-emerald-100'
                        : fechamento.status === 'FECHADO'
                          ? 'bg-amber-50 text-amber-700 border-amber-100'
                          : 'bg-slate-50 text-slate-600 border-slate-100'
                    ]"
                    class="px-2 py-0.5 rounded-full font-bold text-[10px] border"
                  >
                    {{ fechamento.status }}
                  </span>
                </td>

                <td class="py-3 text-center text-slate-400 font-mono">
                  {{ fechamento.dataFechamento || '—' }}
                </td>

                <td class="py-3 text-right">
                  <button
                    v-if="fechamento.status === 'FECHADO'"
                    type="button"
                    @click="efetuarBaixaPagamento(fechamento.id)"
                    class="text-xs font-bold bg-teal-600 hover:bg-teal-700 text-white px-4 py-2 rounded-xl transition shadow-sm active:scale-95 whitespace-nowrap"
                  >
                    Confirmar Pagamento
                  </button>

                  <span
                    v-else-if="fechamento.status === 'PAGO'"
                    class="text-xs font-semibold text-emerald-700 font-mono bg-emerald-50 px-2.5 py-1 rounded-lg border border-emerald-100"
                  >
                    ✓ Pago em {{ fechamento.dataPagamento || '—' }}
                  </span>

                  <span v-else class="text-xs font-medium text-slate-400 font-mono">
                    Aguardando fechamento
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { router } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  Fechamentos: Array,
  fechamentos: Array,
  Vendedores: Array,
  vendedores: Array
})

const criarPeriodoAtual = () => {
  const dataAtual = new Date()
  const ano = dataAtual.getFullYear()
  const mes = String(dataAtual.getMonth() + 1).padStart(2, '0')

  return `${ano}-${mes}`
}

const vendedorFechamentoId = ref('')
const periodoFechamento = ref(criarPeriodoAtual())
const fechandoMes = ref(false)

const listaVendedoresRaw = computed(() => {
  return props.Vendedores ?? props.vendedores ?? []
})

const vendedoresMapeados = ref([])

watch(
  listaVendedoresRaw,
  (novos) => {
    vendedoresMapeados.value = novos.map((vendedor) => ({
      id: vendedor.id || vendedor.Id,
      nome: vendedor.nome || vendedor.Nome,
      email: vendedor.email || vendedor.Email,
      filialLoja: vendedor.filialLoja || vendedor.FilialLoja,
      percentualComissao:
        vendedor.percentualComissao ??
        vendedor.PercentualComissao ??
        3.0,
      taxaEdicao:
        vendedor.percentualComissao ??
        vendedor.PercentualComissao ??
        3.0
    }))
  },
  { immediate: true }
)

const fechamentosMapeados = computed(() => {
  const lista = props.Fechamentos ?? props.fechamentos ?? []

  return lista.map((item) => ({
    id: item.Id ?? item.id,
    vendedorNome: item.VendedorNome ?? item.vendedorNome ?? 'Não informado',
    periodoReferencia: item.PeriodoReferencia ?? item.periodoReferencia ?? '—',
    totalVendasBrutas: item.TotalVendasBrutas ?? item.totalVendasBrutas ?? 0,
    totalComissao: item.TotalComissao ?? item.totalComissao ?? 0,
    qtdOs: item.QtdOs ?? item.qtdOs ?? 0,
    status: item.Status ?? item.status ?? 'ABERTO',
    dataFechamento: formatarData(item.DataFechamento ?? item.dataFechamento),
    dataPagamento: formatarData(item.DataPagamento ?? item.dataPagamento)
  }))
})

const salvarTaxaIndividual = (vendedorId, novaTaxa) => {
  if (!vendedorId || novaTaxa === undefined || novaTaxa === null) {
    return
  }

  const percentualNormalizado = Math.min(
    100,
    Math.max(0, Number(novaTaxa) || 0)
  )

  router.post(
    `/admin/comissoes/atualizar-taxa/${vendedorId}`,
    {
      PercentualComissao: percentualNormalizado
    },
    {
      preserveScroll: true,
      onSuccess: () => {
        alert('Porcentagem de comissão atualizada com sucesso!')
      }
    }
  )
}

const fecharMes = () => {
  if (!vendedorFechamentoId.value || !periodoFechamento.value) {
    alert('Selecione a vendedora e o período para realizar o fechamento.')
    return
  }

  const vendedoraSelecionada = vendedoresMapeados.value.find(
    (vendedor) => vendedor.id === vendedorFechamentoId.value
  )

  const nomeVendedora = vendedoraSelecionada?.nome || 'a vendedora selecionada'

  const confirmar = confirm(
    `Confirmar o fechamento das comissões de ${nomeVendedora} para o período ${periodoFechamento.value}?\n\nSomente comissões pendentes serão consolidadas.`
  )

  if (!confirmar) {
    return
  }

  fechandoMes.value = true

  router.post(
    `/admin/comissoes/fechar-mes?vendedorId=${vendedorFechamentoId.value}&periodo=${periodoFechamento.value}`,
    {},
    {
      preserveScroll: true,
      onSuccess: () => {
        alert('Fechamento de comissão realizado com sucesso!')
      },
      onFinish: () => {
        fechandoMes.value = false
      }
    }
  )
}

const efetuarBaixaPagamento = (id) => {
  if (!id) {
    return
  }

  const confirmar = confirm(
    'Confirmar o pagamento físico da comissão e dar baixa definitiva no sistema?'
  )

  if (!confirmar) {
    return
  }

  router.post(
    `/admin/comissoes/pagar/${id}`,
    {},
    {
      preserveScroll: true,
      onSuccess: () => {
        alert('Pagamento da comissão confirmado com sucesso!')
      }
    }
  )
}

const formatarMoeda = (valor) => {
  const numero = Number(valor)

  if (!Number.isFinite(numero)) {
    return '0,00'
  }

  return numero.toLocaleString('pt-BR', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  })
}

const formatarData = (dataRaw) => {
  if (!dataRaw) {
    return null
  }

  const data = new Date(dataRaw)

  if (Number.isNaN(data.getTime())) {
    return null
  }

  return data.toLocaleDateString('pt-BR')
}
</script>
