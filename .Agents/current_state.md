# Current State - RETSYS (Estado Atual do Projeto)

## 1. Módulos Codificados e Totalmente Funcionais

### 1.1 Autenticação, Usuários e Multi-Tenancy
- **Autenticação por Cookie**: Implementada via ASP.NET Core Authentication (`/login`, `/logout`), armazenando perfil (`Admin`, `Gerente`, `Vendedor`) e `OticaId` nas claims.
- **Isolamento de Tenant**: Baseado na classe abstrata `TenantController`, garantindo que todas as consultas ao banco filtrem obrigatoriamente pelo `OticaId` do usuário logado.
- **Segurança**: Senhas criptografadas via `BCrypt.Net` Enhanced Hash (fator de custo 12).
- **Controle de Desconto e Comissão por Vendedora**:
  - Limite de desconto padrão inicial de `5.00%` para novas vendedoras (extensível por gerentes/admins).
  - Percentual individual de comissão (`PercentualComissao`, padrão `3.00%`).
  - Flags de controle `Ativo` e `ComissaoAtiva`.

### 1.2 Ordens de Serviço (OS) & Prescrições Ópticas
- **Gerenciamento Completo de OS**:
  - Cadastro de novas OS com vínculo obrigatório com Cliente, Vendedor e Ótica.
  - Suporte a OS Retroativas/Históricas (`IsRetroativa = true`), permitindo cadastrar compras antigas com armação e lente descritas manualmente (`ArmacaoModeloManual`, `LenteDescricaoManual`) sem impactar o estoque atual ou caixa do dia.
  - Relacionamentos 1:1 obrigatórios com `OsReceita` e `OsFinanceiro`, e 1:N com `ParcelaPagamento`.
  - Fluxo de Status: `EM_ABERTO` ➔ `EM_LABORATORIO` ➔ `PRONTO` ➔ `ENTREGUE` (ou `CANCELADO`).
- **Controle de Pedido de Lentes**:
  - Rastreamento de lentes encomendadas ao laboratório fornecedor (`LentePedida`, `DataPedidoLente`, `PedidoLentePorId`).
  - Alerta visual no Dashboard para OS com lentes não pedidas (com destaque crítico para pendências > 24 horas).
- **Regras Ópticas Computadas (`OsReceita`)**:
  - Esférico de perto calculado dinamicamente: `OdEsfericoPerto` e `OeEsfericoPerto` somam automaticamente a Adição (`Adicao`) ao Esférico de longe.
  - Cilíndrico e Eixo de perto mantidos idênticos aos de longe sem mutação de estado.
  - Medidas técnicas completas: DNP (OD/OE), Altura de Montagem (OD/OE), Aro, Diagonal Maior (DM), Vertical (Vert), Ponte (Po) e Centro Óptico (CoOd/CoOe).

### 1.3 Laboratório & Esteira de Montagem
- **Painel do Laboratório (`/laboratorio`)**:
  - Filtro em tempo real de OS que estão no status `EM_LABORATORIO`.
  - Exibição organizada de tabela de receitas com grau de longe e grau computado de perto, adição e tipo de lente selecionada para montagem técnica.

### 1.4 Caixa & Fluxo de Recebimentos
- **Terminal de Recebimentos (`/caixa`)**:
  - Listagem de parcelas a receber e recebidas (`ParcelaPagamento`).
  - Baixa manual de parcelas com registro imediato da data de pagamento.
  - Integração com **OpenPix**: Geração de cobrança imediata PIX via API, retornando QR Code em imagem e string Copia-e-Cola (`PixResponseDto`).
  - Conferência de pagamentos pelo Gerente (`PagamentoConferido`, `ConferidoPorId`, `DataConferencia`).
  - Quitação de saldo devedor no momento da retirada do óculos (`ValorRecebidoRetirada`, `FormaPagamentoRetirada`, `DataQuitacao`).

### 1.5 Gestão de Comissões
- **Painel da Vendedora (`/minhas-comissoes`)**:
  - Extrato detalhado de comissões por OS emitida no mês de referência selecionado (`AAAA-MM`).
  - Histórico de fechamentos com consolidação de vendas brutas e valor total de comissão (`FechamentoComissao`).
- **Painel Administrativo (`/admin/comissoes` e `/admin/fechamento`)**:
  - Parametrizador global de comissão (`ConfiguracaoComissao`).
  - Fechamento mensal de comissões por vendedora com controle de status (`ABERTO`, `FECHADO`, `PAGO`).

### 1.6 Clientes & CRM Óptico
- **Cadastro Completo**: Busca automática de endereço por CEP, CPF formatado com índice único por tenant.
- **Histórico Óptico**: Preservação da última receita conhecida diretamente no cadastro do cliente (`UltimaOdEsferico`, `UltimaOdCilindrico`, `UltimaAdicao`, etc.).
- **Dados Legados de Migração**: Campos informativos para migração de sistemas antigos (`ValorGasto`, `ProdutoAdquirido`, `DataUltimaCompra`).
- **Regra de Aniversariantes**: Método de domínio `FazAniversarioEm(DateTime)` para ações de CRM.

### 1.7 Estoque, Armações e Catálogo de Lentes
- **Armações**: Cadastro por Marca (`Marca`), SKU único por etiqueta, modelo/referência, cor, tamanho, material, fornecedor, preço de custo (visível apenas para Admin) e preço de venda, quantidade em estoque e gatilho de quantidade mínima.
- **Lentes & Tabela de Preços**:
  - `Lente`: Cadastro base por laboratório fabricante (Essilor, Hoya, Zeiss, etc.), tipo (`MONOFOCAL`, `BIFOCAL`, `PROGRESSIVA`, `OCUPACIONAL`), faixas de graduação e flag de lente Surfacada.
  - `LentePreco`: Tabela de variações relacionando índice de refração (1.56, 1.67, 1.74), descrição textual de tratamento (ex: Antirreflexo, Crizal, BlueControl), preço de custo e preço de venda.

### 1.8 IA Integrada (Ollama OCR)
- **Serviço de Leitura de Receitas (`ServicoOllama`)**:
  - Comunicação via requisição POST para o Ollama executando o modelo `moondream`.
  - Processamento de imagem em Base64 e parsing de resposta JSON para DTO `ResultadoLeituraReceitaDto`.

---

## 2. Incompletudes, Pendências e Débitos Técnicos

1. **Configuração de Endpoint do Ollama**:
   - O endereço base `http://ollama:11434/` está fixado diretamente no construtor de `ServicoOllama.cs`. Deve ser refatorado para ler de `appsettings.json` ou variável de ambiente via `IConfiguration`.

2. **Código de Auto-Sync SQL no Startup**:
   - Em `Program.cs`, há blocos de execução SQL bruta no carregamento inicial (`CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory"`, `ALTER TABLE "usuarios" ADD COLUMN IF NOT EXISTS "FotoUrl"`) para tratar inconsistências de deploy no Render/VPS.

3. **Repositórios e Camada de Aplicação (CQRS/Services)**:
   - A lógica de negócios e consultas complexas está majoritariamente concentrada dentro dos **Controllers** (`OrdensServicoController`, `DashboardController`, `ComissoesController`). Recomenda-se a gradual introdução de Services de Aplicação ou Commands/Queries para desacoplar os Controllers.

4. **Widget de Spotify**:
   - O player de áudio utiliza a sessão HTTP para controle simples de streaming de token sem fluxo OAuth completo com renovação automática por Refresh Token.

5. **Testes Automatizados**:
   - Inexistência atual de suíte de testes unitários ou de integração para a camada de domínio e infraestrutura.

