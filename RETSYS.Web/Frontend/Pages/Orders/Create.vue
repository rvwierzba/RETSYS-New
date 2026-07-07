<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-5xl mx-auto">

      <div v-if="$page.props.flash?.erro" class="p-4 bg-red-50 border border-red-200 text-red-800 rounded-xl text-sm font-semibold shadow-sm no-print">
        🛑 {{ $page.props.flash.erro }}
      </div>

      <div v-if="erroSubmissao" class="p-4 bg-red-50 border border-red-200 text-red-800 rounded-xl text-sm font-semibold shadow-sm no-print">
        🛑 {{ erroSubmissao }}
      </div>

      <div v-if="!exibirFaturaSucesso" class="bg-white rounded-3xl border border-slate-200 shadow-xl overflow-hidden no-print">

        <div class="bg-slate-950 text-white p-6 flex items-center justify-between">
          <div>
            <h1 class="text-xl font-black tracking-wide">Central Unificada de Emissão de OS</h1>
            <p class="text-xs text-slate-400">Fluxo contínuo: Identificação do cliente, dados clínicos e fechamento financeiro.</p>
          </div>
          <span class="text-xs font-mono bg-teal-500/20 text-teal-400 px-3 py-1 rounded-full border border-teal-500/30">RETSYS CRM v5</span>
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

          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Armação Selecionada (Estoque Real) *</label>
              <select v-model="form.armacaoId" @change="processarSnapshotProdutos" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required>
                <option value="">Selecione o modelo do Inventário</option>
                <option v-for="a in Armacoes" :key="a.id" :value="a.id">
                  {{ a.modeloReferencia }} ({{ a.cor }}) — R$ {{ a.precoVenda.toLocaleString('pt-BR') }}
                </option>
              </select>
            </div>
            <div class="space-y-4">
              <div>
                <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Lente do Catálogo Disponível *</label>
                <select v-model="form.lenteId" @change="processarSnapshotProdutos" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required>
                  <option value="">Selecione a Lente da Matriz</option>
                  <option v-for="l in Lentes" :key="l.id" :value="l.id">
                    {{ l.laboratorio }} — {{ l.tipo }} {{ l.tratamento ? `(${l.tratamento})` : '(Sem Tratamento)' }} (Índice {{ l.indiceRefracao }}) — R$ {{ l.precoVenda.toLocaleString('pt-BR') }}
                  </option>
                </select>
              </div>
              
              <div v-if="lenteManualAtiva" class="animate-fadeIn p-4 bg-teal-50/50 rounded-2xl border border-teal-200/60">
                <label class="block text-xs font-bold uppercase text-teal-900 tracking-wider mb-2">Preço de Venda da Lente Surfaçada (Sob Encomenda) *</label>
                <div class="relative mt-1 rounded-xl shadow-sm">
                  <div class="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                    <span class="text-sm font-semibold text-teal-600">R$</span>
                  </div>
                  <input v-model.number="form.valorLente" type="number" step="0.01" min="0" placeholder="0,00" @input="recalcularTotaisGenericos" class="w-full rounded-xl border-teal-200 pl-9 text-sm font-mono font-bold focus:border-teal-500 focus:ring-teal-500" required />
                </div>
                <p class="text-[10px] text-teal-600 mt-1.5 leading-tight">✓ Lente sob medida identificada. Insira o orçamento acordado com o laboratório parceiro.</p>
              </div>
            </div>
          </div>

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

    <div id="documento-impressao" class="hidden print:block print-area">

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
          <p><strong>Médico:</strong> {{ form.medicoNome || 'Não informado' }} ({{ form.medicoTipo }}) | <strong>CRM:</strong> {{ form.medicoCrm || 'Não informado' }}</p>

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
                <td class="p-2 border">{{ form.odEsferico ?? '0,00' }}</td>
                <td class="p-2 border">{{ form.odCilindrico ?? '0,00' }}</td>
                <td class="p-2 border">{{ form.odEixo ?? '0' }}°</td>
              </tr>
              <tr class="border-b">
                <td class="p-2 border"><strong>OE</strong></td>
                <td class="p-2 border">{{ form.oeEsferico ?? '0,00' }}</td>
                <td class="p-2 border">{{ form.oeCilindrico ?? '0,00' }}</td>
                <td class="p-2 border">{{ form.oeEixo ?? '0' }}°</td>
              </tr>
            </tbody>
          </table>

          <p class="mt-2"><strong>Adição:</strong> {{ form.adicao ?? 'N/A' }} | <strong>DNP OD:</strong> {{ form.dnpOd }}mm | <strong>DNP OE:</strong> {{ form.dnpOe }}mm</p>

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
import { ref, watch, nextTick } from 'vue'
import { useForm, Link, router } from '@inertiajs/vue3'
import axios from 'axios'
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

