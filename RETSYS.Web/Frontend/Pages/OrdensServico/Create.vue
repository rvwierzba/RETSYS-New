<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-5xl mx-auto">

      <!-- Alertas de Erro Globais -->
      <div v-if="$page.props.flash?.erro" class="p-4 bg-red-50 border border-red-200 text-red-800 rounded-xl text-sm font-semibold shadow-sm no-print">
        🛑 {{ $page.props.flash.erro }}
      </div>

      <div v-if="erroSubmissao" class="p-4 bg-red-50 border border-red-200 text-red-800 rounded-xl text-sm font-semibold shadow-sm no-print">
        🛑 {{ erroSubmissao }}
      </div>

      <!-- TELA 1: FORMULÁRIO OPERACIONAL DE EMISSÃO -->
      <div v-if="!exibirFaturaSucesso" class="bg-white rounded-3xl border border-slate-200 shadow-xl overflow-hidden no-print">

        <div class="bg-slate-950 text-white p-6 flex items-center justify-between">
          <div>
            <h1 class="text-xl font-black tracking-wide">Central Unificada de Emissão de OS</h1>
            <p class="text-xs text-slate-400">Fluxo contínuo: Identificação do cliente, dados clínicos e fechamento financeiro.</p>
          </div>
          <span class="text-xs font-mono bg-teal-500/20 text-teal-400 px-3 py-1 rounded-full border border-teal-500/30">RETSYS CRM v5</span>
        </div>

        <form @submit.prevent="salvarOrdemServico" class="p-6 space-y-8">

          <!-- Bloco 1: Identificação do Cliente (CRM) -->
          <div class="bg-slate-50 p-6 rounded-2xl border border-slate-200 space-y-4">
            <h3 class="text-sm font-black text-slate-700 uppercase tracking-wider flex items-center gap-2">
              <span class="w-2 h-2 rounded-full bg-slate-950"></span> 1. Identificação do Cliente (CRM)
            </h3>

            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 items-end">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">CPF do Cliente *</label>
                <div class="flex gap-2">
                  <input v-model="form.cpf" type="text" placeholder="000.000.000-00" class="w-full rounded-xl border-slate-200 text-sm font-mono focus:border-teal-500 focus:ring-teal-500" required />
                  <button type="button" @click="consultarCpfNoBanco" :disabled="consultandoCpf" class="bg-slate-950 hover:bg-slate-800 disabled:bg-slate-400 text-white px-4 py-2.5 rounded-xl text-xs font-bold transition whitespace-nowrap">
                    {{ consultandoCpf ? 'Buscando...' : 'Buscar CPF' }}
                  </button>
                </div>
              </div>
              <div class="md:col-span-2">
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Nome Completo *</label>
                <input v-model="form.nome" type="text" placeholder="Nome do Paciente" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
              </div>
            </div>

            <div v-if="clienteLocalizado !== null" class="animate-fadeIn">
              <div v-if="clienteLocalizado" class="p-3 bg-teal-50 border border-teal-200 text-teal-800 rounded-xl text-xs font-semibold flex items-center gap-2">
                <span>✓ Cliente localizado no CRM! Os dados de cadastro e endereço foram preenchidos de forma automática.</span>
              </div>
              <div v-else class="p-3 bg-amber-50 border border-amber-200 text-amber-800 rounded-xl text-xs font-semibold flex items-center gap-2">
                <span>📝 CPF não localizado. Continue preenchendo os campos abaixo; este cliente será cadastrado automaticamente ao faturar a OS!</span>
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
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">CEP Residência *</label>
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

          <!-- Assistente de IA (Ollama/Moondream) -->
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
                  <span v-if="carregandoIA" class="animate-pulse">Analisando receita...</span>
                  <span v-else>Iniciar Leitura Digital</span>
                </button>
              </div>
            </div>
          </div>

          <!-- Bloco 2: Dados Clínicos da Receita Médica -->
          <div class="bg-slate-50 p-6 rounded-2xl border border-slate-200 space-y-4">
            <h3 class="text-sm font-black text-slate-700 uppercase tracking-wider flex items-center gap-2">
              <span class="w-2 h-2 rounded-full bg-teal-500"></span> 2. Dados Clínicos da Receita Médica
            </h3>

            <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
              <div class="md:col-span-2">
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Médico Responsável</label>
                <input v-model="form.medicoNome" type="text" placeholder="Dr. Nome do Profissional" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">CRM / Registro</label>
                <input v-model="form.medicoCrm" type="text" placeholder="000000-UF" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Tipo de Profissional *</label>
                <select v-model="form.medicoTipo" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required>
                  <option value="NAO_ESPECIFICADO">Não Especificado</option>
                  <option value="OFTALMOLOGISTA">Oftalmologista</option>
                  <option value="OPTOMETRISTA">Optometrista</option>
                </select>
              </div>
            </div>

            <div class="bg-white p-4 rounded-xl border border-slate-200 space-y-3 mt-4">
              <div class="grid grid-cols-4 gap-4 font-bold text-[11px] text-slate-400 uppercase tracking-wider text-center border-b pb-2">
                <div>Olho</div>
                <div>Esférico</div>
                <div>Cilíndrico</div>
                <div>Eixo (°)</div>
              </div>

              <div class="grid grid-cols-4 gap-4 items-center">
                <div class="text-sm font-black text-slate-700 text-center">OD</div>
                <input v-model.number="form.odEsferico" type="number" step="0.25" placeholder="0,00" class="rounded-xl border-slate-200 text-sm text-center font-mono focus:border-teal-500" />
                <input v-model.number="form.odCilindrico" type="number" step="0.25" placeholder="0,00" class="rounded-xl border-slate-200 text-sm text-center font-mono focus:border-teal-500" />
                <input v-model.number="form.odEixo" type="number" min="0" max="180" placeholder="0" class="rounded-xl border-slate-200 text-sm text-center font-mono focus:border-teal-500" />
              </div>

              <div class="grid grid-cols-4 gap-4 items-center">
                <div class="text-sm font-black text-slate-700 text-center">OE</div>
                <input v-model.number="form.oeEsferico" type="number" step="0.25" placeholder="0,00" class="rounded-xl border-slate-200 text-sm text-center font-mono focus:border-teal-500" />
                <input v-model.number="form.oeCilindrico" type="number" step="0.25" placeholder="0,00" class="rounded-xl border-slate-200 text-sm text-center font-mono focus:border-teal-500" />
                <input v-model.number="form.oeEixo" type="number" min="0" max="180" placeholder="0" class="rounded-xl border-slate-200 text-sm text-center font-mono focus:border-teal-500" />
              </div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 items-center pt-2">
              <div class="flex flex-col bg-teal-50/50 p-4 rounded-xl border border-teal-100">
                <label class="block text-xs font-bold uppercase text-teal-800 tracking-wider mb-1.5">Adição (AD)</label>
                <input v-model.number="form.adicao" type="number" step="0.25" placeholder="0.00" class="w-full rounded-xl border-teal-200 text-sm focus:border-teal-500 focus:ring-teal-500 bg-white font-mono text-teal-900" />
                <p class="text-[10px] text-teal-600 mt-1 leading-tight">Obrigatório para lentes progressivas. Se preenchida, a Altura de Montagem também será exigida.</p>
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Atendente / Responsável *</label>
                <select v-model="form.vendedorId" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required>
                  <option value="">Selecione o Vendedor</option>
                  <option v-for="v in Vendedores" :key="v.id" :value="v.id">{{ v.nome }}</option>
                </select>
              </div>
            </div>
          </div>

          <!-- Bloco 3: Medidas Técnicas -->
          <div class="bg-white p-6 rounded-2xl border border-slate-200 space-y-4">
            <h3 class="text-sm font-black text-slate-700 uppercase tracking-wider flex items-center gap-2">
              <span class="w-2 h-2 rounded-full bg-indigo-500"></span> 3. Medidas Técnicas & Prazo de Entrega
            </h3>
            <div class="grid grid-cols-1 md:grid-cols-4 gap-6">
              <div>
                <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">DNP - Olho Direito (mm) *</label>
                <input v-model.number="form.dnpOd" type="number" step="0.5" min="0.1" placeholder="0.0" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" required />
              </div>
              <div>
                <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">DNP - Olho Esquerdo (mm) *</label>
                <input v-model.number="form.dnpOe" type="number" step="0.5" min="0.1" placeholder="0.0" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" required />
              </div>
              <div>
                <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Altura de Montagem (mm)</label>
                <input v-model.number="form.alturaMontagem" type="number" step="0.5" placeholder="Obrigatório p/ progressivas" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" />
              </div>
              <div>
                <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Data Prevista de Entrega *</label>
                <input v-model="form.dataPrevistaEntrega" type="date" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" required />
              </div>
            </div>
            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Observações da Receita / Laboratório</label>
              <input v-model="form.obsReceita" type="text" placeholder="Ex: Quebrar cantos das lentes, canalar alta miopia" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
            </div>
          </div>

          <!-- Seleção de Produtos + Sub-formulários Rápidos (Seção 6) -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div class="space-y-2">
              <div class="flex items-center justify-between">
                <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider">Armação Selecionada (Estoque Real) *</label>
                <button type="button" @click="exibirSubFormMarca = !exibirSubFormMarca" class="text-[10px] text-teal-600 font-bold hover:underline">
                  ＋ Cadastrar Marca
                </button>
              </div>

              <!-- Atalho: Cadastro Expresso de Marca -->
              <div v-if="exibirSubFormMarca" class="bg-slate-50 p-4 rounded-xl border space-y-2 animate-fadeIn mb-2">
                <p class="text-[10px] font-bold uppercase text-slate-500">Nova Marca Express</p>
                <div class="flex gap-2">
                  <input v-model="formSubMarca.nome" type="text" placeholder="Nome da Marca" class="w-full rounded-lg text-xs border-slate-200 bg-white" />
                  <button type="button" @click="enviarSubMarca" :disabled="formSubMarca.processing" class="bg-slate-950 text-white text-[10px] font-bold px-3 py-1.5 rounded-lg uppercase">Salvar</button>
                </div>
              </div>

              <select v-model="form.armacaoId" @change="processarSnapshotProdutos" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required>
                <option value="">Selecione o modelo do Inventário</option>
                <option v-for="a in Armacoes" :key="a.id" :value="a.id">
                  {{ a.modeloReferencia }} ({{ a.cor }}) — R$ {{ a.precoVenda.toLocaleString('pt-BR') }}
                </option>
              </select>
            </div>

            <div class="space-y-2">
              <div class="flex items-center justify-between">
                <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider">Lente do Catálogo Disponível *</label>
                <button type="button" @click="exibirSubFormLente = !exibirSubFormLente" class="text-[10px] text-teal-600 font-bold hover:underline">
                  ＋ Tabela de Lentes (Base)
                </button>
              </div>

              <!-- Atalho: Cadastro Expresso de Lente Base -->
              <div v-if="exibirSubFormLente" class="bg-slate-50 p-4 rounded-xl border space-y-3 animate-fadeIn mb-2 text-[11px]">
                <p class="text-[10px] font-bold uppercase text-slate-500">Nova Lente Base Express</p>
                <div class="grid grid-cols-2 gap-2">
                  <input v-model="formSubLente.laboratorio" type="text" placeholder="Lab (Ex: Essilor)" class="rounded-lg text-xs border-slate-200 bg-white" />
                  <input v-model="formSubLente.tipo" type="text" placeholder="Design (Ex: Orma)" class="rounded-lg text-xs border-slate-200 bg-white" />
                </div>
                <div class="flex items-center justify-between bg-white p-2 rounded-lg border">
                  <span>Lente Surfaçada?</span>
                  <input type="checkbox" v-model="formSubLente.surfacada" class="rounded border-slate-300" />
                </div>
                <button type="button" @click="enviarSubLente" :disabled="formSubLente.processing" class="w-full bg-slate-950 text-white text-[10px] font-bold py-2 rounded-lg uppercase">
                  Gravar Lente Base
                </button>
              </div>

              <select v-model="form.lenteId" @change="processarSnapshotProdutos" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required>
                <option value="">Selecione a Lente da Matriz</option>
                <option v-for="l in Lentes" :key="l.id" :value="l.id">
                  {{ l.laboratorio }} — {{ l.tipo }} {{ l.tratamento ? `(${l.tratamento})` : '(Sem Tratamento)' }} (Índice {{ l.indiceRefracao }}) — R$ {{ l.precoVenda.toLocaleString('pt-BR') }}
                </option>
              </select>
              
              <div v-if="lenteManualAtiva" class="animate-fadeIn p-4 bg-teal-50/50 rounded-2xl border border-teal-200/60">
                <label class="block text-xs font-bold uppercase text-teal-900 tracking-wider mb-2">Preço de Venda da Lente Surfaçada (Sob Encomenda) *</label>
                <div class="relative mt-1 rounded-xl shadow-sm">
                  <div class="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                    <span class="text-sm font-semibold text-teal-600">R$</span>
                  </div>
                  <input v-model.number="form.valorLente" type="number" step="0.01" min="0" placeholder="0,00" @input="recalcularTotaisGenericos" class="w-full rounded-xl border-teal-200 pl-9 text-sm font-mono font-bold focus:border-teal-500 focus:ring-teal-500" required />
                </div>
              </div>
            </div>
          </div>

          <!-- Resumo Financeiro -->
          <div class="p-5 bg-amber-50/40 rounded-2xl border border-amber-200/60 space-y-4">
            <h4 class="font-bold text-amber-950 uppercase tracking-wider text-[10px]">Resumo do Pedido & Condições de Pagamento</h4>
            <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
              <div>
                <label class="block font-bold text-amber-900 uppercase mb-1.5">Forma Pagamento *</label>
                <select v-model="form.formaPagamento" class="w-full rounded-xl border-amber-200 text-xs bg-white">
                  <option value="DINHEIRO">Dinheiro</option>
                  <option value="PIX">Pix</option>
                  <option value="CARTAO_CREDITO">Cartão de Crédito</option>
                  <option value="CARTAO_DEBITO">Cartão de Débito</option>
                </select>
              </div>
              <div v-if="form.formaPagamento === 'CARTAO_CREDITO'">
                <label class="block font-bold text-amber-900 uppercase mb-1.5">Parcelas *</label>
                <select v-model.number="qtdParcelas" class="w-full rounded-xl border-amber-200 text-xs bg-white">
                  <option value="1">1x à Vista</option>
                  <option v-for="n in [2,3,4,5,6,7,8,9,10,11,12]" :key="n" :value="n">{{ n }}x</option>
                </select>
              </div>
              <div>
                <label class="block font-bold text-amber-900 uppercase mb-1.5">Desconto Percentual (%)</label>
                <input v-model.number="form.descontoPercentual" type="number" min="0" max="100" step="0.1" @input="recalcularTotaisGenericos" class="w-full rounded-xl border-amber-200 text-xs bg-white font-mono font-bold" />
              </div>
              <div>
                <label class="block font-bold text-teal-950 uppercase mb-1.5">Total Líquido do Pedido</label>
                <div class="text-base font-black font-mono text-teal-700 bg-teal-50 px-3 py-2 rounded-xl border border-teal-100 text-center">
                  R$ {{ form.valorTotalLiquido.toLocaleString('pt-BR', { minimumFractionDigits: 2 }) }}
                </div>
              </div>
            </div>
          </div>

          <div class="flex items-center justify-end gap-3 border-t border-slate-100 pt-4">
            <Link href="/ordens" class="px-5 py-3 text-sm font-semibold text-slate-500 hover:text-slate-800 transition">
              Cancelar
            </Link>
            <button type="submit" :disabled="salvandoOS" class="bg-teal-600 hover:bg-teal-700 disabled:bg-slate-200 disabled:text-slate-400 text-white font-bold py-3.5 px-8 rounded-xl shadow-md transition text-sm min-w-[200px]">
              <span v-if="salvandoOS">Processando Emissão...</span>
              <span v-else>Faturar Ordem de Serviço</span>
            </button>
          </div>

        </form>
      </div>

      <!-- TELA 2: COMPROVANTE DE CONSOLIDAÇÃO (SUCESSO) -->
      <div class="bg-white rounded-3xl border border-slate-200 shadow-xl overflow-hidden p-8 space-y-6 no-print text-center" v-else>
        <div class="inline-flex items-center justify-center w-16 h-16 rounded-full bg-teal-100 text-teal-600 text-3xl">✓</div>
        <h2 class="text-2xl font-black text-slate-900">OS Emitida com Sucesso!</h2>
        <p class="text-sm text-slate-500">A Ordem de Serviço foi gravada de forma definitiva no sistema.</p>
        
        <div class="border border-slate-100 rounded-2xl p-6 bg-slate-50/50 grid grid-cols-1 sm:grid-cols-2 gap-4 max-w-2xl mx-auto text-left">
          <div>
            <p class="text-xs text-slate-400 uppercase font-bold tracking-wider">Número da OS</p>
            <p class="text-lg font-black font-mono text-slate-800">{{ osFaturadaResponse.numeroOS }}</p>
          </div>
          <div>
            <p class="text-xs text-slate-400 uppercase font-bold tracking-wider">Cliente</p>
            <p class="text-sm font-bold text-slate-800">{{ form.nome }}</p>
          </div>
        </div>

        <div class="flex flex-col sm:flex-row items-center justify-center gap-4 max-w-xl mx-auto pt-4">
          <button @click="imprimirDocumento('completa')" class="w-full bg-slate-950 hover:bg-slate-800 text-white font-bold py-3.5 px-6 rounded-xl text-xs uppercase tracking-wider transition shadow-md flex items-center justify-center gap-2">
            🖨️ Imprimir OS Completa (A4)
          </button>
        </div>
        <div class="pt-6">
          <button @click="voltarAoPainel" class="text-sm font-bold text-teal-600 hover:underline">
            ← Voltar ao Dashboard Principal
          </button>
        </div>
      </div>

    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { ref, watch } from 'vue'
