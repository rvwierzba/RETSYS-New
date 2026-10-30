<template>
  <AuthenticatedLayout>
    <div class="p-4 md:p-8 space-y-6 max-w-6xl mx-auto">
      
      <!-- Cabeçalho de Navegação e Ações -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white p-6 rounded-3xl border border-slate-200 shadow-sm">
        <div>
          <div class="flex items-center gap-2 mb-1">
            <a href="/ordens" class="text-xs font-mono font-bold text-slate-400 hover:text-slate-700 transition">
              ← Voltar para Ordens
            </a>
            <span class="text-slate-300">•</span>
            <span class="text-[10px] font-mono font-black uppercase px-2 py-0.5 rounded-full" :class="obterCorStatus(form.status)">
              {{ form.status }}
            </span>
          </div>
          <h1 class="text-2xl font-black text-slate-900 tracking-tight font-mono">
            Edição da OS #{{ form.numeroOS }}
          </h1>
          <p class="text-xs text-slate-500 mt-0.5">
            Cliente: <b class="text-slate-800">{{ form.clienteNome }}</b> • Emissão: <span class="font-mono">{{ form.dataEntrada }}</span>
          </p>
        </div>

        <div class="flex items-center gap-3">
          <a
            :href="`/ordens`"
            class="px-4 py-2.5 rounded-2xl bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs transition"
          >
            Cancelar
          </a>
          <button
            @click="salvarEdicao"
            :disabled="salvando"
            class="px-6 py-2.5 rounded-2xl bg-teal-600 hover:bg-teal-700 disabled:opacity-50 text-white font-bold text-xs shadow-sm transition flex items-center gap-2"
          >
            <span v-if="salvando">Salvando...</span>
            <span v-else>💾 Salvar Alterações da OS</span>
          </button>
        </div>
      </div>

      <!-- Abas de Edição -->
      <div class="flex items-center gap-2 bg-slate-100 p-1.5 rounded-2xl overflow-x-auto text-xs font-bold font-mono">
        <button
          v-for="aba in abas"
          :key="aba.id"
          @click="abaAtiva = aba.id"
          :class="[
            'px-4 py-2 rounded-xl transition shrink-0',
            abaAtiva === aba.id
              ? 'bg-white text-slate-900 shadow-xs font-black'
              : 'text-slate-500 hover:text-slate-800'
          ]"
        >
          {{ aba.nome }}
        </button>
      </div>

      <!-- CONTEÚDO DAS ABAS -->

      <!-- ABA 1: DADOS GERAIS & CLIENTE -->
      <div v-show="abaAtiva === 'cliente'" class="bg-white p-6 sm:p-8 rounded-3xl border border-slate-200 shadow-sm space-y-6">
        <div class="border-b border-slate-100 pb-3">
          <h3 class="text-base font-black text-slate-900 font-mono flex items-center gap-2">
            <span>👤</span> 1. Dados do Cliente &amp; Atendimento
          </h3>
          <p class="text-xs text-slate-400">Identificação, contato do paciente e responsáveis pela venda.</p>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
          <div class="sm:col-span-2">
            <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Nome Completo do Cliente</label>
            <input v-model="form.clienteNome" type="text" class="w-full rounded-2xl border-slate-200 text-xs font-bold" />
          </div>

          <div>
            <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">CPF</label>
            <input v-model="form.clienteCpf" type="text" class="w-full rounded-2xl border-slate-200 text-xs font-mono" />
          </div>

          <div>
            <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">WhatsApp / Telefone</label>
            <input v-model="form.clienteTelefone" type="text" class="w-full rounded-2xl border-slate-200 text-xs font-bold text-teal-700" />
          </div>

          <div>
            <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Data de Nascimento</label>
            <input v-model="form.clienteDataNascimento" type="date" class="w-full rounded-2xl border-slate-200 text-xs font-mono" />
          </div>

          <div>
            <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">E-mail</label>
            <input v-model="form.clienteEmail" type="email" class="w-full rounded-2xl border-slate-200 text-xs" />
          </div>

          <div>
            <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Convênio / Parceria</label>
            <input v-model="form.clienteConvenio" type="text" class="w-full rounded-2xl border-slate-200 text-xs" />
          </div>

          <div>
            <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Atendente / Vendedor</label>
            <select v-model="form.vendedorId" class="w-full rounded-2xl border-slate-200 text-xs font-bold">
              <option value="">(Selecione)</option>
              <option v-for="v in (vendedores || Vendedores || [])" :key="v.id || v.Id" :value="v.id || v.Id">
                {{ v.nome || v.Nome }}
              </option>
            </select>
          </div>

          <div>
            <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Status da OS</label>
            <select v-model="form.status" class="w-full rounded-2xl border-slate-200 text-xs font-bold uppercase" :class="obterCorStatus(form.status)">
              <option value="LANCADA">Lançada</option>
              <option value="EM_ABERTO">Em Aberto</option>
              <option value="CONFIRMADA">Confirmada</option>
              <option value="AGUARDANDO_LENTE">Aguard. Lente</option>
              <option value="EM_MONTAGEM">Em Montagem</option>
              <option value="PRONTA">Pronta</option>
              <option value="ENTREGUE">Entregue</option>
              <option value="CANCELADO">Cancelada</option>
            </select>
          </div>
        </div>

        <!-- Endereço -->
        <div class="pt-4 border-t border-slate-100">
          <span class="text-xs font-mono font-bold uppercase text-slate-700 block mb-3">Endereço Residencial</span>
          <div class="grid grid-cols-1 sm:grid-cols-4 gap-3 text-xs">
            <div class="sm:col-span-2">
              <label class="block text-[10px] font-mono uppercase text-slate-400 mb-1">Logradouro / Rua</label>
              <input v-model="form.clienteLogradouro" type="text" class="w-full rounded-xl border-slate-200 text-xs" />
            </div>
            <div>
              <label class="block text-[10px] font-mono uppercase text-slate-400 mb-1">Número</label>
              <input v-model="form.clienteNumero" type="text" class="w-full rounded-xl border-slate-200 text-xs" />
            </div>
            <div>
              <label class="block text-[10px] font-mono uppercase text-slate-400 mb-1">Bairro</label>
              <input v-model="form.clienteBairro" type="text" class="w-full rounded-xl border-slate-200 text-xs" />
            </div>
            <div>
              <label class="block text-[10px] font-mono uppercase text-slate-400 mb-1">Cidade</label>
              <input v-model="form.clienteCidade" type="text" class="w-full rounded-xl border-slate-200 text-xs" />
            </div>
            <div>
              <label class="block text-[10px] font-mono uppercase text-slate-400 mb-1">Estado (UF)</label>
              <input v-model="form.clienteEstado" type="text" maxlength="2" class="w-full rounded-xl border-slate-200 text-xs uppercase" />
            </div>
            <div>
              <label class="block text-[10px] font-mono uppercase text-slate-400 mb-1">CEP</label>
              <input v-model="form.clienteCep" type="text" class="w-full rounded-xl border-slate-200 text-xs font-mono" />
            </div>
          </div>
        </div>

        <!-- Datas -->
        <div class="pt-4 border-t border-slate-100 grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div>
            <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Data de Emissão (Entrada)</label>
            <input v-model="form.novaDataEntrada" type="date" class="w-full rounded-2xl border-slate-200 text-xs font-mono font-bold" />
          </div>
          <div>
            <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Data Prevista de Entrega</label>
            <input v-model="form.dataPrevistaEntrega" type="date" class="w-full rounded-2xl border-slate-200 text-xs font-mono font-bold" />
          </div>
          <div>
            <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Data de Entrega Realizada</label>
            <input v-model="form.dataEntregaReal" type="date" class="w-full rounded-2xl border-slate-200 text-xs font-mono" />
          </div>
        </div>
      </div>

      <!-- ABA 2: RECEITUÁRIO DE ÓCULOS (PADRÃO OFTALMOLÓGICO BR - LONGE E PERTO) -->
      <div v-show="abaAtiva === 'receita'" class="bg-white p-6 sm:p-8 rounded-3xl border border-slate-200 shadow-sm space-y-6">
        <div class="border-b border-slate-100 pb-3 flex items-center justify-between">
          <div>
            <h3 class="text-base font-black text-slate-900 font-mono flex items-center gap-2">
              <span>👓</span> 2. Receituário de Óculos (Para Longe &amp; Para Perto)
            </h3>
            <p class="text-xs text-slate-400">Prescrição oftalmológica/optométrica completa com cálculo bidirecional de Adição.</p>
          </div>
          <span class="text-[10px] font-mono font-bold text-teal-700 bg-teal-50 border border-teal-200 px-2.5 py-1 rounded-full">
            Receituário Padrão BR
          </span>
        </div>

        <!-- Grade Oftalmológica -->
        <div class="p-5 bg-slate-50 rounded-2xl border border-slate-200 space-y-4">
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
                    <input v-model.number="form.odEsferico" @input="recalcularPerto" type="number" step="0.25" placeholder="0.00" class="w-24 text-center font-bold rounded-lg border-slate-200 text-xs py-1.5" />
                  </td>
                  <td class="py-2 px-2">
                    <input v-model.number="form.odCilindrico" type="number" step="0.25" placeholder="0.00" class="w-24 text-center font-bold text-amber-700 rounded-lg border-slate-200 text-xs py-1.5" />
                  </td>
                  <td class="py-2 px-2">
                    <input v-model.number="form.odEixo" type="number" min="0" max="180" placeholder="0" class="w-20 text-center rounded-lg border-slate-200 text-xs py-1.5" />
                  </td>
                  <td class="py-2 px-2">
                    <input v-model.number="form.dnpOd" @input="onDnpInput" type="number" step="0.5" placeholder="32" class="w-20 text-center rounded-lg border-slate-200 text-xs py-1.5" />
                  </td>
                </tr>

                <!-- Para Longe - OE -->
                <tr class="bg-white">
                  <td class="py-3 px-3 font-bold text-slate-900 border-t border-slate-100">OE</td>
                  <td class="py-2 px-2 border-t border-slate-100">
                    <input v-model.number="form.oeEsferico" @input="recalcularPerto" type="number" step="0.25" placeholder="0.00" class="w-24 text-center font-bold rounded-lg border-slate-200 text-xs py-1.5" />
                  </td>
                  <td class="py-2 px-2 border-t border-slate-100">
                    <input v-model.number="form.oeCilindrico" type="number" step="0.25" placeholder="0.00" class="w-24 text-center font-bold text-amber-700 rounded-lg border-slate-200 text-xs py-1.5" />
                  </td>
                  <td class="py-2 px-2 border-t border-slate-100">
                    <input v-model.number="form.oeEixo" type="number" min="0" max="180" placeholder="0" class="w-20 text-center rounded-lg border-slate-200 text-xs py-1.5" />
                  </td>
                  <td class="py-2 px-2 border-t border-slate-100">
                    <input v-model.number="form.dnpOe" @input="onDnpInput" type="number" step="0.5" placeholder="32" class="w-20 text-center rounded-lg border-slate-200 text-xs py-1.5" />
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

          <!-- Observações Técnicas da Receita -->
          <div class="pt-2 border-t border-slate-100">
            <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Observações Técnicas da Receita / Montagem</label>
            <textarea v-model="form.obsReceita" rows="2" placeholder="Observações específicas para o laboratório ou sobre o receituário do paciente..." class="w-full rounded-2xl border-slate-200 text-xs bg-white"></textarea>
          </div>

          <!-- Médico e Validade -->
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 pt-2 border-t border-slate-100">
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
          </div>
        </div>
      </div>

      <!-- ABA 3: PRODUTOS & LABORATÓRIO -->
      <div v-show="abaAtiva === 'produtos'" class="bg-white p-6 sm:p-8 rounded-3xl border border-slate-200 shadow-sm space-y-6">
        <div class="border-b border-slate-100 pb-3">
          <h3 class="text-base font-black text-slate-900 font-mono flex items-center gap-2">
            <span>🔬</span> 3. Armação, Lentes Oftálmicas &amp; Laboratório
          </h3>
          <p class="text-xs text-slate-400">Itens vinculados à OS e parceiro de montagem.</p>
        </div>

        <!-- Armação -->
        <div class="p-5 bg-slate-50 rounded-2xl border border-slate-200 space-y-3">
          <span class="text-xs font-mono font-bold uppercase text-slate-700 block">Armação de Óculos</span>
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
            <div class="sm:col-span-2">
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Descrição / Modelo</label>
              <input v-model="form.armacaoDescricaoManual" type="text" placeholder="Ex: Ray-Ban Aviador Dourado" class="w-full rounded-2xl border-slate-200 text-xs bg-white font-bold" />
            </div>
            <div>
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Valor Armação (R$)</label>
              <input v-model.number="form.valorArmacao" type="number" step="0.01" class="w-full rounded-2xl border-slate-200 text-xs font-mono font-bold bg-white text-slate-900" />
            </div>
          </div>
        </div>

        <!-- Lente -->
        <div class="p-5 bg-slate-50 rounded-2xl border border-slate-200 space-y-3">
          <span class="text-xs font-mono font-bold uppercase text-slate-700 block">Lentes Oftálmicas &amp; Montagem</span>
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
            <div class="sm:col-span-2">
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Especificação da Lente</label>
              <input v-model="form.lenteDescricaoManual" type="text" placeholder="Ex: Multifocal Varilux Comfort Antirreflexo" class="w-full rounded-2xl border-slate-200 text-xs bg-white font-bold" />
            </div>
            <div>
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Valor Lentes (R$)</label>
              <input v-model.number="form.valorLente" type="number" step="0.01" class="w-full rounded-2xl border-slate-200 text-xs font-mono font-bold bg-white text-slate-900" />
            </div>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-3 gap-3 pt-2">
            <div>
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Laboratório Parceiro</label>
              <select v-model="form.laboratorioParceiro" class="w-full rounded-2xl border-slate-200 text-xs bg-white font-bold">
                <option value="Essilor">Essilor (Varilux / Crizal)</option>
                <option value="Hoya">Hoya (MiyoSmart / BlueControl)</option>
                <option value="Zeiss">Zeiss (DuraVision)</option>
                <option value="Local">Laboratório Local</option>
                <option value="Outro">Outro Parceiro</option>
              </select>
            </div>
            <div>
              <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Nº Pedido Lab</label>
              <input v-model="form.numeroPedidoLaboratorio" type="text" placeholder="Ex: LAB-9921" class="w-full rounded-2xl border-slate-200 text-xs bg-white font-mono" />
            </div>
            <div class="flex items-center pt-6">
              <label class="flex items-center gap-2 text-xs font-bold text-slate-700 cursor-pointer">
                <input v-model="form.lentePedida" type="checkbox" class="rounded text-teal-600 focus:ring-teal-500" />
                <span>✓ Lente já pedida ao laboratório</span>
              </label>
            </div>
          </div>
        </div>
      </div>

      <!-- ABA 4: FINANCEIRO & PAGAMENTO -->
      <div v-show="abaAtiva === 'financeiro'" class="bg-white p-6 sm:p-8 rounded-3xl border border-slate-200 shadow-sm space-y-6">
        <div class="border-b border-slate-100 pb-3">
          <h3 class="text-base font-black text-slate-900 font-mono flex items-center gap-2">
            <span>💰</span> 4. Fechamento Financeiro &amp; Parcelas
          </h3>
          <p class="text-xs text-slate-400">Valores faturados, descontos concedidos, entrada e crediário.</p>
        </div>

        <!-- Cards de Resumo -->
        <div class="grid grid-cols-2 sm:grid-cols-4 gap-3 text-center">
          <div class="p-4 bg-slate-50 rounded-2xl border border-slate-200">
            <span class="text-[10px] font-mono font-bold uppercase text-slate-400 block">Valor Bruto</span>
            <span class="text-lg font-black text-slate-900 font-mono">
              R$ {{ formatarMoeda(valorBrutoCalculado) }}
            </span>
          </div>
          <div class="p-4 bg-amber-50 rounded-2xl border border-amber-200">
            <span class="text-[10px] font-mono font-bold uppercase text-amber-700 block">Desconto</span>
            <span class="text-lg font-black text-amber-800 font-mono">
              R$ {{ formatarMoeda(form.descontoReais) }}
            </span>
          </div>
          <div class="p-4 bg-teal-50 rounded-2xl border border-teal-200">
            <span class="text-[10px] font-mono font-bold uppercase text-teal-700 block">Total Líquido</span>
            <span class="text-lg font-black text-teal-900 font-mono">
              R$ {{ formatarMoeda(valorLiquidoCalculado) }}
            </span>
          </div>
          <div class="p-4 bg-rose-50 rounded-2xl border border-rose-200">
            <span class="text-[10px] font-mono font-bold uppercase text-rose-700 block">Saldo a Receber</span>
            <span class="text-lg font-black text-rose-700 font-mono">
              R$ {{ formatarMoeda(valorRestanteCalculado) }}
            </span>
          </div>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-4 gap-4 pt-2">
          <div>
            <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Desconto em Reais (R$)</label>
            <input v-model.number="form.descontoReais" type="number" step="0.01" class="w-full rounded-2xl border-slate-200 text-xs font-mono font-bold" />
          </div>

          <div>
            <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Valor de Entrada (R$)</label>
            <input v-model.number="form.valorEntrada" type="number" step="0.01" class="w-full rounded-2xl border-slate-200 text-xs font-mono font-bold text-teal-800" />
          </div>

          <div>
            <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Forma de Pagamento</label>
            <select v-model="form.formaPagamento" class="w-full rounded-2xl border-slate-200 text-xs font-bold">
              <option value="DINHEIRO">Dinheiro</option>
              <option value="PIX">PIX</option>
              <option value="CARTAO_CREDITO">Cartão de Crédito</option>
              <option value="CARTAO_DEBITO">Cartão de Débito</option>
              <option value="CARNE">Carnê Próprio da Ótica</option>
              <option value="BOLETO">Boleto</option>
            </select>
          </div>

          <div>
            <label class="block text-[11px] font-mono font-bold uppercase text-slate-500 mb-1">Qtd Parcelas</label>
            <input v-model.number="form.parcelas" type="number" min="1" max="12" class="w-full rounded-2xl border-slate-200 text-xs font-mono font-bold text-center" />
          </div>
        </div>

        <!-- Parcelas Geradas -->
        <div v-if="(ordem?.parcelas || ordem?.Parcelas || []).length > 0" class="pt-4 border-t border-slate-100">
          <span class="text-xs font-mono font-bold uppercase text-slate-700 block mb-2">Parcelas Registradas</span>
          <div class="overflow-x-auto">
            <table class="w-full text-left text-xs border-collapse">
              <thead class="bg-slate-50 text-[10px] font-mono font-bold uppercase text-slate-400">
                <tr>
                  <th class="py-2 px-3">Nº</th>
                  <th class="py-2 px-3">Vencimento</th>
                  <th class="py-2 px-3">Valor</th>
                  <th class="py-2 px-3">Forma</th>
                  <th class="py-2 px-3">Status</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100 font-mono">
                <tr v-for="p in (ordem?.parcelas || ordem?.Parcelas || [])" :key="p.id">
                  <td class="py-2 px-3 font-bold">{{ p.numeroParcela }}ª</td>
                  <td class="py-2 px-3 text-slate-600">{{ p.dataVencimento }}</td>
                  <td class="py-2 px-3 font-bold text-slate-900">R$ {{ formatarMoeda(p.valor) }}</td>
                  <td class="py-2 px-3 text-slate-500">{{ p.formaPagamento }}</td>
                  <td class="py-2 px-3">
                    <span v-if="p.pago" class="text-emerald-700 bg-emerald-50 px-2 py-0.5 rounded text-[10px] font-bold">
                      ✓ Pago em {{ p.dataPagamento }}
                    </span>
                    <span v-else class="text-amber-700 bg-amber-50 px-2 py-0.5 rounded text-[10px] font-bold">
                      ⏳ Pendente
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- ABA 5: HISTÓRICO DE AUDITORIA -->
      <div v-show="abaAtiva === 'auditoria'" class="bg-white p-6 sm:p-8 rounded-3xl border border-slate-200 shadow-sm space-y-6">
        <div class="border-b border-slate-100 pb-3">
          <h3 class="text-base font-black text-slate-900 font-mono flex items-center gap-2">
            <span>🛡️</span> 5. Auditoria de Alterações da OS
          </h3>
          <p class="text-xs text-slate-400">Rastreabilidade completa de todas as modificações realizadas nesta Ordem.</p>
        </div>

        <div v-if="(auditoriaLogs || AuditoriaLogs || []).length === 0" class="text-center py-10 text-slate-400 text-xs">
          Nenhuma alteração registrada até o momento.
        </div>

        <div v-else class="space-y-3">
          <div 
            v-for="log in (auditoriaLogs || AuditoriaLogs || [])" 
            :key="log.id"
            class="p-4 rounded-2xl bg-slate-50 border border-slate-200 text-xs space-y-1"
          >
            <div class="flex items-center justify-between text-[11px] font-mono">
              <span class="font-bold text-slate-900">{{ log.usuarioNome }}</span>
              <span class="text-slate-400">{{ log.dataAlteracao }}</span>
            </div>
            <p class="text-slate-700 font-medium">{{ log.descricao }}</p>
            <div class="flex items-center gap-2 text-[10px] font-mono pt-1 text-slate-500">
              <span>Campo: <b class="text-slate-800">{{ log.campoAlterado }}</b></span>
              <span>•</span>
              <span>De: <s class="text-rose-600">{{ log.valorAntigo }}</s></span>
              <span>➔</span>
              <span>Para: <b class="text-emerald-700">{{ log.valorNovo }}</b></span>
            </div>
          </div>
        </div>
      </div>

      <!-- Barra Inferior de Confirmação -->
      <div class="bg-white p-6 rounded-3xl border border-slate-200 shadow-sm flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div class="text-xs text-slate-500">
          Informe o motivo da alteração (obrigatório para fins de conformidade):
          <input v-model="motivoEdicao" type="text" placeholder="Ex: Correção de dioptria conforme receita médica" class="w-full sm:w-96 rounded-xl border-slate-200 text-xs mt-1 bg-slate-50" />
        </div>

        <div class="flex items-center gap-3">
          <a href="/ordens" class="px-5 py-2.5 rounded-2xl bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs transition">
            Voltar
          </a>
          <button
            @click="salvarEdicao"
            :disabled="salvando"
            class="px-7 py-3 rounded-2xl bg-teal-600 hover:bg-teal-700 disabled:opacity-50 text-white font-bold text-xs shadow-sm transition flex items-center gap-2"
          >
            <span v-if="salvando">Salvando Alterações...</span>
            <span v-else>💾 Confirmar e Salvar OS</span>
          </button>
        </div>
      </div>

    </div>
  </AuthenticatedLayout>