// Controle manual de submit (o backend responde JSON puro em /ordens, incompatível
// com o protocolo Inertia — por isso não usamos form.post/form.processing aqui)
const salvandoOS = ref(false)
const erroSubmissao = ref(null)

const qtdParcelas = ref(1)
const lenteManualAtiva = ref(false) // Controle de edição manual para lentes surfaçadas

// Estado reativo do fluxo de CRM (Encontrar vs Cadastrar Rápido)
const clienteLocalizado = ref(null) // null = sem busca, true = achou perfil, false = cadastrar rápido
const consultandoCpf = ref(false)

// Estados da IA Moondream
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
  lenteId: '', // representa o LentePrecoId esperado pelo backend (props.Lentes)

  valorArmacao: 0,
  valorLente: 0,
  valorTotalBruto: 0,
  descontoPercentual: 0,
  descontoReais: 0,
  valorTotalLiquido: 0,
  valorEntrada: null,
  formaPagamento: 'DINHEIRO'
})

// Se o usuário mudar a forma de pagamento para algo diferente de crédito,
// zera as parcelas para evitar inconsistência no payload/backend
watch(() => form.formaPagamento, (novaForma) => {
  if (novaForma !== 'CARTAO_CREDITO') {
    qtdParcelas.value = 1
  }
})

// Ao selecionar armação/lente, pega o preço direto do snapshot enviado pelo backend
// (props.Lentes já vem por variação de preço, com precoVenda pronto)
const processarSnapshotProdutos = () => {
  const armacao = props.Armacoes.find(a => a.id === form.armacaoId)
  form.valorArmacao = armacao ? armacao.precoVenda : 0

  const lente = props.Lentes.find(l => l.id === form.lenteId)
  if (lente) {
    form.valorLente = lente.precoVenda
    // Ativa campo de digitação se o preço de catálogo for zero (surfaçada sob medida)
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

// BUSCA INTEGRADOR CPF NO BANCO - FLUXO DE FILTRAGEM E PRÉ-CADASTRO
const consultarCpfNoBanco = async () => {
  const cpfLimpo = form.cpf.replace(/\D/g, '')
  if (cpfLimpo.length !== 11) {
    alert('Digite um CPF válido antes de pesquisar.')
    return
  }

  const limparCamposCliente = () => {
    form.nome = ''
    form.telefone = ''
    form.dataNascimento = ''
    form.convenio = ''
    form.cep = ''
    form.logradouro = ''
    form.numero = ''
    form.complemento = ''
    form.bairro = ''
    form.cidade = ''
    form.estado = ''
    form.email = ''
  }

  consultandoCpf.value = true
  try {
    const resposta = await fetch(`/api/clientes/buscar-cpf/${cpfLimpo}`)

    if (resposta.ok) {
      const dados = await resposta.json()
      if (dados && dados.id) {
        form.nome = dados.nome || ''
        form.telefone = dados.telefone || ''
        form.dataNascimento = dados.dataNascimento ? dados.dataNascimento.split('T')[0] : ''
        form.convenio = dados.convenio || ''
        form.cep = dados.cep || ''
        form.logradouro = dados.logradouro || ''
        form.numero = dados.numero || ''
        form.complemento = dados.complemento || ''
        form.bairro = dados.bairro || ''
        form.cidade = dados.cidade || ''
        form.estado = dados.estado || ''
        form.email = dados.email || ''
        clienteLocalizado.value = true
        alert(`Cliente localizado com sucesso: ${dados.nome}`)
      } else {
        limparCamposCliente()
        clienteLocalizado.value = false
        alert('CPF não localizado. Insira os dados nos campos abaixo para realizar o cadastro rápido diretamente por essa tela!')
      }
    } else if (resposta.status === 404) {
      limparCamposCliente()
      clienteLocalizado.value = false
      alert('CPF não localizado. Insira os dados nos campos abaixo para realizar o cadastro rápido diretamente por essa tela!')
    } else {
      alert('Não foi possível consultar o CPF agora. Tente novamente em instantes.')
    }
  } catch (err) {
    console.error(err)
    alert('Erro de conexão ao consultar o CPF. Verifique sua internet e tente novamente.')
  } finally {
    consultandoCpf.value = false
  }
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
      } else {
        alert('CEP não encontrado. Preencha o endereço manualmente.')
      }
    }
  } catch (err) {
    console.error(err)
  }
}

const manipularArquivo = (event) => {
  const arquivos = event.target.files
  if (arquivos.length > 0) {
    arquivoSelecionado.value = arquivos[0]
  }
}

