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
            {{ musicaAtual.titulo || (playerLocalPronto ? 'Som da Ótica Conectado' : 'Nenhuma faixa tocando') }}
          </p>
          <p class="text-[10px] text-slate-400 truncate">
            {{ musicaAtual.artista || (playerLocalPronto ? 'Pronto para tocar nesta aba' : 'Abra o Spotify para sintonizar') }}
          </p>
        </div>

        <span 
          class="w-2 h-2 rounded-full shrink-0 transition-colors"
          :class="playerLocalPronto ? 'bg-emerald-500 animate-pulse' : 'bg-amber-400'"
          :title="playerLocalPronto ? 'Reprodutor de Áudio Ativo no Navegador' : 'Conectando Reprodutor Local...'"
        ></span>
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

const eAdmin = computed(() => page.props.auth?.usuarioPerfil === 'Admin')
const estaConectado = computed(() => !!page.props.auth?.spotifyTokenAtivo)

const intervaloStatus = ref(null)
const playerLocalPronto = ref(false)
const localDeviceId = ref(null)
let playerInstance = null

const musicaAtual = ref({
  titulo: '',
  artista: '',
  capaUrl: '',
  tocando: false
})

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

const carregarEInicializarSDK = () => {
  if (!estaConectado.value) return

  const scriptExistente = document.getElementById('spotify-player-sdk')
  if (!scriptExistente) {
    const script = document.createElement('script')
    script.id = 'spotify-player-sdk'
    script.src = 'https://sdk.scdn.co/spotify-player.js'
    script.async = true
    document.body.appendChild(script)
  }

  window.onSpotifyWebPlaybackSDKReady = () => {
    playerInstance = new window.Spotify.Player({
      name: 'RETSYS Som da Ótica',
      getOAuthToken: async (cb) => {
        try {
          const res = await fetch('/api/spotify/status-atual')
          if (res.ok) {
            const data = await res.json()
            const tokenLimpo = data.token || data.Token
            if (tokenLimpo) {
              cb(tokenLimpo)
              return
            }
          }
        } catch (e) {
          console.error("Erro ao obter token do Spotify:", e)
        }
        cb('')
      },
      volume: 0.8
    })

    playerInstance.addListener('ready', ({ device_id }) => {
      localDeviceId.value = device_id
      playerLocalPronto.value = true
      console.log('[RETSYS Spotify Player] Dispositivo local registrado! ID:', device_id)
    })

    playerInstance.addListener('player_state_changed', (state) => {
      if (!state) return
      const currentTrack = state.track_window.current_track
      if (currentTrack) {
        musicaAtual.value.titulo = currentTrack.name
        musicaAtual.value.artista = currentTrack.artists.map(a => a.name).join(', ')
        musicaAtual.value.capaUrl = currentTrack.album.images[0]?.url || ''
        musicaAtual.value.tocando = !state.paused
      }
    })

    playerInstance.connect()
  }
}

const controlarMidia = async (acao) => {
  if (playerInstance && playerLocalPronto.value) {
    if (acao === 'anterior') playerInstance.previousTrack()
    else if (acao === 'proxima') playerInstance.nextTrack()
    else if (acao === 'tocar' || acao === 'pausar') playerInstance.togglePlay()
  }

  try {
    const devIdParam = localDeviceId.value ? `&deviceId=${localDeviceId.value}` : ''
    await fetch(`/api/spotify/controlar?comando=${acao}${devIdParam}`, { method: 'POST' })
    setTimeout(buscarStatusReproducao, 300)
  } catch (err) {
    console.error(`Erro ao disparar comando [${acao}]:`, err)
  }
}

const alternarPlayPause = () => {
  controlarMidia(musicaAtual.value.tocando ? 'pausar' : 'tocar')
}

onMounted(() => {
  if (estaConectado.value) {
    buscarStatusReproducao()
    carregarEInicializarSDK()
    intervaloStatus.value = setInterval(buscarStatusReproducao, 5000)
  }
})

onUnmounted(() => {
  if (intervaloStatus.value) clearInterval(intervaloStatus.value)
  if (playerInstance) playerInstance.disconnect()
})
</script>