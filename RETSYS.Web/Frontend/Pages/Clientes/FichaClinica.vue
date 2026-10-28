<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-7xl mx-auto">
      
      <!-- Voltar e Cabeçalho do Paciente -->
      <div class="flex items-center gap-3">
        <a href="/clientes" class="p-2 bg-white rounded-xl border border-slate-200 text-slate-600 hover:bg-slate-50 transition shadow-sm">
          ← Voltar para Clientes
        </a>
      </div>

      <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <div class="flex items-center gap-2">
            <span class="bg-teal-50 text-teal-700 text-xs font-bold px-2.5 py-1 rounded-md border border-teal-200">
              Ficha Clínica &amp; Refrativa
            </span>
            <span class="text-xs text-slate-400 font-mono">CPF: {{ (Cliente || cliente)?.cpf || 'Não informado' }}</span>
          </div>
          <h1 class="text-2xl font-black text-slate-900 mt-1">{{ (Cliente || cliente)?.nome }}</h1>
          <p class="text-xs text-slate-500 mt-0.5 flex items-center gap-3">
            <span>📞 {{ (Cliente || cliente)?.telefone || 'Sem telefone' }}</span>
            <span v-if="(Cliente || cliente)?.idade">🎂 {{ (Cliente || cliente)?.idade }} anos ({{ (Cliente || cliente)?.dataNascimento }})</span>
            <span>📍 {{ (Cliente || cliente)?.endereco || 'Endereço não informado' }}</span>
          </p>
        </div>

        <div class="flex items-center gap-3 w-full md:w-auto">
          <button
            @click="abrirModalNovaReceita"
            class="flex-1 md:flex-initial bg-teal-600 hover:bg-teal-700 text-white font-bold py-2.5 px-5 rounded-xl text-xs uppercase tracking-wider transition shadow-sm flex items-center justify-center gap-2"
          >
            <span>+ Cadastrar Nova Receita</span>
          </button>
        </div>
      </div>

      <!-- Histórico de Receitas / Dioptrias -->
      <div class="space-y-4">
        <div class="flex items-center justify-between">
          <h2 class="text-lg font-black text-slate-900">Histórico de Exames e Refrações ({{ (Receitas || receitas)?.length || 0 }})</h2>
        </div>

        <div v-if="!(Receitas || receitas) || (Receitas || receitas).length === 0" class="text-center py-16 bg-white rounded-2xl border border-slate-200">
          <div class="text-4xl mb-2">📋</div>
          <p class="text-slate-500 font-bold">Nenhuma receita ou exame cadastrado para este paciente.</p>
          <p class="text-slate-400 text-xs mt-1">Clique no botão acima para adicionar a primeira prescrição óptica.</p>
        </div>

        <div v-else class="space-y-6">
          <div
            v-for="(rec, index) in (Receitas || receitas)"
            :key="rec.id || rec.Id"
            class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden"
          >
            <!-- Topo do Card de Receita -->
            <div class="bg-slate-900 text-white p-4 flex flex-wrap items-center justify-between gap-3">
              <div class="flex items-center gap-3">
                <span class="bg-teal-500 text-slate-950 font-black text-xs px-2.5 py-1 rounded-md">
                  Exame #{{ (Receitas || receitas).length - index }}
                </span>
                <div>
                  <p class="text-sm font-bold">Data do Exame: {{ rec.dataPrescricao || rec.DataPrescricao }}</p>
                  <p class="text-xs text-slate-400">
                    Especialista: {{ rec.medicoOptometrista || rec.MedicoOptometrista || 'Não informado' }} 
                    <span v-if="rec.crmCroo || rec.CrmCroo">({{ rec.crmCroo || rec.CrmCroo }})</span>
                  </p>
                </div>
              </div>

              <div class="flex items-center gap-2">
                <span v-if="rec.dataValidade || rec.DataValidade" class="text-xs bg-slate-800 text-amber-400 px-3 py-1 rounded-lg border border-slate-700">
                  Validade: {{ rec.dataValidade || rec.DataValidade }}
                </span>
                <a
                  :href="`/ordens/nova?clienteId=${(Cliente || cliente)?.id}&receitaId=${rec.id || rec.Id}`"
                  class="bg-teal-500 hover:bg-teal-400 text-slate-950 font-black text-xs px-3 py-1.5 rounded-lg transition"
                >
                  Criar O.S. com esta Receita →
                </a>
              </div>
            </div>

            <!-- Tabela Refrativa Óptica Padrão -->
            <div class="p-5">
              <div class="overflow-x-auto">
                <table class="w-full text-center border border-slate-200 rounded-xl overflow-hidden text-xs">
                  <thead class="bg-slate-100 text-slate-700 font-bold uppercase text-[10px] tracking-wider">
                    <tr>
                      <th class="py-2.5 px-3 border-r border-slate-200 text-left bg-slate-200">Olho</th>
                      <th class="py-2.5 px-3 border-r border-slate-200">Esférico (Longe)</th>
                      <th class="py-2.5 px-3 border-r border-slate-200">Cilíndrico</th>
                      <th class="py-2.5 px-3 border-r border-slate-200">Eixo</th>
                      <th class="py-2.5 px-3 border-r border-slate-200">Adição</th>
                      <th class="py-2.5 px-3 border-r border-slate-200">Esf. Perto</th>
                      <th class="py-2.5 px-3 border-r border-slate-200">DNP (mm)</th>
                      <th class="py-2.5 px-3">Altura (mm)</th>
                    </tr>
                  </thead>
                  <tbody class="divide-y divide-slate-200 font-mono font-medium text-slate-800">
                    <tr class="hover:bg-teal-50/30">
                      <td class="py-3 px-3 border-r border-slate-200 font-black text-left text-slate-900 bg-slate-50 font-sans">
                        OD (Direito)
                      </td>
                      <td class="py-3 px-3 border-r border-slate-200 font-bold text-teal-700">
                        {{ formatGrau(rec.odEsferico ?? rec.OdEsferico) }}
                      </td>
                      <td class="py-3 px-3 border-r border-slate-200">
                        {{ formatGrau(rec.odCilindrico ?? rec.OdCilindrico) }}
                      </td>
                      <td class="py-3 px-3 border-r border-slate-200">
                        {{ (rec.odEixo ?? rec.OdEixo) ? (rec.odEixo ?? rec.OdEixo) + '°' : '-' }}
                      </td>
                      <td rowspan="2" class="py-3 px-3 border-r border-slate-200 bg-teal-50 font-bold text-teal-800 text-sm align-middle">
                        {{ rec.adicao || rec.Adicao ? '+' + formatGrau(rec.adicao || rec.Adicao) : '-' }}
                      </td>
                      <td class="py-3 px-3 border-r border-slate-200 font-bold text-indigo-700">
                        {{ formatGrau(rec.odEsfericoPerto ?? rec.OdEsfericoPerto) }}
                      </td>
                      <td class="py-3 px-3 border-r border-slate-200">
                        {{ rec.odDnp ?? rec.OdDnp ?? '-' }}
                      </td>
                      <td class="py-3 px-3">
                        {{ rec.odAltura ?? rec.OdAltura ?? '-' }}
                      </td>
                    </tr>
                    <tr class="hover:bg-teal-50/30">
                      <td class="py-3 px-3 border-r border-slate-200 font-black text-left text-slate-900 bg-slate-50 font-sans">
                        OE (Esquerdo)
                      </td>
                      <td class="py-3 px-3 border-r border-slate-200 font-bold text-teal-700">
                        {{ formatGrau(rec.oeEsferico ?? rec.OeEsferico) }}
                      </td>
                      <td class="py-3 px-3 border-r border-slate-200">
                        {{ formatGrau(rec.oeCilindrico ?? rec.OeCilindrico) }}
                      </td>
                      <td class="py-3 px-3 border-r border-slate-200">
                        {{ (rec.oeEixo ?? rec.OeEixo) ? (rec.oeEixo ?? rec.OeEixo) + '°' : '-' }}
                      </td>
                      <td class="py-3 px-3 border-r border-slate-200 font-bold text-indigo-700">
                        {{ formatGrau(rec.oeEsfericoPerto ?? rec.OeEsfericoPerto) }}
                      </td>
                      <td class="py-3 px-3 border-r border-slate-200">
                        {{ rec.oeDnp ?? rec.OeDnp ?? '-' }}
                      </td>
                      <td class="py-3 px-3">
                        {{ rec.oeAltura ?? rec.OeAltura ?? '-' }}
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>

              <!-- Observações / Clínica -->
              <div v-if="rec.observacoes || rec.Observacoes" class="mt-4 p-3 bg-slate-50 rounded-xl border border-slate-100 text-xs text-slate-600">
                <span class="font-bold text-slate-800">Observações Clínicas:</span> {{ rec.observacoes || rec.Observacoes }}
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Modal de Nova Receita -->
      <div v-if="modalAberto" class="fixed inset-0 bg-slate-950/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
        <div class="bg-white rounded-3xl max-w-4xl w-full p-6 md:p-8 space-y-6 max-h-[90vh] overflow-y-auto shadow-2xl">
          <div class="flex justify-between items-center border-b pb-4">
            <div>
              <h3 class="text-xl font-black text-slate-900">Cadastrar Nova Prescrição Óptica</h3>
              <p class="text-xs text-slate-500">Insira as dioptrias de Longe, Perto e medidas pupilares (DNP/Alturas).</p>
            </div>
            <button @click="modalAberto = false" class="text-slate-400 hover:text-slate-700 text-xl font-bold">✕</button>
          </div>

          <form @submit.prevent="salvarReceita" class="space-y-6">
            <!-- Dados do Médico / Exame -->
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 text-xs">
              <div>
                <label class="block font-bold text-slate-700 mb-1">Data da Consulta *</label>
                <input v-model="formReceita.dataPrescricao" type="date" required class="w-full p-2.5 rounded-xl border border-slate-300 font-medium" />
              </div>
              <div>
                <label class="block font-bold text-slate-700 mb-1">Médico / Optometrista</label>
                <input v-model="formReceita.medicoOptometrista" type="text" placeholder="Dr(a). Nome" class="w-full p-2.5 rounded-xl border border-slate-300" />
              </div>
              <div>
                <label class="block font-bold text-slate-700 mb-1">CRM / CROO</label>
                <input v-model="formReceita.crmCroo" type="text" placeholder="Ex: CRM-SP 12345" class="w-full p-2.5 rounded-xl border border-slate-300" />
              </div>
            </div>

            <!-- Grade Refrativa (OD e OE) -->
            <div class="space-y-4">
              <h4 class="font-black text-slate-800 text-sm flex items-center gap-2">
                <span>👓 Dioptrias e Refração</span>
              </h4>

              <!-- Olho Direito -->
              <div class="p-4 bg-teal-50/40 rounded-2xl border border-teal-100 space-y-3">
                <p class="font-bold text-teal-800 text-xs uppercase">Olho Direito (OD)</p>
                <div class="grid grid-cols-2 md:grid-cols-5 gap-3 text-xs">
                  <div>
                    <label class="block text-slate-600 mb-1 font-bold">Esférico Longe</label>
                    <input v-model.number="formReceita.odEsferico" type="number" step="0.25" placeholder="0.00" class="w-full p-2 rounded-lg border border-slate-300 font-mono text-center font-bold" />
                  </div>
                  <div>
                    <label class="block text-slate-600 mb-1 font-bold">Cilíndrico</label>
                    <input v-model.number="formReceita.odCilindrico" type="number" step="0.25" placeholder="0.00" class="w-full p-2 rounded-lg border border-slate-300 font-mono text-center font-bold" />
                  </div>
                  <div>
                    <label class="block text-slate-600 mb-1 font-bold">Eixo (°)</label>
                    <input v-model.number="formReceita.odEixo" type="number" min="0" max="180" placeholder="0" class="w-full p-2 rounded-lg border border-slate-300 font-mono text-center font-bold" />
                  </div>
                  <div>
                    <label class="block text-slate-600 mb-1 font-bold">DNP OD (mm)</label>
                    <input v-model.number="formReceita.dnpOd" type="number" step="0.5" placeholder="31.5" class="w-full p-2 rounded-lg border border-slate-300 font-mono text-center font-bold" />
                  </div>
                  <div>
                    <label class="block text-slate-600 mb-1 font-bold">Altura OD (mm)</label>
                    <input v-model.number="formReceita.alturaOd" type="number" step="0.5" placeholder="18.0" class="w-full p-2 rounded-lg border border-slate-300 font-mono text-center font-bold" />
                  </div>
                </div>
              </div>

              <!-- Olho Esquerdo -->
              <div class="p-4 bg-teal-50/40 rounded-2xl border border-teal-100 space-y-3">
                <p class="font-bold text-teal-800 text-xs uppercase">Olho Esquerdo (OE)</p>
                <div class="grid grid-cols-2 md:grid-cols-5 gap-3 text-xs">
                  <div>
                    <label class="block text-slate-600 mb-1 font-bold">Esférico Longe</label>
                    <input v-model.number="formReceita.oeEsferico" type="number" step="0.25" placeholder="0.00" class="w-full p-2 rounded-lg border border-slate-300 font-mono text-center font-bold" />
                  </div>
                  <div>
                    <label class="block text-slate-600 mb-1 font-bold">Cilíndrico</label>
                    <input v-model.number="formReceita.oeCilindrico" type="number" step="0.25" placeholder="0.00" class="w-full p-2 rounded-lg border border-slate-300 font-mono text-center font-bold" />
                  </div>
                  <div>
                    <label class="block text-slate-600 mb-1 font-bold">Eixo (°)</label>
                    <input v-model.number="formReceita.oeEixo" type="number" min="0" max="180" placeholder="0" class="w-full p-2 rounded-lg border border-slate-300 font-mono text-center font-bold" />
                  </div>
                  <div>
                    <label class="block text-slate-600 mb-1 font-bold">DNP OE (mm)</label>
                    <input v-model.number="formReceita.dnpOe" type="number" step="0.5" placeholder="31.5" class="w-full p-2 rounded-lg border border-slate-300 font-mono text-center font-bold" />
                  </div>
                  <div>
                    <label class="block text-slate-600 mb-1 font-bold">Altura OE (mm)</label>
                    <input v-model.number="formReceita.alturaOe" type="number" step="0.5" placeholder="18.0" class="w-full p-2 rounded-lg border border-slate-300 font-mono text-center font-bold" />
                  </div>
                </div>
              </div>

              <!-- Adição e Perto -->
              <div class="p-4 bg-slate-50 rounded-2xl border border-slate-200 grid grid-cols-1 md:grid-cols-3 gap-4 text-xs">
                <div>
                  <label class="block font-bold text-slate-700 mb-1">Adição (Perto) +</label>
                  <input v-model.number="formReceita.adicao" type="number" step="0.25" placeholder="+2.00" class="w-full p-2 rounded-lg border border-slate-300 font-mono text-center font-bold text-teal-700" />
                </div>
                <div>
                  <label class="block font-bold text-slate-700 mb-1">OD Esférico Perto</label>
                  <input v-model.number="formReceita.odEsfericoPerto" type="number" step="0.25" placeholder="Auto ou manual" class="w-full p-2 rounded-lg border border-slate-300 font-mono text-center" />
                </div>
                <div>
                  <label class="block font-bold text-slate-700 mb-1">OE Esférico Perto</label>
                  <input v-model.number="formReceita.oeEsfericoPerto" type="number" step="0.25" placeholder="Auto ou manual" class="w-full p-2 rounded-lg border border-slate-300 font-mono text-center" />
                </div>
              </div>
            </div>

            <!-- Observações -->
            <div>
              <label class="block font-bold text-slate-700 text-xs mb-1">Observações Técnicas / Recomendações</label>
              <textarea v-model="formReceita.observacoes" rows="2" placeholder="Ex: Paciente sensível a reflexos, indicado antirreflexo premium ou filtro azul..." class="w-full p-3 rounded-xl border border-slate-300 text-xs"></textarea>
            </div>

            <div class="flex justify-end gap-3 pt-4 border-t">
              <button @click="modalAberto = false" type="button" class="px-5 py-2.5 rounded-xl border border-slate-300 text-slate-700 font-bold text-xs hover:bg-slate-50">
                Cancelar
              </button>
              <button type="submit" class="px-6 py-2.5 rounded-xl bg-teal-600 hover:bg-teal-700 text-white font-black text-xs uppercase tracking-wider shadow-md">
                Salvar Prescrição no Prontuário
              </button>
            </div>
          </form>
        </div>
      </div>

    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { router } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  Cliente: Object,
  cliente: Object,
  Receitas: Array,
  receitas: Array
})

