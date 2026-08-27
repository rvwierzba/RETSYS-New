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

      <!-- Aviso de Rascunho Restaurado -->
      <div v-if="rascunhoRestaurado" class="p-3 bg-teal-50 border border-teal-200 text-teal-800 rounded-xl text-xs font-semibold flex items-center justify-between no-print animate-fadeIn">
        <span>💾 Rascunho não finalizado recuperado automaticamente! Seus dados digitados foram preservados.</span>
        <button @click="limparRascunhoManual" class="text-[10px] uppercase font-bold text-teal-900 underline hover:text-teal-950">
          Descartar Rascunho
        </button>
      </div>

      <!-- FORMULÁRIO OPERACIONAL DE EMISSÃO -->
      <div v-if="!exibirFaturaSucesso" class="bg-white rounded-3xl border border-slate-200 shadow-xl overflow-hidden no-print">

        <div class="bg-slate-950 text-white p-6 flex items-center justify-between">
          <div>
            <h1 class="text-xl font-black tracking-wide">Central Unificada de Emissão de OS</h1>
            <p class="text-xs text-slate-400">Fluxo flexível: Identificação do cliente, dados clínicos e fechamento financeiro.</p>
          </div>
          <span class="text-xs font-mono bg-teal-500/20 text-teal-400 px-3 py-1 rounded-full border border-teal-500/30">RETSYS CRM v5</span>
        </div>

        <form @submit.prevent="salvarOrdemServico" @keydown.enter.prevent class="p-6 space-y-8">

          <!-- 1. Identificação do Cliente (CRM) -->
          <div class="bg-slate-50 p-6 rounded-2xl border border-slate-200 space-y-4">
            <h3 class="text-sm font-black text-slate-700 uppercase tracking-wider flex items-center gap-2">
              <span class="w-2 h-2 rounded-full bg-slate-950"></span> 1. Identificação do Cliente (CRM)
            </h3>

            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 items-end">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">CPF do Cliente *</label>
                <div class="flex gap-2">
                  <input
                    v-model="form.cpf"
                    type="text"
                    placeholder="Apenas os 11 números"
                    maxlength="11"
                    @input="form.cpf = form.cpf.replace(/\D/g, '').slice(0, 11)"
                    @keydown.enter.prevent
                    class="w-full rounded-xl border-slate-200 text-sm font-mono focus:border-teal-500 focus:ring-teal-500"
                    required
                  />
                  <button type="button" @click="consultarCpfNoBanco" :disabled="consultandoCpf" class="bg-slate-950 hover:bg-slate-800 disabled:bg-slate-400 text-white px-4 py-2.5 rounded-xl text-xs font-bold transition whitespace-nowrap">
                    {{ consultandoCpf ? 'Buscando...' : 'Buscar CPF' }}
                  </button>
                </div>
              </div>

              <div class="md:col-span-2">
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Nome Completo *</label>
                <input v-model="form.nome" type="text" placeholder="Nome do Paciente" @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
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
                <label class="block text-[11px] font-bold uppercase text-teal-700 tracking-wider mb-1.5">Emissão da OS (Hoje) 🔒</label>
                <input
                  :value="dataEmissaoHoje"
                  type="date"
                  disabled
                  class="w-full rounded-xl border-slate-200 text-sm font-mono font-bold bg-slate-100 text-slate-600 cursor-not-allowed"
                  title="Data de emissão definida automaticamente com a data de hoje"
                />
              </div>

              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">WhatsApp / Telefone *</label>
                <input v-model="form.telefone" type="text" placeholder="(00) 00000-0000" @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
              </div>

              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Data de Nascimento</label>
                <input v-model="form.dataNascimento" type="date" @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>

              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Convênio / Plano Óptico</label>
                <input v-model="form.convenio" type="text" placeholder="Particular, Porto Seguro, etc." @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-4 gap-4 pt-2 border-t border-slate-200/60">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">
                  CEP Residência <span v-if="buscandoCep" class="text-teal-600 animate-pulse">(buscando...)</span>
                </label>
                <input
                  v-model="form.cep"
                  type="text"
                  @input="tratarDigitacaoCep"
                  @blur="buscarEnderecoViaCep"
                  @keydown.enter.prevent
                  placeholder="00000-000"
                  maxlength="9"
                  class="w-full rounded-xl border-slate-200 text-sm font-mono focus:border-teal-500 focus:ring-teal-500"
                />
              </div>

              <div class="md:col-span-2">
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Logradouro</label>
                <input v-model="form.logradouro" type="text" placeholder="Rua / Avenida" @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>

              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Número</label>
                <input v-model="form.numero" type="text" placeholder="Nº" @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Complemento</label>
                <input v-model="form.complemento" type="text" placeholder="Apto, Bloco, etc." @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>

              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Bairro</label>
                <input v-model="form.bairro" type="text" @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>

              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Cidade</label>
                <input v-model="form.cidade" type="text" @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>

              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Estado (UF)</label>
                <input v-model="form.estado" type="text" maxlength="2" placeholder="EX: SP" @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm uppercase text-center focus:border-teal-500 focus:ring-teal-500" />
              </div>
            </div>

            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">E-mail do Cliente</label>
              <input v-model="form.email" type="email" placeholder="cliente@provedor.com" @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
            </div>
          </div>

          <!-- Anexar foto da receita -->
          <div class="bg-amber-50/50 p-6 rounded-2xl border border-amber-200 space-y-3">
            <h3 class="text-xs font-black text-amber-950 uppercase tracking-wider flex items-center gap-2">
              <span>📸 Anexar Foto/Scan da Receita na OS</span>
            </h3>

            <p class="text-[11px] text-amber-700">Anexe a imagem da receita para salvar no histórico do cliente e da OS (opcional e independente da IA).</p>

            <div class="flex items-center gap-3 pt-1">
              <input type="file" id="fotoReceitaAnexo" accept="image/*" @change="vincularFotoReceitaDireta" class="hidden" />

              <label for="fotoReceitaAnexo" class="bg-amber-600 hover:bg-amber-700 text-white text-xs font-bold px-4 py-2.5 rounded-xl transition cursor-pointer shadow-sm select-none">
                {{ fotoAnexaArquivo ? 'Alterar Foto Anexada' : '📎 Selecionar Imagem da Receita' }}
              </label>

              <span class="text-xs font-mono text-amber-900 truncate">
                {{ fotoAnexaArquivo ? fotoAnexaArquivo.name : 'Nenhuma imagem anexada' }}
              </span>
            </div>
          </div>

          <!-- Assistente de IA -->
          <div class="bg-white rounded-2xl border-2 border-dashed border-slate-200 p-6 space-y-4">
            <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2">
              <div>
                <h3 class="text-sm font-black text-slate-950 uppercase tracking-wider flex items-center gap-2">
                  <span class="w-2.5 h-2.5 rounded-full bg-teal-500 animate-pulse"></span>
                  Assistente de Leitura por IA (Moondream)
                </h3>

                <p class="text-xs text-slate-400 mt-0.5">Opcional: Preenche os graus automaticamente analisando a foto.</p>
              </div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 pt-2">
              <div class="space-y-3">
                <div class="flex items-center gap-3">
                  <input type="file" id="fotoReceitaIa" accept="image/*" @change="manipularArquivoIa" class="hidden" />

                  <label for="fotoReceitaIa" class="bg-slate-900 hover:bg-slate-800 text-white text-xs font-bold px-4 py-3 rounded-xl transition cursor-pointer shadow-sm active:scale-95 select-none">
                    {{ arquivoIaSelecionado ? 'Alterar Imagem IA' : 'Selecionar Foto para Leitura' }}
                  </label>

                  <span class="text-xs font-mono text-slate-500 truncate block max-w-[200px]">
                    {{ arquivoIaSelecionado ? arquivoIaSelecionado.name : 'Nenhum arquivo para IA' }}
                  </span>
                </div>

                <div class="flex items-start gap-2">
                  <input type="checkbox" id="termoOcr" v-model="termoAceito" class="mt-0.5 rounded border-slate-300 text-teal-600 focus:ring-teal-500" />

                  <label for="termoOcr" class="text-[11px] text-slate-500 leading-tight cursor-pointer select-none">
                    Confirmo que revisarei os graus após a leitura.
                  </label>
                </div>
              </div>

              <div class="flex items-end justify-start md:justify-end">
                <button type="button" @click="executarOcrInteligente" :disabled="!termoAceito || !arquivoIaSelecionado || carregandoIA" class="w-full sm:w-auto bg-teal-600 hover:bg-teal-700 disabled:bg-slate-100 text-white disabled:text-slate-400 font-bold py-3 px-6 rounded-xl text-xs uppercase tracking-wider transition shadow-sm flex items-center justify-center gap-2">
                  <span v-if="carregandoIA" class="animate-pulse">Analisando...</span>
                  <span v-else>Iniciar Leitura Digital</span>
                </button>
              </div>
            </div>
          </div>

          <!-- 2. Dados Clínicos da Receita Médica -->
          <div class="bg-slate-50 p-6 rounded-2xl border border-slate-200 space-y-4">
            <h3 class="text-sm font-black text-slate-700 uppercase tracking-wider flex items-center gap-2">
              <span class="w-2 h-2 rounded-full bg-teal-500"></span> 2. Dados Clínicos da Receita Médica (Opcional se sem grau)
            </h3>

            <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
              <div class="md:col-span-2">
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Médico Responsável</label>
                <input v-model="form.medicoNome" type="text" placeholder="Dr. Nome do Profissional" @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>

              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">CRM / Registro</label>
                <input v-model="form.medicoCrm" type="text" placeholder="000000-UF" @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>

              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Tipo de Profissional</label>
                <select v-model="form.medicoTipo" @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500">
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
                <div>Cilíndrico (-)</div>
                <div>Eixo (0° a 180°)</div>
              </div>

              <div class="grid grid-cols-4 gap-4 items-center">
                <div class="text-sm font-black text-slate-700 text-center">OD</div>

                <input v-model.number="form.odEsferico" type="number" step="0.25" placeholder="0,00" @keydown.enter.prevent class="rounded-xl border-slate-200 text-sm text-center font-mono focus:border-teal-500" />

                <input v-model.number="form.odCilindrico" type="number" step="0.25" max="0" placeholder="-0,00" @input="validarCilindrico('odCilindrico')" @keydown.enter.prevent class="rounded-xl border-slate-200 text-sm text-center font-mono text-amber-700 font-bold focus:border-teal-500" />

                <input v-model.number="form.odEixo" type="number" min="0" max="180" step="1" placeholder="0" @input="validarEixo('odEixo')" @keydown.enter.prevent class="rounded-xl border-slate-200 text-sm text-center font-mono focus:border-teal-500" />
              </div>

              <div class="grid grid-cols-4 gap-4 items-center">
                <div class="text-sm font-black text-slate-700 text-center">OE</div>

                <input v-model.number="form.oeEsferico" type="number" step="0.25" placeholder="0,00" @keydown.enter.prevent class="rounded-xl border-slate-200 text-sm text-center font-mono focus:border-teal-500" />

                <input v-model.number="form.oeCilindrico" type="number" step="0.25" max="0" placeholder="-0,00" @input="validarCilindrico('oeCilindrico')" @keydown.enter.prevent class="rounded-xl border-slate-200 text-sm text-center font-mono text-amber-700 font-bold focus:border-teal-500" />

                <input v-model.number="form.oeEixo" type="number" min="0" max="180" step="1" placeholder="0" @input="validarEixo('oeEixo')" @keydown.enter.prevent class="rounded-xl border-slate-200 text-sm text-center font-mono focus:border-teal-500" />
              </div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 items-center pt-2">
              <div class="flex flex-col bg-teal-50/50 p-4 rounded-xl border border-teal-100">
                <label class="block text-xs font-bold uppercase text-teal-800 tracking-wider mb-1.5">
                  Adição (AD) <span class="text-[10px] text-teal-600">(Máx +3.50)</span>
                </label>

                <input v-model.number="form.adicao" type="number" step="0.25" min="0" max="3.5" placeholder="0.00" @input="validarAdicao" @keydown.enter.prevent class="w-full rounded-xl border-teal-200 text-sm focus:border-teal-500 focus:ring-teal-500 bg-white font-mono text-teal-900 font-bold" />
              </div>

              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Atendente / Responsável *</label>

                <select v-model="form.vendedorId" @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required>
                  <option value="">Selecione o Vendedor</option>
                  <option v-for="v in (Vendedores ?? vendedores)" :key="v.id || v.Id" :value="v.id || v.Id">
                    {{ v.nome || v.Nome }}
                  </option>
                </select>
              </div>
            </div>
          </div>

          <!-- 3. Medidas Técnicas & Montagem da Armação -->
          <div class="bg-white p-6 rounded-2xl border border-slate-200 space-y-4">
            <h3 class="text-sm font-black text-slate-700 uppercase tracking-wider flex items-center gap-2">
              <span class="w-2 h-2 rounded-full bg-indigo-500"></span> 3. Medidas Técnicas & Montagem da Armação
            </h3>

            <div class="grid grid-cols-1 md:grid-cols-5 gap-4">
              <div>
                <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">DNP OD (20 a 40 mm)</label>
                <input v-model="form.dnpOd" type="text" placeholder="30.0" @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 font-mono font-bold text-slate-800" />
              </div>

              <div>
                <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">DNP OE (20 a 40 mm)</label>
                <input v-model="form.dnpOe" type="text" placeholder="30.0" @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 font-mono font-bold text-slate-800" />
              </div>

              <div>
                <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Altura Mont. OD (mm)</label>
                <input v-model="form.alturaMontagemOd" type="text" placeholder="Ex: 18.0" @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 font-mono font-bold text-slate-800" />
              </div>

              <div>
                <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Altura Mont. OE (mm)</label>
                <input v-model="form.alturaMontagemOe" type="text" placeholder="Ex: 18.0" @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 font-mono font-bold text-slate-800" />
              </div>

              <div>
                <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Data Prevista de Entrega *</label>
                <input v-model="form.dataPrevistaEntrega" type="date" @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 font-mono" required />
              </div>
            </div>

            <div class="p-4 bg-indigo-50/40 rounded-xl border border-indigo-100 space-y-3">
              <span class="text-xs font-black uppercase text-indigo-900 tracking-wider block">📐 Medidas Físicas da Armação (Digitação Livre)</span>

              <div class="grid grid-cols-2 md:grid-cols-6 gap-3">
                <div>
                  <label class="block text-[10px] font-bold uppercase text-indigo-700 tracking-wider mb-1">ARO (Máx 80)</label>
                  <input v-model="form.aro" type="text" placeholder="Ex: 52.0" @keydown.enter.prevent class="w-full rounded-xl border-indigo-200 text-xs text-center font-mono font-bold bg-white" />
                </div>

                <div>
                  <label class="block text-[10px] font-bold uppercase text-indigo-700 tracking-wider mb-1">DM (Máx 80)</label>
                  <input v-model="form.dm" type="text" placeholder="Ex: 55.0" @keydown.enter.prevent class="w-full rounded-xl border-indigo-200 text-xs text-center font-mono font-bold bg-white" />
                </div>

                <div>
                  <label class="block text-[10px] font-bold uppercase text-indigo-700 tracking-wider mb-1">VERT (Máx 80)</label>
                  <input v-model="form.vert" type="text" placeholder="Ex: 40.0" @keydown.enter.prevent class="w-full rounded-xl border-indigo-200 text-xs text-center font-mono font-bold bg-white" />
                </div>

                <div>
                  <label class="block text-[10px] font-bold uppercase text-indigo-700 tracking-wider mb-1">PO (Máx 25)</label>
                  <input v-model="form.po" type="text" placeholder="Ex: 18.0" @keydown.enter.prevent class="w-full rounded-xl border-indigo-200 text-xs text-center font-mono font-bold bg-white" />
                </div>

                <div>
                  <label class="block text-[10px] font-bold uppercase text-indigo-700 tracking-wider mb-1">C.O OD (80)</label>
                  <input v-model="form.coOd" type="text" placeholder="Ex: 31.0" @keydown.enter.prevent class="w-full rounded-xl border-indigo-200 text-xs text-center font-mono font-bold bg-white" />
                </div>

                <div>
                  <label class="block text-[10px] font-bold uppercase text-indigo-700 tracking-wider mb-1">C.O OE (80)</label>
                  <input v-model="form.coOe" type="text" placeholder="Ex: 31.0" @keydown.enter.prevent class="w-full rounded-xl border-indigo-200 text-xs text-center font-mono font-bold bg-white" />
                </div>
              </div>
            </div>

            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Observações da Receita / Laboratório</label>
              <input v-model="form.obsReceita" type="text" placeholder="Ex: Quebrar cantos das lentes" @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500" />
            </div>
          </div>

          <!-- Seleção Direta de Produtos -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div class="space-y-2">
              <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-1.5">Armação Selecionada (Opcional)</label>

              <select v-model="form.armacaoId" @change="processarSnapshotProdutos" @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500">
                <option value="">(Nenhuma / Cliente trouxe armação própria)</option>

                <option v-for="a in (Armacoes ?? armacoes)" :key="a.id || a.Id" :value="a.id || a.Id">
                  [{{ a.marcaNome || a.MarcaNome || 'Sem Marca' }}] {{ a.modeloReferencia || a.Modelo }} ({{ a.cor || a.Cor || 'Padrão' }}) — {{ formatarMoeda(a.precoVenda ?? a.PrecoFinal ?? 0) }}
                </option>
              </select>
            </div>

            <div class="space-y-2">
              <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-1.5">Lente do Catálogo / Tabela (Opcional)</label>

              <select v-model="form.lenteId" @change="processarSnapshotProdutos" @keydown.enter.prevent class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500">
                <option value="">(Nenhuma / Tabela Própria)</option>

                <option v-for="l in (Lentes ?? lentes)" :key="l.id || l.Id" :value="l.id || l.Id">
                  {{ l.laboratorio || l.Laboratorio }} — {{ l.tipo || l.Tipo }} {{ (l.tratamento || l.Tratamento) ? `(${l.tratamento || l.Tratamento})` : '' }} — {{ formatarMoeda(l.precoVenda ?? l.PrecoFinal ?? 0) }}
                </option>
              </select>

              <div class="animate-fadeIn p-4 bg-teal-50/50 rounded-2xl border border-teal-200/60 mt-3">
                <label class="block text-xs font-bold uppercase text-teal-900 tracking-wider mb-1">Preço da Lente (Tabela Própria / Editável)</label>

                <div class="relative mt-1 rounded-xl shadow-sm">
                  <input
                    :value="valorLenteFormatado"
                    @input="tratarInputValorLente"
                    @keydown.enter.prevent
                    type="text"
                    placeholder="R$ 0,00"
                    class="w-full rounded-xl border-teal-200 text-sm font-mono font-bold focus:border-teal-500"
                  />
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

                <select v-model="form.formaPagamento" @keydown.enter.prevent class="w-full rounded-xl border-amber-200 text-xs bg-white">
                  <option value="DINHEIRO">Dinheiro</option>
                  <option value="PIX">Pix</option>
                  <option value="CARTAO_CREDITO">Cartão de Crédito</option>
                  <option value="CARTAO_DEBITO">Cartão de Débito</option>
                </select>
              </div>

              <div v-if="form.formaPagamento === 'CARTAO_CREDITO'">
                <label class="block font-bold text-amber-900 uppercase mb-1.5">Parcelas *</label>

                <select v-model.number="qtdParcelas" @keydown.enter.prevent class="w-full rounded-xl border-amber-200 text-xs bg-white">
                  <option value="1">1x à Vista</option>
                  <option v-for="n in [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]" :key="n" :value="n">
                    {{ n }}x
                  </option>
                </select>
              </div>

              <div>
                <label class="block font-bold text-amber-900 uppercase mb-1.5">Desconto (R$)</label>

                <div class="relative">
                  <input
                    :value="descontoReaisFormatado"
                    @input="tratarInputDescontoReais"
                    @keydown.enter.prevent
                    type="text"
                    placeholder="R$ 0,00"
                    class="w-full rounded-xl border-amber-200 text-xs bg-white font-mono font-bold pr-16"
                  />

                  <span class="absolute right-2 top-1.5 text-[10px] text-amber-700 font-mono font-bold bg-amber-100 px-1.5 py-0.5 rounded">
                    {{ form.descontoPercentual.toFixed(1) }}% off
                  </span>
                </div>
              </div>

              <div>
                <label class="block font-bold text-teal-950 uppercase mb-1.5">Total Líquido do Pedido</label>

                <div class="text-base font-black font-mono text-teal-700 bg-teal-50 px-3 py-2 rounded-xl border border-teal-100 text-center">
                  {{ formatarMoeda(form.valorTotalLiquido) }}
                </div>
              </div>
            </div>
          </div>

          <div class="flex items-center justify-end gap-3 border-t border-slate-100 pt-4">
            <Link href="/ordens" class="px-5 py-3 text-sm font-semibold text-slate-500 hover:text-slate-800 transition">
              Cancelar
            </Link>

            <button type="submit" :disabled="salvandoOS" class="bg-teal-600 hover:bg-teal-700 disabled:bg-slate-200 text-white font-bold py-3.5 px-8 rounded-xl shadow-md transition text-sm min-w-[200px]">
              <span v-if="salvandoOS">Processando Emissão...</span>
              <span v-else>Faturar Ordem de Serviço</span>
            </button>
          </div>
        </form>
      </div>

      <!-- TELA DE SUCESSO -->
      <div class="bg-white rounded-3xl border border-slate-200 shadow-xl overflow-hidden p-8 space-y-6 no-print text-center" v-else>
        <div class="inline-flex items-center justify-center w-16 h-16 rounded-full bg-teal-100 text-teal-600 text-3xl">✓</div>

        <h2 class="text-2xl font-black text-slate-900">OS Emitida com Sucesso!</h2>

        <p class="text-sm text-slate-500">A Ordem de Serviço {{ osFaturadaResponse.numeroOS }} foi gravada de forma definitiva no sistema.</p>

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
import { ref, watch, onMounted, onBeforeUnmount, computed } from 'vue'
import { useForm, Link, router } from '@inertiajs/vue3'
import axios from 'axios'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  Vendedores: Array,
  vendedores: Array,
  Armacoes: Array,
  armacoes: Array,
  Lentes: Array,
  lentes: Array
})

