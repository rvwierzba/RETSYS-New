<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-5xl mx-auto">

      <!-- CABEÇALHO DO FLUXO DE EMISSÃO -->
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 bg-white p-6 rounded-3xl border border-slate-200 shadow-sm">
        <div>
          <div class="flex items-center gap-2">
            <span class="w-2.5 h-2.5 rounded-full bg-teal-500"></span>
            <h1 class="text-2xl font-black text-slate-950 font-mono tracking-tight">Nova Ordem de Serviço (O.S.)</h1>
          </div>
          <p class="text-xs text-slate-500 mt-1">Fluxo passo a passo: Identificação, Refração Clínica, Produtos e Fechamento Financeiro.</p>
        </div>

        <div class="flex items-center gap-2 font-mono text-xs">
          <span class="px-3 py-1.5 rounded-xl bg-slate-100 text-slate-700 font-bold border border-slate-200">
            Nº OS: <b class="text-teal-700">{{ form.numeroOS }}</b>
          </span>
          <Link href="/ordens" class="px-3.5 py-1.5 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-600 font-bold transition">
            Cancelar
          </Link>
        </div>
      </div>

      <!-- PROGRESSO DAS ETAPAS (STEPPER VISUAL ESTILO SSÓTICA / PWI) -->
      <div class="grid grid-cols-4 gap-2 bg-white p-2.5 rounded-2xl border border-slate-200 text-xs font-mono">
        <button 
          type="button"
          @click="etapaAtual = 1"
          :class="etapaAtual === 1 ? 'bg-teal-600 text-white font-bold shadow-xs' : (etapaAtual > 1 ? 'bg-teal-50 text-teal-800' : 'text-slate-400 hover:text-slate-700')"
          class="py-2.5 px-3 rounded-xl transition flex items-center justify-center gap-2 text-center"
        >
          <span class="w-5 h-5 rounded-full border flex items-center justify-center text-[10px]" :class="etapaAtual === 1 ? 'border-white' : 'border-current'">1</span>
          <span class="hidden sm:inline">Cliente</span>
        </button>

        <button 
          type="button"
          @click="etapaAtual = 2"
          :class="etapaAtual === 2 ? 'bg-teal-600 text-white font-bold shadow-xs' : (etapaAtual > 2 ? 'bg-teal-50 text-teal-800' : 'text-slate-400 hover:text-slate-700')"
          class="py-2.5 px-3 rounded-xl transition flex items-center justify-center gap-2 text-center"
        >
          <span class="w-5 h-5 rounded-full border flex items-center justify-center text-[10px]" :class="etapaAtual === 2 ? 'border-white' : 'border-current'">2</span>
          <span class="hidden sm:inline">Receita</span>
        </button>

        <button 
          type="button"
          @click="etapaAtual = 3"
          :class="etapaAtual === 3 ? 'bg-teal-600 text-white font-bold shadow-xs' : (etapaAtual > 3 ? 'bg-teal-50 text-teal-800' : 'text-slate-400 hover:text-slate-700')"
          class="py-2.5 px-3 rounded-xl transition flex items-center justify-center gap-2 text-center"
        >
          <span class="w-5 h-5 rounded-full border flex items-center justify-center text-[10px]" :class="etapaAtual === 3 ? 'border-white' : 'border-current'">3</span>
          <span class="hidden sm:inline">Produtos</span>
        </button>

        <button 
          type="button"
          @click="etapaAtual = 4"
          :class="etapaAtual === 4 ? 'bg-teal-600 text-white font-bold shadow-xs' : (etapaAtual > 4 ? 'bg-teal-50 text-teal-800' : 'text-slate-400 hover:text-slate-700')"
          class="py-2.5 px-3 rounded-xl transition flex items-center justify-center gap-2 text-center"
        >
          <span class="w-5 h-5 rounded-full border flex items-center justify-center text-[10px]" :class="etapaAtual === 4 ? 'border-white' : 'border-current'">4</span>
          <span class="hidden sm:inline">Pagamento</span>
        </button>
      </div>

      <!-- FORMULÁRIO OPERACIONAL -->
      <form @submit.prevent="salvarOrdemServico" class="space-y-6">

        <!-- ETAPA 1: DADOS DO CLIENTE & ATENDIMENTO -->
        <div v-show="etapaAtual === 1" class="bg-white p-6 sm:p-8 rounded-3xl border border-slate-200 shadow-sm space-y-6 animate-fadeIn">
          <div class="border-b border-slate-100 pb-3">
            <h3 class="text-base font-black text-slate-900 font-mono flex items-center gap-2">
              <span>👤</span> 1. Identificação do Cliente &amp; Atendente
            </h3>
            <p class="text-xs text-slate-400">Consulte pelo CPF para preenchimento automático de cadastros existentes.</p>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <!-- Número da OS -->
            <div>
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Número da OS *</label>
              <input 
                v-model="form.numeroOS" 
                type="text" 
                class="w-full rounded-2xl border-slate-200 text-xs font-mono font-bold bg-slate-50 focus:border-teal-500" 
                required 
              />
            </div>

            <!-- Vendedora / Atendente -->
            <div>
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Atendente / Responsável</label>
              <select v-model="form.vendedorId" class="w-full rounded-2xl border-slate-200 text-xs font-bold bg-slate-50 focus:border-teal-500">
                <option v-for="v in (vendedores || Vendedores || [])" :key="v.id || v.Id" :value="v.id || v.Id">
                  👩‍💼 {{ v.nome || v.Nome }}
                </option>
              </select>
            </div>

            <!-- Busca por CPF -->
            <div>
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Buscar por CPF</label>
              <div class="flex gap-2">
                <input 
                  v-model="form.cpf" 
                  type="text" 
                  placeholder="000.000.000-00" 
                  class="w-full rounded-2xl border-slate-200 text-xs font-mono bg-slate-50 focus:border-teal-500"
                />
                <button 
                  type="button" 
                  @click="consultarCpf"
                  :disabled="buscandoCpf"
                  class="px-3 py-2 bg-slate-900 hover:bg-slate-800 text-white rounded-xl text-xs font-bold transition font-mono whitespace-nowrap"
                >
                  {{ buscandoCpf ? '...' : 'Buscar' }}
                </button>
              </div>
            </div>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <!-- Nome do Cliente -->
            <div class="md:col-span-2">
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Nome Completo do Cliente / Paciente *</label>
              <input 
                v-model="form.nome" 
                type="text" 
                placeholder="Ex: Maria das Graças Silva"
                class="w-full rounded-2xl border-slate-200 text-xs font-bold bg-slate-50 focus:border-teal-500" 
                required 
              />
            </div>

            <!-- WhatsApp / Telefone -->
            <div>
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">WhatsApp / Telefone *</label>
              <input 
                v-model="form.telefone" 
                type="text" 
                placeholder="(00) 00000-0000"
                class="w-full rounded-2xl border-slate-200 text-xs font-bold bg-slate-50 focus:border-teal-500" 
                required 
              />
            </div>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Data de Nascimento</label>
              <input 
                v-model="form.dataNascimento" 
                type="date" 
                class="w-full rounded-2xl border-slate-200 text-xs bg-slate-50 focus:border-teal-500" 
              />
            </div>

            <div>
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">E-mail</label>
              <input 
                v-model="form.email" 
                type="email" 
                placeholder="cliente@email.com"
                class="w-full rounded-2xl border-slate-200 text-xs bg-slate-50 focus:border-teal-500" 
              />
            </div>

            <div>
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Convênio / Parceria</label>
              <input 
                v-model="form.convenio" 
                type="text" 
                placeholder="Ex: Unimed, Sindicato, etc."
                class="w-full rounded-2xl border-slate-200 text-xs bg-slate-50 focus:border-teal-500" 
              />
            </div>
          </div>

          <div class="flex justify-end pt-4 border-t border-slate-100">
            <button 
              type="button" 
              @click="etapaAtual = 2"
              class="px-6 py-2.5 rounded-2xl bg-teal-600 hover:bg-teal-700 text-white font-bold text-xs shadow-sm transition flex items-center gap-1.5"
            >
              <span>Avançar para Receita ➔</span>
            </button>
          </div>
        </div>

        <!-- ETAPA 2: PRESCRIÇÃO CLÍNICA (REFRAÇÃO OFTALMOLÓGICA) -->
        <div v-show="etapaAtual === 2" class="bg-white p-6 sm:p-8 rounded-3xl border border-slate-200 shadow-sm space-y-6 animate-fadeIn">
          <div class="border-b border-slate-100 pb-3 flex items-center justify-between">
            <div>
              <h3 class="text-base font-black text-slate-900 font-mono flex items-center gap-2">
                <span>👓</span> 2. Refração Clínica &amp; Medidas Ópticas
              </h3>
              <p class="text-xs text-slate-400">Preencha a prescrição médica de Longe e Perto com cálculo automático de Adição.</p>
            </div>
            <span class="text-[10px] font-mono font-bold text-teal-700 bg-teal-50 border border-teal-200 px-2.5 py-1 rounded-full">
              Padrão Oftalmológico BR
            </span>
          </div>

          <!-- Grade de Longe e Perto (Padrão Oftalmológico BR) -->
          <div class="p-5 bg-slate-50 rounded-2xl border border-slate-200 space-y-4">
            <div class="flex items-center justify-between">
              <span class="text-xs font-mono font-bold uppercase text-slate-700 tracking-wider">
                Receituário de Óculos (Para Longe &amp; Para Perto)
              </span>
              <span class="text-[10px] font-mono font-bold text-teal-700 bg-teal-50 border border-teal-200 px-2 py-0.5 rounded-full">
                OD / OE
              </span>
            </div>

            <div class="overflow-x-auto">
              <table class="w-full text-center text-xs border-collapse">
                <thead>
                  <tr class="text-[10px] font-mono font-bold uppercase text-slate-400 border-b border-slate-200">
                    <th class="py-2.5 px-3 text-left">Visão</th>
                    <th class="py-2.5 px-3">Olho</th>
                    <th class="py-2.5 px-3">Esférico</th>
                    <th class="py-2.5 px-3">Cilíndrico</th>
                    <th class="py-2.5 px-3">Eixo (0° a 180°)</th>
                    <th class="py-2.5 px-3">DNP / DP (mm)</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-slate-200 font-mono">
                  <!-- Para Longe - OD -->
                  <tr class="bg-white">
                    <td rowspan="2" class="py-3 px-3 text-left font-black text-slate-800 bg-slate-100/60 uppercase text-[10px] tracking-wider border-r border-slate-200">
                      Para Longe
                    </td>
                    <td class="py-3 px-3 font-bold text-slate-900">OD</td>
                    <td class="py-2 px-2">
                      <input v-model.number="form.odEsferico" @input="recalcularPerto" type="number" step="0.25" placeholder="0.00" class="w-24 text-center font-bold rounded-lg border-slate-200 text-xs py-1.5 bg-white" />
                    </td>
                    <td class="py-2 px-2">
                      <input v-model.number="form.odCilindrico" type="number" step="0.25" placeholder="0.00" class="w-24 text-center font-bold text-amber-700 rounded-lg border-slate-200 text-xs py-1.5 bg-white" />
                    </td>
                    <td class="py-2 px-2">
                      <input v-model.number="form.odEixo" type="number" min="0" max="180" placeholder="0" class="w-20 text-center rounded-lg border-slate-200 text-xs py-1.5 bg-white" />
                    </td>
                    <td class="py-2 px-2">
                      <input v-model.number="form.dnpOd" @input="onDnpInput" type="number" step="0.5" placeholder="32" class="w-20 text-center rounded-lg border-slate-200 text-xs py-1.5 bg-white" />
                    </td>
                  </tr>

                  <!-- Para Longe - OE -->
                  <tr class="bg-white">
                    <td class="py-3 px-3 font-bold text-slate-900 border-t border-slate-100">OE</td>
                    <td class="py-2 px-2 border-t border-slate-100">
                      <input v-model.number="form.oeEsferico" @input="recalcularPerto" type="number" step="0.25" placeholder="0.00" class="w-24 text-center font-bold rounded-lg border-slate-200 text-xs py-1.5 bg-white" />
                    </td>
                    <td class="py-2 px-2 border-t border-slate-100">
                      <input v-model.number="form.oeCilindrico" type="number" step="0.25" placeholder="0.00" class="w-24 text-center font-bold text-amber-700 rounded-lg border-slate-200 text-xs py-1.5 bg-white" />
                    </td>
                    <td class="py-2 px-2 border-t border-slate-100">
                      <input v-model.number="form.oeEixo" type="number" min="0" max="180" placeholder="0" class="w-20 text-center rounded-lg border-slate-200 text-xs py-1.5 bg-white" />
                    </td>
                    <td class="py-2 px-2 border-t border-slate-100">
                      <input v-model.number="form.dnpOe" @input="onDnpInput" type="number" step="0.5" placeholder="32" class="w-20 text-center rounded-lg border-slate-200 text-xs py-1.5 bg-white" />
                    </td>
                  </tr>

                  <!-- Para Perto - OD -->
                  <tr class="bg-teal-50/20">
                    <td rowspan="2" class="py-3 px-3 text-left font-black text-teal-800 bg-teal-50/60 uppercase text-[10px] tracking-wider border-r border-slate-200">
                      Para Perto
                    </td>
                    <td class="py-3 px-3 font-bold text-teal-900">OD</td>
                    <td class="py-2 px-2">
                      <input v-model.number="form.odEsfericoPerto" @input="onPertoOdInput" type="number" step="0.25" placeholder="0.00" class="w-24 text-center font-bold text-teal-900 bg-teal-50/50 rounded-lg border-teal-200 text-xs py-1.5" />
                    </td>
                    <td class="py-2 px-2">
                      <input :value="form.odCilindrico" disabled placeholder="0.00" class="w-24 text-center font-bold text-slate-400 bg-slate-100/50 rounded-lg border-slate-200 text-xs py-1.5" title="Cilíndrico de perto é idêntico ao de longe" />
                    </td>
                    <td class="py-2 px-2">
                      <input :value="form.odEixo" disabled placeholder="0" class="w-20 text-center text-slate-400 bg-slate-100/50 rounded-lg border-slate-200 text-xs py-1.5" title="Eixo de perto é idêntico ao de longe" />
                    </td>
                    <td class="py-2 px-2">
                      <input :value="form.dnpOd ? Math.max(0, form.dnpOd - 2) : ''" disabled placeholder="30" class="w-20 text-center text-slate-400 bg-slate-100/50 rounded-lg border-slate-200 text-xs py-1.5" title="DNP perto (convergência)" />
                    </td>
                  </tr>

                  <!-- Para Perto - OE -->
                  <tr class="bg-teal-50/20">
                    <td class="py-3 px-3 font-bold text-teal-900 border-t border-teal-100/60">OE</td>
                    <td class="py-2 px-2 border-t border-teal-100/60">
                      <input v-model.number="form.oeEsfericoPerto" @input="onPertoOeInput" type="number" step="0.25" placeholder="0.00" class="w-24 text-center font-bold text-teal-900 bg-teal-50/50 rounded-lg border-teal-200 text-xs py-1.5" />
                    </td>
                    <td class="py-2 px-2 border-t border-teal-100/60">
                      <input :value="form.oeCilindrico" disabled placeholder="0.00" class="w-24 text-center font-bold text-slate-400 bg-slate-100/50 rounded-lg border-slate-200 text-xs py-1.5" title="Cilíndrico de perto é idêntico ao de longe" />
                    </td>
                    <td class="py-2 px-2 border-t border-teal-100/60">
                      <input :value="form.oeEixo" disabled placeholder="0" class="w-20 text-center text-slate-400 bg-slate-100/50 rounded-lg border-slate-200 text-xs py-1.5" title="Eixo de perto é idêntico ao de longe" />
                    </td>
                    <td class="py-2 px-2 border-t border-teal-100/60">
                      <input :value="form.dnpOe ? Math.max(0, form.dnpOe - 2) : ''" disabled placeholder="30" class="w-20 text-center text-slate-400 bg-slate-100/50 rounded-lg border-slate-200 text-xs py-1.5" title="DNP perto (convergência)" />
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>

            <!-- Adição, DP e Alturas -->
            <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 pt-3 border-t border-slate-200">
              <div>
                <label class="block text-[11px] font-mono font-bold uppercase text-teal-800 mb-1">
                  Adição (AD) [Perto - Longe]
                </label>
                <input 
                  v-model.number="form.adicao" 
                  @input="onAdicaoInput"
                  type="number" 
                  step="0.25" 
                  min="0" 
                  max="4.00" 
                  placeholder="+2.00"
                  class="w-full rounded-2xl border-teal-300 text-xs font-mono font-bold text-teal-900 bg-teal-50/40 focus:border-teal-500" 
                />
              </div>

              <div>
                <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">
                  DP Total (mm) [OD + OE]
                </label>
                <input 
                  v-model.number="form.dpTotal" 
                  @input="onDpTotalInput"
                  type="number" 
                  step="0.5" 
                  placeholder="Ex: 64"
                  class="w-full rounded-2xl border-slate-200 text-xs font-mono font-bold text-center bg-white" 
                />
              </div>

              <div>
                <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Altura Montagem OD / OE (mm)</label>
                <div class="grid grid-cols-2 gap-2">
                  <input v-model.number="form.alturaMontagemOd" type="number" step="0.5" placeholder="Alt OD" class="rounded-2xl border-slate-200 text-xs font-mono text-center bg-white" />
                  <input v-model.number="form.alturaMontagemOe" type="number" step="0.5" placeholder="Alt OE" class="rounded-2xl border-slate-200 text-xs font-mono text-center bg-white" />
                </div>
              </div>
            </div>

            <!-- MEDIDAS DA ARMAÇÃO & MONTAGEM TÉCNICA (LABORATÓRIO) -->
            <div class="pt-4 border-t border-slate-200 space-y-3">
              <div class="flex items-center justify-between">
                <span class="text-xs font-mono font-bold uppercase text-slate-700 tracking-wider flex items-center gap-1.5">
                  <span>📐</span> Medidas da Armação &amp; Montagem Técnica
                </span>
                <span class="text-[10px] text-slate-400 font-mono">Essilor / Hoya / Zeiss / Lab</span>
              </div>

              <div class="grid grid-cols-2 sm:grid-cols-4 lg:grid-cols-7 gap-2.5 text-xs font-mono">
                <div>
                  <label class="block text-[10px] uppercase text-slate-400 mb-1">Aro / Horiz. (A)</label>
                  <input v-model.number="form.aro" type="number" step="0.5" placeholder="54 mm" class="w-full rounded-xl border-slate-200 text-xs text-center bg-white" />
                </div>
                <div>
                  <label class="block text-[10px] uppercase text-slate-400 mb-1">Ponte / DBL (PO)</label>
                  <input v-model.number="form.po" type="number" step="0.5" placeholder="18 mm" class="w-full rounded-xl border-slate-200 text-xs text-center bg-white" />
                </div>
                <div>
                  <label class="block text-[10px] uppercase text-slate-400 mb-1">Diag. Maior (DM)</label>
                  <input v-model.number="form.dm" type="number" step="0.5" placeholder="58 mm" class="w-full rounded-xl border-slate-200 text-xs text-center bg-white" />
                </div>
                <div>
                  <label class="block text-[10px] uppercase text-slate-400 mb-1">Vertical (VERT)</label>
                  <input v-model.number="form.vert" type="number" step="0.5" placeholder="38 mm" class="w-full rounded-xl border-slate-200 text-xs text-center bg-white" />
                </div>
                <div>
                  <label class="block text-[10px] uppercase text-slate-400 mb-1">Haste (mm)</label>
                  <input v-model.number="form.haste" type="number" step="1" placeholder="140 mm" class="w-full rounded-xl border-slate-200 text-xs text-center bg-white" />
                </div>
                <div>
                  <label class="block text-[10px] uppercase text-slate-400 mb-1">CO OD (mm)</label>
                  <input v-model.number="form.coOd" type="number" step="0.5" placeholder="Centro OD" class="w-full rounded-xl border-slate-200 text-xs text-center bg-white" />
                </div>
                <div>
                  <label class="block text-[10px] uppercase text-slate-400 mb-1">CO OE (mm)</label>
                  <input v-model.number="form.coOe" type="number" step="0.5" placeholder="Centro OE" class="w-full rounded-xl border-slate-200 text-xs text-center bg-white" />
                </div>
              </div>
            </div>

            <!-- Observações da Receita -->
            <div class="pt-2 border-t border-slate-100">
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Observações Técnicas da Receita / Montagem</label>
              <textarea v-model="form.obsReceita" rows="2" placeholder="Observações específicas para o laboratório ou sobre o receituário do paciente..." class="w-full rounded-2xl border-slate-200 text-xs bg-white"></textarea>
            </div>
          </div>

          <!-- Médico e Validade -->
          <div class="grid grid-cols-1 sm:grid-cols-4 gap-4 pt-2 border-t border-slate-100">
            <div>
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Médico / Especialista</label>
              <input v-model="form.medicoNome" type="text" placeholder="Dr. Nome do Especialista" class="w-full rounded-2xl border-slate-200 text-xs bg-slate-50" />
            </div>

            <div>
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">CRM / CROO</label>
              <input v-model="form.medicoCrm" type="text" placeholder="CRM 12345 / SP" class="w-full rounded-2xl border-slate-200 text-xs bg-slate-50 font-mono" />
            </div>

            <div>
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Tipo de Profissional</label>
              <select v-model="form.medicoTipo" class="w-full rounded-2xl border-slate-200 text-xs bg-slate-50 font-bold">
                <option value="OFTALMOLOGISTA">Oftalmologista</option>
                <option value="OPTOMETRISTA">Optometrista</option>
                <option value="NAO_ESPECIFICADO">Outro Especialista</option>
              </select>
            </div>

            <div>
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Data do Exame</label>
              <input v-model="form.receitaDataExame" type="date" class="w-full rounded-2xl border-slate-200 text-xs bg-slate-50 font-mono" />
            </div>
          </div>

          <div class="flex justify-between pt-4 border-t border-slate-100">
            <button type="button" @click="etapaAtual = 1" class="px-5 py-2.5 rounded-2xl bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs transition">
              ➔ Voltar
            </button>
            <button type="button" @click="etapaAtual = 3" class="px-6 py-2.5 rounded-2xl bg-teal-600 hover:bg-teal-700 text-white font-bold text-xs shadow-sm transition">
              Avançar para Produtos ➔
            </button>
          </div>
        </div>

        <!-- ETAPA 3: PRODUTOS (ARMAÇÃO & LENTES) -->
        <div v-show="etapaAtual === 3" class="bg-white p-6 sm:p-8 rounded-3xl border border-slate-200 shadow-sm space-y-6 animate-fadeIn">
          <div class="border-b border-slate-100 pb-3">
            <h3 class="text-base font-black text-slate-900 font-mono flex items-center gap-2">
              <span>🔬</span> 3. Escolha da Armação &amp; Lentes
            </h3>
            <p class="text-xs text-slate-400">Vincule as peças do estoque físico ou descreva itens sob encomenda.</p>
          </div>

          <!-- Armação -->
          <div class="p-5 bg-slate-50 rounded-2xl border border-slate-200 space-y-4">
            <div class="flex items-center justify-between">
              <span class="text-xs font-mono font-bold uppercase text-slate-700">1. Armação de Óculos</span>
              <label class="flex items-center gap-2 text-xs cursor-pointer text-slate-600">
                <input v-model="form.armacaoPropria" type="checkbox" class="rounded text-teal-600 focus:ring-teal-500" />
                <span>Armação Própria do Cliente</span>
              </label>
            </div>

            <div v-if="!form.armacaoPropria" class="grid grid-cols-1 sm:grid-cols-3 gap-3">
              <div class="sm:col-span-2">
                <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Selecione no Estoque</label>
                <select v-model="form.armacaoId" @change="onArmacaoSelecionada" class="w-full rounded-2xl border-slate-200 text-xs bg-white font-bold">
                  <option value="">(Selecione uma armação do estoque...)</option>
                  <option v-for="a in (armacoes || Armacoes || [])" :key="a.id || a.Id" :value="a.id || a.Id">
                    {{ a.marcaNome || a.MarcaNome }} - {{ a.modeloReferencia || a.ModeloReferencia }} ({{ a.cor || a.Cor }}) - R$ {{ formatarMoeda(a.precoVenda || a.PrecoVenda) }}
                  </option>
                </select>
              </div>

              <div>
                <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Valor Armação (R$)</label>
                <input v-model.number="form.valorArmacao" type="number" step="0.01" class="w-full rounded-2xl border-slate-200 text-xs font-mono font-bold bg-white text-slate-900" />
              </div>
            </div>

            <div v-else class="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div>
                <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Descrição da Armação Própria</label>
                <input v-model="form.armacaoDescricaoManual" type="text" placeholder="Ex: Ray-Ban tartaruga trazida pelo cliente" class="w-full rounded-2xl border-slate-200 text-xs bg-white" />
              </div>
              <div>
                <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Taxa de Montagem / Manuseio (R$)</label>
                <input v-model.number="form.valorArmacao" type="number" step="0.01" placeholder="0.00" class="w-full rounded-2xl border-slate-200 text-xs font-mono bg-white" />
              </div>
            </div>
          </div>

          <!-- Lente -->
          <div class="p-5 bg-slate-50 rounded-2xl border border-slate-200 space-y-4">
            <span class="text-xs font-mono font-bold uppercase text-slate-700 block">2. Lentes Oftálmicas</span>

            <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
              <div class="sm:col-span-2">
                <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Tabela de Lentes da Rede</label>
                <select v-model="form.lentePrecoId" @change="onLenteSelecionada" class="w-full rounded-2xl border-slate-200 text-xs bg-white font-bold">
                  <option value="">(Selecione da tabela ou digite abaixo...)</option>
                  <option v-for="l in (lentes || Lentes || [])" :key="l.id || l.Id" :value="l.id || l.Id">
                    {{ l.laboratorio || l.Laboratorio }} • {{ l.tipo || l.Tipo }} ({{ l.tratamento || l.Tratamento }}) - R$ {{ formatarMoeda(l.precoVenda || l.PrecoVenda) }}
                  </option>
                </select>
              </div>

              <div>
                <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Valor Lentes (R$)</label>
                <input v-model.number="form.valorLente" type="number" step="0.01" class="w-full rounded-2xl border-slate-200 text-xs font-mono font-bold bg-white text-slate-900" />
              </div>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div>
                <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Descrição / Especificação Manual</label>
                <input v-model="form.lenteDescricaoManual" type="text" placeholder="Ex: Multifocal Digital Antirreflexo Crizal" class="w-full rounded-2xl border-slate-200 text-xs bg-white" />
              </div>
              <div>
                <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Laboratório Parceiro</label>
                <select v-model="form.laboratorioParceiro" class="w-full rounded-2xl border-slate-200 text-xs bg-white font-bold">
                  <option value="Essilor">Essilor (Varilux / Crizal)</option>
                  <option value="Hoya">Hoya (MiyoSmart / BlueControl)</option>
                  <option value="Zeiss">Zeiss (DuraVision)</option>
                  <option value="Local">Laboratório de Montagem Local</option>
                  <option value="Outro">Outro Fornecedor</option>
                </select>
              </div>
            </div>
          </div>

          <div class="flex justify-between pt-4 border-t border-slate-100">
            <button type="button" @click="etapaAtual = 2" class="px-5 py-2.5 rounded-2xl bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs transition">
              ➔ Voltar
            </button>
            <button type="button" @click="etapaAtual = 4" class="px-6 py-2.5 rounded-2xl bg-teal-600 hover:bg-teal-700 text-white font-bold text-xs shadow-sm transition">
              Avançar para Pagamento ➔
            </button>
          </div>
        </div>

        <!-- ETAPA 4: FECHAMENTO FINANCEIRO & CREDIÁRIO -->
        <div v-show="etapaAtual === 4" class="bg-white p-6 sm:p-8 rounded-3xl border border-slate-200 shadow-sm space-y-6 animate-fadeIn">
          <div class="border-b border-slate-100 pb-3">
            <h3 class="text-base font-black text-slate-900 font-mono flex items-center gap-2">
              <span>💰</span> 4. Fechamento Financeiro &amp; Carnê / Crediário
            </h3>
            <p class="text-xs text-slate-400">Totalize os valores, aplique desconto e gere o parcelamento ou carnê.</p>
          </div>

          <!-- Totalizadores em Destaque -->
          <div class="p-5 bg-teal-50/60 rounded-3xl border border-teal-200 grid grid-cols-2 sm:grid-cols-4 gap-3 text-center font-mono">
            <div class="bg-white p-3 rounded-2xl border border-teal-100">
              <span class="text-[10px] text-slate-400 uppercase font-bold block">Armação</span>
              <b class="text-slate-900 text-sm">R$ {{ formatarMoeda(form.valorArmacao) }}</b>
            </div>
            <div class="bg-white p-3 rounded-2xl border border-teal-100">
              <span class="text-[10px] text-slate-400 uppercase font-bold block">Lentes</span>
              <b class="text-slate-900 text-sm">R$ {{ formatarMoeda(form.valorLente) }}</b>
            </div>
            <div class="bg-white p-3 rounded-2xl border border-teal-100">
              <span class="text-[10px] text-amber-700 uppercase font-bold block">Desconto</span>
              <b class="text-amber-800 text-sm">R$ {{ formatarMoeda(form.descontoReais) }}</b>
            </div>
            <div class="bg-teal-900 text-white p-3 rounded-2xl shadow-sm">
              <span class="text-[10px] text-teal-300 uppercase font-bold block">Total Líquido</span>
              <b class="text-base">R$ {{ formatarMoeda(totalLiquidoCalculado) }}</b>
            </div>
          </div>

          <!-- Desconto e Entrada -->
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div>
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Desconto em Reais (R$)</label>
              <input v-model.number="form.descontoReais" type="number" step="0.01" min="0" placeholder="0.00" class="w-full rounded-2xl border-slate-200 text-xs font-mono font-bold bg-slate-50 text-amber-800" />
            </div>

            <div>
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Valor de Entrada (R$)</label>
              <input v-model.number="form.valorEntrada" type="number" step="0.01" min="0" placeholder="0.00" class="w-full rounded-2xl border-slate-200 text-xs font-mono font-bold bg-slate-50 text-emerald-800" />
            </div>

            <div>
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Saldo Restante na Entrega</label>
              <div class="h-10 px-4 rounded-2xl bg-slate-100 flex items-center font-mono font-bold text-sm" :class="saldoRestanteCalculado > 0 ? 'text-rose-700' : 'text-emerald-700'">
                R$ {{ formatarMoeda(saldoRestanteCalculado) }}
              </div>
            </div>
          </div>

          <!-- Forma de Pagamento e Parcelas -->
          <div class="p-5 bg-slate-50 rounded-2xl border border-slate-200 space-y-4">
            <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
              <div>
                <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Forma de Pagamento</label>
                <select v-model="form.formaPagamento" class="w-full rounded-2xl border-slate-200 text-xs bg-white font-bold">
                  <option value="DINHEIRO">💵 Dinheiro à Vista</option>
                  <option value="PIX">⚡ PIX</option>
                  <option value="CARTAO_CREDITO">💳 Cartão de Crédito</option>
                  <option value="CARTAO_DEBITO">💳 Cartão de Débito</option>
                  <option value="CREDIARIO_PROPRIO">📜 Crediário Próprio (Carnê)</option>
                </select>
              </div>

              <div>
                <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Quantidade de Parcelas</label>
                <select v-model.number="form.parcelas" class="w-full rounded-2xl border-slate-200 text-xs bg-white font-bold font-mono">
                  <option v-for="n in 12" :key="n" :value="n">
                    {{ n }}x de R$ {{ formatarMoeda((saldoRestanteCalculado || totalLiquidoCalculado) / n) }}
                  </option>
                </select>
              </div>

              <div>
                <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Data Prevista de Entrega</label>
                <input v-model="form.dataPrevistaEntrega" type="date" class="w-full rounded-2xl border-slate-200 text-xs bg-white font-mono" />
              </div>
            </div>

            <!-- Aviso de Carnê -->
            <div v-if="form.formaPagamento === 'CREDIARIO_PROPRIO'" class="p-3 bg-amber-50 rounded-xl border border-amber-200 text-xs text-amber-900 flex items-center gap-2">
              <span>📜</span>
              <span><b>Crediário Próprio Selecionado:</b> As lâminas de carnê (via do cliente e canhoto da loja) serão geradas automaticamente após salvar!</span>
            </div>
          </div>

          <!-- Observações Gerais -->
          <div>
            <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Observações Internas do Laboratório</label>
            <textarea v-model="form.observacoes" rows="2" placeholder="Ex: Cliente tem sensibilidade à luz; fazer montagem com chanfro fino..." class="w-full rounded-2xl border-slate-200 text-xs bg-slate-50"></textarea>
          </div>

          <!-- Botão Salvar e Emitir -->
          <div class="flex items-center justify-between pt-4 border-t border-slate-100">
            <button type="button" @click="etapaAtual = 3" class="px-5 py-2.5 rounded-2xl bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs transition">
              ➔ Voltar
            </button>

            <button 
              type="submit" 
              :disabled="salvando"
              class="px-8 py-3 rounded-2xl bg-teal-600 hover:bg-teal-700 disabled:opacity-50 text-white font-black text-xs uppercase tracking-wider shadow-lg shadow-teal-700/20 transition flex items-center gap-2"
            >
              <span v-if="salvando">Gravando Ordem...</span>
              <span v-else>💾 Finalizar e Emitir Ordem de Serviço</span>
            </button>
          </div>
        </div>

      </form>

    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { router, Link, usePage } from '@inertiajs/vue3'