</template>

<script setup>
import { ref, computed } from 'vue'
import { router } from '@inertiajs/vue3'
import AuthenticatedLayout from '../../Shared/AuthenticatedLayout.vue'

const props = defineProps({
  Ordem: Object,
  ordem: Object,
  Vendedores: Array,
  vendedores: Array,
  Armacoes: Array,
  armacoes: Array,
  Lentes: Array,
  lentes: Array,
  Oticas: Array,
  oticas: Array,
  AuditoriaLogs: Array,
  auditoriaLogs: Array
})

const ordemData = computed(() => props.Ordem || props.ordem || {})
const o = ordemData.value
const rec = o.receita || o.Receita || {}
const cli = o.cliente || o.Cliente || {}
const fin = o.financeiro || o.Financeiro || {}

const abaAtiva = ref('cliente')
const abas = [
  { id: 'cliente', nome: '1. Cliente & Atendimento' },
  { id: 'receita', nome: '2. Receita (Longe e Perto)' },
  { id: 'produtos', nome: '3. Armação & Lentes' },
  { id: 'financeiro', nome: '4. Fechamento Financeiro' },
  { id: 'auditoria', nome: '5. Histórico Auditoria' }
]

const form = ref({
  numeroOS: o.numeroOS || o.NumeroOS || '',
  status: o.status || o.Status || 'EM_ABERTO',
  vendedorId: o.vendedorId || o.VendedorId || '',
  novaDataEntrada: o.dataEntradaFormatada || o.dataEntrada || '',
  dataPrevistaEntrega: o.dataPrevistaEntregaFormatada || o.dataPrevistaEntrega || '',
  dataEntregaReal: o.dataEntregaRealFormatada || o.dataEntregaReal || '',

  // Cliente
  clienteNome: cli.nome || cli.Nome || '',
  clienteCpf: cli.cpf || cli.CPF || '',
  clienteTelefone: cli.telefone || cli.Telefone || '',
  clienteDataNascimento: cli.dataNascimentoFormatada || cli.dataNascimento || '',
  clienteEmail: cli.email || cli.Email || '',
  clienteConvenio: cli.convenio || cli.Convenio || '',
  clienteLogradouro: cli.logradouro || cli.Logradouro || '',
  clienteNumero: cli.numero || cli.Numero || '',
  clienteBairro: cli.bairro || cli.Bairro || '',
  clienteCidade: cli.cidade || cli.Cidade || '',
  clienteEstado: cli.estado || cli.Estado || '',
  clienteCep: cli.cep || cli.Cep || '',

  // Receita
  odEsferico: rec.odEsferico ?? rec.OdEsferico ?? null,
  odCilindrico: rec.odCilindrico ?? rec.OdCilindrico ?? null,
  odEixo: rec.odEixo ?? rec.OdEixo ?? null,
  oeEsferico: rec.oeEsferico ?? rec.OeEsferico ?? null,
  oeCilindrico: rec.oeCilindrico ?? rec.OeCilindrico ?? null,
  oeEixo: rec.oeEixo ?? rec.OeEixo ?? null,
  adicao: rec.adicao ?? rec.Adicao ?? null,
  odEsfericoPerto: rec.odEsfericoPerto ?? ((rec.odEsferico ?? 0) + (rec.adicao ?? 0)),
  oeEsfericoPerto: rec.oeEsfericoPerto ?? ((rec.oeEsferico ?? 0) + (rec.adicao ?? 0)),
  dpTotal: ((rec.dnpOd ?? rec.DnpOd ?? 0) + (rec.dnpOe ?? rec.DnpOe ?? 0)) > 0 ? Number(((rec.dnpOd ?? rec.DnpOd ?? 0) + (rec.dnpOe ?? rec.DnpOe ?? 0)).toFixed(1)) : null,
  dnpOd: rec.dnpOd ?? rec.DnpOd ?? null,
  dnpOe: rec.dnpOe ?? rec.DnpOe ?? null,
  alturaMontagemOd: rec.alturaMontagemOd ?? rec.AlturaMontagemOd ?? null,
  alturaMontagemOe: rec.alturaMontagemOe ?? rec.AlturaMontagemOe ?? null,
  aro: rec.aro ?? rec.Aro ?? null,
  dm: rec.dm ?? rec.Dm ?? null,
  vert: rec.vert ?? rec.Vert ?? null,
  po: rec.po ?? rec.Po ?? null,
  haste: rec.haste ?? rec.Haste ?? null,
  coOd: rec.coOd ?? rec.CoOd ?? null,
  coOe: rec.coOe ?? rec.CoOe ?? null,
  obsReceita: rec.obsReceita || rec.ObsReceita || '',
  medicoNome: o.medicoNome || o.MedicoNome || '',
  medicoCrm: o.medicoCrm || o.MedicoCrm || '',
  medicoTipo: o.medicoTipo || o.MedicoTipo || 'OFTALMOLOGISTA',

  // Produtos
  armacaoDescricaoManual: o.armacaoModeloManual || o.ArmacaoModeloManual || '',
  lenteDescricaoManual: o.lenteDescricaoManual || o.LenteDescricaoManual || '',
  laboratorioParceiro: o.laboratorioParceiro || o.LaboratorioParceiro || 'Essilor',
  numeroPedidoLaboratorio: o.numeroPedidoLaboratorio || o.NumeroPedidoLaboratorio || '',
  lentePedida: o.lentePedida || o.LentePedida || false,
  valorArmacao: fin.valorArmacao ?? fin.ValorArmacao ?? 0,
  valorLente: fin.valorLente ?? fin.ValorLente ?? 0,

  // Financeiro
  descontoReais: fin.descontoReais ?? fin.DescontoReais ?? 0,
  valorEntrada: fin.valorEntrada ?? fin.ValorEntrada ?? 0,
  formaPagamento: fin.formaPagamento || fin.FormaPagamento || 'DINHEIRO',
  parcelas: fin.parcelas || fin.Parcelas || 1
})

