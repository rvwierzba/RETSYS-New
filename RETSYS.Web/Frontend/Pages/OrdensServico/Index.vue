<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-7xl mx-auto">

      <!-- CABEÇALHO DO PAINEL DE OS (PADRÃO SSÓTICA / PWI) -->
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 bg-white p-6 rounded-3xl border border-slate-200 shadow-sm">
        <div>
          <div class="flex items-center gap-3">
            <span class="w-10 h-10 rounded-2xl bg-teal-500/10 text-teal-600 flex items-center justify-center text-xl font-black border border-teal-500/20">
              📋
            </span>
            <div>
              <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight">Gestão de Ordens de Serviço (OS)</h1>
              <p class="text-xs text-slate-500 mt-0.5">Controle completo de receitas, pedidos de lentes ao laboratório, carnês e entregas.</p>
            </div>
          </div>
        </div>

        <div class="flex flex-wrap items-center gap-2">
          <!-- Seletor de Visão Rápida -->
          <div class="flex items-center bg-slate-100 p-1 rounded-2xl border border-slate-200 text-xs font-mono">
            <span class="bg-slate-950 text-white px-3.5 py-1.5 rounded-xl font-black shadow-sm flex items-center gap-1.5">
              <span>📋</span> Lista
            </span>
            <Link 
              href="/laboratorio" 
              class="text-slate-600 hover:text-slate-950 px-3 py-1.5 rounded-xl transition flex items-center gap-1.5 font-bold"
              title="Esteira Especialista de Laboratório"
            >
              <span>🔬</span> Laboratório
            </Link>
            <Link 
              href="/ordens/kanban" 
              class="text-slate-600 hover:text-slate-950 px-3 py-1.5 rounded-xl transition flex items-center gap-1.5 font-bold"
              title="Quadro Kanban de Produção"
            >
              <span>📊</span> Kanban
            </Link>
          </div>

          <Link 
            href="/ordens/nova"
            class="bg-teal-600 hover:bg-teal-700 text-white font-bold py-2 px-5 rounded-2xl text-xs transition shadow-sm uppercase tracking-wider flex items-center gap-1.5"
          >
            <span>+</span> Emitir Nova OS
          </Link>
        </div>
      </div>

      <!-- CARDS RESUMO DE STATUS (MÉTRICAS RÁPIDAS NO TOPO) -->
      <div class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-3">
        <button 
          @click="filtrarPorStatusRapido('')"
          :class="filtroStatusSelecionado === '' ? 'ring-2 ring-teal-500 border-teal-500 bg-teal-50/50' : 'bg-white hover:bg-slate-50 border-slate-200'"
          class="p-3.5 rounded-2xl border shadow-xs text-left transition flex flex-col justify-between"
        >
          <span class="text-[10px] font-mono font-bold uppercase text-slate-400">Todas as OS</span>
          <p class="text-xl font-black text-slate-900 mt-1 font-mono">{{ contadores.todas }}</p>
          <span class="text-[9px] text-teal-700 font-bold mt-1">Ver Todas</span>
        </button>

        <button 
          @click="filtrarPorStatusRapido('AGUARDANDO_LENTE')"
          :class="filtroStatusSelecionado === 'AGUARDANDO_LENTE' ? 'ring-2 ring-amber-500 border-amber-500 bg-amber-50/50' : 'bg-white hover:bg-slate-50 border-slate-200'"
          class="p-3.5 rounded-2xl border shadow-xs text-left transition flex flex-col justify-between"
        >
          <span class="text-[10px] font-mono font-bold uppercase text-amber-600">Aguard. Lente</span>
          <p class="text-xl font-black text-amber-700 mt-1 font-mono">{{ contadores.aguardandoLente }}</p>
          <span class="text-[9px] text-amber-600 font-bold mt-1">Pendente Lab</span>
        </button>

        <button 
          @click="filtrarPorStatusRapido('EM_MONTAGEM')"
          :class="filtroStatusSelecionado === 'EM_MONTAGEM' ? 'ring-2 ring-indigo-500 border-indigo-500 bg-indigo-50/50' : 'bg-white hover:bg-slate-50 border-slate-200'"
          class="p-3.5 rounded-2xl border shadow-xs text-left transition flex flex-col justify-between"
        >
          <span class="text-[10px] font-mono font-bold uppercase text-indigo-600">Em Montagem</span>
          <p class="text-xl font-black text-indigo-700 mt-1 font-mono">{{ contadores.emMontagem }}</p>
          <span class="text-[9px] text-indigo-600 font-bold mt-1">Na Oficina</span>
        </button>

        <button 
          @click="filtrarPorStatusRapido('PRONTA')"
          :class="filtroStatusSelecionado === 'PRONTA' ? 'ring-2 ring-emerald-500 border-emerald-500 bg-emerald-50/50' : 'bg-white hover:bg-slate-50 border-slate-200'"
          class="p-3.5 rounded-2xl border shadow-xs text-left transition flex flex-col justify-between"
        >
          <span class="text-[10px] font-mono font-bold uppercase text-emerald-600">Prontas</span>
          <p class="text-xl font-black text-emerald-700 mt-1 font-mono">{{ contadores.prontas }}</p>
          <span class="text-[9px] text-emerald-600 font-bold mt-1">Avisar Cliente</span>
        </button>

        <button 
          @click="filtrarPorStatusRapido('ENTREGUE')"
          :class="filtroStatusSelecionado === 'ENTREGUE' ? 'ring-2 ring-slate-800 border-slate-800 bg-slate-50' : 'bg-white hover:bg-slate-50 border-slate-200'"
          class="p-3.5 rounded-2xl border shadow-xs text-left transition flex flex-col justify-between"
        >
          <span class="text-[10px] font-mono font-bold uppercase text-slate-500">Entregues</span>
          <p class="text-xl font-black text-slate-800 mt-1 font-mono">{{ contadores.entregues }}</p>
          <span class="text-[9px] text-slate-400 font-bold mt-1">Finalizadas</span>
        </button>

        <div class="p-3.5 rounded-2xl border border-teal-200 bg-teal-50/70 shadow-xs flex flex-col justify-between">
          <span class="text-[10px] font-mono font-bold uppercase text-teal-800">Faturamento</span>
          <p class="text-base font-black text-teal-900 mt-1 font-mono truncate">
            R$ {{ formatarMoeda(totalExibido) }}
          </p>
          <span class="text-[9px] text-teal-700 font-bold mt-1">Total do Filtro</span>
        </div>
      </div>

      <!-- BARRA DE FILTROS & BUSCA RÁPIDA (PADRÃO PWI/SSÓTICA) -->
      <div class="bg-white p-5 rounded-3xl border border-slate-200 shadow-sm space-y-4">
        <div class="grid grid-cols-1 md:grid-cols-4 gap-3 items-center">
          
          <!-- Busca Unificada por Nº da OS ou Nome/CPF -->
          <div class="md:col-span-2 relative">
            <span class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">
              🔍
            </span>
            <input
              v-model="termoBusca"
              @input="onInputBusca"
              type="text"
              placeholder="Buscar por Nº da OS (ex: 123) ou Nome do Cliente (mín. 3 caracteres)..."
              class="w-full pl-10 pr-24 py-2.5 rounded-2xl border border-slate-200 text-xs font-semibold text-slate-800 focus:border-teal-500 focus:ring-teal-500 bg-slate-50"
            />
            <div class="absolute inset-y-0 right-0 pr-2.5 flex items-center gap-1">
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

          <!-- Filtro por Loja / Filial -->
          <div>
            <select
              v-model="lojaSelecionada"
              @change="aplicarFiltros()"
              class="w-full rounded-2xl border-slate-200 text-xs focus:border-teal-500 font-bold text-slate-700 bg-slate-50 py-2.5 px-3"
            >
              <option value="">🏢 (Todas as Lojas da Rede)</option>
              <option v-for="l in (Lojas ?? lojas)" :key="l.id || l.Id" :value="l.id || l.Id">
                {{ l.nome || l.Nome }}
              </option>
            </select>
          </div>

          <!-- Filtro por Vendedora / Atendente -->
          <div>
            <select 
              v-model="vendedorSelecionado" 
              @change="aplicarFiltros()"
              class="w-full rounded-2xl border-slate-200 text-xs focus:border-teal-500 font-bold text-slate-700 bg-slate-50 py-2.5 px-3"
            >
              <option value="">👩‍💼 (Todas as Vendedoras)</option>
              <option v-for="v in (Vendedores ?? vendedores)" :key="v.id || v.Id" :value="v.id || v.Id">
                {{ v.nome || v.Nome }}
              </option>
            </select>
          </div>

        </div>

        <!-- Chips de Composição do Pedido -->
        <div class="flex flex-wrap items-center justify-between gap-3 pt-3 border-t border-slate-100 text-xs font-mono">
          <div class="flex flex-wrap items-center gap-1.5">
            <span class="text-slate-400 font-bold mr-1 text-[11px] uppercase">Composição:</span>
            <button 
              @click="filtrarPorComposicao('total')"
              :class="filtroAtivoComp === 'total' ? 'bg-slate-950 text-white font-black' : 'bg-slate-100 text-slate-600 hover:bg-slate-200'"
              class="px-3 py-1.5 rounded-xl transition"
            >
              Total
            </button>
            <button 
              @click="filtrarPorComposicao('armacao')"
              :class="filtroAtivoComp === 'armacao' ? 'bg-slate-950 text-white font-black' : 'bg-slate-100 text-slate-600 hover:bg-slate-200'"
              class="px-3 py-1.5 rounded-xl transition"
            >
              👓 Armações
            </button>
            <button 
              @click="filtrarPorComposicao('lente')"
              :class="filtroAtivoComp === 'lente' ? 'bg-slate-950 text-white font-black' : 'bg-slate-100 text-slate-600 hover:bg-slate-200'"
              class="px-3 py-1.5 rounded-xl transition"
            >
              🔬 Lentes
            </button>
            <button 
              @click="filtrarPorComposicao('completo')"
              :class="filtroAtivoComp === 'completo' ? 'bg-slate-950 text-white font-black' : 'bg-slate-100 text-slate-600 hover:bg-slate-200'"
              class="px-3 py-1.5 rounded-xl transition"
            >
              💎 Completo (Armação + Lente)
            </button>
          </div>

          <span class="text-slate-400 text-[11px]">
            Exibindo <b>{{ (Ordens ?? ordens ?? []).length }}</b> ordens de serviço
          </span>
        </div>
      </div>

      <!-- TABELA DE ORDENS DE SERVIÇO REMODELADA -->
      <div class="bg-white rounded-3xl border border-slate-200 shadow-sm overflow-hidden">
        
        <div v-if="!(Ordens ?? ordens) || (Ordens ?? ordens).length === 0" class="text-center py-16 px-4 space-y-3">
          <span class="text-4xl block">🔍</span>
          <h3 class="text-base font-bold text-slate-800 font-mono">Nenhuma ordem de serviço localizada</h3>
          <p class="text-xs text-slate-400 max-w-sm mx-auto">
            Não foram encontradas ordens para os filtros selecionados ou busca realizada.
          </p>
          <button 
            @click="limparTodosFiltros"
            class="px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 text-xs font-bold rounded-xl transition font-mono"
          >
            Limpar Todos os Filtros
          </button>
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-left text-xs border-collapse">
            <thead>
              <tr class="border-b border-slate-100 bg-slate-50/70 text-slate-400 font-mono font-bold uppercase tracking-wider text-[11px]">
                <th class="py-3.5 px-4">Nº OS / Data</th>
                <th class="py-3.5 px-4">Cliente / Paciente</th>
                <th class="py-3.5 px-4">Lentes &amp; Laboratório</th>
                <th class="py-3.5 px-4 text-center">Status da OS</th>
                <th class="py-3.5 px-4 text-right">Valor / Saldo</th>
                <th class="py-3.5 px-4 text-center">Ações Rápidas</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100 font-sans">
              <tr 
                v-for="os in (Ordens ?? ordens)" 
                :key="os.id || os.Id" 
                class="hover:bg-slate-50/80 transition group"
              >
                <!-- 1. Documento e Datas -->
                <td class="py-3.5 px-4 font-mono">
                  <span class="inline-block px-2.5 py-1 bg-slate-100 text-slate-900 rounded-lg font-black text-xs border border-slate-200">
                    #{{ os.numeroOS || os.NumeroOS }}
                  </span>
                  <p class="text-[11px] text-slate-400 mt-1">
                    {{ formatarData(os.dataVenda || os.DataVenda || os.dataEntrada || os.DataEntrada) }}
                  </p>
                  <span v-if="os.lojaVenda || os.LojaVenda" class="text-[9px] text-teal-800 bg-teal-50 px-1.5 py-0.5 rounded border border-teal-200 block w-fit mt-1">
                    🏢 {{ os.lojaVenda || os.LojaVenda }}
                  </span>
                </td>

                <!-- 2. Cliente e Contato -->
                <td class="py-3.5 px-4">
                  <p class="font-bold text-slate-900 text-sm leading-tight">{{ os.clienteNome || os.ClienteNome }}</p>
                  <p class="text-[11px] text-slate-400 mt-0.5 flex items-center gap-1">
                    <span>📱 {{ os.clienteTelefone || os.ClienteTelefone || 'Sem telefone' }}</span>
                  </p>
                  <p class="text-[10px] text-slate-400 mt-0.5">
                    Atendente: <b class="text-slate-600">{{ os.vendedorNome || os.VendedorNome || 'Não atribuído' }}</b>
                  </p>
                </td>

                <!-- 3. Lente & Laboratório -->
                <td class="py-3.5 px-4">
                  <div class="space-y-1">
                    <p class="font-semibold text-slate-800 text-[11px] truncate max-w-[200px]" :title="os.lenteDescricao || os.LenteDescricao">
                      {{ os.lenteDescricao || os.LenteDescricao || 'Armação / Sem Lente' }}
                    </p>
                    <div v-if="temLente(os)">
                      <span 
                        v-if="os.lentePedida || os.LentePedida" 
                        class="inline-flex items-center gap-1 text-[10px] font-mono font-bold text-emerald-700 bg-emerald-50 px-2 py-0.5 rounded-md border border-emerald-200"
                      >
                        ✓ Pedida ao Lab
                      </span>
                      <button 
                        v-else-if="os.status !== 'CANCELADO' && os.status !== 'CANCELADA'"
                        @click="marcarLentePedida(os.id || os.Id)"
                        class="text-[10px] font-mono font-bold text-amber-800 bg-amber-50 hover:bg-amber-100 px-2 py-0.5 rounded-md border border-amber-300 transition"
                      >
                        📦 Pedir Lente
                      </button>
                    </div>
                  </div>
                </td>

                <!-- 4. Status com Cores Padronizadas -->
                <td class="py-3.5 px-4 text-center">
                  <select 
                    :value="os.status === 'CANCELADA' ? 'CANCELADO' : (os.status || 'EM_ABERTO')"
                    @change="alterarStatusOS(os.id || os.Id, $event.target.value)"
                    :disabled="os.status === 'CANCELADO' || os.status === 'CANCELADA'"
                    :class="obterCorStatus(os.status)"
                    class="px-2.5 py-1 rounded-xl text-[10px] font-mono font-black border uppercase cursor-pointer focus:ring-2 focus:ring-teal-500 transition"
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

                <!-- 5. Financeiro & Saldo a Pagar -->
                <td class="py-3.5 px-4 text-right font-mono">
                  <p class="font-black text-slate-900 text-xs">
                    R$ {{ formatarMoeda(os.valorTotal ?? os.ValorTotal) }}
                  </p>
                  <p v-if="obterValorRestante(os) > 0" class="text-[10px] font-bold text-rose-600 mt-0.5">
                    A Receber: R$ {{ formatarMoeda(obterValorRestante(os)) }}
                  </p>
                  <p v-else-if="os.status !== 'CANCELADO' && os.status !== 'CANCELADA'" class="text-[10px] font-bold text-emerald-600 mt-0.5">
                    ✓ Quitado
                  </p>
                </td>

                <!-- 6. Ações Rápidas Integradas -->
                <td class="py-3.5 px-4 text-center">
                  <div class="flex items-center justify-center gap-1.5 flex-wrap">
                    
                    <!-- Ver Prancheta Completa -->
                    <button 
                      @click="abrirPranchetaClinica(os)"
                      class="px-2.5 py-1.5 rounded-xl bg-slate-950 hover:bg-slate-800 text-white font-bold text-xs shadow-xs transition flex items-center gap-1"
                      title="Ver Detalhes Clínicos e Financeiros da OS"
                    >
                      <span>👁️</span> OS
                    </button>

                    <!-- WhatsApp com Mensagem Pronta -->
                    <button 
                      v-if="os.status !== 'CANCELADO' && os.status !== 'CANCELADA'"
                      @click="abrirWhatsAppOS(os)"
                      class="p-1.5 rounded-xl bg-emerald-50 hover:bg-emerald-100 text-emerald-700 border border-emerald-200 transition text-xs"
                      title="Enviar WhatsApp ao Cliente"
                    >
                      💬
                    </button>

                    <!-- Imprimir Carnê da Ótica -->
                    <a 
                      :href="`/ordens/${os.id || os.Id}/carne`" 
                      target="_blank"
                      class="p-1.5 rounded-xl bg-amber-50 hover:bg-amber-100 text-amber-800 border border-amber-200 transition text-xs"
                      title="Imprimir Carnê de Pagamento / Parcelas"
                    >
                      📜
                    </a>

                    <!-- Entregar & Quitar -->
                    <button 
                      v-if="os.status !== 'ENTREGUE' && os.status !== 'CANCELADO' && os.status !== 'CANCELADA'"
                      @click="abrirModalEntrega(os)"
                      class="px-2.5 py-1.5 rounded-xl bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-xs shadow-xs transition flex items-center gap-1"
                      title="Marcar como Entregue ao Cliente"
                    >
                      <span>🚚</span>
                    </button>

                    <!-- Edição Administrativa (Exclusivo Admin) -->
                    <button 
                      v-if="eAdmin && os.status !== 'CANCELADO' && os.status !== 'CANCELADA'"
                      @click="abrirModalEditarAdmin(os)"
                      class="p-1.5 rounded-xl bg-indigo-50 hover:bg-indigo-100 text-indigo-700 border border-indigo-200 transition text-xs"
                      title="Editar Dados da OS (Admin)"
                    >
                      ✏️
                    </button>

                    <!-- Cancelar OS -->
                    <button 
                      v-if="os.status !== 'CANCELADO' && os.status !== 'CANCELADA'"
                      @click="cancelarOS(os.id || os.Id, os.numeroOS || os.NumeroOS)"
                      class="p-1.5 rounded-xl bg-rose-50 hover:bg-rose-100 text-rose-700 border border-rose-200 transition text-xs"
                      title="Cancelar OS"
                    >
                      🚫
                    </button>

                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

      </div>

      <!-- MODAL LATERAL: PRANCHETA DA ORDEM DE SERVIÇO (DETALHES COMPLETOS) -->
      <div v-if="osSelecionada" class="fixed inset-0 bg-slate-950/70 backdrop-blur-xs z-50 flex items-center justify-center p-4">
        <div class="bg-white rounded-3xl border border-slate-200 shadow-2xl max-w-3xl w-full max-h-[90vh] overflow-y-auto p-6 space-y-6">
          
          <div class="flex items-center justify-between border-b border-slate-100 pb-4">
            <div>
              <span class="text-[10px] font-mono font-bold uppercase tracking-wider text-teal-600">Prancheta Técnica da Ordem</span>
              <h2 class="text-xl font-black text-slate-950 font-mono">
                OS #{{ osSelecionada.numeroOS || osSelecionada.NumeroOS }}
              </h2>
            </div>
            <button @click="osSelecionada = null" class="text-slate-400 hover:text-slate-900 font-bold text-lg p-2">✕</button>
          </div>

          <!-- Informações do Cliente -->
          <div class="p-4 bg-slate-50 rounded-2xl border border-slate-200 grid grid-cols-2 sm:grid-cols-4 gap-3 text-xs">
            <div>
              <span class="text-[10px] uppercase font-bold text-slate-400 block">Cliente</span>
              <b class="text-slate-900">{{ osSelecionada.clienteNome || osSelecionada.ClienteNome }}</b>
            </div>
            <div>
              <span class="text-[10px] uppercase font-bold text-slate-400 block">Telefone</span>
              <b class="text-slate-900">{{ osSelecionada.clienteTelefone || osSelecionada.ClienteTelefone || 'Não informado' }}</b>
            </div>
            <div>
              <span class="text-[10px] uppercase font-bold text-slate-400 block">Atendente</span>
              <b class="text-slate-900">{{ osSelecionada.vendedorNome || osSelecionada.VendedorNome }}</b>
            </div>
            <div>
              <span class="text-[10px] uppercase font-bold text-slate-400 block">Status</span>
              <b class="text-teal-700 font-mono">{{ osSelecionada.status }}</b>
            </div>
          </div>

          <!-- Tabela de Refração Oftalmológica -->
          <div v-if="osSelecionada.receita || osSelecionada.Receita" class="p-4 bg-teal-50/50 rounded-2xl border border-teal-200 space-y-2">
            <h4 class="text-xs font-mono font-bold uppercase text-teal-900 tracking-wider">Prescrição Óptica (Graus)</h4>
            <table class="w-full text-xs text-center border-collapse bg-white rounded-xl overflow-hidden shadow-xs">
              <thead>
                <tr class="bg-teal-900 text-white font-mono text-[10px]">
                  <th class="py-2">Olho</th>
                  <th class="py-2">Esférico (Longe)</th>
                  <th class="py-2">Cilíndrico</th>
                  <th class="py-2">Eixo</th>
                  <th class="py-2">Adição</th>
                  <th class="py-2">DNP</th>
                  <th class="py-2">Altura</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100 font-mono">
                <tr>
                  <td class="py-2 font-bold text-slate-900 bg-slate-50">OD</td>
                  <td class="py-2">{{ (osSelecionada.receita || osSelecionada.Receita).odEsferico ?? '--' }}</td>
                  <td class="py-2 text-amber-700 font-bold">{{ (osSelecionada.receita || osSelecionada.Receita).odCilindrico ?? '--' }}</td>
                  <td class="py-2">{{ (osSelecionada.receita || osSelecionada.Receita).odEixo ? (osSelecionada.receita || osSelecionada.Receita).odEixo + '°' : '--' }}</td>
                  <td class="py-2 text-teal-700 font-bold">{{ (osSelecionada.receita || osSelecionada.Receita).adicao ?? '--' }}</td>
                  <td class="py-2">{{ (osSelecionada.receita || osSelecionada.Receita).dnpOd ?? '--' }} mm</td>
                  <td class="py-2">{{ (osSelecionada.receita || osSelecionada.Receita).alturaMontagemOd ?? '--' }} mm</td>
                </tr>
                <tr>
                  <td class="py-2 font-bold text-slate-900 bg-slate-50">OE</td>
                  <td class="py-2">{{ (osSelecionada.receita || osSelecionada.Receita).oeEsferico ?? '--' }}</td>
                  <td class="py-2 text-amber-700 font-bold">{{ (osSelecionada.receita || osSelecionada.Receita).oeCilindrico ?? '--' }}</td>
                  <td class="py-2">{{ (osSelecionada.receita || osSelecionada.Receita).oeEixo ? (osSelecionada.receita || osSelecionada.Receita).oeEixo + '°' : '--' }}</td>
                  <td class="py-2 text-teal-700 font-bold">{{ (osSelecionada.receita || osSelecionada.Receita).adicao ?? '--' }}</td>
                  <td class="py-2">{{ (osSelecionada.receita || osSelecionada.Receita).dnpOe ?? '--' }} mm</td>
                  <td class="py-2">{{ (osSelecionada.receita || osSelecionada.Receita).alturaMontagemOe ?? '--' }} mm</td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Produtos & Financeiro -->
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 text-xs">
            <div class="p-4 bg-slate-50 rounded-2xl border border-slate-200 space-y-2">
              <span class="text-[10px] font-mono font-bold uppercase text-slate-500 block">Itens da Ordem</span>
              <p><b>Armação:</b> {{ osSelecionada.armacaoDescricao || osSelecionada.ArmacaoDescricao || 'Armação Própria' }}</p>
              <p><b>Lente:</b> {{ osSelecionada.lenteDescricao || osSelecionada.LenteDescricao || 'Sem Lente' }}</p>
            </div>
            <div class="p-4 bg-slate-50 rounded-2xl border border-slate-200 space-y-2 font-mono">
              <span class="text-[10px] font-mono font-bold uppercase text-slate-500 block">Valores da Venda</span>
              <p class="flex justify-between"><span>Valor Total:</span> <b>R$ {{ formatarMoeda(osSelecionada.valorTotal ?? osSelecionada.ValorTotal) }}</b></p>
              <p class="flex justify-between text-rose-600 font-bold"><span>Saldo a Pagar:</span> <b>R$ {{ formatarMoeda(obterValorRestante(osSelecionada)) }}</b></p>
            </div>
          </div>

          <!-- Botões do Rodapé da Prancheta -->
          <div class="flex items-center justify-between pt-4 border-t border-slate-100 text-xs">
            <a 
              :href="`/ordens/${osSelecionada.id || osSelecionada.Id}/carne`" 
              target="_blank"
              class="px-4 py-2 rounded-xl bg-amber-500 hover:bg-amber-600 text-white font-bold transition flex items-center gap-1.5"
            >
              <span>📜</span> Imprimir Carnê
            </a>

            <button 
              @click="osSelecionada = null" 
              class="px-5 py-2 rounded-xl bg-slate-900 text-white font-bold hover:bg-slate-800 transition"
            >
              Fechar Prancheta
            </button>
          </div>

        </div>
      </div>

      <!-- MODAL DE ENTREGA & QUITAÇÃO DE SALDO -->
      <div v-if="modalEntregaAberta" class="fixed inset-0 bg-slate-950/70 backdrop-blur-xs z-50 flex items-center justify-center p-4">
        <div class="bg-white rounded-3xl border border-slate-200 shadow-2xl max-w-md w-full p-6 space-y-5">
          <div class="flex items-center justify-between border-b border-slate-100 pb-3">
            <h3 class="text-base font-bold text-slate-950 font-mono flex items-center gap-2">
              <span>🚚</span> Confirmar Entrega de Óculos
            </h3>
            <button @click="modalEntregaAberta = false" class="text-slate-400 hover:text-slate-800 font-bold">✕</button>
          </div>

          <div v-if="osParaEntrega" class="space-y-4 text-xs">
            <div class="p-3 bg-slate-50 rounded-2xl border border-slate-200 space-y-1">
              <p><b>OS:</b> #{{ osParaEntrega.numeroOS || osParaEntrega.NumeroOS }}</p>
              <p><b>Cliente:</b> {{ osParaEntrega.clienteNome || osParaEntrega.ClienteNome }}</p>
              <p class="font-mono text-sm pt-1 border-t border-slate-200 mt-1">
                Saldo Restante: <b class="text-rose-600">R$ {{ formatarMoeda(obterValorRestante(osParaEntrega)) }}</b>
              </p>
            </div>

            <div v-if="obterValorRestante(osParaEntrega) > 0" class="space-y-3">
              <label class="block font-bold uppercase text-slate-500 text-[10px]">Forma de Pagamento do Saldo</label>
              <select v-model="formEntrega.formaPagamentoRetirada" class="w-full rounded-2xl border-slate-200 text-xs bg-slate-50 focus:border-teal-500 font-bold">
                <option value="DINHEIRO">Dinheiro</option>
                <option value="PIX">PIX</option>
                <option value="CARTAO_DEBITO">Cartão de Débito</option>
                <option value="CARTAO_CREDITO">Cartão de Crédito</option>
              </select>
            </div>

            <div class="flex justify-end gap-2 pt-3 border-t border-slate-100">
              <button @click="modalEntregaAberta = false" class="px-4 py-2 rounded-xl text-slate-600 font-bold hover:bg-slate-100">
                Cancelar
              </button>
              <button 
                @click="confirmarEntrega" 
                :disabled="processandoEntrega"
                class="px-5 py-2 rounded-xl bg-emerald-600 hover:bg-emerald-700 text-white font-bold transition shadow-sm"
              >
                {{ processandoEntrega ? 'Confirmando...' : 'Confirmar Entrega' }}
              </button>
            </div>
          </div>
        </div>
      </div>

    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { ref, computed } from 'vue'
