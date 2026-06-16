<template>
  <div class="min-h-screen bg-slate-50 p-6 font-sans">
    
    <div class="max-w-6xl mx-auto mb-8">
      <h1 class="text-2xl font-bold text-slate-900">Gerenciamento de Marcas</h1>
      <p class="text-sm text-slate-500">Cadastre e organize as marcas de armações disponíveis no estoque.</p>
    </div>

    <div class="max-w-6xl mx-auto grid grid-cols-1 lg:grid-cols-3 gap-8">
      
      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm h-fit">
        <h3 class="text-lg font-bold text-slate-900 mb-4">Nova Marca</h3>
        
        <form @submit.prevent="submeterFormulario" class="space-y-4">
          <div>
            <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Nome da Marca</label>
            <input 
              v-model="form.Nome" 
              type="text" 
              placeholder="Ex: Ray-Ban, Oakley" 
              class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 placeholder:text-slate-300"
              required 
            />
          </div>

          <div>
            <label class="block text-xs font-bold uppercase text-slate-400 tracking-wider mb-2">Descrição / Notas</label>
            <textarea 
              v-model="form.Descricao" 
              rows="3" 
              placeholder="Notas sobre o fabricante..." 
              class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 placeholder:text-slate-300"
            ></textarea>
          </div>

          <button 
            type="submit" 
            class="w-full bg-teal-600 hover:bg-teal-700 text-white font-bold py-3 rounded-xl shadow-sm transition text-sm"
          >
            Salvar Marca
          </button>
        </form>
      </div>

      <div class="lg:col-span-2 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <h3 class="text-lg font-bold text-slate-900 mb-4">Marcas Cadastradas</h3>

        <div v-if="marcas.length === 0" class="text-center py-12 border-2 border-dashed border-slate-100 rounded-xl">
          <p class="text-slate-400 text-sm">Nenhuma marca cadastrada no sistema ainda.</p>
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-left border-collapse">
            <thead>
              <tr class="border-b border-slate-100 text-slate-400 text-xs font-bold uppercase tracking-wider">
                <th class="py-3">Nome</th>
                <th class="py-3">Descrição</th>
                <th class="py-3 text-center">Status</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="marca in marcas" :key="marca.id" class="border-b border-slate-50 hover:bg-slate-50/60 transition text-sm">
                <td class="py-4 font-semibold text-slate-800">{{ marca.nome }}</td>
                <td class="py-4 text-slate-500 max-w-xs truncate">{{ marca.descricao || '---' }}</td>
                <td class="py-4 text-center">
                  <span :class="marca.ativo ? 'bg-emerald-50 text-emerald-700 border border-emerald-200' : 'bg-slate-50 text-slate-500'" class="px-2.5 py-1 rounded-full text-xs font-medium">
                    {{ marca.ativo ? 'Ativo' : 'Inativo' }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { reactive } from 'vue'
import { router } from '@inertiajs/vue3'

// Recebe a lista mapeada diretamente do C# através das Props do Inertia
defineProps({
  marcas: Array
})

// Estado reativo do formulário baseado na nossa entidade C#
const form = reactive({
  Nome: '',
  Descricao: '',
  Ativo: true
})

// Envia os dados para a rota POST do C# sem dar reload na página
const submeterFormulario = () => {
  router.post('/marcas', form, {
    onSuccess: () => {
      // Limpa os campos após a inserção bem-sucedida
      form.Nome = ''
      form.Descricao = ''
    }
  })
}
</script>