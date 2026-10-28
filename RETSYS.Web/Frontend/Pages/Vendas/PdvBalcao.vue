<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-7xl mx-auto">
      
      <!-- Cabeçalho -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <div>
          <div class="flex items-center gap-2">
            <span class="bg-emerald-50 text-emerald-700 text-xs font-bold px-2.5 py-1 rounded-md border border-emerald-200">
              ⚡ PDV Rápido de Balcão
            </span>
          </div>
          <h1 class="text-2xl font-black text-slate-900 mt-1">Frente de Caixa Óptica</h1>
          <p class="text-sm text-slate-500">Venda direta de óculos solares, armações pronta-entrega, lentes de contato e acessórios.</p>
        </div>

        <div class="flex items-center gap-3">
          <a href="/vendas" class="px-4 py-2.5 bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs rounded-xl transition">
            📋 Histórico de Vendas
          </a>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
        
        <!-- Coluna Esquerda: Catálogo / Seleção de Produtos e Cliente (7 Colunas) -->
        <div class="lg:col-span-7 space-y-6">
          
          <!-- Seleção de Cliente e Vendedor -->
          <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm space-y-4">
            <h3 class="font-black text-slate-900 text-sm">1. Atendimento</h3>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 text-xs">
              <div>
                <label class="block font-bold text-slate-700 mb-1">Cliente</label>
                <select v-model="venda.clienteId" class="w-full p-2.5 rounded-xl border border-slate-300 font-medium">
                  <option :value="null">👤 Consumidor Balcão (Não Identificado)</option>
                  <option v-for="c in (Clientes || clientes)" :key="c.id || c.Id" :value="c.id || c.Id">
                    {{ c.nome || c.Nome }} ({{ c.telefone || c.Telefone || 'Sem tel' }})
                  </option>
                </select>
              </div>

              <div>
                <label class="block font-bold text-slate-700 mb-1">Vendedor / Consultor</label>
                <select v-model="venda.vendedorId" class="w-full p-2.5 rounded-xl border border-slate-300 font-medium">
                  <option :value="null">Selecione o vendedor...</option>
                  <option v-for="v in (Vendedores || vendedores)" :key="v.id || v.Id" :value="v.id || v.Id">
                    {{ v.nome || v.Nome }}
                  </option>
                </select>
              </div>
            </div>
          </div>

          <!-- Busca de Produtos / Estoque da Ótica -->
          <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm space-y-4">
            <div class="flex justify-between items-center">
              <h3 class="font-black text-slate-900 text-sm">2. Adicionar Produtos</h3>
              <button @click="abrirItemPersonalizado" type="button" class="text-xs text-teal-600 font-bold hover:underline">
                + Item Personalizado / Acessório
              </button>
            </div>

            <!-- Campo de Busca -->
            <input
              v-model="filtroProduto"
              type="text"
              placeholder="Buscar por marca, modelo, cor ou SKU..."
              class="w-full p-3 rounded-xl border border-slate-300 text-xs shadow-sm focus:border-teal-500"
            />

            <!-- Grade de Produtos Rápidos -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-3 max-h-[380px] overflow-y-auto pr-1">
              <div
                v-for="p in produtosFiltrados"
                :key="p.id || p.Id"
                @click="adicionarArmacao(p)"
                class="p-3 rounded-xl border border-slate-200 hover:border-teal-500 hover:bg-teal-50/20 cursor-pointer transition flex items-center justify-between"
              >
                <div>
                  <span class="text-[10px] font-bold text-teal-700 uppercase bg-teal-50 px-1.5 py-0.5 rounded">
                    {{ p.marca || p.Marca }}
                  </span>
                  <p class="font-bold text-slate-900 text-xs mt-1">{{ p.modeloReferencia || p.ModeloReferencia }}</p>
                  <p class="text-[11px] text-slate-500">{{ p.cor || p.Cor }} | Estq: {{ p.quantidadeEstoque || p.QuantidadeEstoque }} un.</p>
                </div>
                <div class="text-right">
                  <p class="font-black text-teal-700 text-sm">R$ {{ formatMoney(p.precoVenda || p.PrecoVenda) }}</p>
                  <span class="text-[10px] text-teal-600 font-bold">+ Inserir</span>
                </div>
              </div>
            </div>
          </div>

        </div>

        <!-- Coluna Direita: Carrinho e Finalização (5 Colunas) -->
        <div class="lg:col-span-5 space-y-6">
          <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm space-y-4">
            <h3 class="font-black text-slate-900 text-sm">3. Carrinho de Venda ({{ venda.itens.length }})</h3>

            <div v-if="venda.itens.length === 0" class="text-center py-12 text-slate-400 text-xs">
              Nenhum item adicionado ao carrinho.
            </div>

            <div v-else class="space-y-3 max-h-[300px] overflow-y-auto pr-1">
              <div
                v-for="(item, idx) in venda.itens"
                :key="idx"
                class="p-3 rounded-xl bg-slate-50 border border-slate-200 flex items-center justify-between gap-3 text-xs"
              >
                <div class="flex-1">
                  <p class="font-bold text-slate-800">{{ item.descricao }}</p>
                  <p class="text-[11px] text-slate-500">R$ {{ formatMoney(item.precoUnitario) }} un.</p>
                </div>

                <div class="flex items-center gap-2">
                  <input
                    v-model.number="item.quantidade"
                    type="number"
                    min="1"
                    class="w-12 p-1 text-center font-bold rounded-lg border border-slate-300"
                  />
                  <p class="font-black text-slate-900 text-xs min-w-[70px] text-right">
                    R$ {{ formatMoney(item.quantidade * item.precoUnitario) }}
                  </p>
                  <button @click="removerItem(idx)" class="text-rose-500 hover:text-rose-700 font-bold px-1">✕</button>
                </div>
              </div>
            </div>

            <!-- Resumo Financeiro -->
            <div class="pt-4 border-t border-slate-200 space-y-2 text-xs">
              <div class="flex justify-between text-slate-600">
                <span>Subtotal Bruto:</span>
                <span class="font-bold font-mono">R$ {{ formatMoney(totalBruto) }}</span>
              </div>

              <div class="flex justify-between items-center text-slate-600">
                <span>Desconto (R$):</span>
                <input
                  v-model.number="venda.descontoReais"
                  type="number"
                  min="0"
                  step="0.50"
                  class="w-24 p-1 text-right font-bold rounded-lg border border-slate-300 font-mono"
                />
              </div>

              <div class="flex justify-between text-base font-black text-slate-900 pt-2 border-t">
                <span>Total a Pagar:</span>
                <span class="text-teal-700 font-mono">R$ {{ formatMoney(totalLiquido) }}</span>
              </div>
            </div>

            <!-- Forma de Pagamento -->
            <div class="space-y-3 pt-3 border-t">
              <label class="block font-bold text-slate-700 text-xs">Forma de Pagamento</label>
              <div class="grid grid-cols-2 gap-2 text-xs font-bold">
                <button
                  type="button"
                  @click="venda.formaPagamento = 'DINHEIRO'"
                  :class="venda.formaPagamento === 'DINHEIRO' ? 'bg-teal-600 text-white' : 'bg-slate-100 text-slate-700'"
                  class="p-2.5 rounded-xl border transition"
                >
                  💵 Dinheiro
                </button>
                <button
                  type="button"
                  @click="venda.formaPagamento = 'PIX'"
                  :class="venda.formaPagamento === 'PIX' ? 'bg-teal-600 text-white' : 'bg-slate-100 text-slate-700'"
                  class="p-2.5 rounded-xl border transition"
                >
                  💠 PIX
                </button>
                <button
                  type="button"
                  @click="venda.formaPagamento = 'CARTAO_DEBITO'"
                  :class="venda.formaPagamento === 'CARTAO_DEBITO' ? 'bg-teal-600 text-white' : 'bg-slate-100 text-slate-700'"
                  class="p-2.5 rounded-xl border transition"
                >
                  💳 Débito
                </button>
                <button
                  type="button"
                  @click="venda.formaPagamento = 'CARTAO_CREDITO'"
                  :class="venda.formaPagamento === 'CARTAO_CREDITO' ? 'bg-teal-600 text-white' : 'bg-slate-100 text-slate-700'"
                  class="p-2.5 rounded-xl border transition"
                >
                  💳 Crédito
                </button>
              </div>

              <div v-if="venda.formaPagamento === 'CARTAO_CREDITO'" class="text-xs">
                <label class="block font-bold text-slate-700 mb-1">Parcelamento Cartão</label>
                <select v-model.number="venda.parcelas" class="w-full p-2 rounded-xl border border-slate-300">
                  <option :value="1">1x de R$ {{ formatMoney(totalLiquido) }} (À vista)</option>
                  <option v-for="n in 12" :key="n" :value="n">
                    {{ n }}x de R$ {{ formatMoney(totalLiquido / n) }}
                  </option>
                </select>
              </div>
            </div>

            <!-- Botão de Finalizar -->
            <button
              @click="finalizarVenda"
              :disabled="venda.itens.length === 0 || processando"
              class="w-full bg-teal-600 hover:bg-teal-700 disabled:bg-slate-300 text-white font-black py-3.5 rounded-xl text-xs uppercase tracking-wider transition shadow-lg flex items-center justify-center gap-2"
            >
              <span v-if="!processando">✅ Finalizar Venda &amp; Emitir Cupom</span>
              <span v-else>Processando Venda...</span>
            </button>
          </div>
        </div>

      </div>

    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { router } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  Clientes: Array,
  clientes: Array,
  Armacoes: Array,
  armacoes: Array,
  Vendedores: Array,
  vendedores: Array,
  NomeLoja: String
})

