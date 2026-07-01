<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-5xl mx-auto">
      
      <!-- Alertas de Erro de Alçada ou Servidor (no-print) -->
      <div v-if="$page.props.flash?.erro" class="p-4 bg-red-50 border border-red-200 text-red-800 rounded-xl text-sm font-semibold shadow-sm no-print">
        🛑 {{ $page.props.flash.erro }}
      </div>

      <!-- TELA PRINCIPAL DO FORMULÁRIO (no-print) -->
      <div v-if="!exibirFaturaSucesso" class="bg-white rounded-3xl border border-slate-200 shadow-xl overflow-hidden no-print">
        
        <div class="bg-slate-950 text-white p-6 flex items-center justify-between">
          <div>
            <h1 class="text-xl font-black tracking-wide">Central Unificada de Emissão de OS</h1>
            <p class="text-xs text-slate-400">Fluxo contínuo: Identificação do cliente, dados clínicos e fechamento financeiro.</p>
          </div>
          <span class="text-xs font-mono bg-teal-500/20 text-teal-400 px-3 py-1 rounded-full border border-teal-500/30">RETSYS CRM v5</span>
        </div>

        <form @submit.prevent="salvarOrdemServico" class="p-6 space-y-8">
          
          <!-- SEÇÃO 1: CRM - IDENTIFICAÇÃO DO CLIENTE (BUSCA INTEGRADA) -->
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

            <!-- Morada Residencial via ViaCEP -->
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

          <!-- SEÇÃO DA IA MOONDREAM (PRESERVADA INTEGRALMENTE) -->
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

          <!-- SEÇÃO 2: DADOS CLÍNICOS DA RECEITA MÉDICA (TABELA EM GRID DE 4 COLUNAS EM DESTAQUE - REVISÃO 01/07) -->
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
                <p class="text-[10px] text-teal-600 mt-1 leading-tight">Obrigatório para lentes progressivas. Grau de perto calculado automaticamente via memória.</p>
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Atendente / Responsável *</label>
                <select v-model="form.vendedorId" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required>
                  <option value="">Selecione o Vendedor</option>
                  <option v-for="v in Vendedores" :key="v.Id" :value="v.Id">{{ v.Nome }}</option>
                </select>
              </div>
            </div>
          </div>

          <!-- SEÇÃO 3: MEDIDAS TÉCNICAS DE LABORATÓRIO -->
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

          <!-- SEÇÃO 4: SELEÇÃO DE PRODUTOS DO ESTOQUE (ARMCÃO E LENTE) -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Armação Selecionada (Estoque Real) *</label>
              <select v-model="form.armacaoId" @change="processarSnapshotProdutos" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required>
                <option value="">Selecione o modelo do Inventário</option>
                <option v-for="a in Armacoes" :key="a.Id" :value="a.Id">
                  {{ a.ModeloReferencia }} - Cor: {{ a.Cor }} (Qtd: {{ a.QuantidadeEstoque }} un) - R$ {{ a.PrecoVenda }}
                </option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Lente de Laboratório Disponível *</label>
              <!-- Dispara a mudança que verifica se é surfaçada de forma integrada -->
              <select v-model="form.lenteId" @change="carregarOpcoesLente" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required>
                <option value="">Selecione o Bloco de Lente Homologado</option>
                <option v-for="l in Lentes" :key="l.Id" :value="l.Id">
                  [{{ l.Laboratorio }}] {{ l.Tipo }} {{ l.Surfacada ? ' (SURFAÇADA) ⚙️' : '' }}
                </option>
              </select>
            </div>
          </div>

          <!-- SEÇÃO: MATRIZ DE PREÇOS DINÂMICA DA LENTE (EXIGÊNCIA 01/07 - Aparece apenas se não for surfaçada) -->
          <div v-if="form.lenteId && !lenteSurfacada" class="grid grid-cols-1 md:grid-cols-3 gap-4 bg-slate-50 p-4 rounded-xl border border-slate-200">
            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 mb-1">Tipo de Lente *</label>
              <select v-model="form.lenteTipo" @change="obterPrecoLenteDinamico" class="w-full rounded-xl border-slate-200 text-xs focus:ring-teal-500" required>
                <option value="">Selecione Tipo</option>
                <option value="MONOFOCAL">Monofocal</option>
                <option value="BIFOCAL">Bifocal</option>
                <option value="PROGRESSIVA">Progressiva</option>
                <option value="OCUPACIONAL">Ocupacional</option>
              </select>
            </div>

            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 mb-1">Índice de Refração *</label>
              <select v-model="form.lenteIndiceRefracao" @change="obterPrecoLenteDinamico" class="w-full rounded-xl border-slate-200 text-xs focus:ring-teal-500" required>
                <option value="">Selecione Índice</option>
                <option v-for="ind in indicesDisponiveis" :key="ind" :value="ind">{{ ind }}</option>
              </select>
            </div>

            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 mb-1">Tratamento Opcional</label>
              <select v-model="form.tratamentoId" @change="obterPrecoLenteDinamico" class="w-full rounded-xl border-slate-200 text-xs focus:ring-teal-500">
                <option :value="null">Sem Tratamento</option>
                <option v-for="t in tratamentos" :key="t.Id" :value="t.Id">
                  {{ t.Nome }} (+ R$ {{ t.AcrescimoValor }})
                </option>
              </select>
            </div>
          </div>

          <!-- BADGE VISUAL DE IDENTIFICAÇÃO DE LENTE SURFAÇADA (EXIGÊNCIA 01/07) -->
          <div v-if="lenteSurfacada" class="p-3.5 bg-amber-50 border border-amber-200 text-amber-800 rounded-xl text-xs font-bold flex items-center gap-2">
            <span>⚙️ Lente Surfaçada Sob Medida Detectada. O preço da lente é editável livremente no fechamento.</span>
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

          <!-- BLOCO FINANCEIRO COM DESCONTO EM % ATIVO E LENTE EDITÁVEL SE FOR SURFAÇADA -->
          <div class="bg-teal-50/30 border border-teal-100 p-6 rounded-2xl grid grid-cols-1 sm:grid-cols-4 gap-4">
            <div>
              <label class="block text-[11px] font-bold uppercase text-teal-800 tracking-wider mb-1.5">Total Bruto da Venda (R$)</label>
              <input v-model.number="form.valorTotalBruto" type="number" step="0.01" class="w-full rounded-xl border-teal-200 bg-slate-100 font-bold text-slate-600 font-mono" readonly />
            </div>
            <div>
              <label class="block text-[11px] font-bold uppercase text-teal-800 tracking-wider mb-1.5">Desconto Percentual (%) *</label>
              <input v-model.number="form.descontoPercentual" type="number" step="0.1" @input="recalcularDescontoPorPorcentagem" placeholder="0%" class="w-full rounded-xl border-teal-200 text-sm font-black text-slate-900 focus:border-teal-500 focus:ring-teal-500 font-mono" required />
            </div>
            <div>
              <label class="block text-[11px] font-bold uppercase text-teal-800 tracking-wider mb-1.5">Desconto Concedido (R$)</label>
              <input v-model.number="form.descontoReais" type="number" step="0.01" class="w-full rounded-xl border-teal-200 bg-slate-100 font-bold text-slate-600 font-mono" readonly />
            </div>
            <div>
              <label class="block text-[11px] font-bold uppercase text-teal-800 tracking-wider mb-1.5">Valor Líquido Cobrado (R$)</label>
              <input v-model.number="form.valorTotalLiquido" type="number" step="0.01" class="w-full rounded-xl border-teal-200 text-base font-black text-teal-700 bg-teal-50/70 font-mono" readonly />
            </div>
          </div>

          <!-- ENTRADA DE SINAL, SELEÇÃO DE PARCELAS DO CRÉDITO E EDIÇÃO DE LENTES SURFAÇADAS -->
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 items-center">
            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Sinal / Valor de Entrada (Se houver)</label>
              <input v-model.number="form.valorEntrada" type="number" step="0.01" placeholder="0,00" class="w-full rounded-xl border-slate-200 text-sm font-semibold text-slate-700 focus:border-teal-500 focus:ring-teal-500 font-mono" />
            </div>

            <!-- VALOR DA LENTE: Só fica editável se a lente selecionada for Surfaçada (EXIGÊNCIA 01/07) -->
            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Valor Unitário da Lente (R$) *</label>
              <input 
                v-model.number="form.valorLente" 
                type="number" 
                step="0.01" 
                @input="recalcularPrecosManuaisLente"
                :readonly="!lenteSurfacada"
                :class="[!lenteSurfacada ? 'bg-slate-100 text-slate-600' : 'bg-white text-slate-950 font-black border-amber-300 focus:border-amber-500 focus:ring-amber-500']"
                class="w-full rounded-xl border-slate-200 text-sm font-mono" 
                required 
              />
            </div>

            <div v-if="form.formaPagamento === 'CARTAO_CREDITO'">
              <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Parcelas (Crédito) *</label>
              <select v-model.number="qtdParcelas" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" required>
                <option value="1">1x - À Vista Rotativo</option>
                <option v-for="n in [2,3,4,5,6,7,8,9,10,11,12]" :key="n" :value="n">{{ n }}x</option>
              </select>
            </div>
          </div>

          <div>
            <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Observações Gerais Internas</label>
            <input v-model="form.observacoes" type="text" placeholder="Anotações comerciais e prazos especiais do balcão..." class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
          </div>

          <div class="flex items-center justify-end gap-3 border-t border-slate-100 pt-4">
            <Link href="/ordens" class="px-5 py-3 text-sm font-semibold text-slate-500 hover:text-slate-800 transition">
              Cancelar
            </Link>
            <button type="submit" :disabled="form.processing" class="bg-teal-600 hover:bg-teal-700 disabled:bg-slate-200 disabled:text-slate-400 text-white font-bold py-3.5 px-8 rounded-xl shadow-md transition text-sm min-w-[200px]">
              <span v-if="form.processing">Processando Emissão...</span>
              <span v-else>Faturar Ordem de Serviço</span>
            </button>
          </div>

        </form>
      </div>

      <!-- TELA DE SUCESSO DE EMISSÃO: OPÇÕES DE IMPRESSÃO A4 (EXIGÊNCIA 01/07 e VIA DO CLIENTE) -->
      <div v-else class="bg-white rounded-3xl border border-slate-200 shadow-xl overflow-hidden p-8 space-y-6 no-print text-center">
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

        <!-- Botões de Impressão Direta A4 -->
        <div class="flex flex-col sm:flex-row items-center justify-center gap-4 max-w-xl mx-auto pt-4">
          <button @click="imprimirDocumento('completa')" class="w-full bg-slate-950 hover:bg-slate-800 text-white font-bold py-3.5 px-6 rounded-xl text-xs uppercase tracking-wider transition shadow-md flex items-center justify-center gap-2">
            🖨️ Imprimir OS Completa (A4)
          </button>
          <button @click="imprimirDocumento('cliente')" class="w-full bg-teal-600 hover:bg-teal-700 text-white font-bold py-3.5 px-6 rounded-xl text-xs uppercase tracking-wider transition shadow-md flex items-center justify-center gap-2">
            👤 Imprimir Via Cliente (A4)
          </button>
        </div>

        <div class="pt-6">
          <button @click="voltarAoPainel" class="text-sm font-bold text-teal-600 hover:underline">
            ← Voltar ao Dashboard Principal
          </button>
        </div>
      </div>

    </div>

    <!-- DOCUMENTOS FÍSICOS OCULTOS NO ECRÃ - SÓ APARECEM NA IMPRESSÃO FÍSICA (A4) -->
    <div id="documento-impressao" class="hidden print:block print-area">
      
      <!-- DOCUMENTO 1: OS COMPLETA (INTERNA) -->
      <div v-if="tipoComprovanteImpressao === 'completa'" class="space-y-6">
        <div class="print-header flex items-center justify-between border-b-2 border-black pb-4">
          <div>
            <h1 class="print-title font-black text-xl">RETSYS - ORDEM DE SERVIÇO</h1>
            <p class="text-xs">Data de Emissão: {{ new Date().toLocaleDateString('pt-BR') }}</p>
          </div>
          <div class="text-right font-mono">
            <h2 class="text-xl font-bold">{{ osFaturadaResponse.numeroOS }}</h2>
            <p class="text-xs">Entrega Prevista: {{ formatarDataA4(form.dataPrevistaEntrega) }}</p>
          </div>
        </div>

        <div class="space-y-4">
          <h3 class="print-section-title font-bold bg-slate-100 p-1 text-xs uppercase">Dados do Cliente</h3>
          <p><strong>Nome:</strong> {{ form.nome }} | <strong>CPF:</strong> {{ form.cpf }}</p>
          <p><strong>WhatsApp:</strong> {{ form.telefone }} | <strong>Endereço:</strong> {{ form.logradouro }}, {{ form.numero }} - {{ form.bairro }} (CEP: {{ form.cep }})</p>

          <h3 class="print-section-title font-bold bg-slate-100 p-1 text-xs uppercase">Dados Clínicos / Receita</h3>
          <p><strong>Médico:</strong> {{ form.medicoNome }} ({{ form.medicoTipo }}) | <strong>CRM:</strong> {{ form.medicoCrm || 'Não informado' }}</p>
          
          <table class="table-print w-full text-center border-collapse border mt-2">
            <thead>
              <tr class="bg-slate-50 border-b">
                <th class="p-2 border">Olho</th>
                <th class="p-2 border">Esférico</th>
                <th class="p-2 border">Cilíndrico</th>
                <th class="p-2 border">Eixo</th>
              </tr>
            </thead>
            <tbody>
              <tr class="border-b">
                <td class="p-2 border"><strong>OD</strong></td>
                <td class="p-2 border">{{ form.odEsferico || '0,00' }}</td>
                <td class="p-2 border">{{ form.odCilindrico || '0,00' }}</td>
                <td class="p-2 border">{{ form.odEixo || '0' }}°</td>
              </tr>
              <tr class="border-b">
                <td class="p-2 border"><strong>OE</strong></td>
                <td class="p-2 border">{{ form.oeEsferico || '0,00' }}</td>
                <td class="p-2 border">{{ form.oeCilindrico || '0,00' }}</td>
                <td class="p-2 border">{{ form.oeEixo || '0' }}°</td>
              </tr>
            </tbody>
          </table>

          <p class="mt-2"><strong>Adição:</strong> {{ form.adicao || 'N/A' }} | <strong>DNP OD:</strong> {{ form.dnpOd }}mm | <strong>DNP OE:</strong> {{ form.dnpOe }}mm</p>

          <h3 class="print-section-title font-bold bg-slate-100 p-1 text-xs uppercase">Valores e Fechamento</h3>
          <p><strong>Forma de Pagamento:</strong> {{ form.formaPagamento }} {{ form.formaPagamento === 'CARTAO_CREDITO' ? `(${qtdParcelas}x)` : '' }}</p>
          <p><strong>Valor Total Bruto:</strong> R$ {{ form.valorTotalBruto.toFixed(2) }}</p>
          <p><strong>Desconto Concedido:</strong> {{ form.descontoPercentual }}% (R$ {{ form.descontoReais.toFixed(2) }})</p>
          <p class="text-lg"><strong>Valor Líquido Cobrado:</strong> R$ {{ form.valorTotalLiquido.toFixed(2) }}</p>
        </div>

        <div class="pt-16 text-center">
          <div class="print-signature-area border-t border-dashed border-black w-64 mx-auto pt-2 text-center text-xs">
            Assinatura do Cliente
          </div>
        </div>
      </div>

      <!-- DOCUMENTO 2: VIA DO CLIENTE COMPACTADA (REVISÃO 01/07) -->
      <div v-if="tipoComprovanteImpressao === 'cliente'" class="space-y-6">
        <div class="print-header text-center border-b-2 border-black pb-4">
          <h1 class="print-title font-black text-xl">RETSYS ÓTICA</h1>
          <p class="text-sm font-bold">Via do Cliente (Comprovante Simplificado)</p>
        </div>

        <div class="space-y-4 text-base">
          <p><strong>OS N°:</strong> <span class="font-mono font-bold text-lg">{{ osFaturadaResponse.numeroOS }}</span></p>
          <p><strong>Cliente:</strong> {{ form.nome }}</p>
          <p><strong>Data de Compra:</strong> {{ new Date().toLocaleDateString('pt-BR') }}</p>
          <p><strong>Entrega Prevista:</strong> {{ formatarDataA4(form.dataPrevistaEntrega) }}</p>

          <div class="border-t border-b border-black py-4 space-y-2">
            <p><strong>Valor Total Bruto:</strong> R$ {{ form.valorTotalBruto.toFixed(2) }}</p>
            <p><strong>Desconto Aplicado:</strong> {{ form.descontoPercentual }}% (R$ {{ form.descontoReais.toFixed(2) }})</p>
            <p class="text-lg font-black"><strong>Valor Cobrado:</strong> R$ {{ form.valorTotalLiquido.toFixed(2) }}</p>
          </div>

          <p><strong>Forma de Pagamento:</strong> {{ form.formaPagamento }} {{ form.formaPagamento === 'CARTAO_CREDITO' ? `(${qtdParcelas}x)` : '' }}</p>
        </div>

        <div class="pt-24">
          <div class="print-signature-area border-t border-dashed border-black w-64 mx-auto pt-2 text-center text-xs">
            Assinatura do Cliente
          </div>
        </div>
      </div>

    </div>

  </AuthenticatedLayout>