import { useForm, Link, router } from '@inertiajs/vue3'
import axios from 'axios'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  Vendedores: Array,
  Armacoes: Array,
  Lentes: Array
})

const exibirFaturaSucesso = ref(false)
const tipoComprovanteImpressao = ref('completa')
const osFaturadaResponse = ref({ numeroOS: 'OS-TEMP-00000' })
const salvandoOS = ref(false)
const erroSubmissao = ref(null)
const qtdParcelas = ref(1)
const lenteManualAtiva = ref(false)
const clienteLocalizado = ref(null)
const consultandoCpf = ref(false)
const termoAceito = ref(false)
const carregandoIA = ref(false)
const arquivoSelecionado = ref(null)

const exibirSubFormMarca = ref(false)
const exibirSubFormLente = ref(false)

const formSubMarca = useForm({ nome: '' })
const formSubLente = useForm({ laboratorio: '', tipo: '', surfacada: false })

const form = useForm({
  cpf: '', nome: '', telefone: '', dataNascimento: '', logradouro: '', numero: '', complemento: '',
  bairro: '', cidade: '', estado: '', cep: '', convenio: '', email: '', vendedorId: '',
  dataReceita: new Date().toISOString().split('T')[0], dataPrevistaEntrega: '', medicoNome: '',
  medicoCrm: '', medicoTipo: 'NAO_ESPECIFICADO', observacoes: '', odEsferico: 0, odCilindrico: 0,
  odEixo: 0, oeEsferico: 0, oeCilindrico: 0, oeEixo: 0, adicao: null, dnpOd: 0, dnpOe: 0,
  alturaMontagem: null, obsReceita: '', armacaoId: '', lenteId: '', valorArmacao: 0, valorLente: 0,
  valorTotalBruto: 0, descontoPercentual: 0, descontoReais: 0, valorTotalLiquido: 0, valorEntrada: null,
  formaPagamento: 'DINHEIRO'
})

