<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-7xl mx-auto">
      
      <!-- CABEÇALHO DO CRM ÓPTICO & RECALL (PADRÃO SSÓTICA / PWI) -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white p-6 rounded-3xl border border-slate-200 shadow-sm">
        <div>
          <div class="flex items-center gap-2">
            <span class="bg-amber-50 text-amber-800 text-[10px] font-mono font-bold px-3 py-1 rounded-full border border-amber-200 uppercase tracking-wider flex items-center gap-1.5">
              <span class="w-2 h-2 rounded-full bg-amber-500 animate-pulse"></span>
              CRM Óptico &amp; Fidelização de Pacientes
            </span>
          </div>
          <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight mt-1.5">Central de Recall &amp; Retorno Anual</h1>
          <p class="text-xs text-slate-500">Recupere pacientes após 1 ano da receita, fidelize com pós-venda multifocal e celebre aniversários no WhatsApp.</p>
        </div>

        <div class="flex items-center gap-3 font-mono text-xs">
          <div class="text-right bg-slate-50 p-3 rounded-2xl border border-slate-200">
            <span class="text-[10px] text-slate-400 font-bold block uppercase">Oportunidades</span>
            <span class="text-xl font-black text-teal-800">{{ totalContatos }} contatos</span>
          </div>
        </div>
      </div>

      <!-- ABAS DE NAVEGAÇÃO SEGMENTADAS -->
      <div class="flex flex-wrap gap-2 bg-slate-100 p-1.5 rounded-2xl border border-slate-200 text-xs font-mono">
        <button
          @click="abaAtiva = 'recall'"
          :class="abaAtiva === 'recall' ? 'bg-slate-950 text-white font-black shadow-sm' : 'text-slate-600 hover:text-slate-900 font-bold'"
          class="px-4 py-2 rounded-xl transition flex items-center gap-2"
        >
          <span>👁️ Recall 1 Ano (Renovação de Grau)</span>
          <span 
            :class="abaAtiva === 'recall' ? 'bg-amber-400 text-slate-950' : 'bg-slate-200 text-slate-700'"
            class="text-[10px] px-2 py-0.5 rounded-full font-bold"
          >
            {{ recallList.length }}
          </span>
        </button>

        <button
          @click="abaAtiva = 'posvenda'"
          :class="abaAtiva === 'posvenda' ? 'bg-slate-950 text-white font-black shadow-sm' : 'text-slate-600 hover:text-slate-900 font-bold'"
          class="px-4 py-2 rounded-xl transition flex items-center gap-2"
        >
          <span>👓 Pós-Venda Multifocal (15 a 30 dias)</span>
          <span 
            :class="abaAtiva === 'posvenda' ? 'bg-teal-400 text-slate-950' : 'bg-slate-200 text-slate-700'"
            class="text-[10px] px-2 py-0.5 rounded-full font-bold"
          >
            {{ posVendaList.length }}
          </span>
        </button>

        <button
          @click="abaAtiva = 'aniversariantes'"
          :class="abaAtiva === 'aniversariantes' ? 'bg-slate-950 text-white font-black shadow-sm' : 'text-slate-600 hover:text-slate-900 font-bold'"
          class="px-4 py-2 rounded-xl transition flex items-center gap-2"
        >
          <span>🎂 Aniversariantes do Mês</span>
          <span 
            :class="abaAtiva === 'aniversariantes' ? 'bg-pink-400 text-slate-950' : 'bg-slate-200 text-slate-700'"
            class="text-[10px] px-2 py-0.5 rounded-full font-bold"
          >
            {{ aniversariantesList.length }}
          </span>
        </button>
      </div>

      <!-- ABA 1: RECALL DE 1 ANO -->
      <div v-if="abaAtiva === 'recall'" class="space-y-4">
        <div class="bg-amber-50 border border-amber-200 p-4 rounded-2xl text-amber-950 text-xs flex items-center justify-between">
          <div class="flex items-center gap-3">
            <span class="text-2xl">💡</span>
            <div>
              <p class="font-bold">Estratégia de Retorno Oftalmológico:</p>
              <p class="text-[11px] text-amber-800">Estes pacientes compraram óculos há cerca de 11 a 12 meses. O contato amigável garante o retorno para nova consulta!</p>
            </div>
          </div>
        </div>

        <div v-if="recallList.length === 0" class="text-center py-16 bg-white rounded-3xl border border-slate-200 font-mono text-slate-400 text-xs">
          Nenhum paciente na janela de retorno de 1 ano para a loja ativa hoje.
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div
            v-for="cli in recallList"
            :key="cli.Id || cli.id"
            class="bg-white rounded-3xl border border-slate-200 p-5 shadow-xs hover:border-slate-300 transition flex flex-col justify-between space-y-4"
          >
            <div>
              <div class="flex justify-between items-start">
                <div>
                  <h3 class="font-black text-slate-900 text-sm leading-tight">{{ cli.Nome || cli.nome }}</h3>
                  <p class="text-xs text-slate-500 font-mono mt-0.5">{{ cli.Telefone || cli.telefone || 'Sem telefone' }}</p>
                </div>
                <span class="bg-amber-50 text-amber-800 text-[10px] font-mono font-bold px-2 py-0.5 rounded-lg border border-amber-200">
                  {{ cli.DiasDesdeUltimaCompra || cli.diasDesdeUltimaCompra }} dias
                </span>
              </div>

              <div class="mt-3 pt-3 border-t border-slate-100 space-y-1 text-xs text-slate-600 font-mono">
                <p><b>Última Compra:</b> {{ cli.UltimaCompra || cli.ultimaCompra || 'Não informado' }}</p>
                <p><b>Modelo / Lente:</b> {{ cli.UltimoModelo || cli.ultimoModelo || 'Óculos Completo' }}</p>
                <p v-if="cli.Medico" class="text-[11px] text-slate-400"><b>Médico:</b> {{ cli.Medico || cli.medico }}</p>
              </div>
            </div>

            <a
              :href="gerarLinkWhatsApp(cli.Telefone || cli.telefone, cli.Nome || cli.nome, 'recall', cli.UltimaCompra || cli.ultimaCompra)"
              target="_blank"
              class="w-full bg-emerald-600 hover:bg-emerald-700 text-white font-mono font-bold py-2.5 px-4 rounded-2xl text-xs flex items-center justify-center gap-2 shadow-xs transition"
            >
              <span>💬 Convidar no WhatsApp (1-Clique)</span>
            </a>
          </div>
        </div>
      </div>

      <!-- ABA 2: PÓS-VENDA MULTIFOCAL -->
      <div v-if="abaAtiva === 'posvenda'" class="space-y-4">
        <div class="bg-teal-50 border border-teal-200 p-4 rounded-2xl text-teal-950 text-xs flex items-center gap-3">
          <span class="text-2xl">👓</span>
          <div>
            <p class="font-bold">Acompanhamento de Adaptação:</p>
            <p class="text-[11px] text-teal-800">Clientes que retiraram os óculos há 15 a 30 dias. Cheque o conforto visual e convide para ajustes de plaqueta.</p>
          </div>
        </div>

        <div v-if="posVendaList.length === 0" class="text-center py-16 bg-white rounded-3xl border border-slate-200 font-mono text-slate-400 text-xs">
          Nenhum paciente na janela de pós-venda hoje.
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div
            v-for="cli in posVendaList"
            :key="cli.Id || cli.id"
            class="bg-white rounded-3xl border border-slate-200 p-5 shadow-xs flex flex-col justify-between space-y-4"
          >
            <div>
              <div class="flex justify-between items-start">
                <div>
                  <h3 class="font-black text-slate-900 text-sm leading-tight">{{ cli.Nome || cli.nome }}</h3>
                  <p class="text-xs text-slate-500 font-mono mt-0.5">{{ cli.Telefone || cli.telefone }}</p>
                </div>
                <span class="bg-teal-50 text-teal-800 text-[10px] font-mono font-bold px-2 py-0.5 rounded-lg border border-teal-200">
                  Entregue há {{ cli.DiasDesdeEntrega || cli.diasDesdeEntrega }}d
                </span>
              </div>
              <p class="mt-3 pt-3 border-t border-slate-100 text-xs text-slate-600 font-mono">
                <b>OS #{{ cli.NumeroOS || cli.numeroOS }}</b> • {{ cli.Modelo || cli.modelo }}
              </p>
            </div>

            <a
              :href="gerarLinkWhatsApp(cli.Telefone || cli.telefone, cli.Nome || cli.nome, 'posvenda')"
              target="_blank"
              class="w-full bg-teal-600 hover:bg-teal-700 text-white font-mono font-bold py-2.5 px-4 rounded-2xl text-xs flex items-center justify-center gap-2 shadow-xs transition"
            >
              <span>💬 Checar Adaptação no WhatsApp</span>
            </a>
          </div>
        </div>
      </div>

      <!-- ABA 3: ANIVERSARIANTES -->
      <div v-if="abaAtiva === 'aniversariantes'" class="space-y-4">
        <div class="bg-pink-50 border border-pink-200 p-4 rounded-2xl text-pink-950 text-xs flex items-center gap-3">
          <span class="text-2xl">🎂</span>
          <div>
            <p class="font-bold">Aniversariantes do Mês:</p>
            <p class="text-[11px] text-pink-800">Envie mensagens de parabéns com cupom de presente para atrair o cliente para uma nova compra.</p>
          </div>
        </div>

        <div v-if="aniversariantesList.length === 0" class="text-center py-16 bg-white rounded-3xl border border-slate-200 font-mono text-slate-400 text-xs">
          Nenhum aniversariante cadastrado com data no mês vigente.
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div
            v-for="cli in aniversariantesList"
            :key="cli.Id || cli.id"
            class="bg-white rounded-3xl border border-slate-200 p-5 shadow-xs flex flex-col justify-between space-y-4"
          >
            <div>
              <div class="flex justify-between items-start">
                <div>
                  <h3 class="font-black text-slate-900 text-sm leading-tight">{{ cli.Nome || cli.nome }}</h3>
                  <p class="text-xs text-slate-500 font-mono mt-0.5">{{ cli.Telefone || cli.telefone }}</p>
                </div>
                <span class="bg-pink-50 text-pink-800 text-[10px] font-mono font-bold px-2 py-0.5 rounded-lg border border-pink-200">
                  🎉 Dia {{ cli.DiaAniversario || cli.diaAniversario }}
                </span>
              </div>
            </div>

            <a
              :href="gerarLinkWhatsApp(cli.Telefone || cli.telefone, cli.Nome || cli.nome, 'aniversario')"
              target="_blank"
              class="w-full bg-emerald-600 hover:bg-emerald-700 text-white font-mono font-bold py-2.5 px-4 rounded-2xl text-xs flex items-center justify-center gap-2 shadow-xs transition"
            >
              <span>💬 Enviar Parabéns no WhatsApp</span>
            </a>
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
  RecallClientes: Array, recallClientes: Array,
  PosVendaClientes: Array, posVendaClientes: Array,
  Aniversariantes: Array, aniversariantes: Array,
  NomeLoja: String, nomeLoja: String
})

