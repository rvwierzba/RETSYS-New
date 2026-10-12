<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-6xl mx-auto">

      <!-- Cabeçalho do Painel -->
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <div>
          <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight">Painel de Ordens de Serviço</h1>
          <p class="text-xs text-slate-500 mt-1">Consulte receitas, acompanhe o pedido de lentes, entregue produtos com quitação de saldo e gerencie a esteira comercial.</p>
        </div>
        <div class="flex items-center gap-3">
          <Link 
            href="/ordens/kanban" 
            class="bg-purple-600 hover:bg-purple-700 text-white font-bold py-2.5 px-5 rounded-xl text-xs transition shadow-sm uppercase tracking-wider flex items-center gap-1.5"
          >
            <span>📊</span> Quadro Kanban
          </Link>
          <button 
            @click="irParaNovaOrdem"
            class="bg-teal-600 hover:bg-teal-700 text-white font-bold py-2.5 px-6 rounded-xl text-xs transition shadow-sm uppercase tracking-wider h-fit"
          >
            + Emitir Nova OS
          </button>
        </div>
      </div>

      <!-- SEÇÃO 3 DO PDF: Campo de Busca Unificado com Auto-detecção e Filtro de Loja -->
      <div class="bg-white p-4 rounded-2xl border border-slate-200 shadow-sm space-y-3">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-3 items-center">
          <!-- Campo Único de Busca (Detecta número ou texto, debounce 300ms) -->
          <div class="md:col-span-2 relative">
            <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">
              <span class="text-sm">🔍</span>
            </div>
            <input
              v-model="termoBusca"
              @input="onInputBusca"
              type="text"
              placeholder="Buscar por Nº da OS (ex: 123) ou Nome do Cliente (mín. 3 caracteres)..."
              class="w-full pl-10 pr-24 py-2.5 rounded-xl border border-slate-200 text-xs font-semibold text-slate-800 focus:border-teal-500 focus:ring-teal-500 bg-slate-50/50"
            />
            <div class="absolute inset-y-0 right-0 pr-2 flex items-center gap-1">
              <span v-if="tipoBuscaDetectado" class="px-2 py-0.5 rounded text-[10px] font-mono font-bold" :class="tipoBuscaDetectado === 'NUMERO' ? 'bg-indigo-100 text-indigo-800' : 'bg-emerald-100 text-emerald-800'">
                {{ tipoBuscaDetectado === 'NUMERO' ? 'Nº OS' : 'Cliente' }}
              </span>
              <button
                v-if="termoBusca"
                type="button"
                @click="limparBusca"
                class="text-slate-400 hover:text-slate-600 p-1 text-xs"
                title="Limpar busca"
              >
                ✕
              </button>
            </div>
          </div>

          <!-- Select de Loja -->
          <div>
            <select
              v-model="lojaSelecionada"
              @change="aplicarFiltros()"
              class="w-full rounded-xl border-slate-200 text-xs focus:border-teal-500 font-bold text-slate-700 bg-slate-50 py-2.5 px-3"
            >
              <option value="">(Todas as Lojas / Filiais)</option>
              <option v-for="l in (Lojas ?? lojas)" :key="l.id || l.Id" :value="l.id || l.Id">
                🏬 {{ l.nome || l.Nome }}
              </option>
            </select>
          </div>
        </div>

        <!-- Linha inferior: Composição, Vendedora e Faturamento -->
        <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-3 pt-2 border-t border-slate-100">
          <div class="flex flex-wrap items-center gap-2">
            <!-- Filtro por Composição da Venda -->
            <div class="flex flex-wrap gap-1.5">
              <button 
                @click="filtrarPorComposicao('total')"
                :class="[filtroAtivoComp === 'total' ? 'bg-slate-950 text-white font-black' : 'bg-slate-50 text-slate-600 hover:bg-slate-100 font-semibold']"
                class="px-3 py-1.5 rounded-xl text-xs uppercase tracking-wider transition border border-transparent"
              >
                📋 Total
              </button>
              <button 
                @click="filtrarPorComposicao('armacao')"
                :class="[filtroAtivoComp === 'armacao' ? 'bg-slate-950 text-white font-black' : 'bg-slate-50 text-slate-600 hover:bg-slate-100 font-semibold']"
                class="px-3 py-1.5 rounded-xl text-xs uppercase tracking-wider transition border"
              >
                👓 Armações
              </button>
              <button 
                @click="filtrarPorComposicao('lente')"
                :class="[filtroAtivoComp === 'lente' ? 'bg-slate-950 text-white font-bold' : 'bg-slate-50 text-slate-600 font-medium']"
                class="px-3 py-1.5 rounded-xl text-xs border border-transparent hover:bg-slate-100 transition"
              >
                🔬 Lentes
              </button>
              <button 
                @click="filtrarPorComposicao('completo')"
                :class="[filtroAtivoComp === 'completo' ? 'bg-slate-950 text-white font-black' : 'bg-slate-50 text-slate-600 hover:bg-slate-100 font-semibold']"
                class="px-3 py-1.5 rounded-xl text-xs uppercase tracking-wider transition border"
              >
                💎 Completo
              </button>
            </div>

            <!-- Select de Filtro por Vendedora -->
            <div class="w-full sm:w-auto min-w-[170px]">
              <select 
                v-model="vendedorSelecionado" 
                @change="aplicarFiltros()"
                class="w-full rounded-xl border-slate-200 text-xs focus:border-teal-500 font-bold text-slate-700 bg-slate-50 py-1.5 px-3"
              >
                <option value="">(Todas as Vendedoras)</option>
                <option v-for="v in (Vendedores ?? vendedores)" :key="v.id || v.Id" :value="v.id || v.Id">
                  👩‍💼 {{ v.nome || v.Nome }}
                </option>
              </select>
            </div>
          </div>

          <!-- Faturamento Calculado -->
          <div class="bg-teal-50 border border-teal-200 rounded-xl px-4 py-1.5 flex items-center justify-between lg:justify-end gap-3">
            <span class="text-[10px] font-bold text-teal-800 uppercase tracking-wider">Faturamento:</span>
            <span class="text-base font-black text-teal-700 font-mono">
              {{ formatarMoeda(totalExibido) }}
            </span>
          </div>
        </div>
      </div>

      <!-- Tabela de OSs -->
      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <div v-if="!(Ordens ?? ordens) || (Ordens ?? ordens).length === 0" class="text-center py-12 border-2 border-dashed border-slate-100 rounded-xl text-slate-400 text-sm">
          Nenhuma ordem de serviço localizada para os filtros e parâmetros indicados.
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-left text-sm border-collapse">
            <thead>
              <tr class="border-b border-slate-100 text-slate-400 text-xs font-bold uppercase tracking-wider">
                <th class="pb-3">Nº Documento / Data</th>
                <th class="pb-3">Cliente / Paciente</th>
                <th class="pb-3">Lentes / Laboratório</th>
                <th class="pb-3 text-center">Status</th>
                <th class="pb-3 text-right">Valor / Saldo</th>
                <th class="pb-3 text-center">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="os in (Ordens ?? ordens)" :key="os.id || os.Id" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
                <td class="py-4">
                  <p class="font-mono font-bold text-slate-900 text-xs bg-slate-100 px-2 py-0.5 rounded w-fit mb-1">
                    {{ os.numeroOS || os.NumeroOS }}
                  </p>
                  <p class="text-xs text-slate-400">
                    {{ formatarData(os.dataVenda || os.DataVenda || os.dataEntrada || os.DataEntrada) }}
                  </p>
                  <span v-if="os.dataAjustadaLog || os.DataAjustadaLog" class="text-[9px] text-amber-700 bg-amber-50 border border-amber-200 px-1.5 py-0.5 rounded font-mono font-bold block w-fit mt-1">
                    {{ os.dataAjustadaLog || os.DataAjustadaLog }}
                  </span>
                  <span v-if="os.ultimaEdicaoLog || os.UltimaEdicaoLog" class="text-[9px] text-indigo-700 bg-indigo-50 border border-indigo-200 px-1.5 py-0.5 rounded font-mono font-bold block w-fit mt-1" title="Histórico de Edição Administrativa">
                    ✏️ {{ os.ultimaEdicaoLog || os.UltimaEdicaoLog }}
                  </span>
                </td>
                <td class="py-4">
                  <p class="font-semibold text-slate-800">{{ os.clienteNome || os.ClienteNome }}</p>
                  <p class="text-[11px] text-slate-400">Atendente: {{ os.vendedorNome || os.VendedorNome || 'Não atribuído' }} • <b class="text-indigo-600">{{ os.lojaVenda || os.LojaVenda || 'Matriz' }}</b></p>
                </td>

                <!-- SEÇÃO 3.2: CONTROLE VISUAL DE LENTES PEDIDAS NA TABELA -->
                <td class="py-4">
                  <div v-if="temLente(os)" class="space-y-1">
                    <div v-if="os.lentePedida || os.LentePedida" class="flex items-center gap-1 text-[11px] font-bold text-emerald-700 bg-emerald-50 px-2.5 py-1 rounded-lg border border-emerald-200 w-fit">
                      <span>✓ Lente Pedida</span>
                      <span class="text-[9px] text-emerald-600 font-normal">({{ formatarData(os.dataPedidoLente || os.DataPedidoLente) }})</span>
                    </div>
                    <button 
                      v-else-if="os.status !== 'CANCELADO' && os.status !== 'CANCELADA'"
                      @click="marcarLentePedida(os.id || os.Id)"
                      class="bg-amber-100 hover:bg-amber-200 text-amber-800 text-[11px] font-bold px-2.5 py-1 rounded-lg transition border border-amber-300 flex items-center gap-1 shadow-sm"
                    >
                      <span>📦 Marcar Lente Pedida</span>
                    </button>
                  </div>
                  <span v-else class="text-xs text-slate-300 font-mono">-- (Sem Lente)</span>
                </td>

                <td class="py-4 text-center">
                  <select 
                    :value="os.status === 'CANCELADA' ? 'CANCELADO' : (os.status || 'EM_ABERTO')"
                    @change="alterarStatusOS(os.id || os.Id, $event.target.value)"
                    :disabled="os.status === 'CANCELADO' || os.status === 'CANCELADA'"
                    :class="[
                      (os.status === 'CANCELADO' || os.status === 'CANCELADA') ? 'bg-rose-50 text-rose-700 border-rose-200 cursor-not-allowed' :
                      os.status === 'ENTREGUE' ? 'bg-emerald-50 text-emerald-700 border-emerald-200' :
                      (os.status === 'PRONTA' || os.status === 'PRONTO') ? 'bg-blue-50 text-blue-700 border-blue-200' :
                      (os.status === 'EM_MONTAGEM' || os.status === 'EM_LABORATORIO') ? 'bg-indigo-50 text-indigo-700 border-indigo-200' :
                      os.status === 'AGUARDANDO_LENTE' ? 'bg-purple-50 text-purple-700 border-purple-200' :
                      os.status === 'CONFIRMADA' ? 'bg-sky-50 text-sky-700 border-sky-200' :
                      'bg-amber-50 text-amber-700 border-amber-200'
                    ]"
                    class="px-2.5 py-1 rounded-xl text-[11px] font-black border uppercase font-mono cursor-pointer focus:ring-2 focus:ring-teal-500 transition"
                  >
                    <option value="LANCADA">Lançada</option>
                    <option value="CONFIRMADA">Confirmada</option>
                    <option value="AGUARDANDO_LENTE">Aguard. Lente</option>
                    <option value="EM_MONTAGEM">Em Montagem</option>
                    <option value="PRONTA">Pronta</option>
                    <option value="ENTREGUE">Entregue</option>
                    <option value="CANCELADO" disabled>Cancelada</option>
                  </select>
                </td>

                <td class="py-4 text-right font-mono">
                  <p class="font-black text-slate-950 text-xs">{{ formatarMoeda(os.valorTotal ?? os.ValorTotal) }}</p>
                  <!-- SEÇÃO 5: Saldo Restante -->
                  <p v-if="obterValorRestante(os) > 0" class="text-[10px] font-bold text-rose-600 mt-0.5">
                    A Pagar: {{ formatarMoeda(obterValorRestante(os)) }}
                  </p>
                  <p v-else-if="os.status !== 'CANCELADO' && os.status !== 'CANCELADA'" class="text-[10px] font-bold text-emerald-600 mt-0.5">
                    ✓ Quitado
                  </p>
                </td>

                <td class="py-4 text-center flex items-center justify-center gap-1.5 flex-wrap">
                  <!-- Ver detalhes da OS -->
                  <button 
                    @click="abrirPranchetaClinica(os)"
                    class="bg-slate-950 hover:bg-slate-800 text-white text-xs font-bold px-2.5 py-1.5 rounded-lg transition shadow-sm font-mono flex items-center gap-1"
                    title="Visualizar OS Completa"
                  >
                    <span>👁️</span> OS
                  </button>

                  <!-- WHATSAPP DIRETO -->
                  <button 
                    v-if="os.status !== 'CANCELADO' && os.status !== 'CANCELADA'"
                    @click="abrirWhatsAppOS(os)"
                    class="bg-emerald-50 hover:bg-emerald-100 text-emerald-700 border border-emerald-200 text-xs font-bold px-2 py-1.5 rounded-lg transition shadow-sm font-mono flex items-center gap-1"
                    title="Enviar WhatsApp ao Cliente"
                  >
                    <span>💬</span>
                  </button>

                  <!-- EDIÇÃO ADMIN DA DATA DE EMISSÃO -->
                  <button 
                    v-if="eAdmin && os.status !== 'CANCELADO' && os.status !== 'CANCELADA'"
                    @click="abrirModalEditarAdmin(os)"
                    class="bg-indigo-50 hover:bg-indigo-100 text-indigo-700 border border-indigo-200 text-xs font-bold px-2 py-1.5 rounded-lg transition shadow-sm font-mono"
                    title="Editar Data de Emissão (Exclusivo Admin)"
                  >
                    ✏️
                  </button>

                  <!-- SEÇÃO 3.1 & 6: MARCAR COMO ENTREGUE / QUITAR SALDO -->
                  <button 
                    v-if="os.status !== 'ENTREGUE' && os.status !== 'CANCELADO' && os.status !== 'CANCELADA'"
                    @click="abrirModalEntrega(os)"
                    class="bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-bold px-2.5 py-1.5 rounded-lg transition shadow-sm font-mono flex items-center gap-1"
                    title="Marcar OS como Entregue"
                  >
                    <span>🚚</span> Entregar
                  </button>

                  <!-- Cancelar OS -->
                  <button 
                    v-if="os.status !== 'CANCELADO' && os.status !== 'CANCELADA'"
                    @click="cancelarOS(os.id || os.Id, os.numeroOS || os.NumeroOS)"
                    class="bg-rose-50 hover:bg-rose-100 text-rose-700 border border-rose-200 text-xs font-bold px-2 py-1.5 rounded-lg transition shadow-sm font-mono"
                    title="Cancelar Ordem de Serviço"
                  >
                    🚫
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- MODAL / PRANCHETA COMPLETA DA ORDEM DE SERVIÇO -->
      <div v-if="osSelecionada" class="fixed inset-0 bg-slate-950/60 backdrop-blur-sm z-50 flex items-center justify-center p-4 overflow-y-auto">
        <div class="bg-white rounded-3xl border border-slate-200 shadow-2xl max-w-3xl w-full max-h-[90vh] overflow-y-auto p-6 md:p-8 space-y-6">
          
          <!-- Cabeçalho da Modal -->
          <div class="flex items-center justify-between border-b border-slate-100 pb-4">
            <div>
              <span 
                :class="[
                  (osSelecionada.status === 'CANCELADO' || osSelecionada.status === 'CANCELADA') ? 'bg-rose-100 text-rose-800' :
                  osSelecionada.status === 'ENTREGUE' ? 'bg-emerald-100 text-emerald-800' :
                  'bg-teal-100 text-teal-800'
                ]" 
                class="text-[10px] font-mono font-bold px-2.5 py-0.5 rounded-full uppercase"
              >
                Ordem de Serviço — {{ osSelecionada.status || 'EM_ABERTO' }}
              </span>
              <h2 class="text-xl font-black font-mono text-slate-950 mt-1">
                {{ osSelecionada.numeroOS || osSelecionada.NumeroOS }}
              </h2>
            </div>
            <button 
              @click="osSelecionada = null" 
              class="w-9 h-9 rounded-full bg-slate-100 hover:bg-slate-200 font-bold text-slate-500 hover:text-slate-900 transition flex items-center justify-center"
            >
              ✕
            </button>
          </div>

          <!-- Bloco 1: Cliente e Atendimento -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4 bg-slate-50 p-4 rounded-2xl border border-slate-200/80 text-xs">
            <div>
              <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider block mb-1">Cliente / Paciente</span>
              <p class="font-black text-slate-800 text-sm">{{ osSelecionada.clienteNome || osSelecionada.ClienteNome }}</p>
              <p class="text-slate-500 font-mono mt-0.5">CPF: {{ osSelecionada.cliente?.cpf || osSelecionada.Cliente?.CPF || 'Não informado' }}</p>
              <p class="text-slate-500 mt-0.5">Tel: {{ osSelecionada.cliente?.telefone || osSelecionada.Cliente?.Telefone || 'Sem telefone' }}</p>
            </div>
            <div>
              <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider block mb-1">Dados da Emissão</span>
              <p class="text-slate-700 font-semibold">Data Entrada: {{ formatarData(osSelecionada.dataEntrada || osSelecionada.DataEntrada) }}</p>
              <p class="text-slate-700 font-semibold">Data Entrega Prevista: {{ formatarData(osSelecionada.dataPrevistaEntrega || osSelecionada.DataPrevistaEntrega) }}</p>
              <p v-if="osSelecionada.dataEntregaReal || osSelecionada.DataEntregaReal" class="text-emerald-700 font-bold">
                Data Entrega Real: {{ formatarData(osSelecionada.dataEntregaReal || osSelecionada.DataEntregaReal) }}
              </p>
              <p class="text-slate-700 font-semibold">Vendedora: {{ osSelecionada.vendedorNome || osSelecionada.VendedorNome || 'Não informado' }}</p>
            </div>
          </div>

          <!-- SEÇÃO 3.2: Status de Pedido de Lentes na Modal -->
          <div v-if="temLente(osSelecionada)" class="p-3 rounded-2xl border flex items-center justify-between text-xs font-mono" :class="osSelecionada.lentePedida || osSelecionada.LentePedida ? 'bg-emerald-50 border-emerald-200 text-emerald-900' : 'bg-amber-50 border-amber-200 text-amber-900'">
            <div class="flex items-center gap-2">
              <span class="text-base">{{ osSelecionada.lentePedida || osSelecionada.LentePedida ? '📦' : '⏳' }}</span>
              <div>
                <p class="font-bold">
                  {{ osSelecionada.lentePedida || osSelecionada.LentePedida ? 'Lentes Pedidas ao Laboratório' : 'Aguardando Pedido de Lentes' }}
                </p>
                <p v-if="osSelecionada.lentePedida || osSelecionada.LentePedida" class="text-[10px] text-emerald-700">
                  Data: {{ formatarData(osSelecionada.dataPedidoLente || osSelecionada.DataPedidoLente) }} 
                  <span v-if="osSelecionada.pedidoLentePorNome || osSelecionada.PedidoLentePorNome">| Por: {{ osSelecionada.pedidoLentePorNome || osSelecionada.PedidoLentePorNome }}</span>
                </p>
              </div>
            </div>
            <button 
              v-if="!(osSelecionada.lentePedida || osSelecionada.LentePedida) && osSelecionada.status !== 'CANCELADO' && osSelecionada.status !== 'CANCELADA'"
              @click="marcarLentePedida(osSelecionada.id || osSelecionada.Id)"
              class="bg-amber-600 hover:bg-amber-700 text-white font-bold px-3 py-1.5 rounded-xl transition text-xs shadow-sm"
            >
              Marcar Lente Pedida
            </button>
          </div>

          <!-- Bloco 2: Receita Médica / Grau Clínico -->
          <div v-if="osSelecionada.receita || osSelecionada.Receita" class="space-y-3">
            <h4 class="text-xs font-black uppercase tracking-wider text-slate-950 font-mono flex items-center gap-2">
              <span>🩺 Receita Oftalmológica / Refração</span>
              <span class="text-[10px] text-slate-400 font-normal">
                (Dr(a). {{ osSelecionada.medico || osSelecionada.MedicoNome || 'Não informado' }})
              </span>
            </h4>

            <!-- Tabela Grau de Longe -->
            <div class="bg-white rounded-2xl border border-slate-200 overflow-hidden text-xs">
              <div class="bg-slate-100 px-3 py-1.5 border-b text-[10px] font-bold uppercase tracking-wider text-slate-600 flex justify-between">
                <span>👓 Grau de Longe</span>
                <span v-if="receitaObj.adicao" class="text-teal-700 font-bold">Adição (AD): +{{ formatarGrau(receitaObj.adicao) }}</span>
              </div>
              <table class="w-full text-center border-collapse">
                <thead>
                  <tr class="bg-slate-50 text-slate-500 text-[10px] font-bold uppercase tracking-wider border-b border-slate-200">
                    <th class="py-2">Olho</th>
                    <th class="py-2">Esférico</th>
                    <th class="py-2">Cilíndrico</th>
                    <th class="py-2">Eixo</th>
                    <th class="py-2">DNP</th>
                    <th class="py-2">Altura Mont.</th>
                  </tr>
                </thead>
                <tbody class="font-mono">
                  <tr class="border-b border-slate-100">
                    <td class="py-2.5 font-black text-slate-800">OD</td>
                    <td class="py-2.5 font-bold text-slate-900">{{ formatarGrau(receitaObj.odEsferico) }}</td>
                    <td class="py-2.5 font-bold text-amber-700">{{ formatarGrau(receitaObj.odCilindrico) }}</td>
                    <td class="py-2.5 text-slate-700">{{ receitaObj.odEixo }}°</td>
                    <td class="py-2.5 text-slate-700">{{ receitaObj.dnpOd || '--' }} mm</td>
                    <td class="py-2.5 font-bold text-indigo-700">{{ receitaObj.alturaMontagemOd || receitaObj.alturaMontagem || '--' }} mm</td>
                  </tr>
                  <tr>
                    <td class="py-2.5 font-black text-slate-800">OE</td>
                    <td class="py-2.5 font-bold text-slate-900">{{ formatarGrau(receitaObj.oeEsferico) }}</td>
                    <td class="py-2.5 font-bold text-amber-700">{{ formatarGrau(receitaObj.oeCilindrico) }}</td>
                    <td class="py-2.5 text-slate-700">{{ receitaObj.oeEixo }}°</td>
                    <td class="py-2.5 text-slate-700">{{ receitaObj.dnpOe || '--' }} mm</td>
                    <td class="py-2.5 font-bold text-indigo-700">{{ receitaObj.alturaMontagemOe || receitaObj.alturaMontagem || '--' }} mm</td>
                  </tr>
                </tbody>
              </table>
            </div>

            <!-- Tabela Grau de Perto (Exibida quando há Adição) -->
            <div v-if="receitaObj.adicao" class="bg-white rounded-2xl border border-teal-200 overflow-hidden text-xs">
              <div class="bg-teal-50 px-3 py-1.5 border-b border-teal-100 text-[10px] font-bold uppercase tracking-wider text-teal-900 flex justify-between">
                <span>🔍 Grau de Perto (Longe + Adição)</span>
                <span class="text-teal-700">AD: +{{ formatarGrau(receitaObj.adicao) }}</span>
              </div>
              <table class="w-full text-center border-collapse font-mono">
                <thead>
                  <tr class="bg-teal-50/50 text-teal-800 text-[10px] font-bold uppercase tracking-wider border-b border-teal-100">
                    <th class="py-2">Olho</th>
                    <th class="py-2">Esférico Perto</th>
                    <th class="py-2">Cilíndrico Perto</th>
                    <th class="py-2">Eixo Perto</th>
                  </tr>
                </thead>
                <tbody>
                  <tr class="border-b border-teal-100/50">
                    <td class="py-2.5 font-black text-slate-800">OD</td>
                    <td class="py-2.5 font-bold text-teal-950">{{ formatarGrau(receitaObj.odEsfericoPerto ?? receitaObj.esfericoPertoDireito ?? (Number(receitaObj.odEsferico || 0) + Number(receitaObj.adicao || 0))) }}</td>
                    <td class="py-2.5 font-bold text-amber-700">{{ formatarGrau(receitaObj.odCilindricoPerto ?? receitaObj.odCilindrico) }}</td>
                    <td class="py-2.5 text-slate-700">{{ receitaObj.odEixoPerto ?? receitaObj.odEixo }}°</td>
                  </tr>
                  <tr>
                    <td class="py-2.5 font-black text-slate-800">OE</td>
                    <td class="py-2.5 font-bold text-teal-950">{{ formatarGrau(receitaObj.oeEsfericoPerto ?? receitaObj.esfericoPertoEsquerdo ?? (Number(receitaObj.oeEsferico || 0) + Number(receitaObj.adicao || 0))) }}</td>
                    <td class="py-2.5 font-bold text-amber-700">{{ formatarGrau(receitaObj.oeCilindricoPerto ?? receitaObj.oeCilindrico) }}</td>
                    <td class="py-2.5 text-slate-700">{{ receitaObj.oeEixoPerto ?? receitaObj.oeEixo }}°</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <!-- Bloco 3: Medidas da Armação -->
          <div v-if="receitaObj && (receitaObj.aro || receitaObj.dm || receitaObj.vert)" class="p-4 bg-indigo-50/50 rounded-2xl border border-indigo-100 space-y-2">
            <span class="text-[10px] font-black uppercase text-indigo-900 tracking-wider block">📐 Medidas Físicas da Armação / Laboratório</span>
            <div class="grid grid-cols-3 md:grid-cols-6 gap-2 text-center text-xs font-mono">
              <div class="bg-white p-2 rounded-xl border border-indigo-100"><span class="block text-[9px] text-slate-400">ARO</span><b>{{ receitaObj.aro || '--' }}</b></div>
              <div class="bg-white p-2 rounded-xl border border-indigo-100"><span class="block text-[9px] text-slate-400">DM</span><b>{{ receitaObj.dm || '--' }}</b></div>
              <div class="bg-white p-2 rounded-xl border border-indigo-100"><span class="block text-[9px] text-slate-400">VERT</span><b>{{ receitaObj.vert || '--' }}</b></div>
              <div class="bg-white p-2 rounded-xl border border-indigo-100"><span class="block text-[9px] text-slate-400">PO</span><b>{{ receitaObj.po || '--' }}</b></div>
              <div class="bg-white p-2 rounded-xl border border-indigo-100"><span class="block text-[9px] text-slate-400">C.O OD</span><b>{{ receitaObj.coOd || '--' }}</b></div>
              <div class="bg-white p-2 rounded-xl border border-indigo-100"><span class="block text-[9px] text-slate-400">C.O OE</span><b>{{ receitaObj.coOe || '--' }}</b></div>
            </div>
          </div>

          <!-- Bloco 4: Financeiro e Condições de Pagamento -->
          <div v-if="osSelecionada.financeiro || osSelecionada.Financeiro" class="bg-slate-50 p-4 rounded-2xl border border-slate-200 space-y-3 text-xs">
            <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider block">Resumo Financeiro & Condições</span>
            
            <div class="grid grid-cols-2 md:grid-cols-5 gap-3 font-mono">
              <div>
                <span class="text-[10px] text-slate-400 block">Total Bruto</span>
                <span class="font-bold text-slate-700">{{ formatarMoeda(financeiroObj.valorTotalBruto) }}</span>
              </div>
              <div>
                <span class="text-[10px] text-slate-400 block">Desconto</span>
                <span class="font-bold text-amber-700">{{ formatarMoeda(financeiroObj.descontoReais) }}</span>
              </div>
              <div>
                <span class="text-[10px] text-slate-400 block">Total Líquido</span>
                <span class="font-black text-slate-900">{{ formatarMoeda(financeiroObj.valorTotalLiquido || osSelecionada.valorTotal) }}</span>
              </div>
              <div>
                <span class="text-[10px] text-slate-400 block">Entrada Pago</span>
                <span class="font-bold text-teal-700">{{ formatarMoeda(financeiroObj.valorEntrada) }}</span>
              </div>
              <div>
                <span class="text-[10px] text-slate-400 block">Saldo Restante</span>
                <span :class="obterValorRestante(osSelecionada) > 0 ? 'text-rose-700 font-black' : 'text-emerald-700 font-bold'">
                  {{ obterValorRestante(osSelecionada) > 0 ? formatarMoeda(obterValorRestante(osSelecionada)) : 'R$ 0,00 - Quitado' }}
                </span>
              </div>
            </div>

            <!-- Tabela de Parcelas -->
            <div v-if="parcelasLista.length > 0" class="pt-2 border-t border-slate-200">
              <span class="text-[10px] font-bold uppercase text-slate-400 tracking-wider block mb-1">Cronograma de Parcelas</span>
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-2">
                <div v-for="p in parcelasLista" :key="p.numeroParcela" class="bg-white p-2 rounded-xl border border-slate-200 flex justify-between font-mono text-[11px]">
                  <span>Parcela {{ p.numeroParcela }}</span>
                  <span class="text-slate-500">{{ formatarData(p.dataVencimento) }}</span>
                  <b class="text-slate-900">{{ formatarMoeda(p.valor) }}</b>
                </div>
              </div>
            </div>
          </div>

          <!-- SEÇÃO 1 DO PDF: Log Obrigatório e Visível de Alterações da OS -->
          <div class="bg-slate-50 p-4 rounded-2xl border border-slate-200 space-y-3 text-xs">
            <div class="flex items-center justify-between">
              <h4 class="text-xs font-black uppercase tracking-wider text-slate-900 font-mono flex items-center gap-2">
                <span>📋 Histórico de Auditoria & Alterações</span>
              </h4>
              <span v-if="osSelecionada.ultimaEdicaoLog || osSelecionada.UltimaEdicaoLog" class="text-[10px] font-bold text-indigo-700 bg-indigo-50 border border-indigo-200 px-2 py-0.5 rounded-full font-mono">
                ✏️ {{ osSelecionada.ultimaEdicaoLog || osSelecionada.UltimaEdicaoLog }}
              </span>
            </div>

            <div v-if="carregandoAuditoria" class="py-4 text-center text-slate-400 font-mono">
              Carregando histórico de auditoria...
            </div>
            <div v-else-if="logsAuditoria.length === 0" class="p-3 bg-white rounded-xl border border-slate-200 text-slate-400 text-center text-xs">
              Nenhuma alteração registrada após a emissão desta Ordem de Serviço.
            </div>
            <div v-else class="overflow-x-auto bg-white rounded-xl border border-slate-200 max-h-48 overflow-y-auto">
              <table class="w-full text-left text-xs font-mono">
                <thead class="bg-slate-100 text-slate-600 text-[10px] uppercase font-bold border-b border-slate-200 sticky top-0">
                  <tr>
                    <th class="py-2 px-3">Data/Hora</th>
                    <th class="py-2 px-3">Responsável</th>
                    <th class="py-2 px-3">Campo Alterado</th>
                    <th class="py-2 px-3">Valor Anterior</th>
                    <th class="py-2 px-3">Valor Novo</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="log in logsAuditoria" :key="log.id" class="border-b border-slate-100 hover:bg-slate-50">
                    <td class="py-2 px-3 text-slate-500 whitespace-nowrap">{{ log.dataFormatada || formatarData(log.dataAlteracao) }}</td>
                    <td class="py-2 px-3 font-semibold text-slate-700">{{ log.usuarioNome }}</td>
                    <td class="py-2 px-3 font-bold text-indigo-700">{{ log.campoAlterado }}</td>
                    <td class="py-2 px-3 text-slate-500 truncate max-w-[120px]" :title="log.valorAntigo">{{ log.valorAntigo }}</td>
                    <td class="py-2 px-3 font-bold text-emerald-700 truncate max-w-[120px]" :title="log.valorNovo">{{ log.valorNovo }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <div class="flex justify-end gap-2 border-t border-slate-100 pt-4">
            <button 
              v-if="osSelecionada.status !== 'ENTREGUE' && osSelecionada.status !== 'CANCELADO' && osSelecionada.status !== 'CANCELADA'"
              @click="abrirModalEntrega(osSelecionada)"
              class="bg-emerald-600 hover:bg-emerald-700 text-white font-bold py-2.5 px-6 rounded-xl text-xs uppercase transition shadow-sm"
            >
              🚚 Entregar e Quitar OS
            </button>
            <button 
              @click="osSelecionada = null" 
              class="bg-slate-950 hover:bg-slate-800 text-white font-bold py-2.5 px-6 rounded-xl text-xs uppercase transition shadow-sm"
            >
              Fechar Prancheta
            </button>
          </div>

        </div>
      </div>

      <!-- SEÇÃO 6: MODAL DE QUITAÇÃO DE SALDO E CONFIRMAÇÃO DE ENTREGA -->
      <div v-if="modalEntregaAberta && osParaEntrega" class="fixed inset-0 bg-slate-950/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
        <div class="bg-white rounded-3xl border border-slate-200 shadow-2xl max-w-md w-full p-6 space-y-5">
          <div class="flex items-center justify-between border-b border-slate-100 pb-3">
            <div>
              <span class="text-[10px] font-mono font-bold bg-emerald-100 text-emerald-800 px-2 py-0.5 rounded-full uppercase">Entrega de Óculos</span>
              <h3 class="text-base font-black text-slate-950 font-mono mt-1">OS {{ osParaEntrega.numeroOS || osParaEntrega.NumeroOS }}</h3>
            </div>
            <button @click="modalEntregaAberta = false" class="text-slate-400 hover:text-slate-800 font-bold">✕</button>
          </div>

          <!-- Caso haja saldo a receber na retirada -->
          <div v-if="obterValorRestante(osParaEntrega) > 0" class="space-y-4">
            <div class="p-3 bg-rose-50 border border-rose-200 rounded-xl text-xs text-rose-900 font-mono">
              <span>Existe um saldo pendente de <b>{{ formatarMoeda(obterValorRestante(osParaEntrega)) }}</b> para esta OS.</span>
            </div>

            <div class="space-y-3 text-xs">
              <label class="block font-bold uppercase text-slate-400 tracking-wider">Como fica o saldo na entrega? *</label>
              
              <div class="grid grid-cols-2 gap-2">
                <button 
                  type="button"
                  @click="formEntrega.opcaoQuitacao = 'PAGO_RETIRADA'"
                  :class="formEntrega.opcaoQuitacao === 'PAGO_RETIRADA' ? 'bg-teal-600 text-white font-bold border-teal-700' : 'bg-slate-50 text-slate-700 border-slate-200 hover:bg-slate-100'"
                  class="p-3 rounded-xl border text-center transition"
                >
                  💳 Pago na Retirada
                </button>
                <button 
                  type="button"
                  @click="formEntrega.opcaoQuitacao = 'JA_PAGO'"
                  :class="formEntrega.opcaoQuitacao === 'JA_PAGO' ? 'bg-slate-900 text-white font-bold border-slate-950' : 'bg-slate-50 text-slate-700 border-slate-200 hover:bg-slate-100'"
                  class="p-3 rounded-xl border text-center transition"
                >
                  ✓ Já Pago Antes
                </button>
              </div>

              <!-- Se selecionado 'Pago na retirada', exibe forma de pagamento -->
              <div v-if="formEntrega.opcaoQuitacao === 'PAGO_RETIRADA'" class="space-y-3 pt-2">
                <div>
                  <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1">Forma de Pagamento da Retirada *</label>
                  <select v-model="formEntrega.formaPagamentoRetirada" class="w-full rounded-xl border-slate-200 text-xs font-bold text-slate-800 bg-slate-50 focus:border-teal-500">
                    <option value="DINHEIRO">Dinheiro</option>
                    <option value="PIX">Pix</option>
                    <option value="CARTAO_CREDITO">Cartão de Crédito</option>
                    <option value="CARTAO_DEBITO">Cartão de Débito</option>
                    <option value="BOLETO">Boleto</option>
                  </select>
                </div>

                <div v-if="formEntrega.formaPagamentoRetirada === 'CARTAO_CREDITO'">
                  <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1">Parcelas Cartão *</label>
                  <select v-model.number="formEntrega.parcelasRetirada" class="w-full rounded-xl border-slate-200 text-xs font-bold text-slate-800 bg-slate-50 focus:border-teal-500">
                    <option value="1">1x à Vista</option>
                    <option v-for="n in [2,3,4,5,6,7,8,9,10,11,12]" :key="n" :value="n">{{ n }}x</option>
                  </select>
                </div>
              </div>
            </div>
          </div>

          <!-- Caso a venda já esteja quitada -->
          <div v-else class="p-4 bg-emerald-50 border border-emerald-200 text-emerald-900 rounded-xl text-xs space-y-1">
            <p class="font-bold">✓ Ordem de Serviço totalmente quitada!</p>
            <p class="text-[11px] text-emerald-700">Ao confirmar, o status passará para ENTREGUE e a data de entrega real será salva hoje.</p>
          </div>

          <div class="flex justify-end gap-2 pt-3 border-t border-slate-100">
            <button type="button" @click="modalEntregaAberta = false" class="px-4 py-2 rounded-xl text-slate-600 font-bold hover:bg-slate-100 text-xs">Cancelar</button>
            <button @click="confirmarEntregaEQuitacao" :disabled="processandoEntrega" class="px-5 py-2.5 rounded-xl bg-emerald-600 hover:bg-emerald-700 text-white font-bold transition text-xs shadow-sm">
              <span v-if="processandoEntrega">Gravando...</span>
              <span v-else>Confirmar Entrega</span>
            </button>
          </div>
        </div>
      </div>

      <!-- SEÇÃO 1 DO PDF: MODAL DE EDIÇÃO COMPLETA DE OS (EXCLUSIVA DO ADMIN) -->
      <div v-if="modalEditarAdminAberta && osParaEdicaoAdmin" class="fixed inset-0 bg-slate-950/70 backdrop-blur-sm z-50 flex items-center justify-center p-3 md:p-6 overflow-y-auto">
        <div class="bg-white rounded-3xl border border-slate-200 shadow-2xl max-w-4xl w-full max-h-[92vh] overflow-y-auto flex flex-col">
          <!-- Cabeçalho da Modal -->
          <div class="p-6 bg-slate-950 text-white flex items-center justify-between rounded-t-3xl sticky top-0 z-10 shadow">
            <div>
              <div class="flex items-center gap-2">
                <span class="text-[10px] font-mono font-bold bg-indigo-500/30 text-indigo-300 border border-indigo-400/30 px-2.5 py-0.5 rounded-full uppercase">
                  🔒 Edição Administrativa Completa (Admin)
                </span>
                <span v-if="osParaEdicaoAdmin.ultimaEdicaoLog || osParaEdicaoAdmin.UltimaEdicaoLog" class="text-[10px] text-slate-400 font-mono">
                  {{ osParaEdicaoAdmin.ultimaEdicaoLog || osParaEdicaoAdmin.UltimaEdicaoLog }}
                </span>
              </div>
              <h3 class="text-xl font-black font-mono mt-1">
                OS {{ osParaEdicaoAdmin.numeroOS || osParaEdicaoAdmin.NumeroOS }}
              </h3>
            </div>
            <button @click="modalEditarAdminAberta = false" class="text-slate-400 hover:text-white font-bold text-lg p-2">✕</button>
          </div>

          <!-- Abas de Navegação -->
          <div class="flex border-b border-slate-200 bg-slate-50 px-6 pt-3 gap-2 overflow-x-auto text-xs font-bold">
            <button 
              type="button" 
              @click="abaEdicaoAdmin = 'geral'" 
              :class="abaEdicaoAdmin === 'geral' ? 'border-b-2 border-indigo-600 text-indigo-700 bg-white' : 'text-slate-500 hover:text-slate-800'"
              class="px-4 py-2.5 rounded-t-xl transition whitespace-nowrap"
            >
              📋 Dados Gerais & Status
            </button>
            <button 
              type="button" 
              @click="abaEdicaoAdmin = 'cliente'" 
              :class="abaEdicaoAdmin === 'cliente' ? 'border-b-2 border-indigo-600 text-indigo-700 bg-white' : 'text-slate-500 hover:text-slate-800'"
              class="px-4 py-2.5 rounded-t-xl transition whitespace-nowrap"
            >
              👤 Dados do Cliente
            </button>
            <button 
              type="button" 
              @click="abaEdicaoAdmin = 'receita'" 
              :class="abaEdicaoAdmin === 'receita' ? 'border-b-2 border-indigo-600 text-indigo-700 bg-white' : 'text-slate-500 hover:text-slate-800'"
              class="px-4 py-2.5 rounded-t-xl transition whitespace-nowrap"
            >
              🩺 Receita & Medidas
            </button>
            <button 
              type="button" 
              @click="abaEdicaoAdmin = 'financeiro'" 
              :class="abaEdicaoAdmin === 'financeiro' ? 'border-b-2 border-indigo-600 text-indigo-700 bg-white' : 'text-slate-500 hover:text-slate-800'"
              class="px-4 py-2.5 rounded-t-xl transition whitespace-nowrap"
            >
              💰 Produtos & Financeiro
            </button>
          </div>

          <!-- Conteúdo do Formulário -->
          <form @submit.prevent="salvarEdicaoAdmin" class="p-6 space-y-6 flex-1 overflow-y-auto">
            <!-- ABA 1: DADOS GERAIS & STATUS -->
            <div v-show="abaEdicaoAdmin === 'geral'" class="space-y-4 text-xs">
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Número da OS *</label>
                  <input v-model="formEdicaoAdmin.numeroOS" type="text" required class="w-full rounded-xl border-slate-200 text-sm font-mono font-bold bg-slate-50 focus:border-indigo-500" />
                </div>
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Data de Lançamento (Emissão) *</label>
                  <input v-model="formEdicaoAdmin.novaDataEntrada" type="date" required class="w-full rounded-xl border-slate-200 text-sm font-mono font-bold bg-slate-50 focus:border-indigo-500" />
                </div>
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Status da OS *</label>
                  <select v-model="formEdicaoAdmin.status" class="w-full rounded-xl border-slate-200 text-xs font-bold bg-slate-50 focus:border-indigo-500 uppercase">
                    <option value="LANCADA">Lançada</option>
                    <option value="CONFIRMADA">Confirmada</option>
                    <option value="AGUARDANDO_LENTE">Aguardando Lente</option>
                    <option value="EM_MONTAGEM">Em Montagem</option>
                    <option value="PRONTA">Pronta</option>
                    <option value="ENTREGUE">Entregue</option>
                    <option value="CANCELADO">Cancelada</option>
                  </select>
                </div>
              </div>

              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Data Prevista de Entrega</label>
                  <input v-model="formEdicaoAdmin.dataPrevistaEntrega" type="date" class="w-full rounded-xl border-slate-200 text-xs font-mono bg-slate-50 focus:border-indigo-500" />
                </div>
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Data de Entrega Real</label>
                  <input v-model="formEdicaoAdmin.dataEntregaReal" type="date" class="w-full rounded-xl border-slate-200 text-xs font-mono bg-slate-50 focus:border-indigo-500" />
                </div>
              </div>

              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Loja da Venda *</label>
                  <select v-model="formEdicaoAdmin.oticaId" class="w-full rounded-xl border-slate-200 text-xs font-bold bg-slate-50 focus:border-indigo-500">
                    <option value="">Selecione a Loja</option>
                    <option v-for="l in (Lojas ?? lojas)" :key="l.id || l.Id" :value="l.id || l.Id">
                      {{ l.nome || l.Nome }}
                    </option>
                  </select>
                </div>
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Vendedora Responsável *</label>
                  <select v-model="formEdicaoAdmin.vendedorId" class="w-full rounded-xl border-slate-200 text-xs font-bold bg-slate-50 focus:border-indigo-500">
                    <option value="">Selecione a Vendedora</option>
                    <option v-for="v in (Vendedores ?? vendedores)" :key="v.id || v.Id" :value="v.id || v.Id">
                      {{ v.nome || v.Nome }}
                    </option>
                  </select>
                </div>
              </div>

              <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Médico Responsável</label>
                  <input v-model="formEdicaoAdmin.medicoNome" type="text" placeholder="Dr(a)..." class="w-full rounded-xl border-slate-200 text-xs bg-slate-50 focus:border-indigo-500" />
                </div>
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">CRM / Registro</label>
                  <input v-model="formEdicaoAdmin.medicoCrm" type="text" placeholder="00000-UF" class="w-full rounded-xl border-slate-200 text-xs bg-slate-50 focus:border-indigo-500" />
                </div>
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Tipo Profissional</label>
                  <select v-model="formEdicaoAdmin.medicoTipo" class="w-full rounded-xl border-slate-200 text-xs bg-slate-50 focus:border-indigo-500">
                    <option value="NAO_ESPECIFICADO">Não Especificado</option>
                    <option value="OFTALMOLOGISTA">Oftalmologista</option>
                    <option value="OPTOMETRISTA">Optometrista</option>
                  </select>
                </div>
              </div>

              <div>
                <label class="block font-bold uppercase text-slate-500 mb-1">Observações Gerais</label>
                <textarea v-model="formEdicaoAdmin.observacoes" rows="2" class="w-full rounded-xl border-slate-200 text-xs bg-slate-50 focus:border-indigo-500"></textarea>
              </div>
            </div>

            <!-- ABA 2: DADOS DO CLIENTE -->
            <div v-show="abaEdicaoAdmin === 'cliente'" class="space-y-4 text-xs">
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div class="md:col-span-2">
                  <label class="block font-bold uppercase text-slate-500 mb-1">Nome Completo do Cliente *</label>
                  <input v-model="formEdicaoAdmin.clienteNome" type="text" required class="w-full rounded-xl border-slate-200 text-xs font-bold bg-slate-50 focus:border-indigo-500" />
                </div>
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">CPF</label>
                  <input v-model="formEdicaoAdmin.clienteCpf" type="text" class="w-full rounded-xl border-slate-200 text-xs font-mono bg-slate-50 focus:border-indigo-500" />
                </div>
              </div>

              <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Telefone / WhatsApp</label>
                  <input v-model="formEdicaoAdmin.clienteTelefone" type="text" class="w-full rounded-xl border-slate-200 text-xs bg-slate-50 focus:border-indigo-500" />
                </div>
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">E-mail</label>
                  <input v-model="formEdicaoAdmin.clienteEmail" type="email" class="w-full rounded-xl border-slate-200 text-xs bg-slate-50 focus:border-indigo-500" />
                </div>
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Convênio</label>
                  <input v-model="formEdicaoAdmin.clienteConvenio" type="text" class="w-full rounded-xl border-slate-200 text-xs bg-slate-50 focus:border-indigo-500" />
                </div>
              </div>

              <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
                <div class="md:col-span-2">
                  <label class="block font-bold uppercase text-slate-500 mb-1">Logradouro</label>
                  <input v-model="formEdicaoAdmin.clienteLogradouro" type="text" class="w-full rounded-xl border-slate-200 text-xs bg-slate-50 focus:border-indigo-500" />
                </div>
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Número</label>
                  <input v-model="formEdicaoAdmin.clienteNumero" type="text" class="w-full rounded-xl border-slate-200 text-xs bg-slate-50 focus:border-indigo-500" />
                </div>
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Bairro</label>
                  <input v-model="formEdicaoAdmin.clienteBairro" type="text" class="w-full rounded-xl border-slate-200 text-xs bg-slate-50 focus:border-indigo-500" />
                </div>
              </div>

              <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Cidade</label>
                  <input v-model="formEdicaoAdmin.clienteCidade" type="text" class="w-full rounded-xl border-slate-200 text-xs bg-slate-50 focus:border-indigo-500" />
                </div>
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Estado (UF)</label>
                  <input v-model="formEdicaoAdmin.clienteEstado" type="text" maxlength="2" class="w-full rounded-xl border-slate-200 text-xs uppercase bg-slate-50 focus:border-indigo-500" />
                </div>
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">CEP</label>
                  <input v-model="formEdicaoAdmin.clienteCep" type="text" class="w-full rounded-xl border-slate-200 text-xs font-mono bg-slate-50 focus:border-indigo-500" />
                </div>
              </div>
            </div>

            <!-- ABA 3: RECEITA & MEDIDAS -->
            <div v-show="abaEdicaoAdmin === 'receita'" class="space-y-4 text-xs">
              <div class="p-3 bg-slate-50 rounded-xl border border-slate-200 space-y-3">
                <span class="font-black uppercase text-slate-700 tracking-wider block">Grau de Longe</span>
                <div class="grid grid-cols-4 gap-2 text-center font-bold text-slate-400">
                  <div>Olho</div><div>Esférico</div><div>Cilíndrico</div><div>Eixo</div>
                </div>
                <div class="grid grid-cols-4 gap-2 items-center">
                  <span class="font-black text-center text-slate-800">OD</span>
                  <input v-model.number="formEdicaoAdmin.odEsferico" type="number" step="0.25" class="rounded-xl border-slate-200 text-xs text-center font-mono bg-white" />
                  <input v-model.number="formEdicaoAdmin.odCilindrico" type="number" step="0.25" class="rounded-xl border-slate-200 text-xs text-center font-mono text-amber-700 bg-white" />
                  <input v-model.number="formEdicaoAdmin.odEixo" type="number" min="0" max="180" class="rounded-xl border-slate-200 text-xs text-center font-mono bg-white" />
                </div>
                <div class="grid grid-cols-4 gap-2 items-center">
                  <span class="font-black text-center text-slate-800">OE</span>
                  <input v-model.number="formEdicaoAdmin.oeEsferico" type="number" step="0.25" class="rounded-xl border-slate-200 text-xs text-center font-mono bg-white" />
                  <input v-model.number="formEdicaoAdmin.oeCilindrico" type="number" step="0.25" class="rounded-xl border-slate-200 text-xs text-center font-mono text-amber-700 bg-white" />
                  <input v-model.number="formEdicaoAdmin.oeEixo" type="number" min="0" max="180" class="rounded-xl border-slate-200 text-xs text-center font-mono bg-white" />
                </div>
              </div>

              <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Adição (AD)</label>
                  <input v-model.number="formEdicaoAdmin.adicao" type="number" step="0.25" min="0" max="3.5" class="w-full rounded-xl border-slate-200 text-xs font-mono font-bold text-teal-800 bg-slate-50" />
                </div>
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">DNP OD (mm)</label>
                  <input v-model.number="formEdicaoAdmin.dnpOd" type="number" step="0.5" class="w-full rounded-xl border-slate-200 text-xs font-mono bg-slate-50" />
                </div>
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">DNP OE (mm)</label>
                  <input v-model.number="formEdicaoAdmin.dnpOe" type="number" step="0.5" class="w-full rounded-xl border-slate-200 text-xs font-mono bg-slate-50" />
                </div>
              </div>

              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Altura Mont. OD (mm)</label>
                  <input v-model.number="formEdicaoAdmin.alturaMontagemOd" type="number" step="0.5" class="w-full rounded-xl border-slate-200 text-xs font-mono bg-slate-50" />
                </div>
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Altura Mont. OE (mm)</label>
                  <input v-model.number="formEdicaoAdmin.alturaMontagemOe" type="number" step="0.5" class="w-full rounded-xl border-slate-200 text-xs font-mono bg-slate-50" />
                </div>
              </div>

              <div class="p-3 bg-slate-50 rounded-xl border border-slate-200 space-y-2">
                <span class="font-black uppercase text-slate-700 tracking-wider block">Medidas da Armação (Laboratório)</span>
                <div class="grid grid-cols-3 md:grid-cols-6 gap-2">
                  <div>
                    <label class="block text-[9px] uppercase font-bold text-slate-400">ARO</label>
                    <input v-model.number="formEdicaoAdmin.aro" type="number" step="0.5" class="w-full rounded-xl border-slate-200 text-xs text-center font-mono bg-white" />
                  </div>
                  <div>
                    <label class="block text-[9px] uppercase font-bold text-slate-400">DM</label>
                    <input v-model.number="formEdicaoAdmin.dm" type="number" step="0.5" class="w-full rounded-xl border-slate-200 text-xs text-center font-mono bg-white" />
                  </div>
                  <div>
                    <label class="block text-[9px] uppercase font-bold text-slate-400">VERT</label>
                    <input v-model.number="formEdicaoAdmin.vert" type="number" step="0.5" class="w-full rounded-xl border-slate-200 text-xs text-center font-mono bg-white" />
                  </div>
                  <div>
                    <label class="block text-[9px] uppercase font-bold text-slate-400">PO</label>
                    <input v-model.number="formEdicaoAdmin.po" type="number" step="0.5" class="w-full rounded-xl border-slate-200 text-xs text-center font-mono bg-white" />
                  </div>
                  <div>
                    <label class="block text-[9px] uppercase font-bold text-slate-400">CO OD</label>
                    <input v-model.number="formEdicaoAdmin.coOd" type="number" step="0.5" class="w-full rounded-xl border-slate-200 text-xs text-center font-mono bg-white" />
                  </div>
                  <div>
                    <label class="block text-[9px] uppercase font-bold text-slate-400">CO OE</label>
                    <input v-model.number="formEdicaoAdmin.coOe" type="number" step="0.5" class="w-full rounded-xl border-slate-200 text-xs text-center font-mono bg-white" />
                  </div>
                </div>
              </div>
            </div>

            <!-- ABA 4: PRODUTOS & FINANCEIRO COM RECÁLCULO AUTOMÁTICO -->
            <div v-show="abaEdicaoAdmin === 'financeiro'" class="space-y-4 text-xs">
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Descrição / Modelo da Armação</label>
                  <input v-model="formEdicaoAdmin.armacaoDescricaoManual" type="text" placeholder="Ex: Ray-Ban Aviador Dourado" class="w-full rounded-xl border-slate-200 text-xs bg-slate-50 focus:border-indigo-500" />
                </div>
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Descrição / Tabela da Lente</label>
                  <input v-model="formEdicaoAdmin.lenteDescricaoManual" type="text" placeholder="Ex: Multifocal Varilux Comfort" class="w-full rounded-xl border-slate-200 text-xs bg-slate-50 focus:border-indigo-500" />
                </div>
              </div>

              <!-- Grade de Valores Financeiros com Recálculo Dinâmico (Regra 3 do PDF) -->
              <div class="p-4 bg-slate-50 rounded-2xl border border-slate-200 space-y-4">
                <span class="font-black uppercase text-slate-800 tracking-wider block">Valores e Cadeia de Cálculo Automática</span>
                <div class="grid grid-cols-2 md:grid-cols-4 gap-3">
                  <div>
                    <label class="block font-bold uppercase text-slate-500 mb-1">Valor Armação (R$)</label>
                    <input v-model.number="formEdicaoAdmin.valorArmacao" type="number" step="0.01" min="0" class="w-full rounded-xl border-slate-200 text-xs font-mono font-bold bg-white focus:border-indigo-500" />
                  </div>
                  <div>
                    <label class="block font-bold uppercase text-slate-500 mb-1">Valor Lente (R$)</label>
                    <input v-model.number="formEdicaoAdmin.valorLente" type="number" step="0.01" min="0" class="w-full rounded-xl border-slate-200 text-xs font-mono font-bold bg-white focus:border-indigo-500" />
                  </div>
                  <div>
                    <label class="block font-bold uppercase text-slate-500 mb-1">Desconto (R$)</label>
                    <input v-model.number="formEdicaoAdmin.descontoReais" type="number" step="0.01" min="0" class="w-full rounded-xl border-slate-200 text-xs font-mono font-bold text-amber-700 bg-white focus:border-indigo-500" />
                  </div>
                  <div>
                    <label class="block font-bold uppercase text-slate-500 mb-1">Valor Entrada (R$)</label>
                    <input v-model.number="formEdicaoAdmin.valorEntrada" type="number" step="0.01" min="0" class="w-full rounded-xl border-slate-200 text-xs font-mono font-bold text-teal-700 bg-white focus:border-indigo-500" />
                  </div>
                </div>

                <!-- Painel de Conferência da Cadeia -->
                <div class="p-3 bg-white rounded-xl border border-slate-200 grid grid-cols-2 md:grid-cols-4 gap-2 font-mono text-center">
                  <div class="p-2 bg-slate-50 rounded-lg">
                    <span class="block text-[9px] text-slate-400 uppercase font-bold">Total Bruto</span>
                    <b class="text-slate-800">{{ formatarMoeda(valorBrutoEdicaoAdmin) }}</b>
                  </div>
                  <div class="p-2 bg-amber-50 rounded-lg">
                    <span class="block text-[9px] text-amber-700 uppercase font-bold">Desconto</span>
                    <b class="text-amber-800">{{ formatarMoeda(formEdicaoAdmin.descontoReais) }}</b>
                  </div>
                  <div class="p-2 bg-indigo-50 rounded-lg">
                    <span class="block text-[9px] text-indigo-700 uppercase font-bold">Total Líquido</span>
                    <b class="text-indigo-900">{{ formatarMoeda(valorLiquidoEdicaoAdmin) }}</b>
                  </div>
                  <div class="p-2 bg-rose-50 rounded-lg">
                    <span class="block text-[9px] text-rose-700 uppercase font-bold">Saldo Restante</span>
                    <b :class="valorRestanteEdicaoAdmin > 0 ? 'text-rose-700' : 'text-emerald-700'">
                      {{ formatarMoeda(valorRestanteEdicaoAdmin) }}
                    </b>
                  </div>
                </div>
              </div>

              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Forma de Pagamento</label>
                  <select v-model="formEdicaoAdmin.formaPagamento" class="w-full rounded-xl border-slate-200 text-xs bg-slate-50 focus:border-indigo-500">
                    <option value="DINHEIRO">Dinheiro</option>
                    <option value="PIX">Pix</option>
                    <option value="CARTAO_CREDITO">Cartão de Crédito</option>
                    <option value="CARTAO_DEBITO">Cartão de Débito</option>
                    <option value="BOLETO">Boleto</option>
                  </select>
                </div>
                <div>
                  <label class="block font-bold uppercase text-slate-500 mb-1">Parcelas</label>
                  <input v-model.number="formEdicaoAdmin.parcelas" type="number" min="1" max="24" class="w-full rounded-xl border-slate-200 text-xs font-mono bg-slate-50 focus:border-indigo-500" />
                </div>
              </div>
            </div>

            <!-- MOTIVO DO AJUSTE & AVISOS (SEMPRE VISÍVEIS) -->
            <div class="pt-4 border-t border-slate-200 space-y-3">
              <div>
                <label class="block font-bold uppercase text-slate-700 mb-1">
                  Motivo da Alteração / Justificativa (Obrigatório para Auditoria) *
                </label>
                <input 
                  v-model="formEdicaoAdmin.motivo" 
                  type="text" 
                  placeholder="Ex: Correção de receita médica solicitada pelo cliente / ajuste de valor" 
                  required 
                  class="w-full rounded-xl border-slate-300 text-xs text-slate-900 bg-slate-50 focus:border-indigo-500 focus:ring-indigo-500"
                />
              </div>

              <div class="p-3 bg-amber-50 border border-amber-200 rounded-xl text-[11px] text-amber-900 space-y-1 font-sans">
                <p>⚠️ <b>Regras Mandatórias:</b></p>
                <p>• Toda alteração é registrada no <b>log de auditoria visível</b> com nome do responsável e data/hora.</p>
                <p>• O registro de comissão da vendedora é <b>automaticamente recalculado</b> com o novo valor líquido.</p>
                <p>• Se a OS pertencer a um período contábil <b>já fechado e pago</b>, alterações financeiras e de data serão bloqueadas.</p>
              </div>
            </div>

            <!-- Rodapé / Ações da Modal -->
            <div class="flex items-center justify-between pt-4 border-t border-slate-100">
              <span class="text-[11px] text-slate-400 font-mono">
                Ação restrita a Administradores
              </span>
              <div class="flex gap-2">
                <button 
                  type="button" 
                  @click="modalEditarAdminAberta = false" 
                  class="px-5 py-2.5 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-xl font-bold transition text-xs"
                >
                  Cancelar
                </button>
                <button 
                  type="submit" 
                  :disabled="processandoEdicaoAdmin" 
                  class="px-6 py-2.5 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white rounded-xl font-bold transition shadow-sm text-xs flex items-center gap-1.5"
                >
                  <span v-if="processandoEdicaoAdmin">Processando Alterações...</span>
                  <span v-else>💾 Gravar Alterações da OS</span>
                </button>
              </div>
            </div>
          </form>
        </div>
      </div>

    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { ref, computed } from 'vue'
import { router, usePage, useForm } from '@inertiajs/vue3'
import axios from 'axios'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  Ordens: Array, ordens: Array,
  Vendedores: Array, vendedores: Array,
  Lojas: Array, lojas: Array,
  FiltroComposicao: String, filtroComposicao: String,
  FiltroVendedorId: String, filtroVendedorId: String,
  FiltroLojaId: String, filtroLojaId: String,
  Busca: String, busca: String,
  TotalFaturamento: Number, totalFaturamento: Number,
  FiltroAtivo: String, filtroAtivo: String,
  VendedorFiltro: String, vendedorFiltro: String,
  TotalFiltroAtivo: Number, totalFiltroAtivo: Number
})