const modalAberto = ref(false)

const formReceita = reactive({
  dataPrescricao: new Date().toISOString().substring(0, 10),
  medicoOptometrista: '',
  crmCroo: '',
  odEsferico: 0,
  odCilindrico: 0,
  odEixo: 0,
  dnpOd: 31,
  alturaOd: 18,
  oeEsferico: 0,
  oeCilindrico: 0,
  oeEixo: 0,
  dnpOe: 31,
  alturaOe: 18,
  adicao: null,
  odEsfericoPerto: null,
  oeEsfericoPerto: null,
  observacoes: ''
})

const abrirModalNovaReceita = () => {
  modalAberto.value = true
}

const salvarReceita = () => {
  const clienteId = props.Cliente?.id || props.Cliente?.Id || props.cliente?.id || props.cliente?.Id
  router.post(`/clientes/${clienteId}/receitas`, formReceita, {
    preserveScroll: true,
    onSuccess: () => {
      modalAberto.value = false
      alert('Prescrição óptica cadastrada com sucesso no histórico do paciente!')
    }
  })
}

const formatGrau = (valor) => {
  if (valor === undefined || valor === null || valor === '') return '0.00'
  const num = Number(valor)
  return num > 0 ? `+${num.toFixed(2)}` : num.toFixed(2)
}
</script>