const CHAVE_RASCUNHO = 'retsys_os_rascunho'

const dataEmissaoHoje = computed(() => new Date().toISOString().split('T')[0])

const exibirFaturaSucesso = ref(false)
const tipoComprovanteImpressao = ref('completa')
const osFaturadaResponse = ref({ numeroOS: 'OS-TEMP-00000' })
const salvandoOS = ref(false)
const erroSubmissao = ref(null)
const qtdParcelas = ref(1)
const clienteLocalizado = ref(null)
const consultandoCpf = ref(false)
const buscandoCep = ref(false)
const termoAceito = ref(false)
const carregandoIA = ref(false)
const arquivoIaSelecionado = ref(null)
const fotoAnexaArquivo = ref(null)
const rascunhoRestaurado = ref(false)

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
  alturaMontagemOd: null,
  alturaMontagemOe: null,
  aro: null,
  dm: null,
  vert: null,
  po: null,
  coOd: null,
  coOe: null,
  obsReceita: '',
  armacaoId: '',
  lenteId: '',
  valorArmacao: 0,
  valorLente: 0,
  valorTotalBruto: 0,
  descontoReais: 0,
  descontoPercentual: 0,
  valorTotalLiquido: 0,
  valorEntrada: null,
  formaPagamento: 'DINHEIRO'
})

