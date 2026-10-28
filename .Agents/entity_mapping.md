# Entity Mapping - RETSYS (Mapeamento de Entidades e Banco de Dados)

## 1. Convenções Globais de Mapeamento

- **ORM**: Entity Framework Core 10 operando em banco de dados PostgreSQL via driver `Npgsql`.
- **Nomenclatura de Tabelas**: Nomes em minúsculo, no plural, utilizando o padrão `snake_case` (ex: `ordens_servico`, `lentes_tabela_precos`, `receitas_cliente`, `vendas_balcao`).
- **Chaves Primárias (PK)**: Mapeadas prioritariamente como `Guid` (`uuid` no PostgreSQL) gerados via `Guid.NewGuid()`, com exceção de tabelas de configuração global com registro único (`ConfiguracaoComissao`, PK `int`).
- **Multi-Tenancy (Isolamento por Ótica)**: Todas as entidades principais contêm a FK `OticaId` referenciando a tabela `oticas`, acompanhada de índices únicos compostos por tenant quando aplicável.
- **Tipos e Precisão Numérica**:
  - Valores Monetários: `decimal(10,2)` ou `decimal(18,2)`.
  - Percentuais de Desconto e Comissão: `decimal(5,2)`.
  - Índice de Refração Óptica: `decimal(4,2)`.
  - Graus Ópticos (Dioptrias): `decimal(5,2)` (esférico/cilíndrico) e `decimal(4,2)` (adição).
  - Medidas de Centragem e Montagem: `decimal(4,1)` (DNP, altura, aro, DM, vertical, ponte, CO, haste).

---

## 2. Documentação Exaustiva das Entidades

### 2.1 `Otica` (Tabela: `oticas`)
- `Id`: `Guid` (PK)
- `Nome`: `string` (max 150, Obrigatório)
- `MatrizId`: `Guid?` (FK `oticas`, Restrict, Auto-relacionamento: aponta para a ótica Matriz se for filial)
- `CriadoEm`: `DateTime` (Default `UtcNow`)
- **Navegação**: 
  - `Matriz`: Referência para a `Otica` Matriz (se filial).
  - `Filiais`: Coleção `ICollection<Otica>` das filiais subordinadas.
  - `Usuarios`: Coleção de usuários vinculados à ótica.

### 2.2 `Usuario` (Tabela: `usuarios`)
- `Id`: `Guid` (PK)
- `OticaId`: `Guid` (FK `oticas`, Restrict)
- `Nome`: `string` (max 100, Obrigatório)
- `Email`: `string` (max 150, Obrigatório, Índice Único)
- `SenhaHash`: `string` (Obrigatório, BCrypt Enhanced)
- `FilialLoja`: `string` (max 100)
- `FotoUrl`: `string?` (Caminho da foto de perfil)
- `Perfil`: Enum `PerfilUsuario` (Mapeado como `int`: `1 = Admin`, `2 = Vendedor`, `3 = Sistema`, `4 = Dono`)
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
- `CPF`: `string?` (max 14, **Opcional**, Índice Único Composto: `(OticaId, CPF)`)
- `Telefone`: `string` (max 20, Obrigatório - WhatsApp preferencial)
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
- **Navegações**:
  - `OrdensServico`: Coleção `ICollection<OrdemServico>`
  - `Receitas`: Coleção `ICollection<ReceitaCliente>`
  - `VendasBalcao`: Coleção `ICollection<VendaBalcao>`
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
- **Atributos Especialistas de Ótica (RETSYS 2.0)**:
  - `Formato`: `string` (max 50, Default `Retangular` - Redondo, Quadrado, Aviador, Gatinho, Retangular, Oval, Hexagonal, Esportivo)
  - `Genero`: `string` (max 30, Default `Unissex` - Unissex, Masculino, Feminino, Infantil)
  - `Aro`: `decimal(4,1)?` (Largura da lente em mm, ex: 52 mm)
  - `Ponte`: `decimal(4,1)?` (Distância entre lentes em mm, ex: 18 mm)
  - `Haste`: `decimal(4,1)?` (Comprimento da haste em mm, ex: 140 mm)
