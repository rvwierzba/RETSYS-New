<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-7xl mx-auto">
      
      <!-- CABEÇALHO DO PDV EXPRESSO (PADRÃO SSÓTICA / PWI) -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white p-6 rounded-3xl border border-slate-200 shadow-sm">
        <div>
          <div class="flex items-center gap-2">
            <span class="bg-amber-50 text-amber-800 text-[10px] font-mono font-bold px-2.5 py-1 rounded-full border border-amber-200 uppercase tracking-wider flex items-center gap-1.5">
              <span>🛒</span> Frente de Caixa &amp; Balcão
            </span>
          </div>
          <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight mt-1">PDV Rápido de Venda Direta</h1>
          <p class="text-xs text-slate-500">Venda expressa de óculos de sol, armações avulsas, lentes de contato e kits de limpeza.</p>
        </div>

        <div class="flex items-center gap-2 font-mono text-xs">
          <Link 
            href="/vendas" 
            class="px-4 py-2.5 bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold rounded-2xl transition"
          >
            📋 Histórico de Vendas
          </Link>
          <span class="px-3.5 py-2.5 bg-teal-50 text-teal-800 font-bold rounded-2xl border border-teal-200">
            Caixa Operacional
          </span>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
        
        <!-- COLUNA ESQUERDA: CATÁLOGO DE PRODUTOS & BUSCA (7 COLUNAS) -->
        <div class="lg:col-span-7 space-y-4">
          
          <!-- Seleção Rápida de Atendente e Cliente -->
          <div class="bg-white p-5 rounded-3xl border border-slate-200 shadow-sm grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs font-mono">
            <div>
              <label class="block font-bold text-slate-400 uppercase text-[10px] mb-1">Cliente / Consumidor</label>
              <select v-model="venda.clienteId" class="w-full rounded-2xl border-slate-200 text-xs bg-slate-50 font-bold">
                <option :value="null">👤 Consumidor Balcão (Avulso)</option>
                <option v-for="c in (Clientes || clientes || [])" :key="c.id || c.Id" :value="c.id || c.Id">
                  {{ c.nome || c.Nome }} ({{ c.telefone || c.Telefone || 'Sem tel' }})
                </option>
              </select>
            </div>

            <div>
              <label class="block font-bold text-slate-400 uppercase text-[10px] mb-1">Vendedora / Consultora</label>
              <select v-model="venda.vendedorId" class="w-full rounded-2xl border-slate-200 text-xs bg-slate-50 font-bold">
                <option :value="null">👩‍💼 (Vendedora do Caixa)</option>
                <option v-for="v in (Vendedores || vendedores || [])" :key="v.id || v.Id" :value="v.id || v.Id">
                  {{ v.nome || v.Nome }}
                </option>
              </select>
            </div>
          </div>

          <!-- Busca de Produtos no Estoque -->
          <div class="bg-white p-5 rounded-3xl border border-slate-200 shadow-sm space-y-4">
            <div class="flex items-center justify-between">
              <span class="text-xs font-mono font-bold uppercase text-slate-700">Catálogo de Pronta-Entrega</span>
              <button 
                type="button" 
                @click="abrirItemPersonalizado" 
                class="text-xs font-mono font-bold text-teal-600 hover:text-teal-700"
              >
                + Item Avulso / Serviço
              </button>
            </div>

            <div class="relative">
              <span class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">🔍</span>
              <input
                v-model="filtroProduto"
                type="text"
                placeholder="Buscar por marca, modelo, cor ou código SKU..."
                class="w-full pl-10 pr-4 py-2.5 rounded-2xl border border-slate-200 text-xs font-semibold text-slate-800 focus:border-teal-500 bg-slate-50"
              />
            </div>

            <!-- Grade de Cards de Produtos com Clique Rápido -->
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 max-h-[460px] overflow-y-auto pr-1 custom-scrollbar">
              <div
                v-for="p in produtosFiltrados"
                :key="p.id || p.Id"
                @click="adicionarArmacao(p)"
                class="p-4 rounded-2xl border border-slate-200 hover:border-teal-500 hover:bg-teal-50/20 cursor-pointer transition flex flex-col justify-between space-y-2 group bg-slate-50/40"
              >
                <div>
                  <div class="flex items-center justify-between">
                    <span class="text-[9px] font-mono font-bold text-teal-800 uppercase bg-teal-50 px-2 py-0.5 rounded-md border border-teal-200">
                      {{ p.marca || p.Marca || 'Marca' }}
                    </span>
                    <span class="text-[10px] font-mono text-slate-400">
                      Estq: <b>{{ p.quantidadeEstoque || p.QuantidadeEstoque }}</b>
                    </span>
                  </div>
                  <h4 class="font-black text-slate-900 text-sm mt-1.5 group-hover:text-teal-700 transition">
                    {{ p.modeloReferencia || p.ModeloReferencia }}
                  </h4>
                  <p class="text-[11px] text-slate-500">{{ p.cor || p.Cor }}</p>
                </div>

                <div class="flex items-center justify-between pt-2 border-t border-slate-100 font-mono">
                  <p class="font-black text-slate-900 text-sm">
                    R$ {{ formatMoney(p.precoVenda || p.PrecoVenda) }}
                  </p>
                  <span class="text-[10px] font-bold text-teal-600 bg-teal-50 px-2 py-1 rounded-lg border border-teal-200 group-hover:bg-teal-600 group-hover:text-white transition">
                    + Inserir
                  </span>
                </div>
              </div>
            </div>
          </div>

        </div>

        <!-- COLUNA DIREITA: CARRINHO DE COMPRAS & FECHAMENTO (5 COLUNAS) -->
        <div class="lg:col-span-5 space-y-4">
          <div class="bg-white p-6 rounded-3xl border border-slate-200 shadow-sm space-y-5">
            <div class="flex items-center justify-between border-b border-slate-100 pb-3">
              <h3 class="text-sm font-black text-slate-900 font-mono flex items-center gap-2">
                <span>🛒</span> Carrinho de Venda ({{ venda.itens.length }})
              </h3>
              <button 
                v-if="venda.itens.length > 0"
                @click="venda.itens = []"
                class="text-[10px] font-mono text-rose-600 hover:underline"
              >
                Esvaziar Carrinho
              </button>
            </div>

            <!-- Lista de Itens no Carrinho -->
            <div v-if="venda.itens.length === 0" class="text-center py-12 text-slate-400 text-xs font-mono space-y-2">
              <span class="text-3xl block">🛍️</span>
              <p>Carrinho vazio. Clique nos produtos à esquerda para inseri-los na venda.</p>
            </div>

            <div v-else class="space-y-2.5 max-h-[260px] overflow-y-auto pr-1 custom-scrollbar">
              <div 
                v-for="(item, idx) in venda.itens" 
                :key="idx" 
                class="p-3 bg-slate-50 rounded-2xl border border-slate-200 flex items-center justify-between text-xs"
              >
                <div class="flex-1 truncate mr-2">
                  <p class="font-bold text-slate-900 truncate">{{ item.descricao }}</p>
                  <p class="text-[10px] text-slate-400 font-mono">
                    {{ item.quantidade }}x de R$ {{ formatMoney(item.precoUnitario) }}
                  </p>
                </div>

                <div class="flex items-center gap-2 font-mono">
                  <span class="font-black text-slate-900">
                    R$ {{ formatMoney(item.quantidade * item.precoUnitario) }}
                  </span>
                  <button 
                    @click="removerItem(idx)" 
                    class="text-slate-400 hover:text-rose-600 font-bold p-1"
                    title="Remover Item"
                  >
                    ✕
                  </button>
                </div>
              </div>
            </div>

            <!-- Totalizadores -->
            <div class="p-4 bg-teal-50/60 rounded-2xl border border-teal-200 space-y-2 font-mono text-xs">
              <div class="flex justify-between text-slate-600">
                <span>Subtotal Bruto:</span>
                <b>R$ {{ formatMoney(subtotal) }}</b>
              </div>

              <div class="flex items-center justify-between text-amber-800">
                <span>Desconto (R$):</span>
                <input 
                  v-model.number="venda.desconto" 
                  type="number" 
                  step="0.01" 
                  min="0" 
                  placeholder="0.00" 
                  class="w-24 text-right rounded-xl border-amber-300 text-xs font-mono font-bold bg-white p-1"
                />
              </div>

              <div class="flex justify-between text-base font-black text-teal-900 pt-2 border-t border-teal-200/80">
                <span>TOTAL A PAGAR:</span>
                <span>R$ {{ formatMoney(totalFinal) }}</span>
              </div>
            </div>

            <!-- Formas de Pagamento Expressa -->
            <div class="space-y-2 font-mono text-xs">
              <label class="block font-bold text-slate-400 uppercase text-[10px]">Forma de Pagamento</label>
              <div class="grid grid-cols-2 gap-2">
                <button 
                  type="button" 
                  v-for="fp in formasPagamento" 
                  :key="fp.id"
                  @click="venda.formaPagamento = fp.id"
                  :class="venda.formaPagamento === fp.id ? 'bg-teal-600 text-white font-black' : 'bg-slate-100 text-slate-700 hover:bg-slate-200 font-bold'"
                  class="p-2.5 rounded-xl transition flex items-center gap-1.5 justify-center"
                >
                  <span>{{ fp.icone }}</span>
                  <span>{{ fp.nome }}</span>
                </button>
              </div>
            </div>

            <!-- Botão de Finalizar Venda -->
            <button
              @click="finalizarVenda"
              :disabled="venda.itens.length === 0 || processando"
              class="w-full py-3.5 rounded-2xl bg-teal-600 hover:bg-teal-700 disabled:bg-slate-200 text-white font-mono font-black text-xs uppercase tracking-wider shadow-lg shadow-teal-700/20 transition flex items-center justify-center gap-2"
            >
              <span v-if="processando">Emitindo Venda...</span>
              <span v-else>✓ Finalizar e Emitir Cupom</span>
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
  Clientes: Array, clientes: Array,
  Armacoes: Array, armacoes: Array,
  Vendedores: Array, vendedores: Array,
  NomeLoja: String, nomeLoja: String
})

