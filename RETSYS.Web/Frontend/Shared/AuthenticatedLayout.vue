<template>
  <div class="min-h-screen bg-slate-50 font-sans text-slate-900 flex">
    
    <!-- BACKDROP MOBILE -->
    <div 
      v-if="sidebarMobileAberta" 
      @click="sidebarMobileAberta = false"
      class="fixed inset-0 bg-slate-950/80 backdrop-blur-xs z-40 md:hidden transition-opacity"
    ></div>

    <!-- BARRA LATERAL (SIDEBAR COM DESIGN SAAS MODERNO INSPIRADO EM SSÓTICA E PWI) -->
    <aside 
      :class="[
        'fixed inset-y-0 left-0 z-50 w-64 bg-slate-950 text-slate-300 border-r border-slate-800 flex flex-col transition-transform duration-300 ease-in-out no-print shadow-2xl',
        sidebarMobileAberta ? 'translate-x-0' : '-translate-x-full md:translate-x-0'
      ]"
    >
      <!-- Cabeçalho da Sidebar (Logo & Marca) -->
      <div class="flex items-center justify-between px-5 py-4 border-b border-slate-800 shrink-0 bg-slate-950">
        <Link href="/dashboard" class="flex items-center gap-3 group">
          <img 
            :src="'/img/logo-wo.png'" 
            alt="WO Logo" 
            class="h-9 w-auto object-contain group-hover:scale-105 transition duration-200"
          />
          <div class="flex flex-col">
            <span class="text-xl font-black tracking-wider text-white font-mono leading-none">
              RET<span class="text-teal-400">SYS</span>
            </span>
            <span class="text-[9px] font-mono text-slate-400 tracking-widest uppercase mt-0.5">Gestão Óptica Pro</span>
          </div>
        </Link>
        
        <!-- Botão Fechar no Mobile -->
        <button 
          @click="sidebarMobileAberta = false" 
          class="md:hidden text-slate-400 hover:text-white p-1 rounded-lg hover:bg-slate-800 transition"
          title="Fechar Menu"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <!-- Menu de Navegação Segmentado -->
      <div class="flex-1 overflow-y-auto px-3.5 py-4 space-y-5 custom-scrollbar text-xs">

        <!-- 1. PAINEL PRINCIPAL -->
        <div>
          <div class="px-3 mb-1.5 text-[10px] font-mono font-bold tracking-wider text-slate-500 uppercase">
            Painel Geral
          </div>
          <div class="space-y-1">
            <Link 
              href="/dashboard" 
              :class="itemClasses('/dashboard', true)"
              @click="sidebarMobileAberta = false"
            >
              <svg class="w-4 h-4 shrink-0 text-teal-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
              </svg>
              <span>Dashboard de Performance</span>
            </Link>
          </div>
        </div>

        <!-- 2. ATENDIMENTO, ORDENS DE SERVIÇO & LABORATÓRIO -->
        <div>
          <div class="px-3 mb-1.5 text-[10px] font-mono font-bold tracking-wider text-slate-500 uppercase flex items-center justify-between">
            <span>Atendimento &amp; Vendas</span>
            <span class="text-teal-400 text-[10px]">OS</span>
          </div>
          <div class="space-y-1">
            <Link 
              href="/ordens" 
              :class="itemClasses('/ordens', true)"
              @click="sidebarMobileAberta = false"
            >
              <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
              <span>Ordens de Serviço (OS)</span>
            </Link>

            <Link 
              href="/ordens/nova" 
              :class="itemClasses('/ordens/nova')"
              @click="sidebarMobileAberta = false"
            >
              <svg class="w-4 h-4 shrink-0 text-emerald-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
              <div class="flex items-center justify-between flex-1">
                <span class="font-bold text-white">Emitir Nova O.S.</span>
                <span class="text-[9px] bg-emerald-500/20 text-emerald-300 px-1.5 py-0.2 rounded border border-emerald-500/40 font-mono font-bold">+ Criar</span>
              </div>
            </Link>

            <Link 
              href="/laboratorio" 
              :class="itemClasses('/laboratorio')"
              @click="sidebarMobileAberta = false"
            >
              <svg class="w-4 h-4 shrink-0 text-cyan-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z" />
              </svg>
              <div class="flex items-center justify-between flex-1">
                <span>Laboratório &amp; Montagem</span>
                <span class="text-[9px] bg-cyan-950 text-cyan-300 px-1.5 py-0.2 rounded border border-cyan-800">Lab</span>
              </div>
            </Link>

            <Link 
              href="/ordens/kanban" 
              :class="itemClasses('/ordens/kanban')"
              @click="sidebarMobileAberta = false"
            >
              <svg class="w-4 h-4 shrink-0 text-indigo-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 17V7m0 10a2 2 0 01-2 2H5a2 2 0 01-2-2V7a2 2 0 012-2h2a2 2 0 012 2m0 10a2 2 0 002 2h2a2 2 0 002-2M9 7a2 2 0 012-2h2a2 2 0 012 2m0 10V7m0 10a2 2 0 002 2h2a2 2 0 002-2V7a2 2 0 00-2-2h-2a2 2 0 00-2 2" />
              </svg>
              <span>Quadro Kanban OS</span>
            </Link>

            <Link 
              href="/vendas/pdv" 
              :class="itemClasses('/vendas/pdv')"
              @click="sidebarMobileAberta = false"
            >
              <svg class="w-4 h-4 shrink-0 text-amber-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
              </svg>
              <div class="flex items-center justify-between flex-1">
                <span class="font-bold text-amber-300">PDV Balcão Rápido</span>
                <span class="text-[9px] bg-amber-500/20 text-amber-300 px-1.5 py-0.2 rounded border border-amber-500/40 font-mono font-bold">PDV</span>
              </div>
            </Link>

            <Link 
              href="/vendas" 
              :class="itemClasses('/vendas', true)"
              @click="sidebarMobileAberta = false"
            >
              <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
              </svg>
              <span>Histórico de Vendas</span>
            </Link>
          </div>
        </div>

        <!-- 3. CRM ÓPTICO & PACIENTES -->
        <div>
          <div class="px-3 mb-1.5 text-[10px] font-mono font-bold tracking-wider text-slate-500 uppercase flex items-center justify-between">
            <span>CRM &amp; Prescrições</span>
            <span class="text-pink-400 text-[10px]">CRM</span>
          </div>
          <div class="space-y-1">
            <Link 
              href="/crm" 
              :class="itemClasses('/crm')"
              @click="sidebarMobileAberta = false"
            >
              <svg class="w-4 h-4 shrink-0 text-amber-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
              </svg>
              <div class="flex items-center justify-between flex-1">
                <span class="font-bold text-amber-300">CRM &amp; Recall (1 Ano)</span>
                <span class="text-[9px] bg-amber-500/20 text-amber-300 px-1.5 py-0.2 rounded border border-amber-500/40 font-mono font-bold">Recall</span>
              </div>
            </Link>

            <Link 
              href="/clientes" 
              :class="itemClasses('/clientes', true)"
              @click="sidebarMobileAberta = false"
            >
              <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
              </svg>
              <span>Clientes &amp; Prontuários</span>
            </Link>

            <Link 
              href="/clientes/aniversariantes" 
              :class="itemClasses('/clientes/aniversariantes')"
              @click="sidebarMobileAberta = false"
            >
              <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 15.546c-.523 0-1.046.151-1.5.454a2.704 2.704 0 01-3 0 2.704 2.704 0 00-3 0 2.704 2.704 0 01-3 0 2.704 2.704 0 00-3 0 2.704 2.704 0 01-1.5-.454M9 6v2m3-2v2m3-2v2M9 3h.01M12 3h.01M15 3h.01M21 21v-7a2 2 0 00-2-2H5a2 2 0 00-2 2v7h18zm-3-9v-2a2 2 0 00-2-2H8a2 2 0 00-2 2v2h12z" />
              </svg>
              <div class="flex items-center justify-between flex-1">
                <span>Aniversariantes do Mês</span>
                <span class="text-[9px] bg-pink-950 text-pink-300 px-1.5 py-0.2 rounded border border-pink-800">Mês</span>
              </div>
            </Link>
          </div>
        </div>

        <!-- 4. FINANCEIRO & CREDIÁRIO PRÓPRIO -->
        <div>
          <div class="px-3 mb-1.5 text-[10px] font-mono font-bold tracking-wider text-slate-500 uppercase flex items-center justify-between">
            <span>Financeiro &amp; Carnês</span>
            <span class="text-emerald-400 text-[10px]">R$</span>
          </div>
          <div class="space-y-1">
            <Link 
              href="/caixa" 
              :class="itemClasses('/caixa', true)"
              @click="sidebarMobileAberta = false"
            >
              <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span>Fluxo de Caixa Diário</span>
            </Link>

            <template v-if="temPermissaoAdmin">
              <Link 
                href="/caixa/fechamento" 
                :class="itemClasses('/caixa/fechamento')"
                @click="sidebarMobileAberta = false"
              >
                <svg class="w-4 h-4 shrink-0 text-indigo-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
                <div class="flex items-center justify-between flex-1">
                  <span>Fechamento de Caixa</span>
                  <span class="text-[9px] bg-indigo-950 text-indigo-300 px-1.5 py-0.2 rounded border border-indigo-800">Admin</span>
                </div>
              </Link>
            </template>

            <Link 
              href="/minhas-comissoes" 
              :class="itemClasses('/minhas-comissoes')"
              @click="sidebarMobileAberta = false"
            >
              <svg class="w-4 h-4 shrink-0 text-yellow-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11.049 2.927c.3-.921 1.603-.921 1.902 0l1.519 4.674a1 1 0 00.95.69h4.915c.969 0 1.371 1.24.588 1.81l-3.976 2.888a1 1 0 00-.363 1.118l1.518 4.674c.3.922-.755 1.688-1.538 1.118l-3.976-2.888a1 1 0 00-1.176 0l-3.976 2.888c-.783.57-1.838-.197-1.538-1.118l1.518-4.674a1 1 0 00-.363-1.118l-3.976-2.888c-.784-.57-.38-1.81.588-1.81h4.914a1 1 0 00.951-.69l1.519-4.674z" />
              </svg>
              <span>Minhas Comissões</span>
            </Link>

            <Link 
              v-if="temPermissaoAdmin"
              href="/admin/comissoes" 
              :class="itemClasses('/admin/comissoes')"
              @click="sidebarMobileAberta = false"
            >
              <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
              </svg>
              <div class="flex items-center justify-between flex-1">
                <span>Comissões da Equipe</span>
                <span class="text-[9px] bg-indigo-950 text-indigo-300 px-1.5 py-0.2 rounded border border-indigo-800">Admin</span>
              </div>
            </Link>
          </div>
        </div>

        <!-- 5. ESTOQUE & CATÁLOGO ÓPTICO -->
        <div>
          <div class="px-3 mb-1.5 text-[10px] font-mono font-bold tracking-wider text-slate-500 uppercase flex items-center justify-between">
            <span>Estoque &amp; Catálogo</span>
            <span class="text-teal-400 text-[10px]">👓</span>
          </div>
          <div class="space-y-1">
            <Link 
              href="/estoque" 
              :class="itemClasses('/estoque')"
              @click="sidebarMobileAberta = false"
            >
              <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
              </svg>
              <span>Armações &amp; Óculos</span>
            </Link>

            <Link 
              href="/lentes" 
              :class="itemClasses('/lentes')"
              @click="sidebarMobileAberta = false"
            >
              <svg class="w-4 h-4 shrink-0 text-cyan-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0zM10 7v3m0 0v3m0-3h3m-3 0H7" />
              </svg>
              <span>Tabela de Lentes</span>
            </Link>

            <Link 
              href="/marcas" 
              :class="itemClasses('/marcas')"
              @click="sidebarMobileAberta = false"
            >
              <svg class="w-4 h-4 shrink-0 text-purple-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
              </svg>
              <span>Marcas &amp; Grifes</span>
            </Link>
          </div>
        </div>

        <!-- 6. GESTÃO, LOJAS & CONFIGURAÇÕES (ADMINISTRATIVO) -->
        <div v-if="temPermissaoAdmin">
          <div class="px-3 mb-1.5 text-[10px] font-mono font-bold tracking-wider text-slate-500 uppercase flex items-center justify-between">
            <span>Gestão da Rede</span>
            <span class="text-purple-400 text-[10px]">⚙️</span>
          </div>
          <div class="space-y-1">
            <Link 
              href="/equipe" 
              :class="itemClasses('/equipe')"
              @click="sidebarMobileAberta = false"
            >
              <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
              </svg>
              <span>Usuários &amp; Equipe</span>
            </Link>

            <Link 
              href="/configuracoes" 
              :class="itemClasses('/configuracoes')"
              @click="sidebarMobileAberta = false"
            >
              <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
              <span>Parâmetros da Ótica</span>
            </Link>
          </div>
        </div>

      </div>

      <!-- Rodapé da Sidebar: Info da Ótica e Perfil -->
      <div class="p-3 border-t border-slate-800 bg-slate-950 shrink-0 text-xs">
        <div class="flex items-center justify-between px-3 py-2 bg-slate-900 rounded-xl border border-slate-800">
          <div class="truncate mr-2">
            <span class="text-[9px] uppercase font-bold text-slate-500 block tracking-wider">Ótica Conectada</span>
            <span class="text-xs font-bold text-teal-300 truncate block">{{ nomeOtica }}</span>
          </div>
          <span class="text-[10px] font-mono text-teal-400 font-bold bg-teal-950 px-2 py-0.5 rounded border border-teal-800/80 shrink-0 uppercase">
            {{ perfil }}
          </span>
        </div>
      </div>
    </aside>

    <!-- ÁREA PRINCIPAL DA APLICAÇÃO (CONTEÚDO + HEADER) -->
    <div class="flex-1 flex flex-col md:pl-64 min-w-0">

      <!-- CABEÇALHO SUPERIOR (TOP NAVBAR MODERNO COM SELETOR DE LOJAS E AÇÕES RÁPIDAS) -->
      <header class="bg-slate-950 text-white px-4 md:px-6 py-2.5 flex items-center justify-between border-b border-slate-800 sticky top-0 z-30 shadow-md">
        
        <!-- LADO ESQUERDO: Botão Mobile e Seletor de Loja Ativa -->
        <div class="flex items-center gap-3">
          <button 
            @click="sidebarMobileAberta = true" 
            class="md:hidden text-slate-400 hover:text-white p-2 rounded-xl bg-slate-900 border border-slate-800 hover:bg-slate-800 transition"
            title="Abrir Menu Lateral"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
            </svg>
          </button>

          <!-- SELETOR DE ÓTICA ATIVA (MULTI-LOJA NATIVO) -->
          <div class="relative">
            <button 
              v-if="podeAlternarOtica"
              @click="menuOticasAberto = !menuOticasAberto" 
              class="flex items-center gap-2.5 bg-slate-900 hover:bg-slate-800 border border-teal-500/40 hover:border-teal-400 px-3 py-1.5 rounded-xl text-left transition shadow-sm group"
              title="Clique para alternar entre as lojas da rede"
            >
              <span class="flex items-center justify-center w-6 h-6 rounded-lg bg-teal-500/20 text-teal-300 text-xs font-black">
                🏢
              </span>
              <div class="flex flex-col text-left">
                <span class="text-[9px] uppercase font-mono font-bold tracking-wider text-teal-400 leading-none">
                  Ótica em Operação ▾
                </span>
                <span class="text-xs font-bold text-white max-w-[150px] sm:max-w-[200px] truncate block leading-tight mt-0.5">
                  {{ nomeOtica }}
                </span>
              </div>
            </button>

            <!-- Badge Fixo se tiver apenas 1 ótica -->
            <div 
              v-else
              class="flex items-center gap-2 bg-slate-900/80 border border-slate-800 px-3 py-1.5 rounded-xl text-xs font-bold text-slate-200"
            >
              <span class="text-teal-400">🏢</span>
              <span class="truncate max-w-[180px]">{{ nomeOtica }}</span>
            </div>

            <!-- Dropdown das Óticas Cadastradas -->
            <div 
              v-if="menuOticasAberto" 
              class="absolute left-0 top-12 w-72 bg-slate-900 text-white rounded-2xl shadow-2xl border border-teal-500/30 p-2.5 z-50 animate-fadeIn"
            >
              <div class="px-2.5 py-1.5 border-b border-slate-800 mb-2 flex items-center justify-between">
                <div>
                  <p class="text-[10px] font-black uppercase tracking-wider text-teal-400">
                    Lojas &amp; Filiais da Rede
                  </p>
                  <p class="text-[11px] text-slate-400">Selecione para filtrar dados:</p>
                </div>
                <button 
                  v-if="ehSistema"
                  @click="abrirModalNovaOtica"
                  class="text-[10px] bg-teal-600 hover:bg-teal-500 text-white px-2 py-1 rounded-lg font-bold transition flex items-center gap-1"
                  title="Cadastrar Nova Ótica"
                >
                  <span>+</span> Loja
                </button>
              </div>

              <div class="max-h-64 overflow-y-auto space-y-1 pr-1 custom-scrollbar">
                <button 
                  v-for="otica in (oticasDisponiveis || [])" 
                  :key="otica.id"
                  @click="selecionarOtica(otica.id)"
                  :class="otica.nome === nomeOtica || otica.id === oticaIdAtual ? 'bg-teal-500/20 border-teal-500/60 text-teal-200 font-bold' : 'hover:bg-slate-800 text-slate-300 border-transparent'"
                  class="w-full text-left px-3 py-2.5 rounded-xl text-xs flex items-center justify-between border transition"
                >
                  <div class="flex items-center gap-2 truncate">
                    <span class="text-sm">🏬</span>
                    <span class="truncate">{{ otica.nome }}</span>
                  </div>
                  <span v-if="otica.nome === nomeOtica || otica.id === oticaIdAtual" class="text-teal-400 font-bold text-sm">✓</span>
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- LADO DIREITO: Ações Rápidas (Nova OS e PDV), Cronômetro e Perfil -->
        <div class="relative flex items-center gap-2 sm:gap-3">
          
          <!-- BOTÕES DE AÇÃO RÁPIDA (PADRÃO SSÓTICA / PWI) -->
          <div class="hidden sm:flex items-center gap-2">
            <Link 
              href="/ordens/nova" 
              class="flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-teal-600 hover:bg-teal-500 text-white font-bold text-xs shadow-sm transition active:scale-95"
              title="Emitir Nova Ordem de Serviço"
            >
              <span>+</span>
              <span>Nova OS</span>
            </Link>

            <Link 
              href="/vendas/pdv" 
              class="flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-amber-600 hover:bg-amber-500 text-white font-bold text-xs shadow-sm transition active:scale-95"
              title="Abrir PDV Rápido de Balcão"
            >
              <span>🛒</span>
              <span>PDV Balcão</span>
            </Link>
          </div>

          <!-- CRONÔMETRO DE SESSÃO -->
          <div class="text-right hidden lg:block border-l border-slate-800 pl-3">
            <span class="text-xs font-bold text-slate-200 block">{{ nomeUsuario }}</span>
            <p class="text-[10px] text-slate-400 font-mono">
              Online: <span class="text-teal-400 font-bold">{{ tempoConectado }}</span>
            </p>
          </div>

          <!-- BOTÃO DO PERFIL COM AVATAR -->
          <div class="relative">
            <button 
              @click="menuAberto = !menuAberto" 
              class="w-9 h-9 rounded-full border-2 border-teal-500 overflow-hidden focus:outline-none active:scale-95 transition bg-slate-800 flex items-center justify-center shrink-0 shadow-sm"
              title="Meu Perfil"
            >
              <img 
                v-if="fotoPerfil" 
                :src="fotoPerfil" 
                alt="Foto de Perfil" 
                class="w-full h-full object-cover" 
              />
              <span v-else class="text-xs font-bold text-teal-400 uppercase font-mono">
                {{ nomeUsuario?.substring(0, 2) }}
              </span>
            </button>

            <!-- MENU SUSPENSO DO USUÁRIO -->
            <div 
              v-if="menuAberto" 
              class="absolute right-0 top-12 w-52 bg-white rounded-2xl shadow-xl border border-slate-200 p-2 text-slate-800 z-50 animate-fadeIn"
            >
              <div class="px-3 py-2 border-b border-slate-100 mb-1">
                <p class="text-xs font-bold text-slate-900 truncate">{{ nomeUsuario }}</p>
                <p class="text-[10px] text-teal-700 uppercase font-mono font-bold">{{ perfil }} • {{ nomeOtica }}</p>
              </div>
              <Link 
                href="/perfil" 
                @click="menuAberto = false"
                class="block px-3 py-2 text-xs font-semibold hover:bg-slate-50 rounded-xl transition text-slate-700"
              >
                👤 Minha Conta
              </Link>
              <Link 
                href="/logout" 
                method="post" 
                as="button" 
                class="w-full text-left block px-3 py-2 text-xs font-bold text-rose-600 hover:bg-rose-50 rounded-xl transition border-t border-slate-100 mt-1"
              >
                🚪 Sair do Sistema
              </Link>
            </div>
          </div>

        </div>

      </header>

      <!-- CONTEÚDO PRINCIPAL DAS PÁGINAS -->
      <main class="flex-grow">
        <slot />
      </main>

      <!-- RODAPÉ GERAL DO PAINEL -->
      <footer class="py-4 border-t border-slate-200 bg-white mt-auto no-print">
        <div class="max-w-7xl mx-auto px-6 flex flex-col sm:flex-row items-center justify-between gap-3 text-xs text-slate-400 font-medium">
          <p>© {{ new Date().getFullYear() }} RETSYS Óticas. Todos os direitos reservados.</p>
          <p class="flex items-center gap-1.5 font-mono text-[11px]">
            <span>Ambiente Operacional Seguro</span>
            <span class="font-bold text-teal-700 bg-teal-50 px-2 py-0.5 rounded border border-teal-200">v2.0 Especialista</span>
          </p>
        </div>
      </footer>

      <!-- WIDGET SPOTIFY -->
      <div class="fixed bottom-4 right-4 z-40 hidden md:block no-print">
        <SpotifyPlayer />
      </div>

    </div>

    <!-- MODAL PARA CRIAÇÃO RÁPIDA DE NOVA ÓTICA (EXCLUSIVO SISTEMA) -->
    <div v-if="modalNovaOticaAberta" class="fixed inset-0 bg-slate-950/70 backdrop-blur-sm z-50 flex items-center justify-center p-4">
      <div class="bg-white rounded-3xl border border-slate-200 shadow-2xl max-w-md w-full p-6 space-y-5">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <div class="flex items-center gap-2">
            <span class="w-2.5 h-2.5 rounded-full bg-teal-600"></span>
            <h3 class="text-base font-bold text-slate-950 font-mono">Cadastrar Nova Loja / Filial</h3>
          </div>
          <button @click="modalNovaOticaAberta = false" class="text-slate-400 hover:text-slate-800 font-bold">✕</button>
        </div>

        <form @submit.prevent="salvarNovaOtica" class="space-y-4 text-xs">
          <div>
            <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1">Nome Fantasia da Ótica *</label>
            <input 
              v-model="formNovaOtica.nome" 
              type="text" 
              placeholder="Ex: Ótica RETSYS - Filial Centro" 
              class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" 
              required 
            />
          </div>

          <div class="flex justify-end gap-2 pt-3 border-t border-slate-100">
            <button type="button" @click="modalNovaOticaAberta = false" class="px-4 py-2 rounded-xl text-slate-600 font-bold hover:bg-slate-100">
              Cancelar
            </button>
            <button 
              type="submit" 
              :disabled="criandoOtica" 
              class="px-5 py-2 rounded-xl bg-teal-600 hover:bg-teal-700 text-white font-bold transition shadow-sm"
            >
              {{ criandoOtica ? 'Criando...' : 'Criar e Ativar Ótica' }}
            </button>
          </div>
        </form>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { Link, usePage, router } from '@inertiajs/vue3'
