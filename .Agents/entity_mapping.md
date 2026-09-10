# Entity Mapping - RETSYS (Mapeamento de Entidades e Banco de Dados)

## 1. Convenções Globais de Mapeamento

- **ORM**: Entity Framework Core 10 operando em banco de dados PostgreSQL via driver `Npgsql`.
- **Nomenclatura de Tabelas**: Nomes em minúsculo, no plural, utilizando o padrão `snake_case` (ex: `ordens_servico`, `lentes_tabela_precos`).
- **Chaves Primárias (PK)**: Mapeadas prioritariamente como `Guid` (`uuid` no PostgreSQL) gerados via `Guid.NewGuid()`, com exceção de tabelas de configuração global com registro único (`ConfiguracaoComissao`, PK `int`).
- **Multi-Tenancy (Isolamento por Ótica)**: Todas as entidades principais contêm a FK `OticaId` referenciando a tabela `oticas`, acompanhada de índices únicos compostos por tenant quando aplicável.
- **Tipos e Precisão Numérica**:
  - Valores Monetários: `decimal(10,2)` ou `decimal(18,2)`.
  - Percentuais de Desconto e Comissão: `decimal(5,2)`.
  - Índice de Refração Óptica: `decimal(4,2)`.
  - Graus Ópticos (Dioptrias): `decimal(5,2)` (esférico/cilíndrico) e `decimal(4,2)` (adição).
  - Medidas de Centragem e Montagem: `decimal(4,1)` (DNP, altura, aro, DM, vertical, ponte, CO).

---

## 2. Documentação Exaustiva das Entidades

### 2.1 `Otica` (Tabela: `oticas`)
- `Id`: `Guid` (PK)
- `Nome`: `string` (max 150, Obrigatório)
- `CriadoEm`: `DateTime` (Default `UtcNow`)
- **Navegação**: `Usuarios` (1:N `Usuario`)

### 2.2 `Usuario` (Tabela: `usuarios`)
- `Id`: `Guid` (PK)
- `OticaId`: `Guid` (FK `oticas`, Restrict)
- `Nome`: `string` (max 100, Obrigatório)
- `Email`: `string` (max 150, Obrigatório, Índice Único)
- `SenhaHash`: `string` (Obrigatório, BCrypt Enhanced)
- `FilialLoja`: `string` (max 100)
- `FotoUrl`: `string?` (Caminho da foto de perfil)
- `Perfil`: Enum `PerfilUsuario` (Mapeado como `int`: `1 = Admin`, `2 = Vendedor`)
- `LimiteDesconto`: `decimal(5,2)` (Default `5.00`)
- `Ativo`: `bool` (Default `true`)
- `MetaMensal`: `decimal(18,2)`
- `ComissaoAtiva`: `bool` (Default `true`)
- `PercentualComissao`: `decimal(5,2)` (Default `3.00`)
- `UltimoAcesso`: `DateTime?`
- `CriadoEm`: `DateTime` (Default `UtcNow`)

### 2.3 `Cliente` (Tabela: `clientes`)
- `Id`: `Guid` (PK)
- `OticaId`: `Guid` (FK `oticas`, Restrict)
- `Nome`: `string` (max 150, Obrigatório)
- `CPF`: `string` (max 14, Obrigatório, **Índice Único Composto: `(OticaId, CPF)`**)
- `Telefone`: `string` (max 20, Obrigatório)
- `DataNascimento`: `DateTime?`
- `Logradouro`: `string` (max 150, Obrigatório)
- `Numero`: `string` (max 10, Obrigatório)
- `Complemento`: `string?` (max 60)
- `Bairro`: `string` (max 80, Obrigatório)
- `Cidade`: `string` (max 80, Obrigatório)
- `Estado`: `string` (max 2, Obrigatório)
- `Cep`: `string` (max 9, Obrigatório)
- `Convenio`: `string?` (max 100)
- `Email`: `string?` (max 150)
- `Observacoes`: `text?`
- **Campos de Migração (Dados Legados)**:
  - `ValorGasto`: `decimal(10,2)?`
  - `ProdutoAdquirido`: `string?` (max 150)
  - `DataUltimaCompra`: `DateTime?`