const page = usePage()
const eAdmin = computed(() => {
  const perfil = (page.props.auth?.usuarioPerfil || '').toLowerCase()
  return ['admin', 'sistema', 'gerente'].includes(perfil)
})

// Filtros Reativos (Seção 3 do PDF)
const termoBusca = ref(props.Busca ?? props.busca ?? '')
const lojaSelecionada = ref(props.FiltroLojaId ?? props.filtroLojaId ?? '')
const vendedorSelecionado = ref(props.FiltroVendedorId ?? props.filtroVendedorId ?? props.VendedorFiltro ?? props.vendedorFiltro ?? '')
const filtroAtivoComp = computed(() => props.FiltroComposicao ?? props.filtroComposicao ?? props.FiltroAtivo ?? props.filtroAtivo ?? 'total')
const totalExibido = computed(() => props.TotalFaturamento ?? props.totalFaturamento ?? props.TotalFiltroAtivo ?? props.totalFiltroAtivo ?? 0)

const tipoBuscaDetectado = computed(() => {
  const val = (termoBusca.value || '').trim()
  if (!val) return null
  return /^\d+$/.test(val) ? 'NUMERO' : 'CLIENTE'
})

let timerBusca = null
const onInputBusca = () => {
  if (timerBusca) clearTimeout(timerBusca)
  timerBusca = setTimeout(() => {
    const val = (termoBusca.value || '').trim()
    const ehNumero = /^\d+$/.test(val)
    if (ehNumero || val.length >= 3 || val.length === 0) {
      aplicarFiltros()
    }
  }, 300)
}

