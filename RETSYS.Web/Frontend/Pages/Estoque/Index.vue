<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6">
      
      <div class="max-w-6xl mx-auto">
        <h1 class="text-2xl font-black text-slate-950">Controle de Estoque</h1>
        <p class="text-sm text-slate-500">Gerencie o inventário de armações e óculos de sol da loja.</p>
      </div>

      <div class="max-w-6xl mx-auto grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm h-fit space-y-4">
          <h3 class="text-base font-bold text-slate-950">Adicionar Armação</h3>
          
          <form @submit.prevent="salvarArmacao" class="space-y-4">
            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Marca Fabricante *</label>
              <select v-model="form.MarcaId" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required>
                <option value="">Selecione a Marca</option>
                <option v-for="m in Marcas" :key="m.id || m.Id" :value="m.id || m.Id">{{ m.nome || m.Nome }}</option>
              </select>
            </div>

            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Modelo / Nome *</label>
              <input v-model="form.ModeloReferencia" type="text" placeholder="Ex: Aviator Clássico" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Código / SKU *</label>
                <input v-model="form.CodigoSku" type="text" placeholder="Ex: RB3025" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" required />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Cor *</label>
                <input v-model="form.Cor" type="text" placeholder="Ex: Dourado" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
              </div>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Tamanho</label>
                <input v-model="form.Tamanho" type="text" placeholder="Ex: 55" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Material</label>
                <input v-model="form.Material" type="text" placeholder="Ex: Metal" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Qtd Estoque</label>
                <input v-model.number="form.QuantidadeEstoque" type="number" min="0" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Preço de Venda (R$) *</label>
                <input v-model.number="form.PrecoVenda" type="number" step="0.01" placeholder="0,00" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500 font-mono" required />
              </div>
            </div>

            <button 
              type="submit" 
              :disabled="form.processing"
              class="w-full bg-teal-600 hover:bg-teal-700 disabled:bg-slate-200 disabled:text-slate-400 text-white font-bold py-3 rounded-xl text-xs transition shadow-sm uppercase tracking-wider flex items-center justify-center min-h-[42px]"
            >
              <span v-if="form.processing">Registrando...</span>
              <span v-else>Registrar no Estoque</span>
            </button>
          </form>
        </div>

        <div class="lg:col-span-2 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
          <h3 class="text-base font-bold text-slate-950 mb-4">Peças em Vitrine</h3>

          <div v-if="!Estoque || Estoque.length === 0" class="text-center py-12 border-2 border-dashed border-slate-100 rounded-xl text-slate-400 text-sm">
            Nenhuma armação registrada no inventário da ótica.
          </div>

          <div v-else class="overflow-x-auto">
            <table class="w-full text-left text-sm border-collapse">
              <thead>
                <tr class="border-b border-slate-100 text-slate-400 text-xs font-bold uppercase tracking-wider">
                  <th class="pb-3">Armação / Especificações</th>
                  <th class="pb-3 text-center">Código</th>
                  <th class="pb-3 text-center">Estoque</th>
                  <th class="pb-3 text-right">Preço Final</th>
                  <th v-if="eAdmin" class="pb-3 text-center">Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in Estoque" :key="item.id || item.Id" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
                  <td class="py-4">
                    <p class="font-bold text-slate-800">{{ item.modeloReferencia || item.ModeloReferencia }}</p>
                    <p class="text-xs text-teal-600 font-medium">
                      {{ item.marcaNome || item.MarcaNome }} 
                      <span class="text-slate-400">• {{ item.material || item.Material || 'N/A' }}</span>
                    </p>
                    <p class="text-[11px] text-slate-400">
                      Cor: {{ item.cor || item.Cor }} | Tam: {{ item.tamanho || item.Tamanho || 'padrão' }}
                    </p>
                  </td>
                  <td class="py-4 text-center font-mono text-xs text-slate-500">
                    {{ item.codigo || item.Codigo }}
                  </td>
                  <td class="py-4 text-center">
                    <span 
                      :class="(item.quantidadeEstoque || item.QuantidadeEstoque) <= 2 ? 'bg-red-50 text-red-700 border-red-100' : 'bg-slate-100 text-slate-700 border-slate-200'" 
                      class="px-2.5 py-0.5 rounded-full text-xs font-bold border"
                    >
                      {{ item.quantidadeEstoque ?? item.QuantidadeEstoque }} un
                    </span>
                  </td>
                  <td class="py-4 text-right font-black text-slate-950 font-mono">
                    R$ {{ formatarPreco(item.precoFinal ?? item.PrecoFinal) }}
                  </td>
                  
                  <td v-if="eAdmin" class="py-4 text-center">
                    <button 
                      @click="removerPecaEstoque(item.id || item.Id)"
                      class="text-[11px] font-bold text-red-600 bg-red-50 hover:bg-red-100 border border-red-200 px-2.5 py-1 rounded-lg transition active:scale-95"
                    >
                      Remover
                    </button>
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
import { computed } from 'vue'
import { useForm, usePage, router } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const page = usePage()

// Sincroniza dinamicamente se o usuário possui credenciais corporativas de Admin
const eAdmin = computed(() => {
  const perfil = page.props.auth?.usuarioPerfil || page.props.auth?.user?.perfil || ''
  return perfil.toLowerCase() === 'admin'
})

defineProps({
  Estoque: Array,
  Marcas: Array
})

// Adaptado para enviar as propriedades corretas exigidas pela entidade do C#
const form = useForm({
  MarcaId: '',
  ModeloReferencia: '',
  CodigoSku: '',
  Cor: '',
  Tamanho: '',
  Material: '',
  QuantidadeEstoque: 1,
  PrecoVenda: null
})

const salvarArmacao = () => {
  form.post('/estoque', {
    preserveScroll: true,
    onSuccess: () => {
      form.reset()
    }
  })
}

const removerPecaEstoque = (id) => {
  if (!id) return
  if (confirm('Tem certeza de que deseja remover permanentemente esta armação do inventário?')) {
    router.delete(`/estoque/${id}`, { preserveScroll: true })
  }
}

const formatarPreco = (valor) => {
  if (valor === undefined || valor === null) return '0,00'
  return Number(valor).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}
</script>