watch(() => form.formaPagamento, (novaForma) => {
  if (novaForma !== 'CARTAO_CREDITO') qtdParcelas.value = 1
})

const enviarSubMarca = () => {
  if (!formSubMarca.nome.trim()) return
  formSubMarca.post('/armacoes/marcas', {
    preserveScroll: true,
    onSuccess: () => {
      formSubMarca.reset()
      exibirSubFormMarca.value = false
      alert('Nova marca adicionada com sucesso express!')
    }
  })
}

const enviarSubLente = () => {
  if (!formSubLente.laboratorio.trim() || !formSubLente.tipo.trim()) return
  formSubLente.post('/lentes', {
    preserveScroll: true,
    onSuccess: () => {
      formSubLente.reset()
      exibirSubFormLente.value = false
      alert('Nova lente base adicionada ao catálogo express!')
    }
  })
}

const processarSnapshotProdutos = () => {
  const armacao = props.Armacoes.find(a => a.id === form.armacaoId)
  form.valorArmacao = armacao ? armacao.precoVenda : 0

  const lente = props.Lentes.find(l => l.id === form.lenteId)
  if (lente) {
    form.valorLente = lente.precoVenda
    lenteManualAtiva.value = lente.precoVenda === 0
  } else {
    form.valorLente = 0
    lenteManualAtiva.value = false
  }
  recalcularTotaisGenericos()
}

