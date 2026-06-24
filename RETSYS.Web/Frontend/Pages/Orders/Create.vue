<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-5xl mx-auto">
      
      <div class="bg-white rounded-3xl border border-slate-200 shadow-xl overflow-hidden">
        
        <div class="bg-slate-950 text-white p-6 flex items-center justify-between">
          <div>
            <h1 class="text-xl font-black tracking-wide">Nova Ordem de Serviço</h1>
            <p class="text-xs text-slate-400">Insira os dados clínicos da receita e condições comerciais.</p>
          </div>
          <span class="text-xs font-mono bg-teal-500/20 text-teal-400 px-3 py-1 rounded-full border border-teal-500/30">RETSYS Web v2</span>
        </div>

        <form @submit.prevent="salvarOrdemServico" class="p-6 space-y-8">
          
          <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div>
              <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Cliente *</label>
              <select v-model="form.ClienteId" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required>
                <option value="">Selecione o Cliente</option>
                <option v-for="c in Clientes" :key="c.id || c.Id" :value="c.id || c.Id">
                  {{ c.nome || c.Nome }} ({{ c.cpf || c.CPF }})
                </option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Vendedor / Atendente *</label>
              <select v-model="form.UsuarioId" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required>
                <option value="">Selecione o Vendedor</option>
                <option v-for="v in Vendedores" :key="v.id || v.Id" :value="v.id || v.Id">
                  {{ v.nome || v.Nome }}
                </option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Médico Oftalmologista *</label>
              <input v-model="form.Medico" type="text" placeholder="Dr. Nome do Médico" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>
          </div>

          <div class="bg-white rounded-2xl border-2 border-dashed border-slate-200 p-6 space-y-4">
            <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2">
              <div>
                <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider flex items-center gap-2">
                  <span class="w-2.5 h-2.5 rounded-full bg-teal-500 animate-pulse"></span>
                  Assistente de Leitura por IA
                </h3>
                <p class="text-xs text-slate-400 mt-0.5">Faça o upload da foto da receita para preenchimento automatizado.</p>
              </div>
              <span class="text-[10px] font-mono font-bold bg-slate-100 text-slate-600 px-2 py-0.5 rounded border border-slate-200">
                Motor Local: Moondream 1.4B
              </span>
            </div>

            <div class="p-3.5 bg-amber-50 border border-amber-200 rounded-xl flex items-start gap-2.5 text-amber-800 text-xs leading-relaxed">
              <span class="text-base leading-none">⚠️</span>
              <div>
                <strong class="font-bold block mb-0.5">Função Experimental Ativa</strong>
                Este recurso utiliza um modelo de IA local para decodificar receitas médicas. A conferência minuciosa e a correção manual de todos os graus gerados são de inteira responsabilidade do operador antes da emissão da OS.
              </div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 pt-2">
              <div class="space-y-3">
                <div class="flex items-center gap-3">
                  <input type="file" id="fotoReceita" accept="image/*" @change="manipularArquivo" class="hidden" />
                  <label for="fotoReceita" class="bg-slate-950 hover:bg-slate-800 text-white text-xs font-bold px-4 py-3 rounded-xl transition cursor-pointer shadow-sm whitespace-nowrap active:scale-95 select-none">
                    {{ arquivoSelecionado ? 'Alterar Imagem' : 'Selecionar Foto Receita' }}
                  </label>
                  <span class="text-xs font-mono text-slate-500 truncate block max-w-[200px]">
                    {{ arquivoSelecionado ? arquivoSelecionado.name : 'Nenhum arquivo anexado' }}
                  </span>
                </div>

                <div class="flex items-start gap-2">
                  <input type="checkbox" id="termoOcr" v-model="termoAceito" class="mt-0.5 rounded border-slate-300 text-teal-600 focus:ring-teal-500" />
                  <label for="termoOcr" class="text-[11px] text-slate-500 leading-tight cursor-pointer select-none">
                    Confirmo que farei a revisão dos graus clínicos gerados pela inteligência artificial.
                  </label>
                </div>
              </div>

              <div class="flex items-end justify-start md:justify-end">
                <button
                  type="button"
                  @click="executarOcrInteligente"
                  :disabled="!termoAceito || !arquivoSelecionado || carregandoIA"
                  class="w-full sm:w-auto bg-teal-600 hover:bg-teal-700 disabled:bg-slate-100 text-white disabled:text-slate-400 font-bold py-3 px-6 rounded-xl text-xs uppercase tracking-wider transition shadow-sm disabled:shadow-none flex items-center justify-center gap-2"
                >
                  <span v-if="carregandoIA" class="animate-pulse">Analisando receita no servidor...</span>
                  <span v-else>Iniciar Leitura Digital</span>
                </button>
              </div>
            </div>
          </div>

          <div class="bg-slate-50 p-6 rounded-2xl border border-slate-200 space-y-4">
            <h3 class="text-sm font-black text-slate-700 uppercase tracking-wider flex items-center gap-2">
              <span class="w-2 h-2 rounded-full bg-teal-500"></span> Dados de Refração: Longe
            </h3>
            
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div class="space-y-2 border-r border-slate-200/60 pr-4">
                <span class="text-xs font-bold text-slate-400 block">Olho Direito (OD)</span>
                <input v-model.number="form.EsfericoLongeDireito" type="number" step="0.25" placeholder="Esférico" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" />
                <input v-model.number="form.CilindricoLongeDireito" type="number" step="0.25" placeholder="Cilindrico" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" />
                <input v-model.number="form.EixoLongeDireito" type="number" placeholder="Eixo °" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" />
              </div>

              <div class="space-y-2 border-r border-slate-200/60 pr-4">
                <span class="text-xs font-bold text-slate-400 block">Olho Esquerdo (OE)</span>
                <input v-model.number="form.EsfericoLongeEsquerdo" type="number" step="0.25" placeholder="Esférico" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" />
                <input v-model.number="form.CilindricoLongeEsquerdo" type="number" step="0.25" placeholder="Cilindrico" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" />
                <input v-model.number="form.EixoLongeEsquerdo" type="number" placeholder="Eixo °" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" />
              </div>

              <div class="flex flex-col justify-center bg-teal-50/50 p-4 rounded-xl border border-teal-100">
                <label class="block text-xs font-bold uppercase text-teal-800 tracking-wider mb-2">Adição (AD)</label>
                <input v-model.number="form.Adicao" type="number" step="0.25" placeholder="0.00" class="w-full rounded-xl border-teal-200 text-sm focus:border-teal-500 focus:ring-teal-500 bg-white font-mono text-teal-900" />
                <p class="text-[11px] text-teal-600 mt-2 leading-tight">O sistema calculará as propriedades de Perto automaticamente somando a Adição.</p>
              </div>
            </div>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div>
              <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Tipo de Lente</label>
              <input v-model="form.TipoLente" type="text" placeholder="Ex: Monofocal, Multifocal" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
            </div>
            <div>
              <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Marca / Modelo</label>
              <input v-model="form.MarcaModeloLente" type="text" placeholder="Ex: Varilux, Crizal" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
            </div>
            <div>
              <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Material</label>
              <input v-model="form.MaterialLente" type="text" placeholder="Ex: Resina, Policarbonato" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
            </div>
          </div>

          <div class="border-t border-slate-200 pt-6 grid grid-cols-1 md:grid-cols-3 gap-6 items-end">
            <div>
              <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Data da Receita</label>
              <input v-model="form.DataReceita" type="date" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>
            <div>
              <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Dividir Venda em Quantas Vezes?</label>
              <select v-model.number="qtdParcelas" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono">
                <option v-for="n in 6" :key="n" :value="n">{{ n }}x sem juros</option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Valor Total da Venda (R$) *</label>
              <input v-model.number="form.ValorTotal" type="number" step="0.01" placeholder="0,00" class="w-full rounded-xl border-slate-200 text-base font-bold text-teal-600 focus:border-teal-500 focus:ring-teal-500 font-mono" required />
            </div>
          </div>

          <div class="flex items-center justify-end gap-3 border-t border-slate-100 pt-4">
            <Link href="/ordens" class="px-5 py-3 text-sm font-semibold text-slate-500 hover:text-slate-800 transition">
              Cancelar
            </Link>
            <button 
              type="submit" 
              :disabled="form.processing"
              class="bg-teal-600 hover:bg-teal-700 disabled:bg-slate-200 disabled:text-slate-400 text-white font-bold py-3.5 px-8 rounded-xl shadow-md transition text-sm min-w-[200px]"
            >
              <span v-if="form.processing">Processando OS...</span>
              <span v-else>Emitir Ordem de Serviço</span>
            </button>
          </div>

        </form>
      </div>
    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { ref } from 'vue'