const limparBusca = () => {
  termoBusca.value = ''
  aplicarFiltros()
}

const aplicarFiltros = (tipoComposicao = null) => {
  const comp = tipoComposicao !== null ? tipoComposicao : filtroAtivoComp.value
  const params = {}
  if (comp && comp !== 'total') params.filtroComposicao = comp
  if (vendedorSelecionado.value) params.vendedorId = vendedorSelecionado.value
  if (lojaSelecionada.value) params.lojaId = lojaSelecionada.value
  if (termoBusca.value && termoBusca.value.trim().length > 0) params.busca = termoBusca.value.trim()

  router.get('/ordens', params, { preserveState: true, replace: true })
}

const filtrarPorComposicao = (tipo) => aplicarFiltros(tipo)
const filtrarPorVendedor = () => aplicarFiltros()

// Modal de Detalhes / Prancheta Clínica e Auditoria (Seção 1 do PDF)
const osSelecionada = ref(null)
const logsAuditoria = ref([])
const carregandoAuditoria = ref(false)

const abrirPranchetaClinica = async (ordem) => { 
  osSelecionada.value = { ...ordem } 
  logsAuditoria.value = []
  carregandoAuditoria.value = true
  try {
    const id = ordem.id || ordem.Id
    const { data } = await axios.get(`/api/ordens/${id}/auditoria`)
    logsAuditoria.value = data || []
  } catch {
    logsAuditoria.value = []
  } finally {
    carregandoAuditoria.value = false
  }
}

