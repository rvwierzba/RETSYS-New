<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-7xl mx-auto">
      
      <!-- CABEÇALHO DA CARTEIRA DE CLIENTES (PADRÃO SSÓTICA / PWI) -->
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 bg-white p-6 rounded-3xl border border-slate-200 shadow-sm">
        <div>
          <div class="flex items-center gap-3">
            <span class="w-10 h-10 rounded-2xl bg-teal-500/10 text-teal-600 flex items-center justify-center text-xl font-black border border-teal-500/20">
              👥
            </span>
            <div>
              <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight">Carteira de Clientes &amp; Prontuários</h1>
              <p class="text-xs text-slate-500 mt-0.5">Gestão de fichas clínicas, histórico oftalmológico, receitas e contatos via WhatsApp.</p>
            </div>
          </div>
        </div>

        <div class="flex items-center gap-2 font-mono text-xs">
          <Link 
            href="/crm" 
            class="px-4 py-2.5 rounded-2xl bg-amber-50 hover:bg-amber-100 text-amber-900 border border-amber-200 font-bold transition flex items-center gap-1.5"
          >
            <span>📢</span> Central de Recall
          </Link>
          <button 
            @click="exibirModalNovoCliente = true"
            class="px-5 py-2.5 rounded-2xl bg-teal-600 hover:bg-teal-700 text-white font-bold transition shadow-sm flex items-center gap-1.5 uppercase tracking-wider"
          >
            <span>+</span> Novo Cliente
          </button>
        </div>
      </div>

      <!-- BARRA DE BUSCA & FILTROS -->
      <div class="bg-white p-5 rounded-3xl border border-slate-200 shadow-sm space-y-3">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-3 items-center">
          
          <div class="md:col-span-2 relative">
            <span class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">🔍</span>
            <input 
              v-model="termoBusca" 
              @input="onInputBusca"
              type="text" 
              placeholder="Buscar cliente por Nome, CPF ou Telefone..." 
              class="w-full pl-10 pr-10 py-2.5 rounded-2xl border border-slate-200 text-xs font-semibold text-slate-800 focus:border-teal-500 bg-slate-50"
            />
            <button 
              v-if="termoBusca" 
              @click="termoBusca = ''; aplicarFiltros()"
              class="absolute inset-y-0 right-0 pr-3 text-slate-400 hover:text-slate-600 text-xs"
            >
              ✕
            </button>
          </div>

          <div class="flex items-center justify-between text-xs font-mono text-slate-500 px-2">
            <span>Total: <b>{{ listaClientes.length }}</b> pacientes</span>
            <span class="text-teal-700 font-bold">● Base Ativa</span>
          </div>

        </div>
      </div>

      <!-- LISTAGEM DE CLIENTES EM TABELA MODERNA -->
      <div class="bg-white rounded-3xl border border-slate-200 shadow-sm overflow-hidden">
        
        <div v-if="listaClientes.length === 0" class="text-center py-16 px-4 space-y-3">
          <span class="text-4xl block">👤</span>
          <h3 class="text-base font-bold text-slate-800 font-mono">Nenhum cliente encontrado</h3>
          <p class="text-xs text-slate-400 max-w-sm mx-auto">
            Não foram localizados clientes com o termo pesquisado.
          </p>
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-left text-xs border-collapse">
            <thead>
              <tr class="border-b border-slate-100 bg-slate-50/70 text-slate-400 font-mono font-bold uppercase tracking-wider text-[11px]">
                <th class="py-3.5 px-4">Paciente</th>
                <th class="py-3.5 px-4">CPF / Documento</th>
                <th class="py-3.5 px-4">Contato (WhatsApp)</th>
                <th class="py-3.5 px-4 text-center">Última Compra</th>
                <th class="py-3.5 px-4 text-right">Total Investido</th>
                <th class="py-3.5 px-4 text-center">Ações Clínicas</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100 font-sans">
              <tr 
                v-for="c in listaClientes" 
                :key="c.id" 
                class="hover:bg-slate-50/80 transition"
              >
                <!-- Paciente -->
                <td class="py-3.5 px-4">
                  <div class="flex items-center gap-3">
                    <div class="w-8 h-8 rounded-full bg-teal-50 border border-teal-200 text-teal-800 font-bold flex items-center justify-center font-mono text-xs uppercase shrink-0">
                      {{ c.nome.substring(0, 2) }}
                    </div>
                    <div>
                      <p class="font-bold text-slate-900 text-sm leading-tight">{{ c.nome }}</p>
                      <span v-if="c.convenio" class="text-[10px] text-teal-700 bg-teal-50 px-1.5 py-0.2 rounded border border-teal-200 mt-0.5 inline-block">
                        {{ c.convenio }}
                      </span>
                    </div>
                  </div>
                </td>

                <!-- CPF -->
                <td class="py-3.5 px-4 font-mono text-slate-600 text-xs">
                  {{ c.cpf || 'Não cadastrado' }}
                </td>

                <!-- WhatsApp -->
                <td class="py-3.5 px-4 font-mono">
                  <a 
                    v-if="c.telefone"
                    :href="gerarLinkWhatsApp(c.telefone, c.nome)"
                    target="_blank"
                    class="inline-flex items-center gap-1.5 text-emerald-700 bg-emerald-50 hover:bg-emerald-100 px-2.5 py-1 rounded-xl border border-emerald-200 transition text-[11px] font-bold"
                  >
                    <span>💬</span> {{ c.telefone }}
                  </a>
                  <span v-else class="text-slate-400 text-xs">--</span>
                </td>

                <!-- Última Compra -->
                <td class="py-3.5 px-4 text-center font-mono text-xs text-slate-700">
                  <span class="px-2 py-0.5 rounded-lg bg-slate-100 font-bold">
                    {{ c.ultimaOs || 'Nenhuma OS' }}
                  </span>
                </td>

                <!-- Total Gasto -->
                <td class="py-3.5 px-4 text-right font-mono font-black text-slate-900 text-xs">
                  R$ {{ formatarMoeda(c.totalGasto) }}
                </td>

                <!-- Ações Clínicas -->
                <td class="py-3.5 px-4 text-center">
                  <div class="flex items-center justify-center gap-1.5">
                    
                    <!-- Ficha Clínica / Receitas -->
                    <Link 
                      :href="`/clientes/${c.id}/receitas`" 
                      class="px-2.5 py-1.5 rounded-xl bg-teal-50 hover:bg-teal-100 text-teal-800 border border-teal-200 font-bold transition text-xs font-mono flex items-center gap-1"
                      title="Ver Histórico de Receitas e Graus Clínicos"
                    >
                      <span>📜</span> Ficha
                    </Link>

                    <!-- Histórico Completo de Compras -->
                    <Link 
                      :href="`/clientes/${c.id}/historico`" 
                      class="p-1.5 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-700 transition text-xs"
                      title="Histórico de Ordens e Compras"
                    >
                      👁️
                    </Link>

                    <!-- Editar Cadastro -->
                    <button 
                      @click="abrirEdicaoCliente(c)" 
                      class="p-1.5 rounded-xl bg-indigo-50 hover:bg-indigo-100 text-indigo-700 border border-indigo-200 transition text-xs"
                      title="Editar Dados Cadastrais"
                    >
                      ✏️
                    </button>

                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

      </div>

      <!-- MODAL: CADASTRAR NOVO CLIENTE -->
      <div v-if="exibirModalNovoCliente" class="fixed inset-0 bg-slate-950/70 backdrop-blur-xs z-50 flex items-center justify-center p-4">
        <div class="bg-white rounded-3xl border border-slate-200 shadow-2xl max-w-xl w-full p-6 space-y-5">
          <div class="flex items-center justify-between border-b border-slate-100 pb-3">
            <h3 class="text-base font-bold text-slate-950 font-mono flex items-center gap-2">
              <span>👤</span> Cadastrar Novo Cliente
            </h3>
            <button @click="exibirModalNovoCliente = false" class="text-slate-400 hover:text-slate-800 font-bold">✕</button>
          </div>

          <form @submit.prevent="salvarNovoCliente" class="space-y-4 text-xs">
            <div>
              <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1">Nome Completo *</label>
              <input v-model="formNovo.Nome" type="text" placeholder="Ex: Roberto Carlos Silva" class="w-full rounded-2xl border-slate-200 text-xs font-bold bg-slate-50 focus:border-teal-500" required />
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div>
                <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1">CPF</label>
                <input v-model="formNovo.CPF" type="text" placeholder="000.000.000-00" class="w-full rounded-2xl border-slate-200 text-xs font-mono bg-slate-50 focus:border-teal-500" />
              </div>
              <div>
                <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1">Telefone / WhatsApp *</label>
                <input v-model="formNovo.Telefone" type="text" placeholder="(00) 00000-0000" class="w-full rounded-2xl border-slate-200 text-xs font-bold bg-slate-50 focus:border-teal-500" required />
              </div>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div>
                <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1">E-mail</label>
                <input v-model="formNovo.Email" type="email" placeholder="cliente@email.com" class="w-full rounded-2xl border-slate-200 text-xs bg-slate-50 focus:border-teal-500" />
              </div>
              <div>
                <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1">Convênio / Parceria</label>
                <input v-model="formNovo.Convenio" type="text" placeholder="Ex: Unimed, Aposentados" class="w-full rounded-2xl border-slate-200 text-xs bg-slate-50 focus:border-teal-500" />
              </div>
            </div>

            <div class="flex justify-end gap-2 pt-3 border-t border-slate-100">
              <button type="button" @click="exibirModalNovoCliente = false" class="px-4 py-2 rounded-xl text-slate-600 font-bold hover:bg-slate-100">
                Cancelar
              </button>
              <button type="submit" :disabled="salvando" class="px-5 py-2 rounded-xl bg-teal-600 hover:bg-teal-700 text-white font-bold transition shadow-sm">
                {{ salvando ? 'Gravando...' : 'Gravar Cliente' }}
              </button>
            </div>
          </form>
        </div>
      </div>

      <!-- MODAL: EDITAR CLIENTE -->
      <div v-if="exibirModalEditarCliente" class="fixed inset-0 bg-slate-950/70 backdrop-blur-xs z-50 flex items-center justify-center p-4">
        <div class="bg-white rounded-3xl border border-slate-200 shadow-2xl max-w-xl w-full p-6 space-y-5">
          <div class="flex items-center justify-between border-b border-slate-100 pb-3">
            <h3 class="text-base font-bold text-slate-950 font-mono flex items-center gap-2">
              <span>✏️</span> Editar Dados do Cliente
            </h3>
            <button @click="exibirModalEditarCliente = false" class="text-slate-400 hover:text-slate-800 font-bold">✕</button>
          </div>

          <form @submit.prevent="salvarEdicaoCliente" class="space-y-4 text-xs">
            <div>
              <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1">Nome Completo *</label>
              <input v-model="formEditar.Nome" type="text" placeholder="Ex: Roberto Carlos Silva" class="w-full rounded-2xl border-slate-200 text-xs font-bold bg-slate-50 focus:border-teal-500" required />
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div>
                <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1">CPF</label>
                <input v-model="formEditar.CPF" type="text" placeholder="000.000.000-00" class="w-full rounded-2xl border-slate-200 text-xs font-mono bg-slate-50 focus:border-teal-500" />
              </div>
              <div>
                <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1">Telefone / WhatsApp *</label>
                <input v-model="formEditar.Telefone" type="text" placeholder="(00) 00000-0000" class="w-full rounded-2xl border-slate-200 text-xs font-bold bg-slate-50 focus:border-teal-500" required />
              </div>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div>
                <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1">E-mail</label>
                <input v-model="formEditar.Email" type="email" placeholder="cliente@email.com" class="w-full rounded-2xl border-slate-200 text-xs bg-slate-50 focus:border-teal-500" />
              </div>
              <div>
                <label class="block font-bold uppercase text-slate-400 tracking-wider mb-1">Convênio / Parceria</label>
                <input v-model="formEditar.Convenio" type="text" placeholder="Ex: Unimed, Aposentados" class="w-full rounded-2xl border-slate-200 text-xs bg-slate-50 focus:border-teal-500" />
              </div>
            </div>

            <div class="flex items-center justify-between pt-3 border-t border-slate-100">
              <button 
                type="button" 
                @click="excluirClienteAtual" 
                class="px-4 py-2 rounded-xl text-rose-600 font-bold hover:bg-rose-50 transition"
              >
                🗑️ Excluir Cliente
              </button>

              <div class="flex gap-2">
                <button type="button" @click="exibirModalEditarCliente = false" class="px-4 py-2 rounded-xl text-slate-600 font-bold hover:bg-slate-100">
                  Cancelar
                </button>
                <button type="submit" :disabled="salvando" class="px-5 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white font-bold transition shadow-sm">
                  {{ salvando ? 'Salvando...' : 'Salvar Alterações' }}
                </button>
              </div>
            </div>
          </form>
        </div>
      </div>

    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { ref, computed } from 'vue'