// Integração com o endpoint real do backend: POST /ordens/processar-receita-ia
const ejecutarOcrInteligente = async () => {
  if (!arquivoSelecionado.value || !termoAceito.value) return

  carregandoIA.value = true
  try {
    const formData = new FormData()
    formData.append('imagemReceita', arquivoSelecionado.value)

    const resposta = await axios.post('/ordens/processar-receita-ia', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })

    const dados = resposta.data
    form.medicoNome = dados.medicoNome || form.medicoNome
    form.odEsferico = dados.odEsferico ?? form.odEsferico
    form.odCilindrico = dados.odCilindrico ?? form.odCilindrico
    form.odEixo = dados.odEixo ?? form.odEixo
    form.oeEsferico = dados.oeEsferico ?? form.oeEsferico
    form.oeCilindrico = dados.oeCilindrico ?? form.oeCilindrico
    form.oeEixo = dados.oeEixo ?? form.oeEixo
    form.adicao = dados.adicao ?? form.adicao
    alert('Leitura da receita concluída! Revise os campos antes de continuar.')
  } catch (err) {
    console.error(err)
    const msg = err.response?.data?.mensagem || 'Não foi possível processar a receita. Preencha manualmente.'
    alert(msg)
  } finally {
    carregandoIA.value = false
  }
}

const validarCamposObrigatorios = () => {
  const cpfLimpo = form.cpf.replace(/\D/g, '')
  if (cpfLimpo.length !== 11) {
    erroSubmissao.value = 'Informe um CPF válido (11 dígitos).'
    return false
  }
  if (!form.nome.trim()) {
    erroSubmissao.value = 'Informe o nome completo do cliente.'
    return false
  }
  if (!form.vendedorId) {
    erroSubmissao.value = 'Selecione o vendedor/atendente responsável.'
    return false
  }
  if (!form.dataPrevistaEntrega) {
    erroSubmissao.value = 'Informe a data prevista de entrega.'
    return false
  }
  if (!form.dnpOd || form.dnpOd <= 0) {
    erroSubmissao.value = 'Informe um valor válido para DNP do Olho Direito.'
    return false
  }
  if (!form.dnpOe || form.dnpOe <= 0) {
    erroSubmissao.value = 'Informe um valor válido para DNP do Olho Esquerdo.'
    return false
  }
  if (!form.armacaoId) {
    erroSubmissao.value = 'Selecione uma armação do inventário.'
    return false
  }
  if (!form.lenteId) {
    erroSubmissao.value = 'Selecione uma lente do catálogo.'
    return false
  }
  if (form.adicao && (!form.alturaMontagem || form.alturaMontagem <= 0)) {
    erroSubmissao.value = 'Para lentes com Adição informada, a Altura de Montagem é obrigatória.'
    return false
  }
  if (form.formaPagamento === 'CARTAO_CREDITO' && (!qtdParcelas.value || qtdParcelas.value < 1)) {
    erroSubmissao.value = 'Selecione a quantidade de parcelas do cartão de crédito.'
    return false
  }
  return true
}

const salvarOrdemServico = async () => {
  erroSubmissao.value = null

  if (!validarCamposObrigatorios()) return

  salvandoOS.value = true

  try {
    const query = form.formaPagamento === 'CARTAO_CREDITO'
      ? `?quantidadeParcelas=${qtdParcelas.value}`
      : ''

    const payload = {
      vendedorId: form.vendedorId,
      cpf: form.cpf.replace(/\D/g, ''),
      nome: form.nome,
      telefone: form.telefone,
      dataNascimento: form.dataNascimento || null,
      logradouro: form.logradouro,
      numero: form.numero,
      complemento: form.complemento,
      bairro: form.bairro,
      cidade: form.cidade,
      estado: form.estado,
      cep: form.cep,
      convenio: form.convenio,
      email: form.email,

      dataPrevistaEntrega: form.dataPrevistaEntrega,
      medicoNome: form.medicoNome,
      medicoCrm: form.medicoCrm,
      medicoTipo: form.medicoTipo,
      observacoes: form.observacoes,

      odEsferico: form.odEsferico,
      odCilindrico: form.odCilindrico,
      odEixo: form.odEixo,
      oeEsferico: form.oeEsferico,
      oeCilindrico: form.oeCilindrico,
      oeEixo: form.oeEixo,
      adicao: form.adicao,
      dnpOd: form.dnpOd,
      dnpOe: form.dnpOe,
      alturaMontagem: form.alturaMontagem,

      armacaoId: form.armacaoId,
      lentePrecoId: form.lenteId,
      valorArmacao: form.valorArmacao,
      valorLente: form.valorLente,
      descontoPercentual: form.descontoPercentual,

      formaPagamento: form.formaPagamento,
      valorEntrada: form.valorEntrada
    }

    const { data } = await axios.post(`/ordens${query}`, payload)

    osFaturadaResponse.value = { numeroOS: data.numeroOS }
    exibirFaturaSucesso.value = true

  } catch (err) {
    const respData = err.response?.data
    erroSubmissao.value =
      typeof respData === 'string' ? respData :
      respData?.mensagem || respData?.message || respData?.erro ||
      'Erro ao emitir a Ordem de Serviço. Verifique os dados informados.'
    console.error(err)
  } finally {
    salvandoOS.value = false
  }
}
</script>

