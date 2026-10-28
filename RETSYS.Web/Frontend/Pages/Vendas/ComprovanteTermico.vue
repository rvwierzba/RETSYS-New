<template>
  <div class="min-h-screen bg-slate-100 flex flex-col items-center justify-start p-4 md:p-8 font-mono text-slate-900">
    
    <!-- Barra Superior de Controle (Oculta na Impressão) -->
    <div class="w-full max-w-[360px] mb-4 flex justify-between items-center print:hidden">
      <a href="/vendas/pdv" class="text-xs font-bold text-slate-600 hover:text-slate-900">
        ← Voltar ao PDV
      </a>
      <button
        @click="imprimir"
        class="bg-teal-600 hover:bg-teal-700 text-white font-bold text-xs py-2 px-4 rounded-xl shadow transition flex items-center gap-1.5"
      >
        🖨️ Imprimir Cupom (80mm)
      </button>
    </div>

    <!-- Comprovante Térmico (80mm / 300px ~ 340px) -->
    <div class="w-full max-w-[360px] bg-white p-5 rounded-2xl shadow-md border border-slate-200 text-xs leading-tight print:shadow-none print:border-none print:p-0 print:max-w-full">
      
      <!-- Cabeçalho da Empresa -->
      <div class="text-center pb-3 border-b border-dashed border-slate-300 space-y-1">
        <h2 class="font-black text-sm uppercase tracking-wider">{{ Loja?.Nome || loja?.nome || 'RETSYS ÓTICA' }}</h2>
        <p v-if="Loja?.Cnpj || loja?.cnpj" class="text-[10px] text-slate-600">CNPJ: {{ Loja?.Cnpj || loja?.cnpj }}</p>
        <p v-if="Loja?.Endereco || loja?.endereco" class="text-[10px] text-slate-600">{{ Loja?.Endereco || loja?.endereco }}</p>
        <p v-if="Loja?.Telefone || loja?.telefone" class="text-[10px] text-slate-600">Tel: {{ Loja?.Telefone || loja?.telefone }}</p>
        <div class="pt-1">
          <span class="text-[9px] uppercase font-bold bg-slate-100 px-2 py-0.5 rounded">Comprovante Não-Fiscal</span>
        </div>
      </div>

      <!-- Detalhes do Documento -->
      <div class="py-2.5 border-b border-dashed border-slate-300 space-y-0.5 text-[11px]">
        <div class="flex justify-between">
          <span>VENDA:</span>
          <span class="font-bold">{{ Venda?.NumeroVenda || venda?.numeroVenda }}</span>
        </div>
        <div class="flex justify-between">
          <span>DATA/HORA:</span>
          <span>{{ Venda?.DataVenda || venda?.dataVenda }}</span>
        </div>
        <div class="flex justify-between">
          <span>ATENDENTE:</span>
          <span>{{ Venda?.VendedorNome || venda?.vendedorNome }}</span>
        </div>
        <div class="flex justify-between pt-1">
          <span>CLIENTE:</span>
          <span class="font-bold truncate max-w-[180px]">{{ Venda?.ClienteNome || venda?.clienteNome }}</span>
        </div>
        <div v-if="Venda?.ClienteCpf || venda?.clienteCpf" class="flex justify-between text-[10px]">
          <span>CPF:</span>
          <span>{{ Venda?.ClienteCpf || venda?.clienteCpf }}</span>
        </div>
      </div>

      <!-- Lista de Itens -->
      <div class="py-3 border-b border-dashed border-slate-300">
        <div class="text-[10px] uppercase font-bold text-slate-500 flex justify-between mb-1.5 pb-1 border-b border-slate-150">
          <span>QTD x ITEM</span>
          <span>TOTAL</span>
        </div>

        <div class="space-y-2">
          <div
            v-for="(item, idx) in (Venda?.Itens || venda?.itens)"
            :key="idx"
            class="space-y-0.5"
          >
            <p class="font-bold text-[11px]">{{ item.descricao || item.Descricao }}</p>
            <div class="flex justify-between text-[10px] text-slate-600">
              <span>{{ item.quantidade || item.Quantidade }} un x R$ {{ formatMoney(item.precoUnitario || item.PrecoUnitario) }}</span>
              <span class="font-bold text-slate-900">R$ {{ formatMoney(item.total || item.Total) }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Totais & Pagamento -->
      <div class="py-3 border-b border-dashed border-slate-300 space-y-1 text-xs">
        <div class="flex justify-between">
          <span>Subtotal:</span>
          <span>R$ {{ formatMoney(Venda?.ValorTotalBruto || venda?.valorTotalBruto) }}</span>
        </div>

        <div v-if="(Venda?.DescontoReais || venda?.descontoReais) > 0" class="flex justify-between text-rose-600">
          <span>Desconto:</span>
          <span>- R$ {{ formatMoney(Venda?.DescontoReais || venda?.descontoReais) }}</span>
        </div>

        <div class="flex justify-between font-black text-sm pt-1 border-t border-slate-200">
          <span>TOTAL PAGO:</span>
          <span>R$ {{ formatMoney(Venda?.ValorTotalLiquido || venda?.valorTotalLiquido) }}</span>
        </div>

        <div class="flex justify-between text-[11px] pt-1">
          <span>FORMA PGTO:</span>
          <span class="font-bold">{{ Venda?.FormaPagamento || venda?.formaPagamento }}</span>
        </div>

        <div v-if="(Venda?.Parcelas || venda?.parcelas) > 1" class="flex justify-between text-[11px]">
          <span>PARCELAMENTO:</span>
          <span>{{ Venda?.Parcelas || venda?.parcelas }}x</span>
        </div>
      </div>

      <!-- Rodapé / Termo de Garantia -->
      <div class="text-center pt-4 space-y-1.5 text-[10px] text-slate-500">
        <p class="font-bold text-slate-700">Obrigado pela preferência!</p>
        <p>Apresente este comprovante para ajustes, limpezas periódicas e garantias.</p>
        <p class="text-[9px] pt-1">RETSYS Ótica Cloud ERP</p>
      </div>

    </div>

  </div>
</template>

<script setup>
import { onMounted } from 'vue'

const props = defineProps({
  Venda: Object,
  venda: Object,
  Loja: Object,
  loja: Object
})

const imprimir = () => {
  window.print()
}

onMounted(() => {
  // auto-trigger print dialog if opened in popup
  // window.print()
})

const formatMoney = (val) => {
  if (val === undefined || val === null) return '0,00'
  return Number(val).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}
</script>

<style>
@media print {
  body {
    background: transparent !important;
  }
}
</style>

