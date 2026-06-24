<template>
  <div class="bg-white border border-slate-200 rounded-2xl p-4 shadow-sm max-w-sm w-full transition-all duration-300">
    
    <div v-if="!estaConectado" class="flex items-center justify-between gap-3">
      <div class="flex items-center gap-2.5">
        <span class="text-xl">🎵</span>
        <div>
          <h4 class="text-xs font-bold text-slate-800">Som da Loja Desativado</h4>
          <p class="text-[10px] text-slate-400 leading-tight">Conecte uma conta Spotify Premium nas configurações.</p>
        </div>
      </div>
      <Link 
        v-if="eAdmin"
        href="/configuracoes" 
        class="text-[10px] font-bold text-teal-600 bg-teal-50 hover:bg-teal-100 border border-teal-100 px-2.5 py-1.5 rounded-lg transition shrink-0 uppercase tracking-wider"
      >
        Conectar
      </Link>
    </div>

    <div v-else class="space-y-3">
      
      <div class="flex items-center gap-3">
        <div class="w-12 h-12 rounded-lg bg-slate-100 border border-slate-200 overflow-hidden shrink-0 flex items-center justify-center relative group">
          <img 
            v-if="musicaAtual.capaUrl" 
            :src="musicaAtual.capaUrl" 
            alt="Capa do Álbum" 
            class="w-full h-full object-cover" 
          />
          <span v-else class="text-lg">📻</span>
        </div>
        
        <div class="overflow-hidden flex-1">
          <p class="text-xs font-bold text-slate-800 truncate" :title="musicaAtual.titulo">
            {{ musicaAtual.titulo || 'Nenhuma faixa tocando' }}
          </p>
          <p class="text-[10px] text-slate-400 truncate">
            {{ musicaAtual.artista || 'Abra o Spotify para sintonizar' }}
          </p>
        </div>

        <span class="w-2 h-2 rounded-full bg-emerald-500 animate-pulse shrink-0" title="Sincronizado com a loja"></span>
      </div>

      <div class="flex items-center justify-center gap-4 bg-slate-50 py-1.5 px-3 rounded-xl border border-slate-100">
        <button 
          @click="controlarMidia('anterior')" 
          class="text-slate-500 hover:text-slate-800 transition active:scale-90 text-sm font-bold"
          title="Música Anterior"
        >
          ⏮️
        </button>
        
        <button 
          @click="alternarPlayPause" 
          class="w-8 h-8 rounded-full bg-slate-950 hover:bg-slate-800 text-white flex items-center justify-center transition active:scale-90 text-xs shadow-sm"
          :title="musicaAtual.tocando ? 'Pausar' : 'Tocar'"
        >
          {{ musicaAtual.tocando ? '⏸️' : '▶️' }}
        </button>
        
        <button 
          @click="controlarMidia('proxima')" 
          class="text-slate-500 hover:text-slate-800 transition active:scale-90 text-sm font-bold"
          title="Próxima Música"
        >
          ⏭️
        </button>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { Link, usePage } from '@inertiajs/vue3'

const page = usePage()

// Avalia de forma reativa os dados globais de autorização do ecossistema
const eAdmin = computed(() => page.props.auth?.usuarioPerfil === 'Admin')
const estaConectado = computed(() => !!page.props.auth?.spotifyTokenAtivo)

const intervaloStatus = ref(null)
const musicaAtual = ref({
  titulo: '',
  artista: '',
  capaUrl: '',
  tocando: false
})

// 1. Polling de Atualização da Faixa (Sincroniza o que está tocando em segundo plano)
const buscarStatusReproducao = async () => {
  if (!estaConectado.value) return

  try {
    const resposta = await fetch('/api/spotify/status-atual')
    if (resposta.ok) {
      const dados = await resposta.json()
      musicaAtual.value.titulo = dados.titulo || dados.Titulo || ''
      musicaAtual.value.artista = dados.artista || dados.Artista || ''
      musicaAtual.value.capaUrl = dados.capaUrl || dados.CapaUrl || ''
      musicaAtual.value.tocando = dados.tocando ?? dados.Tocando ?? false
    }
  } catch (err) {
    console.error("Falha silenciosa ao sincronizar faixa do Spotify:", err)
  }
}

// 2. Dispara Comandos de Mídia para o C# repassar para a API do Spotify
const controlarMidia = async (acao) => {
  try {
    await fetch(`/api/spotify/controlar?comando=${acao}`, { method: 'POST' })
    // Força uma atualização visual imediata logo após o comando
    setTimeout(buscarStatusReproducao, 300)
  } catch (err) {
    console.error(`Erro ao disparar comando [${acao}]:`, err)
  }
}

const alternarPlayPause = () => {
  controlarMidia(musicaAtual.value.tocando ? 'pausar' : 'tocar')
}

// Ciclo de vida isolado: Atualiza o status a cada 5 segundos se houver login ativo
onMounted(() => {
  if (estaConectado.value) {
    buscarStatusReproducao()
    intervaloStatus.value = setInterval(buscarStatusReproducao, 5000)
  }
})

onUnmounted(() => {
  if (intervaloStatus.value) clearInterval(intervaloStatus.value)
})
</script>