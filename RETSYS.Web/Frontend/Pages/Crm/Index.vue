<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-7xl mx-auto">
      
      <!-- Cabeçalho Principal -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white p-6 rounded-3xl border border-slate-200 shadow-sm">
        <div>
          <div class="flex items-center gap-2 mb-1">
            <span class="text-xs font-mono font-bold uppercase tracking-wider text-pink-700 bg-pink-50 px-2.5 py-0.5 rounded-full border border-pink-200">
              CRM Óptico &amp; Fidelização
            </span>
          </div>
          <h1 class="text-2xl font-black text-slate-900 tracking-tight">Central de Disparos WhatsApp</h1>
          <p class="text-xs text-slate-500 mt-0.5">
            Disparos automáticos e com 1 clique: Recall de 1 ano, adaptação de multifocais e aniversariantes do mês.
          </p>
        </div>

        <div class="flex items-center gap-3">
          <div class="bg-slate-50 border border-slate-200 px-4 py-2 rounded-2xl flex items-center gap-2">
            <span class="text-xs font-mono font-bold text-slate-500 uppercase">Total na Esteira:</span>
            <span class="text-base font-black text-slate-900 font-mono">{{ totalContatos }}</span>
          </div>
        </div>
      </div>

      <!-- Abas de Navegação Segmentada -->
      <div class="flex items-center gap-2 bg-slate-100 p-1.5 rounded-2xl overflow-x-auto text-xs font-bold font-mono">
        <button
          @click="abaAtiva = 'recall'"
          :class="[
            'px-4 py-2.5 rounded-xl transition shrink-0 flex items-center gap-2',
            abaAtiva === 'recall'
              ? 'bg-white text-slate-900 shadow-xs font-black'
              : 'text-slate-500 hover:text-slate-800'
          ]"
        >
          <span>⏰ Retorno de 1 Ano (Recall)</span>
          <span class="px-2 py-0.5 rounded-full text-[10px] font-mono" :class="abaAtiva === 'recall' ? 'bg-amber-100 text-amber-800' : 'bg-slate-200 text-slate-600'">
            {{ recallList.length }}
          </span>
        </button>

        <button
          @click="abaAtiva = 'posvenda'"
          :class="[
            'px-4 py-2.5 rounded-xl transition shrink-0 flex items-center gap-2',
            abaAtiva === 'posvenda'
              ? 'bg-white text-slate-900 shadow-xs font-black'
              : 'text-slate-500 hover:text-slate-800'
          ]"
        >
          <span>👓 Pós-Venda (Adaptação)</span>
          <span class="px-2 py-0.5 rounded-full text-[10px] font-mono" :class="abaAtiva === 'posvenda' ? 'bg-teal-100 text-teal-800' : 'bg-slate-200 text-slate-600'">
            {{ posVendaList.length }}
          </span>
        </button>

        <button
          @click="abaAtiva = 'aniversariantes'"
          :class="[
            'px-4 py-2.5 rounded-xl transition shrink-0 flex items-center gap-2',
            abaAtiva === 'aniversariantes'
              ? 'bg-white text-slate-900 shadow-xs font-black'
              : 'text-slate-500 hover:text-slate-800'
          ]"
        >
          <span>🎂 Aniversariantes do Mês</span>
          <span class="px-2 py-0.5 rounded-full text-[10px] font-mono" :class="abaAtiva === 'aniversariantes' ? 'bg-pink-100 text-pink-800' : 'bg-slate-200 text-slate-600'">
            {{ aniversariantesList.length }}
          </span>
        </button>
      </div>

      <!-- ABA 1: RECALL DE 1 ANO -->
      <div v-show="abaAtiva === 'recall'" class="space-y-4">
        <div class="bg-amber-50 border border-amber-200 p-4 rounded-2xl text-amber-950 text-xs flex items-center justify-between gap-4">
          <div class="flex items-center gap-3">
            <span class="text-2xl">⏰</span>
            <div>
              <p class="font-bold">Regra de Recall Visual (300 a 480 dias):</p>
              <p class="text-[11px] text-amber-800">
                Pacientes cuja receita oftálmica ou exame de vista completou ou está completando 1 ano. Dispare o convite no WhatsApp para renovação de grau.
              </p>
            </div>
          </div>
        </div>

        <div v-if="recallList.length === 0" class="text-center py-16 bg-white rounded-3xl border border-slate-200 font-mono text-slate-400 text-xs">
          Nenhum paciente na janela de retorno de 1 ano para a loja ativa hoje.
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div
            v-for="cli in recallList"
            :key="cli.id || cli.Id"
            class="bg-white rounded-3xl border border-slate-200 p-5 shadow-xs hover:border-slate-300 transition flex flex-col justify-between space-y-4"
          >
            <div>
              <div class="flex justify-between items-start">
                <div>
                  <h3 class="font-black text-slate-900 text-sm leading-tight">{{ cli.nome || cli.Nome }}</h3>
                  <p class="text-xs text-slate-500 font-mono mt-0.5">{{ cli.telefone || cli.Telefone || 'Sem telefone' }}</p>
                </div>
                <span class="bg-amber-50 text-amber-800 text-[10px] font-mono font-bold px-2 py-0.5 rounded-lg border border-amber-200">
                  {{ cli.diasPassados || cli.DiasPassados }} dias
                </span>
              </div>

              <div class="mt-3 pt-3 border-t border-slate-100 space-y-1 text-xs text-slate-600 font-mono">
                <p><b>Data Exame:</b> {{ cli.dataExame || cli.DataExame }}</p>
                <p v-if="cli.medicoNome"><b>Médico:</b> {{ cli.medicoNome }}</p>
                <p v-if="cli.ultimoOdEsferico"><b>Último Grau:</b> OD: {{ cli.ultimoOdEsferico }} | OE: {{ cli.ultimoOeEsferico }}</p>
              </div>
            </div>

            <button
              @click="abrirModalDisparo(cli, 'recall')"
              class="w-full bg-emerald-600 hover:bg-emerald-700 text-white font-mono font-bold py-2.5 px-4 rounded-2xl text-xs flex items-center justify-center gap-2 shadow-xs transition"
            >
              <span>💬 Disparar Aviso de 1 Ano</span>
            </button>
          </div>
        </div>
      </div>

      <!-- ABA 2: PÓS-VENDA MULTIFOCAL & ADAPTAÇÃO -->
      <div v-show="abaAtiva === 'posvenda'" class="space-y-4">
        <div class="bg-teal-50 border border-teal-200 p-4 rounded-2xl text-teal-950 text-xs flex items-center gap-3">
          <span class="text-2xl">👓</span>
          <div>
            <p class="font-bold">Acompanhamento de Adaptação:</p>
            <p class="text-[11px] text-teal-800">Clientes que retiraram os óculos entre 7 e 45 dias atrás. Cheque o conforto visual e convide para ajustes de plaqueta e hastes.</p>
          </div>
        </div>

        <div v-if="posVendaList.length === 0" class="text-center py-16 bg-white rounded-3xl border border-slate-200 font-mono text-slate-400 text-xs">
          Nenhum paciente na janela de pós-venda hoje.
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div
            v-for="cli in posVendaList"
            :key="cli.id || cli.Id"
            class="bg-white rounded-3xl border border-slate-200 p-5 shadow-xs flex flex-col justify-between space-y-4"
          >
            <div>
              <div class="flex justify-between items-start">
                <div>
                  <h3 class="font-black text-slate-900 text-sm leading-tight">{{ cli.nome || cli.Nome }}</h3>
                  <p class="text-xs text-slate-500 font-mono mt-0.5">{{ cli.telefone || cli.Telefone }}</p>
                </div>
                <span class="bg-teal-50 text-teal-800 text-[10px] font-mono font-bold px-2 py-0.5 rounded-lg border border-teal-200">
                  Entregue há {{ cli.diasEntrega || cli.DiasEntrega }}d
                </span>
              </div>
              <p class="mt-3 pt-3 border-t border-slate-100 text-xs text-slate-600 font-mono">
                <b>OS #{{ cli.numeroOS || cli.NumeroOS }}</b> • Retirado em {{ cli.dataEntrega || cli.DataEntrega }}
              </p>
            </div>

            <button
              @click="abrirModalDisparo(cli, 'posvenda')"
              class="w-full bg-teal-600 hover:bg-teal-700 text-white font-mono font-bold py-2.5 px-4 rounded-2xl text-xs flex items-center justify-center gap-2 shadow-xs transition"
            >
              <span>💬 Disparar Checagem de Adaptação</span>
            </button>
          </div>
        </div>
      </div>

      <!-- ABA 3: ANIVERSARIANTES -->
      <div v-show="abaAtiva === 'aniversariantes'" class="space-y-4">
        <div class="bg-pink-50 border border-pink-200 p-4 rounded-2xl text-pink-950 text-xs flex items-center gap-3">
          <span class="text-2xl">🎂</span>
          <div>
            <p class="font-bold">Aniversariantes do Mês Atual:</p>
            <p class="text-[11px] text-pink-800">Envie um cumprimento caloroso e ofereça uma condição especial na compra de solares ou lentes.</p>
          </div>
        </div>

        <div v-if="aniversariantesList.length === 0" class="text-center py-16 bg-white rounded-3xl border border-slate-200 font-mono text-slate-400 text-xs">
          Nenhum aniversariante registrado neste mês.
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div
            v-for="cli in aniversariantesList"
            :key="cli.id || cli.Id"
            class="bg-white rounded-3xl border border-slate-200 p-5 shadow-xs flex flex-col justify-between space-y-4"
          >
            <div>
              <div class="flex justify-between items-start">
                <div>
                  <h3 class="font-black text-slate-900 text-sm leading-tight">{{ cli.nome || cli.Nome }}</h3>
                  <p class="text-xs text-slate-500 font-mono mt-0.5">{{ cli.telefone || cli.Telefone }}</p>
                </div>
                <span :class="cli.ehHoje || cli.EhHoje ? 'bg-pink-600 text-white animate-pulse' : 'bg-pink-50 text-pink-800 border-pink-200'" class="text-[10px] font-mono font-bold px-2 py-0.5 rounded-lg border">
                  {{ cli.ehHoje || cli.EhHoje ? '🎉 É HOJE!' : `Dia ${cli.diaAniversario || cli.DiaAniversario}` }}
                </span>
              </div>
              <p class="mt-3 pt-3 border-t border-slate-100 text-xs text-slate-600 font-mono">
                Nascimento: {{ cli.dataNascimento || cli.DataNascimento }}
              </p>
            </div>

            <button
              @click="abrirModalDisparo(cli, 'aniversario')"
              class="w-full bg-pink-600 hover:bg-pink-700 text-white font-mono font-bold py-2.5 px-4 rounded-2xl text-xs flex items-center justify-center gap-2 shadow-xs transition"
            >
              <span>🎉 Disparar Parabéns no WhatsApp</span>
            </button>
          </div>
        </div>
      </div>

      <!-- MODAL DE PREVIEW E DISPARO DE WHATSAPP -->
      <div v-if="modalDisparoAberta" class="fixed inset-0 bg-slate-950/70 backdrop-blur-xs z-50 flex items-center justify-center p-4 animate-fadeIn">
        <div class="bg-white rounded-3xl border border-slate-200 shadow-2xl max-w-lg w-full p-6 space-y-5">
          <div class="flex items-center justify-between border-b border-slate-100 pb-3">
            <h3 class="text-base font-bold text-slate-950 font-mono flex items-center gap-2">
              <span class="text-xl">💬</span> Disparo de WhatsApp
            </h3>
            <button @click="modalDisparoAberta = false" class="text-slate-400 hover:text-slate-800 font-bold">✕</button>
          </div>

          <div v-if="clienteDisparo" class="space-y-4 text-xs">
            <div class="p-3 bg-slate-50 rounded-2xl border border-slate-200 space-y-1">
              <p><b>Paciente:</b> {{ clienteDisparo.nome || clienteDisparo.Nome }}</p>
              <p><b>WhatsApp:</b> {{ clienteDisparo.telefone || clienteDisparo.Telefone || 'Sem telefone informado' }}</p>
            </div>

            <div>
              <label class="block font-bold uppercase text-slate-500 text-[10px] mb-1">Mensagem a Ser Enviada (editável)</label>
              <textarea
                v-model="mensagemDisparoEditavel"
                rows="5"
                class="w-full rounded-2xl border-slate-200 text-xs p-3 font-mono bg-white focus:border-teal-500 outline-none"
              ></textarea>
            </div>

            <div class="flex justify-end gap-2 pt-3 border-t border-slate-100">
              <button @click="modalDisparoAberta = false" class="px-4 py-2 rounded-xl text-slate-600 font-bold hover:bg-slate-100">
                Cancelar
              </button>
              <button 
                @click="confirmarEnvioWhatsApp" 
                class="px-5 py-2.5 rounded-xl bg-emerald-600 hover:bg-emerald-700 text-white font-bold transition shadow-sm flex items-center gap-2"
              >
                <span>💬 Abrir WhatsApp e Disparar</span>
              </button>
            </div>
          </div>
        </div>
      </div>

    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { ref, computed } from 'vue'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  ListaRecall: Array, listaRecall: Array,
  RecallClientes: Array, recallClientes: Array,
  ListaPosVenda: Array, listaPosVenda: Array,
  PosVendaClientes: Array, posVendaClientes: Array,
  ListaAniversariantes: Array, listaAniversariantes: Array,
  Aniversariantes: Array, aniversariantes: Array,
  NomeLoja: String, nomeLoja: String
})

