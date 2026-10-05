<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-6 space-y-6 max-w-[1600px] mx-auto">
      
      <!-- Cabeçalho do Painel Kanban -->
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <div>
          <div class="flex items-center gap-3">
            <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight">Esteira de Produção e Acompanhamento de OS</h1>
            <span class="bg-teal-50 text-teal-700 border border-teal-200 text-[11px] font-black uppercase px-2.5 py-0.5 rounded-full">
              Kanban Ao Vivo
            </span>
          </div>
          <p class="text-xs text-slate-500 mt-1">
            Acompanhe o ciclo completo de cada óculos: da confirmação ao pedido de lentes, montagem em laboratório, aviso via WhatsApp e entrega final.
          </p>
        </div>

        <div class="flex items-center gap-3">
          <Link 
            href="/ordens" 
            class="bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold py-2.5 px-4 rounded-xl text-xs transition border border-slate-200 flex items-center gap-2"
          >
            <span>📋</span> Visão Tabela
          </Link>
          <Link 
            href="/ordens/nova" 
            class="bg-teal-600 hover:bg-teal-700 text-white font-bold py-2.5 px-5 rounded-xl text-xs transition shadow-sm uppercase tracking-wider flex items-center gap-1.5"
          >
            <span>+</span> Emitir Nova OS
          </Link>
        </div>
      </div>

      <!-- Barra de Filtros e Alertas de Lentes -->
      <div class="space-y-4">
        
        <!-- ALERTAS INTELIGENTES NO TOPO -->
        <div v-if="(Estatisticas?.lentesChegamHoje || estatisticas?.LentesChegamHoje || 0) > 0 || (Estatisticas?.lentesAtrasadas || estatisticas?.LentesAtrasadas || 0) > 0" class="grid grid-cols-1 md:grid-cols-2 gap-4">
          
          <!-- Alerta: Lentes Chegando Hoje -->
          <div v-if="(Estatisticas?.lentesChegamHoje || estatisticas?.LentesChegamHoje || 0) > 0" class="bg-gradient-to-r from-amber-50 to-orange-50 border border-amber-200 rounded-2xl p-4 flex items-center justify-between shadow-sm animate-pulse">
            <div class="flex items-center gap-3">
              <span class="text-2xl">📦</span>
              <div>
                <h4 class="text-xs font-black uppercase tracking-wider text-amber-900">
                  Lentes com Previsão para Chegada HOJE: {{ Estatisticas?.lentesChegamHoje || estatisticas?.LentesChegamHoje }}
                </h4>
                <p class="text-[11px] text-amber-700 mt-0.5">
                  Verifique a entrega do motoboy/laboratório e mova as ordens para montagem.
                </p>
              </div>
            </div>
            <button 
              @click="filtrarApenasLentesHoje" 
              class="bg-amber-500 hover:bg-amber-600 text-white font-bold text-xs px-3 py-1.5 rounded-xl transition shadow-sm"
            >
              Filtrar
            </button>
          </div>

          <!-- Alerta: Lentes Atrasadas -->
          <div v-if="(Estatisticas?.lentesAtrasadas || estatisticas?.LentesAtrasadas || 0) > 0" class="bg-gradient-to-r from-rose-50 to-red-50 border border-rose-200 rounded-2xl p-4 flex items-center justify-between shadow-sm">
            <div class="flex items-center gap-3">
              <span class="text-2xl">⚠️</span>
              <div>
                <h4 class="text-xs font-black uppercase tracking-wider text-rose-900">
                  Lentes com Previsão Atrasada: {{ Estatisticas?.lentesAtrasadas || estatisticas?.LentesAtrasadas }}
                </h4>
                <p class="text-[11px] text-rose-700 mt-0.5">
                  Contate o laboratório para checar o status do envio dessas lentes.
                </p>
              </div>
            </div>
            <button 
              @click="filtrarApenasLentesAtrasadas" 
              class="bg-rose-600 hover:bg-rose-700 text-white font-bold text-xs px-3 py-1.5 rounded-xl transition shadow-sm"
            >
              Cobrar Lab
            </button>
          </div>
        </div>

        <!-- Filtros Rápidos de Vendedora e Busca -->
        <div class="bg-white p-4 rounded-2xl border border-slate-200 shadow-sm flex flex-col md:flex-row items-center justify-between gap-4">
          <div class="flex flex-wrap items-center gap-3 w-full md:w-auto">
            <!-- Select Vendedor -->
            <div class="min-w-[200px]">
              <select 
                v-model="filtroVendedor" 
                @change="aplicarFiltros"
                class="w-full rounded-xl border-slate-200 text-xs focus:border-teal-500 font-bold text-slate-700 bg-slate-50 py-2 px-3"
              >
                <option value="">(Todas as Vendedoras)</option>
                <option v-for="v in (Vendedores || vendedores || [])" :key="v.id || v.Id" :value="v.id || v.Id">
                  👩‍💼 {{ v.nome || v.Nome }}
                </option>
              </select>
            </div>

            <!-- Campo de Busca -->
            <div class="relative w-full md:w-72">
              <input 
                v-model="filtroBusca" 
                @keyup.enter="aplicarFiltros"
                type="text" 
                placeholder="Buscar por Nº OS, Cliente ou CPF..." 
                class="w-full rounded-xl border-slate-200 text-xs focus:border-teal-500 pl-8 pr-3 py-2 bg-slate-50"
              />
              <span class="absolute left-2.5 top-2.5 text-xs text-slate-400">🔍</span>
            </div>

            <button 
              @click="aplicarFiltros" 
              class="bg-slate-900 hover:bg-slate-800 text-white font-bold text-xs px-4 py-2 rounded-xl transition"
            >
              Filtrar
            </button>

            <button 
              v-if="filtroVendedor || filtroBusca" 
              @click="limparFiltros" 
              class="text-xs text-slate-500 hover:text-slate-800 font-medium underline"
            >
              Limpar
            </button>
          </div>

          <!-- Totalizadores rápidos -->
          <div class="flex items-center gap-3 text-xs font-mono text-slate-500">
            <span>Ativas no Fluxo: <b class="text-teal-600">{{ Estatisticas?.totalAtivas || estatisticas?.TotalAtivas || 0 }}</b></span>
            <span>•</span>
            <span>Entregues: <b class="text-slate-700">{{ Estatisticas?.totalEntregues || estatisticas?.TotalEntregues || 0 }}</b></span>
          </div>
        </div>

      </div>

      <!-- QUADRO KANBAN (6 COLUNAS) -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4 items-start overflow-x-auto pb-6">
        
        <!-- COLUNA 1: OS LANÇADA -->
        <div class="bg-slate-100/90 rounded-2xl p-3.5 border border-slate-200 space-y-3 min-w-[250px]">
          <div class="flex items-center justify-between pb-2 border-b border-slate-200">
            <div class="flex items-center gap-2">
              <span class="w-2.5 h-2.5 rounded-full bg-amber-400"></span>
              <h3 class="text-xs font-black uppercase tracking-wider text-slate-800">1. Lançada</h3>
            </div>
            <span class="bg-amber-100 text-amber-900 text-[11px] font-mono font-bold px-2 py-0.5 rounded-full">
              {{ (Lancadas || lancadas || []).length }}
            </span>
          </div>

          <div class="space-y-3 max-h-[75vh] overflow-y-auto pr-1">
            <div v-if="(Lancadas || lancadas || []).length === 0" class="text-center py-8 text-slate-400 text-xs border border-dashed border-slate-200 rounded-xl">
              Nenhuma OS nesta etapa
            </div>

            <div 
              v-for="os in (Lancadas || lancadas || [])" 
              :key="os.id || os.Id"
              class="bg-white rounded-xl p-3.5 border border-slate-200 shadow-sm hover:shadow-md transition space-y-2.5"
            >
              <div class="flex items-start justify-between gap-1">
                <span class="font-mono text-xs font-bold bg-slate-100 text-slate-800 px-2 py-0.5 rounded">
                  #{{ os.numeroOS || os.NumeroOS }}
                </span>
                <span class="text-[10px] text-slate-400 font-mono">{{ formatarDataCurta(os.dataEntrada || os.DataEntrada) }}</span>
              </div>

              <div>
                <p class="text-xs font-bold text-slate-900 truncate" :title="os.clienteNome || os.ClienteNome">
                  {{ os.clienteNome || os.ClienteNome }}
                </p>
                <p class="text-[10px] text-slate-400 truncate">
                  Atendente: {{ os.vendedorNome || os.VendedorNome }}
                </p>
              </div>

              <div class="text-[11px] text-slate-600 bg-slate-50 p-2 rounded-lg border border-slate-100 space-y-0.5">
                <p v-if="os.armacaoDescricao || os.ArmacaoDescricao" class="truncate">
                  👓 <span class="font-medium">{{ os.armacaoDescricao || os.ArmacaoDescricao }}</span>
                </p>
                <p v-if="os.lenteDescricao || os.LenteDescricao" class="truncate">
                  🔬 <span class="font-medium">{{ os.lenteDescricao || os.LenteDescricao }}</span>
                </p>
              </div>

              <div class="flex items-center justify-between text-xs pt-1 border-t border-slate-100">
                <span class="font-bold font-mono text-teal-700">
                  {{ formatarMoeda(os.valorTotal || os.ValorTotal) }}
                </span>
                <span class="text-[10px] bg-amber-50 text-amber-700 px-1.5 py-0.5 rounded font-medium border border-amber-100">
                  Aguardando Aprov.
                </span>
              </div>

              <!-- Ações da Coluna 1 -->
              <div class="pt-2 flex flex-col gap-1.5">
                <button 
                  @click="confirmarOS(os.id || os.Id)" 
                  class="w-full bg-slate-950 hover:bg-slate-800 text-white text-[11px] font-bold py-1.5 px-3 rounded-lg transition shadow-sm flex items-center justify-center gap-1"
                >
                  <span>✓</span> Confirmar Pedido
                </button>
                <div class="flex items-center gap-1.5">
                  <button 
                    @click="abrirModalWhatsApp(os, 'cadastro')" 
                    class="flex-1 bg-emerald-50 hover:bg-emerald-100 text-emerald-700 border border-emerald-200 text-[10px] font-bold py-1 rounded-lg transition flex items-center justify-center gap-1"
                    title="Enviar WhatsApp de Cadastro"
                  >
                    <span>💬</span> WhatsApp
                  </button>
                  <button 
                    @click="abrirDetalhesOS(os)" 
                    class="px-2 py-1 bg-slate-100 hover:bg-slate-200 text-slate-600 rounded-lg text-[10px] font-bold transition"
                    title="Ver Detalhes"
                  >
                    🔍
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- COLUNA 2: OS CONFIRMADA -->
        <div class="bg-sky-50/70 rounded-2xl p-3.5 border border-sky-200/80 space-y-3 min-w-[250px]">
          <div class="flex items-center justify-between pb-2 border-b border-sky-200">
            <div class="flex items-center gap-2">
              <span class="w-2.5 h-2.5 rounded-full bg-sky-500"></span>
              <h3 class="text-xs font-black uppercase tracking-wider text-sky-950">2. Confirmada</h3>
            </div>
            <span class="bg-sky-200/70 text-sky-900 text-[11px] font-mono font-bold px-2 py-0.5 rounded-full">
              {{ (Confirmadas || confirmadas || []).length }}
            </span>
          </div>

          <div class="space-y-3 max-h-[75vh] overflow-y-auto pr-1">
            <div v-if="(Confirmadas || confirmadas || []).length === 0" class="text-center py-8 text-sky-400 text-xs border border-dashed border-sky-200 rounded-xl">
              Nenhuma OS nesta etapa
            </div>

            <div 
              v-for="os in (Confirmadas || confirmadas || [])" 
              :key="os.id || os.Id"
              class="bg-white rounded-xl p-3.5 border border-sky-200 shadow-sm hover:shadow-md transition space-y-2.5"
            >
              <div class="flex items-start justify-between gap-1">
                <span class="font-mono text-xs font-bold bg-sky-100 text-sky-900 px-2 py-0.5 rounded">
                  #{{ os.numeroOS || os.NumeroOS }}
                </span>
                <span class="text-[10px] text-slate-400 font-mono">{{ formatarDataCurta(os.dataEntrada || os.DataEntrada) }}</span>
              </div>

              <div>
                <p class="text-xs font-bold text-slate-900 truncate">{{ os.clienteNome || os.ClienteNome }}</p>
                <p class="text-[10px] text-slate-400 truncate">Vendedora: {{ os.vendedorNome || os.VendedorNome }}</p>
              </div>

              <div class="text-[11px] text-slate-600 bg-sky-50/50 p-2 rounded-lg border border-sky-100 space-y-0.5">
                <p v-if="os.armacaoDescricao || os.ArmacaoDescricao" class="truncate">
                  👓 <span class="font-medium">{{ os.armacaoDescricao || os.ArmacaoDescricao }}</span>
                </p>
                <p v-if="os.lenteDescricao || os.LenteDescricao" class="truncate">
                  🔬 <span class="font-medium">{{ os.lenteDescricao || os.LenteDescricao }}</span>
                </p>
              </div>

              <!-- Ações da Coluna 2 -->
              <div class="pt-2 flex flex-col gap-1.5">
                <button 
                  @click="abrirModalPedirLente(os)" 
                  class="w-full bg-purple-600 hover:bg-purple-700 text-white text-[11px] font-bold py-1.5 px-3 rounded-lg transition shadow-sm flex items-center justify-center gap-1"
                >
                  <span>📦</span> Pedir Lente ao Lab
                </button>
                <button 
                  @click="moverDiretoMontagem(os.id || os.Id)" 
                  class="w-full bg-slate-100 hover:bg-slate-200 text-slate-700 text-[10px] font-bold py-1 px-2 rounded-lg transition border border-slate-200 flex items-center justify-center gap-1"
                >
                  <span>🔧</span> Direto p/ Montagem
                </button>
                <div class="flex items-center gap-1.5">
                  <button 
                    @click="abrirModalWhatsApp(os, 'cadastro')" 
                    class="flex-1 bg-emerald-50 hover:bg-emerald-100 text-emerald-700 border border-emerald-200 text-[10px] font-bold py-1 rounded-lg transition flex items-center justify-center gap-1"
                  >
                    <span>💬</span> WhatsApp
                  </button>
                  <button 
                    @click="abrirDetalhesOS(os)" 
                    class="px-2 py-1 bg-slate-100 hover:bg-slate-200 text-slate-600 rounded-lg text-[10px] font-bold transition"
                  >
                    🔍
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- COLUNA 3: AGUARDANDO LENTE -->
        <div class="bg-purple-50/70 rounded-2xl p-3.5 border border-purple-200/80 space-y-3 min-w-[250px]">
          <div class="flex items-center justify-between pb-2 border-b border-purple-200">
            <div class="flex items-center gap-2">
              <span class="w-2.5 h-2.5 rounded-full bg-purple-500"></span>
              <h3 class="text-xs font-black uppercase tracking-wider text-purple-950">3. Aguard. Lente</h3>
            </div>
            <span class="bg-purple-200/70 text-purple-900 text-[11px] font-mono font-bold px-2 py-0.5 rounded-full">
              {{ (AguardandoLente || aguardandoLente || []).length }}
            </span>
          </div>

          <div class="space-y-3 max-h-[75vh] overflow-y-auto pr-1">
            <div v-if="(AguardandoLente || aguardandoLente || []).length === 0" class="text-center py-8 text-purple-400 text-xs border border-dashed border-purple-200 rounded-xl">
              Nenhuma OS aguardando lente
            </div>

            <div 
              v-for="os in (AguardandoLente || aguardandoLente || [])" 
              :key="os.id || os.Id"
              class="bg-white rounded-xl p-3.5 border shadow-sm hover:shadow-md transition space-y-2.5"
              :class="obterStatusPrevisaoLente(os.dataPrevisaoLente || os.DataPrevisaoLente) === 'HOJE' ? 'border-amber-400 ring-2 ring-amber-200' : (obterStatusPrevisaoLente(os.dataPrevisaoLente || os.DataPrevisaoLente) === 'ATRASADA' ? 'border-rose-400 ring-2 ring-rose-200' : 'border-purple-200')"
            >
              <div class="flex items-start justify-between gap-1">
                <span class="font-mono text-xs font-bold bg-purple-100 text-purple-900 px-2 py-0.5 rounded">
                  #{{ os.numeroOS || os.NumeroOS }}
                </span>
                
                <!-- Badge de Previsão de Lente -->
                <span 
                  :class="obterClasseBadgePrevisao(os.dataPrevisaoLente || os.DataPrevisaoLente)"
                  class="text-[9px] font-bold px-1.5 py-0.5 rounded border uppercase"
                >
                  {{ obterTextoBadgePrevisao(os.dataPrevisaoLente || os.DataPrevisaoLente) }}
                </span>
              </div>

              <div>
                <p class="text-xs font-bold text-slate-900 truncate">{{ os.clienteNome || os.ClienteNome }}</p>
                <p class="text-[10px] text-slate-400 truncate">Vendedora: {{ os.vendedorNome || os.VendedorNome }}</p>
              </div>

              <div class="text-[11px] text-purple-950 bg-purple-50/50 p-2 rounded-lg border border-purple-100 space-y-0.5">
                <p class="truncate font-medium">🔬 {{ os.lenteDescricao || os.LenteDescricao || 'Lente de Laboratório' }}</p>
                <p class="text-[10px] text-purple-700 font-mono">
                  📅 Previsão: <b>{{ formatarDataBR(os.dataPrevisaoLente || os.DataPrevisaoLente) }}</b>
                </p>
              </div>

              <!-- Ações da Coluna 3 -->
              <div class="pt-2 flex flex-col gap-1.5">
                <button 
                  @click="marcarLenteChegou(os.id || os.Id)" 
                  class="w-full bg-indigo-600 hover:bg-indigo-700 text-white text-[11px] font-bold py-1.5 px-3 rounded-lg transition shadow-sm flex items-center justify-center gap-1"
                >
                  <span>✓</span> Lente Chegou (Montar)
                </button>
                <div class="flex items-center gap-1.5">
                  <button 
                    @click="abrirModalPedirLente(os)" 
                    class="flex-1 bg-slate-100 hover:bg-slate-200 text-slate-700 text-[10px] font-bold py-1 rounded-lg transition border border-slate-200"
                    title="Ajustar data prevista"
                  >
                    ✏️ Data
                  </button>
                  <button 
                    @click="abrirModalWhatsApp(os, 'cadastro')" 
                    class="flex-1 bg-emerald-50 hover:bg-emerald-100 text-emerald-700 border border-emerald-200 text-[10px] font-bold py-1 rounded-lg transition flex items-center justify-center gap-1"
                  >
                    <span>💬</span> Zap
                  </button>
                  <button 
                    @click="abrirDetalhesOS(os)" 
                    class="px-2 py-1 bg-slate-100 hover:bg-slate-200 text-slate-600 rounded-lg text-[10px] font-bold transition"
                  >
                    🔍
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- COLUNA 4: EM MONTAGEM -->
        <div class="bg-indigo-50/70 rounded-2xl p-3.5 border border-indigo-200/80 space-y-3 min-w-[250px]">
          <div class="flex items-center justify-between pb-2 border-b border-indigo-200">
            <div class="flex items-center gap-2">
              <span class="w-2.5 h-2.5 rounded-full bg-indigo-500"></span>
              <h3 class="text-xs font-black uppercase tracking-wider text-indigo-950">4. Em Montagem</h3>
            </div>
            <span class="bg-indigo-200/70 text-indigo-900 text-[11px] font-mono font-bold px-2 py-0.5 rounded-full">
              {{ (EmMontagem || emMontagem || []).length }}
            </span>
          </div>

          <div class="space-y-3 max-h-[75vh] overflow-y-auto pr-1">
            <div v-if="(EmMontagem || emMontagem || []).length === 0" class="text-center py-8 text-indigo-400 text-xs border border-dashed border-indigo-200 rounded-xl">
              Nenhuma OS na bancada
            </div>

            <div 
              v-for="os in (EmMontagem || emMontagem || [])" 
              :key="os.id || os.Id"
              class="bg-white rounded-xl p-3.5 border border-indigo-200 shadow-sm hover:shadow-md transition space-y-2.5"
            >
              <div class="flex items-start justify-between gap-1">
                <span class="font-mono text-xs font-bold bg-indigo-100 text-indigo-900 px-2 py-0.5 rounded">
                  #{{ os.numeroOS || os.NumeroOS }}
                </span>
                <span class="text-[10px] text-indigo-700 font-bold bg-indigo-50 px-1.5 py-0.5 rounded border border-indigo-100">
                  Na Bancada
                </span>
              </div>

              <div>
                <p class="text-xs font-bold text-slate-900 truncate">{{ os.clienteNome || os.ClienteNome }}</p>
                <p class="text-[10px] text-slate-400 truncate">Vendedora: {{ os.vendedorNome || os.VendedorNome }}</p>
              </div>

              <!-- Especificações rápidas da receita para o montador -->
              <div v-if="os.receita || os.Receita" class="text-[10px] bg-slate-900 text-white p-2 rounded-lg font-mono space-y-0.5">
                <div class="flex justify-between">
                  <span>OD: {{ os.receita?.odEsferico || os.Receita?.odEsferico || 0 }} {{ os.receita?.odCilindrico || os.Receita?.odCilindrico || '' }}</span>
                  <span v-if="os.receita?.odEixo || os.Receita?.odEixo">Eixo: {{ os.receita?.odEixo || os.Receita?.odEixo }}°</span>
                </div>
                <div class="flex justify-between">
                  <span>OE: {{ os.receita?.oeEsferico || os.Receita?.oeEsferico || 0 }} {{ os.receita?.oeCilindrico || os.Receita?.oeCilindrico || '' }}</span>
                  <span v-if="os.receita?.oeEixo || os.Receita?.oeEixo">Eixo: {{ os.receita?.oeEixo || os.Receita?.oeEixo }}°</span>
                </div>
                <div v-if="os.receita?.adicao || os.Receita?.adicao" class="text-teal-300 font-bold">
                  Adição: +{{ os.receita?.adicao || os.Receita?.adicao }}
                </div>
              </div>

              <!-- Ações da Coluna 4 -->
              <div class="pt-2 flex flex-col gap-1.5">
                <button 
                  @click="concluirMontagem(os.id || os.Id)" 
                  class="w-full bg-emerald-600 hover:bg-emerald-700 text-white text-[11px] font-bold py-1.5 px-3 rounded-lg transition shadow-sm flex items-center justify-center gap-1"
                >
                  <span>👓</span> Pronto p/ Retirada
                </button>
                <button 
                  @click="abrirDetalhesOS(os)" 
                  class="w-full bg-slate-100 hover:bg-slate-200 text-slate-700 text-[10px] font-bold py-1 px-2 rounded-lg transition border border-slate-200 flex items-center justify-center gap-1"
                >
                  <span>🔍</span> Ver Receita Completa
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- COLUNA 5: OS PRONTA (COM WHATSAPP) -->
        <div class="bg-emerald-50/70 rounded-2xl p-3.5 border border-emerald-200/80 space-y-3 min-w-[250px]">
          <div class="flex items-center justify-between pb-2 border-b border-emerald-200">
            <div class="flex items-center gap-2">
              <span class="w-2.5 h-2.5 rounded-full bg-emerald-500"></span>
              <h3 class="text-xs font-black uppercase tracking-wider text-emerald-950">5. OS Pronta</h3>
            </div>
            <span class="bg-emerald-200/70 text-emerald-900 text-[11px] font-mono font-bold px-2 py-0.5 rounded-full">
              {{ (Prontas || prontas || []).length }}
            </span>
          </div>

          <div class="space-y-3 max-h-[75vh] overflow-y-auto pr-1">
            <div v-if="(Prontas || prontas || []).length === 0" class="text-center py-8 text-emerald-400 text-xs border border-dashed border-emerald-200 rounded-xl">
              Nenhuma OS aguardando retirada
            </div>

            <div 
              v-for="os in (Prontas || prontas || [])" 
              :key="os.id || os.Id"
              class="bg-white rounded-xl p-3.5 border border-emerald-300 shadow-sm hover:shadow-md transition space-y-2.5"
            >
              <div class="flex items-start justify-between gap-1">
                <span class="font-mono text-xs font-bold bg-emerald-100 text-emerald-900 px-2 py-0.5 rounded">
                  #{{ os.numeroOS || os.NumeroOS }}
                </span>
                
                <!-- Badge de Notificação do WhatsApp -->
                <span 
                  v-if="os.whatsAppNotificadoPronto || os.WhatsAppNotificadoPronto" 
                  class="text-[9px] bg-emerald-50 text-emerald-700 border border-emerald-200 px-1.5 py-0.5 rounded font-bold"
                  title="Cliente já foi avisado via WhatsApp"
                >
                  ✓ Zap Enviado
                </span>
                <span 
                  v-else 
                  class="text-[9px] bg-amber-50 text-amber-700 border border-amber-200 px-1.5 py-0.5 rounded font-bold animate-pulse"
                  title="Avisar cliente via WhatsApp"
                >
                  📲 Avisar Cliente
                </span>
              </div>

              <div>
                <p class="text-xs font-bold text-slate-900 truncate">{{ os.clienteNome || os.ClienteNome }}</p>
                <p class="text-[10px] text-slate-400 truncate">Telefone: {{ os.clienteTelefone || os.ClienteTelefone || 'Sem tel.' }}</p>
              </div>

              <!-- Status Financeiro da Retirada -->
              <div 
                :class="(os.valorRestante || os.ValorRestante || 0) > 0 ? 'bg-amber-50 border-amber-200 text-amber-900' : 'bg-emerald-50 border-emerald-200 text-emerald-900'"
                class="p-2 rounded-lg border text-xs font-mono space-y-0.5"
              >
                <div class="flex justify-between items-center text-[10px] uppercase font-bold text-slate-500">
                  <span>Situação:</span>
                  <span :class="(os.valorRestante || os.ValorRestante || 0) > 0 ? 'text-amber-700' : 'text-emerald-700'">
                    {{ (os.valorRestante || os.ValorRestante || 0) > 0 ? 'Saldo Pendente' : '100% Quitado' }}
                  </span>
                </div>
                <div class="flex justify-between items-center font-bold">
                  <span>Total: {{ formatarMoeda(os.valorTotal || os.ValorTotal) }}</span>
                  <span :class="(os.valorRestante || os.ValorRestante || 0) > 0 ? 'text-rose-600 font-black' : 'text-emerald-600'">
                    {{ (os.valorRestante || os.ValorRestante || 0) > 0 ? `Pagar: ${formatarMoeda(os.valorRestante || os.ValorRestante)}` : 'R$ 0,00' }}
                  </span>
                </div>
              </div>

              <!-- Ações da Coluna 5 -->
              <div class="pt-2 flex flex-col gap-1.5">
                
                <!-- BOTÃO DESTACADO DE WHATSAPP -->
                <button 
                  @click="abrirModalWhatsApp(os, 'pronto')" 
                  class="w-full bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-black py-2 px-3 rounded-lg transition shadow-md flex items-center justify-center gap-1.5 active:scale-95"
                >
                  <span>📱</span> Avisar via WhatsApp
                </button>

                <!-- BOTÃO ENTREGAR / QUITAR -->
                <button 
                  @click="abrirModalEntrega(os)" 
                  class="w-full bg-slate-900 hover:bg-slate-800 text-white text-[11px] font-bold py-1.5 px-3 rounded-lg transition flex items-center justify-center gap-1"
                >
                  <span>🎁</span> Entregar ao Cliente
                </button>

                <button 
                  @click="abrirDetalhesOS(os)" 
                  class="w-full bg-slate-100 hover:bg-slate-200 text-slate-600 text-[10px] font-bold py-1 rounded-lg transition"
                >
                  🔍 Ver Detalhes
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- COLUNA 6: OS ENTREGUE -->
        <div class="bg-slate-50 rounded-2xl p-3.5 border border-slate-200 space-y-3 min-w-[250px]">
          <div class="flex items-center justify-between pb-2 border-b border-slate-200">
            <div class="flex items-center gap-2">
              <span class="w-2.5 h-2.5 rounded-full bg-slate-400"></span>
              <h3 class="text-xs font-black uppercase tracking-wider text-slate-700">6. Entregue</h3>
            </div>
            <span class="bg-slate-200 text-slate-700 text-[11px] font-mono font-bold px-2 py-0.5 rounded-full">
              {{ (Entregues || entregues || []).length }}
            </span>
          </div>

          <div class="space-y-3 max-h-[75vh] overflow-y-auto pr-1">
            <div v-if="(Entregues || entregues || []).length === 0" class="text-center py-8 text-slate-400 text-xs border border-dashed border-slate-200 rounded-xl">
              Nenhuma OS entregue recentemente
            </div>

            <div 
              v-for="os in (Entregues || entregues || [])" 
              :key="os.id || os.Id"
              class="bg-white rounded-xl p-3.5 border border-slate-200 shadow-sm opacity-90 hover:opacity-100 transition space-y-2"
            >
              <div class="flex items-start justify-between gap-1">
                <span class="font-mono text-xs font-bold bg-slate-100 text-slate-700 px-2 py-0.5 rounded">
                  #{{ os.numeroOS || os.NumeroOS }}
                </span>
                <span class="text-[10px] text-emerald-700 font-bold bg-emerald-50 px-1.5 py-0.5 rounded border border-emerald-100">
                  ✓ Entregue
                </span>
              </div>

              <div>
                <p class="text-xs font-bold text-slate-800 truncate">{{ os.clienteNome || os.ClienteNome }}</p>
                <p class="text-[10px] text-slate-400 truncate">
                  Retirado em: {{ formatarDataBR(os.dataEntregaReal || os.DataEntregaReal || os.dataQuitacao || os.DataQuitacao) }}
                </p>
              </div>

              <div class="flex items-center justify-between text-xs pt-1 border-t border-slate-100 font-mono">
                <span class="text-slate-400 text-[10px]">Total:</span>
                <span class="font-bold text-slate-900">{{ formatarMoeda(os.valorTotal || os.ValorTotal) }}</span>
              </div>

              <div class="pt-1">
                <button 
                  @click="abrirDetalhesOS(os)" 
                  class="w-full bg-slate-100 hover:bg-slate-200 text-slate-700 text-[10px] font-bold py-1 rounded-lg transition"
                >
                  🔍 Detalhes da OS
                </button>
              </div>
            </div>
          </div>
        </div>

      </div>

      <!-- ========================================================================= -->
      <!-- MODAL: PEDIR LENTE / DEFINIR DATA PREVISTA -->
      <!-- ========================================================================= -->
      <div v-if="modalPedirLenteAberto" class="fixed inset-0 bg-slate-950/70 backdrop-blur-sm z-50 flex items-center justify-center p-4">
        <div class="bg-white rounded-3xl max-w-md w-full p-6 space-y-4 shadow-2xl border border-slate-100">
          <div class="flex items-center justify-between border-b border-slate-100 pb-3">
            <div>
              <h3 class="text-base font-black text-slate-900">📦 Pedido de Lente ao Laboratório</h3>
              <p class="text-xs text-slate-500">OS #{{ osSelecionada?.numeroOS || osSelecionada?.NumeroOS }} • {{ osSelecionada?.clienteNome || osSelecionada?.ClienteNome }}</p>
            </div>
            <button @click="modalPedirLenteAberto = false" class="text-slate-400 hover:text-slate-600 text-xl font-bold">✕</button>
          </div>

          <div class="space-y-3">
            <div class="bg-purple-50 p-3 rounded-xl border border-purple-100 text-xs text-purple-950">
              <p class="font-bold">🔬 Lente:</p>
              <p class="text-purple-800">{{ osSelecionada?.lenteDescricao || osSelecionada?.LenteDescricao || 'Lentes de Grau' }}</p>
            </div>

            <div>
              <label class="block text-xs font-bold uppercase text-slate-700 tracking-wider mb-1.5">
                Data Estimada de Chegada da Lente na Ótica *
              </label>
              <input 
                v-model="dataPrevisaoInput" 
                type="date" 
                class="w-full rounded-xl border-slate-200 text-sm focus:border-purple-500 focus:ring-purple-500 font-mono"
                required
              />
            </div>

            <!-- Atalhos de data -->
            <div>
              <p class="text-[10px] font-bold uppercase text-slate-400 mb-1">Previsão Rápida:</p>
              <div class="flex flex-wrap gap-1.5">
                <button 
                  type="button" 
                  @click="definirAtalhoData(2)"
                  class="bg-slate-100 hover:bg-slate-200 text-slate-700 text-xs px-2.5 py-1 rounded-lg font-mono font-bold transition"
                >
                  +2 dias
                </button>
                <button 
                  type="button" 
                  @click="definirAtalhoData(3)"
                  class="bg-slate-100 hover:bg-slate-200 text-slate-700 text-xs px-2.5 py-1 rounded-lg font-mono font-bold transition"
                >
                  +3 dias
                </button>
                <button 
                  type="button" 
                  @click="definirAtalhoData(5)"
                  class="bg-slate-100 hover:bg-slate-200 text-slate-700 text-xs px-2.5 py-1 rounded-lg font-mono font-bold transition"
                >
                  +5 dias
                </button>
                <button 
                  type="button" 
                  @click="definirAtalhoData(7)"
                  class="bg-slate-100 hover:bg-slate-200 text-slate-700 text-xs px-2.5 py-1 rounded-lg font-mono font-bold transition"
                >
                  +7 dias
                </button>
              </div>
            </div>
          </div>

          <div class="flex items-center justify-end gap-2 pt-3 border-t border-slate-100">
            <button 
              @click="modalPedirLenteAberto = false" 
              class="px-4 py-2 text-xs font-bold text-slate-500 hover:text-slate-800 transition"
            >
              Cancelar
            </button>
            <button 
              @click="salvarPedidoLente" 
              :disabled="salvandoFluxo || !dataPrevisaoInput"
              class="bg-purple-600 hover:bg-purple-700 disabled:bg-slate-300 text-white font-bold text-xs py-2.5 px-5 rounded-xl transition shadow-md"
            >
              <span v-if="salvandoFluxo">Gravando...</span>
              <span v-else>Confirmar Pedido de Lente</span>
            </button>
          </div>
        </div>
      </div>

      <!-- ========================================================================= -->
      <!-- MODAL: DISPARO / PRÉVIA DE WHATSAPP -->
      <!-- ========================================================================= -->
      <div v-if="modalWhatsAppAberto" class="fixed inset-0 bg-slate-950/70 backdrop-blur-sm z-50 flex items-center justify-center p-4">
        <div class="bg-white rounded-3xl max-w-lg w-full p-6 space-y-4 shadow-2xl border border-slate-100">
          <div class="flex items-center justify-between border-b border-slate-100 pb-3">
            <div class="flex items-center gap-2.5">
              <span class="w-8 h-8 rounded-xl bg-emerald-100 text-emerald-600 flex items-center justify-center text-lg">💬</span>
              <div>
                <h3 class="text-base font-black text-slate-900">
                  {{ tipoWhatsAppAtual === 'pronto' ? 'Avisar Cliente: Óculos Pronto' : 'WhatsApp de Boas-Vindas' }}
                </h3>
                <p class="text-xs text-slate-500">OS #{{ infoWhatsApp?.numeroOS }} • {{ infoWhatsApp?.clienteNome }}</p>
              </div>
            </div>
            <button @click="modalWhatsAppAberto = false" class="text-slate-400 hover:text-slate-600 text-xl font-bold">✕</button>
          </div>

          <div v-if="carregandoWhatsApp" class="py-12 text-center text-slate-400 text-sm">
            Gerando mensagem personalizada...
          </div>

          <div v-else class="space-y-3">
            <div>
              <label class="block text-[10px] font-bold uppercase text-slate-400 tracking-wider mb-1">
                Destinatário (WhatsApp do Cliente)
              </label>
              <div class="flex items-center gap-2">
                <input 
                  v-model="infoWhatsApp.telefone" 
                  type="text" 
                  class="w-full rounded-xl border-slate-200 text-xs font-mono focus:border-emerald-500 focus:ring-emerald-500" 
                  placeholder="DDD + Número (ex: 5511999999999)"
                />
              </div>
            </div>

            <div>
              <label class="block text-[10px] font-bold uppercase text-slate-400 tracking-wider mb-1">
                Pré-visualização do Texto
              </label>
              <textarea 
                v-model="infoWhatsApp.mensagem" 
                rows="7" 
                class="w-full rounded-xl border-slate-200 text-xs font-sans leading-relaxed focus:border-emerald-500 focus:ring-emerald-500 bg-slate-50 p-3"
              ></textarea>
            </div>
          </div>

          <div class="flex flex-col sm:flex-row items-center justify-between gap-3 pt-3 border-t border-slate-100">
            <button 
              @click="copiarTextoWhatsApp" 
              class="w-full sm:w-auto px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs rounded-xl transition"
            >
              📋 Copiar Texto
            </button>

            <div class="flex items-center gap-2 w-full sm:w-auto justify-end">
              <button 
                @click="modalWhatsAppAberto = false" 
                class="px-4 py-2 text-xs font-bold text-slate-500 hover:text-slate-800 transition"
              >
                Fechar
              </button>
              <a 
                :href="gerarLinkWhatsAppAtual()" 
                target="_blank" 
                @click="registrarEnvioWhatsApp"
                class="w-full sm:w-auto bg-emerald-600 hover:bg-emerald-700 text-white font-black text-xs py-2.5 px-6 rounded-xl transition shadow-md flex items-center justify-center gap-2"
              >
                <span>📱</span> Abrir no WhatsApp
              </a>
            </div>
          </div>
        </div>
      </div>

      <!-- ========================================================================= -->
      <!-- MODAL: QUITAÇÃO NA RETIRADA E ENTREGA -->
      <!-- ========================================================================= -->
      <div v-if="modalEntregaAberto" class="fixed inset-0 bg-slate-950/70 backdrop-blur-sm z-50 flex items-center justify-center p-4">
        <div class="bg-white rounded-3xl max-w-md w-full p-6 space-y-4 shadow-2xl border border-slate-100">
          <div class="flex items-center justify-between border-b border-slate-100 pb-3">
            <div>
              <h3 class="text-base font-black text-slate-900">🎁 Entregar Óculos ao Cliente</h3>
              <p class="text-xs text-slate-500">OS #{{ osSelecionada?.numeroOS || osSelecionada?.NumeroOS }} • {{ osSelecionada?.clienteNome || osSelecionada?.ClienteNome }}</p>
            </div>
            <button @click="modalEntregaAberto = false" class="text-slate-400 hover:text-slate-600 text-xl font-bold">✕</button>
          </div>

          <div class="space-y-4">
            
            <!-- Quadro Financeiro -->
            <div class="bg-slate-50 p-4 rounded-2xl border border-slate-200 space-y-2">
              <div class="flex justify-between text-xs">
                <span class="text-slate-500">Valor Total da OS:</span>
                <span class="font-bold text-slate-900 font-mono">{{ formatarMoeda(osSelecionada?.valorTotal || osSelecionada?.ValorTotal) }}</span>
              </div>
              <div class="flex justify-between text-xs">
                <span class="text-slate-500">Já Quitado na Emissão:</span>
                <span class="font-bold text-emerald-600 font-mono">{{ formatarMoeda((osSelecionada?.valorTotal || osSelecionada?.ValorTotal || 0) - (osSelecionada?.valorRestante || osSelecionada?.ValorRestante || 0)) }}</span>
              </div>
              <div class="flex justify-between text-sm font-bold pt-2 border-t border-slate-200">
                <span class="text-slate-800">Saldo a Quitar Agora:</span>
                <span :class="(osSelecionada?.valorRestante || osSelecionada?.ValorRestante || 0) > 0 ? 'text-rose-600 font-black' : 'text-emerald-600 font-mono'">
                  {{ formatarMoeda(osSelecionada?.valorRestante || osSelecionada?.ValorRestante || 0) }}
                </span>
              </div>
            </div>

            <!-- Se houver saldo a pagar -->
            <div v-if="(osSelecionada?.valorRestante || osSelecionada?.ValorRestante || 0) > 0" class="space-y-3">
              <div>
                <label class="block text-xs font-bold uppercase text-slate-700 tracking-wider mb-1.5">
                  Forma de Pagamento da Retirada *
                </label>
                <select 
                  v-model="formaPagamentoRetirada" 
                  class="w-full rounded-xl border-slate-200 text-xs focus:border-teal-500 font-bold text-slate-700 bg-slate-50 py-2.5 px-3"
                >
                  <option value="DINHEIRO">💵 Dinheiro</option>
                  <option value="PIX">⚡ PIX</option>
                  <option value="CARTAO_DEBITO">💳 Cartão de Débito</option>
                  <option value="CARTAO_CREDITO">💳 Cartão de Crédito</option>
                </select>
              </div>

              <div v-if="formaPagamentoRetirada === 'CARTAO_CREDITO'">
                <label class="block text-xs font-bold uppercase text-slate-700 tracking-wider mb-1">
                  Parcelas no Crédito
                </label>
                <select 
                  v-model="parcelasRetirada" 
                  class="w-full rounded-xl border-slate-200 text-xs focus:border-teal-500 font-bold"
                >
                  <option :value="1">1x à vista</option>
                  <option :value="2">2x</option>
                  <option :value="3">3x</option>
                  <option :value="4">4x</option>
                  <option :value="5">5x</option>
                  <option :value="6">6x</option>
                </select>
              </div>
            </div>

            <div v-else class="p-3 bg-emerald-50 border border-emerald-200 rounded-xl text-xs text-emerald-800 font-bold text-center">
              ✓ Esta Ordem de Serviço já está totalmente quitada!
            </div>
          </div>

          <div class="flex items-center justify-end gap-2 pt-3 border-t border-slate-100">
            <button 
              @click="modalEntregaAberto = false" 
              class="px-4 py-2 text-xs font-bold text-slate-500 hover:text-slate-800 transition"
            >
              Cancelar
            </button>
            <button 
              @click="confirmarEntrega" 
              :disabled="salvandoFluxo"
              class="bg-slate-950 hover:bg-slate-800 disabled:bg-slate-300 text-white font-bold text-xs py-2.5 px-6 rounded-xl transition shadow-md"
            >
              <span v-if="salvandoFluxo">Processando...</span>
              <span v-else>Confirmar Entrega</span>
            </button>
          </div>
        </div>
      </div>

      <!-- ========================================================================= -->
      <!-- MODAL: DETALHES COMPLETOS DA OS -->
      <!-- ========================================================================= -->
      <div v-if="modalDetalhesAberto" class="fixed inset-0 bg-slate-950/70 backdrop-blur-sm z-50 flex items-center justify-center p-4">
        <div class="bg-white rounded-3xl max-w-2xl w-full p-6 space-y-4 shadow-2xl border border-slate-100 max-h-[90vh] overflow-y-auto">
          <div class="flex items-center justify-between border-b border-slate-100 pb-3">
            <div>
              <h3 class="text-base font-black text-slate-900">Detalhes da Ordem de Serviço #{{ osDetalhe?.numeroOS || osDetalhe?.NumeroOS }}</h3>
              <p class="text-xs text-slate-500">Cadastrada em {{ formatarDataBR(osDetalhe?.dataEntrada || osDetalhe?.DataEntrada) }} • Loja {{ osDetalhe?.lojaVenda || osDetalhe?.LojaVenda || 'Matriz' }}</p>
            </div>
            <button @click="modalDetalhesAberto = false" class="text-slate-400 hover:text-slate-600 text-xl font-bold">✕</button>
          </div>

          <div class="space-y-4 text-xs">
            
            <!-- Dados do Cliente -->
            <div class="bg-slate-50 p-3.5 rounded-xl border border-slate-200">
              <h4 class="font-bold text-slate-800 uppercase tracking-wider text-[10px] mb-2">👤 Cliente</h4>
              <div class="grid grid-cols-2 gap-2">
                <p><b>Nome:</b> {{ osDetalhe?.clienteNome || osDetalhe?.ClienteNome }}</p>
                <p><b>Telefone / Zap:</b> {{ osDetalhe?.clienteTelefone || osDetalhe?.ClienteTelefone || 'Não informado' }}</p>
                <p><b>CPF:</b> {{ osDetalhe?.clienteCpf || osDetalhe?.ClienteCpf || 'Não informado' }}</p>
                <p><b>Vendedora:</b> {{ osDetalhe?.vendedorNome || osDetalhe?.VendedorNome }}</p>
              </div>
            </div>

            <!-- Produtos -->
            <div class="bg-slate-50 p-3.5 rounded-xl border border-slate-200 space-y-1.5">
              <h4 class="font-bold text-slate-800 uppercase tracking-wider text-[10px] mb-1">👓 Itens do Pedido</h4>
              <p><b>Armação:</b> {{ osDetalhe?.armacaoDescricao || osDetalhe?.ArmacaoDescricao || 'Armação do Cliente / Manual' }}</p>
              <p><b>Lente:</b> {{ osDetalhe?.lenteDescricao || osDetalhe?.LenteDescricao || 'Lentes de Grau' }}</p>
              <p v-if="osDetalhe?.observacoes || osDetalhe?.Observacoes"><b>Observações:</b> {{ osDetalhe?.observacoes || osDetalhe?.Observacoes }}</p>
            </div>

            <!-- Receita Óptica -->
            <div v-if="osDetalhe?.receita || osDetalhe?.Receita" class="bg-slate-900 text-white p-4 rounded-xl font-mono space-y-2">
              <h4 class="font-bold text-teal-400 uppercase tracking-wider text-[10px]">🔬 Especificações de Grau</h4>
              <div class="grid grid-cols-2 gap-4">
                <div class="bg-slate-800 p-2.5 rounded-lg">
                  <p class="text-teal-300 font-bold border-b border-slate-700 pb-1 mb-1">Olho Direito (OD)</p>
                  <p>Esférico: {{ osDetalhe.receita?.odEsferico || osDetalhe.Receita?.odEsferico || '0.00' }}</p>
                  <p>Cilíndrico: {{ osDetalhe.receita?.odCilindrico || osDetalhe.Receita?.odCilindrico || '0.00' }}</p>
                  <p>Eixo: {{ osDetalhe.receita?.odEixo || osDetalhe.Receita?.odEixo || '0' }}°</p>
                </div>
                <div class="bg-slate-800 p-2.5 rounded-lg">
                  <p class="text-teal-300 font-bold border-b border-slate-700 pb-1 mb-1">Olho Esquerdo (OE)</p>
                  <p>Esférico: {{ osDetalhe.receita?.oeEsferico || osDetalhe.Receita?.oeEsferico || '0.00' }}</p>
                  <p>Cilíndrico: {{ osDetalhe.receita?.oeCilindrico || osDetalhe.Receita?.oeCilindrico || '0.00' }}</p>
                  <p>Eixo: {{ osDetalhe.receita?.oeEixo || osDetalhe.Receita?.oeEixo || '0' }}°</p>
                </div>
              </div>
              <p v-if="osDetalhe.receita?.adicao || osDetalhe.Receita?.adicao" class="text-teal-300 font-bold">
                Adição Perto: +{{ osDetalhe.receita?.adicao || osDetalhe.Receita?.adicao }}
              </p>
            </div>

            <!-- Financeiro -->
            <div class="bg-teal-50 p-3.5 rounded-xl border border-teal-200 flex items-center justify-between font-mono">
              <div>
                <span class="text-[10px] font-bold uppercase text-teal-800">Total Líquido:</span>
                <p class="text-base font-black text-teal-950">{{ formatarMoeda(osDetalhe?.valorTotal || osDetalhe?.ValorTotal) }}</p>
              </div>
              <div class="text-right">
                <span class="text-[10px] font-bold uppercase text-teal-800">Saldo Restante:</span>
                <p :class="(osDetalhe?.valorRestante || osDetalhe?.ValorRestante || 0) > 0 ? 'text-rose-600 font-black' : 'text-emerald-700 font-bold'" class="text-base">
                  {{ formatarMoeda(osDetalhe?.valorRestante || osDetalhe?.ValorRestante || 0) }}
                </p>
              </div>
            </div>

          </div>

          <div class="flex justify-end pt-2 border-t border-slate-100">
            <button 
              @click="modalDetalhesAberto = false" 
              class="bg-slate-900 hover:bg-slate-800 text-white font-bold text-xs py-2 px-6 rounded-xl transition"
            >
              Fechar
            </button>
          </div>
        </div>
      </div>

    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { ref } from 'vue'