const filtroProduto = ref('')
const processando = ref(false)

const venda = reactive({
  clienteId: null,
  vendedorId: null,
  formaPagamento: 'DINHEIRO',
  descontoReais: 0,
  parcelas: 1,
  observacoes: '',
  itens: []
})

const listaArmacoes = computed(() => props.Armacoes || props.armacoes || [])

const produtosFiltrados = computed(() => {
  if (!filtroProduto.value) return listaArmacoes.value.slice(0, 15)
  const termo = filtroProduto.value.toLowerCase()
  return listaArmacoes.value.filter(a => {
    const marca = (a.marca || a.Marca || '').toLowerCase()
    const modelo = (a.modeloReferencia || a.ModeloReferencia || '').toLowerCase()
    const cor = (a.cor || a.Cor || '').toLowerCase()
    const sku = (a.codigoSku || a.CodigoSku || '').toLowerCase()
    return marca.includes(termo) || modelo.includes(termo) || cor.includes(termo) || sku.includes(termo)
  }).slice(0, 20)
})

const adicionarArmacao = (prod) => {
  venda.itens.push({
    armacaoId: prod.id || prod.Id,
    tipoItem: 'ARMACAO',
    descricao: `${prod.marca || prod.Marca} ${prod.modeloReferencia || prod.ModeloReferencia} (${prod.cor || prod.Cor})`,
    quantidade: 1,
    precoUnitario: Number(prod.precoVenda || prod.PrecoVenda) || 0
  })
}

