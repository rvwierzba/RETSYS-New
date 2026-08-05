<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-6xl mx-auto">
      
      <!-- Cabeçalho -->
      <div>
        <h1 class="text-2xl font-black text-slate-950">Estoque de Armações</h1>
        <p class="text-sm text-slate-500">Cadastre e controle a quantidade em estoque e valores das armações.</p>
      </div>

      <!-- Mensagens de Erro -->
      <div v-if="$page.props.erro || $page.props.flash?.erro" class="p-4 rounded-xl bg-red-50 border border-red-200 text-red-700 text-sm font-medium">
        {{ $page.props.erro || $page.props.flash?.erro }}
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        <!-- Formulário de Cadastro / Edição -->
        <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm h-fit space-y-4">
          <div class="flex items-center justify-between">
            <h3 class="text-base font-bold text-slate-900">
              {{ modoEdicao ? 'Editar Armação' : 'Nova Armação' }}
            </h3>
            <button v-if="modoEdicao" @click="cancelarEdicao" class="text-xs text-slate-400 hover:text-slate-600 font-semibold underline">
              Cancelar
            </button>
          </div>

          <form @submit.prevent="submeterFormulario" class="space-y-4">
            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Marca *</label>
              <select v-model="form.MarcaId" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required>
                <option value="" disabled>Selecione uma Marca</option>
                <option v-for="m in (Marcas ?? marcas)" :key="m.id || m.Id" :value="m.id || m.Id">
                  {{ m.nome || m.Nome }}
                </option>
              </select>
            </div>

            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Código SKU *</label>
                <input v-model="form.Codigo" type="text" placeholder="EX: RB3025" class="w-full rounded-xl border-slate-200 text-sm uppercase focus:border-teal-500 focus:ring-teal-500" required />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Modelo / Ref *</label>
                <input v-model="form.Modelo" type="text" placeholder="Aviador" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
              </div>
            </div>

            <div class="grid grid-cols-3 gap-2">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Cor</label>
                <input v-model="form.Cor" type="text" placeholder="Dourado" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Tamanho</label>
                <input v-model="form.Tamanho" type="text" placeholder="58" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Material</label>
                <input v-model="form.Material" type="text" placeholder="Metal" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>
            </div>

            <div>
              <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Fornecedor</label>
              <input v-model="form.Fornecedor" type="text" placeholder="Ex: Luxottica" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
            </div>

            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Qtd Estoque</label>
                <input v-model.number="form.QuantidadeEstoque" type="number" min="0" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Qtd Mínima</label>
                <input v-model.number="form.QuantidadeMinima" type="number" min="0" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>
            </div>

            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Preço Custo (R$)</label>
                <input v-model.number="form.PrecoCusto" type="number" step="0.01" min="0" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" />
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase text-slate-400 tracking-wider mb-1.5">Preço Venda (R$)</label>
                <input v-model.number="form.PrecoFinal" type="number" step="0.01" min="0" class="w-full rounded-xl border-slate-200 text-sm focus:border-teal-500 focus:ring-teal-500" required />
              </div>
            </div>

            <button 
              type="submit" 
              :disabled="form.processing"
              class="w-full bg-teal-600 hover:bg-teal-700 disabled:bg-slate-200 text-white font-bold py-3 rounded-xl text-xs transition uppercase tracking-wider flex items-center justify-center min-h-[42px]"
            >
              <span v-if="form.processing">{{ modoEdicao ? 'Atualizando...' : 'Cadastrando...' }}</span>
              <span v-else>{{ modoEdicao ? 'Atualizar Armação' : 'Cadastrar Armação' }}</span>
            </button>
          </form>
        </div>

        <!-- Tabela do Estoque -->
        <div class="lg:col-span-2 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
          <h3 class="text-base font-bold text-slate-900 mb-4">Armações no Estoque</h3>

          <div v-if="!(Armacoes ?? armacoes) || (Armacoes ?? armacoes).length === 0" class="text-center py-12 border-2 border-dashed border-slate-100 rounded-xl">
            <p class="text-slate-400 text-sm font-medium">Nenhuma armação cadastrada no estoque.</p>
          </div>

          <div v-else class="overflow-x-auto">
            <table class="w-full text-left border-collapse">
              <thead>
                <tr class="border-b border-slate-100 text-slate-400 text-xs font-bold uppercase tracking-wider">
                  <th class="pb-3">Código / Modelo</th>
                  <th class="pb-3 text-center">Marca</th>
                  <th class="pb-3 text-center">Estoque</th>
                  <th class="pb-3 text-right">Preço Venda</th>
                  <th class="pb-3 text-center">Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in (Armacoes ?? armacoes)" :key="item.id || item.Id" class="border-b border-slate-50 hover:bg-slate-50/60 transition text-sm">
                  <td class="py-4">
                    <p class="font-bold text-slate-900">{{ item.codigo || item.Codigo }}</p>
                    <p class="text-xs text-slate-400">{{ item.modelo || item.Modelo }} {{ (item.cor || item.Cor) ? `- ${item.cor || item.Cor}` : '' }}</p>
                  </td>
                  <td class="py-4 text-center font-medium text-slate-700">
                    {{ item.marcaNome || item.MarcaNome || 'Sem Marca' }}
                  </td>
                  <td class="py-4 text-center">
                    <span :class="(item.quantidadeEstoque ?? item.QuantidadeEstoque) > 0 ? 'bg-emerald-50 text-emerald-700 border-emerald-100' : 'bg-red-50 text-red-700 border-red-100'" class="px-2.5 py-0.5 rounded-full text-xs font-bold border">
                      {{ item.quantidadeEstoque ?? item.QuantidadeEstoque }} un
                    </span>
                  </td>
                  <td class="py-4 text-right font-bold text-slate-900">
                    R$ {{ Number(item.precoFinal ?? item.PrecoFinal ?? 0).toFixed(2) }}
                  </td>
                  <td class="py-4 text-center">
                    <div class="flex items-center justify-center gap-1.5">
                      <button @click="prepararEdicao(item)" class="px-2.5 py-1 text-xs font-bold text-slate-700 bg-slate-100 hover:bg-slate-200 rounded-lg transition">
                        Editar
                      </button>
                      <button @click="excluirArmacao(item.id || item.Id)" class="px-2.5 py-1 text-xs font-bold text-red-600 hover:bg-red-50 rounded-lg transition">
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
  Armacoes: Array,
  armacoes: Array,
  Marcas: Array,
  marcas: Array
})