// Modal de Entrega & Quitação
const modalEntregaAberta = ref(false)
const osParaEntrega = ref(null)
const processandoEntrega = ref(false)
const formEntrega = ref({
  opcaoQuitacao: 'PAGO_RETIRADA',
  formaPagamentoRetirada: 'DINHEIRO',
  parcelasRetirada: 1
})

// Modal de Edição Administrativa Completa (Seção 1 do PDF)
const modalEditarAdminAberta = ref(false)
const osParaEdicaoAdmin = ref(null)
const processandoEdicaoAdmin = ref(false)
const abaEdicaoAdmin = ref('geral')

const formEdicaoAdmin = useForm({
  numeroOS: '',
  novaDataEntrada: '',
  dataPrevistaEntrega: '',
  dataEntregaReal: '',
  lojaVenda: '',
  oticaId: '',
  vendedorId: '',
  medicoNome: '',
  medicoCrm: '',
  medicoTipo: 'NAO_ESPECIFICADO',
  observacoes: '',
  motivo: '',
  status: 'LANCADA',

  clienteNome: '',
  clienteCpf: '',
  clienteTelefone: '',
  clienteDataNascimento: '',
  clienteEmail: '',
  clienteConvenio: '',
  clienteLogradouro: '',
  clienteNumero: '',
  clienteBairro: '',
  clienteCidade: '',
  clienteEstado: '',
  clienteCep: '',

  odEsferico: null,
  odCilindrico: null,
  odEixo: null,
  oeEsferico: null,
  oeCilindrico: null,
  oeEixo: null,
  adicao: null,
  dnpOd: null,
  dnpOe: null,
  alturaMontagemOd: null,
  alturaMontagemOe: null,
  aro: null,
  dm: null,
  vert: null,
  po: null,
  coOd: null,
  coOe: null,
  obsReceita: '',

  armacaoDescricaoManual: '',
  lenteDescricaoManual: '',

  valorArmacao: 0,
  valorLente: 0,
  descontoReais: 0,
  valorTotalLiquido: 0,
  valorEntrada: 0,
  formaPagamento: 'DINHEIRO',
  parcelas: 1
})

