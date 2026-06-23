<template>
  <div class="min-h-screen bg-slate-50 p-4 md:p-8 font-sans text-slate-900">
    <div class="max-w-6xl mx-auto space-y-6">
      
      <div>
        <h1 class="text-2xl font-bold text-slate-950">Caixa e Fluxo de Recebimentos</h1>
        <p class="text-sm text-slate-500">Monitore as contas a receber, gere cobranças via PIX e realize baixas.</p>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        <div class="lg:col-span-2 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
          <h3 class="text-base font-bold text-slate-900">Parcelas em Aberto / Recebidas</h3>

          <div v-if="!(Parcelas ?? parcelas) || (Parcelas ?? parcelas).length === 0" class="text-center py-12 text-slate-400 text-sm">
            Nenhuma parcela financeira encontrada no sistema.
          </div>

          <div v-else class="overflow-x-auto">
            <table class="w-full text-left text-sm border-collapse">
              <thead>
                <tr class="border-b border-slate-100 text-slate-400 text-xs font-bold uppercase tracking-wider">
                  <th class="pb-3">Cliente / Origem</th>
                  <th class="pb-3 text-center">Vencimento</th>
                  <th class="pb-3 text-right">Valor</th>
                  <th class="pb-3 text-center">Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="p in (Parcelas ?? parcelas)" :key="p.id || p.Id" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
                  <td class="py-4">
                    <p class="font-semibold text-slate-800">{{ p.clienteNome || p.ClienteNome }}</p>
                    <p class="text-xs text-slate-400 font-mono">
                      {{ p.numeroOS || p.NumeroOS }} • Parc. {{ p.numeroParcela || p.NumeroParcela }}
                    </p>
                  </td>
                  <td class="py-4 text-center text-slate-600">
                    {{ formatarData(p.dataVencimento || p.DataVencimento) }}
                  </td>
                  <td class="py-4 text-right font-bold text-slate-950">
                    R$ {{ formatarMoeda(p.valor ?? p.Valor) }}
                  </td>
                  <td class="py-4 text-center">
                    <span v-if="p.dataPagamento || p.DataPagamento" class="bg-emerald-50 text-emerald-700 px-3 py-1 rounded-full text-xs font-semibold border border-emerald-100">
                      Recebido
                    </span>
                    
                    <div v-else class="flex items-center justify-center gap-2">
                      <button 
                        v-if="$page.props.PixHabilitadoNestaLoja || $page.props.pixHabilitadoNestaLoja"
                        @click="solicitarPixProducao(p)"
                        class="bg-teal-600 hover:bg-teal-700 text-white text-xs font-bold px-3 py-1.5 rounded-lg transition shadow-sm"
                      >
                        Gerar PIX Real
                      </button>

                      <button 
                        v-else 
                        @click="confirmarBaixaManual(p.id || p.Id)"
                        class="bg-slate-950 hover:bg-slate-800 text-white text-xs font-bold px-3 py-1.5 rounded-lg transition shadow-sm"
                      >
                        Baixar Manual
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm h-fit space-y-6">
          <h3 class="text-base font-bold text-slate-900">Terminal de Checkout</h3>
          
          <div v-if="!parcelaSelecionada && !$page.props.DadosPixAtivo" class="text-center py-12 border-2 border-dashed border-slate-100 rounded-xl">
            <p class="text-slate-400 text-xs px-4">Inicie um atendimento clicando em uma das ações de recebimento da tabela.</p>
          </div>

          <div v-else-if="$page.props.DadosPixAtivo" class="space-y-6">
            <div class="bg-slate-50 p-4 rounded-xl border border-slate-100 space-y-1">
              <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider">Aguardando Pagamento</span>
              <p class="text-2xl font-black text-teal-600 font-mono">
                R$ {{ formatarMoeda(parcelaSelecionada?.valor ?? parcelaSelecionada?.Valor) }}
              </p>
              <p class="text-xs text-slate-500 truncate">
                Ref: {{ parcelaSelecionada?.clienteNome || parcelaSelecionada?.ClienteNome }}
              </p>
            </div>

            <div class="flex flex-col items-center justify-center p-4 bg-white border border-slate-200 rounded-xl space-y-3 shadow-inner">
              <img 
                :src="$page.props.DadosPixAtivo.qrCodeImagemUrl || $page.props.DadosPixAtivo.QrCodeImagemUrl" 
                alt="QR Code PIX Real" 
                class="w-44 h-44 object-contain border border-slate-100 rounded-lg bg-white shadow-sm"
              />
              <span class="text-[11px] font-bold text-teal-600 uppercase tracking-wider bg-teal-50 px-2.5 py-0.5 rounded animate-pulse">
                Monitorando via Webhook...
              </span>
            </div>

            <div class="space-y-1">
              <label class="block text-[10px] font-bold uppercase text-slate-400">Código PIX Copia e Cola</label>
              <div class="flex gap-2">
                <input 
                  type="text" 
                  readonly 
                  :value="$page.props.DadosPixAtivo.pixCopiaECola || $page.props.DadosPixAtivo.PixCopiaECola" 
                  class="w-full bg-slate-50 border-slate-200 rounded-xl text-xs text-slate-500 truncate font-mono"
                />
                <button 
                  @click="copiarCopiaECola($page.props.DadosPixAtivo.pixCopiaECola || $page.props.DadosPixAtivo.PixCopiaECola)"
                  class="bg-slate-100 hover:bg-slate-200 text-slate-700 px-3 rounded-xl text-xs font-bold border border-slate-200 transition active:scale-95 whitespace-nowrap"
                >
                  Copiar
                </button>
              </div>
            </div>

            <button 
              @click="cancelarOperacaoPix"
              class="w-full bg-white border border-slate-200 text-slate-500 hover:text-slate-800 py-2.5 rounded-xl transition text-xs font-semibold"
            >
              Cancelar Cobrança PIX
            </button>
          </div>

          <div v-else-if="parcelaSelecionada" class="space-y-4">
            <div class="bg-slate-950 text-white p-4 rounded-xl space-y-1">
              <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider">Modo Manual Ativo</span>
              <p class="text-2xl font-black text-teal-400 font-mono">
                R$ {{ formatarMoeda(parcelaSelecionada.valor ?? parcelaSelecionada.Valor) }}
              </p>
              <p class="text-xs text-slate-300 truncate">
                {{ parcelaSelecionada.clienteNome || parcelaSelecionada.ClienteNome }}
              </p>
            </div>
            <p class="text-xs text-slate-500 leading-relaxed bg-slate-50 p-3 rounded-xl border border-slate-150">
              Esta filial opera sem chaves API ativas. Certifique-se de recolher o valor na maquininha ou dinheiro físico antes de confirmar a baixa.
            </p>
            <div class="flex flex-col gap-2">
              <button 
                @click="confirmarBaixaManual(parcelaSelecionada.id || parcelaSelecionada.Id)"
                class="w-full bg-slate-950 hover:bg-slate-800 text-white font-bold py-3 rounded-xl transition text-sm shadow-sm"
              >
                Confirmar Recebimento Manual
              </button>
              <button 
                @click="parcelaSelecionada = null"
                class="w-full bg-white border border-slate-200 text-slate-500 py-2 rounded-xl transition text-xs font-semibold"
              >
                Voltar
              </button>
            </div>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onUnmounted } from 'vue'
