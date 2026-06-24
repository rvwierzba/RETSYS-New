<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-3xl mx-auto">
      
      <div>
        <h1 class="text-2xl font-black text-slate-950">Minha Conta</h1>
        <p class="text-sm text-slate-500">Gerencie suas informações de acesso, altere sua senha e atualize sua foto de perfil.</p>
      </div>

      <div class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
        <form @submit.prevent="atualizarPerfil" class="p-6 space-y-6">
          
          <div class="flex flex-col sm:flex-row items-center gap-6 bg-slate-50 p-4 rounded-xl border border-slate-100">
            <div class="relative w-24 h-24 rounded-full border-4 border-teal-500 overflow-hidden bg-slate-200 shrink-0 flex items-center justify-center">
              <img 
                v-if="urlPreview || form.FotoAntiga" 
                :src="urlPreview || form.FotoAntiga" 
                alt="Avatar" 
                class="w-full h-full object-cover" 
              />
              <span v-else class="text-2xl font-black text-teal-600 uppercase">
                {{ form.Nome?.substring(0, 2) }}
              </span>
            </div>

            <div class="space-y-2 text-center sm:text-left">
              <h4 class="text-sm font-bold text-slate-800">Foto de Identificação</h4>
              <p class="text-xs text-slate-400 leading-tight">Carregue uma imagem clara. Ela aparecerá no topo do terminal e nos relatórios de desempenho.</p>
              
              <div class="flex items-center justify-center sm:justify-start gap-2 pt-1">
                <input type="file" id="uploadFoto" accept="image/*" @change="processarFoto" class="hidden" />
                <label for="uploadFoto" class="bg-slate-950 hover:bg-slate-800 text-white text-xs font-bold px-3 py-2 rounded-lg transition cursor-pointer select-none active:scale-95">
                  Selecionar Nova Foto
                </label>
                <button v-if="urlPreview" type="button" @click="cancelarNovaFoto" class="text-xs font-semibold text-red-600 hover:underline px-2 py-1">
                  Cancelar
                </button>
              </div>
            </div>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Nome Completo *</label>
              <input v-model="form.Nome" type="text" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>

            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">E-mail de Acesso (Login)</label>
              <input :value="form.Email" type="email" class="w-full rounded-xl border-slate-200 text-sm bg-slate-50 text-slate-400 cursor-not-allowed font-mono" readonly />
            </div>
          </div>

          <div class="border-t border-slate-100 pt-4 space-y-4">
            <h3 class="text-xs font-bold uppercase tracking-wider text-teal-600">Alterar Senha de Acesso</h3>
            <p class="text-[11px] text-slate-400 leading-tight">Preencha os campos abaixo apenas se desejar modificar sua senha atual. Caso contrário, deixe em branco.</p>
            
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Nova Senha</label>
                <input v-model="form.NovaSenha" type="password" placeholder="Mínimo 6 caracteres" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Confirme a Nova Senha</label>
                <input v-model="form.ConfirmarSenha" type="password" placeholder="Repita a nova senha" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" />
              </div>
            </div>
          </div>

          <div class="flex items-center justify-end gap-3 border-t border-slate-100 pt-4">
            <button 
              type="submit" 
              :disabled="form.processing"
              class="bg-teal-600 hover:bg-teal-700 disabled:bg-slate-200 disabled:text-slate-400 text-white font-bold py-3 px-8 rounded-xl text-xs uppercase tracking-wider transition shadow-sm min-w-[160px] flex items-center justify-center"
            >
              <span v-if="form.processing">Atualizando...</span>
              <span v-else>Salvar Alterações</span>
            </button>
          </div>

        </form>
      </div>

    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { ref } from 'vue'
import { useForm } from '@inertiajs/vue3'
import AuthenticatedLayout from '../Shared/AuthenticatedLayout.vue'

// O controller do C# deve injetar os dados do usuário atual nesta rota
const props = defineProps({
  Colaborador: Object,
  colaborador: Object
})

const dadosIniciais = props.Colaborador ?? props.colaborador ?? {}

const urlPreview = ref(null)

// Utiliza o useForm preparado para upload de arquivos (multipart/form-data automático)
const form = useForm({
  Nome: dadosIniciais.nome || dadosIniciais.Nome || '',
  Email: dadosIniciais.email || dadosIniciais.Email || '',
  FotoAntiga: dadosIniciais.fotoUrl || dadosIniciais.FotoUrl || null,
  FotoNova: null,
  NovaSenha: '',
  ConfirmarSenha: ''
})

// Processa o arquivo de imagem selecionado e gera o preview local em tempo de execução
const processarFoto = (event) => {
  const arquivos = event.target.files
  if (arquivos.length > 0) {
    form.FotoNova = arquivos[0]
    urlPreview.value = URL.createObjectURL(arquivos[0])
  }
}

const cancelarNovaFoto = () => {
  form.FotoNova = null
  if (urlPreview.value) {
    URL.revokeObjectURL(urlPreview.value)
    urlPreview.value = null
  }
}

const atualizarPerfil = () => {
  // Validação preventiva de senha casada antes do disparo HTTP
  if (form.NovaSenha && form.NovaSenha !== form.ConfirmarSenha) {
    alert('A confirmação de senha não confere com a nova senha digitada!')
    return
  }

  // Despacha os dados via POST (padrão seguro para upload de arquivos no .NET)
  form.post('/perfil', {
    preserveScroll: true,
    onSuccess: () => {
      form.NovaSenha = ''
      form.ConfirmarSenha = ''
      cancelarNovaFoto()
      alert('Seu perfil foi atualizado com sucesso em todo o ecossistema!')
    }
  })
}
</script>