const converterParaNumeroSeguro = (valor) => {
  const numero = Number(valor)
  return Number.isFinite(numero) ? numero : 0
}

const formatarMoeda = (valor) => {
  return converterParaNumeroSeguro(valor).toLocaleString('pt-BR', {
    style: 'currency',
    currency: 'BRL'
  })
}

const valorLenteFormatado = computed(() => formatarMoeda(form.valorLente))

const descontoReaisFormatado = computed(() => formatarMoeda(form.descontoReais))

const tratarInputValorLente = (event) => {
  const digitos = event.target.value.replace(/\D/g, '')

  form.valorLente = digitos
    ? parseFloat(digitos) / 100
    : 0

  recalcularTotaisGenericos()
}

const tratarInputDescontoReais = (event) => {
  const digitos = event.target.value.replace(/\D/g, '')

  form.descontoReais = digitos
    ? parseFloat(digitos) / 100
    : 0

  recalcularTotaisGenericos()
}

onMounted(() => {
  const dadosSalvos = localStorage.getItem(CHAVE_RASCUNHO)

  if (dadosSalvos) {
    try {
      const objetoConvertido = JSON.parse(dadosSalvos)

      Object.assign(form, objetoConvertido)

      rascunhoRestaurado.value = true

      processarSnapshotProdutos()
    } catch {
      localStorage.removeItem(CHAVE_RASCUNHO)
    }
  }

  window.addEventListener('beforeunload', avisarSairPagina)
})