import axios from 'axios'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  ProximoNumeroOS: String, proximoNumeroOS: String,
  Vendedores: Array, vendedores: Array,
  Armacoes: Array, armacoes: Array,
  Lentes: Array, lentes: Array,
  UsuarioLogadoId: String, usuarioLogadoId: String,
  UsuarioLogadoNome: String, usuarioLogadoNome: String
})

const page = usePage()
const etapaAtual = ref(1)
const salvando = ref(false)
const buscandoCpf = ref(false)

const form = ref({
  numeroOS: props.ProximoNumeroOS || props.proximoNumeroOS || '',
  vendedorId: props.UsuarioLogadoId || props.usuarioLogadoId || '',
  cpf: '',
  nome: '',
  telefone: '',
  dataNascimento: '',
  email: '',
  convenio: '',

  // Receita
  odEsferico: null,
  odCilindrico: null,
  odEixo: null,
  oeEsferico: null,
  oeCilindrico: null,
  oeEixo: null,
  adicao: null,
  odEsfericoPerto: null,
  oeEsfericoPerto: null,
  dpTotal: null,
  dnpOd: null,
  dnpOe: null,
  alturaMontagemOd: null,
  alturaMontagemOe: null,
  aro: null,
  po: null,
  dm: null,
  vert: null,
  haste: null,
  coOd: null,
  coOe: null,
  obsReceita: '',
  medicoTipo: 'OFTALMOLOGISTA',
  medicoNome: '',
  medicoCrm: '',
  receitaDataExame: new Date().toISOString().substring(0, 10),

  // Produtos
  armacaoPropria: false,
  armacaoId: '',
  armacaoDescricaoManual: '',
  valorArmacao: 0,

  lentePrecoId: '',
  lenteDescricaoManual: '',
  laboratorioParceiro: 'Essilor',
  valorLente: 0,

  // Financeiro
  descontoReais: 0,
  valorEntrada: 0,
  formaPagamento: 'DINHEIRO',
  parcelas: 1,
  dataPrevistaEntrega: new Date(Date.now() + 7 * 86400000).toISOString().substring(0, 10),
  observacoes: ''
})

