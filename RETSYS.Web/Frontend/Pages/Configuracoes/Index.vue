<template>
  <!-- Injeta a moldura unificada com o Header e o Timer automáticos -->
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-5xl mx-auto">
      
      <!-- Cabeçalho de Título -->
      <div>
        <h1 class="text-2xl font-black text-slate-950">Configurações do Sistema</h1>
        <p class="text-sm text-slate-500">Gerencie os dados institucionais, integração WhatsApp de atendimento e chaves de pagamento.</p>
      </div>

      <div class="space-y-6">
        
        <!-- Formulário de Parâmetros Básicos, PIX e WhatsApp -->
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

          <!-- INTEGRAÇÃO WHATSAPP & MENSAGENS AUTOMÁTICAS -->
          <div class="bg-white p-6 rounded-2xl border border-emerald-200 shadow-sm space-y-6">
            <div class="flex items-center justify-between border-b border-slate-100 pb-4">
              <div class="flex items-center gap-3">
                <span class="w-10 h-10 rounded-xl bg-emerald-50 text-emerald-600 flex items-center justify-center text-xl font-bold">
                  💬
                </span>
                <div>
                  <h3 class="text-base font-bold text-slate-900">Integração WhatsApp & Mensagens aos Clientes</h3>
                  <p class="text-xs text-slate-500">Configure o canal de contato da loja e personalize as mensagens automáticas de cadastro e retirada da OS.</p>
                </div>
              </div>
              <span :class="form.WhatsappNumero ? 'bg-emerald-50 text-emerald-700 border-emerald-200' : 'bg-slate-100 text-slate-500 border-slate-200'" class="px-2.5 py-1 rounded-lg text-[10px] font-black border uppercase font-mono">
                {{ form.WhatsappNumero ? '📱 WhatsApp Ativo' : 'Não Configurado' }}
              </span>
            </div>

            <!-- Número Oficial do WhatsApp da Ótica -->
            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-500 tracking-wider mb-1.5">
                Número de WhatsApp Oficial da Loja / Balcão (DDD + Número)
              </label>
              <input 
                v-model="form.WhatsappNumero" 
                type="text" 
                placeholder="(11) 99999-9999" 
                class="w-full max-w-md rounded-xl border-slate-200 text-sm focus:border-emerald-500 focus:ring-emerald-500 font-mono" 
              />
              <p class="text-[11px] text-slate-400 mt-1">Este número será associado como o remetente oficial da ótica para contatos e atendimentos.</p>
            </div>

            <!-- Tags disponíveis para templates -->
            <div class="bg-slate-50 border border-slate-200 rounded-xl p-4">
              <p class="text-[11px] font-bold uppercase text-slate-600 tracking-wider mb-2">🏷️ Variáveis Dinâmicas Disponíveis nos Textos:</p>
              <div class="flex flex-wrap gap-2 text-xs font-mono">
                <span class="bg-white border border-slate-200 px-2 py-1 rounded text-teal-700 font-bold">{cliente}</span>
                <span class="bg-white border border-slate-200 px-2 py-1 rounded text-teal-700 font-bold">{numero_os}</span>
                <span class="bg-white border border-slate-200 px-2 py-1 rounded text-teal-700 font-bold">{otica}</span>
                <span class="bg-white border border-slate-200 px-2 py-1 rounded text-teal-700 font-bold">{resumo_pedido}</span>
                <span class="bg-white border border-slate-200 px-2 py-1 rounded text-teal-700 font-bold">{previsao_entrega}</span>
                <span class="bg-white border border-slate-200 px-2 py-1 rounded text-emerald-700 font-bold">{status_pagamento}</span>
                <span class="bg-white border border-slate-200 px-2 py-1 rounded text-emerald-700 font-bold">{saldo_devedor}</span>
              </div>
            </div>

            <!-- Template 1: Mensagem de Boas-Vindas / Cadastro da OS -->
            <div class="space-y-2">
              <div class="flex items-center justify-between">
                <label class="block text-xs font-bold text-slate-800 uppercase tracking-wider">
                  📝 Mensagem 1: Confirmação de Cadastro da OS (Boas-vindas e Resumo)
                </label>
                <button 
                  type="button" 
                  @click="restaurarTemplateCadastro" 
                  class="text-[11px] text-teal-600 hover:text-teal-800 font-bold transition flex items-center gap-1"
                >
                  🔄 Restaurar Padrão
                </button>
              </div>
              <textarea 
                v-model="form.WhatsappMsgCadastroTemplate" 
                rows="6" 
                class="w-full rounded-xl border-slate-200 text-xs focus:border-emerald-500 focus:ring-emerald-500 font-mono leading-relaxed"
                placeholder="Digite o texto da mensagem de confirmação de cadastro..."
              ></textarea>
            </div>

            <!-- Template 2: Mensagem de OS Pronta para Retirada -->
            <div class="space-y-2">
              <div class="flex items-center justify-between">
                <label class="block text-xs font-bold text-slate-800 uppercase tracking-wider">
                  👓 Mensagem 2: Notificação de Óculos Pronto para Retirada (+ Status Financeiro)
                </label>
                <button 
                  type="button" 
                  @click="restaurarTemplatePronto" 
                  class="text-[11px] text-teal-600 hover:text-teal-800 font-bold transition flex items-center gap-1"
                >
                  🔄 Restaurar Padrão
                </button>
              </div>
              <textarea 
                v-model="form.WhatsappMsgProntoTemplate" 
                rows="6" 
                class="w-full rounded-xl border-slate-200 text-xs focus:border-emerald-500 focus:ring-emerald-500 font-mono leading-relaxed"
                placeholder="Digite o texto da mensagem de óculos pronto..."
              ></textarea>
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

        <!-- Bloco de Sonorização Ambiente -->
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

