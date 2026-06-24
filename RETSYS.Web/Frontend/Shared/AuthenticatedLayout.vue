<template>
  <div class="min-h-screen bg-slate-50 font-sans text-slate-900 flex flex-col">
    
    <header class="bg-slate-950 text-white px-6 py-4 flex items-center justify-between border-b border-slate-800 sticky top-0 z-50">
      
      <nav class="flex items-center space-x-6 text-sm font-medium">
        <span class="text-xl font-black tracking-wider text-white font-mono mr-4">
          RET<span class="text-teal-400">SYS</span>
        </span>
        
        <Link href="/dashboard" class="hover:text-teal-400 transition">Dashboard</Link>
        <Link href="/ordens" class="hover:text-teal-400 transition">Ordens de Serviço</Link>
        <Link href="/clientes" class="hover:text-teal-400 transition">Clientes</Link>
        <Link href="/estoque" class="hover:text-teal-400 transition">Armações</Link>

        <template v-if="perfil === 'Admin'">
          <Link href="/equipe" class="text-indigo-400 hover:text-indigo-300 transition pl-2 border-l border-slate-800">
            • Gerenciar Equipe
          </Link>
          <Link href="/configuracoes" class="text-indigo-400 hover:text-indigo-300 transition">
            • Parâmetros e APIs
          </Link>
        </template>
      </nav>

      <div class="relative flex items-center gap-4">
        <div class="text-right hidden sm:block">
          <p class="text-xs font-bold text-slate-200">{{ nomeUsuario }}</p>
          <p class="text-[10px] text-slate-400 font-mono">
            Conectado há: <span class="text-teal-400 font-bold">{{ tempoConectado }}</span>
          </p>
        </div>

        <button 
          @click="menuAberto = !menuAberto" 
          class="w-10 h-10 rounded-full border-2 border-teal-500 overflow-hidden focus:outline-none active:scale-95 transition bg-slate-800 flex items-center justify-center"
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
            <p class="text-[9px] text-slate-400 uppercase font-bold">{{ perfil }}</p>
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

    <main class="flex-grow">
      <slot />
    </main>

    <div class="fixed bottom-4 right-4 z-40 hidden md:block">
      <SpotifyPlayer />
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { Link, usePage } from '@inertiajs/vue3'
import SpotifyPlayer from './SpotifyPlayer.vue'

const page = usePage()
const menuAberto = ref(false)
const tempoConectado = ref('00:00:00')
let cronometro = null

// Captura de forma defensiva os dados que injetamos globais no middleware do C#
const authData = computed(() => page.props.auth || {})
const perfil = computed(() => authData.value.usuarioPerfil || 'Vendedor')
const nomeUsuario = computed(() => authData.value.usuarioNome || 'Colaborador')
const fotoPerfil = computed(() => authData.value.usuarioFoto || null)

onMounted(() => {
  const tempoInicio = Date.now()
  
  // Cronômetro progressivo de tempo de conexão em tempo real
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