const abaAtiva = ref('recall')

const recallList = computed(() => props.ListaRecall || props.listaRecall || props.RecallClientes || props.recallClientes || [])
const posVendaList = computed(() => props.ListaPosVenda || props.listaPosVenda || props.PosVendaClientes || props.posVendaClientes || [])
const aniversariantesList = computed(() => props.ListaAniversariantes || props.listaAniversariantes || props.Aniversariantes || props.aniversariantes || [])

const totalContatos = computed(() => {
  return recallList.value.length + posVendaList.value.length + aniversariantesList.value.length
})

// Modal de Disparo
const modalDisparoAberta = ref(false)
const clienteDisparo = ref(null)
const tipoDisparo = ref('recall')
const mensagemDisparoEditavel = ref('')

const abrirModalDisparo = (cli, tipo) => {
  clienteDisparo.value = cli
  tipoDisparo.value = tipo

  const primeiroNome = (cli.nome || cli.Nome || '').split(' ')[0]
  const nomeLoja = props.NomeLoja || props.nomeLoja || 'Ótica Retina'

  if (tipo === 'recall') {
    mensagemDisparoEditavel.value = cli.mensagemWhatsapp || cli.MensagemWhatsapp || `Olá, ${primeiroNome}! Tudo bem? Passando para lembrar que já faz cerca de 1 ano desde sua última consulta de vista. Cuidar da sua saúde visual é muito importante! Que tal agendarmos uma nova avaliação na ${nomeLoja}?`
  } else if (tipo === 'posvenda') {
    const numOs = cli.numeroOS || cli.NumeroOS || ''
    mensagemDisparoEditavel.value = cli.mensagemWhatsapp || cli.MensagemWhatsapp || `Olá, ${primeiroNome}! Tudo bem? Aqui é da ${nomeLoja}. Como está sendo sua adaptação com seus novos óculos (OS #${numOs})? Caso precise de qualquer ajuste nas plaquetas ou hastes, estamos à sua inteira disposição!`
  } else if (tipo === 'aniversario') {
    mensagemDisparoEditavel.value = `Parabéns ${primeiroNome}! 🎉 Toda a equipe da ${nomeLoja} deseja a você um feliz aniversário com muita saúde e alegria! Preparamos um presente especial para comemorar junto com você na sua próxima visita à loja.`
  }

  modalDisparoAberta.value = true
}

const confirmarEnvioWhatsApp = () => {
  if (!clienteDisparo.value) return
  const tel = (clienteDisparo.value.telefone || clienteDisparo.value.Telefone || '').replace(/\D/g, '')
  if (!tel) {
    alert('Cliente sem telefone cadastrado.')
    return
  }

  const url = `https://api.whatsapp.com/send?phone=55${tel}&text=${encodeURIComponent(mensagemDisparoEditavel.value)}`
  window.open(url, '_blank')
  modalDisparoAberta.value = false
}
</script>