import { router, usePage, Link } from '@inertiajs/vue3'
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
  return ['admin', 'sistema', 'gerente', 'dono'].includes(perfil)
})

const termoBusca = ref(props.Busca ?? props.busca ?? '')
const lojaSelecionada = ref(props.FiltroLojaId ?? props.filtroLojaId ?? '')
const vendedorSelecionado = ref(props.FiltroVendedorId ?? props.filtroVendedorId ?? props.VendedorFiltro ?? props.vendedorFiltro ?? '')
const filtroAtivoComp = computed(() => props.FiltroComposicao ?? props.filtroComposicao ?? props.FiltroAtivo ?? props.filtroAtivo ?? 'total')
const totalExibido = computed(() => props.TotalFaturamento ?? props.totalFaturamento ?? props.TotalFiltroAtivo ?? props.totalFiltroAtivo ?? 0)
const filtroStatusSelecionado = ref('')

const listaOrdens = computed(() => props.Ordens ?? props.ordens ?? [])

// Contadores de Status para os Cards do Topo
const contadores = computed(() => {
  const lista = listaOrdens.value
  return {
    todas: lista.length,
    aguardandoLente: lista.filter(o => o.status === 'AGUARDANDO_LENTE' || (!o.lentePedida && !o.LentePedida && temLente(o))).length,
    emMontagem: lista.filter(o => o.status === 'EM_MONTAGEM' || o.status === 'EM_LABORATORIO').length,
    prontas: lista.filter(o => o.status === 'PRONTA' || o.status === 'PRONTO').length,
    entregues: lista.filter(o => o.status === 'ENTREGUE').length
  }
})

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