</template>

<script setup>
import { ref, onMounted, nextTick } from 'vue'
import { useForm, Link, router } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  Vendedores: Array,
  Armacoes: Array,
  Lentes: Array
})

// Controladores de fluxo e modais de faturamento
const exibirFaturaSucesso = ref(false)
const tipoComprovanteImpressao = ref('completa')
const osFaturadaResponse = ref({ numeroOS: 'OS-TEMP-00000' })

const qtdParcelas = ref(1)
const tratamentos = ref([])
const indicesDisponiveis = ref([1.50, 1.56, 1.67, 1.74])
const lenteSurfacada = ref(false)

// Estados da IA Moondream (Preservada)
const termoAceito = ref(false)
const carregandoIA = ref(false)
const arquivoSelecionado = ref(null)

const form = useForm({
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
  
  vendedorId: '',
  dataReceita: new Date().toISOString().split('T')[0],
  dataPrevistaEntrega: '',
  medicoNome: '',
  medicoCrm: '',
  medicoTipo: 'NAO_ESPECIFICADO',
  observacoes: '',

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

  armacaoId: '',
  lenteId: '',
  // Novos campos para precificação inteligente (01/07)
  lenteTipo: '',
  lenteIndiceRefracao: '',
  tratamentoId: null,

  valorArmacao: 0,
  valorLente: 0,
  valorTotalBruto: 0,
  descontoPercentual: 0,
  descontoReais: 0,
  valorTotalLiquido: 0,
  valorEntrada: null,
  formaPagamento: 'DINHEIRO'
})