// Sincronização Longe <-> Perto <-> Adição
const recalcularPerto = () => {
  if (form.value.adicao !== null && form.value.adicao !== undefined && form.value.adicao !== '') {
    const ad = Number(form.value.adicao || 0)
    form.value.odEsfericoPerto = Number(((form.value.odEsferico || 0) + ad).toFixed(2))
    form.value.oeEsfericoPerto = Number(((form.value.oeEsferico || 0) + ad).toFixed(2))
  }
}

const onAdicaoInput = () => {
  recalcularPerto()
}

const onDpTotalInput = () => {
  if (form.value.dpTotal !== null && form.value.dpTotal !== undefined && form.value.dpTotal !== '') {
    const half = Number((Number(form.value.dpTotal) / 2).toFixed(1))
    form.value.dnpOd = half
    form.value.dnpOe = half
  }
}

const onDnpInput = () => {
  if (form.value.dnpOd || form.value.dnpOe) {
    form.value.dpTotal = Number(((Number(form.value.dnpOd || 0) + Number(form.value.dnpOe || 0))).toFixed(1))
  }
}

const onPertoOdInput = () => {
  if (form.value.odEsfericoPerto !== null && form.value.odEsfericoPerto !== undefined && form.value.odEsfericoPerto !== '') {
    const longe = Number(form.value.odEsferico || 0)
    const perto = Number(form.value.odEsfericoPerto || 0)
    const adCalculada = Number((perto - longe).toFixed(2))
    if (adCalculada >= 0) {
      form.value.adicao = adCalculada
      form.value.oeEsfericoPerto = Number(((form.value.oeEsferico || 0) + adCalculada).toFixed(2))
    }
  }
}