import SpotifyPlayer from './SpotifyPlayer.vue'

const page = usePage()
const sidebarMobileAberta = ref(false)
const menuAberto = ref(false)
const menuOticasAberto = ref(false)
const modalNovaOticaAberta = ref(false)
const criandoOtica = ref(false)
const formNovaOtica = ref({ nome: '' })
const tempoConectado = ref('00:00:00')
let cronometro = null

const authData = computed(() => page.props.auth || {})
const perfil = computed(() => authData.value.usuarioPerfil || 'Vendedor')
const ehSistema = computed(() => !!authData.value.ehSistema || perfil.value === 'Sistema')
const ehDono = computed(() => !!authData.value.ehDono || perfil.value === 'Dono')
const temPermissaoAdmin = computed(() => ehSistema.value || ehDono.value || perfil.value === 'Admin' || perfil.value === 'Gerente')
const nomeUsuario = computed(() => authData.value.usuarioNome || 'Colaborador')
const fotoPerfil = computed(() => authData.value.usuarioFoto || null)
const nomeOtica = computed(() => authData.value.oticaNome || 'Ótica RETSYS')
const oticaIdAtual = computed(() => authData.value.oticaId || null)

const oticasDisponiveis = computed(() => {
  const lista = authData.value.oticasDisponiveis || []
  return lista.map(o => ({
    ...o,
    nome: o.nome ? o.nome.trim() : ''
  }))
})

