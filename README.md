# RETSYS Web v2 — Sistema de Gestão Inteligente para Óticas e Laboratórios Óticos

Bem-vindo ao repositório oficial do **RETSYS Web v2**, um ecossistema de software empresarial de alta performance desenvolvido especificamente para o ecossistema de óticas modernas e laboratórios de surfaçagem/montagem de lentes. O sistema foi projeto sob os mais rígidos padrões de engenharia de software para sanar os gargalos operacionais e financeiros do balcão de atendimento, integrando inteligência artificial local e gateways de pagamento dinâmicos.

---

## 🚀 1. Diferenciais Tecnológicos & Visão do Produto

Ao contrário de sistemas tradicionais de mercado que operam de forma isolada e reativa, o RETSYS Web v2 atua como o centro de inteligência do estabelecimento através de quatro pilares de inovação computacional:

### A. Automação de Cálculo de Refração
O sistema elimina o risco de erro humano na interpretação de receitas. O backend em C# possui um motor de regras óticas que calcula de forma totalmente autônoma o grau esférico de perto do paciente. O cálculo utiliza a fórmula técnica padrão cruzando as dioptrias de longe com o fator de Adição para presbiopia:
- $EsfericoPerto = EsfericoLonge + Adicao$

### B. Assistente OCR por Inteligência Artificial Local (Ollama + Moondream)
Um dos maiores diferenciais competitivos do MVP é o scanner inteligente de receitas. Utilizando o **Ollama** com o modelo de visão multimodal **Moondream (1.4B parâmetros)** encapsulado localmente, o operador faz o upload da foto da receita (computadorizada ou manuscrita) e o sistema extrai automaticamente os eixos, cilíndricos e esféricos diretamente para os campos do formulário Vue 3. Isso garante:
- **Custo zero de infraestrutura:** Sem taxas abusivas por requisição em APIs como OpenAI ou Google Vision.
- **Soberania e Privacidade:** Os dados de refração dos clientes nunca deixam o servidor local da empresa.

### C. Terminal de Checkout Duplo Interativo (OpenPix & Contingência)
O caixa possui uma interface de checkout integrada ao ecossistema **OpenPix**. Quando ativado por chave de API, o sistema gera instantaneamente um QR Code dinâmico com o código EMV "Copia e Cola" associado ao valor exato da parcela do crediário. 
O frontend em Vue 3 inicia um monitoramento duplo em tempo real:
- Escuta ativa via **Webhooks** no backend.
- Mecanismo de **Polling reativo** (consultas automatizadas a cada 3 segundos) como fallback de segurança.
- **Modo Contingência:** Se a filial operar sem internet ou sem token de API, o terminal chaveia automaticamente para o formato de baixa manual tradicional (Dinheiro/Cartão físico).

### D. Prontuário Ótico Temporal (Linha do Tempo)
Cada cliente possui uma ficha histórica exclusiva. Quando um paciente retorna à ótica, o vendedor consegue examinar graficamente a evolução das dioptrias (graus) ao longo dos anos, permitindo uma venda técnica e consultiva baseada no histórico de receitas do cliente.

---

## 🛠️ 2. Arquitetura do Sistema & Stack Tecnológica

O RETSYS Web v2 adota o padrão de desenvolvimento em camadas (Clean Architecture / DDD adaptado) para o backend, casado com uma interface SPA (Single Page Application) fluida no frontend mediada pelo **Inertia.js**.

### Backend
- **.NET 10 (ASP.NET Core / Web API):** Engine principal compilada nativamente para máxima performance e baixo consumo de memória.
- **Entity Framework Core 10:** Camada de persistência e ORM com mapeamento de coleções complexas e consultas LINQ otimizadas.
- **PostgreSQL 16:** Banco de dados relacional robusto com suporte nativo a índices complexos e integridade referencial estrita.
- **InertiaCore (.NET Core Adapter):** Elimina a necessidade de criar APIs REST públicas complexas e autenticação por tokens JWT expostos no cliente. O C# renderiza e despacha os dados de estado diretamente para os componentes Vue.