onBeforeUnmount(() => {
  window.removeEventListener('beforeunload', avisarSairPagina)
})

watch(form, (novoForm) => {
  if (!exibirFaturaSucesso.value) {
    localStorage.setItem(CHAVE_RASCUNHO, JSON.stringify(novoForm))
  }
}, { deep: true })

watch(() => form.formaPagamento, (novaForma) => {
  if (novaForma !== 'CARTAO_CREDITO') {
    qtdParcelas.value = 1
  }
})

const limparRascunhoManual = () => {
  localStorage.removeItem(CHAVE_RASCUNHO)
  form.reset()
  rascunhoRestaurado.value = false
}

const avisarSairPagina = (event) => {
  if (!exibirFaturaSucesso.value && form.cpf) {
    event.preventDefault()
    event.returnValue = ''
  }
}

const vincularFotoReceitaDireta = (event) => {
  const files = event.target.files

  if (files.length > 0) {
    fotoAnexaArquivo.value = files[0]
  }
}

const validarCilindrico = (campo) => {
  let valor = converterParaNumeroSeguro(form[campo])

  if (valor > 0) {
    valor = -Math.abs(valor)
  }

  if (valor < -15) {
    valor = -15
  }

  form[campo] = valor
}

const validarEixo = (campo) => {
  let valor = form[campo]

  if (valor !== null && valor !== undefined && valor !== '') {
    valor = Math.floor(converterParaNumeroSeguro(valor))

    if (valor < 0) {
      form[campo] = 0
      return
    }

    if (valor > 180) {
      form[campo] = 180
      return
    }

    form[campo] = valor
  }
}