- **Campos da Última Receita Conhecida**:
  - `DataReceita`: `DateTime?`
  - `UltimaOdEsferico`, `UltimaOdCilindrico`, `UltimaOeEsferico`, `UltimaOeCilindrico`: `decimal(5,2)?`
  - `UltimaOdEixo`, `UltimaOeEixo`: `int?`
  - `UltimaAdicao`: `decimal(4,2)?`
  - `UltimaDnpOd`, `UltimaDnpOe`, `UltimaAlturaMontagem`: `decimal(4,1)?`
- `CreatedAt`, `UpdatedAt`: `DateTime`
- **Métodos de Domínio**: `FazAniversarioEm(DateTime data)`

### 2.4 `Marca` (Tabela: `marcas`)
- `Id`: `Guid` (PK)
- `OticaId`: `Guid` (FK `oticas`, Restrict)
- `Nome`: `string` (max 100, Obrigatório)
- `Descricao`: `string` (max 250)
- `Ativo`: `bool` (Default `true`)
- `CriadoEm`: `DateTime`

### 2.5 `Armacao` (Tabela: `armacoes`)
- `Id`: `Guid` (PK)
- `OticaId`: `Guid` (FK `oticas`, Restrict)
- `MarcaId`: `Guid` (FK `marcas`, Restrict)
- `CodigoSku`: `string` (max 50, Obrigatório)
- `ModeloReferencia`: `string` (max 100, Obrigatório)
- `Cor`: `string` (max 50)
- `Tamanho`: `string` (max 50)
- `Material`: `string` (max 100)
- `Fornecedor`: `string` (max 100)
- `PrecoCusto`: `decimal(18,2)` (Visível apenas para Admin/Gerente)
- `PrecoVenda`: `decimal(18,2)`
- `QuantidadeEstoque`: `int` (Default `0`)
- `QuantidadeMinima`: `int`
- `Ativo`: `bool` (Default `true`)
- `CriadoEm`: `DateTime`

### 2.6 `Lente` (Tabela: `lentes`)
- `Id`: `Guid` (PK)
- `OticaId`: `Guid` (FK `oticas`, Restrict)
- `CodigoSku`: `string` (max 50, Obrigatório)
- `Laboratorio`: `string` (max 100, Obrigatório - ex: Essilor, Hoya, Zeiss)
- `Tipo`: `string` (max 50, Obrigatório - Valores: `MONOFOCAL`, `BIFOCAL`, `PROGRESSIVA`, `OCUPACIONAL`)
- `GraduacaoMin`: `decimal(5,2)`
- `GraduacaoMax`: `decimal(5,2)`
- `Surfacada`: `bool` (Coluna `surfacada`, Default `false`)
- `Ativo`: `bool` (Default `true`)
- `CriadoEm`: `DateTime`
- **Navegação**: `Precos` (1:N `LentePreco`, Delete Cascade)

### 2.7 `LentePreco` (Tabela: `lentes_tabela_precos`)
- `Id`: `Guid` (PK)
- `LenteId`: `Guid` (FK `lentes`, Coluna `lente_id`)
- `Tipo`: `string` (Coluna `tipo`, Default `MONOFOCAL`)
- `IndiceRefracao`: `decimal(4,2)` (Coluna `indice_refracao` - ex: `1.56`, `1.67`, `1.74`)
- `Tratamento`: `string?` (max 100, Coluna `tratamento` - Texto livre)
- `PrecoCusto`: `decimal(10,2)` (Coluna `preco_custo`)
- `PrecoVenda`: `decimal(10,2)` (Coluna `preco_venda`)
- `Ativo`: `bool` (Coluna `ativo`, Default `true`)