- `Fornecedor`: `string` (max 100)
- `PrecoCusto`: `decimal(18,2)` (Visível apenas para Admin/Gerente)
- `PrecoVenda`: `decimal(18,2)`
- `QuantidadeEstoque`: `int` (Default `0`)
- `QuantidadeMinima`: `int`
- `LojaUnidade`: `string` (max 100, Default `Matriz`)
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
- `ReceitaClienteId`: `Guid?` (FK `receitas_cliente`, Restrict - Vínculo opcional com a ficha clínica histórica)
- `VendedorId`: `Guid?` (FK `usuarios`, Restrict)
- `DataEntrada`: `DateTime` (Default `UtcNow`)
- `DataPrevistaEntrega`: `DateTime`
- `DataEntregaReal`: `DateTime?`
- `Status`: `string` (max 50, Valores: `EM_ABERTO`, `CONFIRMADA`, `AGUARDANDO_LENTE`, `EM_MONTAGEM`, `PRONTA`, `ENTREGUE`, `CANCELADO`)
- **Esteira de Laboratório & Qualidade (RETSYS 2.0)**:
  - `EtapaLaboratorio`: `string` (max 50, Default `BALCAO` - Valores: `BALCAO`, `AGUARDANDO_INSUMOS`, `LABORATORIO_EXTERNO`, `EM_MONTAGEM`, `LENSOMETRIA_QUALIDADE`, `PRONTO_RETIRADA`, `ENTREGUE`)
  - `LaboratorioParceiro`: `string?` (max 100 - ex: Essilor, Hoya, Zeiss)
  - `NumeroPedidoLaboratorio`: `string?` (max 50 - Número de rastreio no laboratório externo)
  - `DataEnvioLaboratorio`: `DateTime?`
  - `DataRetornoLaboratorio`: `DateTime?`
  - `ConferidoLensometro`: `bool` (Default `false`)
  - `ConferidoLensometroEm`: `DateTime?`
  - `ConferidoLensometroPorId`: `Guid?` (FK `usuarios`, Restrict)
- `MedicoNome`: `string?` (max 100)
- `MedicoCrm`: `string?` (max 20)
- `MedicoTipo`: `string` (max 30, Default `NAO_ESPECIFICADO`)
- `Observacoes`: `text?`
- `CreatedAt`: `DateTime`
- **Controle de Pedido de Lentes**:
  - `LentePedida`: `bool` (Default `false`)
  - `DataPedidoLente`: `DateTime?`
  - `PedidoLentePorId`: `Guid?` (FK `usuarios`, Restrict)
  - `LenteChegou`: `bool` (Default `false`)
- **Campos para Suporte Retroativo/Histórico**:
  - `IsRetroativa`: `bool` (Default `false`)
  - `ArmacaoModeloManual`: `string?` (max 150)
  - `LenteDescricaoManual`: `string?` (max 200)
  - `Ativo`: `bool` (Default `true`)
- **Multi-Loja & Auditoria de Edição**:
  - `LojaVenda`: `string` (max 100, Default `Matriz`)
  - `DataAjustadaLog`: `string?` (max 255)
  - `DataEntradaOriginal`: `DateTime?`
  - `UltimaEdicaoLog`: `string?` (max 255)
  - `WhatsAppNotificadoCriacao`, `WhatsAppNotificadoPronto`: `bool`

### 2.9 `OsReceita` (Tabela: `os_receita`)
- `OsId`: `Guid` (PK e FK `ordens_servico`, Delete Cascade - **Relacionamento 1:1**)
- `OdEsferico`, `OdCilindrico`, `OeEsferico`, `OeCilindrico`: `decimal(5,2)`
- `OdEixo`, `OeEixo`: `int`
- `Adicao`: `decimal(4,2)?`
- `DnpOd`, `DnpOe`: `decimal(4,1)`
- `AlturaMontagemOd`, `AlturaMontagemOe`: `decimal(4,1)?`
- `Aro`, `Dm`, `Vert`, `Po`, `CoOd`, `CoOe`: `decimal(4,1)?`
- `ObsReceita`: `text?`
- **Propriedades Computadas**:
  - `OdEsfericoPerto => OdEsferico + (Adicao ?? 0)`
  - `OeEsfericoPerto => OeEsferico + (Adicao ?? 0)`

