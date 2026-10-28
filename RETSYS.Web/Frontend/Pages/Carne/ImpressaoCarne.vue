<template>
  <div class="min-h-screen bg-slate-100 p-4 md:p-8 font-sans text-slate-900">
    
    <!-- Barra Superior de Controle (Oculta na Impressão) -->
    <div class="max-w-4xl mx-auto mb-6 flex flex-wrap justify-between items-center gap-3 print:hidden bg-white p-4 rounded-2xl shadow-sm border border-slate-200">
      <div class="flex items-center gap-3">
        <a href="/ordens" class="text-xs font-bold text-slate-600 hover:text-slate-900">
          ← Voltar para Ordens
        </a>
        <span class="text-slate-300">|</span>
        <span class="text-xs font-bold text-teal-700">
          Carnê da O.S. {{ (Ordem || ordem)?.numeroOS || (Ordem || ordem)?.NumeroOS }}
        </span>
      </div>
      
      <div class="flex items-center gap-2">
        <button
          @click="imprimir"
          class="bg-slate-950 hover:bg-teal-600 text-white font-black text-xs py-2.5 px-5 rounded-xl shadow transition flex items-center gap-2"
        >
          <span>🖨️ Imprimir Carnê de Pagamento</span>
        </button>
      </div>
    </div>

    <!-- Lista de Lâminas do Carnê (Impressão A4) -->
    <div class="max-w-4xl mx-auto space-y-6 print:space-y-4 print:max-w-full">
      
      <div
        v-for="(p, index) in parcelasLista"
        :key="p.id || p.Id || index"
        class="bg-white p-6 rounded-2xl border border-slate-300 shadow-sm print:shadow-none print:border-slate-800 print:rounded-none page-break-inside-avoid"
      >
        <div class="grid grid-cols-1 md:grid-cols-12 gap-4">
          
          <!-- Canhoto da Ótica (4 Colunas) -->
          <div class="md:col-span-4 border-r-0 md:border-r-2 border-dashed border-slate-300 md:pr-4 space-y-2 text-xs">
            <div class="pb-2 border-b border-slate-200">
              <p class="font-black text-xs uppercase">{{ (Loja || loja)?.Nome || (Loja || loja)?.nome || 'RETSYS ÓTICA' }}</p>
              <p class="text-[10px] text-slate-500 font-bold uppercase">Canhoto da Ótica</p>
            </div>

            <div class="space-y-1 text-[11px]">
              <p><span class="font-bold">O.S.:</span> {{ (Ordem || ordem)?.numeroOS || (Ordem || ordem)?.NumeroOS }}</p>
              <p><span class="font-bold">Parcela:</span> {{ p.numeroParcela || p.NumeroParcela }} / {{ p.totalParcelas || p.TotalParcelas || parcelasLista.length }}</p>
              <p><span class="font-bold">Vencimento:</span> <span class="font-black text-slate-900">{{ p.dataVencimento || p.DataVencimento }}</span></p>
              <p><span class="font-bold">Valor:</span> <span class="font-black text-teal-700">R$ {{ formatMoney(p.valor || p.Valor) }}</span></p>
              <p class="truncate"><span class="font-bold">Cliente:</span> {{ (Ordem || ordem)?.cliente?.nome || (Ordem || ordem)?.Cliente?.Nome }}</p>
            </div>

            <div class="pt-3 border-t border-slate-200 text-[10px] space-y-1">
              <p>Autenticação Mecânica / Visto:</p>
              <div class="h-8 border border-dashed border-slate-300 rounded bg-slate-50"></div>
            </div>
          </div>

          <!-- Lâmina do Cliente / Carnê Principal (8 Colunas) -->
          <div class="md:col-span-8 md:pl-2 space-y-3 text-xs">
            
            <div class="flex justify-between items-start pb-2 border-b border-slate-200">
              <div>
                <h3 class="font-black text-sm uppercase text-slate-900">{{ (Loja || loja)?.Nome || (Loja || loja)?.nome || 'RETSYS ÓTICA' }}</h3>
                <p class="text-[10px] text-slate-500">
                  CNPJ: {{ (Loja || loja)?.Cnpj || (Loja || loja)?.cnpj || '00.000.000/0001-00' }} | Tel: {{ (Loja || loja)?.Telefone || (Loja || loja)?.telefone }}
                </p>
              </div>
              <div class="text-right">
                <span class="bg-slate-900 text-white font-mono font-bold text-xs px-2.5 py-1 rounded">
                  Parcela {{ p.numeroParcela || p.NumeroParcela }} / {{ p.totalParcelas || p.TotalParcelas || parcelasLista.length }}
                </span>
              </div>
            </div>

            <!-- Dados da Parcela -->
            <div class="grid grid-cols-2 md:grid-cols-4 gap-2 bg-slate-50 p-2.5 rounded-xl border border-slate-200 text-center font-mono">
              <div>
                <p class="text-[9px] uppercase font-bold text-slate-500 font-sans">Nº Documento</p>
                <p class="font-bold text-xs">{{ (Ordem || ordem)?.numeroOS || (Ordem || ordem)?.NumeroOS }}-{{ p.numeroParcela || p.NumeroParcela }}</p>
              </div>
              <div>
                <p class="text-[9px] uppercase font-bold text-slate-500 font-sans">Emissão</p>
                <p class="font-bold text-xs">{{ (Ordem || ordem)?.dataEntrada || (Ordem || ordem)?.DataEntrada }}</p>
              </div>
              <div>
                <p class="text-[9px] uppercase font-bold text-slate-500 font-sans">Vencimento</p>
                <p class="font-black text-xs text-rose-700">{{ p.dataVencimento || p.DataVencimento }}</p>
              </div>
              <div>
                <p class="text-[9px] uppercase font-bold text-slate-500 font-sans">Valor da Parcela</p>
                <p class="font-black text-xs text-teal-700">R$ {{ formatMoney(p.valor || p.Valor) }}</p>
              </div>
            </div>

            <!-- Dados do Pagador -->
            <div class="text-[11px] space-y-0.5">
              <p><span class="font-bold">Pagador:</span> {{ (Ordem || ordem)?.cliente?.nome || (Ordem || ordem)?.Cliente?.Nome }} - CPF: {{ (Ordem || ordem)?.cliente?.cpf || (Ordem || ordem)?.Cliente?.CPF }}</p>
              <p><span class="font-bold">Endereço:</span> {{ (Ordem || ordem)?.cliente?.endereco || (Ordem || ordem)?.Cliente?.Endereco }}</p>
            </div>

            <!-- Instruções de Pagamento -->
            <div class="bg-amber-50/60 p-2 rounded-xl border border-amber-200 text-[10px] text-amber-900 space-y-0.5">
              <p class="font-bold">INSTRUÇÕES DE PAGAMENTO:</p>
              <p>• Pagável no caixa da ótica até o vencimento.</p>
              <p>• Após o vencimento, incidirão juros de mora de 1% ao mês e multa por atraso.</p>
              <p>• Mantenha seus pagamentos em dia para preservar seu limite de crediário.</p>
            </div>

            <!-- Botão de Quitação Rápida no Balcão (Oculto na Impressão) -->
            <div v-if="!p.paga && !p.Paga" class="pt-2 flex justify-end print:hidden">
              <button
                @click="quitarParcela(p.id || p.Id, p.valor || p.Valor)"
                class="bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-xs py-1.5 px-3 rounded-lg shadow-sm transition flex items-center gap-1"
              >
                <span>💵 Quitar esta Parcela no Caixa</span>
              </button>
            </div>
            <div v-else class="pt-2 flex justify-end print:hidden">
              <span class="text-xs font-bold text-emerald-700 bg-emerald-50 px-3 py-1 rounded-lg border border-emerald-200">
                ✅ Parcela Quitada em {{ p.dataPagamento || p.DataPagamento }}
              </span>
            </div>

          </div>

        </div>
      </div>

    </div>

  </div>
</template>

<script setup>
import { computed } from 'vue'
import { router } from '@inertiajs/vue3'

const props = defineProps({
  Ordem: Object,
  ordem: Object,
  Loja: Object,
  loja: Object
})

const parcelasLista = computed(() => {
  return (props.Ordem?.Parcelas || props.Ordem?.parcelas || props.ordem?.Parcelas || props.ordem?.parcelas || [])
})

const imprimir = () => {
  window.print()
}

const quitarParcela = (parcelaId, valor) => {
  if (confirm(`Confirmar recebimento de R$ ${formatMoney(valor)} para quitação desta parcela?`)) {
    router.post(`/parcelas/${parcelaId}/quitar`, {
      valorPago: valor,
      formaPagamento: 'DINHEIRO',
      observacao: 'Quitação efetuada no balcão'
    }, {
      preserveScroll: true,
      onSuccess: () => {
        alert('Parcela quitada com sucesso!')
      }
    })
  }
}

const formatMoney = (val) => {
  if (val === undefined || val === null) return '0,00'
  return Number(val).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}
</script>

<style>
@media print {
  .page-break-inside-avoid {
    break-inside: avoid;
    page-break-inside: avoid;
  }
}
</style>

