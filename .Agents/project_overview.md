# Project Overview - RETSYS (Sistema de Gestão de Ótica Especialista)

## 1. Visão Geral do Sistema
O **RETSYS** é um sistema de gestão especializado para óticas (ERP/CRM Óptico), construído para operacionalizar desde o atendimento no balcão e receita médica até a montagem em laboratório, controle de estoque de armações/lentes, comissionamento de vendedoras, fluxo de caixa com cobrança PIX automática e leitura inteligente de prescrições via IA local (Ollama).

Na versão **RETSYS 2.0 (Arquitetura Especialista)**, o sistema atinge paridade e supera sistemas de referência do mercado brasileiro (como SSÓtica e PWI):
- **CRM Óptico & Recall Anual**: Retorno de consulta após 1 ano, felicitação de aniversariantes e pós-venda multifocal com WhatsApp em 1 clique.
- **Ficha Clínica do Paciente**: Histórico independente de prescrições (`ReceitaCliente`) com comparativo temporal de evolução diópica.
- **PDV Ágil de Balcão**: Venda rápida de óculos solares, lentes de contato e acessórios com baixa no estoque e emissão de cupom térmico (80mm).
- **Crediário Próprio & Carnê**: Emissão de carnês personalizados (canhoto da loja + via do cliente), controle de parcelas em atraso com juros/multa.
- **Esteira de Laboratório & Lensometria**: Fluxo visual do ciclo da lente (Balcão ➔ Lab Externo como Essilor/Hoya ➔ Montagem ➔ Lensometria & Controle de Qualidade ➔ Liberação).
- **Catálogo Óptico Enriquecido**: Formato (Aviador, Gatinho, Redondo, etc.), Gênero, Aro, Ponte e Haste nas armações; Tratamentos e Índices de Refração nas lentes.

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
   - **Componentes**: Entidades com regras de domínio incorporadas (`Cliente`, `ReceitaCliente`, `VendaBalcao`, `ItemVendaBalcao`, `OrdemServico`, `OsReceita`, `OsFinanceiro`, `ParcelaPagamento`, `Armacao`, `Lente`, `Usuario`, etc.), Enums (`PerfilUsuario`, `MetodoPagamento`) e Contratos de Serviços (`IServicoIa`, `IServicoPix`, `IServicoCriptografia`).

2. **RETSYS.Infrastructure**:
   - **Responsabilidade**: Persistência, mapeamento de banco e integrações externas.
   - **Componentes**: `ApplicationDbContext` (EF Core com Npgsql e Migrations), `DatabaseSeeder`, `ServicoOllama` (OCR via modelo vision `moondream`), `ServicoPix` (Gateway OpenPix), `ServicoCriptografia` (BCrypt Enhanced).

3. **RETSYS.Web**:
   - **Responsabilidade**: Apresentação, controllers, autenticação por cookies e roteamento com Vue 3.
   - **Controllers**: `TenantController` (classe base com multi-tenancy), `CrmController`, `ReceitasController`, `VendasBalcaoController`, `CarneController`, `LaboratorioController`, `OrdensServicoController`, `CaixaController`, `ComissoesController`, `ArmacoesController`, `LentesController`.
   - **Frontend**: SPA Vue 3 montada via Vite v8 com TailwindCSS e componentes modulares.

---

## 3. Segurança e Infraestrutura na VPS (UFW & Docker)

### 3.1 Isolamento de Rede no Docker
No arquivo `docker-compose.yml`, todos os serviços de dados e aplicação estão amarrados estritamente à interface loopback (`127.0.0.1`):
- `retsys_db` (PostgreSQL): `127.0.0.1:5432:5432`
- `ollama` (Motor de OCR IA): `127.0.0.1:11434:11434`
- `retsys_app` (Aplicação Web C#): `127.0.0.1:5000:8080` e `127.0.0.1:5001:8081`

### 3.2 Conformidade com o Firewall UFW
Como a aplicação é servida através do Nginx local como Proxy Reverso com SSL Let's Encrypt (`wo-retsys.tech`), o firewall da VPS necessita ter liberadas **exclusivamente**:
- **Porta 22/tcp (SSH)**: Acesso administrativo seguro.
- **Porta 80/tcp (HTTP)**: Redirecionamento automático para HTTPS.
- **Porta 443/tcp (HTTPS)**: Tráfego seguro web criptografado via TLS 1.2/1.3.

Nenhuma porta de banco de dados ou da aplicação precisa estar exposta na internet pública, blindando a VPS contra invasões ou tentativas de acesso indevido.