import { router, Link } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  Clientes: Array, clientes: Array,
  FiltroBusca: String, filtroBusca: String
})

const termoBusca = ref(props.FiltroBusca ?? props.filtroBusca ?? '')
const exibirModalNovoCliente = ref(false)
const exibirModalEditarCliente = ref(false)
const salvando = ref(false)

const formNovo = ref({
  Nome: '',
  CPF: '',
  Telefone: '',
  Email: '',
  Convenio: ''
})

const formEditar = ref({
  id: '',
  Nome: '',
  CPF: '',
  Telefone: '',
  Email: '',
  Convenio: ''
})

const abrirEdicaoCliente = (c) => {
  formEditar.value = {
    id: c.id,
    Nome: c.nome,
    CPF: c.cpf,
    Telefone: c.telefone,
    Email: c.email,
    Convenio: c.convenio
  }
  exibirModalEditarCliente.value = true
}

const salvarEdicaoCliente = () => {
  if (!formEditar.value.Nome || !formEditar.value.Telefone) {
    alert('Preencha Nome e Telefone.')
    return
  }
  salvando.value = true
  router.post(`/clientes/editar/${formEditar.value.id}`, formEditar.value, {
    preserveScroll: true,
    onSuccess: () => {
      exibirModalEditarCliente.value = false
      salvando.value = false
    },
    onError: () => {
      salvando.value = false
    }
  })
}