const limparTodosFiltros = () => {
  termoBusca.value = ''
  lojaSelecionada.value = ''
  vendedorSelecionado.value = ''
  filtroStatusSelecionado.value = ''
  router.get('/ordens')
}

const aplicarFiltros = (tipoComposicao = null) => {
  const comp = tipoComposicao !== null ? tipoComposicao : filtroAtivoComp.value
  const params = {}
  if (comp && comp !== 'total') params.filtroComposicao = comp
  if (vendedorSelecionado.value) params.vendedorId = vendedorSelecionado.value
  if (lojaSelecionada.value) params.lojaId = lojaSelecionada.value
  if (filtroStatusSelecionado.value) params.filtroStatus = filtroStatusSelecionado.value
  if (termoBusca.value && termoBusca.value.trim().length > 0) params.busca = termoBusca.value.trim()

  router.get('/ordens', params, { preserveState: true, replace: true })
}

const filtrarPorComposicao = (tipo) => aplicarFiltros(tipo)
const filtrarPorStatusRapido = (st) => {
  filtroStatusSelecionado.value = st
  aplicarFiltros()
}

// Prancheta Lateral
const osSelecionada = ref(null)
const abrirPranchetaClinica = (ordem) => {
  osSelecionada.value = { ...ordem }
}

// Entrega de OS
const modalEntregaAberta = ref(false)
const osParaEntrega = ref(null)
const processandoEntrega = ref(false)
const formEntrega = ref({
  formaPagamentoRetirada: 'DINHEIRO'
})

