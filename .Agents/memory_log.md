# Memory Log - RETSYS (Histórico Incremental de Desenvolvimento de IA)

Este documento atua como o registro de **Memória Incremental** do projeto RETSYS. Toda nova funcionalidade, refatoração ou alteração arquitetural executada por agentes de IA deve registrar um novo bloco neste log seguindo o padrão estruturado estabelecido.

---

## Estrutura Padronizada para Novos Registros

```markdown
## [LOG-XXX] - YYYY-MM-DD: Título Curto da Intervenção
- **Autor/Agente**: Nome do Agente / Papel (ex: Arquiteto de Software AI)
- **Módulos Afetados**: ex: `RETSYS.Domain`, `RETSYS.Infrastructure`, `RETSYS.Web` (Frontend/Pages/OrdensServico)
- **Contexto & Solicitante**: Motivo da alteração e requisitos atendidos.

### 1. Resumo das Alterações
- Descrição detalhada do que foi modificado ou criado.

### 2. Impacto no Banco de Dados / Entidades
- Migrações executadas, novas colunas ou enums adicionados (ou `Nenhum`).

### 3. Validação e Verificação
- Comandos de build, testes executados e resultados de execução.

### 4. Riscos / Notas para Próximas Sessões
- Observações de débito técnico ou dependências futuras.
```

---

## Registros de Histórico

## [LOG-001] - 2026-09-10: Mapeamento de Arquitetura e Criação da Memória Base (.Agents/)
- **Autor/Agente**: Arquiteto de Software & Assistente de IA Avançado (Antigravity)
- **Módulos Afetados**: `RETSYS.Domain`, `RETSYS.Infrastructure`, `RETSYS.Web`, `.Agents/`
- **Contexto & Solicitante**: Varredura inicial completa do repositório RETSYS (RETSYS-New) para criar a documentação agnóstica de IA e servir como memória base para futuras iterações.

### 1. Resumo das Alterações
- Varredura técnica e leitura profunda das 3 camadas da aplicação C# ASP.NET Core MVC (.NET 10), modelos EF Core, infraestrutura e páginas Vue 3 / Inertia.js.
- Gerada a pasta `.Agents/` contendo a documentação arquitetural completa:
  - `project_overview.md`: Visão geral da arquitetura Clean/DDD, monólito SPA com Inertia.js, integrações de IA e gateway PIX.
  - `current_state.md`: Mapeamento minucioso dos módulos operacionais (Balcão, OS, Caixa, Comissões, Laboratório, Clientes, Estoque, Lentes) e pendências técnicas.
  - `entity_mapping.md`: Dicionário completo de tabelas PostgreSQL, entidades EF Core, convenções de nomenclatura e enums (`PerfilUsuario`, `MetodoPagamento`).
  - `frontend_guidelines.md`: Normas estritas de UI/UX (CSS inline/Tailwind sem tags `<style scoped>`, grid responsivo, regras imutáveis de alinhamento em tabelas).
  - `memory_log.md`: Formato oficial de log incremental para sessões com IA.

### 2. Impacto no Banco de Dados / Entidades
- Nenhum. Apenas criação de artefatos de documentação dentro de `.Agents/`.

### 3. Validação e Verificação
- Verificados e confirmados todos os arquivos e estruturas no repositório local `/home/rvwierzba/DEV/RETSYS-New`.
- Garantida a conformidade rigorosa com a **REGRA CRÍTICA**: Nenhum arquivo de infraestrutura/deploy (`docker-compose.yml`, `nginx`, `.github`) foi modificado ou analisado.

### 4. Riscos / Notas para Próximas Sessões
- Próximas sessões de desenvolvimento devem consultar a pasta `.Agents/` antes de propor alterações e registrar os seus logs sequenciais (`[LOG-002]`, `[LOG-003]`, etc.) neste documento.

---

## [LOG-002] - 2026-09-10: Implementação de Ajustes do Documento PDF e Solicitados via Áudio WhatsApp
- **Autor/Agente**: Arquiteto de Software & Assistente de IA Avançado (Antigravity)
- **Módulos Afetados**: `RETSYS.Domain` (`Cliente`, `OrdemServico`, `Armacao`, `OsAuditoriaLog`), `RETSYS.Infrastructure` (`ApplicationDbContext`), `RETSYS.Web` (`ClientesController`, `DashboardController`, `CaixaController`, `OrdensServicoController`, `Clientes/Index.vue`, `Dashboard/Index.vue`, `Caixa/Index.vue`, `OrdensServico/Create.vue`, `OrdensServico/Index.vue`)
- **Contexto & Solicitante**: Atendimento à demanda de remoção de CPF obrigatório no cadastro de cliente (solicitado via WhatsApp), exibição e busca fluida de clientes, restruturação de cards do Dashboard, unificação do card de atrasos, edição exclusiva de OS por Admin com log e trava de fechamento, apresentação de vendas parceladas em linha única no Caixa com duplo totalizador, seletor de Loja (Matriz, Travessa Itália, Parque) e rateio proporcional de descontos.

### 1. Resumo das Alterações
- **Clientes**: Tornado o CPF opcional na entidade `Cliente` e gravado como `NULL` (não `""`) para evitar falha no índice único composto `(OticaId, CPF)` do PostgreSQL. Removida a exigência de CPF no cadastro rápido.
- **Dashboard**: Card *"Comissão das vendedoras"* ativo para perfil `Admin` (soma de todas as vendedoras no mês). Unificação de pendências no card *"Serviços Atrasados"* (`DataPrevistaEntrega` < hoje e não entregue/cancelado). Adicionados cliques em todos os cards redirecionando para telas de resolução. Seletor de Loja no topo.
- **Caixa & Recebimentos**: Apresentação de vendas em linha única por OS (não duplicando parcelas de cartão). Exibição da coluna Situação (`1/1` para quitada e `1/2` para entrada + saldo pendente com alerta visual). Implementados dois totalizadores: **Total Vendido** (volume) e **Total Recebido** (caixa físico / gaveta com entradas + saldos quitados), além do **Total a Receber** e quebra por forma de pagamento.
- **Edição de OS pelo Admin**: Restrição exclusiva para perfil `Admin`/`Gerente`. Permite editar `DataEntrada`, recalcula comissões, grava log auditável em `os_auditoria_logs` e exibe o badge `"data ajustada em DD/MM por [usuario]"`. Adicionada trava de segurança que impede alteração se o fechamento de comissão do mês antigo/novo estiver `FECHADO` ou `PAGO`.
- **Suporte Multi-Loja**: Adicionado campo `LojaVenda` na emissão da OS (Matriz, Travessa Itália, Parque) e filtro por loja no estoque de armações, caixa e dashboard.
- **Rateio Proporcional de Desconto**: Desconto total da OS distribuído proporcionalmente entre a armação e a lente.

### 2. Impacto no Banco de Dados / Entidades
- `Cliente.CPF`: Tipo atualizado para `string?` (opcional/nullable).
- `OrdemServico`: Adicionadas colunas `LojaVenda`, `DataAjustadaLog`, `DataEntradaOriginal`.
- `Armacao`: Adicionada coluna `LojaUnidade`.
- Criada a nova tabela `os_auditoria_logs` (`OsAuditoriaLog`).

### 3. Validação e Verificação
- Compilação realizada via `dotnet build RETSYS.slnx` com resultado: **Build Succeeded** (0 erros).

### 4. Riscos / Notas para Próximas Sessões
- As próximas migrations do EF Core ao rodar no ambiente de desenvolvimento/produção irão criar as novas colunas e a tabela `os_auditoria_logs`.