const excluirClienteAtual = () => {
  if (confirm(`Deseja realmente excluir o cliente ${formEditar.value.Nome}?`)) {
    salvando.value = true
    router.post(`/clientes/excluir/${formEditar.value.id}`, {}, {
      preserveScroll: true,
      onSuccess: () => {
        exibirModalEditarCliente.value = false
        salvando.value = false
      },
      onError: () => {
        salvando.value = false
      }
    })
  }
}

const listaClientes = computed(() => {
  const bruta = props.Clientes ?? props.clientes ?? []
  return bruta.map(c => ({
    id: c.Id ?? c.id,
    nome: c.Nome ?? c.nome ?? '',
    cpf: c.CPF ?? c.cpf ?? '',
    telefone: c.Telefone ?? c.telefone ?? '',
    email: c.Email ?? c.email ?? '',
    convenio: c.Convenio ?? c.convenio ?? '',
    ultimaOs: c.UltimaOs ?? c.ultimaOs ?? '',
    totalGasto: c.TotalGasto ?? c.totalGasto ?? 0
  }))
})

let timerBusca = null
const onInputBusca = () => {
  if (timerBusca) clearTimeout(timerBusca)
  timerBusca = setTimeout(() => {
    aplicarFiltros()
  }, 300)
}

const aplicarFiltros = () => {
  router.get('/clientes', { busca: termoBusca.value }, { preserveState: true, replace: true })
}

const salvarNovoCliente = () => {
  if (!formNovo.value.Nome || !formNovo.value.Telefone) {
    alert('Preencha Nome e Telefone.')
    return
  }
  salvando.value = true
  router.post('/clientes', formNovo.value, {
    preserveScroll: true,
    onSuccess: () => {
      exibirModalNovoCliente.value = false
      salvando.value = false
      formNovo.value = { Nome: '', CPF: '', Telefone: '', Email: '', Convenio: '' }
    },
    onError: () => {
      salvando.value = false
    }
  })
}

const formatarMoeda = (val) => {
  return Number(val || 0).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const gerarLinkWhatsApp = (tel, nome) => {
  const clean = (tel || '').replace(/\D/g, '')
  const primeiroNome = (nome || '').split(' ')[0]
  const msg = `Olá ${primeiroNome}, tudo bem? Aqui é da Ótica Retina. Como estão seus óculos? Estamos à disposição se precisar de qualquer ajuste gratuito!`
  return `https://api.whatsapp.com/send?phone=55${clean}&text=${encodeURIComponent(msg)}`
}
</script>