const valorBrutoEdicaoAdmin = computed(() => {
  return Number(formEdicaoAdmin.valorArmacao || 0) + Number(formEdicaoAdmin.valorLente || 0)
})

const valorLiquidoEdicaoAdmin = computed(() => {
  const bruto = valorBrutoEdicaoAdmin.value
  let desc = Number(formEdicaoAdmin.descontoReais || 0)
  if (desc > bruto) desc = bruto
  return Math.max(0, bruto - desc)
})

const valorRestanteEdicaoAdmin = computed(() => {
  const liq = valorLiquidoEdicaoAdmin.value
  let ent = Number(formEdicaoAdmin.valorEntrada || 0)
  if (ent > liq) ent = liq
  return Math.max(0, liq - ent)
})

const abrirModalEditarAdmin = (os) => {
  osParaEdicaoAdmin.value = os
  abaEdicaoAdmin.value = 'geral'

  const dataRaw = os.dataEntrada || os.DataEntrada
  const dataIso = dataRaw ? new Date(dataRaw).toISOString().split('T')[0] : ''
  const prevRaw = os.dataPrevistaEntrega || os.DataPrevistaEntrega
  const prevIso = prevRaw ? new Date(prevRaw).toISOString().split('T')[0] : ''
  const realRaw = os.dataEntregaReal || os.DataEntregaReal
  const realIso = realRaw ? new Date(realRaw).toISOString().split('T')[0] : ''

  const cli = os.cliente || os.Cliente || {}
  const cliNascRaw = cli.dataNascimento || cli.DataNascimento
  const cliNascIso = cliNascRaw ? new Date(cliNascRaw).toISOString().split('T')[0] : ''

  const rec = os.receita || os.Receita || {}
  const fin = os.financeiro || os.Financeiro || {}

  formEdicaoAdmin.numeroOS = os.numeroOS || os.NumeroOS || ''
  formEdicaoAdmin.novaDataEntrada = dataIso
  formEdicaoAdmin.dataPrevistaEntrega = prevIso
  formEdicaoAdmin.dataEntregaReal = realIso
  formEdicaoAdmin.lojaVenda = os.lojaVenda || os.LojaVenda || ''
  formEdicaoAdmin.oticaId = os.oticaId || os.OticaId || ''
  formEdicaoAdmin.vendedorId = os.vendedorId || os.VendedorId || ''
  formEdicaoAdmin.medicoNome = os.medico || os.medicoNome || os.MedicoNome || ''
  formEdicaoAdmin.medicoCrm = os.medicoCrm || os.MedicoCrm || ''
  formEdicaoAdmin.medicoTipo = os.medicoTipo || os.MedicoTipo || 'NAO_ESPECIFICADO'
  formEdicaoAdmin.observacoes = os.observacoes || os.Observacoes || ''
  formEdicaoAdmin.motivo = ''
  formEdicaoAdmin.status = (os.status || os.Status || 'LANCADA').toUpperCase()

  formEdicaoAdmin.clienteNome = cli.nome || cli.Nome || os.clienteNome || os.ClienteNome || ''
  formEdicaoAdmin.clienteCpf = cli.cpf || cli.Cpf || cli.CPF || ''
  formEdicaoAdmin.clienteTelefone = cli.telefone || cli.Telefone || ''
  formEdicaoAdmin.clienteDataNascimento = cliNascIso
  formEdicaoAdmin.clienteEmail = cli.email || cli.Email || ''
  formEdicaoAdmin.clienteConvenio = cli.convenio || cli.Convenio || ''
  formEdicaoAdmin.clienteLogradouro = cli.logradouro || cli.Logradouro || ''
  formEdicaoAdmin.clienteNumero = cli.numero || cli.Numero || ''
  formEdicaoAdmin.clienteBairro = cli.bairro || cli.Bairro || ''
  formEdicaoAdmin.clienteCidade = cli.cidade || cli.Cidade || ''
  formEdicaoAdmin.clienteEstado = cli.estado || cli.Estado || ''
  formEdicaoAdmin.clienteCep = cli.cep || cli.Cep || ''

  formEdicaoAdmin.odEsferico = rec.odEsferico !== undefined && rec.odEsferico !== null ? Number(rec.odEsferico) : null
  formEdicaoAdmin.odCilindrico = rec.odCilindrico !== undefined && rec.odCilindrico !== null ? Number(rec.odCilindrico) : null
  formEdicaoAdmin.odEixo = rec.odEixo !== undefined && rec.odEixo !== null ? Number(rec.odEixo) : null
  formEdicaoAdmin.oeEsferico = rec.oeEsferico !== undefined && rec.oeEsferico !== null ? Number(rec.oeEsferico) : null
  formEdicaoAdmin.oeCilindrico = rec.oeCilindrico !== undefined && rec.oeCilindrico !== null ? Number(rec.oeCilindrico) : null
  formEdicaoAdmin.oeEixo = rec.oeEixo !== undefined && rec.oeEixo !== null ? Number(rec.oeEixo) : null
  formEdicaoAdmin.adicao = rec.adicao !== undefined && rec.adicao !== null ? Number(rec.adicao) : null
  formEdicaoAdmin.dnpOd = rec.dnpOd !== undefined && rec.dnpOd !== null ? Number(rec.dnpOd) : null
  formEdicaoAdmin.dnpOe = rec.dnpOe !== undefined && rec.dnpOe !== null ? Number(rec.dnpOe) : null
  formEdicaoAdmin.alturaMontagemOd = rec.alturaMontagemOd !== undefined && rec.alturaMontagemOd !== null ? Number(rec.alturaMontagemOd) : null
  formEdicaoAdmin.alturaMontagemOe = rec.alturaMontagemOe !== undefined && rec.alturaMontagemOe !== null ? Number(rec.alturaMontagemOe) : null
  formEdicaoAdmin.aro = rec.aro !== undefined && rec.aro !== null ? Number(rec.aro) : null
  formEdicaoAdmin.dm = rec.dm !== undefined && rec.dm !== null ? Number(rec.dm) : null
  formEdicaoAdmin.vert = rec.vert !== undefined && rec.vert !== null ? Number(rec.vert) : null
  formEdicaoAdmin.po = rec.po !== undefined && rec.po !== null ? Number(rec.po) : null
  formEdicaoAdmin.coOd = rec.coOd !== undefined && rec.coOd !== null ? Number(rec.coOd) : null
  formEdicaoAdmin.coOe = rec.coOe !== undefined && rec.coOe !== null ? Number(rec.coOe) : null
  formEdicaoAdmin.obsReceita = rec.obsReceita || ''

  formEdicaoAdmin.armacaoDescricaoManual = os.armacaoModeloManual || os.ArmacaoModeloManual || ''
  formEdicaoAdmin.lenteDescricaoManual = os.lenteDescricaoManual || os.LenteDescricaoManual || ''

  formEdicaoAdmin.valorArmacao = Number(fin.valorArmacao || fin.ValorArmacao || 0)
  formEdicaoAdmin.valorLente = Number(fin.valorLente || fin.ValorLente || 0)
  formEdicaoAdmin.descontoReais = Number(fin.descontoReais || fin.DescontoReais || 0)
  formEdicaoAdmin.valorTotalLiquido = Number(fin.valorTotalLiquido || fin.ValorTotalLiquido || os.valorTotal || os.ValorTotal || 0)
  formEdicaoAdmin.valorEntrada = Number(fin.valorEntrada || fin.ValorEntrada || 0)
  formEdicaoAdmin.formaPagamento = fin.formaPagamento || fin.FormaPagamento || 'DINHEIRO'
  formEdicaoAdmin.parcelas = Number(fin.parcelas || fin.Parcelas || 1)

  modalEditarAdminAberta.value = true
}

