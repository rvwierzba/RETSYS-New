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

---

## [LOG-003] - 2026-10-08: Reestruturação Multi-Tenancy, Hierarquia Matriz/Filiais e Perfil Dono
- **Autor/Agente**: Arquiteto de Software & Assistente de IA Avançado (Antigravity)
- **Módulos Afetados**: `RETSYS.Domain` (`Otica`, `PerfilUsuario`), `RETSYS.Infrastructure` (`ApplicationDbContext`, `DatabaseSeeder`, Migrations), `RETSYS.Web` (`TenantController`, `Program.cs`, `OrdensServicoController`, `DashboardController`, `LentesController`, `UsuariosController`, `ArmacoesController`, `ConfiguracoesController`, `ComissoesController`, `SistemaController`, `AuthenticatedLayout.vue`, `Users/Index.vue`)
- **Contexto & Solicitante**: Auditoria e correção de vazamentos de dados entre óticas, implementação do modelo hierárquico Matriz/Filiais, suporte ao Perfil Dono (gestão multi-loja de rede), compartilhamento de tabela de preços de lentes entre matriz e filiais, transferência de colaboradores entre lojas da rede, proteção de preços de custo e restrição de visualizações para vendedores.

### 1. Resumo das Alterações
- **Isolamento de Tenant Rigoroso**:
  - `Admin` e `Vendedor` têm suas lojas resolvidas exclusivamente pelo vínculo direto do usuário (`Usuario.OticaId`). Trava absoluta contra adulterações via query string ou parâmetros forjados.
  - Perfil `Sistema` ("Deus"/teste) pode alternar livremente entre qualquer ótica via seletor de sessão (`Session["OticaAtivaId"]`).
  - Perfil `Dono` pode alternar entre a Matriz e todas as filiais da sua rede através do seletor de sessão.
- **Hierarquia Matriz e Filiais**:
  - Adicionado campo `MatrizId` auto-referenciado na entidade `Otica` com navegação `Matriz` e coleção `Filiais`.
  - Herança de lentes e tabela de preços: filiais visualizam e vendem automaticamente as lentes cadastradas na sua Matriz (`lp.Lente.OticaId == oticaId || lp.Lente.OticaId == matrizId`).
  - Transferência de colaboradores: Administradores e Donos podem realocar funcionários (`OticaId` e `FilialLoja`) entre lojas da mesma rede.
- **Privacidade & Segurança de Acesso**:
  - `Vendedor`: Visualiza todas as OSs da loja para suporte no balcão, porém os nomes de outros vendedores aparecem mascarados como *"Outro Vendedor"*. Preço de custo de armações e lentes é omitido (`0.00`). No Dashboard e Comissões, vê exclusivamente suas próprias métricas e metas.
  - `Admin`: Gestão completa de vendas, equipe e configurações da sua respectiva loja.
  - `Dono`: Gestão consolidada e por loja de toda a rede de óticas subordinadas.
  - `Sistema`: Acesso irrestrito a todas as entidades e funcionalidades para suporte e homologação.
- **Frontend**:
  - `AuthenticatedLayout.vue`: Seletor de ótica no topo habilitado dinamicamente para usuários `Sistema` (badge `SISTEMA ⚡`) e `Dono` (badge `REDE 🏢`), exibindo apenas as filiais pertencentes ao seu escopo.
  - `Users/Index.vue`: Adicionado suporte ao perfil `Dono`, listagem com badge da loja e dropdown de seleção de unidade para transferência de funcionários da rede.

### 2. Impacto no Banco de Dados / Entidades
- `Otica`: Adicionada coluna `MatrizId` (Guid, Nullable, FK para `oticas(Id)` com `OnDelete(DeleteBehavior.Restrict)`).
- `PerfilUsuario`: Adicionado valor `Dono = 4`.
- Migration EF Core gerada: `AdicionaHierarquiaMatrizFiliaisEPerfilDono`.

### 3. Validação e Verificação
- Backend: `dotnet build` executado com sucesso (0 erros, 1 warning inócuo de nullable em Razor).
- Frontend: `npm run build` executado com sucesso via Vite.

### 4. Riscos / Notas para Próximas Sessões
- Executar a migration no PostgreSQL via Docker na VPS e rodar o script de associação cirúrgica de dados para amarrar a Matriz às filiais (Travessa Itália e Parque).

---