import { useForm, Link } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

defineProps({
  Clientes: Array,
  Vendedores: Array
})

const qtdParcelas = ref(1)
const termoAceito = ref(false)
const carregandoIA = ref(false)
const arquivoSelecionado = ref(null)

const form = useForm({
  ClienteId: '',
  UsuarioId: '',
  Medico: '',
  DataReceita: '',
  TipoLente: '',
  MarcaModeloLente: '',
  MaterialLente: '',
  EsfericoLongeDireito: 0,
  EsfericoLongeEsquerdo: 0,
  CilindricoLongeDireito: 0,
  CilindricoLongeEsquerdo: 0,
  EixoLongeDireito: 0,
  EixoLongeEsquerdo: 0,
  Adicao: 0,
  ValorTotal: 0
})

const manipularArquivo = (event) => {
  const arquivos = event.target.files
  if (arquivos.length > 0) {
    arquivoSelecionado.value = arquivos[0]
  }
}

const executarOcrInteligente = async () => {
  if (!arquivoSelecionado.value || !termoAceito.value) return

  carregandoIA.value = true
  const formData = new FormData()
  formData.append('imagemReceita', arquivoSelecionado.value)

  try {
    const resposta = await fetch('/ordens/processar-receita-ia', {
      method: 'POST',
      body: formData
    })

    if (resposta.ok) {
      const dadosIa = await resposta.json()
      
      form.Medico = dadosIa.medico || dadosIa.Medico || ''
      form.TipoLente = dadosIa.tipoLente || dadosIa.TipoLente || ''
      form.EsfericoLongeDireito = dadosIa.esfericoLongeDireito ?? dadosIa.EsfericoLongeDireito ?? 0
      form.EsfericoLongeEsquerdo = dadosIa.esfericoLongeEsquerdo ?? dadosIa.EsfericoLongeEsquerdo ?? 0
      form.CilindricoLongeDireito = dadosIa.cilindricoLongeDireito ?? dadosIa.CilindricoLongeDireito ?? 0
      form.CilindricoLongeEsquerdo = dadosIa.cilindricoLongeEsquerdo ?? dadosIa.CilindricoLongeEsquerdo ?? 0
      form.EixoLongeDireito = dadosIa.eixoLongeDireito ?? dadosIa.EixoLongeDireito ?? 0
      form.EixoLongeEsquerdo = dadosIa.eixoLongeEsquerdo ?? dadosIa.EixoLongeEsquerdo ?? 0
      form.Adicao = dadosIa.adicao ?? dadosIa.Adicao ?? 0

      alert('Dados extraídos da receita com sucesso! Por favor, confira os campos antes de salvar.')
    } else {
      alert('O motor local não conseguiu estruturar os dados. Verifique a imagem ou use a digitação manual.')
    }
  } catch (erro) {
    console.error(erro)
    alert('Erro de comunicação com o serviço local de inteligência.')
  } finally {
    carregandoIA.value = false
  }
}

const salvarOrdemServico = () => {
  form.post(`/ordens?quantidadeParcelas=${qtdParcelas.value}`)
}
</script>