<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-6xl mx-auto">
      
      <div>
        <h1 class="text-2xl font-black text-slate-950">Controle de Equipe</h1>
        <p class="text-sm text-slate-500">Gerencie os acessos, transferência entre lojas da rede e taxas individuais de comissão de vendedoras, administradores e donos.</p>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        <!-- Formulário de Cadastro -->
        <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm h-fit space-y-4">
          <h3 class="text-base font-bold text-slate-950">Novo Integrante</h3>
          
          <form @submit.prevent="cadastrarColaborador" class="space-y-4">
            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Nome Completo *</label>
              <input v-model="form.Nome" type="text" placeholder="Ex: Carlos Souza" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>

            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">E-mail Corporativo *</label>
              <input v-model="form.Email" type="email" placeholder="carlos@otica.com" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>

            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Senha de Acesso *</label>
              <input v-model="form.Senha" type="password" placeholder="••••••••" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>

            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Perfil *</label>
                <select v-model="form.Perfil" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500">
                  <option :value="2">Vendedor</option>
                  <option :value="1">Administrador</option>
                  <option v-if="ehDono || ehSistema" :value="4">🏢 Dono (Rede)</option>
                  <option v-if="ehSistema" :value="3">⚡ Sistema (Deus)</option>
                </select>
              </div>

              <!-- Novo Campo: Porcentagem de Comissão Individual -->
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Comissão (%) *</label>
                <input v-model.number="form.PercentualComissao" type="number" step="0.1" min="0" max="100" placeholder="3.0" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono font-bold" required />
              </div>
            </div>

            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Loja / Unidade *</label>
              <select 
                v-if="listaLojas.length > 0" 
                v-model="form.OticaId" 
                @change="atualizarNomeLojaForm"
                class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500"
              >
                <option v-for="loja in listaLojas" :key="loja.id || loja.Id" :value="loja.id || loja.Id">
                  {{ loja.nome || loja.Nome }}
                </option>
              </select>
              <input v-else v-model="form.FilialLoja" type="text" placeholder="Ex: Ótica Matriz" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>

            <button 
              type="submit" 
              :disabled="form.processing"
              class="w-full bg-teal-600 hover:bg-teal-700 disabled:bg-slate-200 disabled:text-slate-400 text-white font-bold py-3 rounded-xl text-xs transition shadow-sm uppercase tracking-wider flex items-center justify-center min-h-[40px]"
            >
              <span v-if="form.processing">Registrando...</span>
              <span v-else>Registrar Colaborador</span>
            </button>
          </form>
        </div>

        <!-- Tabela da Equipe -->
        <div class="lg:col-span-2 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
          <h3 class="text-base font-bold text-slate-950 mb-4">Funcionários Cadastrados</h3>

          <div v-if="!listaEquipe || listaEquipe.length === 0" class="text-center py-12 border-2 border-dashed border-slate-100 rounded-xl text-slate-400 text-sm">
            Nenhum colaborador registrado no sistema.
          </div>

          <div v-else class="overflow-x-auto">
            <table class="w-full text-left text-sm border-collapse">
              <thead>
                <tr class="border-b border-slate-100 text-slate-400 text-xs font-bold uppercase tracking-wider">
                  <th class="pb-3">Colaborador</th>
                  <th class="pb-3">Loja</th>
                  <th class="pb-3 text-center">Perfil</th>
                  <th class="pb-3 text-center">% Comis.</th>
                  <th class="pb-3 text-center">Último Acesso</th>
                  <th class="pb-3 text-center">Status</th>
                  <th class="pb-3 text-center">Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="user in listaEquipe" :key="user.id || user.Id" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
                  <td class="py-4">
                    <p class="font-bold text-slate-800">{{ user.nome || user.Nome }}</p>
                    <p class="text-xs text-slate-400 font-mono">{{ user.email || user.Email }}</p>
                  </td>
                  <td class="py-4 text-xs font-semibold text-slate-600">
                    {{ user.oticaNome || user.OticaNome || user.filialLoja || user.FilialLoja || 'Matriz' }}
                  </td>
                  <td class="py-4 text-center">
                    <span 
                      :class="[
                        (user.perfil === 3 || user.perfilNome === 'Sistema')
                          ? 'bg-purple-100 text-purple-800 border-purple-300 font-black'
                          : (user.perfil === 4 || user.perfilNome === 'Dono')
                            ? 'bg-amber-100 text-amber-900 border-amber-300 font-black'
                            : (user.perfil === 1 || user.perfilNome === 'Administrador' || user.perfilNome === 'Admin')
                              ? 'bg-indigo-50 text-indigo-700 border-indigo-200 font-bold'
                              : 'bg-slate-100 text-slate-700 border-slate-200 font-medium'
                      ]"
                      class="px-2.5 py-0.5 rounded-full text-xs border"
                    >
                      {{ user.perfilNome || user.PerfilNome || (user.perfil === 3 ? 'Sistema' : (user.perfil === 4 ? 'Dono' : (user.perfil === 1 ? 'Admin' : 'Vendedor'))) }}
                    </span>
                  </td>
                  <!-- Coluna do % Individual de Comissão -->
                  <td class="py-4 text-center font-mono text-xs font-black text-teal-700">
                    {{ (user.percentualComissao ?? user.PercentualComissao ?? 3).toFixed(2) }}%
                  </td>
                  <td class="py-4 text-center font-mono text-xs text-slate-500">
                    {{ formatarDataAcesso(user.ultimoAcesso || user.UltimoAcesso) }}
                  </td>
                  <td class="py-4 text-center">
                    <span :class="(user.ativo ?? user.Ativo) ? 'bg-emerald-50 text-emerald-700 border-emerald-100' : 'bg-red-50 text-red-700 border-red-100'" class="px-2.5 py-0.5 rounded-full text-xs font-bold border">
                      {{ (user.ativo ?? user.Ativo) ? 'Ativo' : 'Inativo' }}
                    </span>
                  </td>
                  <td class="py-4 text-center flex items-center justify-center gap-1.5">
                    <button 
                      @click="abrirModalEdicao(user)"
                      class="bg-slate-100 hover:bg-slate-200 text-slate-700 text-xs font-bold px-2.5 py-1.5 rounded-lg transition shadow-sm"
                      title="Editar Colaborador / Transferir Loja"
                    >
                      ✏️ Editar
                    </button>
                    <button 
                      @click="alterarStatusUsuario(user.id || user.Id)"
                      :class="(user.ativo ?? user.Ativo) ? 'bg-slate-950 hover:bg-slate-800' : 'bg-teal-600 hover:bg-teal-700'"
                      class="text-white text-xs font-bold px-2.5 py-1.5 rounded-lg transition shadow-sm whitespace-nowrap"
                    >
                      {{ (user.ativo ?? user.Ativo) ? 'Desativar' : 'Reativar' }}
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

      </div>

      <!-- MODAL DE EDIÇÃO DE COLABORADOR E TRANSFERÊNCIA DE LOJA -->
      <div v-if="modalEdicaoAberta" class="fixed inset-0 bg-slate-950/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
        <div class="bg-white rounded-3xl border border-slate-200 shadow-2xl max-w-md w-full p-6 space-y-5">
          <div class="flex items-center justify-between border-b border-slate-100 pb-3">
            <h3 class="text-base font-bold text-slate-950">Editar Colaborador</h3>
            <button @click="modalEdicaoAberta = false" class="text-slate-400 hover:text-slate-800 font-bold">✕</button>
          </div>

          <form @submit.prevent="salvarEdicao" class="space-y-4 text-xs">
            <div>
              <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1">Nome Completo *</label>
              <input v-model="formEdicao.Nome" type="text" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>

            <div>
              <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1">E-mail Corporativo *</label>
              <input v-model="formEdicao.Email" type="email" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>

            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1">Perfil *</label>
                <select v-model="formEdicao.Perfil" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500">
                  <option :value="2">Vendedor</option>
                  <option :value="1">Administrador</option>
                  <option v-if="ehDono || ehSistema" :value="4">🏢 Dono (Rede)</option>
                  <option v-if="ehSistema" :value="3">⚡ Sistema (Deus)</option>
                </select>
              </div>

              <!-- Edição de Comissão Individual -->
              <div>
                <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1">Comissão (%) *</label>
                <input v-model.number="formEdicao.PercentualComissao" type="number" step="0.1" min="0" max="100" class="w-full rounded-xl border-slate-200 text-sm font-mono font-bold text-teal-700 focus:border-teal-500 focus:ring-teal-500" required />
              </div>
            </div>

            <div>
              <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1">Loja / Transferir de Unidade *</label>
              <select 
                v-if="listaLojas.length > 0" 
                v-model="formEdicao.OticaId" 
                @change="atualizarNomeLojaEdicao"
                class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500"
              >
                <option v-for="loja in listaLojas" :key="loja.id || loja.Id" :value="loja.id || loja.Id">
                  {{ loja.nome || loja.Nome }}
                </option>
              </select>
              <input v-else v-model="formEdicao.FilialLoja" type="text" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>

            <div>
              <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1">Nova Senha (deixe em branco para manter a atual)</label>
              <input v-model="formEdicao.NovaSenha" type="password" placeholder="••••••••" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
            </div>

            <div class="flex items-center gap-2 pt-1">
              <input v-model="formEdicao.Ativo" type="checkbox" id="editAtivo" class="rounded border-slate-300 text-teal-600 focus:ring-teal-500" />
              <label for="editAtivo" class="font-bold text-slate-700">Usuário Ativo no Sistema</label>
            </div>

            <div class="flex justify-end gap-2 pt-3 border-t border-slate-100">
              <button type="button" @click="modalEdicaoAberta = false" class="px-4 py-2 rounded-xl text-slate-600 font-bold hover:bg-slate-100">Cancelar</button>
              <button type="submit" :disabled="formEdicao.processing" class="px-5 py-2 rounded-xl bg-teal-600 hover:bg-teal-700 text-white font-bold transition">Salvar Alterações</button>
            </div>
          </form>
        </div>
      </div>

    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { useForm, router, usePage } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  Equipe: Array,
  equipe: Array,
  Lojas: Array,
  lojas: Array,
  EhSistema: Boolean,
  ehSistema: Boolean,
  EhDono: Boolean,
  ehDono: Boolean
})

