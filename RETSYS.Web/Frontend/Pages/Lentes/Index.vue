<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-6xl mx-auto">
      
      <!-- Cabeçalho Principal da Tabela de Preço da Ótica -->
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <div>
          <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight flex items-center gap-2">
            <span class="w-2.5 h-2.5 rounded-full bg-teal-500 animate-pulse"></span>
            Tabela de Preço da Ótica
          </h1>
          <p class="text-xs text-slate-500 mt-1">
            Gerencie o catálogo oficial de lentes da ótica, valores de custo, venda, tratamentos e refrações.
          </p>
        </div>

        <!-- Seleção de Abas Operacionais -->
        <div class="flex items-center gap-1.5 bg-slate-100 p-1.5 rounded-xl border border-slate-200/60 self-start md:self-auto">
          <button 
            @click="abaAtiva = 'precos'" 
            :class="[abaAtiva === 'precos' ? 'bg-white text-slate-950 shadow-sm font-black' : 'text-slate-500 hover:text-slate-800 font-medium']"
            class="px-4 py-2 rounded-lg text-xs transition"
          >
            Tabela de Preços
          </button>
          <button 
            @click="abaAtiva = 'catalogo'" 
            :class="[abaAtiva === 'catalogo' ? 'bg-white text-slate-950 shadow-sm font-black' : 'text-slate-500 hover:text-slate-800 font-medium']"
            class="px-4 py-2 rounded-lg text-xs transition"
          >
            Lentes Cadastradas
          </button>
          <button 
            @click="abaAtiva = 'importar'" 
            :class="[abaAtiva === 'importar' ? 'bg-white text-slate-950 shadow-sm font-black' : 'text-slate-500 hover:text-slate-800 font-medium']"
            class="px-4 py-2 rounded-lg text-xs transition flex items-center gap-1"
          >
            <span>Importador IA</span>
            <span class="bg-indigo-500/20 text-indigo-700 px-1.5 py-0.5 rounded text-[8px] font-bold uppercase">Ollama</span>
          </button>
        </div>
      </div>

      <!-- =========================================================================
           ABA 1: TABELA DE PREÇOS DE VENDAS
           ========================================================================= -->
      <div v-if="abaAtiva === 'precos'" class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        <!-- Listagem e Filtro de Preços Configurados (Duas Colunas) -->
        <div class="lg:col-span-2 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
          <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono">Tabela de Preços Ativa</h3>
            <input 
              v-model="filtroBusca"
              type="text" 
              placeholder="Filtrar por fabricante, bloco ou tratamento..." 
              class="rounded-xl border-slate-200 text-xs focus:border-teal-500 focus:ring-teal-500 max-w-xs placeholder:text-slate-400"
            />
          </div>

          <div v-if="precosFiltrados.length === 0" class="text-center py-12 border-2 border-dashed border-slate-100 rounded-xl text-slate-400 text-xs">
            Nenhuma combinação de preço localizada para os filtros inseridos.
          </div>

          <div v-else class="overflow-x-auto">
            <table class="w-full text-left text-xs border-collapse">
              <thead>
                <tr class="border-b border-slate-100 text-slate-400 font-bold uppercase tracking-wider">
                  <th class="pb-3">Laboratório / Bloco</th>
                  <th class="pb-3">Tipo / Tratamento</th>
                  <th class="pb-3 text-center">Índice</th>
                  <th class="pb-3 text-center">Preço Custo</th>
                  <th class="pb-3 text-right">Preço Venda</th>
                  <th class="pb-3 text-center">Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="preco in precosFiltrados" :key="preco.id" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
                  <td class="py-3">
                    <p class="font-bold text-slate-800">{{ preco.laboratorio }}</p>
                    <p class="text-[10px] text-slate-400 font-mono">{{ preco.blocoTipo }}</p>
                  </td>
                  <td class="py-3 font-semibold text-slate-600">
                    <span class="px-2 py-0.5 rounded bg-slate-100 border text-[10px] uppercase font-sans">{{ preco.tipo }}</span>
                    <p class="text-[10px] text-teal-600 font-bold mt-1" v-if="preco.tratamento">
                      ✨ {{ preco.tratamento }}
                    </p>
                  </td>
                  <td class="py-3 text-center font-mono font-bold text-slate-700">{{ preco.indiceRefracao }}</td>
                  <td class="py-3 text-center font-mono text-slate-500">R$ {{ formatMoeda(preco.precoCusto) }}</td>
                  <td class="py-3 text-right font-black text-teal-600 font-mono text-sm">R$ {{ formatMoeda(preco.precoVenda) }}</td>
                  <td class="py-3 text-center flex items-center justify-center gap-1.5">
                    <button @click="abrirModalEdicaoPreco(preco)" class="text-slate-700 hover:text-slate-900 bg-slate-100 hover:bg-slate-200 font-bold px-2 py-1 text-[10px] rounded transition font-mono">
                      Editar
                    </button>
                    <button @click="removerPreco(preco.id)" class="text-red-500 hover:text-red-700 font-bold px-2 py-1 text-[10px] transition font-mono">
                      Remover
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Painel Lateral Direto: Cadastros Operacionais Unificados (Uma Coluna) -->
        <div class="space-y-6 h-fit">
          
          <!-- FORMULÁRIO RÁPIDO: NOVO PREÇO NA TABELA -->
          <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
            <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono text-teal-600">＋ Novo Preço na Tabela</h3>
            
            <form @submit.prevent="cadastrarPreco" class="space-y-4 text-xs">
              <div>
                <label class="block font-bold text-slate-400 uppercase mb-1">Escolher Lente ou Criar Nova *</label>
                <select v-model="formPreco.LenteId" class="w-full rounded-xl border-slate-200 bg-slate-50/50" required>
                  <option value="NOVA">➕ [Nova Lente / Fabricante]</option>
                  <option v-for="l in LentesMapeadas" :key="l.id" :value="l.id">
                    [{{ l.laboratorio }}] {{ l.tipo }} {{ l.surfacada ? '(SURFAÇADA)' : '' }}
                  </option>
                </select>
              </div>

              <!-- Campos de Nova Lente se selecionado NOVA -->
              <div v-if="formPreco.LenteId === 'NOVA'" class="space-y-3 p-3 bg-teal-50/60 border border-teal-100 rounded-xl">
                <div>
                  <label class="block font-bold text-teal-900 uppercase mb-1">Laboratório / Fabricante *</label>
                  <input v-model="formPreco.Laboratorio" type="text" placeholder="Ex: Essilor, Hoya, Zeiss" class="w-full rounded-xl border-slate-200 bg-white" required />
                </div>
                <div>
                  <label class="block font-bold text-teal-900 uppercase mb-1">Nome do Bloco / Modelo *</label>
                  <input v-model="formPreco.NomeBloco" type="text" placeholder="Ex: Varilux Comfort, Orma" class="w-full rounded-xl border-slate-200 bg-white" required />
                </div>
                <div class="flex items-center justify-between">
                  <span class="font-bold text-teal-900 text-xs">Lente Surfaçada?</span>
                  <input type="checkbox" v-model="formPreco.Surfacada" class="rounded border-slate-300 text-teal-600 focus:ring-teal-500 h-4 w-4" />
                </div>
              </div>

              <div>
                <label class="block font-bold text-slate-400 uppercase mb-1">Tipo de Variação *</label>
                <select v-model="formPreco.Tipo" class="w-full rounded-xl border-slate-200 bg-slate-50/50" required>
                  <option value="MONOFOCAL">Monofocal</option>
                  <option value="BIFOCAL">Bifocal</option>
                  <option value="PROGRESSIVA">Progressiva</option>
                  <option value="OCUPACIONAL">Ocupacional</option>
                </select>
              </div>

              <div class="grid grid-cols-1 gap-3">
                <div>
                  <label class="block font-bold text-slate-400 uppercase mb-1">Índice Refração *</label>
                  <input v-model.number="formPreco.IndiceRefracao" type="number" step="0.01" placeholder="Ex: 1.56" class="w-full rounded-xl border-slate-200 font-mono bg-slate-50/50" required />
                </div>
                
                <div>
                  <label class="block font-bold text-teal-800 uppercase mb-1">Tratamento Descritivo *</label>
                  <input 
                    v-model="formPreco.Tratamento" 
                    type="text" 
                    list="tratamentos-sugeridos" 
                    placeholder="Ex: Antirreflexo Premium, BlueCut" 
                    class="w-full rounded-xl border-slate-200 font-medium bg-slate-50/50" 
                    required 
                  />
                  <datalist id="tratamentos-sugeridos">
                    <option v-for="t in props.TratamentosSugeridos" :key="t" :value="t" />
                  </datalist>
                </div>
              </div>

              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block font-bold text-slate-400 uppercase mb-1">Preço Custo (R$) *</label>
                  <input v-model.number="formPreco.PrecoCusto" type="number" step="0.01" class="w-full rounded-xl border-slate-200 font-mono bg-slate-50/50" required />
                </div>
                <div>
                  <label class="block font-bold text-slate-400 uppercase mb-1">Preço Venda (R$) *</label>
                  <input v-model.number="formPreco.PrecoVenda" type="number" step="0.01" class="w-full rounded-xl border-slate-200 font-mono bg-slate-50/50" required />
                </div>
              </div>

              <button type="submit" :disabled="formPreco.processing" class="w-full bg-slate-950 hover:bg-slate-800 text-white font-bold py-3 rounded-xl transition shadow-sm uppercase tracking-wider text-[10px]">
                <span v-if="formPreco.processing">Gravando Preço...</span>
                <span v-else>Salvar na Tabela</span>
              </button>
            </form>
          </div>
        </div>
      </div>

      <!-- =========================================================================
           ABA 2: CATÁLOGO DE LENTES CADASTRADAS (CRUD BASE)
           ========================================================================= -->
      <div v-if="abaAtiva === 'catalogo'" class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
        <div class="flex items-center justify-between border-b border-slate-100 pb-4">
          <div>
            <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono">Lentes Base Cadastradas</h3>
            <p class="text-xs text-slate-400 mt-0.5">Catálogo de blocos e laboratórios cadastrados para a sua ótica.</p>
          </div>
        </div>

        <div v-if="LentesMapeadas.length === 0" class="text-center py-12 border-2 border-dashed border-slate-100 rounded-xl text-slate-400 text-xs">
          Nenhuma lente base cadastrada no catálogo.
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-left text-xs border-collapse">
            <thead>
              <tr class="border-b border-slate-100 text-slate-400 font-bold uppercase tracking-wider">
                <th class="pb-3">Laboratório / Fornecedor</th>
                <th class="pb-3">Bloco / Design</th>
                <th class="pb-3 text-center">Tipo de Receita</th>
                <th class="pb-3 text-center">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="lente in LentesMapeadas" :key="lente.id" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
                <td class="py-3 font-bold text-slate-800">{{ lente.laboratorio }}</td>
                <td class="py-3 font-medium text-slate-700">{{ lente.tipo }}</td>
                <td class="py-3 text-center">
                  <span :class="lente.surfacada ? 'bg-amber-50 text-amber-700 border-amber-200' : 'bg-slate-100 text-slate-600 border-slate-200'" class="px-2 py-0.5 rounded text-[10px] font-bold border">
                    {{ lente.surfacada ? 'SURFAÇADA' : 'PADRÃO / PRONTA' }}
                  </span>
                </td>
                <td class="py-3 text-center flex items-center justify-center gap-1.5">
                  <button @click="abrirModalEdicaoLente(lente)" class="text-slate-700 hover:text-slate-900 bg-slate-100 hover:bg-slate-200 font-bold px-2 py-1 text-[10px] rounded transition font-mono">
                    Editar
                  </button>
                  <button @click="removerLenteBase(lente.id)" class="text-red-500 hover:text-red-700 font-bold px-2 py-1 text-[10px] transition font-mono">
                    Excluir
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- =========================================================================
           ABA 3: IMPORTADOR INTELIGENTE POR IA (OLLAMA LOCAL)
           ========================================================================= -->
      <div v-if="abaAtiva === 'importar'" class="bg-white p-6 rounded-2xl border border-slate-200 shadow-xl space-y-4 max-w-3xl mx-auto animate-fadeIn">
        <div>
          <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono flex items-center gap-1.5">
            <span class="w-2.5 h-2.5 rounded-full bg-indigo-600 animate-pulse"></span>
            Importador Inteligente de Catálogos (Moondream/Ollama)
          </h3>
          <p class="text-[11px] text-slate-400 mt-1">
            Copie os dados brutos de qualquer PDF ou tabela do fornecedor de lentes e cole abaixo para que a IA normalize e importe automaticamente.
          </p>
        </div>

        <div class="space-y-4 mt-6 text-xs">
          <div>
            <label class="block font-bold text-slate-400 uppercase mb-1.5">Laboratório / Fornecedor Alvo *</label>
            <input v-model="formImportacao.Laboratorio" type="text" placeholder="Ex: Lab de Surfaçagem Regional Ltda" class="w-full rounded-xl border-slate-200 text-xs" />
          </div>

          <div>
            <label class="block font-bold text-slate-400 uppercase mb-1.5">Dados Brutos Colados (Tabela do Fornecedor) *</label>
            <textarea v-model="formImportacao.TextoBruto" rows="10" placeholder="Cole as linhas da planilha de custos aqui (Ex: Monofocal 1.56 AR - Custo 45.00)..." class="w-full rounded-xl border-slate-200 text-[10px] font-mono"></textarea>
          </div>

          <div class="flex justify-end pt-2">
            <button 
              @click="processarTabelaPorIA" 
              :disabled="carregandoImportacao || !formImportacao.Laboratorio || !formImportacao.TextoBruto" 
              class="bg-slate-950 hover:bg-slate-800 disabled:bg-slate-100 disabled:text-slate-400 text-white font-bold py-3 px-6 rounded-xl text-[10px] uppercase tracking-wider transition shadow-md flex items-center gap-2"
            >
              <span v-if="carregandoImportacao" class="animate-pulse">A IA está decodificando e salvando...</span>
              <span v-else>Iniciar Processamento Digital</span>
            </button>
          </div>
        </div>
      </div>

      <!-- MODAL DE EDIÇÃO DE PREÇO DE LENTE -->
      <div v-if="modalEdicaoPrecoAberta" class="fixed inset-0 bg-slate-950/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
        <div class="bg-white rounded-3xl border border-slate-200 shadow-2xl max-w-md w-full p-6 space-y-4">
          <div class="flex items-center justify-between border-b border-slate-100 pb-3">
            <h3 class="text-base font-bold text-slate-950">Editar Preço da Matriz</h3>
            <button @click="modalEdicaoPrecoAberta = false" class="text-slate-400 hover:text-slate-800 font-bold">✕</button>
          </div>

          <form @submit.prevent="salvarEdicaoPreco" class="space-y-4 text-xs">
            <div>
              <label class="block font-bold text-slate-400 uppercase mb-1">Lente Base *</label>
              <select v-model="formEdicaoPreco.LenteId" class="w-full rounded-xl border-slate-200 bg-slate-50/50 text-xs" required>
                <option v-for="l in LentesMapeadas" :key="l.id" :value="l.id">
                  [{{ l.laboratorio }}] {{ l.tipo }}
                </option>
              </select>
            </div>

            <div>
              <label class="block font-bold text-slate-400 uppercase mb-1">Tipo de Variação *</label>
              <select v-model="formEdicaoPreco.Tipo" class="w-full rounded-xl border-slate-200 bg-slate-50/50 text-xs" required>
                <option value="MONOFOCAL">Monofocal</option>
                <option value="BIFOCAL">Bifocal</option>
                <option value="PROGRESSIVA">Progressiva</option>
                <option value="OCUPACIONAL">Ocupacional</option>
              </select>
            </div>

            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block font-bold text-slate-400 uppercase mb-1">Índice Refração *</label>
                <input v-model.number="formEdicaoPreco.IndiceRefracao" type="number" step="0.01" class="w-full rounded-xl border-slate-200 font-mono text-xs" required />
              </div>
              <div>
                <label class="block font-bold text-slate-400 uppercase mb-1">Tratamento *</label>
                <input v-model="formEdicaoPreco.Tratamento" type="text" class="w-full rounded-xl border-slate-200 text-xs" required />
              </div>
            </div>

            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block font-bold text-slate-400 uppercase mb-1">Preço Custo (R$) *</label>
                <input v-model.number="formEdicaoPreco.PrecoCusto" type="number" step="0.01" class="w-full rounded-xl border-slate-200 font-mono text-xs" required />
              </div>
              <div>
                <label class="block font-bold text-slate-400 uppercase mb-1">Preço Venda (R$) *</label>
                <input v-model.number="formEdicaoPreco.PrecoVenda" type="number" step="0.01" class="w-full rounded-xl border-slate-200 font-mono text-xs" required />
              </div>
            </div>

            <div class="flex justify-end gap-2 pt-3 border-t border-slate-100">
              <button type="button" @click="modalEdicaoPrecoAberta = false" class="px-4 py-2 rounded-xl text-slate-600 font-bold hover:bg-slate-100">Cancelar</button>
              <button type="submit" class="px-5 py-2 rounded-xl bg-teal-600 hover:bg-teal-700 text-white font-bold transition">Salvar Preço</button>
            </div>
          </form>
        </div>
      </div>

      <!-- MODAL DE EDIÇÃO DE LENTE BASE -->
      <div v-if="modalEdicaoLenteAberta" class="fixed inset-0 bg-slate-950/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
        <div class="bg-white rounded-3xl border border-slate-200 shadow-2xl max-w-md w-full p-6 space-y-4">
          <div class="flex items-center justify-between border-b border-slate-100 pb-3">
            <h3 class="text-base font-bold text-slate-950">Editar Lente Base</h3>
            <button @click="modalEdicaoLenteAberta = false" class="text-slate-400 hover:text-slate-800 font-bold">✕</button>
          </div>

          <form @submit.prevent="salvarEdicaoLente" class="space-y-4 text-xs">
            <div>
              <label class="block font-bold text-slate-400 uppercase mb-1">Laboratório / Fornecedor *</label>
              <input v-model="formEdicaoLente.Laboratorio" type="text" class="w-full rounded-xl border-slate-200 text-xs" required />
            </div>

            <div>
              <label class="block font-bold text-slate-400 uppercase mb-1">Nome do Bloco / Design *</label>
              <input v-model="formEdicaoLente.Tipo" type="text" class="w-full rounded-xl border-slate-200 text-xs" required />
            </div>

            <div class="flex items-center justify-between bg-slate-50 p-3 rounded-xl border border-slate-200">
              <div>
                <span class="block font-bold text-slate-700 text-xs">Lente Surfaçada?</span>
                <span class="text-[10px] text-slate-400">Marque se for bloco de receita sob medida</span>
              </div>
              <input type="checkbox" v-model="formEdicaoLente.Surfacada" class="rounded border-slate-300 text-teal-600 focus:ring-teal-500 h-4 w-4" />
            </div>

            <div class="flex justify-end gap-2 pt-3 border-t border-slate-100">
              <button type="button" @click="modalEdicaoLenteAberta = false" class="px-4 py-2 rounded-xl text-slate-600 font-bold hover:bg-slate-100">Cancelar</button>
              <button type="submit" class="px-5 py-2 rounded-xl bg-teal-600 hover:bg-teal-700 text-white font-bold transition">Salvar Lente Base</button>
            </div>
          </form>
        </div>
      </div>

    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { ref, computed } from 'vue'
