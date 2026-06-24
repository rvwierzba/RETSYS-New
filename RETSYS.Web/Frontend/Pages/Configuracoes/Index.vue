<template>
  <!-- Injeta a moldura unificada com o Header e o Timer automáticos -->
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-4xl mx-auto">
      
      <!-- Cabeçalho de Título -->
      <div>
        <h1 class="text-2xl font-black text-slate-950">Configurações do Sistema</h1>
        <p class="text-sm text-slate-500">Gerencie os dados institucionais da empresa e chaves de integração de pagamento.</p>
      </div>

      <div class="space-y-6">
        
        <!-- Formulário de Parâmetros Básicos e PIX -->
        <form @submit.prevent="salvarConfiguracoes" class="space-y-6">
          
          <!-- Identificação da Empresa -->
          <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
            <h3 class="text-sm font-bold uppercase tracking-wider text-slate-400">Identificação da Empresa</h3>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Razão Social / Nome Fantasia *</label>
                <input v-model="form.NomeLoja" type="text" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">CNPJ Estabelecimento *</label>
                <input v-model="form.Cnpj" type="text" placeholder="00.000.000/0001-00" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" required />
              </div>
            </div>
          </div>

          <!-- Integração Gateway PIX -->
          <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
            <div class="flex items-center justify-between">
              <h3 class="text-sm font-bold uppercase tracking-wider text-slate-400">Gateway de Pagamentos (PIX API)</h3>
              <span :class="form.PixApiKey ? 'bg-emerald-50 text-emerald-700 border-emerald-100' : 'bg-slate-100 text-slate-500 border-slate-200'" class="px-2 py-0.5 rounded text-[10px] font-bold border font-sans uppercase">
                {{ form.PixApiKey ? 'Conectado' : 'Modo Manual' }}
              </span>
            </div>

            <div class="space-y-2">
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider">Chave de Autorização OpenPix (App Token)</label>
              <input 
                v-model="form.PixApiKey" 
                type="password" 
                placeholder="Cole aqui o seu token de produção (ex: live_...) ou de sandbox (ex: tests_...)" 
                class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono placeholder:text-slate-300" 
              />
              <p class="text-[11px] text-slate-400 leading-relaxed">
                Deixe este campo em branco para operar o terminal de checkout em modo manual tradicional (Dinheiro/Maquininha física). Ao inserir um token válido, a geração de QR Code dinâmico será ativada automaticamente no caixa.
              </p>
            </div>
          </div>

          <!-- Botão de Gravação do Formulário Principal -->
          <div class="flex justify-end">
            <button 
              type="submit" 
              :disabled="form.processing"
              class="bg-slate-950 hover:bg-slate-800 disabled:bg-slate-200 disabled:text-slate-400 text-white font-bold py-3 px-8 rounded-xl text-xs transition shadow-sm uppercase tracking-wider min-w-[180px]"
            >
              <span v-if="form.processing">Salvando...</span>
              <span v-else>Salvar Parâmetros</span>
            </button>
          </div>
        </form>

        <!-- 🔥 ADICIONADO: Bloco de Sonorização Ambiente (Separado do Form para evitar conflito de Submissão) -->
        <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm space-y-4">
          <div class="flex items-center justify-between">
            <h3 class="text-sm font-bold uppercase tracking-wider text-slate-400">Sonorização Ambiente (Spotify Loja)</h3>
            <span :class="$page.props.auth?.spotifyTokenAtivo ? 'bg-emerald-50 text-emerald-700 border-emerald-100' : 'bg-slate-100 text-slate-500 border-slate-200'" class="px-2 py-0.5 rounded text-[10px] font-bold border uppercase">
              {{ $page.props.auth?.spotifyTokenAtivo ? 'Sincronizado' : 'Desconectado' }}
            </span>
          </div>
          <p class="text-[11px] text-slate-400 leading-relaxed">
            Vincule a conta corporativa Premium da óptica para liberar o controle integrado de faixas musicais diretamente no player flutuante de todas as estações de atendimento.
          </p>
          <div class="pt-2">
            <a 
              href="/api/spotify/login" 
              class="inline-flex items-center justify-center bg-teal-600 hover:bg-teal-700 text-white font-bold py-2.5 px-6 rounded-xl text-xs uppercase tracking-wider transition shadow-sm active:scale-95"
            >
              {{ $page.props.auth?.spotifyTokenAtivo ? '🔄 Reautenticar Conta Spotify' : '🎵 Sincronizar com Spotify Premium' }}
            </a>
          </div>
        </div>

      </div>
    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { useForm } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

// Suporte formal para as duas variações de fontes de dados do C#
const props = defineProps({
  nomeLoja: String,
  NomeLoja: String,
  cnpj: String,
  Cnpj: String,
  pixApiKey: String,
  PixApiKey: String
})

// Acoplado ao useForm com fallbacks defensivos contra chaves nulas ou com case diferente
const form = useForm({
  NomeLoja: props.NomeLoja ?? props.nomeLoja ?? '',
  Cnpj: props.Cnpj ?? props.cnpj ?? '',
  PixApiKey: props.PixApiKey ?? props.pixApiKey ?? ''
})

const salvarConfiguracoes = () => {
  form.post('/configuracoes', {
    preserveScroll: true,
    onSuccess: () => {
      alert('Parâmetros e chaves de API da Ótica salvos com sucesso!')
    }
  })
}
</script>