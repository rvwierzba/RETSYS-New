<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-6xl mx-auto">
      
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <div>
          <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight flex items-center gap-2">
            <span class="w-2.5 h-2.5 rounded-full bg-teal-500 animate-pulse"></span>
            Catálogo e Matriz de Lentes
          </h1>
          <p class="text-xs text-slate-500 mt-1">
            Configure os preços cheios de venda de lentes por variação de laboratório, tipo, índice de refração e rótulo descritivo de tratamento.
          </p>
        </div>

        <div class="flex items-center gap-1.5 bg-slate-100 p-1.5 rounded-xl border border-slate-200/60 self-start md:self-auto">
          <button 
            @click="abaAtiva = 'precos'" 
            :class="[abaAtiva === 'precos' ? 'bg-white text-slate-950 shadow-sm font-black' : 'text-slate-500 hover:text-slate-800 font-medium']"
            class="px-4 py-2 rounded-lg text-xs transition"
          >
            Matriz de Preços
          </button>
          <button 
            v-if="isAdmin"
            @click="abaAtiva = 'importar'" 
            :class="[abaAtiva === 'importar' ? 'bg-white text-slate-950 shadow-sm font-black' : 'text-slate-500 hover:text-slate-800 font-medium']"
            class="px-4 py-2 rounded-lg text-xs transition flex items-center gap-1"
          >
            <span>Importador IA</span>
            <span class="bg-indigo-500/20 text-indigo-700 px-1.5 py-0.5 rounded text-[8px] font-bold uppercase">Ollama</span>
          </button>
        </div>
      </div>

      <div v-if="abaAtiva === 'precos'" class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        <div class="lg:col-span-2 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
          <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono">Tabela de Preços Ativa</h3>
            <input 
              v-model="filtroBusca"
              type="text" 
              placeholder="Filtrar por laboratório, tipo ou tratamento..." 
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
                  <th class="pb-3 text-center" v-if="isAdmin">Preço Custo</th>
                  <th class="pb-3 text-right">Preço Venda</th>
                  <th class="pb-3 text-center" v-if="isAdmin">Ações</th>
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
                  <td class="py-3 text-center font-mono text-slate-500" v-if="isAdmin">R$ {{ formatMoeda(preco.precoCusto) }}</td>
                  <td class="py-3 text-right font-black text-teal-600 font-mono text-sm">R$ {{ formatMoeda(preco.precoVenda) }}</td>
                  <td class="py-3 text-center" v-if="isAdmin">
                    <button @click="removerPreco(preco.id)" class="text-red-500 hover:text-red-700 font-bold px-2 py-1 text-[10px] transition font-mono">
                      Remover
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4 h-fit">
          <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono">Novo Preço Matriz</h3>
          
          <div v-if="!isAdmin" class="p-4 bg-slate-50 text-slate-500 rounded-xl text-xs text-center border">
            🔐 Apenas utilizadores administradores ou gerentes podem parametrizar novos preços de lentes.
          </div>

          <form v-else @submit.prevent="cadastrarPreco" class="space-y-4 text-xs">
            <div>
              <label class="block font-bold text-slate-400 uppercase mb-1">Lente Base *</label>
              <select v-model="formPreco.LenteId" class="w-full rounded-xl border-slate-200" required>
                <option value="">Selecione o Bloco de Lente</option>
                <option v-for="l in LentesMapeadas" :key="l.id" :value="l.id">
                  [{{ l.laboratorio }}] {{ l.tipo }} {{ l.surfacada ? '(SURFAÇADA)' : '' }}
                </option>
              </select>
            </div>

            <div>
              <label class="block font-bold text-slate-400 uppercase mb-1">Tipo de Variação *</label>
              <select v-model="formPreco.Tipo" class="w-full rounded-xl border-slate-200" required>
                <option value="MONOFOCAL">Monofocal</option>
                <option value="BIFOCAL">Bifocal</option>
                <option value="PROGRESSIVA">Progressiva</option>
                <option value="OCUPACIONAL">Ocupacional</option>
              </select>
            </div>

            <div class="grid grid-cols-1 gap-3">
              <div>
                <label class="block font-bold text-slate-400 uppercase mb-1">Índice Refração *</label>
                <input v-model.number="formPreco.IndiceRefracao" type="number" step="0.01" placeholder="Ex: 1.56" class="w-full rounded-xl border-slate-200 font-mono" required />
              </div>
              
              <div>
                <label class="block font-bold text-teal-800 uppercase mb-1">Tratamento Descritivo *</label>
                <input 
                  v-model="formPreco.Tratamento" 
                  type="text" 
                  list="tratamentos-sugeridos" 
                  placeholder="Ex: Antirreflexo Premium, Fotossensível" 
                  class="w-full rounded-xl border-slate-200 font-medium" 
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
                <input v-model.number="formPreco.PrecoCusto" type="number" step="0.01" class="w-full rounded-xl border-slate-200 font-mono" required />
              </div>
              <div>
                <label class="block font-bold text-slate-400 uppercase mb-1">Preço Venda (R$) *</label>
                <input v-model.number="formPreco.PrecoVenda" type="number" step="0.01" class="w-full rounded-xl border-slate-200 font-mono" required />
              </div>
            </div>

            <button type="submit" class="w-full bg-slate-950 hover:bg-slate-800 text-white font-bold py-3 rounded-xl transition shadow-sm uppercase tracking-wider text-[10px]">
              Gravar na Matriz
            </button>
          </form>
        </div>
      </div>

      <div v-if="abaAtiva === 'importar' && isAdmin" class="bg-white p-6 rounded-2xl border border-slate-200 shadow-xl space-y-4 max-w-3xl mx-auto animate-fadeIn">
        <div>
          <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono flex items-center gap-1.5">
            <span class="w-2.5 h-2.5 rounded-full bg-indigo-600 animate-pulse"></span>
            Importador Inteligente de Catálogos (Moondream/Ollama)
          </h3>
          <p class="text-[11px] text-slate-400 mt-1">
            Copie os dados brutos de qualquer PDF, folha de custos ou conversa de WhatsApp com fornecedor de lentes e cole abaixo. Nossa IA normalizará os dados do catálogo automaticamente aplicando a margem parametrizada.
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
  IsAdmin: { type: Boolean, default: false }
})