## [LOG-004] - 2026-10-12: Implementação Completa dos Ajustes Operacionais e de Auditoria da OS (Demanda PDF de 5 Páginas)
- **Autor/Agente**: Arquiteto de Software & Assistente de IA Avançado (Antigravity)
- **Módulos Afetados**: `RETSYS.Domain` (`OrdemServico`), `RETSYS.Infrastructure` (Migration `AddUltimaEdicaoLogNaOS`), `RETSYS.Web` (`CaixaController`, `OrdensServicoController`, `Admin/Fechamento/Index.vue`, `OrdensServico/Create.vue`, `OrdensServico/Index.vue`)
- **Contexto & Solicitante**: Atendimento integral ao documento de 5 páginas de requisitos operacionais do cliente:
  1. Fechamento de Caixa com tratamento de datas e valores nulos;
  2. Edição de OS Completa pelo Admin pós-emissão com recálculo e auditoria;
  3. Auditoria visual discreta com badge e histórico detalhado no modal da prancheta;
  4. Ordem dos campos de receita (Longe -> Perto -> Adição -> Medidas) com cálculo automático de Adição;
  5. Vendedora amarrada ao usuário logado na emissão e busca unificada de OS por auto-detecção.

### 1. Resumo das Alterações
- **Fechamento de Caixa (`CaixaController.cs` e `Admin/Fechamento/Index.vue`)**:
  - Implementado parse tolerante de datas suportando múltiplos formatos (`yyyy-MM-dd`, `dd/MM/yyyy`, ISO com hora), coalescência defensiva de valores nulos e filtro por loja com fallback gracioso.
- **Edição Completa de OS por Administrador (`OrdensServicoController.cs` - `EditarAdmin`)**:
  - Endpoint seguro com autorização restrita para perfis `Admin`, `Gerente` e `Sistema`.
  - Edição granular de dados gerais (`LojaVenda`, `VendedorId`, `DataEntrada`, `DataPrevistaEntrega`, `Status`, dados do médico), receita óptica (`OsReceita`), medidas técnicas e financeiro (`OsFinanceiro`).
  - Recálculo dinâmico da cadeia financeira no backend: $Bruto = Armacao + Lente$, $Liquido = Bruto - Desconto$, $Restante = Liquido - Entrada$.
  - Recálculo atômico e sincronização de comissões vinculadas à OS ($ValorBase = Liquido$).
  - **Trava de Segurança Contábil**: Validação que bloqueia alterações de data ou financeiro caso o período de comissão do vendedor esteja com status `FECHADO` ou `PAGO`.
  - **Trilha de Auditoria Granular**: Gravação em `OsAuditoriaLog` para cada campo modificado e carimbo `UltimaEdicaoLog` na OS (`"editada em dd/MM/yyyy HH:mm por [usuário]"`).
- **Interface de Edição e Auditoria Visual (`OrdensServico/Index.vue`)**:
  - Modal administrativo estruturado em 4 abas (Geral, Prescrição Clínica, Medidas Técnicas, Produtos & Valores) com recálculo reativo em tempo real de totais e parcelas.
  - Badge discreto e elegante na listagem de OS indicando a última edição.
  - Tabela completa de histórico de auditoria (`OsAuditoriaLog`) exibida na modal da prancheta com data/hora formatada, campo alterado, valor anterior, novo valor e operador.
  - Busca rápida unificada com auto-detecção via regex: se apenas dígitos busca por `NumeroOS`; se texto (≥ 3 caracteres) busca por `Cliente.Nome`, com debounce de 300ms e filtro por filial.
- **Prescrição Clínica & Emissão de OS (`OrdensServico/Create.vue`)**:
  - Reorganização dos blocos de refração conforme ordem oftalmológica: **1º Longe ➔ 2º Perto ➔ 3º Adição ➔ 4º Medidas Técnicas**.
  - Automação inteligente da Adição: ao preencher Longe e Perto, calcula automaticamente a Adição ($Adicao = Perto - Longe$) no evento `@blur`, permitindo edição manual assistida com badge de advertência `"ajustado manualmente"`.
  - Atendente fixo: campo de vendedora amarrado obrigatoriamente ao usuário logado na sessão (`auth.user.id`), com badge de exibição somente leitura.

### 2. Impacto no Banco de Dados / Entidades
- `OrdemServico`: Adicionada coluna `UltimaEdicaoLog` (`varchar(255)` nullable).
- Migration EF Core gerada: `20261012120727_AddUltimaEdicaoLogNaOS`.

### 3. Validação e Verificação
- Frontend: `npm run build` executado com sucesso (0 erros de compilação Vite).
- Backend: `dotnet build` executado com sucesso (0 erros de compilação .NET 10).
- Integridade: Migração gerada sem quebras estruturais ou perdas de dados.

### 4. Riscos / Notas para Próximas Sessões
- Aplicar a migration `20261012120727_AddUltimaEdicaoLogNaOS` no PostgreSQL de homologação/produção antes ou durante o deploy (executada automaticamente pelo `Program.cs` no start do container).

---

## Sessão: 2026-10-12 (Tarde) - Validação dos 5 Requisitos do Documento & Runbook da VPS