// Busca de tratamentos disponíveis no início (01/07)
onMounted(async () => {
  try {
    const res = await fetch('/api/lentes/tratamentos')
    if (res.ok) {
      tratamentos.value = await res.json()
    }
  } catch (err) {
    console.error('Falha ao carregar tratamentos.')
  }
})

// Detecta se a lente é surfaçada e busca as opções da matriz (01/07)
const carregarOpcoesLente = async () => {
  const lente = props.Lentes.find(l => l.Id === form.lenteId)
  if (!lente) {
    lenteSurfacada.value = false
    return
  }

  // Verifica a flag na tabela de lentes
  lenteSurfacada.value = lente.Surfacada

  if (lente.Surfacada) {
    form.valorLente = 0.00
    form.lenteTipo = 'SURFAÇADA'
    form.lenteIndiceRefracao = '1.50'
    form.tratamentoId = null
    recalcularTotaisGenericos()
  } else {
    // Busca os tipos e índices mapeados para essa lente específica na matriz de preços
    try {
      const res = await fetch(`/api/lentes/${form.lenteId}/opcoes-matriz`)
      if (res.ok) {
        const dados = await res.json()
        indicesDisponiveis.value = [...new Set(dados.map(d => d.indiceRefracao))]
      }
    } catch (e) {
      console.error(e)
    }
  }
}

