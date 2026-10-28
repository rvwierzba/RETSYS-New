# Current State - RETSYS (Estado Atual do Projeto)

## 1. Módulos Codificados e Totalmente Funcionais (RETSYS 2.0 Especialista)

### 1.1 Autenticação, Usuários e Multi-Tenancy
- **Autenticação por Cookie**: Implementada via ASP.NET Core Authentication (`/login`, `/logout`), armazenando perfil (`Admin`, `Gerente`, `Vendedor`, `Dono`) e `OticaId` nas claims.
- **Isolamento de Tenant**: Baseado na classe abstrata `TenantController`, garantindo que todas as consultas ao banco filtrem obrigatoriamente pelo `OticaId` do usuário logado.
- **Segurança**: Senhas criptografadas via `BCrypt.Net` Enhanced Hash (fator de custo 12).
- **Controle de Desconto e Comissão por Vendedora**:
  - Limite de desconto padrão inicial de `5.00%` para novas vendedoras (extensível por gerentes/admins).
  - Percentual individual de comissão (`PercentualComissao`, padrão `3.00%`).
  - Flags de controle `Ativo` e `ComissaoAtiva`.

### 1.2 Ordens de Serviço (OS) & Prescrições Ópticas
- **Gerenciamento Completo & Emissão de OS**:
  - Cadastro de novas OS com vínculo obrigatório com Cliente, Vendedor e Ótica.
  - Vendedor amarrado automaticamente ao usuário autenticado na sessão (`VendedorId`), exibido como badge somente leitura no balcão de emissão.
  - Sequência clínica padronizada: **1º Longe ➔ 2º Perto ➔ 3º Adição ➔ 4º Medidas Técnicas**.
  - Automação inteligente de refração: cálculo automático de Adição ($Adicao = Perto - Longe$) no blur de longe e perto com fallback manual e badge `"ajustado manualmente"`.
  - Suporte a OS Retroativas/Históricas (`IsRetroativa = true`), permitindo cadastrar compras antigas com armação e lente descritas manualmente (`ArmacaoModeloManual`, `LenteDescricaoManual`) sem impactar o estoque atual ou caixa do dia.
  - Relacionamentos 1:1 obrigatórios com `OsReceita` e `OsFinanceiro`, e 1:N com `ParcelaPagamento`.
  - Fluxo de Status: `EM_ABERTO` ➔ `CONFIRMADA` ➔ `AGUARDANDO_LENTE` ➔ `EM_MONTAGEM` ➔ `PRONTA` ➔ `ENTREGUE` (ou `CANCELADO`).
- **Busca Rápida Unificada na Listagem (`/ordens`)**:
  - Input único com debounce de 300ms e auto-detecção:
    - Dígitos puros: busca exata por `NumeroOS`.
    - Texto (≥ 3 caracteres): busca textual por `Cliente.Nome`.
  - Seletor de filtro por unidade física (`LojaVenda`).
  - Botão de acesso direto e impressão de Carnê / Boletos (`/ordens/{id}/carne`) na listagem e na prancheta.
- **Edição Administrativa Completa Pós-Emissão (`/ordens/editar-admin/{id}`)**:
  - Acesso exclusivo para perfis `Admin`, `Gerente` e `Sistema`.
  - Modal com abas: Dados Gerais, Prescrição Clínica, Medidas Técnicas e Produtos/Valores.
  - Recálculo atômico das comissões do vendedor associado com base no novo valor líquido.
  - **Trava de Segurança Contábil**: Rejeita alterações se o período de comissão correspondente estiver `FECHADO` ou `PAGO`.
  - **Trilha de Auditoria Granular**: Grava registros imutáveis em `OsAuditoriaLog` e carimba `UltimaEdicaoLog` na OS.

### 1.3 Central de CRM & Recall Óptico (`/crm`)
- **Recall de Consulta (1 Ano / 330 a 480 dias)**:
  - Monitora automaticamente a data do último exame oftalmológico ou receita do cliente.
  - Alerta pacientes com receitas prestes a vencer ou vencidas.
  - Botão de **WhatsApp em 1 clique** com template personalizado contendo o primeiro nome do cliente, nome do médico oftalmologista e data do último exame.
- **Aniversariantes do Mês**:
  - Agrupamento inteligente pelo dia de nascimento com destaque visual para quem faz aniversário hoje.
  - Disparo de mensagem de felicitações e cupom de desconto via WhatsApp.
- **Pós-Venda Multifocal & Adaptação**:
  - Filtro de clientes cuja OS foi entregue entre 7 e 45 dias atrás.
  - Mensagem de checagem de adaptação das lentes multifocais e convite para ajustes gratuitos na armação.