const onPertoOeInput = () => {
  if (form.value.oeEsfericoPerto !== null && form.value.oeEsfericoPerto !== undefined && form.value.oeEsfericoPerto !== '') {
    const longe = Number(form.value.oeEsferico || 0)
    const perto = Number(form.value.oeEsfericoPerto || 0)
    const adCalculada = Number((perto - longe).toFixed(2))
    if (adCalculada >= 0) {
      form.value.adicao = adCalculada
      form.value.odEsfericoPerto = Number(((form.value.odEsferico || 0) + adCalculada).toFixed(2))
    }
  }
}

// Cálculos Financeiros Dinâmicos
const totalLiquidoCalculado = computed(() => {
  const bruto = Number(form.value.valorArmacao || 0) + Number(form.value.valorLente || 0)
  const desc = Number(form.value.descontoReais || 0)
  return Math.max(0, bruto - desc)
})

const saldoRestanteCalculado = computed(() => {
  const liquido = totalLiquidoCalculado.value
  const entrada = Number(form.value.valorEntrada || 0)
  return Math.max(0, liquido - entrada)
})

// Manipuladores de Seleção de Armação e Lente
const onArmacaoSelecionada = () => {
  const lista = props.armacoes || props.Armacoes || []
  const item = lista.find(a => (a.id || a.Id) === form.value.armacaoId)
  if (item) {
    form.value.valorArmacao = item.precoVenda || item.PrecoVenda || 0
  }
}

