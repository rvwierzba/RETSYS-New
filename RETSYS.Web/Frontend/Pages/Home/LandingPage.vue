<template>
  <div class="min-h-screen bg-slate-50 flex flex-col justify-between font-sans selection:bg-teal-500 selection:text-white">
    
    <header class="w-full max-w-7xl mx-auto px-6 py-5 flex items-center justify-between border-b border-slate-200/60 bg-white/80 backdrop-blur sticky top-0 z-50 rounded-b-2xl shadow-sm">
      <div class="flex items-center gap-2">
        <span class="text-2xl font-black tracking-wider text-slate-950 font-mono">
          R<span class="text-teal-600">E</span>TSYS<span class="text-teal-600">.</span>
        </span>
      </div>
      <nav class="flex items-center gap-4">
        <button class="text-sm font-semibold text-slate-600 hover:text-slate-900 transition">
          Suporte
        </button>
        <Link href="/login" class="bg-slate-950 hover:bg-slate-800 text-white text-xs font-bold uppercase tracking-wider py-2.5 px-5 rounded-xl shadow-sm transition text-center">
          Entrar no Sistema
        </Link>
      </nav>
    </header>

    <main class="w-full max-w-7xl mx-auto px-6 py-16 md:py-24 flex flex-col lg:flex-row items-center gap-12 flex-grow">
      <div class="w-full lg:w-1/2 space-y-6 text-center lg:text-left">
        <h2 class="text-4xl md:text-5xl lg:text-6xl font-black text-slate-950 tracking-tight leading-none">
          O controle total da sua ótica, <br class="hidden md:inline" />
          <span class="text-teal-600">focado na visão</span> do negócio.
        </h2>
        <p class="text-base md:text-lg text-slate-600 max-w-xl mx-auto lg:mx-0 leading-relaxed">
          Gerencie ordens de serviço clínicas, controle estoque de armações por marcas, calcule graus automaticamente e automatize o caixa com PIX dinâmico. Tudo em uma plataforma rápida e segura.
        </p>
        <div class="flex flex-col sm:flex-row items-center justify-center lg:justify-start gap-4 pt-4">
          <Link href="/vendas" class="w-full sm:w-auto bg-teal-600 hover:bg-teal-700 text-white font-bold py-4 px-8 rounded-xl shadow-md hover:shadow-lg transition text-center">
            Acessar o Painel de Vendas
          </Link>
          <Link href="/estoque" class="w-full sm:w-auto bg-white border border-slate-200 text-slate-800 font-semibold py-4 px-8 rounded-xl hover:bg-slate-50 transition text-center">
            Ver Estoque de Armações
          </Link>
        </div>
      </div>

      <div class="w-full lg:w-1/2 grid grid-cols-2 gap-4 bg-white p-6 rounded-3xl shadow-xl border border-slate-200/80 max-w-lg mx-auto">
        <div class="bg-slate-50 p-5 rounded-2xl border border-slate-100 space-y-2">
          <div class="w-8 h-8 rounded-lg bg-teal-500/10 flex items-center justify-center text-teal-600 font-bold">OS</div>
          <p class="text-xs font-bold text-slate-400 uppercase tracking-wider">Ordens de Serviço</p>
          <p class="text-2xl font-black text-slate-900">Ativas</p>
        </div>
        <div class="bg-slate-50 p-5 rounded-2xl border border-slate-100 space-y-2">
          <div class="w-8 h-8 rounded-lg bg-emerald-500/10 flex items-center justify-center text-emerald-600 font-bold">$</div>
          <p class="text-xs font-bold text-slate-400 uppercase tracking-wider">Faturamento Hoje</p>
          <p class="text-2xl font-black text-slate-900">R$ 0,00</p>
        </div>
        <div class="col-span-2 bg-slate-950 p-5 rounded-2xl text-white flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-slate-400 uppercase tracking-wider">Perfil Autenticado</p>
            <p class="text-lg font-bold text-teal-400">Modo Administrativo</p>
          </div>
          <div class="w-2.5 h-2.5 rounded-full bg-teal-400 animate-pulse"></div>
        </div>
      </div>
    </main>

    <footer class="w-full border-t border-slate-200 bg-white py-6 text-center text-xs text-slate-400">
      <p>&copy; 2026 RETSYS. Sistema Gerenciador de Ótica. Todos os direitos reservados.</p>
    </footer>

  </div>
</template>

<script setup>
import { Link } from '@inertiajs/vue3';
</script>