import { router, useForm, usePage } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const page = usePage()

const props = defineProps({
  Lentes: { type: Array, default: () => [] },
  precos: { type: Array, default: () => [] },
  Precos: { type: Array, default: () => [] },
  TratamentosSugeridos: { type: Array, default: () => ['Antirreflexo Comum', 'Antirreflexo Premium', 'Filtro Azul (BlueCut)', 'Fotossensível (Transitions)', 'Resina Incolor'] },
  IsAdmin: { type: Boolean, default: true }
})

const abaAtiva = ref('precos')
const filtroBusca = ref('')
const carregandoImportacao = ref(false)

const modalEdicaoPrecoAberta = ref(false)
const modalEdicaoLenteAberta = ref(false)

// Normalização defensiva do payload JSON vindo do back-end
const listaPrecosNormalizada = computed(() => {
  const bruta = props.Precos ?? props.precos ?? []
  return bruta.map(p => ({
    id: p.Id ?? p.id,
    lenteId: p.LenteId ?? p.lenteId,
    laboratorio: p.Lente?.Laboratorio ?? p.lente?.laboratorio ?? 'Genérico',
    blocoTipo: p.Lente?.Tipo ?? p.lente?.tipo ?? 'Lente Base',
    tipo: p.Tipo ?? p.tipo,
    indiceRefracao: p.IndiceRefracao ?? p.indiceRefracao,
    tratamento: p.Tratamento ?? p.tratamento,
    precoCusto: p.PrecoCusto ?? p.precoCusto ?? 0,
    precoVenda: p.PrecoVenda ?? p.precoVenda ?? 0
  }))
})