const abrirModalEntrega = (ordem) => {
  osParaEntrega.value = ordem
  modalEntregaAberta.value = true
}

const confirmarEntrega = async () => {
  if (!osParaEntrega.value) return
  processandoEntrega.value = true
  try {
    const id = osParaEntrega.value.id || osParaEntrega.value.Id
    await router.post(`/ordens/${id}/entregar`, {
      formaPagamentoRetirada: formEntrega.value.formaPagamentoRetirada
    }, {
      preserveScroll: true,
      onSuccess: () => {
        modalEntregaAberta.value = false
        processandoEntrega.value = false
      },
      onError: () => {
        processandoEntrega.value = false
      }
    })
  } catch {
    processandoEntrega.value = false
  }
}

// WhatsApp Direto
const abrirWhatsAppOS = (os) => {
  const tel = (os.clienteTelefone || os.ClienteTelefone || '').replace(/\D/g, '')
  if (!tel) {
    alert('Cliente sem telefone cadastrado.')
    return
  }
  const nome = (os.clienteNome || os.ClienteNome || '').split(' ')[0]
  const num = os.numeroOS || os.NumeroOS
  let msg = `Olá ${nome}! Aqui é da Ótica Retina referente à sua Ordem de Serviço #${num}. `
  if (os.status === 'PRONTA' || os.status === 'PRONTO') {
    msg += `Seus óculos ficaram prontos com muito carinho! Você já pode passar na loja para experimentá-los e fazer os ajustes finais. Te aguardamos!`
  } else {
    msg += `Estamos acompanhando a confecção dos seus óculos no laboratório e avisaremos assim que estiverem prontos!`
  }
  window.open(`https://api.whatsapp.com/send?phone=55${tel}&text=${encodeURIComponent(msg)}`, '_blank')
}