const page = usePage()
const ehSistema = computed(() => props.EhSistema ?? props.ehSistema ?? !!page.props.auth?.ehSistema ?? page.props.auth?.usuarioPerfil === 'Sistema')
const ehDono = computed(() => props.EhDono ?? props.ehDono ?? !!page.props.auth?.ehDono ?? page.props.auth?.usuarioPerfil === 'Dono')
const listaEquipe = computed(() => props.Equipe ?? props.equipe ?? [])
const listaLojas = computed(() => props.Lojas ?? props.lojas ?? [])

const modalEdicaoAberta = ref(false)

const form = useForm({
  Nome: '',
  Email: '',
  Senha: '',
  Perfil: 2, // Padrão: Vendedor (2)
  OticaId: null,
  FilialLoja: 'Ótica Matriz',
  PercentualComissao: 3.00
})

const formEdicao = useForm({
  id: null,
  Nome: '',
  Email: '',
  OticaId: null,
  FilialLoja: '',
  Perfil: 2,
  Ativo: true,
  PercentualComissao: 3.00,
  NovaSenha: ''
})

// Inicializa loja padrão no formulário de cadastro
watch(listaLojas, (lojas) => {
  if (lojas && lojas.length > 0 && !form.OticaId) {
    form.OticaId = lojas[0].id || lojas[0].Id
    form.FilialLoja = lojas[0].nome || lojas[0].Nome
  }
}, { immediate: true })