import { router, Link } from '@inertiajs/vue3'
import axios from 'axios'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  lancadas: Array,
  Lancadas: Array,
  confirmadas: Array,
  Confirmadas: Array,
  aguardandoLente: Array,
  AguardandoLente: Array,
  emMontagem: Array,
  EmMontagem: Array,
  prontas: Array,
  Prontas: Array,
  entregues: Array,
  Entregues: Array,
  estatisticas: Object,
  Estatisticas: Object,
  vendedores: Array,
  Vendedores: Array,
  vendedorFiltro: String,
  VendedorFiltro: String,
  buscaFiltro: String,
  BuscaFiltro: String,
  lojaWhatsapp: String,
  LojaWhatsapp: String
})

const filtroVendedor = ref(props.VendedorFiltro || props.vendedorFiltro || '')
const filtroBusca = ref(props.BuscaFiltro || props.buscaFiltro || '')
const salvandoFluxo = ref(false)

// Modais State
const modalPedirLenteAberto = ref(false)
const modalWhatsAppAberto = ref(false)
const modalEntregaAberto = ref(false)
const modalDetalhesAberto = ref(false)

const osSelecionada = ref(null)
const osDetalhe = ref(null)
const dataPrevisaoInput = ref('')
const formaPagamentoRetirada = ref('DINHEIRO')
const parcelasRetirada = ref(1)