### 2.8 `OrdemServico` (Tabela: `ordens_servico`)
- `Id`: `Guid` (PK)
- `OticaId`: `Guid` (FK `oticas`, Restrict)
- `NumeroOS`: `string` (max 50, Obrigatório, **Índice Único Composto: `(OticaId, NumeroOS)`**)
- `ClienteId`: `Guid` (FK `clientes`, Restrict)
- `VendedorId`: `Guid?` (FK `usuarios`, Restrict - Opcional para OS legadas)
- `DataEntrada`: `DateTime` (Default `UtcNow`)
- `DataPrevistaEntrega`: `DateTime`
- `DataEntregaReal`: `DateTime?`
- `Status`: `string` (max 50, Valores: `EM_ABERTO`, `EM_LABORATORIO`, `PRONTO`, `ENTREGUE`, `CANCELADO`)
- `MedicoNome`: `string?` (max 100)
- `MedicoCrm`: `string?` (max 20)
- `MedicoTipo`: `string` (max 30, Default `NAO_ESPECIFICADO` - Valores: `OFTALMOLOGISTA`, `OPTOMETRISTA`, `NAO_ESPECIFICADO`)
- `Observacoes`: `text?`
- `CreatedAt`: `DateTime`
- **Controle de Pedido de Lentes**:
  - `LentePedida`: `bool` (Default `false`)
  - `DataPedidoLente`: `DateTime?`
  - `PedidoLentePorId`: `Guid?` (FK `usuarios`, Restrict)
- **Campos para Suporte Retroativo/Histórico**:
  - `IsRetroativa`: `bool` (Default `false`)
  - `ArmacaoModeloManual`: `string?` (max 150)
  - `LenteDescricaoManual`: `string?` (max 200)
  - `Ativo`: `bool` (Default `true`)

### 2.9 `OsReceita` (Tabela: `os_receita`)
- `OsId`: `Guid` (PK e FK `ordens_servico`, Delete Cascade - **Garante Relacionamento 1:1**)
- `OdEsferico`, `OdCilindrico`, `OeEsferico`, `OeCilindrico`: `decimal(5,2)`
- `OdEixo`, `OeEixo`: `int`
- `Adicao`: `decimal(4,2)?`
- `DnpOd`, `DnpOe`: `decimal(4,1)`
- `AlturaMontagemOd`, `AlturaMontagemOe`: `decimal(4,1)?`
- `Aro`, `Dm`, `Vert`, `Po`, `CoOd`, `CoOe`: `decimal(4,1)?`
- `ObsReceita`: `text?`
- **Propriedades Computadas (Ignoradas no Banco via `b.Ignore(...)`)**:
  - `OdEsfericoPerto => OdEsferico + (Adicao ?? 0)`
  - `OeEsfericoPerto => OeEsferico + (Adicao ?? 0)`
  - `OdCilindricoPerto`, `OeCilindricoPerto`, `OdEixoPerto`, `OeEixoPerto`

### 2.10 `OsFinanceiro` (Tabela: `os_financeiro`)
- `OsId`: `Guid` (PK e FK `ordens_servico`, Delete Cascade - **Relacionamento 1:1**)
- `ArmacaoId`: `Guid?` (FK `armacoes`, Restrict)
- `LentePrecoId`: `Guid?` (FK `lentes_tabela_precos`, Restrict)
- `ValorTotalBruto`: `decimal(10,2)`
- `DescontoReais`: `decimal(10,2)`
- `DescontoPercentual`: `decimal(5,2)`
- `ValorTotalLiquido`: `decimal(10,2)`
- `FormaPagamento`: `string` (max 50, Obrigatório - Valores: `DINHEIRO`, `PIX`, `CARTAO_CREDITO`, `CARTAO_DEBITO`, `BOLETO`)
- `Parcelas`: `int?` (Default `1`)
- `ValorArmacao`: `decimal(10,2)` (Obrigatório)
- `ValorLente`: `decimal(10,2)` (Obrigatório)
- `ValorEntrada`: `decimal(10,2)?`
- `ValorRestante`: `decimal(10,2)`
- `PagamentoConferido`: `bool` (Default `false`)
- `ConferidoPorId`: `Guid?` (FK `usuarios`, Restrict)
- `DataConferencia`: `DateTime?`
- `ValorRecebidoRetirada`: `decimal(10,2)?`
- `FormaPagamentoRetirada`: `string?` (max 50)
- `ParcelasRetirada`: `int?`
- `DataQuitacao`: `DateTime?`
- `QuitacaoRegistradaPorId`: `Guid?` (FK `usuarios`, Restrict)

