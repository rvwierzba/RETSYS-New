# Frontend Guidelines - RETSYS (Diretrizes de UI/UX e Frontend)

## 1. Regras Estritas de UI/UX e Estilização

### 1.1 Remoção Obrigatória de Tags de Estilo Padrão
- **REGRA DE CSS INLINE E TAILWIND**: É **estritamente proibido** criar blocos de estilo CSS tradicionais (`<style>` ou `<style scoped>`) dentro dos arquivos `.vue`.
- Toda a estilização visual deve ser construída utilizando **Tailwind CSS** ou **estilos CSS inline dinâmicos** diretamente no atributo `style="..."` do elemento HTML quando houver cálculo dinâmico em tempo de execução.

### 1.2 Uso Obrigatório do Sistema de Grid Responsivo (UIkit / Tailwind Grid)
- As telas e formulários devem utilizar a estrutura de grid responsiva baseada em breakpoints padronizados:
  ```html
  <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
    <!-- Cards de Conteúdo -->
  </div>
  ```
- **Padrão de Container**: Conteúdo principal envelopado em `<div class="p-4 md:p-8 space-y-6 max-w-6xl mx-auto">`.
- **Padrão de Cards**: Superfícies brancas com borda suave e sombras leves: `bg-white p-6 rounded-2xl border border-slate-200 shadow-sm`.

---

## 2. Regras Imutáveis de Alinhamento em Grids de Dados (Tabelas)

Em **todas as tabelas e grids de dados** do sistema (`<table>`, `v-for`, exibições de lista), a alinhamento das colunas deve seguir rigorosamente as regras abaixo:

| Tipo de Dado | Regra de Alinhamento | Classe Tailwind Aplicável | Exemplo de Aplicação |
| :--- | :--- | :--- | :--- |
| **Textos / Nomes** | **Alinhado à Esquerda** | `text-left` | Nome do cliente, SKU, modelo, laboratório, vendedor |
| **Números / Valores** | **Alinhado à Direita** | `text-right` | Valores em R$, percentuais, quantidade em estoque, dioptrias |
| **Datas e Status** | **Centralizado** | `text-center` | Data de vencimento, data de entrega, status badge (Pendente, Pago) |

### 2.1 Exemplo Padrão de Tabela Homologada
```html
<table class="w-full text-sm border-collapse">
  <thead>
    <tr class="border-b border-slate-100 text-slate-400 text-xs font-bold uppercase tracking-wider">
      <!-- 1. Texto: Alinhado à Esquerda -->
      <th class="pb-3 text-left">Cliente / Descrição</th>
      
      <!-- 2. Data: Centralizado -->
      <th class="pb-3 text-center">Vencimento</th>
      
      <!-- 3. Número / Valor: Alinhado à Direita -->
      <th class="pb-3 text-right">Valor Total</th>
      
      <!-- 4. Status / Ações: Centralizado -->
      <th class="pb-3 text-center">Status</th>
    </tr>
  </thead>
  <tbody>
    <tr v-for="item in lista" :key="item.id" class="border-b border-slate-50 hover:bg-slate-50/50 transition">
      <!-- Alinhado à Esquerda -->
      <td class="py-4 text-left font-semibold text-slate-800">
        {{ item.nome }}
      </td>
      
      <!-- Centralizado -->
      <td class="py-4 text-center text-slate-600">
        {{ formatarData(item.dataVencimento) }}
      </td>
      
      <!-- Alinhado à Direita -->
      <td class="py-4 text-right font-bold text-slate-950">
        R$ {{ formatarMoeda(item.valor) }}
      </td>
      
      <!-- Centralizado -->
      <td class="py-4 text-center">
        <span class="px-3 py-1 rounded-full text-xs font-semibold bg-emerald-50 text-emerald-700 border border-emerald-100">
          {{ item.status }}
        </span>
      </td>
    </tr>
  </tbody>
</table>
```

---

## 3. Padrão de Layout e Componentes Globais

### 3.1 Layout Autenticado (`AuthenticatedLayout.vue`)
- Todas as telas internas do sistema devem ser envolvidas pela tag `<AuthenticatedLayout>`.
- **Cabeçalho Sticky**: Mantém a marca `RETSYS` com realce `SYS` em verde água (Teal) e logo WO, além da barra de navegação com links Inertia `<Link href="...">`.
- **Menu por Perfil**: Opções administrativas (Fechamento, Equipe, Configurações) exibidas condicionalmente apenas se `perfil === 'Admin'` ou `perfil === 'Gerente'`.
- **Relógio de Sessão**: Indicador visual do tempo de conexão ativa do usuário logado.
- **Widget do Spotify**: Player suspenso fixo no canto inferior direito (`SpotifyPlayer.vue`), oculto durante a impressão via classe `.no-print`.

### 3.2 Formatação de Valores e Datas
- **Moeda BR**: Toda renderização de valores financeiros deve utilizar o helper `formatarMoeda(valor)` de `utils/currency.js` para garantir duas casas decimais e separadores no padrão brasileiro (`1.250,50`).
- **Datas**: Formato `DD/MM/AAAA` para datas simples e `DD/MM/AAAA HH:mm` para carimbos de data/hora.

### 3.3 Formulários e Manipulação de Estado
- Utilização da Vue 3 Composition API (`<script setup>`).
- Submissões via utilitários da biblioteca `@inertiajs/vue3` (`router.post`, `router.put`, `useForm`).

