<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-5xl mx-auto">
      
      <div v-if="$page.props.flash?.erro" class="p-4 bg-red-50 border border-red-200 text-red-800 rounded-xl text-sm font-semibold shadow-sm">
        🛑 {{ $page.props.flash.erro }}
      </div>

      <div class="bg-white rounded-3xl border border-slate-200 shadow-xl overflow-hidden">
        
        <div class="bg-slate-950 text-white p-6 flex items-center justify-between">
          <div>
            <h1 class="text-xl font-black tracking-wide">Central Unificada de Emissão de OS</h1>
            <p class="text-xs text-slate-400">Fluxo contínuo: Identificação do cliente, dados clínicos e fechamento financeiro.</p>
          </div>
          <span class="text-xs font-mono bg-teal-500/20 text-teal-400 px-3 py-1 rounded-full border border-teal-500/30">RETSYS CRM v3</span>
        </div>

        <form @submit.prevent="salvarOrdemServico" class="p-6 space-y-8">
          
          <div class="bg-slate-50 p-6 rounded-2xl border border-slate-200 space-y-4">
            <h3 class="text-sm font-black text-slate-700 uppercase tracking-wider flex items-center gap-2">
              <span class="w-2 h-2 rounded-full bg-slate-950"></span> 1. Identificação do Cliente (CRM)
            </h3>
            
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 items-end">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">CPF do Cliente *</label>
                <div class="flex gap-2">
                  <input v-model="form.cpf" type="text" placeholder="000.000.000-00" class="w-full rounded-xl border-slate-200 text-sm font-mono focus:border-teal-500 focus:ring-teal-500" required />
                  <button type="button" @click="consultarCpfNoBanco" class="bg-slate-950 hover:bg-slate-800 text-white px-4 py-2.5 rounded-xl text-xs font-bold transition whitespace-nowrap">
                    Buscar CPF
                  </button>
                </div>
              </div>
              <div class="md:col-span-2">
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Nome Completo *</label>
                <input v-model="form.nome" type="text" placeholder="Nome do Paciente" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
              </div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">WhatsApp / Telefone *</label>
                <input v-model="form.telefone" type="text" placeholder="(00) 00000-0000" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Data de Nascimento *</label>
                <input v-model="form.dataNascimento" type="date" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
              </div>
              <div class="md:col-span-2">
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Convênio / Plano Óptico</label>
                <input v-model="form.convenio" type="text" placeholder="Particular, Porto Seguro, etc." class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-4 gap-4 pt-2 border-t border-slate-200/60">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">CEP Residencia *</label>
                <input v-model="form.cep" type="text" @blur="buscarEnderecoViaCep" placeholder="00000-000" class="w-full rounded-xl border-slate-200 text-sm font-mono focus:border-teal-500 focus:ring-teal-500" required />
              </div>
              <div class="md:col-span-2">
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Logradouro *</label>
                <input v-model="form.logradouro" type="text" placeholder="Rua / Avenida" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Número *</label>
                <input v-model="form.numero" type="text" placeholder="Nº" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
              </div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Complemento</label>
                <input v-model="form.complemento" type="text" placeholder="Apto, Bloco, etc." class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Bairro *</label>
                <input v-model="form.bairro" type="text" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Cidade *</label>
                <input v-model="form.cidade" type="text" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Estado (UF) *</label>
                <input v-model="form.estado" type="text" maxlength="2" placeholder="EX: SP" class="w-full rounded-xl border-slate-200 text-sm uppercase text-center focus:border-teal-500 focus:ring-teal-500" required />
              </div>
            </div>

            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">E-mail do Cliente</label>
              <input v-model="form.email" type="email" placeholder="cliente@provedor.com" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
            </div>
          </div>

          <div class="bg-white rounded-2xl border-2 border-dashed border-slate-200 p-6 space-y-4">
            <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2">
              <div>
                <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider flex items-center gap-2">
                  <span class="w-2.5 h-2.5 rounded-full bg-teal-500 animate-pulse"></span>
                  Assistente de Leitura por IA (Moondream)
                </h3>
                <p class="text-xs text-slate-400 mt-0.5">Faça o upload da receita médica para decodificação automatizada.</p>
              </div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 pt-2">
              <div class="space-y-3">
                <div class="flex items-center gap-3">
                  <input type="file" id="fotoReceita" accept="image/*" @change="manipularArquivo" class="hidden" />
                  <label for="fotoReceita" class="bg-slate-950 hover:bg-slate-800 text-white text-xs font-bold px-4 py-3 rounded-xl transition cursor-pointer shadow-sm active:scale-95 select-none">
                    {{ arquivoSelecionado ? 'Alterar Imagem' : 'Selecionar Foto Receita' }}
                  </label>
                  <span class="text-xs font-mono text-slate-500 truncate block max-w-[200px]">
                    {{ arquivoSelecionado ? arquivoSelecionado.name : 'Nenhum arquivo anexado' }}
                  </span>
                </div>
                <div class="flex items-start gap-2">
                  <input type="checkbox" id="termoOcr" v-model="termoAceito" class="mt-0.5 rounded border-slate-300 text-teal-600 focus:ring-teal-500" />
                  <label for="termoOcr" class="text-[11px] text-slate-500 leading-tight cursor-pointer select-none">
                    Confirmo que revisarei minuciosamente todos os graus gerados pela IA antes de emitir a OS.
                  </label>
                </div>
              </div>
              <div class="flex items-end justify-start md:justify-end">
                <button type="button" @click="executarOcrInteligente" :disabled="!termoAceito || !arquivoSelecionado || carregandoIA" class="w-full sm:w-auto bg-teal-600 hover:bg-teal-700 disabled:bg-slate-100 text-white disabled:text-slate-400 font-bold py-3 px-6 rounded-xl text-xs uppercase tracking-wider transition shadow-sm flex items-center justify-center gap-2">
                  <span v-if="carregandoIA" class="animate-pulse">Analisando receita no servidor...</span>
                  <span v-else>Iniciar Leitura Digital</span>
                </button>
              </div>
            </div>
          </div>

          <div class="bg-slate-50 p-6 rounded-2xl border border-slate-200 space-y-4">
            <h3 class="text-sm font-black text-slate-700 uppercase tracking-wider flex items-center gap-2">
              <span class="w-2 h-2 rounded-full bg-teal-500"></span> 2. Dados Gerais da Receita e Refração
            </h3>

            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Médico Oftalmologista *</label>
                <input v-model="form.medicoNome" type="text" placeholder="Dr. Nome do Médico" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">CRM do Médico</label>
                <input v-model="form.medicoCrm" type="text" placeholder="000000-UF" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Atendente / Responsável *</label>
                <select v-model="form.vendedorId" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required>
                  <option value="">Selecione o Vendedor</option>
                  <option v-for="v in Vendedores" :key="v.Id" :value="v.Id">{{ v.Nome }}</option>
                </select>
              </div>
            </div>
            
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 pt-2">
              <div class="space-y-2 border-r border-slate-200/60 pr-4">
                <span class="text-xs font-bold text-slate-400 block uppercase tracking-wider">Olho Direito (OD)</span>
                <input v-model.number="form.odEsferico" type="number" step="0.25" placeholder="Esférico" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" />
                <input v-model.number="form.odCilindrico" type="number" step="0.25" placeholder="Cilíndrico" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" />
                <input v-model.number="form.odEixo" type="number" placeholder="Eixo ° (0 a 180)" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" />
              </div>

              <div class="space-y-2 border-r border-slate-200/60 pr-4">
                <span class="text-xs font-bold text-slate-400 block uppercase tracking-wider">Olho Esquerdo (OE)</span>
                <input v-model.number="form.oeEsferico" type="number" step="0.25" placeholder="Esférico" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" />
                <input v-model.number="form.oeCilindrico" type="number" step="0.25" placeholder="Cilíndrico" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" />
                <input v-model.number="form.oeEixo" type="number" placeholder="Eixo ° (0 a 180)" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" />
              </div>

              <div class="flex flex-col justify-center bg-teal-50/50 p-4 rounded-xl border border-teal-100">
                <label class="block text-xs font-bold uppercase text-teal-800 tracking-wider mb-2">Adição (AD)</label>
                <input v-model.number="form.adicao" type="number" step="0.25" placeholder="0.00" class="w-full rounded-xl border-teal-200 text-sm focus:border-teal-500 focus:ring-teal-500 bg-white font-mono text-teal-900" />
                <p class="text-[10px] text-teal-600 mt-2 leading-tight">Obrigatório para lentes Progressivas/Multifocais. O perto é calculado de forma automática.</p>
              </div>
            </div>
          </div>

          <div class="bg-white p-6 rounded-2xl border border-slate-200 space-y-4">
            <h3 class="text-sm font-black text-slate-700 uppercase tracking-wider flex items-center gap-2">
              <span class="w-2 h-2 rounded-full bg-indigo-500"></span> 3. Medidas Técnicas de Laboratório
            </h3>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
              <div>
                <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">DNP - Olho Direito (mm) *</label>
                <input v-model.number="form.dnpOd" type="number" step="0.5" placeholder="0.0" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" required />
              </div>
              <div>
                <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">DNP - Olho Esquerdo (mm) *</label>
                <input v-model.number="form.dnpOe" type="number" step="0.5" placeholder="0.0" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" required />
              </div>
              <div>
                <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Altura de Montagem (mm)</label>
                <input v-model.number="form.alturaMontagem" type="number" step="0.5" placeholder="Obrigatório para progressivas" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" />
              </div>
            </div>
            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Observações da Receita / Laboratório</label>
              <input v-model="form.obsReceita" type="text" placeholder="Ex: Quebrar cantos das lentes, canalar alta miopia" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
            </div>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Armação Selecionada (Estoque Real) *</label>
              <select v-model="form.armacaoId" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required>
                <option value="">Selecione o modelo do Inventário</option>
                <option v-for="a in Armacoes" :key="a.Id" :value="a.Id">
                  {{ a.ModeloReferencia }} - Cor: {{ a.Cor }} (Estoque: {{ a.QuantidadeEstoque }} un) - R$ {{ a.PrecoVenda }}
                </option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Lente de Laboratório Disponível *</label>
              <select v-model="form.lenteId" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required>
                <option value="">Selecione o Bloco de Lente Homologado</option>
                <option v-for="l in Lentes" :key="l.Id" :value="l.Id">
                  [{{ l.Laboratorio }}] {{ l.Tipo }} - Tratamento: {{ l.Tratamento || 'N/A' }} - R$ {{ l.PrecoVenda }}
                </option>
              </select>
            </div>
          </div>

          <div class="border-t border-slate-200 pt-6 grid grid-cols-1 md:grid-cols-3 gap-6 items-end">
            <div>
              <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Data da Emissão/Receita *</label>
              <input v-model="form.dataReceita" type="date" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>
            <div>
              <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Previsão de Entrega (Laboratório) *</label>
              <input v-model="form.dataPrevistaEntrega" type="date" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>
            <div>
              <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Forma de Liquidação Principal *</label>
              <select v-model="form.formaPagamento" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required>
                <option value="DINHEIRO">Dinheiro à Vista</option>
                <option value="PIX">Pix Transferência</option>
                <option value="CARTAO_CREDITO">Cartão de Crédito</option>
                <option value="CARTAO_DEBITO">Cartão de Débito</option>
                <option value="CONVENIO">Faturamento Convênio</option>
              </select>
            </div>
          </div>

          <div class="bg-teal-50/30 border border-teal-100 p-6 rounded-2xl grid grid-cols-1 sm:grid-cols-4 gap-4">
            <div>
              <label class="block text-[11px] font-bold uppercase text-teal-800 tracking-wider mb-1.5">Total Bruto da Venda (R$)</label>
              <input v-model.number="form.valorTotalBruto" type="number" step="0.01" @input="calcularValoresFinais" placeholder="0,00" class="w-full rounded-xl border-teal-200 text-sm font-semibold text-slate-700 focus:border-teal-500 focus:ring-teal-500 font-mono" required />
            </div>
            <div>
              <label class="block text-[11px] font-bold uppercase text-teal-800 tracking-wider mb-1.5">Desconto Concedido (R$)</label>
              <input v-model.number="form.descontoReais" type="number" step="0.01" @input="calcularValoresFinais" placeholder="0,00" class="w-full rounded-xl border-teal-200 text-sm font-semibold text-red-600 focus:border-teal-500 focus:ring-teal-500 font-mono" />
            </div>
            <div>
              <label class="block text-[11px] font-bold uppercase text-teal-800 tracking-wider mb-1.5">Valor Líquido Cobrado (R$)</label>
              <input v-model.number="form.valorTotalLiquido" type="number" step="0.01" class="w-full rounded-xl border-teal-200 text-base font-black text-teal-600 bg-teal-50/50 font-mono" readonly />
            </div>
            <div>
              <label class="block text-[11px] font-bold uppercase text-teal-800 tracking-wider mb-1.5">Sinal / Valor de Entrada</label>
              <input v-model.number="form.valorEntrada" type="number" step="0.01" placeholder="0,00" class="w-full rounded-xl border-teal-200 text-sm font-semibold text-slate-700 focus:border-teal-500 focus:ring-teal-500 font-mono" />
            </div>
          </div>

          <div>
            <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Observações Gerais Internas</label>
            <input v-model="form.observacoes" type="text" placeholder="Anotações comerciais e prazos especiais do balcão..." class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-6 items-center border-t border-slate-100 pt-4">
            <div>
              <label class="inline-block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Dividir saldo restante em:</label>
              <select v-model.number="qtdParcelas" class="w-32 ml-3 rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono">
                <option v-for="n in 6" :key="n" :value="n">{{ n }}x</option>
              </select>
            </div>

            <div class="flex items-center justify-end gap-3">
              <Link href="/ordens" class="px-5 py-3 text-sm font-semibold text-slate-500 hover:text-slate-800 transition">
                Cancelar
              </Link>
              <button type="submit" :disabled="form.processing" class="bg-teal-600 hover:bg-teal-700 disabled:bg-slate-200 disabled:text-slate-400 text-white font-bold py-3.5 px-8 rounded-xl shadow-md transition text-sm min-w-[200px]">
                <span v-if="form.processing">Processando Emissão...</span>
                <span v-else>Faturar Ordem de Serviço</span>
              </button>
            </div>
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
  Vendedores: Array,
  Armacoes: Array,
  Lentes: Array
})