### 1. Status de Cobertura do Documento
1. **Edição Completa de OS (Admin)**: 100% implementada com auditoria visual (`OsAuditoriaLog`), badge discreto (`UltimaEdicaoLog`), recálculo integral da cadeia financeira e de comissões, e trava contábil para períodos com status `FECHADO` ou `PAGO`.
2. **Correção de Vínculo de Loja em OS**: Documentada a query e procedimento via `docker exec` no PostgreSQL e na tela de `/equipe` para alinhar vendedoras alocadas incorretamente à Matriz. Edição de OS permite reatribuir loja e vendedora retroativamente com recálculo de comissão.
3. **Busca Rápida de OS**: Campo único com auto-detecção numérica (`NumeroOS`) ou alfanumérica (`Cliente.Nome`), debounce de 300ms e combinação com filtros de loja, status e vendedora.
4. **Correção do Módulo de Fechamento**: Suporte multi-formato de datas (`yyyy-MM-dd`, `dd/MM/yyyy`, UTC), soma com `COALESCE`/null-coalescing (`0m`), filtro por loja e renderização segura de períodos zerados.
5. **Emissão de Nova OS**: Sequência clínica reorganizada (1º Longe ➔ 2º Perto ➔ 3º Adição ➔ 4º Medidas Técnicas), cálculo reativo de Adição no blur com badge de edição manual e atendente bloqueado para edição (somente leitura com usuário logado).

### 2. Validação de Build
- `dotnet build`: Sucesso (0 erros).
- `npm run build`: Sucesso (0 erros no Vite).

### 3. Procedimento Operacional para a VPS
- `git pull origin main`
- `docker compose build --no-cache retsys_app`
- `docker compose up -d retsys_app`
- Comandos psql documentados para conferência de `OticaId` das vendedoras.



---

## [LOG-004] - 2026-10-28: RETSYS 2.0 - Arquitetura Especialista de Ótica & Paridade de Mercado

- **Autor/Agente**: Arquiteto de Software & Assistente de IA Avançado (Antigravity)
- **Módulos Afetados**: `RETSYS.Domain`, `RETSYS.Infrastructure`, `RETSYS.Web` (Controllers & Frontend), `.Agents/`, `docker-compose.yml`, `nginx`
- **Contexto & Solicitante**: Evolução estratégica do RETSYS baseada nos líderes de mercado de óticas (SSÓtica e PWI), com preservação integral da base restaurada da VPS (143 clientes, 139 OSs, 239 parcelas, 117 marcas). Atendimento à solicitação de verificação de firewall UFW da VPS, documentação completa e preparação para deploy.

### 1. Resumo das Alterações
1. **Central de CRM Óptico & Recall Anual (`/crm`)**:
   - Criação do `CrmController` e da página `Frontend/Pages/Crm/Index.vue`.
   - Monitoramento de receitas com mais de 330 dias (11 meses) a 480 dias para retorno oftalmológico.
   - Disparo de mensagens personalizadas no WhatsApp em 1 clique com primeiro nome e nome do médico.
   - Pós-venda para clientes com óculos entregues entre 7 e 45 dias para suporte de adaptação multifocal.
   - Acompanhamento de aniversariantes do mês com destaque para quem faz aniversário no dia.
2. **Prescrições Clínicas & Histórico Diópico (`ReceitaCliente`)**:
   - Criação da entidade `ReceitaCliente` e do `ReceitasController` (`/clientes/{id}/receitas`).
   - Interface `Frontend/Pages/Clientes/FichaClinica.vue` para consulta da evolução temporal de graus de longe, perto, adição, DNP, alturas e medidas de armação ao longo dos anos.
   - Botões de acesso direto adicionados em `Clientes/Index.vue` e `Clientes/Historico.vue`.
3. **PDV de Balcão & Venda Rápida de Produtos (`/vendas/pdv` e `/vendas`)**:
   - Criação das entidades `VendaBalcao` e `ItemVendaBalcao`.
   - Criação do `VendasBalcaoController` e telas `Vendas/PdvBalcao.vue` e `Vendas/Index.vue`.
   - Faturamento ágil de armações solares, lentes de contato e acessórios com baixa em estoque e emissão de comprovante térmico não-fiscal 80mm (`Vendas/ComprovanteTermico.vue`).
4. **Crediário Próprio & Impressão de Carnê (`/ordens/{id}/carne`)**:
   - Criação do `CarneController` e da página `Frontend/Pages/Carne/ImpressaoCarne.vue`.
   - Layout de carnê com canhoto destacável da ótica (via da loja) + corpo de cobrança (via do cliente), dados cadastrais, número da parcela, cálculo de atraso e vencimento.
   - Endpoint de quitação de parcelas com juros e multa (`/parcelas/{parcelaId}/quitar`).
   - Botão de impressão de Carnê adicionado diretamente na listagem de OS e no modal de detalhes da prancheta.