const validarAdicao = () => {
  if (form.adicao === null || form.adicao === undefined || form.adicao === '') {
    return
  }

  const valor = converterParaNumeroSeguro(form.adicao)

  if (valor < 0) {
    form.adicao = 0
    return
  }

  if (valor > 3.5) {
    form.adicao = 3.5
    return
  }

  form.adicao = valor
}

const tratarDigitacaoCep = () => {
  form.cep = form.cep
    .replace(/\D/g, '')
    .replace(/^(\d{5})(\d)/, '$1-$2')
    .slice(0, 9)

  if (form.cep.replace(/\D/g, '').length === 8) {
    buscarEnderecoViaCep()
  }
}

const buscarEnderecoViaCep = async () => {
  const cepLimpo = form.cep.replace(/\D/g, '')

  if (cepLimpo.length !== 8) {
    return
  }

  buscandoCep.value = true

  try {
    const resposta = await fetch(`https://viacep.com.br/ws/${cepLimpo}/json/`)

    if (resposta.ok) {
      const dados = await resposta.json()

      if (!dados.erro) {
        form.logradouro = dados.logradouro || form.logradouro
        form.bairro = dados.bairro || form.bairro
        form.cidade = dados.localidade || form.cidade
        form.estado = dados.uf || form.estado
      }
    }
  } catch (erro) {
    console.error(erro)
  } finally {
    buscandoCep.value = false
  }
}