const abaAtiva = ref('recall')

const recallList = computed(() => props.RecallClientes || props.recallClientes || [])
const posVendaList = computed(() => props.PosVendaClientes || props.posVendaClientes || [])
const aniversariantesList = computed(() => props.Aniversariantes || props.aniversariantes || [])

const totalContatos = computed(() => {
  return recallList.value.length + posVendaList.value.length + aniversariantesList.value.length
})

const gerarLinkWhatsApp = (tel, nome, tipo, ultimaData = '') => {
  const clean = (tel || '').replace(/\D/g, '')
  const primeiroNome = (nome || '').split(' ')[0]
  let msg = ''

  if (tipo === 'recall') {
    msg = `Olá ${primeiroNome}, tudo bem? Aqui é da Ótica Retina. Faz cerca de 1 ano desde a sua última receita de óculos. A saúde da sua visão é fundamental: que tal agendar um retorno com seu oftalmologista para renovar seus graus? Estamos à sua disposição!`
  } else if (tipo === 'posvenda') {
    msg = `Olá ${primeiroNome}! Aqui é da Ótica Retina. Como você está se adaptando aos seus novos óculos? Se precisar de qualquer ajuste gratuito nas plaquetas ou hastes, estamos de portas abertas!`
  } else if (tipo === 'aniversario') {
    msg = `Parabéns ${primeiroNome}! Toda a equipe da Ótica Retina deseja a você um feliz aniversário, muita saúde e sucesso! Preparamos um presente especial para você na sua próxima visita à loja.`
  }

  return `https://api.whatsapp.com/send?phone=55${clean}&text=${encodeURIComponent(msg)}`
}
</script>