const salvarEdicaoAdmin = () => {
  if (!osParaEdicaoAdmin.value) return
  const id = osParaEdicaoAdmin.value.id || osParaEdicaoAdmin.value.Id
  processandoEdicaoAdmin.value = true
  formEdicaoAdmin.valorTotalLiquido = valorLiquidoEdicaoAdmin.value

  formEdicaoAdmin.post(`/ordens/editar-admin/${id}`, {
    preserveScroll: true,
    onSuccess: () => {
      processandoEdicaoAdmin.value = false
      modalEditarAdminAberta.value = false
      osParaEdicaoAdmin.value = null
      alert('✅ Ordem de Serviço atualizada com sucesso pelo administrador!')
    },
    onError: (errors) => {
      processandoEdicaoAdmin.value = false
      const msg = errors?.erro || errors?.mensagem || 'Erro ao salvar alterações da OS. Verifique se o período está fechado.'
      alert(`⚠️ ${msg}`)
    }
  })
}

const receitaObj = computed(() => osSelecionada.value?.receita || osSelecionada.value?.Receita || {})
const financeiroObj = computed(() => osSelecionada.value?.financeiro || osSelecionada.value?.Financeiro || {})
const parcelasLista = computed(() => osSelecionada.value?.parcelas || osSelecionada.value?.Parcelas || [])