5. **Esteira Especialista de Laboratório & Lensometria (`/laboratorio`)**:
   - Reformulação da página `Laboratory/Index.vue` para exibição reativa de ordens laboratoriais (`Ordens` e `FilaMontagem`).
   - Filtros por etapas: Balcão/Aguardando, Lab Externo, Em Montagem, Pronto Retirada.
   - Modal de envio para laboratórios parceiros (Essilor, Hoya, Zeiss, Lab Local) com número de pedido/rastreio.
   - Botão e indicador de conferência técnica no lensômetro com data/hora e operador responsável.
6. **Catálogo Óptico Avançado**:
   - Armações enriquecidas com `Formato`, `Genero`, `Aro`, `Ponte`, `Haste`.
   - Lentes com índices de refração, tratamentos e faixas de dioptrias.

### 2. Impacto no Banco de Dados / Entidades
- Migration EF Core gerada: `20261028150507_EvolucaoEspecialistaOtica`.
- Novas Tabelas: `receitas_cliente`, `vendas_balcao`, `itens_venda_balcao`.
- Novas Colunas:
  - `armacoes`: `Formato`, `Genero`, `Aro`, `Ponte`, `Haste`.
  - `ordens_servico`: `ReceitaClienteId`, `EtapaLaboratorio`, `LaboratorioParceiro`, `NumeroPedidoLaboratorio`, `DataEnvioLaboratorio`, `DataRetornoLaboratorio`, `ConferidoLensometro`, `ConferidoLensometroEm`, `ConferidoLensometroPorId`.
  - `parcelas_pagamento`: `NumeroCarne`, `JurosAtraso`, `MultaAtraso`, `ValorPago`, `ObservacaoPagamento`.

### 3. Validação e Verificação
- **Compilação Backend (.NET 10)**: `dotnet build RETSYS.Web/RETSYS.Web.csproj --no-restore -nodereuse:false -maxcpucount:1` -> **Compilação com êxito: 0 Avisos, 0 Erros**.
- **Compilação Frontend (Vite v8)**: `npm run build` -> **Sucesso absoluto em 7.7s**, gerando bundles otimizados em `wwwroot/assets/`.
- **Integridade da Base**: Todos os 143 clientes e 139 OSs restaurados do backup preservados.

### 4. Segurança de Rede e Firewall UFW (VPS)
- Avaliação detalhada do `docker-compose.yml` e `nginx/wo-retsys.tech`:
  - Todos os containers Docker (`retsys_db`, `ollama`, `retsys_app`) estão restritos à interface de loopback `127.0.0.1`.
  - O UFW na VPS precisa manter liberadas **somente as portas 22 (SSH), 80 (HTTP) e 443 (HTTPS)**.
  - As portas 5000, 5432 e 11434 NÃO devem ser abertas no firewall, garantindo isolamento total contra invasões.

## [LOG-005] - 2026-10-28: Remodelação Visual Completa & Segregação Estrita Multi-Loja (PWI / ssOtica)
- **Autor/Agente**: Arquiteto de Software & Assistente de IA Avançado (Antigravity)
- **Módulos Afetados**: `RETSYS.Web` (Program.cs, Controllers, Frontend/Shared/AuthenticatedLayout.vue, Pages/OrdensServico)
- **Contexto & Solicitante**: Atendimento à exigência do usuário de remodelar por completo a interface do sistema, eliminando a bagunça visual e desorganização, inspirando-se diretamente nas interfaces líderes de mercado (ssOtica e PWI), com garantia absoluta de separação de dados por ótica ativa (Matriz e Filiais) e seletor rápido no topo.

### 1. Resumo das Alterações
1. **Segregação Estrita Multi-Loja no Backend**:
   - `OrdensServicoController.cs`: Ajustado `lojaAlvoId` para padronizar estritamente pelo `oticaId` da sessão ativa, impedindo mistura involuntária de OSs de filiais na mesma listagem.
   - `TenantController.cs` & `SistemaController.cs`: Permitido que perfis `Admin`, `Gerente` e `Dono` possam alternar a ótica ativa via `POST /sistema/trocar-otica`.
   - `Program.cs`: Compartilhamento da lista de lojas (`oticasDisponiveis`) estendido para `Admin` e `Gerente`, permitindo alternância instantânea.