const atualizarNomeLojaForm = () => {
  const loja = listaLojas.value.find(l => (l.id || l.Id) === form.OticaId)
  if (loja) {
    form.FilialLoja = loja.nome || loja.Nome
  }
}

const atualizarNomeLojaEdicao = () => {
  const loja = listaLojas.value.find(l => (l.id || l.Id) === formEdicao.OticaId)
  if (loja) {
    formEdicao.FilialLoja = loja.nome || loja.Nome
  }
}

const cadastrarColaborador = () => {
  form.post('/equipe', {
    preserveScroll: true,
    onSuccess: () => {
      form.reset('Senha')
      form.Nome = ''
      form.Email = ''
      form.PercentualComissao = 3.00
    }
  })
}

const abrirModalEdicao = (user) => {
  formEdicao.id = user.id || user.Id
  formEdicao.Nome = user.nome || user.Nome || ''
  formEdicao.Email = user.email || user.Email || ''
  formEdicao.OticaId = user.oticaId || user.OticaId || null
  formEdicao.FilialLoja = user.oticaNome || user.OticaNome || user.filialLoja || user.FilialLoja || 'Ótica Matriz'
  formEdicao.Perfil = user.perfil ?? user.Perfil ?? 2
  formEdicao.Ativo = user.ativo ?? user.Ativo ?? true
  formEdicao.PercentualComissao = user.percentualComissao ?? user.PercentualComissao ?? 3.00
  formEdicao.NovaSenha = ''
  modalEdicaoAberta.value = true
}

const salvarEdicao = () => {
  formEdicao.post(`/equipe/editar/${formEdicao.id}`, {
    preserveScroll: true,
    onSuccess: () => {
      modalEdicaoAberta.value = false
    }
  })
}

const alterarStatusUsuario = (id) => {
  if (!id) return
  router.post(`/equipe/alternar-status/${id}`, {}, { preserveScroll: true })
}

const formatarDataAcesso = (dataRaw) => {
  if (!dataRaw) return 'Nunca acessou'
  const data = new Date(dataRaw)
  return data.toLocaleDateString('pt-BR') + ' às ' + data.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })
}
</script>