const processarSnapshotProdutos = () => {
  const listaArmacoes = props.Armacoes ?? props.armacoes ?? []

  const armacao = listaArmacoes.find((item) => {
    return (item.id || item.Id) === form.armacaoId
  })

  form.valorArmacao = armacao
    ? converterParaNumeroSeguro(armacao.precoVenda ?? armacao.PrecoFinal)
    : 0

  const listaLentes = props.Lentes ?? props.lentes ?? []

  const lente = listaLentes.find((item) => {
    return (item.id || item.Id) === form.lenteId
  })

  if (lente) {
    form.valorLente = converterParaNumeroSeguro(lente.precoVenda ?? lente.PrecoFinal)
  }

  recalcularTotaisGenericos()
}

const recalcularTotaisGenericos = () => {
  const valorArmacao = Math.max(0, converterParaNumeroSeguro(form.valorArmacao))
  const valorLente = Math.max(0, converterParaNumeroSeguro(form.valorLente))
  const valorTotalBruto = valorArmacao + valorLente

  let descontoReais = Math.max(0, converterParaNumeroSeguro(form.descontoReais))

  if (valorTotalBruto <= 0) {
    form.valorArmacao = 0
    form.valorLente = 0
    form.valorTotalBruto = 0
    form.descontoReais = 0
    form.descontoPercentual = 0
    form.valorTotalLiquido = 0

    return
  }

  if (descontoReais > valorTotalBruto) {
    descontoReais = valorTotalBruto
  }

  form.valorArmacao = valorArmacao
  form.valorLente = valorLente
  form.valorTotalBruto = valorTotalBruto
  form.descontoReais = descontoReais
  form.descontoPercentual = Number(
    ((descontoReais / valorTotalBruto) * 100).toFixed(2)
  )
  form.valorTotalLiquido = Number(
    (valorTotalBruto - descontoReais).toFixed(2)
  )
}