### Frontend
- **Vue 3 (Composition API / Script Setup):** Interface moderna, baseada em reatividade atômica e computada.
- **Tailwind CSS:** Identidade visual corporativa minimalista com paleta estrita baseada em tons de **Slate** (grafite), destaques em **Teal** (verde-água corporativo) e contêineres anatômicos em bordas arredondadas (`rounded-2xl` e `rounded-3xl`).
- **Inertia.js Vue3 Adapter:** Roteamento instantâneo do lado do cliente sem recarregar a página, mantendo o estado global do SPA persistentente.

---

## 📂 3. Estrutura de Diretórios do Projeto

```text
RETSYS-New/
│
├── RETSYS.Domain/                    # Camada Core: Entidades de Domínio, Enums e DTOs
│   ├── Entities/
│   │   ├── Armacao.cs                # Dados do estoque físico de óculos
│   │   ├── Cliente.cs                # Cadastro de pacientes e vínculo de contato (Celular)
│   │   ├── ConfiguracaoLoja.cs       # Parâmetros de filial e tokens de API
│   │   ├── Marca.cs                  # Marcas e fabricantes do portfólio
│   │   ├── OrdemServico.cs           # Ficha técnica de refração, status e motor de perto
│   │   ├── ParcelaPagamento.cs       # Parcelamento automatizado e controle de baixas
│   │   └── Usuario.cs                # Colaboradores, perfis e credenciais criptografadas
│   ├── Enums/
│   │   ├── MetodoPagamento.cs
│   │   └── PerfilUsuario.cs          # Roles: Admin, Gerente, Vendedor, Técnico de Laboratório
│   └── Dto/
│       └── DtoOcrReceita.cs          # Schema estrito de resposta para o modelo de IA
│
├── RETSYS.Infrastructure/            # Camada de Dados, Infraestrutura e Serviços Externos
│   ├── Data/
│   │   ├── ApplicationDbContext.cs   # Contexto do EF Core 10 mapeando tabelas relacionais
│   │   └── DatabaseSeeder.cs         # Carga inicial de dados homologados para o balcão
│   ├── Security/
│   │   └── ServicoCriptografia.cs    # Criptografia BCrypt para senhas de funcionários
│   └── Services/
│       └── ServicoPix.cs             # Engine de comunicação com gateway OpenPix
│
└── RETSYS.Web/                       # Camada de Apresentação e Ponto de Entrada da Aplicação
    ├── Program.cs                    # Configuração de Services, Injeção de Dependências e Middlewares
    ├── Controllers/                  # Controladores ASP.NET Core que despacham dados via Inertia
    │   ├── AutenticacaoController.cs # Login de estado persistente por Cookies corporativos
    │   ├── DashboardController.cs    # Agrupamentos LINQ de faturamento, metas e ranking
    │   ├── ClientesController.cs     # Mecânica de busca com debounce e prontuário ótico
    │   ├── OrdensServicoController.cs # Emissão de OS, parcelamento automático e endpoint de IA
    │   ├── LaboratorioController.cs  # Fila atômica de montagem de lentes (Painel do Técnico)
    │   └── CaixaController.cs        # Recebimentos manuais e escutas dinâmicas do checkout
    │
    └── Frontend/                     # Interface SPA (Vue 3 / Tailwind)
        ├── Shared/
        │   └── MainLayout.vue        # Sidebar unificada, navegação reativa e controle de claims
        └── Pages/                    # Módulos operacionais do produto
            ├── Auth/Login.vue        # Login imersivo com controle de loading e shake-animation
            ├── Dashboard/Index.vue   # KPI Cards, desempenho de vendedores e faturamento de filiais
            ├── Clientes/             # Index.vue (Busca por debounce) e Historico.vue (Timeline de receitas)
            ├── Orders/               # Index.vue (Prancheta de OS) e Create.vue (Abertura + Motor IA)
            ├── Laboratory/Index.vue  # Interface do laboratório ótico (Cards de oficina)
            ├── Caixa/Index.vue       # Painel financeiro, monitoramento PIX e checkout de balcão
            └── Configuracoes/Index.vue # Ajustes cadastrais e colagem de tokens da OpenPix
```