const irParaNovaOrdem = () => router.get('/ordens/nova')

const temLente = (os) => {
  const fin = os?.financeiro || os?.Financeiro
  return (fin && Number(fin.valorLente || fin.ValorLente || 0) > 0) || Boolean(os?.lenteDescricaoManual || os?.LenteDescricaoManual)
}

const obterValorRestante = (os) => {
  const fin = os?.financeiro || os?.Financeiro
  if (!fin) return 0
  if (fin.valorRestante !== undefined && fin.valorRestante !== null) return Number(fin.valorRestante)
  if (fin.ValorRestante !== undefined && fin.ValorRestante !== null) return Number(fin.ValorRestante)
  const liq = Number(fin.valorTotalLiquido || fin.ValorTotalLiquido || os?.valorTotal || os?.ValorTotal || 0)
  const ent = Number(fin.valorEntrada || fin.ValorEntrada || 0)
  return Math.max(0, liq - ent)
}

// Marcar Lente Pedida
const marcarLentePedida = (id) => {
  if (!id) return
  if (confirm('Confirmar que as lentes desta OS foram encomendadas ao laboratório?')) {
    router.post(`/ordens/marcar-lente-pedida/${id}`, {}, {
      preserveScroll: true,
      onSuccess: () => {
        alert('📦 Lente marcada como pedida!')
        if (osSelecionada.value && (osSelecionada.value.id || osSelecionada.value.Id) === id) {
          osSelecionada.value.lentePedida = true
          osSelecionada.value.dataPedidoLente = new Date().toISOString()
        }
      }
    })
  }
}