const props = defineProps({
  nomeLoja: String,
  NomeLoja: String,
  cnpj: String,
  Cnpj: String,
  pixApiKey: String,
  PixApiKey: String,
  whatsappNumero: String,
  WhatsappNumero: String,
  whatsappMsgCadastroTemplate: String,
  WhatsappMsgCadastroTemplate: String,
  whatsappMsgProntoTemplate: String,
  WhatsappMsgProntoTemplate: String,
  templatePadraoCadastro: String,
  TemplatePadraoCadastro: String,
  templatePadraoPronto: String,
  TemplatePadraoPronto: String
})

const padraoCadastro = props.TemplatePadraoCadastro ?? props.templatePadraoCadastro ?? ''
const padraoPronto = props.TemplatePadraoPronto ?? props.templatePadraoPronto ?? ''

const form = useForm({
  NomeLoja: props.NomeLoja ?? props.nomeLoja ?? '',
  Cnpj: props.Cnpj ?? props.cnpj ?? '',
  PixApiKey: props.PixApiKey ?? props.pixApiKey ?? '',
  WhatsappNumero: props.WhatsappNumero ?? props.whatsappNumero ?? '',
  WhatsappMsgCadastroTemplate: props.WhatsappMsgCadastroTemplate ?? props.whatsappMsgCadastroTemplate ?? padraoCadastro,
  WhatsappMsgProntoTemplate: props.WhatsappMsgProntoTemplate ?? props.whatsappMsgProntoTemplate ?? padraoPronto
})

const restaurarTemplateCadastro = () => {
  if (confirm('Deseja restaurar o modelo padrão da mensagem de cadastro?')) {
    form.WhatsappMsgCadastroTemplate = padraoCadastro
  }
}

const restaurarTemplatePronto = () => {
  if (confirm('Deseja restaurar o modelo padrão da mensagem de OS pronta?')) {
    form.WhatsappMsgProntoTemplate = padraoPronto
  }
}

const salvarConfiguracoes = () => {
  form.post('/configuracoes', {
    preserveScroll: true,
    onSuccess: () => {
      alert('Parâmetros, WhatsApp e modelos de mensagens salvos com sucesso!')
    }
  })
}
</script>