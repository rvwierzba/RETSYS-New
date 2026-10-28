# 👓 RETSYS 2.0 - MANUAL OPERACIONAL DA ÓTICA
### Guia Prático Passo a Passo para Atendentes, Gerentes e Proprietários

---

## 📖 Apresentação

Bem-vindo ao **RETSYS 2.0**, o sistema especialista desenvolvido sob medida para a rotina diária da sua ótica.

Este manual foi elaborado com uma linguagem simples, direta e prática para que qualquer membro da sua equipe — desde a vendedora novata até o gerente e o proprietário — domine com facilidade todas as ferramentas do sistema: desde o atendimento no balcão e conferência de receitas oftalmológicas até a emissão de carnês, esteira de laboratório e ações de pós-venda no WhatsApp.

---

## 📑 Índice Rápido
1. [Acesso ao Sistema & Navegação](#1-acesso-ao-sistema--navegação)
2. [Atendimento no Balcão & Emissão de O.S.](#2-atendimento-no-balcão--emissão-de-os)
3. [Venda Rápida de Balcão (PDV Ágil)](#3-venda-rápida-de-balcão-pdv-ágil)
4. [Central de CRM & Recall Óptico (WhatsApp)](#4-central-de-crm--recall-óptico-whatsapp)
5. [Prontuário & Ficha Clínica de Graus](#5-prontuário--ficha-clínica-de-graus)
6. [Crediário Próprio & Impressão de Carnê](#6-crediário-próprio--impressão-de-carnê)
7. [Esteira de Laboratório & Lensometria](#7-esteira-de-laboratório--lensometria)
8. [Caixa, Recebimentos & PIX](#8-caixa-recebimentos--pix)
9. [Comissões da Equipe & Metas](#9-comissões-da-equipe--metas)
10. [Guia de Segurança & Firewall da VPS](#10-guia-de-segurança--firewall-da-vps)

---

## 1. Acesso ao Sistema & Navegação

### Como Entrar no Sistema
1. Abra o navegador (Google Chrome, Edge ou Firefox) no computador ou tablet da ótica.
2. Acesse o endereço da sua ótica: `https://wo-retsys.tech` (ou o link fornecido pelo seu administrador).
3. Digite seu **E-mail** e **Senha** cadastrados.
4. Clique em **Entrar no Sistema**.

### O Menu Lateral (Barra de Navegação)
No lado esquerdo da tela, você encontra as seções organizadas por cores e funções:
- 📊 **Dashboard**: Resumo geral de vendas, faturamento do dia e alertas.
- 👥 **Clientes & CRM Óptico**:
  - **CRM & Recall (1 Ano)**: Clientes com receitas a vencer para agendamento.
  - **Clientes & Prontuário**: Cadastro de clientes e histórico óptico.
  - **Aniversariantes**: Lista de aniversariantes do mês.
- 📋 **Ordens de Serviço**:
  - **Ordens de Serviço**: Listagem de todas as OS com busca rápida e status.
  - **Nova OS**: Tela completa de faturamento de óculos com receita.
  - **Kanban OS**: Painel visual com cards para acompanhar o andamento dos óculos.
  - **Laboratório & Montagem**: Fila de trabalho dos técnicos e conferência no lensômetro.
- 💰 **Vendas & PDV Balcão**:
  - **PDV Rápido Balcão**: Venda direta de óculos solares, lentes de contato e acessórios.
  - **Vendas Diretas**: Histórico de cupons emitidos no balcão.
  - **Fluxo de Caixa**: Entradas, parcelas e baixas financeiras.
  - **Minhas Comissões**: Extrato de ganhos da vendedora no mês.
- 👓 **Estoque & Catálogo**:
  - **Armações & Estoque**: Consulta de modelos, cores, formatos e quantidades.
  - **Marcas**: Marcas cadastradas.
  - **Tabela de Lentes**: Lentes monofocais, multifocais e tratamentos.

---

## 2. Atendimento no Balcão & Emissão de O.S.

A Ordem de Serviço (OS) é o coração da venda de óculos de grau na ótica.

### Passo a Passo para Emitir uma Nova OS:
1. No menu lateral, clique em **Nova OS** (`/ordens/nova`).
2. **Identificação do Atendente**: O seu nome já virá preenchido automaticamente como vendedora responsável.
3. **Seleção do Cliente**:
   - Digite o nome ou CPF do cliente.
   - Se ele já for cadastrado, clique sobre o nome dele.
   - Se for cliente novo, clique no botão **+ Cadastrar Novo Cliente** e preencha Nome e Telefone/WhatsApp (o CPF é opcional caso o cliente não tenha no momento).
4. **Preenchimento da Receita Médica (Sequência Clínica)**:
   - Digite o nome do Médico Oftalmologista ou Optometrista.
   - **1º Grau de Longe**:
     - *Olho Direito (OD)*: Esférico, Cilíndrico e Eixo.
     - *Olho Esquerdo (OE)*: Esférico, Cilíndrico e Eixo.
   - **2º Grau de Perto**:
     - Preencha o esférico de perto.
   - **3º Adição (AD)**:
     - O sistema calcula a Adição automaticamente ($Adição = Perto - Longe$) assim que você preenche os campos! Caso queira ajustar manualmente, basta alterar o valor.
   - **4º Medidas Técnicas da Montagem**:
     - Preencha o DNP (Distância Naso-Pupilar) de cada olho e a Altura de montagem medida com a régua ou pupilômetro.
5. **Seleção dos Produtos**:
   - Escolha a **Armação** do estoque (pelo código ou referência) ou selecione *"Armação Própria do Cliente"*.
   - Escolha a **Lente** da tabela (ex: Monofocal 1.56 Antirreflexo, Multifocal Varilux, etc.).
6. **Condições de Pagamento**:
   - Defina a forma de pagamento (Dinheiro, PIX, Cartão de Débito, Cartão de Crédito ou Carnê Próprio).
   - Informe se houve valor de entrada e em quantas parcelas o saldo restante será dividido.
7. **Finalizar**:
   - Clique em **Emitir e Gravar Ordem de Serviço**.
   - O sistema gera o número da OS e já disponibiliza os botões para **Imprimir OS**, **Imprimir Carnê** e **Avisar no WhatsApp**.

---

## 3. Venda Rápida de Balcão (PDV Ágil)

Para vendas de balcão que **não exigem montagem laboratorial** (como óculos de sol, caixas de lentes de contato, estojos, flanelas, cordões ou sprays limpa-lentes), utilize o **PDV Rápido Balcão**.

### Como Fazer uma Venda Rápida:
1. No menu, clique em **PDV Rápido Balcão** (`/vendas/pdv`).
2. Localize os produtos no catálogo:
   - Você pode pesquisar pelo nome ou referência da armação solar.
   - Ou adicionar itens avulsos (como *"Limpa Lentes 60ml"*, *"Estojo Rígido"*, etc.).
3. Clique no botão **Adicionar** para colocar o item na comanda do lado direito.
4. Ajuste a quantidade desejada.
5. Selecione o Cliente (ou mantenha como *"Consumidor Final"*).
6. Escolha a Forma de Pagamento e informe desconto caso tenha sido concedido.
7. Clique em **Finalizar Venda de Balcão**.
8. Pronto! O sistema baixa o estoque automaticamente e abre a tela de **Comprovante Térmico (80mm)** com layout padrão de impressora de cupom.

---

## 4. Central de CRM & Recall Óptico (WhatsApp)

Estudos do setor óptico comprovam que **mais de 60% dos clientes esquecem de refazer o exame de vista anual**. A Central de CRM do RETSYS faz essa busca ativa para você.

### Como Usar o CRM & Recall:
1. No menu, clique em **CRM & Recall (1 Ano)** (`/crm`).
2. A tela exibe 3 abas essenciais:

#### Aba 1: Recall de Consulta (1 Ano)
- Mostra todos os clientes cuja última receita ou compra foi feita entre 11 e 15 meses atrás (330 a 480 dias).
- Exibe o grau que ele usava e o médico oftalmologista que o atendeu.
- Clique no botão verde **💬 Enviar WhatsApp**:
  - O sistema abre automaticamente o WhatsApp Web ou o app do celular com uma mensagem pronta e calorosa:
    > *"Olá, Maria! Tudo bem? Passando para lembrar que já faz cerca de 1 ano desde sua última consulta de vista com Dr. Silva. Cuidar da sua saúde visual é muito importante! Que tal agendarmos uma nova avaliação?"*

#### Aba 2: Aniversariantes do Mês
- Lista todos os clientes que fazem aniversário no mês atual.
- Quem faz aniversário no dia de hoje ganha um destaque especial.
- Envie uma mensagem de felicitações e ofereça um voucher de desconto para atrair o aniversariante à loja.

#### Aba 3: Pós-Venda Multifocal
- Lista os clientes que retiraram óculos entre 7 e 45 dias atrás.
- Envie uma mensagem perguntando como está a adaptação com as novas lentes e ofereça uma revisão gratuita nas plaquetas e hastes.

---

## 5. Prontuário & Ficha Clínica de Graus

Toda ótica de sucesso mantém o histórico de visão de seus pacientes arquivado para consultas futuras.

### Como Acessar a Ficha Clínica de um Cliente:
1. Vá em **Clientes & Prontuário** (`/clientes`).
2. Localize o cliente pelo nome, CPF ou telefone.
3. Clique no botão **🩺 Receitas** ao lado do cliente.
4. Você verá a **Ficha Clínica Completa**:
   - Histórico comparativo de evolução dos graus ao longo dos anos.
   - Quadro comparativo de Longe (OD/OE: Esférico, Cilíndrico, Eixo) e Perto (Adição).
   - Medidas técnicas de montagem (DNP, Altura, Aro, Ponte).
   - Nome do médico, clínica e data de validade da receita.
   - Foto ou digitalização da receita original (caso tenha sido anexada).
5. Para adicionar uma nova receita trazida pelo cliente sem necessariamente emitir uma OS agora, basta clicar em **+ Adicionar Nova Receita Clínica**.

---

## 6. Crediário Próprio & Impressão de Carnê

O carnê da própria ótica é a ferramenta mais tradicional e lucrativa para fidelizar famílias inteiras.

### Como Imprimir o Carnê de uma Venda:
1. Na listagem de **Ordens de Serviço** (`/ordens`), localize a OS desejada.
2. Clique no ícone de carnê **📜 Carnê** na coluna de ações (ou abra a OS e clique em **📜 Imprimir Carnê**).
3. Uma nova aba será aberta com a página de impressão formatada:
   - **Cabeçalho**: Dados da Ótica (Nome, CNPJ, Telefone).
   - **Lâminas das Parcelas**:
     - *Canhoto da Ótica (Via da Loja)*: Número da parcela, valor, data de vencimento e espaço para carimbo do caixa.
     - *Corpo do Carnê (Via do Cliente)*: Dados do cliente, CPF, endereço, número da OS, valor da parcela e instruções de pagamento.
4. Pressione `Ctrl + P` (ou clique em Imprimir) para enviar para sua impressora A4 ou impressora térmica.

### Como Dar Baixa em Parcela de Carnê:
1. Acesse o **Fluxo de Caixa** (`/caixa`).
2. Localize a parcela do cliente.
3. Clique em **Quitar Parcela**.
4. Caso a parcela esteja em atraso, o sistema calcula os dias de atraso e permite lançar juros/multa acordados com o cliente.
5. Confirme o recebimento para registrar a entrada financeira.

---

## 7. Esteira de Laboratório & Lensometria

A esteira de laboratório garante que nenhum óculos fique parado ou seja entregue com dioptria incorreta.

### Como Gerenciar a Produção dos Óculos:
1. No menu, clique em **Laboratório & Montagem** (`/laboratorio`).
2. Você verá as etapas de produção:
   - **1. Aguardando**: OS lançada no balcão aguardando envio de lente.
   - **2. Lab Externo**: Óculos cujas lentes foram encomendadas em laboratórios externos (como Essilor, Hoya, Zeiss ou laboratórios parceiros da sua região).
     - Clique em **📦 Lab Externo** para anotar o nome do fabricante e o número do pedido de rastreio.
   - **3. Em Montagem**: Lentes já chegaram na ótica e estão na bancada do montador.
   - **4. Pronto Balcão**: Óculos prontos para o cliente buscar.

### Lensometria & Controle de Qualidade:
Antes de avisar o cliente, o técnico deve conferir os graus no lensômetro:
- No card da OS, localize o bloco **Lensometria & Qualidade**.
- Após verificar que o grau montado confere com a receita, clique em **Validar OK**.
- O sistema registra o carimbo verde de conformidade técnica com data, hora e responsável.
- Clique em **Liberar Óculos p/ Balcão** para avisar o cliente da retirada.

---

## 8. Caixa, Recebimentos & PIX

### Conferência de Caixa Diário:
1. Acesse **Fluxo de Caixa** (`/caixa`).
2. Na parte superior, você acompanha:
   - **Total Vendido**: Volume total de vendas geradas na loja.
   - **Total Recebido em Dinheiro/Cartão/PIX**: O dinheiro real que entrou na gaveta ou conta da ótica no dia.
   - **Total a Receber**: Parcelas futuras a vencer de carnê e vendas a prazo.
3. Ao final do expediente, gerentes e administradores podem acessar **Fechamento de Caixa** para conferir as somas por forma de pagamento e fechar o turno.

### Cobrança Instantânea via PIX:
- Ao gerar uma venda com opção PIX, o sistema gera o QR Code na tela. O cliente aponta o aplicativo do banco e paga na hora, garantindo confirmação ágil e sem taxas de cartão.

---

## 9. Comissões da Equipe & Metas

### Para as Vendedoras:
- Clique em **Minhas Comissões** (`/minhas-comissoes`).
- Acompanhe em tempo real cada venda faturada no mês, o percentual de comissão aplicado e o total a receber no fechamento.

### Para o Gerente e Proprietário:
- Acesse **Comissões Equipe** (`/admin/comissoes`).
- Visualize as vendas individuais de cada atendente.
- Efetue o fechamento mensal com segurança contábil: ao fechar o mês, o sistema protege os registros históricos contra alterações indevidas.

---

## 10. Guia de Segurança & Firewall da VPS

Este tópico é voltado aos gerentes e técnicos responsáveis pela infraestrutura do servidor VPS onde o RETSYS está hospedado.

### Política de Firewall UFW (Segurança Máxima)
Para blindar o servidor da ótica contra ataques externos ou acessos indevidos, a regra de ouro é: **liberar no firewall apenas o estritamente necessário para o público.**

#### Portas que DEVEM estar ABERTAS no UFW:
| Porta | Protocolo | Finalidade | Regra UFW |
| :--- | :--- | :--- | :--- |
| **22** | TCP | Acesso administrativo remoto (SSH) | `sudo ufw allow 22/tcp` |
| **80** | TCP | Redirecionamento HTTP para HTTPS | `sudo ufw allow 80/tcp` |
| **443** | TCP | Tráfego web seguro criptografado (HTTPS) | `sudo ufw allow 443/tcp` |

#### Portas que NUNCA DEVEM estar ABERTAS na internet pública:
- ❌ **Porta 5432 (PostgreSQL)**: Deve ficar restrita ao loopback interno (`127.0.0.1`).
- ❌ **Porta 11434 (Ollama IA)**: Deve ficar restrita ao loopback interno (`127.0.0.1`).
- ❌ **Portas 5000 / 5001 (App .NET)**: Devem ficar restritas ao loopback interno (`127.0.0.1`), recebendo requisições apenas através do Nginx local.

#### Como verificar a conformidade no seu servidor:
No terminal da sua VPS, execute:
```bash
sudo ufw status verbose
```
O resultado esperado é:
```text
Status: active
Logging: on (low)
Default: deny (incoming), allow (outgoing), disabled (routed)

To                         Action      From
--                         ------      ----
22/tcp                     ALLOW IN    Anywhere
80/tcp                     ALLOW IN    Anywhere
443/tcp                    ALLOW IN    Anywhere
```

---

## 💡 Dúvidas Frequentes (FAQ)

**1. Posso cadastrar um cliente sem CPF?**
> Sim! No RETSYS o CPF é opcional no balcão para agilizar o atendimento. Você pode cadastrar apenas o Nome e o WhatsApp e preencher o CPF posteriormente se for vender no carnê.

**2. A Adição (AD) de perto é calculada sozinha?**
> Sim! Quando você digita o grau de longe e de perto, o sistema calcula a Adição automaticamente. Caso queira alterá-la, basta digitar o valor desejado.

**3. O que acontece com o estoque quando vendo óculos de sol no PDV de Balcão?**
> A quantidade em estoque da armação solar é reduzida automaticamente no mesmo instante, sem necessidade de abrir uma O.S. de laboratório.

**4. Como avisar o cliente que os óculos ficaram prontos?**
> Tanto na listagem de Ordens de Serviço quanto no Kanban e no Laboratório, basta clicar no ícone do WhatsApp **💬** para enviar a mensagem automática informando que o óculos está montado, conferido e pronto para retirada.

---
*RETSYS 2.0 - Tecnologia Especialista para Óticas de Alta Performance.*