const recalcularTotaisGenericos = () => {
  form.valorTotalBruto = form.valorArmacao + form.valorLente
  const pct = form.descontoPercentual || 0
  form.descontoReais = Number(((form.valorTotalBruto * pct) / 100).toFixed(2))
  form.valorTotalLiquido = form.valorTotalBruto - form.descontoReais
}

const consultarCpfNoBanco = async () => {
  const cpfLimpo = form.cpf.replace(/\D/g, '')
  if (cpfLimpo.length !== 11) return alert('CPF Inválido.')
  consultandoCpf.value = true
  try {
    const resposta = await fetch(`/api/clientes/buscar-cpf/${cpfLimpo}`)
    if (resposta.ok) {
      const dados = await resposta.json()
      Object.assign(form, dados)
      clienteLocalizado.value = true
    } else {
      clienteLocalizado.value = false
    }
  } catch (err) { console.error(err) }
  finally { consultandoCpf.value = false }
}

const buscarEnderecoViaCep = async () => {
  const cepLimpo = form.cep.replace(/\D/g, '')
  if (cepLimpo.length !== 8) return
  try {
    const resposta = await fetch(`https://viacep.com.br/ws/${cepLimpo}/json/`)
    if (resposta.ok) {
      const dados = await resposta.json()
      if (!dados.erro) {
        form.logradouro = dados.logradouro || ''
        form.bairro = dados.bairro || ''
        form.cidade = dados.localidade || ''
        form.estado = dados.uf || ''
      }
    }
  } catch (err) { console.error(err) }
}