const abrirItemPersonalizado = () => {
  const desc = prompt('Descrição do Produto / Acessório (Ex: Kit Limpa Lentes, Flanela, Estojo, Lente Contato):')
  if (!desc) return
  const precoStr = prompt('Preço de Venda (R$):', '25.00')
  const preco = parseFloat(precoStr.replace(',', '.')) || 0

  venda.itens.push({
    armacaoId: null,
    tipoItem: 'ACESSORIO',
    descricao: desc,
    quantidade: 1,
    precoUnitario: preco
  })
}

const removerItem = (idx) => {
  venda.itens.splice(idx, 1)
}

const totalBruto = computed(() => {
  return venda.itens.reduce((acc, item) => acc + (item.quantidade * item.precoUnitario), 0)
})

const totalLiquido = computed(() => {
  const total = totalBruto.value - (venda.descontoReais || 0)
  return Math.max(0, total)
})

const finalizarVenda = async () => {
  if (venda.itens.length === 0) {
    alert('Adicione pelo menos um produto ao carrinho.')
    return
  }

  processando.value = true

  try {
    const res = await fetch('/vendas/finalizar', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      },
      body: JSON.stringify(venda)
    })

    if (res.ok) {
      const data = await res.json()
      alert(`Venda ${data.numeroVenda || data.NumeroVenda} realizada com sucesso!`)
      window.location.href = `/vendas/${data.vendaId || data.VendaId}/comprovante`
    } else {
      const err = await res.text()
      alert('Erro ao finalizar venda: ' + err)
      processando.value = false
    }
  } catch (e) {
    alert('Erro de conexão ao finalizar venda.')
    processando.value = false
  }
}

const formatMoney = (val) => {
  if (val === undefined || val === null) return '0,00'
  return Number(val).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}
</script>

