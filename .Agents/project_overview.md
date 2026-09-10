# Project Overview - RETSYS (Sistema de Gestão de Ótica)

## 1. Visão Geral do Sistema
O **RETSYS** é um sistema de gestão especializado para óticas (ERP/CRM Óptico), construído para operacionalizar desde o atendimento no balcão e receita médica até a montagem em laboratório, controle de estoque de armações/lentes, comissionamento de vendedoras, fluxo de caixa com cobrança PIX automática e leitura inteligente de prescrições via IA local (Ollama).

---

## 2. Arquitetura de Software

O sistema adota uma arquitetura em camadas baseada nos princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**, com o ecossistema frontend operando no modelo de **Monólito Moderno com SPA Integrada** através do **Inertia.js**.

```
                           [ Navegador do Cliente ]
                                      │
                                      ▼
                      [ RETSYS.Web (ASP.NET Core MVC) ]
                               │             │
                    ┌──────────┘             └──────────┐
                    ▼                                   ▼
        [ Inertia.js / Vue 3 ]                 [ Controllers (TenantController) ]
            (Frontend SPA)                              │
                                                        ▼
                                          [ RETSYS.Domain (Entities/Interfaces) ]
                                                        ▲
                                                        │
                                       [ RETSYS.Infrastructure (EF Core) ]
                                          │            │             │
                                          ▼            ▼             ▼
                                     [PostgreSQL]   [Ollama]    [OpenPix API]
```

### 2.1 Mapeamento de Camadas

1. **RETSYS.Domain**:
   - **Responsabilidade**: Core de negócios agnóstico de frameworks externos.
   - **Componentes**: Entidades anêmicas com regras de domínio incorporadas (`Cliente`, `OrdemServico`, `OsReceita`, `Usuario`, etc.), Enums (`PerfilUsuario`, `MetodoPagamento`) e Contratos de Serviços (`IServicoIa`, `IServicoPix`, `IServicoCriptografia`).
   - **Dependências**: Nenhuma dependência externa ou infraestrutural.

2. **RETSYS.Infrastructure**:
   - **Responsabilidade**: Implementação técnica, persistência e integração externa.
   - **Componentes**: `ApplicationDbContext` (EF Core com Npgsql), `DatabaseSeeder`, `ServicoOllama` (OCR via modelo vision `moondream`), `ServicoPix` (Gateway OpenPix), `ServicoCriptografia` (BCrypt Enhanced).
   - **Dependências**: `RETSYS.Domain`, EntityFrameworkCore, Npgsql, BCrypt.Net.

3. **RETSYS.Web**:
   - **Responsabilidade**: Apresentação, roteamento, controle de sessão, middlewares de autenticação e adaptação com o frontend.
   - **Componentes**: Controllers derivados de `TenantController`, Inertia Adapter, Middlewares, Views Razor (`App.cshtml`), Páginas Vue 3 (`Frontend/Pages/*`).
   - **Dependências**: `RETSYS.Domain`, `RETSYS.Infrastructure`, InertiaCore, Vite.

---

## 3. Aplicação dos Princípios SOLID & DDD

### 3.1 Single Responsibility Principle (SRP)
- **Domain Interfaces**: A interface `IServicoIa` cuida exclusivamente da extração OCR de receitas; `IServicoPix` lida unicamente com a geração do payload de cobrança.
- **Controllers Delimitados**: Cada Controller lida estritamente com sua área funcional (`CaixaController`, `ComissoesController`, `OrdensServicoController`, `LaboratorioController`).

### 3.2 Open/Closed Principle (OCP) & Liskov Substitution Principle (LSP)
- **Abstração de Serviços**: Componentes de infraestrutura como `ServicoOllama` e `ServicoPix` implementam interfaces de domínio. Caso o fornecedor de PIX ou o modelo de IA seja alterado, a camada de domínio permanece intocada.

### 3.3 Interface Segregation Principle (ISP)
- Contratos enxutos e focados (`IServicoCriptografia` possui apenas `CriptografarSenha` e `VerificarSenha`).

### 3.4 Dependency Inversion Principle (DIP)
- A camada Web e a camada Infrastructure dependem das abstrações declaradas em `RETSYS.Domain.Interfaces`.
- Injeção de dependência registrada centralizadamente no `Program.cs`.

### 3.5 DDD (Domain-Driven Design) & Isolamento Multi-Tenant
- **Agregados e Entidades**: `OrdemServico` atua como a raiz de agregado para `OsReceita`, `OsFinanceiro` e `ParcelaPagamento`.
- **Propriedades Computadas no Domínio**: Cálculo automático de grau de perto (`OdEsfericoPerto = OdEsferico + (Adicao ?? 0)`), verificação de aniversário de cliente (`FazAniversarioEm`), status de quitação de parcelas (`Paga => DataPagamento.HasValue`).
- **Bounded Context Multi-Tenant**: Toda entidade de negócio principal (`Usuario`, `Cliente`, `Marca`, `Armacao`, `Lente`, `OrdemServico`, `ConfiguracaoLoja`) possui o vínculo obrigatório com `OticaId`. O `TenantController` extrai o `OticaId` do token de autenticação e filtra as consultas por tenant.

---

## 4. Padrões de Integração

1. **Inertia.js (SPA Monolítica)**:
   - Elimina a necessidade de APIs REST tradicionais e serializadores redundantes.
   - O backend ASP.NET Core MVC entrega dados diretamente às views Vue 3 através de `Inertia.Render("Caminho/Da/Pagina", props)`.
   - Estado de autenticação global compartilhado via middleware `Inertia.Share("auth", ...)`.

2. **IA Integrada (Ollama OCR)**:
   - Processamento local via requisição HTTP para o serviço Ollama executando o modelo `moondream`.
   - Leitura direta do `Stream` da foto da receita enviada pelo balcão, retornando DTO estruturado (`ResultadoLeituraReceitaDto`).

3. **Pagamento Automático PIX (OpenPix)**:
   - Comunicação assíncrona com a API OpenPix via `HttpClient` dedicado.
   - Geração dinâmica de QR Code e chave Copia-e-Cola com conversão automática do valor para centavos.

4. **Segurança de Autenticação**:
   - Criptografia de senhas com BCrypt (Enhanced Hash com fator de custo 12).
   - Autenticação baseada em Cookies do ASP.NET Core com expiração de 8 horas e suporte a claims de perfil (`Admin`, `Gerente`, `Vendedor`) e `OticaId`.

