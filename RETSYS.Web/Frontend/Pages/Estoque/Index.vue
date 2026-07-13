<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6">
      
      <!-- Cabeçalho com Botão de Gerenciamento Totalmente Liberado -->
      <div class="max-w-6xl mx-auto flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <div>
          <h1 class="text-2xl font-black text-slate-950">Controle de Estoque</h1>
          <p class="text-sm text-slate-500 mt-1">Gerencie o inventário de armações e óculos de sol da loja.</p>
        </div>
        <div class="flex items-center">
          <!-- Modificado de Link para Botão de Acionamento do Modal Express -->
          <button 
            type="button"
            @click="exibirModalMarcas = true" 
            class="bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold py-2.5 px-5 rounded-xl text-xs transition border border-slate-200 uppercase tracking-wider shadow-sm"
          >
            ⚙️ Gerenciar Marcas
          </button>
        </div>
      </div>

      <div class="max-w-6xl mx-auto grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        <!-- Formulário Operacional de Cadastro -->
        <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm h-fit space-y-4">
          <h3 class="text-base font-bold text-slate-950">Adicionar Armação</h3>
          
          <form @submit.prevent="salvarArmacao" class="space-y-4">
            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Marca Fabricante *</label>
              <select v-model="form.MarcaId" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required>
                <option value="">Selecione a Marca</option>
                <option v-for="m in Marcas" :key="m.id || m.Id" :value="m.id || m.Id">{{ m.nome || m.Nome }}</option>
              </select>
            </div>

            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Modelo / Nome *</label>
              <input v-model="form.ModeloReferencia" type="text" placeholder="Ex: Aviator Clássico" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Código / SKU *</label>
                <input v-model="form.CodigoSku" type="text" placeholder="Ex: RB3025" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" required />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Cor *</label>
                <input v-model="form.Cor" type="text" placeholder="Ex: Dourado" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
              </div>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Tamanho</label>
                <input v-model="form.Tamanho" type="text" placeholder="Ex: 55" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Material</label>
                <input v-model="form.Material" type="text" placeholder="Ex: Metal" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Qtd Estoque</label>
                <input v-model.number="form.QuantidadeEstoque" type="number" min="0" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Preço de Venda (R$) *</label>
                <input v-model.number="form.PrecoVenda" type="number" step="0.01" placeholder="0,00" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" required />
              </div>
            </div>

            <button 
              type="submit" 
              :disabled="form.processing"
              class="w-full bg-teal-600 hover:bg-teal-700 disabled:bg-slate-200 disabled:text-slate-400 text-white font-bold py-3 rounded-xl text-xs transition shadow-sm uppercase tracking-wider flex items-center justify-center min-h-[42px]"
            >
              <span v-if="form.processing">Registrando...</span>
              <span v-else>Registrar no Estoque</span>
            </button>
          </form>
        </div>

        <!-- Tabela do Inventário Físico -->
        <div class="lg:col-span-2 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
          <h3 class="text-base font-bold text-slate-950 mb-4">Peças em Vitrine</h3>

          <div v-if="!Estoque || Estoque.length === 0" class="text-center py-12 border-2 border-dashed border-slate-100 rounded-xl text-slate-400 text-sm">
            Nenhuma armação registrada no inventário da ótica.
          </div>

          <div v-else class="overflow-x-auto">
            <table class="w-full text-left text-sm border-collapse">
              <thead>
                <tr class="border-b border-slate-100 text-slate-400 text-xs font-bold uppercase tracking-wider">
                  <th class="pb-3">Armação / Especificações</th>
                  <th class="pb-3 text-center">Código</th>
                  <th class="pb-3 text-center">Estoque</th>
                  <th class="pb-3 text-right">Preço Final</th>
                  <th v-if="eAdmin" class="pb-3 text-center">Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in Estoque" :key="item.id || item.Id" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
                  <td class="py-4">
                    <p class="font-bold text-slate-800">{{ item.modeloReferencia || item.ModeloReferencia }}</p>
                    <p class="text-xs text-teal-600 font-medium">
                      {{ item.marcaNome || item.MarcaNome }} 
                      <span class="text-slate-400">• {{ item.material || item.Material || 'N/A' }}</span>
                    </p>
                    <p class="text-[11px] text-slate-400">
                      Cor: {{ item.cor || item.Cor }} | Tam: {{ item.tamanho || item.Tamanho || 'padrão' }}
                    </p>
                  </td>
                  <td class="py-4 text-center font-mono text-xs text-slate-500">
                    {{ item.codigo || item.Codigo }}
                  </td>
                  <td class="py-4 text-center">
                    <span 
                      :class="(item.quantidadeEstoque || item.QuantidadeEstoque) <= 2 ? 'bg-red-50 text-red-700 border-red-100' : 'bg-slate-100 text-slate-700 border-slate-200'" 
                      class="px-2.5 py-0.5 rounded-full text-xs font-bold border"
                    >
                      {{ item.quantidadeEstoque ?? item.QuantidadeEstoque }} un
                    </span>
                  </td>
                  <td class="py-4 text-right font-black text-slate-950 font-mono">
                    R$ {{ formatarPreco(item.precoFinal ?? item.PrecoFinal) }}
                  </td>
                  
                  <td v-if="eAdmin" class="py-4 text-center">
                    <button 
                      @click="removerPecaEstoque(item.id || item.Id)"
                      class="text-[11px] font-bold text-red-600 bg-red-50 hover:bg-red-100 border border-red-200 px-2.5 py-1 rounded-lg transition active:scale-95"
                    >
                      Remover
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

      </div>
    </div>

    <!-- MODAL PREMIUM DE GERENCIAMENTO DE MARCAS (ABERTO A TODOS OS USUÁRIOS) -->
    <div v-if="exibirModalMarcas" class="fixed inset-0 bg-slate-950/60 backdrop-blur-sm flex items-center justify-center p-4 z-50">
      <div class="bg-white rounded-3xl border border-slate-200 shadow-2xl w-full max-w-md overflow-hidden animate-fadeIn">
        
        <!-- Header do Modal -->
        <div class="bg-slate-950 text-white p-5 flex items-center justify-between">
          <div>
            <h2 class="text-sm font-black uppercase tracking-wider font-mono">Gerenciador de Marcas</h2>
            <p class="text-[11px] text-slate-400">Adicione ou remova fabricantes do catálogo.</p>
          </div>
          <button @click="exibirModalMarcas = false" class="text-slate-400 hover:text-white text-xl font-bold select-none">&times;</button>
        </div>

        <!-- Corpo do Modal: Listagem das Marcas com Botão ✕ -->
        <div class="p-5 space-y-4">
          <div v-if="erroMarca" class="p-3 bg-red-50 border border-red-200 text-red-800 rounded-xl text-xs font-semibold">
            ⚠️ {{ erroMarca }}
          </div>

          <div class="space-y-1.5 max-h-52 overflow-y-auto pr-1">
            <div v-if="!Marcas || Marcas.length === 0" class="text-center py-6 text-xs text-slate-400 border border-dashed rounded-xl">
              Nenhuma marca cadastrada.
            </div>
            <div 
              v-else 
              v-for="m in Marcas" 
              :key="m.id || m.Id" 
              class="flex items-center justify-between p-2.5 bg-slate-50 hover:bg-slate-100/80 rounded-xl border border-slate-150 text-xs transition"
            >
              <span class="font-bold text-slate-800">{{ m.nome || m.Nome }}</span>
              <!-- Botão X de exclusão requisitado -->
              <button 
                type="button" 
                @click="excluirMarcaDoBanco(m.id || m.Id)" 
                class="text-red-500 hover:text-red-700 bg-red-50 hover:bg-red-100 p-1.5 rounded-lg transition active:scale-95 font-mono font-bold"
                title="Excluir Marca"
              >
                ✕
              </button>
            </div>
          </div>

          <!-- Rodapé do Modal: Formulário Rápido de Inserção -->
          <div class="pt-4 border-t border-slate-100 space-y-2">
            <label class="block text-[10px] font-bold uppercase text-slate-400 tracking-wider">Cadastrar Nova Marca</label>
            <div class="flex gap-2">
              <input 
                v-model="nomeNovaMarca" 
                type="text" 
                placeholder="Ex: Oakley" 
                @keydown.enter.prevent="enviarNovaMarca"
                class="w-full rounded-xl text-xs border-slate-200 bg-white focus:border-teal-500 focus:ring-teal-500" 
              />
              <button 
                type="button" 
                @click="enviarNovaMarca" 
                :disabled="enviandoMarca"
                class="bg-teal-600 hover:bg-teal-700 disabled:bg-slate-300 text-white text-xs font-bold px-4 py-2 rounded-xl uppercase transition whitespace-nowrap"
              >
                {{ enviandoMarca ? 'Gravando...' : 'Gravar' }}
              </button>
            </div>
          </div>
        </div>

      </div>
    </div>

  </AuthenticatedLayout>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useForm, usePage, router } from '@inertiajs/vue3'