const alterarStatusOS = (id, novoStatus) => {
  if (!id || !novoStatus) return
  if (novoStatus === 'ENTREGUE') {
    const lista = props.Ordens ?? props.ordens ?? []
    const os = lista.find(o => (o.id || o.Id) === id)
    if (os) abrirModalEntrega(os)
    return
  }
  router.post(`/ordens/alterar-status/${id}`, { novoStatus }, {
    preserveScroll: true,
    onSuccess: () => alert('Status da OS atualizado com sucesso!')
  })
}

// Quitação e Entrega
const abrirModalEntrega = (os) => {
  osParaEntrega.value = os
  formEntrega.value = {
    opcaoQuitacao: 'PAGO_RETIRADA',
    formaPagamentoRetirada: 'DINHEIRO',
    parcelasRetirada: 1
  }
  modalEntregaAberta.value = true
}

const confirmarEntregaEQuitacao = () => {
  if (!osParaEntrega.value) return
  const id = osParaEntrega.value.id || osParaEntrega.value.Id
  processandoEntrega.value = true

  router.post(`/ordens/quitar-e-entregar/${id}`, formEntrega.value, {
    preserveScroll: true,
    onSuccess: () => {
      processandoEntrega.value = false
      modalEntregaAberta.value = false
      osSelecionada.value = null
      alert('🚚 Ordem de Serviço entregue com sucesso!')
    },
    onError: () => {
      processandoEntrega.value = false
    }
  })
}

// WhatsApp Direto
const abrirWhatsAppOS = async (os) => {
  const id = os.id || os.Id
  const statusAtual = (os.status || '').toUpperCase()
  const tipo = (statusAtual === 'PRONTA' || statusAtual === 'PRONTO') ? 'pronto' : 'cadastro'

  try {
    const { data } = await axios.get(`/api/ordens/${id}/whatsapp-info?tipo=${tipo}`)
    if (data.urlWhatsApp) {
      window.open(data.urlWhatsApp, '_blank')
      await axios.post(`/api/ordens/${id}/marcar-whatsapp-enviado`, { tipo })
    } else {
      alert(`Cliente não possui telefone válido para WhatsApp cadastrado.\nMensagem gerada:\n\n${data.mensagem}`)
    }
  } catch {
    alert('Erro ao gerar mensagem de WhatsApp.')
  }
}

// Cancelamento
const cancelarOS = (id, numero) => {
  if (confirm(`Deseja realmente CANCELAR a OS ${numero}? O estoque da armação será estornado e a venda será desconsiderada dos relatórios e comissões.`)) {
    router.post(`/ordens/cancelar/${id}`, {}, {
      preserveScroll: true,
      onSuccess: () => alert(`OS ${numero} cancelada com sucesso!`)
    })
  }
}

const formatarMoeda = (valor) => {
  if (valor === undefined || valor === null) return 'R$ 0,00'
  return Number(valor).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

const formatarGrau = (valor) => {
  if (valor === undefined || valor === null) return '0,00'
  const num = Number(valor)
  return (num > 0 ? '+' : '') + num.toFixed(2)
}

const formatarData = (dataRaw) => {
  if (!dataRaw) return '--/--/----'
  return new Date(dataRaw).toLocaleDateString('pt-BR')
}
</script>