2. **Remodelação do Shell Global (`AuthenticatedLayout.vue`)**:
   - Cabeçalho superior (Top Navbar) modernizado com **Seletor de Ótica Ativa** em destaque com dropdown elegante para alternar entre as 3 lojas cadastradas (Ótica Retina Matriz, Retina Travessa Itália, Retina Parque).
   - Botões de ação rápida no header: `[ + Nova OS ]` e `[ 🛒 PDV Balcão ]`.
   - Sidebar reorganizada em 6 grupos lógicos limpos (Painel Geral, Atendimento & Vendas, CRM & Prescrições, Financeiro & Carnês, Estoque & Catálogo, Gestão da Rede), removendo poluição visual e inconsistências de badges.
3. **Visão Integrada de OSs (`OrdensServico/Index.vue`)**:
   - Controle segmentado de visualização no topo: `[ 📋 Lista de OSs ]` | `[ 🔬 Laboratório ]` | `[ 📊 Quadro Kanban ]` + `+ Emitir Nova OS`.
   - Ações de linha simplificadas e objetivas: 👁️ OS, 💬 WhatsApp, 📜 Carnê, 🚚 Entregar, ✏️ Editar.
4. **Otimização do Quadro Kanban (`OrdensServico/Kanban.vue`)**:
   - Removido banner redundante que consumia espaço vertical, aproveitando o seletor global do header.

### 2. Validação e Verificação
- **Frontend Build**: `npm run build` executado com êxito (144 módulos transformados, 0 erros).
- **Backend Build**: `dotnet build` executado com 0 avisos e 0 erros.
- **Git**: Código pronto para commit e push na branch `main`.

## [LOG-006] - 2026-10-28: Remodelação Completa e Definitiva de Todas as Telas e Criação do Manual do Usuário em PDF
- **Autor/Agente**: Arquiteto de Software & Assistente de IA Avançado (Antigravity)
- **Módulos Afetados**: `RETSYS.Web/Frontend/Pages/` (`OrdensServico/Index.vue`, `OrdensServico/Create.vue`, `Dashboard/Index.vue`, `Clientes/Index.vue`, `Crm/Index.vue`, `Vendas/PdvBalcao.vue`), `MANUAL_DO_USUARIO_RETSYS.html`, `MANUAL_DO_USUARIO_RETSYS.pdf`
- **Contexto & Solicitante**: Atendimento à exigência enfática do usuário de remodelar o sistema por completo, substituindo integralmente os layouts legados e desorganizados por novas interfaces de alto padrão óptico (ssÓtica e PWI Vixen), com esteira de laboratório integrada, emissão em 4 etapas, CRM ágil, PDV de balcão reativo e manual do usuário não-técnico gerado em formato PDF.

### 1. Resumo das Telas Completamente Remodeladas
1. **`OrdensServico/Index.vue`**:
   - Reconstruído do zero: Cards de status no topo (Todas, Aguardando Lente, Em Montagem, Prontas, Entregues), busca inteligente em tempo real por Nº OS, Cliente ou CPF, segregação estrita por loja no cabeçalho e tabela com prancheta técnica rápida, carnê e entrega.
2. **`OrdensServico/Create.vue`**:
   - Reconstruído em Stepper visual de 4 etapas:
     - Etapa 1: Cliente & Atendente (busca por CPF e dados de contato).
     - Etapa 2: Refração Clínica (Grade médica OD/OE, cálculo automático de Adição, DNP, alturas de montagem e médico).
     - Etapa 3: Produtos (Armação do catálogo ou própria, tabela de lentes e laboratório parceiro Essilor/Hoya/Zeiss).
     - Etapa 4: Fechamento Financeiro (Totalizadores, desconto, entrada, formas de pagamento e crediário/carnê).
3. **`Dashboard/Index.vue`**:
   - Command Center óptico completo: KPIs do dia (OS Hoje, Faturado, Lentes Pendentes, Prontos no Balcão, Atrasados, Comissões), curva de faturamento diário dos últimos 30 dias, ações rápidas e alertas de laboratório.
4. **`Clientes/Index.vue`**:
   - Ficha e carteira de clientes com prontuário visual, acesso direto à ficha clínica de receitas e botão de WhatsApp em 1 clique.
5. **`Crm/Index.vue`**:
   - Central de Recall segmentada: Retorno de 1 ano com WhatsApp automático, pós-venda multifocal e aniversariantes do mês.
6. **`Vendas/PdvBalcao.vue`**:
   - Frente de caixa óptica ágil: catálogo visual de pronta-entrega (óculos de sol, armações avulsas, lentes de contato e acessórios), carrinho em tempo real e comprovante térmico.

### 2. Manual Operacional Não-Técnico em PDF
- Desenvolvido e compilado `MANUAL_DO_USUARIO_RETSYS.pdf` (1.4 MB) e sua versão web `MANUAL_DO_USUARIO_RETSYS.html`, com guia ilustrado passo a passo para atendentes, gerentes e proprietários de ótica.