const modoEdicao = ref(false)
const armacaoIdEdicao = ref(null)

const form = useForm({
  Codigo: '',
  MarcaId: '',
  Modelo: '',
  Cor: '',
  Tamanho: '',
  Material: '',
  Fornecedor: '',
  QuantidadeEstoque: 1,
  QuantidadeMinima: 0,
  PrecoCusto: 0.00,
  PrecoFinal: 0.00
})

const submeterFormulario = () => {
  if (modoEdicao.value && armacaoIdEdicao.value) {
    form.post(`/armacoes/editar/${armacaoIdEdicao.value}`, {
      preserveScroll: true,
      onSuccess: () => cancelarEdicao()
    })
  } else {
    form.post('/armacoes', {
      preserveScroll: true,
      onSuccess: () => form.reset('Codigo', 'Modelo', 'Cor', 'Tamanho', 'Material', 'Fornecedor')
    })
  }
}

const prepararEdicao = (item) => {
  modoEdicao.value = true
  armacaoIdEdicao.value = item.id || item.Id
  form.Codigo = item.codigo || item.Codigo
  form.MarcaId = item.marcaId || item.MarcaId
  form.Modelo = item.modelo || item.Modelo
  form.Cor = item.cor || item.Cor || ''
  form.Tamanho = item.tamanho || item.Tamanho || ''
  form.Material = item.material || item.Material || ''
  form.Fornecedor = item.fornecedor || item.Fornecedor || ''
  form.QuantidadeEstoque = item.quantidadeEstoque ?? item.QuantidadeEstoque ?? 0
  form.QuantidadeMinima = item.quantidadeMinima ?? item.QuantidadeMinima ?? 0
  form.PrecoCusto = item.precoCusto ?? item.PrecoCusto ?? 0
  form.PrecoFinal = item.precoFinal ?? item.PrecoFinal ?? 0
}

const cancelarEdicao = () => {
  modoEdicao.value = false
  armacaoIdEdicao.value = null
  form.reset()
}

const excluirArmacao = (id) => {
  if (!id) return
  if (confirm('Tem certeza que deseja remover esta armação do estoque?')) {
    router.post(`/armacoes/excluir/${id}`, {}, { preserveScroll: true })
  }
}
</script>