// Consulta inteligente ao endpoint do C# para matriz de preços (01/07)
const obterPrecoLenteDinamico = async () => {
  if (!form.lenteId || !form.lenteTipo || !form.lenteIndiceRefracao) return

  try {
    const query = `lenteId=${form.lenteId}&tipo=${form.lenteTipo}&indiceRefracao=${form.lenteIndiceRefracao}` +
                  (form.tratamentoId ? `&tratamentoId=${form.tratamentoId}` : '')
    const res = await fetch(`/api/lentes/calcular-preco?${query}`)
    if (res.ok) {
      const d = await res.json()
      form.valorLente = d.precoVenda
      recalcularTotaisGenericos()
    }
  } catch (err) {
    console.error(err)
  }
}

// Método clássico para congelar snapshot mantendo a compatibilidade do seu estoque
const processarSnapshotProdutos = () => {
  const armacao = props.Armacoes.find(a => a.Id === form.armacaoId)
  form.valorArmacao = armacao ? armacao.PrecoVenda : 0
  recalcularTotaisGenericos()
}

// Calcula os totais com base na digitação da lente surfaçada
const recalcularPrecosManuaisLente = () => {
  recalcularTotaisGenericos()
}

// Inversão lógica comercial baseada na digitação direta de porcentagem
const recalcularDescontoPorPorcentagem = () => {
  recalcularTotaisGenericos()
}