const abaAtiva = ref('precos')
const filtroBusca = ref('')
const carregandoImportacao = ref(false)

const isAdmin = computed(() => props.IsAdmin ?? (page.props.auth?.usuarioPerfil || '').toLowerCase() === 'admin')

// Normalização de chaves de resposta da API (PascalCase / camelCase)
const listaPrecosNormalizada = computed(() => {
  const bruta = props.Precos ?? props.precos ?? []
  return bruta.map(p => ({
    id: p.Id ?? p.id,
    laboratorio: p.Lente?.Laboratorio ?? p.lente?.laboratorio ?? 'Genérico',
    blocoTipo: p.Lente?.Tipo ?? p.lente?.tipo ?? 'Lente Base',
    tipo: p.Tipo ?? p.tipo,
    indiceRefracao: p.IndiceRefracao ?? p.indiceRefracao,
    tratamento: p.Tratamento ?? p.tratamento, // String direta da linha (Seção 6)
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

// Filtro reativo em tempo de digitação
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

// Formulário reconfigurado (Seção 6 - Tratamento passou a ser string)
const formPreco = useForm({
  LenteId: '',
  Tipo: 'MONOFOCAL',
  IndiceRefracao: null,
  Tratamento: '', 
  PrecoCusto: 0,
  PrecoVenda: 0
})

const formImportacao = ref({
  Laboratorio: '',
  TextoBruto: ''
})

const formatMoeda = (valor) => {
  if (valor === undefined || valor === null) return '0,00'
  return Number(valor).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const cadastrarPreco = () => {
  formPreco.post('/lentes/precos', {
    preserveScroll: true,
    onSuccess: () => {
      formPreco.reset()
      alert('Preço inserido com sucesso na matriz ativa!')
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