const qtdParcelas = ref(1)
const termoAceito = ref(false)
const carregandoIA = ref(false)
const arquivoSelecionado = ref(null)

// Configuração estruturada para envio direto via HTTP POST
const form = useForm({
  // CRM / Cliente unificado
  cpf: '',
  nome: '',
  telefone: '',
  dataNascimento: '',
  logradouro: '',
  numero: '',
  complemento: '',
  bairro: '',
  cidade: '',
  estado: '',
  cep: '',
  convenio: '',
  email: '',
  
  // Cabeçalho da Ordem
  vendedorId: '',
  dataReceita: '',
  dataPrevistaEntrega: '',
  medicoNome: '',
  medicoCrm: '',
  observacoes: '',

  // Dados Clínicos
  odEsferico: 0,
  odCilindrico: 0,
  odEixo: 0,
  oeEsferico: 0,
  oeCilindrico: 0,
  oeEixo: 0,
  adicao: null,
  dnpOd: 0,
  dnpOe: 0,
  alturaMontagem: null,
  obsReceita: '',

  // Comercial Estoque
  armacaoId: '',
  lenteId: '',
  valorTotalBruto: 0,
  descontoReais: 0,
  valorTotalLiquido: 0,
  valorEntrada: null,
  formaPagamento: 'DINHEIRO'
})