const LentesMapeadas = computed(() => {
  return (props.Lentes ?? []).map(l => ({
    id: l.Id ?? l.id,
    laboratorio: l.Laboratorio ?? l.laboratorio,
    tipo: l.Tipo ?? l.tipo,
    surfacada: l.Surfacada ?? l.surfacada ?? false
  }))
})

// Filtro em tempo real digitado pelo operador
const precosFiltrados = computed(() => {
  const t = filtroBusca.value.toLowerCase().trim()
  if (!t) return listaPrecosNormalizada.value

  return listaPrecosNormalizada.value.filter(p => 
    p.laboratorio.toLowerCase().includes(t) ||
    p.blocoTipo.toLowerCase().includes(t) ||
    p.tipo.toLowerCase().includes(t) ||
    (p.tratamento || '').toLowerCase().includes(t)
  )
})

// FORMULÁRIO: Atribuição de preço na Matriz (Com suporte a Lente Base existente ou Nova)
const formPreco = useForm({
  LenteId: 'NOVA',
  Laboratorio: '',
  NomeBloco: '',
  Surfacada: false,
  Tipo: 'MONOFOCAL',
  IndiceRefracao: 1.56,
  Tratamento: '', 
  PrecoCusto: 0,
  PrecoVenda: 0
})

const formEdicaoPreco = ref({
  id: null,
  LenteId: '',
  Tipo: 'MONOFOCAL',
  IndiceRefracao: 1.56,
  Tratamento: '',
  PrecoCusto: 0,
  PrecoVenda: 0
})