const consultarCpfNoBanco = async () => {
  const cpfLimpo = form.cpf.replace(/\D/g, '')

  if (cpfLimpo.length !== 11) {
    alert('O CPF precisa conter exatamente 11 números.')
    return
  }

  consultandoCpf.value = true

  try {
    const reply = await axios.get(`/api/clientes/buscar-cpf/${cpfLimpo}`)

    if (reply.data) {
      Object.assign(form, reply.data)
      clienteLocalizado.value = true
    } else {
      clienteLocalizado.value = false
    }
  } catch {
    clienteLocalizado.value = false
  } finally {
    consultandoCpf.value = false
  }
}

const manipularArquivoIa = (event) => {
  const arquivos = event.target.files

  if (arquivos.length > 0) {
    arquivoIaSelecionado.value = arquivos[0]
  }
}

const executarOcrInteligente = async () => {
  if (!arquivoIaSelecionado.value || !termoAceito.value) {
    return
  }

  carregandoIA.value = true

  try {
    const formData = new FormData()

    formData.append('foto', arquivoIaSelecionado.value)

    const resposta = await axios.post(
      '/ordens-servico/processar-receita-ia',
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data'
        }
      }
    )

    const dados = resposta.data

    if (dados.esfericoLongeDireito !== null && dados.esfericoLongeDireito !== undefined) {
      form.odEsferico = dados.esfericoLongeDireito
    }

    if (dados.cilindricoLongeDireito !== null && dados.cilindricoLongeDireito !== undefined) {
      form.odCilindrico = -Math.abs(dados.cilindricoLongeDireito)
    }

    if (dados.eixoLongeDireito !== null && dados.eixoLongeDireito !== undefined) {
      form.odEixo = dados.eixoLongeDireito
      validarEixo('odEixo')
    }

    if (dados.esfericoLongeEsquerdo !== null && dados.esfericoLongeEsquerdo !== undefined) {
      form.oeEsferico = dados.esfericoLongeEsquerdo
    }

    if (dados.cilindricoLongeEsquerdo !== null && dados.cilindricoLongeEsquerdo !== undefined) {
      form.oeCilindrico = -Math.abs(dados.cilindricoLongeEsquerdo)
    }

    if (dados.eixoLongeEsquerdo !== null && dados.eixoLongeEsquerdo !== undefined) {
      form.oeEixo = dados.eixoLongeEsquerdo
      validarEixo('oeEixo')
    }

    if (dados.adicao !== null && dados.adicao !== undefined) {
      form.adicao = dados.adicao
      validarAdicao()
    }

    if (dados.medico) {
      form.medicoNome = dados.medico
    }

    validarCilindrico('odCilindrico')
    validarCilindrico('oeCilindrico')

    alert('✨ Leitura concluída!')
  } catch {
    alert('Preencha os campos manualmente.')
  } finally {
    carregandoIA.value = false
  }
}