import { router } from '@inertiajs/vue3'

defineProps({
  parcelas: Array,
  Parcelas: Array
})

const parcelaSelecionada = ref(null)
const intervaloChecagem = ref(null)

// 1. Fluxo OpenPix
const solicitarPixProducao = (parcela) => {
  parcelaSelecionada.value = { ...parcela }
  const idReal = parcela.id ?? parcela.Id
  
  router.get('/caixa', { gerarPixParaId: idReal }, { 
    preserveState: true, 
    replace: true,
    onSuccess: () => {
      iniciarMonitoramentoPix(idReal)
    }
  })
}

// 2. Polling de Status com tratamento de Case Sensitivity na resposta
const iniciarMonitoramentoPix = (id) => {
  pararMonitoramentoPix()

  intervaloChecagem.value = setInterval(async () => {
    try {
      const resposta = await fetch(`/caixa/status/${id}`)
      if (resposta.ok) {
        const dados = await resposta.json()
              
        if (dados.pago || dados.Pago) {
          pararMonitoramentoPix()
          alert('Pagamento PIX confirmado com sucesso na OpenPix!')
          
          parcelaSelecionada.value = null
          router.get('/caixa', {}, { preserveState: false })
        }
      }
    } catch (erro) {
      console.error("Erro na checagem automática do PIX:", erro)
    }
  }, 3000)
}

// 3. Baixa Manual
const confirmarBaixaManual = (idParcela) => {
  if (confirm('Confirmar recebimento manual via Dinheiro ou Cartão Físico?')) {
    pararMonitoramentoPix()
    
    router.post(`/caixa/baixar/${idParcela}`, {}, {
      onSuccess: () => {
        parcelaSelecionada.value = null
      }
    })
  }
}

// Auxiliares de Formatação Defensiva
const formatarMoeda = (valor) => {
  if (valor === undefined || valor === null) return '0,00'
  return Number(valor).toFixed(2)
}

const formatarData = (dataRaw) => {
  if (!dataRaw) return '--/--/----'
  return new Date(dataRaw).toLocaleDateString('pt-BR')
}

const copiarCopiaECola = (texto) => {
  if (!texto) return
  navigator.clipboard.writeText(texto)
  alert('Código PIX Copia e Cola copiado para a área de transferência!')
}

const cancelarOperacaoPix = () => {
  pararMonitoramentoPix()
  if (router.page?.props) {
    // Evita mutação direta destrutiva limpando o estado de forma segura
    router.page.props.DadosPixAtivo = null
  }
  parcelaSelecionada.value = null
  router.get('/caixa', {}, { preserveState: false })
}

const pararMonitoramentoPix = () => {
  if (intervaloChecagem.value) {
    clearInterval(intervaloChecagem.value)
    intervaloChecagem.value = null
  }
}

onUnmounted(() => {
  pararMonitoramentoPix()
})
</script>