const filtroProduto = ref('')
const processando = ref(false)

const venda = ref({
  clienteId: null,
  vendedorId: null,
  formaPagamento: 'DINHEIRO',
  desconto: 0,
  itens: []
})

const formasPagamento = [
  { id: 'DINHEIRO', nome: 'Dinheiro', icone: '💵' },
  { id: 'PIX', nome: 'PIX', icone: '⚡' },
  { id: 'CARTAO_DEBITO', nome: 'Débito', icone: '💳' },
  { id: 'CARTAO_CREDITO', nome: 'Crédito', icone: '💳' }
]

const produtosFiltrados = computed(() => {
  const lista = props.Armacoes || props.armacoes || []
  const busca = filtroProduto.value.toLowerCase().trim()
  if (!busca) return lista
  return lista.filter(p => {
    const mod = (p.modeloReferencia || p.ModeloReferencia || '').toLowerCase()
    const mar = (p.marca || p.Marca || '').toLowerCase()
    const cor = (p.cor || p.Cor || '').toLowerCase()
    const sku = (p.codigoSku || p.CodigoSku || '').toLowerCase()
    return mod.includes(busca) || mar.includes(busca) || cor.includes(busca) || sku.includes(busca)
  })
})

const subtotal = computed(() => {
  return venda.value.itens.reduce((acc, it) => acc + (it.quantidade * it.precoUnitario), 0)
})