const manipularArquivo = (event) => {
  const arquivos = event.target.files
  if (arquivos.length > 0) arquivoSelecionado.value = arquivos[0]
}

const executarOcrInteligente = async () => {
  if (!arquivoSelecionado.value || !termoAceito.value) return
  carregandoIA.value = true
  try {
    const formData = new FormData()
    formData.append('imagemReceita', arquivoSelecionado.value)
    const resposta = await axios.post('/ordens/processar-receita-ia', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    Object.assign(form, resposta.data)
    alert('Leitura da receita concluída!')
  } catch (err) { alert('Erro ao ler a receita.') }
  finally { carregandoIA.value = false }
}

const salvarOrdemServico = async () => {
  erroSubmissao.value = null
  salvandoOS.value = true
  try {
    const query = form.formaPagamento === 'CARTAO_CREDITO' ? `?quantidadeParcelas=${qtdParcelas.value}` : ''
    const payload = { ...form, cpf: form.cpf.replace(/\D/g, ''), lentePrecoId: form.lenteId }
    const { data } = await axios.post(`/ordens${query}`, payload)
    osFaturadaResponse.value = { numeroOS: data.numeroOS }
    exibirFaturaSucesso.value = true
  } catch (err) {
    erroSubmissao.value = err.response?.data?.mensagem || 'Erro ao emitir a Ordem de Serviço.'
  } finally { salvandoOS.value = false }
}

const voltarAoPainel = () => router.get('/ordens')
</script>