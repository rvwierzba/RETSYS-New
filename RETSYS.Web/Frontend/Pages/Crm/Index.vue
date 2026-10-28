<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-7xl mx-auto">
      
      <!-- Cabeçalho -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
        <div>
          <div class="flex items-center gap-2">
            <span class="bg-teal-500/10 text-teal-700 px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider flex items-center gap-1.5">
              <span class="w-2 h-2 rounded-full bg-teal-500 animate-pulse"></span>
              CRM &amp; Fidelização Óptica
            </span>
          </div>
          <h1 class="text-2xl font-black text-slate-900 mt-2">Central de Relacionamento com o Cliente</h1>
          <p class="text-sm text-slate-500">Aumente o faturamento com lembretes automáticos de consulta, pós-venda multifocal e aniversários.</p>
        </div>

        <div class="flex items-center gap-3">
          <div class="text-right">
            <p class="text-xs text-slate-400 font-medium">Total de Oportunidades</p>
            <p class="text-2xl font-black text-slate-900">{{ totalContatos }}</p>
          </div>
        </div>
      </div>

      <!-- Abas de Navegação -->
      <div class="flex flex-wrap border-b border-slate-200 gap-2">
        <button
          @click="abaAtiva = 'recall'"
          :class="[
            'px-5 py-3 text-sm font-bold rounded-t-xl transition flex items-center gap-2 border-b-2',
            abaAtiva === 'recall'
              ? 'border-teal-600 text-teal-700 bg-white shadow-sm'
              : 'border-transparent text-slate-500 hover:text-slate-800 hover:bg-slate-50'
          ]"
        >
          <span>👁️ Recall 1 Ano (Renovação de Grau)</span>
          <span class="bg-teal-100 text-teal-800 text-xs px-2 py-0.5 rounded-full font-black">
            {{ recallList.length }}
          </span>
        </button>

        <button
          @click="abaAtiva = 'posvenda'"
          :class="[
            'px-5 py-3 text-sm font-bold rounded-t-xl transition flex items-center gap-2 border-b-2',
            abaAtiva === 'posvenda'
              ? 'border-indigo-600 text-indigo-700 bg-white shadow-sm'
              : 'border-transparent text-slate-500 hover:text-slate-800 hover:bg-slate-50'
          ]"
        >
          <span>👓 Pós-Venda &amp; Adaptação (15 a 30 dias)</span>
          <span class="bg-indigo-100 text-indigo-800 text-xs px-2 py-0.5 rounded-full font-black">
            {{ posVendaList.length }}
          </span>
        </button>

        <button
          @click="abaAtiva = 'aniversariantes'"
          :class="[
            'px-5 py-3 text-sm font-bold rounded-t-xl transition flex items-center gap-2 border-b-2',
            abaAtiva === 'aniversariantes'
              ? 'border-amber-600 text-amber-700 bg-white shadow-sm'
              : 'border-transparent text-slate-500 hover:text-slate-800 hover:bg-slate-50'
          ]"
        >
          <span>🎂 Aniversariantes do Mês</span>
          <span class="bg-amber-100 text-amber-800 text-xs px-2 py-0.5 rounded-full font-black">
            {{ aniversariantesList.length }}
          </span>
        </button>
      </div>

      <!-- Conteúdo da Aba: RECALL 1 ANO -->
      <div v-if="abaAtiva === 'recall'" class="space-y-4">
        <div class="bg-teal-50 border border-teal-200 p-4 rounded-xl text-teal-900 text-sm flex items-start gap-3">
          <div class="text-xl">💡</div>
          <div>
            <p class="font-bold">Estratégia de Retorno:</p>
            <p class="text-xs text-teal-800 mt-0.5">Estes clientes fizeram óculos há cerca de 1 ano. 72% dos usuários de óculos renovam a receita quando contatados proativamente pela ótica.</p>
          </div>
        </div>

        <div v-if="recallList.length === 0" class="text-center py-16 bg-white rounded-2xl border border-slate-200">
          <p class="text-slate-400 font-medium">Nenhum cliente na janela de recall de 1 ano hoje.</p>
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div
            v-for="cli in recallList"
            :key="cli.Id || cli.id"
            class="bg-white rounded-2xl border border-slate-200 p-5 shadow-sm hover:shadow-md transition flex flex-col justify-between space-y-4"
          >
            <div>
              <div class="flex justify-between items-start">
                <div>
                  <h3 class="font-black text-slate-900 text-base">{{ cli.Nome || cli.nome }}</h3>
                  <p class="text-xs text-slate-500">{{ cli.Telefone || cli.telefone || 'Sem telefone' }}</p>
                </div>
                <span class="bg-teal-50 text-teal-700 text-[10px] font-bold px-2 py-1 rounded-md border border-teal-200">
                  {{ cli.DiasDesdeUltimaCompra || cli.diasDesdeUltimaCompra }} dias
                </span>
              </div>

              <div class="mt-4 pt-3 border-t border-slate-100 space-y-1.5 text-xs text-slate-600">
                <p><span class="font-bold text-slate-700">Última Compra:</span> {{ cli.UltimaCompra || cli.ultimaCompra || 'Não informado' }}</p>
                <p><span class="font-bold text-slate-700">Último Modelo:</span> {{ cli.UltimoModelo || cli.ultimoModelo || 'Óculos Completo' }}</p>
                <p v-if="cli.Medico" class="text-[11px] text-slate-500"><span class="font-bold">Especialista:</span> {{ cli.Medico || cli.medico }}</p>
              </div>
            </div>

            <div class="pt-2">
              <a
                :href="gerarLinkWhatsApp(cli.Telefone || cli.telefone, cli.Nome || cli.nome, 'recall', cli.UltimaCompra || cli.ultimaCompra)"
                target="_blank"
                class="w-full bg-emerald-600 hover:bg-emerald-700 text-white font-bold py-2.5 px-4 rounded-xl text-xs flex items-center justify-center gap-2 shadow-sm transition"
              >
                <span>💬 Chamar no WhatsApp (1-Clique)</span>
              </a>
            </div>
          </div>
        </div>
      </div>

      <!-- Conteúdo da Aba: PÓS-VENDA ADAPTAÇÃO -->
      <div v-if="abaAtiva === 'posvenda'" class="space-y-4">
        <div class="bg-indigo-50 border border-indigo-200 p-4 rounded-xl text-indigo-900 text-sm flex items-start gap-3">
          <div class="text-xl">🎯</div>
          <div>
            <p class="font-bold">Acompanhamento de Adaptação:</p>
            <p class="text-xs text-indigo-800 mt-0.5">Clientes que retiraram óculos nos últimos 15 a 30 dias. Excelente momento para checar adaptação de lentes multifocais, antirreflexo e ajustes de plaqueta.</p>
          </div>
        </div>

        <div v-if="posVendaList.length === 0" class="text-center py-16 bg-white rounded-2xl border border-slate-200">
          <p class="text-slate-400 font-medium">Nenhum cliente na fila de pós-venda nos últimos 30 dias.</p>
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div
            v-for="cli in posVendaList"
            :key="cli.Id || cli.id"
            class="bg-white rounded-2xl border border-slate-200 p-5 shadow-sm hover:shadow-md transition flex flex-col justify-between space-y-4"
          >
            <div>
              <div class="flex justify-between items-start">
                <div>
                  <h3 class="font-black text-slate-900 text-base">{{ cli.Nome || cli.nome }}</h3>
                  <p class="text-xs text-slate-500">{{ cli.Telefone || cli.telefone }}</p>
                </div>
                <span class="bg-indigo-50 text-indigo-700 text-[10px] font-bold px-2 py-1 rounded-md border border-indigo-200">
                  {{ cli.DiasDesdeEntrega || cli.diasDesdeEntrega }} dias de uso
                </span>
              </div>

              <div class="mt-4 pt-3 border-t border-slate-100 space-y-1.5 text-xs text-slate-600">
                <p><span class="font-bold text-slate-700">OS:</span> {{ cli.NumeroOS || cli.numeroOS }}</p>
                <p><span class="font-bold text-slate-700">Lentes:</span> {{ cli.Lente || cli.lente }}</p>
              </div>
            </div>

            <div class="pt-2">
              <a
                :href="gerarLinkWhatsApp(cli.Telefone || cli.telefone, cli.Nome || cli.nome, 'posvenda', cli.Lente || cli.lente)"
                target="_blank"
                class="w-full bg-emerald-600 hover:bg-emerald-700 text-white font-bold py-2.5 px-4 rounded-xl text-xs flex items-center justify-center gap-2 shadow-sm transition"
              >
                <span>💬 Checar Adaptação no WhatsApp</span>
              </a>
            </div>
          </div>
        </div>
      </div>

      <!-- Conteúdo da Aba: ANIVERSARIANTES -->
      <div v-if="abaAtiva === 'aniversariantes'" class="space-y-4">
        <div class="bg-amber-50 border border-amber-200 p-4 rounded-xl text-amber-900 text-sm flex items-start gap-3">
          <div class="text-xl">🎉</div>
          <div>
            <p class="font-bold">Fidelização de Aniversário:</p>
            <p class="text-xs text-amber-800 mt-0.5">Envie parabéns e presenteie o cliente com um voucher ou brinde (ex: kit limpa-lentes ou 15% de desconto para armações solares).</p>
          </div>
        </div>

        <div v-if="aniversariantesList.length === 0" class="text-center py-16 bg-white rounded-2xl border border-slate-200">
          <p class="text-slate-400 font-medium">Nenhum aniversariante cadastrado com data neste mês.</p>
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div
            v-for="cli in aniversariantesList"
            :key="cli.Id || cli.id"
            class="bg-white rounded-2xl border border-slate-200 p-5 shadow-sm hover:shadow-md transition flex flex-col justify-between space-y-4"
          >
            <div>
              <div class="flex justify-between items-start">
                <div>
                  <h3 class="font-black text-slate-900 text-base">{{ cli.Nome || cli.nome }}</h3>
                  <p class="text-xs text-slate-500">{{ cli.Telefone || cli.telefone }}</p>
                </div>
                <span class="bg-amber-100 text-amber-800 text-[10px] font-black px-2.5 py-1 rounded-md">
                  🎂 {{ cli.DiaMesAniversario || cli.diaMesAniversario }}
                </span>
              </div>

              <div class="mt-4 pt-3 border-t border-slate-100 space-y-1.5 text-xs text-slate-600">
                <p><span class="font-bold text-slate-700">Data Nasc.:</span> {{ cli.DataNascimento || cli.dataNascimento }}</p>
                <p><span class="font-bold text-slate-700">Cidade:</span> {{ cli.Cidade || cli.cidade || 'Local' }}</p>
              </div>
            </div>

            <div class="pt-2">
              <a
                :href="gerarLinkWhatsApp(cli.Telefone || cli.telefone, cli.Nome || cli.nome, 'aniversario')"
                target="_blank"
                class="w-full bg-emerald-600 hover:bg-emerald-700 text-white font-bold py-2.5 px-4 rounded-xl text-xs flex items-center justify-center gap-2 shadow-sm transition"
              >
                <span>🎁 Enviar Felicitações + Cupom</span>
              </a>
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
  Recall1Ano: Array,
  recall1Ano: Array,
  PosVenda: Array,
  posVenda: Array,
  Aniversariantes: Array,
  aniversariantes: Array
})

