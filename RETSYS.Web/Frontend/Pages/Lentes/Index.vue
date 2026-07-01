<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-6xl mx-auto">
      
      <!-- Cabeçalho Principal da Gestão de Lentes -->
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <div>
          <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight flex items-center gap-2">
            <span class="w-2.5 h-2.5 rounded-full bg-teal-500 animate-pulse"></span>
            Catálogo e Matriz de Lentes
          </h1>
          <p class="text-xs text-slate-500 mt-1">
            Configure os preços de venda de lentes por tipo e índice de refração, controle adicionais de tratamentos e utilize o importador de IA.
          </p>
        </div>

        <!-- Seleção de Abas Operacionais -->
        <div class="flex items-center gap-1.5 bg-slate-100 p-1.5 rounded-xl border border-slate-200/60 self-start md:self-auto">
          <button 
            @click="abaAtiva = 'precos'" 
            :class="[abaAtiva === 'precos' ? 'bg-white text-slate-950 shadow-sm' : 'text-slate-500 hover:text-slate-800']"
            class="px-4 py-2 rounded-lg text-xs font-bold transition"
          >
            Matriz de Preços
          </button>
          <button 
            @click="abaAtiva = 'tratamentos'" 
            :class="[abaAtiva === 'tratamentos' ? 'bg-white text-slate-950 shadow-sm' : 'text-slate-500 hover:text-slate-800']"
            class="px-4 py-2 rounded-lg text-xs font-bold transition"
          >
            Tratamentos
          </button>
          <button 
            v-if="isAdmin"
            @click="abaAtiva = 'importar'" 
            :class="[abaAtiva === 'importar' ? 'bg-white text-slate-950 shadow-sm' : 'text-slate-500 hover:text-slate-800']"
            class="px-4 py-2 rounded-lg text-xs font-bold transition flex items-center gap-1"
          >
            <span>Importador IA</span>
            <span class="bg-teal-500/20 text-teal-700 px-1 py-0.5 rounded text-[8px] uppercase">Novo</span>
          </button>
        </div>
      </div>

      <!-- =========================================================================
           ABA 1: MATRIZ DE PREÇOS (TABELA DE PREÇOS)
           ========================================================================= -->
      <div v-if="abaAtiva === 'precos'" class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        <!-- Listagem e Filtro de Preços -->
        <div class="lg:col-span-2 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
          <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono">Preços Configurados</h3>
            <input 
              v-model="filtroBusca"
              type="text" 
              placeholder="Filtrar por laboratório ou tipo..." 
              class="rounded-xl border-slate-200 text-xs focus:border-teal-500 focus:ring-teal-500 max-w-xs"
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
                  <th class="pb-3">Tipo</th>
                  <th class="pb-3 text-center">Índice</th>
                  <th class="pb-3 text-center" v-if="isAdmin">Preço Custo</th>
                  <th class="pb-3 text-right">Preço Venda</th>
                  <th class="pb-3 text-center" v-if="isAdmin">Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="preco in precosFiltrados" :key="preco.Id" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
                  <td class="py-3">
                    <p class="font-bold text-slate-800">{{ preco.Lente?.Laboratorio || 'Genérico' }}</p>
                    <p class="text-[10px] text-slate-400 font-mono">{{ preco.Lente?.Tipo || 'Lente Base' }}</p>
                  </td>
                  <td class="py-3 font-semibold text-slate-600">
                    <span class="px-2 py-0.5 rounded bg-slate-100 border text-[10px]">{{ preco.Tipo }}</span>
                  </td>
                  <td class="py-3 text-center font-mono font-bold text-slate-700">{{ preco.IndiceRefracao }}</td>
                  <td class="py-3 text-center font-mono text-slate-500" v-if="isAdmin">R$ {{ formatMoeda(preco.PrecoCusto) }}</td>
                  <td class="py-3 text-right font-black text-teal-600 font-mono text-sm">R$ {{ formatMoeda(preco.PrecoVenda) }}</td>
                  <td class="py-3 text-center" v-if="isAdmin">
                    <button @click="removerPreco(preco.Id)" class="text-red-500 hover:text-red-700 font-bold px-2 py-1 text-[10px] transition">
                      Remover
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Painel Lateral: Adicionar novo preço na Matriz (Exclusivo Admin) -->
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
                <option v-for="l in Lentes" :key="l.Id" :value="l.Id">
                  [{{ l.Laboratorio }}] {{ l.Tipo }} {{ l.Surfacada ? '(SURFAÇADA)' : '' }}
                </option>
              </select>
            </div>

            <div>
              <label class="block font-bold text-slate-400 uppercase mb-1">Tipo de Lente *</label>
              <select v-model="formPreco.Tipo" class="w-full rounded-xl border-slate-200" required>
                <option value="MONOFOCAL">Monofocal</option>
                <option value="BIFOCAL">Bifocal</option>
                <option value="PROGRESSIVA">Progressiva</option>
                <option value="OCUPACIONAL">Ocupacional</option>
              </select>
            </div>

            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block font-bold text-slate-400 uppercase mb-1">Índice Refração *</label>
                <input v-model.number="formPreco.IndiceRefracao" type="number" step="0.01" placeholder="Ex: 1.56" class="w-full rounded-xl border-slate-200 font-mono" required />
              </div>
              <div>
                <label class="block font-bold text-slate-400 uppercase mb-1">Tratamento Opcional</label>
                <select v-model="formPreco.TratamentoId" class="w-full rounded-xl border-slate-200">
                  <option :value="null">Sem Tratamento</option>
                  <option v-for="t in Tratamentos" :key="t.Id" :value="t.Id">{{ t.Nome }}</option>
                </select>
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

      <!-- =========================================================================
           ABA 2: GESTÃO DE TRATAMENTOS (ANTIRREFLEXOS E ADICIONAIS)
           ========================================================================= -->
      <div v-if="abaAtiva === 'tratamentos'" class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        <!-- Listagem de Tratamentos -->
        <div class="lg:col-span-2 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
          <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono">Tratamentos Cadastrados</h3>
          
          <div v-if="Tratamentos.length === 0" class="text-center py-12 border-2 border-dashed border-slate-100 rounded-xl text-slate-400 text-xs">
            Nenhum tratamento antirreflexo ou fotossensível cadastrado.
          </div>

          <div v-else class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div v-for="t in Tratamentos" :key="t.Id" class="p-4 bg-slate-50 rounded-xl border border-slate-200 flex items-center justify-between">
              <div>
                <p class="font-bold text-slate-900 text-xs">{{ t.Nome }}</p>
                <p class="text-[10px] text-slate-400 mt-0.5 truncate max-w-[180px]">{{ t.Descricao || 'Sem descrição' }}</p>
              </div>
              <div class="text-right">
                <span class="font-black text-teal-600 font-mono text-xs">+ R$ {{ formatMoeda(t.AcrescimoValor) }}</span>
                <button v-if="isAdmin" @click="removerTratamento(t.Id)" class="block text-[9px] text-red-500 hover:text-red-700 font-bold ml-auto mt-1">
                  Excluir
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Painel Lateral: Adicionar novo Tratamento -->
        <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4 h-fit">
          <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider font-mono">Novo Tratamento</h3>

          <div v-if="!isAdmin" class="p-4 bg-slate-50 text-slate-500 rounded-xl text-xs text-center border">
            🔐 Apenas utilizadores administradores podem registar novos tratamentos e adicionais comerciais.
          </div>

          <form v-else @submit.prevent="cadastrarTratamento" class="space-y-4 text-xs">
            <div>
              <label class="block font-bold text-slate-400 uppercase mb-1">Nome do Tratamento *</label>
              <input v-model="formTratamento.Nome" type="text" placeholder="Ex: Antirreflexo Premium Crizal" class="w-full rounded-xl border-slate-200" required />
            </div>

            <div>
              <label class="block font-bold text-slate-400 uppercase mb-1">Acréscimo de Valor (R$) *</label>
              <input v-model.number="formTratamento.AcrescimoValor" type="number" step="0.01" class="w-full rounded-xl border-slate-200 font-mono" required />
            </div>

            <div>
              <label class="block font-bold text-slate-400 uppercase mb-1">Descrição</label>
              <textarea v-model="formTratamento.Descricao" rows="3" placeholder="Informações técnicas..." class="w-full rounded-xl border-slate-200"></textarea>
            </div>

            <button type="submit" class="w-full bg-slate-950 hover:bg-slate-800 text-white font-bold py-3 rounded-xl transition shadow-sm uppercase tracking-wider text-[10px]">
              Salvar Tratamento
            </button>
          </form>
        </div>
      </div>

      <!-- =========================================================================
           ABA 3: IMPORTADOR INTELIGENTE POR IA (PDF / RAW TEXT)
           ========================================================================= -->
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
  Precos: { type: Array, default: () => [] },
  Tratamentos: { type: Array, default: () => [] },
  IsAdmin: { type: Boolean, default: false }
})

