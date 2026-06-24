<template>
  <div class="min-h-screen bg-slate-50 flex flex-col justify-between font-sans selection:bg-teal-500 selection:text-white antialiased">
    
    <header class="w-full max-w-7xl mx-auto px-6 py-5 flex items-center justify-between border-b border-slate-200/60 bg-white/80 backdrop-blur sticky top-0 z-50 rounded-b-2xl shadow-sm">
      <div class="flex items-center gap-2">
        <span class="text-2xl font-black tracking-wider text-slate-950 font-mono">
          RET<span class="text-teal-600">SYS</span>
        </span>
      </div>
      <nav class="flex items-center gap-6">
        <a 
          href="https://wa.me/5511950798396?text=Olá!%20Gostaria%20de%20suporte%20no%20painel%20da%20RET%20SYS." 
          target="_blank" 
          class="text-sm font-bold text-slate-500 hover:text-slate-900 transition flex items-center gap-1.5"
        >
          Suporte Técnico
        </a>
        <Link href="/login" class="bg-slate-950 hover:bg-slate-800 text-white text-xs font-bold uppercase tracking-wider py-2.5 px-5 rounded-xl shadow-sm transition text-center">
          Entrar no Sistema
        </Link>
      </nav>
    </header>

    <main class="w-full max-w-7xl mx-auto px-6 py-12 md:py-20 flex flex-col lg:flex-row items-center gap-16 flex-grow">
      
      <div class="w-full lg:w-1/2 space-y-6 text-center lg:text-left">
        <div class="inline-flex items-center gap-2 bg-teal-50 border border-teal-200/60 text-teal-700 px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider">
          <span class="w-2 h-2 rounded-full bg-teal-500 animate-pulse"></span>
          Gestão Inteligente para Óticas
        </div>
        
        <h2 class="text-4xl md:text-5xl lg:text-6xl font-black text-slate-950 tracking-tight leading-none">
          A evolução da sua ótica <br />
          <span class="text-teal-600">conectada ao futuro.</span>
        </h2>
        
        <p class="text-base md:text-lg text-slate-600 max-w-xl mx-auto lg:mx-0 leading-relaxed">
          Esqueça planilhas e processos manuais. Controle receitas clínicas com IA, gerencie laboratórios, monitore comissões de vendedores e automatize recebimentos com PIX e música ambiente de forma integrada.
        </p>
        
        <div class="flex flex-col sm:flex-row items-center justify-center lg:justify-start gap-4 pt-4">
          <Link href="/login" class="w-full sm:w-auto bg-teal-600 hover:bg-teal-700 text-white font-bold py-4 px-8 rounded-xl shadow-md hover:shadow-lg transition text-center text-sm uppercase tracking-wider">
            Acessar Minha Conta
          </Link>
          <a 
            href="https://wa.me/5500000000000?text=Olá!%20Gostaria de falar com o time RETSYS." 
            target="_blank" 
            class="w-full sm:w-auto bg-white border border-slate-200 text-slate-800 font-bold py-4 px-8 rounded-xl hover:bg-slate-50 transition text-center text-sm uppercase tracking-wider"
          >
            Falar com um Consultor
          </a>
        </div>
      </div>

      <div class="w-full lg:w-1/2 grid grid-cols-2 gap-4 bg-white p-6 rounded-3xl shadow-xl border border-slate-200/80 max-w-lg mx-auto">
        <div class="bg-slate-50 p-5 rounded-2xl border border-slate-100 space-y-2">
          <div class="w-8 h-8 rounded-lg bg-teal-500/10 flex items-center justify-center text-teal-600 font-bold text-xs font-mono">IA</div>
          <p class="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Leitura Clínica</p>
          <p class="text-lg font-black text-slate-900 tracking-tight">Leitor de Receitas</p>
        </div>
        
        <div class="bg-slate-50 p-5 rounded-2xl border border-slate-100 space-y-2">
          <div class="w-8 h-8 rounded-lg bg-emerald-500/10 flex items-center justify-center text-emerald-600 font-bold text-xs font-mono">PX</div>
          <p class="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Gateway Integrado</p>
          <p class="text-lg font-black text-slate-900 tracking-tight">PIX Dinâmico</p>
        </div>

        <div class="bg-slate-50 p-5 rounded-2xl border border-slate-100 space-y-2">
          <div class="w-8 h-8 rounded-lg bg-indigo-500/10 flex items-center justify-center text-indigo-600 font-bold text-xs font-mono">%</div>
          <p class="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Equipe e Caixa</p>
          <p class="text-lg font-black text-slate-900 tracking-tight">Comissões Reais</p>
        </div>

        <div class="bg-slate-50 p-5 rounded-2xl border border-slate-100 space-y-2">
          <div class="w-8 h-8 rounded-lg bg-emerald-500/10 flex items-center justify-center text-emerald-600 font-bold text-xs font-mono">🎵</div>
          <p class="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Experiência em Loja</p>
          <p class="text-lg font-black text-slate-900 tracking-tight">Som via Spotify</p>
        </div>

        <div class="col-span-2 bg-slate-950 p-5 rounded-2xl text-white flex items-center justify-between">
          <div>
            <p class="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Segurança de Dados</p>
            <p class="text-sm font-bold text-teal-400">Acesso Restrito por Função (RBAC)</p>
          </div>
          <div class="w-2 h-2 rounded-full bg-teal-400 animate-pulse"></div>
        </div>
      </div>
    </main>

    <footer class="w-full border-t border-slate-200 bg-white py-6 text-center text-xs text-slate-400">
      <p>&copy; 2026 RETSYS. Sistema Inteligente de Gestão Ótica. Todos os direitos reservados.</p>
    </footer>

  </div>
</template>

<script setup>
import { Link } from '@inertiajs/vue3'
</script>