const abaAtiva = ref('recall')

const recallList = computed(() => props.Recall1Ano || props.recall1Ano || [])
const posVendaList = computed(() => props.PosVenda || props.posVenda || [])
const aniversariantesList = computed(() => props.Aniversariantes || props.aniversariantes || [])

const totalContatos = computed(() => recallList.value.length + posVendaList.value.length + aniversariantesList.value.length)

const gerarLinkWhatsApp = (telefone, nome, tipo, extraInfo = '') => {
  if (!telefone) return '#'
  
  // Limpar telefone (apenas números)
  const telLimpo = telefone.replace(/\D/g, '')
  const telCompleto = telLimpo.startsWith('55') ? telLimpo : `55${telLimpo}`
  const primeiroNome = nome.split(' ')[0]

  let mensagem = ''

  if (tipo === 'recall') {
    mensagem = `Olá, ${primeiroNome}! Tudo bem? 😊 Aqui é da Ótica. Notamos que faz cerca de 1 ano desde a sua última consulta visual conosco (${extraInfo}). A saúde dos seus olhos é muito importante! Que tal agendarmos uma nova revisão para checar se o seu grau mudou? Temos condições especiais para a sua renovação!`
  } else if (tipo === 'posvenda') {
    mensagem = `Olá, ${primeiroNome}! Tudo bem? Passando para saber como está sendo a sua experiência com os seus novos óculos! Está tudo confortável com a sua visão e adaptação? Se precisar de qualquer ajuste na armação ou tiver qualquer dúvida, estamos sempre à sua disposição!`
  } else if (tipo === 'aniversario') {
    mensagem = `Parabéns, ${primeiroNome}! 🎉🎂 Toda a equipe da nossa Ótica deseja a você muita saúde, felicidades e conquistas! Como presente especial de aniversário, preparamos um cupom de desconto exclusivo para você e sua família. Venha nos visitar para tomar um café e retirar seu brinde!`
  }

  return `https://wa.me/${telCompleto}?text=${encodeURIComponent(mensagem)}`
}
</script>