const onLenteSelecionada = () => {
  const lista = props.lentes || props.Lentes || []
  const item = lista.find(l => (l.id || l.Id) === form.value.lentePrecoId)
  if (item) {
    form.value.valorLente = item.precoVenda || item.PrecoVenda || 0
    form.value.laboratorioParceiro = item.laboratorio || item.Laboratorio || 'Essilor'
  }
}

// Consulta de CPF
const consultarCpf = async () => {
  const clean = (form.value.cpf || '').replace(/\D/g, '')
  if (clean.length !== 11) {
    alert('Digite um CPF válido com 11 números.')
    return
  }
  buscandoCpf.value = true
  try {
    const { data } = await axios.get(`/api/clientes/buscar-cpf/${clean}`)
    if (data) {
      form.value.nome = data.nome || form.value.nome
      form.value.telefone = data.telefone || form.value.telefone
      form.value.email = data.email || form.value.email
      form.value.convenio = data.convenio || form.value.convenio
      if (data.dataNascimento) {
        form.value.dataNascimento = data.dataNascimento.substring(0, 10)
      }
      alert('✓ Cliente localizado no cadastro da ótica!')
    }
  } catch {
    alert('Cliente não localizado pelo CPF. Continue preenchendo os dados para cadastrá-lo agora.')
  } finally {
    buscandoCpf.value = false
  }
}

// Salvar OS
const salvarOrdemServico = () => {
  if (!form.value.nome || !form.value.telefone) {
    alert('Preencha o Nome e o WhatsApp do cliente na Etapa 1.')
    etapaAtual.value = 1
    return
  }

  salvando.value = true
  router.post('/ordens', form.value, {
    preserveScroll: true,
    onError: () => {
      salvando.value = false
    }
  })
}

const formatarMoeda = (val) => {
  return Number(val || 0).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}
</script>