const totalFinal = computed(() => {
  return Math.max(0, subtotal.value - (Number(venda.value.desconto) || 0))
})

const adicionarArmacao = (p) => {
  venda.value.itens.push({
    armacaoId: p.id || p.Id,
    descricao: `${p.marca || p.Marca || ''} - ${p.modeloReferencia || p.ModeloReferencia} (${p.cor || p.Cor})`,
    quantidade: 1,
    precoUnitario: p.precoVenda || p.PrecoVenda || 0
  })
}

const removerItem = (idx) => {
  venda.value.itens.splice(idx, 1)
}

const abrirItemPersonalizado = () => {
  const desc = prompt('Nome do Produto ou Acessório (Ex: Flanela, Estojo, Cordão):')
  if (!desc) return
  const preco = parseFloat(prompt('Valor do Item (R$):', '25.00')) || 0
  venda.value.itens.push({
    armacaoId: null,
    descricao: desc,
    quantidade: 1,
    precoUnitario: preco
  })
}

const finalizarVenda = () => {
  if (venda.value.itens.length === 0) return
  processando.value = true

  router.post('/vendas/finalizar', venda.value, {
    preserveScroll: true,
    onSuccess: () => {
      processando.value = false
      venda.value.itens = []
      venda.value.desconto = 0
    },
    onError: () => {
      processando.value = false
    }
  })
}

const formatMoney = (val) => {
  return Number(val || 0).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}
</script>
