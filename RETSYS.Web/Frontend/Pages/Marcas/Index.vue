<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-6xl mx-auto">
      
      <!-- Cabeçalho -->
      <div>
        <h1 class="text-2xl font-black text-slate-950">Gerenciamento de Marcas</h1>
        <p class="text-sm text-slate-500">Cadastre, edite e organize as marcas de armações disponíveis no estoque.</p>
      </div>

      <!-- Alerta de Erro -->
      <div v-if="$page.props.erro || $page.props.flash?.erro" class="p-4 rounded-xl bg-red-50 border border-red-200 text-red-700 text-sm font-medium">
        {{ $page.props.erro || $page.props.flash?.erro }}
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        <!-- Formulário de Cadastro / Edição -->
        <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm h-fit space-y-4">
          <div class="flex items-center justify-between">
            <h3 class="text-base font-bold text-slate-900">
              {{ modoEdicao ? 'Editar Marca' : 'Nova Marca' }}
            </h3>
            <button 
              v-if="modoEdicao" 
              @click="cancelarEdicao" 
              class="text-xs text-slate-400 hover:text-slate-600 font-semibold underline"
            >
              Cancelar
            </button>
          </div>
          
          <form @submit.prevent="submeterFormulario" class="space-y-4">
            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Nome da Marca *</label>
              <input 
                v-model="form.Nome" 
                type="text" 
                placeholder="Ex: Ray-Ban, Oakley" 
                class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 placeholder:text-slate-300"
                required 
              />
            </div>

            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Descrição / Notas</label>
              <textarea 
                v-model="form.Descricao" 
                rows="3" 
                placeholder="Notas sobre o fabricante..." 
                class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 placeholder:text-slate-300"
              ></textarea>
            </div>

            <div v-if="modoEdicao" class="flex items-center gap-2 pt-1">
              <input 
                v-model="form.Ativo" 
                type="checkbox" 
                id="ativoCheckbox" 
                class="rounded border-slate-300 text-teal-600 focus:ring-teal-500" 
              />
              <label for="ativoCheckbox" class="text-xs font-bold text-slate-700">Marca Ativa</label>
            </div>

            <button 
              type="submit" 
              :disabled="form.processing"
              class="w-full bg-teal-600 hover:bg-teal-700 disabled:bg-slate-200 disabled:text-slate-400 text-white font-bold py-3 rounded-xl shadow-sm transition text-xs uppercase tracking-wider flex items-center justify-center min-h-[42px]"
            >
              <span v-if="form.processing">{{ modoEdicao ? 'Atualizando...' : 'Salvando...' }}</span>
              <span v-else>{{ modoEdicao ? 'Atualizar Marca' : 'Salvar Marca' }}</span>
            </button>
          </form>
        </div>

        <!-- Tabela de Marcas Cadastradas -->
        <div class="lg:col-span-2 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
          <h3 class="text-base font-bold text-slate-900 mb-4">Marcas Cadastradas</h3>

          <div v-if="!(Marcas ?? marcas) || (Marcas ?? marcas).length === 0" class="text-center py-12 border-2 border-dashed border-slate-100 rounded-xl">
            <p class="text-slate-400 text-sm font-medium">Nenhuma marca cadastrada no sistema ainda.</p>
          </div>

          <div v-else class="overflow-x-auto">
            <table class="w-full text-left border-collapse">
              <thead>
                <tr class="border-b border-slate-100 text-slate-400 text-xs font-bold uppercase tracking-wider">
                  <th class="pb-3">Nome</th>
                  <th class="pb-3">Descrição</th>
                  <th class="pb-3 text-center">Armações</th>
                  <th class="pb-3 text-center">Status</th>
                  <th class="pb-3 text-center">Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="marca in (Marcas ?? marcas)" :key="marca.id || marca.Id" class="border-b border-slate-50 hover:bg-slate-50/60 transition text-sm">
                  <td class="py-4 font-bold text-slate-800">{{ marca.nome || marca.Nome }}</td>
                  <td class="py-4 text-slate-500 max-w-xs truncate">{{ marca.descricao || marca.Descricao || '---' }}</td>
                  <td class="py-4 text-center">
                    <span class="px-2 py-0.5 rounded-full text-xs font-bold bg-slate-100 text-slate-600 border border-slate-200">
                      {{ marca.totalArmacoes ?? marca.TotalArmacoes ?? 0 }}
                    </span>
                  </td>
                  <td class="py-4 text-center">
                    <span :class="(marca.ativo ?? marca.Ativo) ? 'bg-emerald-50 text-emerald-700 border-emerald-100' : 'bg-red-50 text-red-700 border-red-100'" class="px-2.5 py-0.5 rounded-full text-xs font-bold border">
                      {{ (marca.ativo ?? marca.Ativo) ? 'Ativo' : 'Inativo' }}
                    </span>
                  </td>
                  <td class="py-4 text-center">
                    <div class="flex items-center justify-center gap-1.5">
                      <button 
                        @click="prepararEdicao(marca)"
                        class="px-2.5 py-1 text-xs font-bold text-slate-700 bg-slate-100 hover:bg-slate-200 rounded-lg transition"
                      >
                        Editar
                      </button>
                      
                      <button 
                        @click="alterarStatus(marca.id || marca.Id)"
                        :class="(marca.ativo ?? marca.Ativo) ? 'bg-amber-500 hover:bg-amber-600' : 'bg-teal-600 hover:bg-teal-700'"
                        class="px-2.5 py-1 text-xs font-bold text-white rounded-lg transition"
                      >
                        {{ (marca.ativo ?? marca.Ativo) ? 'Inativar' : 'Ativar' }}
                      </button>

                      <button 
                        @click="excluirMarca(marca.id || marca.Id)"
                        class="px-2.5 py-1 text-xs font-bold text-red-600 hover:bg-red-50 rounded-lg transition"
                      >
                        Excluir
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

      </div>
    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { ref } from 'vue'
import { useForm, router } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

defineProps({
  Marcas: Array,
  marcas: Array
})

const modoEdicao = ref(false)
const marcaIdEdicao = ref(null)

const form = useForm({
  Nome: '',
  Descricao: '',
  Ativo: true
})

const submeterFormulario = () => {
  if (modoEdicao.value && marcaIdEdicao.value) {
    form.post(`/marcas/editar/${marcaIdEdicao.value}`, {
      preserveScroll: true,
      onSuccess: () => {
        cancelarEdicao()
      }
    })
  } else {
    form.post('/marcas', {
      preserveScroll: true,
      onSuccess: () => {
        form.reset()
      }
    })
  }
}

const prepararEdicao = (marca) => {
  modoEdicao.value = true
  marcaIdEdicao.value = marca.id || marca.Id
  form.Nome = marca.nome || marca.Nome
  form.Descricao = marca.descricao || marca.Descricao || ''
  form.Ativo = marca.ativo ?? marca.Ativo ?? true
}

const cancelarEdicao = () => {
  modoEdicao.value = false
  marcaIdEdicao.value = null
  form.reset()
  form.Ativo = true
}

const alterarStatus = (id) => {
  if (!id) return
  router.post(`/marcas/alternar-status/${id}`, {}, { preserveScroll: true })
}

const excluirMarca = (id) => {
  if (!id) return
  if (confirm('Tem certeza que deseja excluir esta marca?')) {
    router.post(`/marcas/excluir/${id}`, {}, { preserveScroll: true })
  }
}
</script>