// Busca Inteligente de CPF no CRM
const consultarCpfNoBanco = async () => {
  if (!form.cpf || form.cpf.length < 11) return

  try {
    const resposta = await fetch(`/api/clientes/buscar-por-cpf/${form.cpf}`)
    if (resposta.ok) {
      const c = await resposta.json()
      form.nome = c.nome || ''
      form.telefone = c.telefone || ''
      form.dataNascimento = c.dataNascimento ? c.dataNascimento.split('T')[0] : ''
      form.cep = c.cep || ''
      form.logradouro = c.logradouro || ''
      form.numero = c.numero || ''
      form.complemento = c.complemento || ''
      form.bairro = c.bairro || ''
      form.cidade = c.cidade || ''
      form.estado = c.estado || ''
      form.convenio = c.convenio || ''
      form.email = c.email || ''
      alert('Cliente localizado no banco de dados! Informações pré-carregadas.')
    }
  } catch (err) {
    console.error('CPF não localizado ou rota indisponível. Seguir com preenchimento manual.')
  }
}

// Integração de Endereço Automática via ViaCEP
const buscarEnderecoViaCep = async () => {
  const limpo = form.cep.replace(/\D/g, '')
  if (limpo.length !== 8) return

  try {
    const res = await fetch(`https://viacep.com.br/ws/${limpo}/json/`)
    const dados = await res.json()
    if (!dados.erro) {
      form.logradouro = dados.logradouro
      form.bairro = dados.bairro
      form.cidade = dados.localidade
      form.estado = dados.uf
    }
  } catch (e) {
    console.error('Falha ao conectar com o serviço de CEP.')
  }
}