## 💻 4. Guia Completo de Instalação, Configuração e Execução
Como o RETSYS Web v2 opera com motores de inteligência artificial locais e banco de dados relacional de produção, siga atentamente o passo a passo abaixo para erguer o ecossistema localmente na sua máquina de desenvolvimento.

Passo 1: Clonar o Repositório e Restaurar Dependências C#
Abra o seu terminal (PowerShell ou Bash) na raiz do projeto e execute os comandos para restaurar os pacotes do ecossistema .NET e compilar as soluções das três camadas:

PowerShell
cd C:\DEV\RETSYS-New
dotnet restore
dotnet build
Passo 2: Instalar e Configurar as Dependências JavaScript do Frontend
Acesse a pasta do projeto web, instale os pacotes npm do Vue 3, Inertia e Tailwind CSS, e inicie o compilador de assets estáticos em segundo plano:

PowerShell
cd RETSYS.Web
npm install
npm run dev
Passo 3: Configurar o Motor de Inteligência Artificial Local (Ollama)
O preenchimento automático de receitas ópticas depende do Ollama. Certifique-se de que o instalador do Ollama está rodando na sua máquina. Abra uma nova janela do terminal e puxe o modelo multimodal Moondream (o download possui cerca de 820 MB e será salvo automaticamente de forma permanente):

PowerShell
ollama run moondream
Para testar se o motor de visão está respondendo localmente na porta padrão do projeto, você pode acessar http://localhost:11434 no seu navegador.

Passo 4: Rodar o Projeto
Com o banco de dados PostgreSQL ativo (conforme as credenciais configuradas na sua connection string no appsettings.json) e o Ollama rodando no background, retorne para a pasta RETSYS.Web no terminal principal e execute o comando de inicialização do ASP.NET Core:

PowerShell
dotnet run
O sistema estará operacional e disponível nos endereços locais fornecidos pelo console (geralmente http://localhost:5000 ou https://localhost:5001).

## 🛡️ 5. Governança e Termos de Uso do Módulo de Inteligência Artificial
O módulo de OCR automatizado foi projetado como uma ferramenta aceleradora de produtividade de balcão. No entanto, considerando as variações extremas e subjetivas em receitas ópticas digitadas ou manuscritas por oftalmologistas (letras ilegíveis, rasuras ou desalinhamentos de eixos), fica estabelecida a seguinte política de governança de dados:

Ação Consultiva: A leitura digital efetuada pelo modelo Moondream possui caráter puramente consultivo e sugestivo.

Responsabilidade do Operador: O vendedor, gerente ou técnico responsável pela emissão do documento tem a obrigação legal e técnica de conferir visualmente cada campo dióptrico (Esférico Longe/Perto, Cilíndrico e Eixo) preenchido na tela antes de clicar no botão "Emitir Ordem de Serviço".

Bloqueio Reativo: O botão de execução da leitura digital em Orders/Create.vue permanece bloqueado por padrão, exigindo que o funcionário marque explicitamente o checkbox de aceite do termo de responsabilidade técnica a cada novo atendimento.

## 📝 6. Licença e Direitos Autorais
Propriedade intelectual e código-fonte registrados sob as especificações de MVP confidencial do ecossistema RETSYS. Todos os direitos reservados. É proibida a redistribuição comercial ou cópia não autorizada da arquitetura em camadas aqui implementada. Developed with 🖤 using .NET 10 & Vue 3.