### 3. Validação de Build
- **Frontend**: `npm run build` executado com êxito (143 módulos, 0 erros).
- **Backend**: `dotnet build` executado com êxito (0 erros, 0 avisos).


## [LOG-007] - 2026-10-28: Correção Crítica do Histórico de Vendas (/vendas), Quirks Mode e Unificação Comercial
- **Autor/Agente**: Arquiteto de Software & Assistente de IA Avançado (Antigravity)
- **Módulos Afetados**: `RETSYS.Web/Controllers/VendasController.cs` (removido), `RETSYS.Web/Controllers/VendasBalcaoController.cs`, `RETSYS.Web/Frontend/Pages/Vendas/Index.vue`, `RETSYS.Web/Views/App.cshtml`
- **Contexto & Solicitante**: Usuário reportou em produção (`https://wo-retsys.tech/`) tela de erro "Ocorreu um erro inesperado. Nossa equipe já foi notificada" e erro no console `TypeError: can't access property "default", e is undefined` ao clicar em "Histórico de Vendas", juntamente com aviso de "Quirks Mode" no navegador.

### 1. Diagnóstico da Causa Raiz
1. **Conflito de Rotas Ambíguas (AmbiguousMatchException - HTTP 500)**:
   - Havia dois controllers registrando exatamente a rota `[HttpGet("/vendas")]`:
     - `VendasController.cs` (controller stub redundante sem injeção de dependências ou dados).
     - `VendasBalcaoController.cs` (controller multi-tenant real).
   - O ASP.NET Core disparava `AmbiguousMatchException` em tempo de execução, caindo no `UseExceptionHandler` que retornava HTTP 500 em texto puro. O Inertia tentava carregar o componente e falhava com `e is undefined` (`can't access property 'default'`).
2. **Quirks Mode no Navegador**:
   - `RETSYS.Web/Views/App.cshtml` continha blocos C# Razor `@using` e `@{ ... }` antes do `<!DOCTYPE html>`, emitindo quebras de linha em branco antes do doctype e forçando navegadores (como Firefox) a entrar em Quirks Mode.
3. **Formatação de Data em LINQ PostgreSQL**:
   - As queries em `VendasBalcaoController.cs` continham `DataVenda.ToString("dd/MM/yyyy HH:mm")` dentro de projeções LINQ do EF Core, o que causava incompatibilidade de tradução SQL no Npgsql.

### 2. Resumo das Soluções Implementadas
1. **Eliminação da Rota Duplicada**:
   - Removido permanentemente o arquivo redundante `RETSYS.Web/Controllers/VendasController.cs`.
2. **Correção do DOCTYPE para Standards Mode**:
   - Reorganizado `RETSYS.Web/Views/App.cshtml` para garantir `<!DOCTYPE html>` exatamente na linha 1 do documento, eliminando o Quirks Mode.
3. **Projeção em Memória e Unificação Comercial no Histórico de Vendas**:
   - `VendasBalcaoController.Index`:
     - Executa busca segura no banco e projeta a formatação de data em memória C# com `.Select(...)` após `.ToListAsync()`.
     - Integra no Histórico de Vendas tanto as saídas de **PDV Balcão (VB-...)** quanto as de **Ordens de Serviço (OS-...)**, permitindo visualização consolidada com métricas de faturamento e totalizadores.
     - `Comprovante`: Consulta individual tratada em memória de forma segura.
4. **Remodelação de `Vendas/Index.vue`**:
   - KPI Cards no topo (Faturamento Filtrado, Total de Operações, Vendas Balcão, Ordens de Serviço).
   - Abas de navegação rápida: `[ Todas as Vendas ]`, `[ Apenas Balcão (VB) ]`, `[ Apenas OSs de Grau ]`.
   - Busca em tempo real, badges visuais e atalhos para emissão de cupom térmico e visualização de OS.

### 3. Validação e Verificação
- **Frontend Build**: `npm run build` executado com êxito (143 módulos, 0 erros).
- **Backend Build**: `dotnet build RETSYS.Web/RETSYS.Web.csproj --no-restore -nodereuse:false -maxcpucount:1` finalizado com êxito (0 erros, 0 avisos).