// Cálculo comercial em tempo de execução
const calcularValoresFinais = () => {
  const bruto = form.valorTotalBruto || 0
  const desconto = form.descontoReais || 0
  form.valorTotalLiquido = Math.max(0, bruto - desconto)
}

const manipularArquivo = (event) => {
  const arquivos = event.target.files
  if (arquivos.length > 0) {
    arquivoSelecionado.value = arquivos[0]
  }
}

// Execução do assistente de Visão Computacional local
const ejecutarOcrInteligente = async () => {
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
      const d = await resposta.json()
      form.medicoNome = d.medicoNome || ''
      form.odEsferico = d.odEsferico ?? 0
      form.odCilindrico = d.odCilindrico ?? 0
      form.odEixo = d.odEixo ?? 0
      form.oeEsferico = d.oeEsferico ?? 0
      form.oeCilindrico = d.oeCilindrico ?? 0
      form.oeEixo = d.oeEixo ?? 0
      form.adicao = d.adicao ?? null
      alert('Dados extraídos da receita com sucesso! Faça a revisão minuciosa antes do faturamento.')
    } else {
      alert('O motor local não conseguiu ler a imagem. Use a digitação dos campos.')
    }
  } catch (erro) {
    console.error(erro)
    alert('Erro de timeout ou queda de conexão com o Ollama local.')
  } finally {
    carregandoIA.value = false
  }
}

const salvarOrdemServico = () => {
  form.post(`/ordens?quantidadeParcelas=${qtdParcelas.value}`)
}
</script>