const recalcularTotaisGenericos = () => {
  const armacao = props.Armacoes.find(a => a.Id === form.armacaoId)
  form.valorArmacao = armacao ? armacao.PrecoVenda : 0

  form.valorTotalBruto = form.valorArmacao + form.valorLente

  const pct = form.descontoPercentual || 0
  form.descontoReais = Number(((form.valorTotalBruto * pct) / 100).toFixed(2))
  form.valorTotalLiquido = Number((form.valorTotalBruto - form.descontoReais).toFixed(2))
}

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
    console.error('CPF não localizado no CRM. Seguir com preenchimento manual.')
  }
}

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
    console.error('Falha de conexão com o ViaCEP.')
  }
}

const manipularArquivo = (event) => {
  const arquivos = event.target.files
  if (arquivos.length > 0) {
    arquivoSelecionado.value = arquivos[0]
  }
}

// Moondream Vision Inteligência Artificial (Preservada com sucesso)
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
      const d = await resposta.json()
      form.medicoNome = d.medicoNome || ''
      form.odEsferico = d.odEsferico ?? 0
      form.odCilindrico = d.odCilindrico ?? 0
      form.odEixo = d.odEixo ?? 0
      form.oeEsferico = d.oeEsferico ?? 0
      form.oeCilindrico = d.oeCilindrico ?? 0
      form.oeEixo = d.oeEixo ?? 0
      form.adicao = d.adicao ?? null
      alert('Graus extraídos pela IA! Revise antes de faturar.')
    } else {
      alert('Falha na decodificação da imagem. Siga manualmente.')
    }
  } catch (erro) {
    console.error(erro)
  } finally {
    carregandoIA.value = false
  }
}

const salvarOrdemServico = async () => {
  const p = form.formaPagamento === 'CARTAO_CREDITO' ? qtdParcelas.value : ''
  
  // POST local via Fetch para interceptar o faturamento e exibir o painel de impressão
  try {
    const response = await fetch(`/ordens?quantidadeParcelas=${p}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(form)
    })

    if (response.ok) {
      const r = await response.json()
      osFaturadaResponse.value = r
      exibirFaturaSucesso.value = true
    } else {
      alert('Erro de validação ou faturamento no servidor.')
    }
  } catch (err) {
    console.error(err)
  }
}

const imprimirDocumento = async (tipo) => {
  tipoComprovanteImpressao.value = tipo
  await nextTick()
  window.print()
}

const voltarAoPainel = () => {
  router.get('/dashboard')
}

const formatarDataA4 = (dataString) => {
  if (!dataString) return '--/--/----'
  const partes = dataString.split('-')
  return `${partes[2]}/${partes[1]}/${partes[0]}`
}
</script>