import axios from 'axios'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const page = usePage()

const eAdmin = computed(() => {
  const perfil = page.props.auth?.usuarioPerfil || page.props.auth?.user?.perfil || ''
  return perfil.toLowerCase() === 'admin'
})

defineProps({
  Estoque: Array,
  Marcas: Array
})

// Estados reativos locais do Modal de Marcas
const exibirModalMarcas = ref(false)
const nomeNovaMarca = ref('')
const enviandoMarca = ref(false)
const erroMarca = ref(null)

const form = useForm({
  MarcaId: '', ModeloReferencia: '', CodigoSku: '', Cor: '', Tamanho: '', Material: '',
  QuantidadeEstoque: 1, PrecoVenda: null
})

// Inserção reativa via Axios para impedir quebras de tela do Inertia
const enviarNovaMarca = async () => {
  erroMarca.value = null
  if (!nomeNovaMarca.value.trim()) return
  
  enviandoMarca.value = true
  try {
    const formData = new FormData()
    formData.append('nome', nomeNovaMarca.value)
    await axios.post('/armacoes/marcas', formData)
    
    nomeNovaMarca.value = ''
    // Atualiza apenas o prop das Marcas mantendo o estado da tela intacto
    router.reload({ only: ['Marcas'] })
  } catch (err) {
    erroMarca.value = err.response?.data?.mensagem || 'Falha ao salvar marca.'
  } finally {
    enviandoMarca.value = false
  }
}

// Remoção reativa via Axios integrada ao Endpoint DELETE do back-end
const excluirMarcaDoBanco = async (id) => {
  erroMarca.value = null
  if (!id) return
  if (!confirm('Deseja realmente remover esta marca do sistema?')) return

  try {
    await axios.delete(`/armacoes/marcas/${id}`)
    // Atualiza dinamicamente o catálogo e limpa seleções inválidas órfãs
    if (form.MarcaId == id) form.MarcaId = ''
    router.reload({ only: ['Marcas', 'Estoque'] })
  } catch (err) {
    erroMarca.value = err.response?.data?.mensagem || 'Não foi possível excluir. Verifique se existem armações vinculadas.'
  }
}

const salvarArmacao = () => {
  form.post('/estoque', {
    preserveScroll: true,
    onSuccess: () => form.reset()
  })
}

const removerPecaEstoque = (id) => {
  if (!id) return
  if (confirm('Deseja remover esta armação permanentemente?')) {
    router.delete(`/estoque/${id}`, { preserveScroll: true })
  }
}

const formatarPreco = (valor) => {
  if (valor === undefined || valor === null) return '0,00'
  return Number(valor).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}
</script>