## [LOG-008] - 2026-10-29: Edição Completa de OS, Receita Padrão Oftalmológico com Grau de Perto e Disparos de WhatsApp
- **Autor/Agente**: Arquiteto de Software & Assistente de IA Avançado (Antigravity)
- **Módulos Afetados**: `RETSYS.Web/Controllers/OrdensServicoController.cs`, `RETSYS.Web/Controllers/ClientesController.cs`, `RETSYS.Web/Frontend/Pages/OrdensServico/Edit.vue` (novo), `RETSYS.Web/Frontend/Pages/OrdensServico/Create.vue`, `RETSYS.Web/Frontend/Pages/OrdensServico/Index.vue`, `RETSYS.Web/Frontend/Pages/Crm/Index.vue`, `RETSYS.Web/Frontend/app.js`
- **Contexto & Solicitante**: Usuário reportou que o botão de editar OS não funcionava, solicitou a adequação da tabela de receitas para o formato oftalmológico brasileiro contendo o campo explícito "Grau de Perto" com cálculo automático de Adição (com base na foto médica enviada do Hospital de Olhos Bragança - Dr. Ulysses Tachibana), integração de disparos de WhatsApp (Óculos Pronto, 1 Ano de Receita / Recall e Pós-venda de adaptação) e auditoria de todos os fluxos de navegação e CRUD.

### 1. Resumo das Soluções Implementadas
1. **Edição Completa de Ordem de Serviço (OS)**:
   - Identificada a causa raiz: `OrdensServico/Index.vue` chamava `abrirModalEditarAdmin` (inexistente) e não havia nem página `Edit.vue` nem endpoint `[HttpGet("/ordens/{id}/editar")]`.
   - Criada a página `RETSYS.Web/Frontend/Pages/OrdensServico/Edit.vue` estruturada em 5 abas completas:
     - Aba 1: Cliente e Dados de Atendimento (Vendedor, Loja, Datas, Status, Observações).
     - Aba 2: Receituário Clínico com Padrão Oftalmológico Brasileiro (Longe e Perto).
     - Aba 3: Produtos, Armação, Lente e Laboratório Parceiro (Essilor, Hoya, Zeiss, etc.).
     - Aba 4: Fechamento Financeiro, Descontos, Entrada e Parcelas.
     - Aba 5: Trilha de Auditoria com histórico de modificações (`OsAuditoriaLogs`).
   - Implementado endpoint `[HttpGet("/ordens/{id:guid}/editar")]` no `OrdensServicoController.cs` com pré-carregamento de todas as entidades relacionadas e permissão multi-tenant.
   - Ajustado `EditarAdmin` (`[HttpPost("/ordens/{id:guid}/editar")]`) com suporte a vendedores e gerentes da loja.

2. **Receituário Clínico Padrão Oftalmológico Brasileiro ("Grau de Perto")**:
   - Atualizados `Create.vue`, `Edit.vue` e a prancheta de `Index.vue` para exibir a grade clássica:
     - **Para Longe**: OD / OE (Esférico, Cilíndrico, Eixo, DP/DNP).
     - **Para Perto**: OD / OE (Esférico, Cilíndrico, Eixo, DP/DNP).
   - Reatividade bidirecional automática:
     - `Grau de Perto = Grau de Longe + Adição`.
     - `Adição = Grau de Perto - Grau de Longe`.
   - Persistência transparente e retrocompatível no backend: caso o operador preencha o grau de perto, a adição é automaticamente calculada e gravada em `OsReceita`.

3. **Módulo de Mensageria e Disparos de WhatsApp (CRM & Notificação Operacional)**:
   - **Óculos Pronto**: Na listagem e na prancheta de OSs (`Index.vue`), integrado modal inteligente de WhatsApp que abre automaticamente quando o status muda para `PRONTA` ou ao clicar no botão de WhatsApp da OS, gerando link formatado `https://api.whatsapp.com/send?phone=...` com mensagem personalizada contendo o nome do cliente, número da OS e convite para retirada na loja.
   - **Recall de 1 Ano & Pós-Venda**: No CRM (`Crm/Index.vue`), corrigido o mapeamento de props enviadas pelo backend (`ListaRecall`, `ListaPosVenda`, `ListaAniversariantes`), adicionado modal com template de mensagem personalizável e botão de disparo com 1 clique.
   - Rota `/clientes/aniversariantes` redirecionada de forma segura para `/crm`.

4. **Resolução de Erros de Roteamento no Inertia**:
   - Modificado `app.js` com resolvedor de páginas case-insensitive defensivo para evitar falhas `TypeError: can't access property "default", e is undefined`.

### 2. Validação e Verificação
- **Backend Build**: `dotnet build RETSYS.Web/RETSYS.Web.csproj --no-restore -nodereuse:false -maxcpucount:1` finalizado com **0 Erros e 0 Avisos**.
- **Frontend Build**: `npm --prefix RETSYS.Web run build` compilou 144 módulos com sucesso (3.28s) gerando `app-CdaYgI3D.js` e `app-DEjK2QdT.css`.