### 2.11 `ParcelaPagamento` (Tabela: `parcelas_pagamento`)
- `Id`: `Guid` (PK)
- `OrdemServicoId`: `Guid` (FK `ordens_servico`, Delete Cascade)
- `NumeroParcela`: `int`
- `DescricaoParcela`: `string` (max 150, Obrigatório)
- `Valor`: `decimal(18,2)`
- `DataVencimento`: `DateTime`
- `DataPagamento`: `DateTime?`
- `Metodo`: Enum `MetodoPagamento` (Mapeado como `string`, max 50)
- `PixQrCodePayload`: `string?` (Payload Copia e Cola)
- `PixTxId`: `string?` (CorrelationID OpenPix)
- **Propriedade Computada**: `Paga => DataPagamento.HasValue`

### 2.12 `ConfiguracaoLoja` (Tabela: `configuracoes_loja`)
- `Id`: `Guid` (PK)
- `OticaId`: `Guid` (FK `oticas`, Restrict, **Índice Único: 1 Configuração por Ótica**)
- `NomeLoja`: `string` (max 100, Default `Matriz`)
- `Cnpj`: `string?` (max 20)
- `PixApiKey`: `string?` (max 500)
- **Propriedade Computada**: `PixAtivo => !string.IsNullOrWhiteSpace(PixApiKey)`

### 2.13 `ConfiguracaoComissao` (Tabela: `configuracao_comissao`)
- `Id`: `int` (PK)
- `PercentualComissao`: `decimal(5,2)`
- `BaseCalculo`: `string` (max 50, Default `VALOR_BRUTO_OS`)
- `MomentoGeracao`: `string` (max 50, Default `EMISSAO_OS`)
- `Ativo`: `bool` (Default `true`)
- `UpdatedAt`: `DateTime`
- `UpdatedById`: `Guid` (FK `usuarios`, Restrict)

### 2.14 `Comissao` (Tabela: `comissoes`)
- `Id`: `Guid` (PK)
- `OrdemServicoId`: `Guid` (FK `ordens_servico`, Restrict)
- `VendedorId`: `Guid` (FK `usuarios`, Restrict)
- `ValorBase`: `decimal(18,2)`
- `PercentualAplicado`: `decimal(5,2)`
- `ValorComissao`: `decimal(18,2)`
- `Status`: `string` (max 30, Valores: `PENDENTE`, `FECHADO`, `PAGO`, `CANCELADO`)
- `DataGeracao`: `DateTime`
- `DataPagamento`: `DateTime?`
- `PeriodoReferencia`: `string` (max 7, Formato `AAAA-MM`)
- `Observacoes`: `string?` (max 250)

### 2.15 `FechamentoComissao` (Tabela: `fechamentos_comissao`)
- `Id`: `Guid` (PK)
- `VendedorId`: `Guid` (FK `usuarios`, Restrict)
- `PeriodoReferencia`: `string` (max 7, Formato `AAAA-MM`)
- `TotalVendasBrutas`: `decimal(18,2)`
- `TotalComissao`: `decimal(18,2)`
- `QtdOs`: `int`
- `Status`: `string` (max 30, Valores: `ABERTO`, `FECHADO`, `PAGO`)
- `DataFechamento`: `DateTime?`
- `DataPagamento`: `DateTime?`
- `FechadoPorId`: `Guid?` (FK `usuarios`, Restrict)

---

## 3. Mapeamento Detalhado dos Enums

### 3.1 `PerfilUsuario` (`RETSYS.Domain.Enums.PerfilUsuario`)
- **Tipo de Armazenamento**: Inteiro (`int`).
- **Valores**:
  - `1`: `Admin` (Acesso irrestrito a configurações, comissões globais, relatórios e cancelamentos).
  - `2`: `Vendedor` (Acesso restrito a vendas próprias, balcão e consulta de extrato individual).

### 3.2 `MetodoPagamento` (`RETSYS.Domain.Enums.MetodoPagamento`)
- **Tipo de Armazenamento**: String (`string`).
- **Valores**:
  - `DINHEIRO`
  - `PIX`
  - `CARTAO_CREDITO`
  - `CARTAO_DEBITO`
  - `BOLETO`