### 2.10 `OsFinanceiro` (Tabela: `os_financeiro`)
- `OsId`: `Guid` (PK e FK `ordens_servico`, Delete Cascade - **Relacionamento 1:1**)
- `ArmacaoId`: `Guid?` (FK `armacoes`, Restrict)
- `LentePrecoId`: `Guid?` (FK `lentes_tabela_precos`, Restrict)
- `ValorTotalBruto`: `decimal(10,2)`
- `DescontoReais`: `decimal(10,2)`
- `DescontoPercentual`: `decimal(5,2)`
- `ValorTotalLiquido`: `decimal(10,2)`
- `FormaPagamento`: `string` (max 50, Obrigatório)
- `Parcelas`: `int?` (Default `1`)
- `ValorArmacao`: `decimal(10,2)`
- `ValorLente`: `decimal(10,2)`
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
- **Crediário Próprio / Carnê (RETSYS 2.0)**:
  - `NumeroCarne`: `string?` (max 50)
  - `JurosAtraso`: `decimal(10,2)?` (Default `0.00`)
  - `MultaAtraso`: `decimal(10,2)?` (Default `0.00`)
  - `ValorPago`: `decimal(10,2)?`
  - `ObservacaoPagamento`: `string?` (max 250)
- `PixQrCodePayload`: `string?` (Payload Copia e Cola)
- `PixTxId`: `string?` (CorrelationID OpenPix)
- **Propriedades Computadas**:
  - `Paga => DataPagamento.HasValue`
  - `DiasAtraso => (!Paga && DateTime.UtcNow.Date > DataVencimento.Date) ? (DateTime.UtcNow.Date - DataVencimento.Date).Days : 0`

### 2.12 `ReceitaCliente` (Tabela: `receitas_cliente`) - NOVO RETSYS 2.0
- `Id`: `Guid` (PK)
- `OticaId`: `Guid` (FK `oticas`, Restrict)
- `ClienteId`: `Guid` (FK `clientes`, Delete Cascade)
- `DataExame`: `DateTime` (Default `UtcNow`)
- `DataValidade`: `DateTime?`
- `MedicoNome`: `string` (max 100, Obrigatório)
- `MedicoCrm`: `string?` (max 30)
- `MedicoTipo`: `string` (max 30, Default `OFTALMOLOGISTA`)
- `Clinica`: `string?` (max 100)
- **Grau de Longe**:
  - `OdEsferico`, `OdCilindrico`, `OeEsferico`, `OeCilindrico`: `decimal(5,2)`
  - `OdEixo`, `OeEixo`: `int`
  - `OdDnp`, `OeDnp`: `decimal(4,1)?`
  - `OdAltura`, `OeAltura`: `decimal(4,1)?`
  - `OdPrisma`, `OePrisma`: `decimal(4,2)?`
  - `OdBasePrisma`, `OeBasePrisma`: `string?` (max 20)
- **Grau de Perto & Medidas**:
  - `Adicao`: `decimal(4,2)?`
  - `Aro`, `Ponte`, `MaiorDiagonal`, `AlturaVertical`: `decimal(4,1)?`
- `Observacoes`: `text?`
- `FotoReceitaUrl`: `string?` (max 500)
- `Ativo`: `bool` (Default `true`)
- `CriadoEm`: `DateTime` (Default `UtcNow`)
- **Propriedades Computadas**:
  - `OdEsfericoPerto => OdEsferico + (Adicao ?? 0)`
  - `OeEsfericoPerto => OeEsferico + (Adicao ?? 0)`
  - `EstaVencida => DataValidade.HasValue ? DataValidade.Value < DateTime.UtcNow.Date : DataExame.AddYears(1) < DateTime.UtcNow.Date`
  - `PrecisaRetornoAnual => DataExame.AddDays(330) <= DateTime.UtcNow.Date`