## [LOG-009] - 2026-10-30: Correção de Emissão de OS (Inertia JSON crash), Edição por Vendedor, Isolamento de Lojas e Medidas Técnicas de Armação/DP
- **Autor/Agente**: Arquiteto de Software & Assistente de IA Avançado (Antigravity)
- **Módulos Afetados**: `RETSYS.Web/Controllers/OrdensServicoController.cs`, `RETSYS.Web/Frontend/Pages/OrdensServico/Create.vue`, `RETSYS.Web/Frontend/Pages/OrdensServico/Edit.vue`, `RETSYS.Web/Frontend/Pages/OrdensServico/Index.vue`
- **Contexto & Solicitante**:
  1. Erro na emissão de OS com modal Inertia: `All Inertia requests must receive a valid Inertia response, however a plain JSON response was received. {"mensagem":"Informe o nome do cliente."}`.
  2. Alterações de dados da OS não salvavam quando o usuário era vendedor/atendente da loja.
  3. Ordens de Serviço apareciam misturadas entre as diferentes filiais da rede.
  4. Faltavam campos de medidas da armação e DP (Aro, Ponte, DM, VERT, Haste, CO OD/OE, DP Total) na emissão e na edição.
  5. Dúvidas sobre configuração e automação de disparos de WhatsApp por loja.

### 1. Resumo das Soluções Implementadas
1. **Eliminação do Erro de Resposta JSON no Inertia durante Emissão de OS**:
   - `OrdensServicoController.Store`:
     - O frontend enviava requisição com payload JSON (`router.post`), mas a action esperava `[FromForm] IFormCollection`. Os campos vinham vazios e disparavam `BadRequest(new { mensagem = "Informe o nome do cliente." })`, que o Inertia intercepta como erro de protocolo.
     - Criado DTO tipado `CriarOsRequest` mapeando todos os parâmetros de cliente, receita, produtos e financeiro.
     - Implementado suporte unificado para requisições JSON e Form, além do método `RetornarErroValidacao` que detecta requisições Inertia (`X-Inertia`) e redireciona com mensagem flash via `Inertia.Share("erro", ...)` em vez de devolver JSON cru.
     - Sucesso redireciona com `Inertia.Share("sucesso", ...); return RedirectToAction(nameof(Index))`.

2. **Permissão e Persistência na Edição de OS por Vendedores / Atendentes**:
   - Em `EditarAdmin`, a checagem anterior `if (!EhAdministrador())` bloqueava e retornava antecipadamente antes de salvar caso o usuário logado não fosse `ADMIN` ou `DONO`.
   - Agora a verificação de permissão ocorre após o carregamento da OS, permitindo que operadores da mesma loja ou rede (`os.OticaId == oticaId || idsRede.Contains(os.OticaId)`) alterem e salvem os dados com sucesso, registrando o histórico de auditoria (`OsAuditoriaLogs`).

3. **Isolamento Estrito Multi-Loja (Fim da Mistura de OSs)**:
   - Em `OrdensServicoController.Index`, a listagem anteriormente unificava todas as óticas da rede (`idsRede.Contains(os.OticaId)`) por padrão caso nenhum filtro explícito fosse passado.
   - Corrigido: por padrão, cada operador enxerga **apenas as OSs da sua ótica ativa** (`lojaFiltroEfetivo = oticaId`). A visualização de todas as unidades da rede só é ativada se um administrador/dono selecionar explicitamente a opção correspondente no filtro.

4. **Medidas Completas de Armação, Centralização Técnica e DP Total**:
   - Tanto em `Create.vue` quanto em `Edit.vue`:
     - Adicionado campo **DP Total (mm)** com cálculo bidirecional automático para DNP OD e OE (`DP / 2`).
     - Adicionados campos técnicos laboratoriais: **Aro / Horizontal (A)**, **Ponte / DBL (PO)**, **Maior Diagonal (DM)**, **Vertical (VERT)**, **Haste**, **CO OD** e **CO OE**.
     - Adicionado campo de **Observações Técnicas da Receita / Montagem** e seleção do tipo de especialista.
     - Sincronização completa de gravação com os campos existentes da tabela `OsReceita`.

5. **Disparos e Configuração de WhatsApp por Loja**:
   - Cada ótica possui seu próprio número e templates de mensagem independentes na tabela `ConfiguracoesLoja`, editáveis em `/configuracoes`.
   - Na prancheta e listagem de OSs (`Index.vue`), o botão de WhatsApp utiliza dinamicamente o nome da loja da venda (`os.lojaVenda`), preenchendo a mensagem oficial com um clique.

### 2. Validação e Verificação
- **Frontend Build**: `npm --prefix RETSYS.Web run build` compilou 144 módulos com êxito (12.87s), gerando `app-B9CSKh-f.js` e `app-nfj1Be0s.css`.
- **Backend Build**: `dotnet build RETSYS.Web/RETSYS.Web.csproj --no-restore -nodereuse:false -maxcpucount:1` finalizado com **0 Erros e 0 Avisos**.