const motivoEdicao = ref('')
const salvando = ref(false)

// Sincronização Longe <-> Perto <-> Adição
const recalcularPerto = () => {
  if (form.value.adicao !== null && form.value.adicao !== undefined) {
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
  if (form.value.odEsfericoPerto !== null && form.value.odEsfericoPerto !== undefined) {
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
  if (form.value.oeEsfericoPerto !== null && form.value.oeEsfericoPerto !== undefined) {
    const longe = Number(form.value.oeEsferico || 0)
    const perto = Number(form.value.oeEsfericoPerto || 0)
    const adCalculada = Number((perto - longe).toFixed(2))
    if (adCalculada >= 0) {
      form.value.adicao = adCalculada
      form.value.odEsfericoPerto = Number(((form.value.odEsferico || 0) + adCalculada).toFixed(2))
    }
  }
}

// Totais Financeiros
const valorBrutoCalculado = computed(() => {
  return Number(form.value.valorArmacao || 0) + Number(form.value.valorLente || 0)
})

const valorLiquidoCalculado = computed(() => {
  return Math.max(0, valorBrutoCalculado.value - Number(form.value.descontoReais || 0))
})

const valorRestanteCalculado = computed(() => {
  return Math.max(0, valorLiquidoCalculado.value - Number(form.value.valorEntrada || 0))
})

const salvarEdicao = () => {
  if (!form.value.clienteNome) {
    alert('O nome do cliente é obrigatório.')
    abaAtiva.value = 'cliente'
    return
  }

  salvando.value = true
  const payload = {
    ...form.value,
    motivo: motivoEdicao.value || 'Edição geral de dados da Ordem de Serviço'
  }

  const id = o.id || o.Id
  router.post(`/ordens/${id}/editar`, payload, {
    preserveScroll: true,
    onFinish: () => {
      salvando.value = false
    },
    onError: (err) => {
      salvando.value = false
      alert('Erro ao salvar edições da OS. Verifique os campos informados.')
    }
  })
}

const formatarMoeda = (val) => {
  return Number(val || 0).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const obterCorStatus = (st) => {
  switch (st) {
    case 'ENTREGUE': return 'bg-emerald-100 text-emerald-800 border border-emerald-300'
    case 'PRONTA':
    case 'PRONTO': return 'bg-blue-100 text-blue-800 border border-blue-300'
    case 'EM_MONTAGEM': return 'bg-indigo-100 text-indigo-800 border border-indigo-300'
    case 'AGUARDANDO_LENTE': return 'bg-amber-100 text-amber-800 border border-amber-300'
    case 'CONFIRMADA': return 'bg-teal-100 text-teal-800 border border-teal-300'
    case 'CANCELADO':
    case 'CANCELADA': return 'bg-rose-100 text-rose-800 border border-rose-300'
    default: return 'bg-slate-100 text-slate-800 border border-slate-300'
  }
}
</script>