const formEdicaoLente = ref({
  id: null,
  Laboratorio: '',
  Tipo: '',
  Surfacada: false
})

const formImportacao = ref({
  Laboratorio: '',
  TextoBruto: ''
})

const formatMoeda = (valor) => {
  if (valor === undefined || valor === null) return '0,00'
  return Number(valor).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const abrirModalEdicaoLente = (lente) => {
  formEdicaoLente.value = {
    id: lente.id,
    Laboratorio: lente.laboratorio,
    Tipo: lente.tipo,
    Surfacada: lente.surfacada
  }
  modalEdicaoLenteAberta.value = true
}

const salvarEdicaoLente = () => {
  router.post(`/lentes/editar/${formEdicaoLente.value.id}`, {
    laboratorio: formEdicaoLente.value.Laboratorio,
    tipo: formEdicaoLente.value.Tipo,
    surfacada: formEdicaoLente.value.Surfacada
  }, {
    preserveScroll: true,
    onSuccess: () => {
      modalEdicaoLenteAberta.value = false
      alert('Lente base atualizada com sucesso!')
    }
  })
}

const removerLenteBase = (id) => {
  if (confirm('Tem certeza que deseja excluir esta lente base do catálogo?')) {
    router.post(`/lentes/excluir/${id}`, {}, { preserveScroll: true })
  }
}

const cadastrarPreco = () => {
  const isNovaLente = formPreco.LenteId === 'NOVA'
  
  router.post('/lentes/precos', {
    lenteId: isNovaLente ? null : formPreco.LenteId,
    laboratorio: isNovaLente ? formPreco.Laboratorio : '',
    nomeBloco: isNovaLente ? formPreco.NomeBloco : '',
    surfacada: isNovaLente ? formPreco.Surfacada : false,
    tipo: formPreco.Tipo,
    indiceRefracao: parseFloat(formPreco.IndiceRefracao) || 0,
    tratamento: formPreco.Tratamento || "",
    precoCusto: parseFloat(formPreco.PrecoCusto) || 0,
    precoVenda: parseFloat(formPreco.PrecoVenda) || 0
  }, {
    preserveScroll: true,
    onSuccess: () => {
      formPreco.reset()
      formPreco.LenteId = 'NOVA'
      alert('Preço inserido com sucesso na tabela!')
    }
  })
}

const abrirModalEdicaoPreco = (preco) => {
  formEdicaoPreco.value = {
    id: preco.id,
    LenteId: preco.lenteId,
    Tipo: preco.tipo,
    IndiceRefracao: preco.indiceRefracao,
    Tratamento: preco.tratamento || '',
    PrecoCusto: preco.precoCusto,
    PrecoVenda: preco.precoVenda
  }
  modalEdicaoPrecoAberta.value = true
}

const salvarEdicaoPreco = () => {
  router.post(`/lentes/precos/editar/${formEdicaoPreco.value.id}`, {
    lenteId: formEdicaoPreco.value.LenteId,
    tipo: formEdicaoPreco.value.Tipo,
    indiceRefracao: parseFloat(formEdicaoPreco.value.IndiceRefracao) || 0,
    tratamento: formEdicaoPreco.value.Tratamento || "",
    precoCusto: parseFloat(formEdicaoPreco.value.PrecoCusto) || 0,
    precoVenda: parseFloat(formEdicaoPreco.value.PrecoVenda) || 0
  }, {
    preserveScroll: true,
    onSuccess: () => {
      modalEdicaoPrecoAberta.value = false
      alert('Preço da matriz atualizado com sucesso!')
    }
  })
}

const removerPreco = (id) => {
  if (confirm('Tem certeza de que deseja remover este preço da matriz?')) {
    router.delete(`/lentes/precos/${id}`, { preserveScroll: true })
  }
}

const processarTabelaPorIA = async () => {
  carregandoImportacao.value = true
  try {
    const resposta = await fetch('/api/admin/lentes/importar-ia', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        laboratorio: formImportacao.value.Laboratorio,
        textoBruto: formImportacao.value.TextoBruto
      })
    })

    const resultado = await resposta.json()
    if (resposta.ok) {
      alert(resultado.mensagem)
      formImportacao.value.TextoBruto = ''
      router.reload() 
    } else {
      alert('Erro IA: ' + (resultado.erro || 'Falha desconhecida.'))
    }
  } catch (err) {
    console.error(err)
    alert('Erro de ligação ao Ollama local. Garanta que o serviço está ativo.')
  } finally {
    carregandoImportacao.value = false
  }
}
</script>