const salvarOrdemServico = async () => {
  erroSubmissao.value = null
  salvandoOS.value = true

  try {
    recalcularTotaisGenericos()

    const formData = new FormData()

    Object.keys(form).forEach((key) => {
      if (form[key] !== null && form[key] !== undefined) {
        formData.append(key, form[key])
      }
    })

    if (fotoAnexaArquivo.value) {
      formData.append('fotoReceitaArquivo', fotoAnexaArquivo.value)
    }

    const query = form.formaPagamento === 'CARTAO_CREDITO'
      ? `?quantidadeParcelas=${qtdParcelas.value}`
      : ''

    const { data } = await axios.post(`/ordens${query}`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })

    localStorage.removeItem(CHAVE_RASCUNHO)

    osFaturadaResponse.value = {
      numeroOS: data.numeroOS || 'OS-FINALIZADA'
    }

    exibirFaturaSucesso.value = true
  } catch (erro) {
    erroSubmissao.value =
      erro.response?.data?.mensagem ||
      erro.response?.data?.erro ||
      'Erro ao emitir a Ordem de Serviço.'
  } finally {
    salvandoOS.value = false
  }
}

const imprimirDocumento = (tipo) => {
  tipoComprovanteImpressao.value = tipo
  window.print()
}

const voltarAoPainel = () => {
  router.get('/ordens')
}
</script>