### 2.13 `VendaBalcao` (Tabela: `vendas_balcao`) - NOVO RETSYS 2.0
- `Id`: `Guid` (PK)
- `OticaId`: `Guid` (FK `oticas`, Restrict)
- `NumeroVenda`: `string` (max 30, Obrigatório, ex: `VB-00001`)
- `ClienteId`: `Guid?` (FK `clientes`, Restrict - Opcional para Consumidor Final)
- `VendedorId`: `Guid?` (FK `usuarios`, Restrict)
- `DataVenda`: `DateTime` (Default `UtcNow`)
- `ValorTotalBruto`: `decimal(10,2)`
- `DescontoReais`: `decimal(10,2)`
- `ValorTotalLiquido`: `decimal(10,2)`
- `FormaPagamento`: `string` (max 50, Default `DINHEIRO`)
- `Parcelas`: `int?` (Default `1`)
- `Status`: `string` (max 30, Default `CONCLUIDA`)
- `Observacoes`: `string?` (max 500)
- `LojaUnidade`: `string` (max 100, Default `Matriz`)
- `CriadoEm`: `DateTime`
- **Navegação**: `Itens` (1:N `ItemVendaBalcao`, Delete Cascade)

### 2.14 `ItemVendaBalcao` (Tabela: `itens_venda_balcao`) - NOVO RETSYS 2.0
- `Id`: `Guid` (PK)
- `VendaBalcaoId`: `Guid` (FK `vendas_balcao`, Delete Cascade)
- `TipoItem`: `string` (max 50, Default `ARMACAO` - ARMACAO, LENTE_CONTATO, ACESSORIO, LIMPEZA, OUTRO)
- `ArmacaoId`: `Guid?` (FK `armacoes`, Restrict - Opcional se for acessório livre)
- `Descricao`: `string` (max 200, Obrigatório)
- `Quantidade`: `int` (Default `1`)
- `PrecoUnitario`: `decimal(10,2)`
- **Propriedade Computada**: `Subtotal => Quantidade * PrecoUnitario`

### 2.15 `ConfiguracaoLoja` (Tabela: `configuracoes_loja`)
- `Id`: `Guid` (PK)
- `OticaId`: `Guid` (FK `oticas`, Restrict)
- `NomeLoja`: `string` (max 100, Default `Matriz`)
- `Cnpj`: `string?` (max 20)
- `PixApiKey`: `string?` (max 500)
- `WhatsappNumero`: `string?` (max 30)

### 2.16 `OsAuditoriaLog` (Tabela: `os_auditoria_logs`)
- `Id`: `Guid` (PK)
- `OticaId`: `Guid` (FK `oticas`, Restrict)
- `OrdemServicoId`: `Guid` (FK `ordens_servico`, Delete Cascade)
- `UsuarioId`: `Guid` (FK `usuarios`, Restrict)
- `DataAlteracao`: `DateTime`
- `CampoAlterado`: `string` (max 100)
- `ValorAntigo`, `ValorNovo`: `string?` (max 500)
- `Descricao`: `string` (max 500)

---

## 3. Mapeamento Detalhado dos Enums

### 3.1 `PerfilUsuario` (`RETSYS.Domain.Enums.PerfilUsuario`)
- `1`: `Admin` (Acesso irrestrito a configurações, comissões globais, edição de OS, relatórios e cancelamentos).
- `2`: `Vendedor` (Acesso a vendas próprias, balcão e consulta de extrato individual).
- `3`: `Sistema` ("Deus"/homologação: acesso irrestrito e chaveamento dinâmico entre quaisquer óticas).
- `4`: `Dono` (Gestão multi-loja de toda a rede: visualização consolidada e alternância entre Matriz e Filiais).

### 3.2 `MetodoPagamento` (`RETSYS.Domain.Enums.MetodoPagamento`)
- `DINHEIRO`
- `PIX`
- `CARTAO_CREDITO`
- `CARTAO_DEBITO`
- `BOLETO`
- `CARNE_LOJA` (Crediário Próprio da Ótica)