// O seletor de óticas é exibido para Donos, Administradores, Gerentes e Perfil Sistema sempre que houver mais de 1 loja
const podeAlternarOtica = computed(() => {
  return ehSistema.value || ((ehDono.value || temPermissaoAdmin.value) && oticasDisponiveis.value.length > 1)
})

const itemClasses = (href, exact = false, excludes = []) => {
  const currentUrl = page.url || ''
  let active = false

  if (exact) {
    active = currentUrl === href || (href === '/dashboard' && currentUrl === '/')
  } else {
    active = currentUrl.startsWith(href)
    if (active && excludes.length > 0) {
      if (excludes.some(ex => currentUrl.startsWith(ex))) {
        active = false
      }
    }
  }

  const base = 'flex items-center gap-3 px-3.5 py-2.5 rounded-xl transition duration-150 font-medium'
  if (active) {
    return `${base} bg-teal-500/15 text-teal-300 font-bold border-l-2 border-teal-400 shadow-xs shadow-teal-950/40`
  }
  return `${base} text-slate-400 hover:text-white hover:bg-slate-900`
}

const selecionarOtica = (id) => {
  menuOticasAberto.value = false
  router.post('/sistema/trocar-otica', { oticaId: id }, {
    preserveScroll: true
  })
}

const abrirModalNovaOtica = () => {
  menuOticasAberto.value = false
  formNovaOtica.value.nome = ''
  modalNovaOticaAberta.value = true
}

const salvarNovaOtica = () => {
  if (!formNovaOtica.value.nome.trim()) return
  criandoOtica.value = true

  router.post('/sistema/criar-otica', { nome: formNovaOtica.value.nome }, {
    preserveScroll: true,
    onSuccess: () => {
      modalNovaOticaAberta.value = false
      criandoOtica.value = false
    },
    onError: () => {
      criandoOtica.value = false
    }
  })
}

onMounted(() => {
  const tempoInicio = Date.now()
  
  cronometro = setInterval(() => {
    const totalSegundos = Math.floor((Date.now() - tempoInicio) / 1000)
    const horas = String(Math.floor(totalSegundos / 3600)).padStart(2, '0')
    const minutos = String(Math.floor((totalSegundos % 3600) / 60)).padStart(2, '0')
    const segundos = String(totalSegundos % 60).padStart(2, '0')
    
    tempoConectado.value = `${horas}:${minutos}:${segundos}`
  }, 1000)
})

onUnmounted(() => {
  if (cronometro) clearInterval(cronometro)
})
</script>