### 1.4 Ficha Clínica do Paciente & Prescrições Independentes (`/clientes/{id}/receitas`)
- **Entidade `ReceitaCliente`**: Histórico oftalmológico independente da Ordem de Serviço, permitindo registrar prescrições de médicos diferentes ao longo de anos.
- **Comparativo Temporal de Graus**: Visualizador de evolução diópica (Longe, Perto, Adição, DNP, Altura, Eixo, Aro, Ponte, DM, Vert).
- **Indicador de Validade**: Sinalização clara se a receita médica está vencida (> 1 ano) ou precisa de retorno anual.
- **Upload de Imagem/Foto da Receita**: Suporte a armazenamento de imagem física da prescrição.

### 1.5 Venda Rápida de Balcão & PDV Ágil (`/vendas/pdv` e `/vendas`)
- **PDV Ágil sem OS Obrigatória**:
  - Faturamento rápido de armações solares, armações de grau avulsas, lentes de contato, estojos, flanelas e kits de limpeza.
  - Carrinho de compras reativo no balcão com seleção instantânea de cliente (ou Consumidor Final) e atendente.
  - Baixa automática no estoque físico de armações com registro da movimentação.
  - Emissão de Comprovante Térmico (80mm) pronto para impressora não fiscal (`/vendas/{id}/comprovante`).

### 1.6 Crediário Próprio & Módulo de Carnês (`/ordens/{id}/carne`)
- **Carnê Tradicional de Ótica**:
  - Geração de lâminas de carnê em layout profissional para impressoras térmicas (80mm) e papel A4.
  - Canhoto destacável da ótica (via do caixa) + corpo da parcela (via do cliente) com nome da loja, CNPJ, dados do cliente, CPF, endereço, número da OS, número da parcela (ex: 01/06) e data de vencimento.
- **Régua de Inadimplência**:
  - Cálculo automático de dias em atraso (`DiasAtraso`) para cada parcela em aberto.
  - Suporte a quitação com acréscimo de juros e multa por atraso (`/parcelas/{id}/quitar`).

### 1.7 Esteira Especialista de Laboratório & Montagem (`/laboratorio` e `/ordens/kanban`)
- **Ciclo Completo da Lente**:
  - Etapas formais: `BALCAO` ➔ `LABORATORIO_EXTERNO` ➔ `EM_MONTAGEM` ➔ `PRONTO_RETIRADA`.
  - Gestão de Pedidos a Laboratórios Parceiros (Essilor, Hoya, Zeiss, Laboratórios Locais) com registro de número do pedido para rastreamento.
  - **Lensometria & Controle de Qualidade**: Flag técnica e carimbo de data/hora confirmando que a lente foi aferida no lensômetro antes da liberação ao cliente.
- **Kanban Interativo de Ordens (`/ordens/kanban`)**:
  - Painel drag-and-drop completo com colunas por status, alertas inteligentes de lentes atrasadas e previsão para hoje.

### 1.8 Caixa & Fluxo de Recebimentos (`/caixa`)
- Listagem em linha única por OS com totalizadores de Total Vendido, Total Recebido em Dinheiro/Cartão/PIX e Total a Receber.
- Quitação de saldo devedor na entrega dos óculos com baixa imediata.
- Integração PIX via OpenPix com QR Code dinâmico.

### 1.9 Comissões & Fechamento
- Extrato individual de vendedoras (`/minhas-comissoes`) e painel administrativo (`/admin/comissoes`, `/admin/fechamento`) com status `ABERTO`, `FECHADO`, `PAGO`.

### 1.10 Catálogo Óptico Enriquecido
- Armações detalhadas com Formato (Aviador, Redondo, Quadrado, Gatinho, Retangular, etc.), Gênero, Aro, Ponte e Haste.
- Tabela de Lentes com índices de refração (1.56, 1.67, 1.74), tratamentos (Crizal, BlueControl, Antirreflexo) e flag de surfaçagem.

---

## 2. Status de Build, Testes e Conformidade

- **Backend .NET 10**: Compilação via MSBuild com **0 Erros e 0 Avisos**.
- **Frontend Vite v8 / Vue 3**: Bundle de produção compilado com **0 Erros** (`npm run build`).
- **Banco de Dados PostgreSQL**: Migration `20261028150507_EvolucaoEspecialistaOtica` gerada preservando 100% dos dados pré-existentes.
- **Firewall VPS (UFW)**: Total conformidade. Apenas portas públicas 22 (SSH), 80 (HTTP) e 443 (HTTPS) necessárias; serviços internos (PostgreSQL 5432, Ollama 11434, App 5000) isolados exclusivamente no `127.0.0.1`.