const abaAtiva = ref('precos')
const filtroBusca = ref('')
const carregandoImportacao = ref(false)

// Garante que a verificação de perfil seja defensiva e segura
const isAdmin = computed(() => props.IsAdmin ?? (page.props.auth?.usuarioPerfil || '').toLowerCase() === 'admin')

// Filtro em tempo de digitação reativo
const precosFiltrados = computed(() => {
  const t = filtroBusca.value.toLowerCase()
  if (!t) return props.Precos

  return props.Precos.filter(p => 
    (p.Lente?.Laboratorio || '').toLowerCase().includes(t) ||
    (p.Lente?.Tipo || '').toLowerCase().includes(t) ||
    (p.Tipo || '').toLowerCase().includes(t)
  )
})

// Formulários reativos para controlo Inertia
const formPreco = useForm({
  LenteId: '',
  Tipo: 'MONOFOCAL',
  IndiceRefracao: null,
  TratamentoId: null,
  PrecoCusto: 0,
  PrecoVenda: 0
})

const formTratamento = useForm({
  Nome: '',
  Descricao: '',
  AcrescimoValor: 0
})

const formImportacao = ref({
  Laboratorio: '',
  TextoBruto: ''
})

const formatMoeda = (valor) => {
  if (valor === undefined || valor === null) return '0,00'
  return Number(valor).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

// Métodos de Cadastro via POST Inertia para consistência nas páginas
const cadastrarPreco = () => {
  formPreco.post('/lentes/precos', {
    onSuccess: () => {
      formPreco.reset()
      alert('Preço inserido com sucesso na matriz ativa!')
    }
  })
}

const cadastrarTratamento = () => {
  formTratamento.post('/lentes/tratamentos', {
    onSuccess: () => {
      formTratamento.reset()
      alert('Tratamento de lentes adicionado com sucesso!')
    }
  })
}

// Remoções seguras via DELETE
const removerPreco = (id) => {
  if (confirm('Tem certeza de que deseja remover este preço da matriz?')) {
    router.delete(`/lentes/precos/${id}`)
  }
}

const removerTratamento = (id) => {
  if (confirm('Tem certeza de que deseja remover este tratamento do sistema?')) {
    router.delete(`/lentes/tratamentos/${id}`)
  }
}

// Importador Inteligente por IA via endpoint de API dedicado (Ollama local)
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
      router.reload() // Atualiza os dados do ecrã sem descer a navegação
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