// Alterar Status
const alterarStatusOS = (id, novoStatus) => {
  router.post(`/ordens/${id}/status`, { status: novoStatus }, { preserveScroll: true })
}

const marcarLentePedida = (id) => {
  router.post(`/ordens/${id}/marcar-lente-pedida`, {}, { preserveScroll: true })
}

const cancelarOS = (id, num) => {
  if (confirm(`Tem certeza que deseja cancelar a Ordem de Serviço #${num}?`)) {
    router.post(`/ordens/${id}/cancelar`, {}, { preserveScroll: true })
  }
}

// Helpers
const temLente = (os) => {
  const f = os.financeiro || os.Financeiro
  return (f && f.valorLente > 0) || !!os.lenteDescricao || !!os.LenteDescricao
}

const obterValorRestante = (os) => {
  const f = os.financeiro || os.Financeiro
  if (!f) return 0
  return f.valorRestante ?? f.ValorRestante ?? 0
}

const formatarMoeda = (val) => {
  return Number(val || 0).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const formatarData = (dt) => {
  if (!dt) return '--'
  try {
    const d = new Date(dt)
    return d.toLocaleDateString('pt-BR')
  } catch {
    return dt
  }
}

const obterCorStatus = (st) => {
  switch (st) {
    case 'ENTREGUE': return 'bg-emerald-50 text-emerald-800 border-emerald-200'
    case 'PRONTA':
    case 'PRONTO': return 'bg-blue-50 text-blue-800 border-blue-200'
    case 'EM_MONTAGEM':
    case 'EM_LABORATORIO': return 'bg-indigo-50 text-indigo-800 border-indigo-200'
    case 'AGUARDANDO_LENTE': return 'bg-amber-50 text-amber-800 border-amber-200'
    case 'CONFIRMADA': return 'bg-teal-50 text-teal-800 border-teal-200'
    case 'CANCELADO':
    case 'CANCELADA': return 'bg-rose-50 text-rose-800 border-rose-200'
    default: return 'bg-slate-100 text-slate-800 border-slate-200'
  }
}
</script>
