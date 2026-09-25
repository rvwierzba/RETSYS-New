<template>
  <div class="min-h-screen bg-slate-50 font-sans text-slate-900 flex flex-col">
    
    <!-- Cabeçalho Principal sticky -->
    <header class="bg-slate-950 text-white px-6 py-3.5 flex items-center justify-between border-b border-slate-800 sticky top-0 z-50">
      
      <nav class="flex items-center space-x-6 text-sm font-medium">
        <!-- Divisão integrada com a nova logo WO e a marca RETSYS -->
        <div class="flex items-center gap-3 mr-4">
          <img 
            :src="'/img/logo-wo.png'" 
            alt="WO Logo" 
            class="h-9 w-auto object-contain hover:scale-105 transition duration-200"
          />
          <span class="text-xl font-black tracking-wider text-white font-mono">
            RET<span class="text-teal-400">SYS</span>
          </span>
        </div>
        
        <Link href="/dashboard" class="hover:text-teal-400 transition">Dashboard</Link>
        <Link href="/ordens" class="hover:text-teal-400 transition">Ordens de Serviço</Link>
        <Link v-if="temPermissaoAdmin" href="/caixa" class="hover:text-teal-400 transition">Caixa</Link>
        <Link href="/clientes" class="hover:text-teal-400 transition">Clientes</Link>
        <Link href="/estoque" class="hover:text-teal-400 transition">Armações</Link>
        <Link href="/marcas" class="hover:text-teal-400 transition">Marcas</Link>
        <Link href="/lentes" class="hover:text-teal-400 transition">Lentes</Link>

        <!-- Acesso exclusivo do Gerente / Admin / Sistema aos relatórios de Fechamento -->
        <template v-if="temPermissaoAdmin">
          <Link href="/caixa/fechamento" class="text-indigo-400 hover:text-indigo-300 transition pl-2 border-l border-slate-800 font-bold">
            • Fechamento
          </Link>
          <Link href="/equipe" class="text-indigo-400 hover:text-indigo-300 transition">
            • Gerenciar Equipe
          </Link>
          <Link href="/configuracoes" class="text-indigo-400 hover:text-indigo-300 transition">
            • Parâmetros e APIs
          </Link>
        </template>
      </nav>

      <!-- Perfil do Utilizador, Seletor de Ótica (Modo Sistema) e Menu Suspenso -->
      <div class="relative flex items-center gap-3">
        
        <!-- SELETOR DE ÓTICA EXCLUSIVO PARA PERFIL SISTEMA -->
        <div v-if="ehSistema" class="relative">
          <button 
            @click="menuOticasAberto = !menuOticasAberto" 
            class="flex items-center gap-2 bg-gradient-to-r from-purple-950/90 to-indigo-950/90 hover:from-purple-900 hover:to-indigo-900 border border-purple-500/50 px-3 py-1.5 rounded-xl text-left transition shadow-lg shadow-purple-950/30 group"
            title="Alternar Ótica Ativa (Acesso Sistema)"
          >
            <span class="flex items-center justify-center w-5 h-5 rounded-md bg-purple-500/20 text-purple-300 text-xs font-black">
              ⚡
            </span>
            <div class="hidden sm:block">
              <div class="flex items-center gap-1.5">
                <span class="text-[9px] uppercase font-black tracking-widest text-purple-300 bg-purple-900/60 px-1.5 py-0.2 rounded border border-purple-700/60">
                  SISTEMA
                </span>
                <span class="text-xs font-bold text-white max-w-[140px] truncate block">{{ nomeOtica }}</span>
              </div>
            </div>
            <span class="text-purple-300 text-xs font-bold group-hover:translate-y-0.5 transition-transform">▼</span>
          </button>

          <!-- Dropdown com a lista de óticas -->
          <div 
            v-if="menuOticasAberto" 
            class="absolute right-0 top-12 w-64 bg-slate-900 text-white rounded-2xl shadow-2xl border border-purple-500/30 p-2.5 z-50 animate-fadeIn"
          >
            <div class="px-2.5 py-1.5 border-b border-slate-800 mb-2 flex items-center justify-between">
              <div>
                <p class="text-[10px] font-black uppercase tracking-wider text-purple-400">Alternar Ambiente</p>
                <p class="text-[11px] text-slate-400">Selecione a ótica para gerenciar:</p>
              </div>
              <button 
                @click="abrirModalNovaOtica"
                class="text-[10px] bg-purple-600 hover:bg-purple-500 text-white px-2 py-1 rounded-lg font-bold transition flex items-center gap-1"
                title="Cadastrar Nova Ótica"
              >
                <span>+</span> Ótica
              </button>
            </div>

            <div class="max-h-60 overflow-y-auto space-y-1 pr-1 custom-scrollbar">
              <button 
                v-for="otica in (oticasDisponiveis || [])" 
                :key="otica.id"
                @click="selecionarOtica(otica.id)"
                :class="otica.nome === nomeOtica || otica.id === oticaIdAtual ? 'bg-purple-600/30 border-purple-500/50 text-purple-200 font-bold' : 'hover:bg-slate-800 text-slate-300 border-transparent'"
                class="w-full text-left px-3 py-2 rounded-xl text-xs flex items-center justify-between border transition"
              >
                <span class="truncate">{{ otica.nome }}</span>
                <span v-if="otica.nome === nomeOtica || otica.id === oticaIdAtual" class="text-teal-400 font-bold text-sm">✓</span>
              </button>
            </div>
          </div>
        </div>

        <!-- PERFIL PADRÃO (NÃO SISTEMA) -->
        <div v-else class="text-right hidden sm:block">
          <div class="flex items-center gap-2 justify-end">
            <span class="text-xs font-bold text-slate-200">{{ nomeUsuario }}</span>
            <span class="bg-teal-900/60 text-teal-300 border border-teal-700/60 px-2 py-0.5 rounded text-[10px] font-black uppercase tracking-wider">{{ nomeOtica }}</span>
          </div>
          <p class="text-[10px] text-slate-400 font-mono mt-0.5">
            Ligado há: <span class="text-teal-400 font-bold">{{ tempoConectado }}</span>
          </p>
        </div>

        <div v-if="ehSistema" class="text-right hidden sm:block">
          <span class="text-xs font-bold text-slate-200 block">{{ nomeUsuario }}</span>
          <p class="text-[10px] text-slate-400 font-mono">
            Ligado há: <span class="text-teal-400 font-bold">{{ tempoConectado }}</span>
          </p>
        </div>

        <button 
          @click="menuAberto = !menuAberto" 
          class="w-10 h-10 rounded-full border-2 border-teal-500 overflow-hidden focus:outline-none active:scale-95 transition bg-slate-800 flex items-center justify-center shrink-0"
        >
          <img 
            v-if="fotoPerfil" 
            :src="fotoPerfil" 
            alt="Foto de Perfil" 
            class="w-full h-full object-cover" 
          />
          <span v-else class="text-xs font-bold text-teal-400 uppercase">
            {{ nomeUsuario?.substring(0, 2) }}
          </span>
        </button>

        <div v-if="menuAberto" class="absolute right-0 top-12 w-48 bg-white rounded-xl shadow-xl border border-slate-200 p-2 text-slate-800 z-50 animate-fadeIn">
          <div class="px-3 py-1.5 border-b border-slate-100 mb-1 sm:hidden">
            <p class="text-xs font-bold truncate">{{ nomeUsuario }}</p>
            <p class="text-[9px] text-purple-600 uppercase font-bold">{{ perfil }}</p>
          </div>
          <Link href="/perfil" class="block px-3 py-2 text-xs font-semibold hover:bg-slate-50 rounded-lg transition">
            Minha Conta
          </Link>
          <Link 
            href="/logout" 
            method="post" 
            as="button" 
            class="w-full text-left block px-3 py-2 text-xs font-bold text-red-600 hover:bg-red-50 rounded-lg transition border-t border-slate-100 mt-1"
          >
            Sair do Sistema
          </Link>
        </div>
      </div>

    </header>

    <!-- Conteúdo principal -->
    <main class="flex-grow">
      <slot />
    </main>

    <!-- Modal para Criação Rápida de Nova Ótica (Exclusivo Sistema) -->
    <div v-if="modalNovaOticaAberta" class="fixed inset-0 bg-slate-950/70 backdrop-blur-sm z-50 flex items-center justify-center p-4">
      <div class="bg-white rounded-3xl border border-slate-200 shadow-2xl max-w-md w-full p-6 space-y-5">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <div class="flex items-center gap-2">
            <span class="w-2.5 h-2.5 rounded-full bg-purple-600"></span>
            <h3 class="text-base font-bold text-slate-950">Cadastrar Nova Ótica</h3>
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
              class="w-full rounded-xl border-slate-200 text-sm focus:border-purple-500 focus:ring-purple-500" 
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
              class="px-5 py-2 rounded-xl bg-purple-600 hover:bg-purple-700 text-white font-bold transition shadow-sm"
            >
              {{ criandoOtica ? 'Criando...' : 'Criar e Ativar Ótica' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Rodapé Geral do Painel com Autoria WO -->
    <footer class="py-6 border-t border-slate-200 bg-white mt-auto no-print">
      <div class="max-w-6xl mx-auto px-6 flex flex-col sm:flex-row items-center justify-between gap-4 text-xs text-slate-400 font-medium">
        <p>© {{ new Date().getFullYear() }} RETSYS. Todos os direitos reservados.</p>
        <p class="flex items-center gap-1.5">
          <span>Desenvolvido e Direitos Reservados</span>
          <span class="font-black text-slate-900 bg-slate-100 px-2.5 py-1 rounded border border-slate-200">WO</span>
        </p>
      </div>
    </footer>

    <!-- Widget de Spotify -->
    <div class="fixed bottom-4 right-4 z-40 hidden md:block no-print">
      <SpotifyPlayer />
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { Link, usePage, router } from '@inertiajs/vue3'
import SpotifyPlayer from './SpotifyPlayer.vue'

const page = usePage()
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
const temPermissaoAdmin = computed(() => ehSistema.value || perfil.value === 'Admin' || perfil.value === 'Gerente')
const nomeUsuario = computed(() => authData.value.usuarioNome || 'Colaborador')
const fotoPerfil = computed(() => authData.value.usuarioFoto || null)
const nomeOtica = computed(() => authData.value.oticaNome || 'Ótica RETSYS')
const oticaIdAtual = computed(() => authData.value.oticaId || null)
const oticasDisponiveis = computed(() => authData.value.oticasDisponiveis || [])

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