const infoWhatsApp = ref({
  telefone: '',
  mensagem: '',
  numeroOS: '',
  clienteNome: '',
  urlWhatsApp: ''
})
const tipoWhatsAppAtual = ref('cadastro')
const carregandoWhatsApp = ref(false)

const formatarMoeda = (val) => {
  const num = Number(val) || 0
  return num.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

const formatarDataCurta = (dataStr) => {
  if (!dataStr) return ''
  const d = new Date(dataStr)
  return isNaN(d.getTime()) ? '' : d.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' })
}

const formatarDataBR = (dataStr) => {
  if (!dataStr) return '--'
  const d = new Date(dataStr)
  return isNaN(d.getTime()) ? '--' : d.toLocaleDateString('pt-BR')
}

const obterStatusPrevisaoLente = (dataPrevisaoStr) => {
  if (!dataPrevisaoStr) return 'SEM_DATA'
  const dataPrevisao = new Date(dataPrevisaoStr)
  const hoje = new Date()
  
  dataPrevisao.setHours(0, 0, 0, 0)
  hoje.setHours(0, 0, 0, 0)

  const diffDias = Math.floor((dataPrevisao - hoje) / (1000 * 60 * 60 * 24))
  if (diffDias < 0) return 'ATRASADA'
  if (diffDias === 0) return 'HOJE'
  return 'NO_PRAZO'
}

const obterClasseBadgePrevisao = (dataPrevisaoStr) => {
  const status = obterStatusPrevisaoLente(dataPrevisaoStr)
  if (status === 'HOJE') return 'bg-amber-100 text-amber-900 border-amber-300 font-black animate-pulse'
  if (status === 'ATRASADA') return 'bg-rose-100 text-rose-900 border-rose-300 font-black'
  return 'bg-purple-100 text-purple-900 border-purple-200'
}

const obterTextoBadgePrevisao = (dataPrevisaoStr) => {
  const status = obterStatusPrevisaoLente(dataPrevisaoStr)
  if (status === 'HOJE') return '⚡ Chega Hoje'
  if (status === 'ATRASADA') return '⚠️ Atrasada'
  return formatarDataCurta(dataPrevisaoStr)
}

const aplicarFiltros = () => {
  router.get('/ordens/kanban', {
    vendedorId: filtroVendedor.value || undefined,
    busca: filtroBusca.value || undefined
  }, {
    preserveState: true,
    preserveScroll: true
  })
}

const limparFiltros = () => {
  filtroVendedor.value = ''
  filtroBusca.value = ''
  router.get('/ordens/kanban')
}

const filtrarApenasLentesHoje = () => {
  filtroBusca.value = ''
  alert('Exibindo ordens com previsão de lentes para hoje na coluna "Aguardando Lente"!')
}

const filtrarApenasLentesAtrasadas = () => {
  filtroBusca.value = ''
  alert('Exibindo ordens com previsão atrasada na coluna "Aguardando Lente"!')
}

// 1. AÇÃO: Confirmar OS
const confirmarOS = async (id) => {
  if (!confirm('Deseja confirmar esta Ordem de Serviço e avançar no fluxo?')) return
  try {
    salvandoFluxo.value = true
    await axios.post(`/ordens/confirmar/${id}`)
    router.reload({ preserveScroll: true })
  } catch (err) {
    alert(err.response?.data?.mensagem || 'Erro ao confirmar a OS.')
  } finally {
    salvandoFluxo.value = false
  }
}

// 2. AÇÃO: Abrir Modal Pedir Lente
const abrirModalPedirLente = (os) => {
  osSelecionada.value = os
  const hojeMaisTres = new Date()
  hojeMaisTres.setDate(hojeMaisTres.getDate() + 3)
  dataPrevisaoInput.value = hojeMaisTres.toISOString().split('T')[0]
  modalPedirLenteAberto.value = true
}

const definirAtalhoData = (dias) => {
  const dt = new Date()
  dt.setDate(dt.getDate() + dias)
  dataPrevisaoInput.value = dt.toISOString().split('T')[0]
}

const salvarPedidoLente = async () => {
  if (!osSelecionada.value || !dataPrevisaoInput.value) return
  try {
    salvandoFluxo.value = true
    await axios.post(`/ordens/pedir-lente/${osSelecionada.value.id || osSelecionada.value.Id}`, {
      dataPrevisaoLente: dataPrevisaoInput.value
    })
    modalPedirLenteAberto.value = false
    router.reload({ preserveScroll: true })
  } catch (err) {
    alert(err.response?.data?.mensagem || 'Erro ao registrar pedido de lente.')
  } finally {
    salvandoFluxo.value = false
  }
}

// Mover Direto Montagem (quando não precisa pedir ao laboratório)
const moverDiretoMontagem = async (id) => {
  if (!confirm('Deseja mover esta OS diretamente para a fila de Montagem?')) return
  try {
    salvandoFluxo.value = true
    await axios.post(`/ordens/alterar-status/${id}?novoStatus=EM_MONTAGEM`)
    router.reload({ preserveScroll: true })
  } catch (err) {
    alert('Erro ao mover para montagem.')
  } finally {
    salvandoFluxo.value = false
  }
}

// 3. AÇÃO: Marcar que a lente chegou
const marcarLenteChegou = async (id) => {
  if (!confirm('Confirmar recebimento da lente? A OS será movida para Montagem.')) return
  try {
    salvandoFluxo.value = true
    await axios.post(`/ordens/lente-chegou/${id}`)
    router.reload({ preserveScroll: true })
  } catch (err) {
    alert(err.response?.data?.mensagem || 'Erro ao registrar chegada da lente.')
  } finally {
    salvandoFluxo.value = false
  }
}

// 4. AÇÃO: Concluir Montagem
const concluirMontagem = async (id) => {
  if (!confirm('Montagem concluída com sucesso? A OS será marcada como PRONTA para retirada.')) return
  try {
    salvandoFluxo.value = true
    await axios.post(`/ordens/concluir-montagem/${id}`)
    router.reload({ preserveScroll: true })
  } catch (err) {
    alert(err.response?.data?.mensagem || 'Erro ao concluir montagem.')
  } finally {
    salvandoFluxo.value = false
  }
}

// 5. AÇÃO: WhatsApp Modal
const abrirModalWhatsApp = async (os, tipo) => {
  tipoWhatsAppAtual.value = tipo
  osSelecionada.value = os
  modalWhatsAppAberto.value = true
  carregandoWhatsApp.value = true

  try {
    const { data } = await axios.get(`/api/ordens/${os.id || os.Id}/whatsapp-info?tipo=${tipo}`)
    infoWhatsApp.value = {
      telefone: data.telefone || '',
      mensagem: data.mensagem || '',
      numeroOS: data.numeroOS || os.numeroOS,
      clienteNome: data.clienteNome || os.clienteNome,
      urlWhatsApp: data.urlWhatsApp || ''
    }
  } catch (err) {
    alert('Erro ao carregar mensagem de WhatsApp.')
    modalWhatsAppAberto.value = false
  } finally {
    carregandoWhatsApp.value = false
  }
}

const gerarLinkWhatsAppAtual = () => {
  const tel = (infoWhatsApp.value.telefone || '').replace(/\D/g, '')
  const msg = encodeURIComponent(infoWhatsApp.value.mensagem || '')
  return tel ? `https://api.whatsapp.com/send?phone=${tel}&text=${msg}` : '#'
}

const copiarTextoWhatsApp = () => {
  navigator.clipboard.writeText(infoWhatsApp.value.mensagem || '')
  alert('Mensagem copiada para a área de transferência!')
}

const registrarEnvioWhatsApp = async () => {
  try {
    if (osSelecionada.value) {
      await axios.post(`/api/ordens/${osSelecionada.value.id || osSelecionada.value.Id}/marcar-whatsapp-enviado`, {
        tipo: tipoWhatsAppAtual.value
      })
    }
  } catch (e) {}
}

// 6. AÇÃO: Modal Entrega e Quitação
const abrirModalEntrega = (os) => {
  osSelecionada.value = os
  formaPagamentoRetirada.value = 'DINHEIRO'
  parcelasRetirada.value = 1
  modalEntregaAberto.value = true
}

const confirmarEntrega = async () => {
  if (!osSelecionada.value) return
  const id = osSelecionada.value.id || osSelecionada.value.Id
  const saldo = Number(osSelecionada.value.valorRestante || osSelecionada.value.ValorRestante || 0)

  try {
    salvandoFluxo.value = true
    await axios.post(`/ordens/quitar-e-entregar/${id}`, {
      opcaoQuitacao: saldo > 0 ? 'PAGO_RETIRADA' : 'JA_PAGO',
      formaPagamentoRetirada: formaPagamentoRetirada.value,
      parcelasRetirada: formaPagamentoRetirada.value === 'CARTAO_CREDITO' ? parcelasRetirada.value : 1
    })
    modalEntregaAberto.value = false
    router.reload({ preserveScroll: true })
  } catch (err) {
    alert(err.response?.data?.mensagem || 'Erro ao finalizar entrega da OS.')
  } finally {
    salvandoFluxo.value = false
  }
}

// 7. AÇÃO: Ver Detalhes Completos da OS
const abrirDetalhesOS = (os) => {
  osDetalhe.value = os
  modalDetalhesAberto.value = true
}
</script>
