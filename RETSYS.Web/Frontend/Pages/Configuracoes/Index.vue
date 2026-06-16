<template>
  <div class="min-h-screen bg-slate-50 p-4 md:p-8 font-sans text-slate-900">
    <div class="max-w-4xl mx-auto space-y-6">
      
      <div>
        <h1 class="text-2xl font-black text-slate-950">Configurações do Sistema</h1>
        <p class="text-sm text-slate-500">Gerencie os dados institucionais da empresa e chaves de integração de pagamento.</p>
      </div>

      <form @submit.prevent="salvarConfiguracoes" class="space-y-6">
        
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

        <div class="flex justify-end">
          <button 
            type="submit" 
            class="bg-slate-950 hover:bg-slate-800 text-white font-bold py-3 px-8 rounded-xl text-xs transition shadow-sm uppercase tracking-wider active:scale-95"
          >
            Salvar Parâmetros
          </button>
        </div>

      </form>

    </div>
  </div>
</template>

<script setup>
import { reactive } from 'vue'
import { router } from '@inertiajs/vue3'

// Recebe os dados atuais armazenados no estado do servidor C#
const props = defineProps({
  nomeLoja: String,
  cnpj: String,
  pixApiKey: String
})

const form = reactive({
  NomeLoja: props.nomeLoja,
  Cnpj: props.cnpj,
  PixApiKey: props.pixApiKey
})

const salvarConfiguracoes = () => {
  // Despacha as configurações de parâmetros para persistência
  router.post('/configuracoes', form, {
    onSuccess: () => {
      alert('Parâmetros e chaves de API da Ótica salvos com sucesso!')
    }
  })
}
</script>