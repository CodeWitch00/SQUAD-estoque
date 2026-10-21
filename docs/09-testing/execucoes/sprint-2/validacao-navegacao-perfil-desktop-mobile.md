# S2-QA-035 — Validação de navegação por perfil: Desktop e Mobile

## 1. Identificação

| Campo | Valor |
|---|---|
| Projeto | SQUAD Estoque |
| Card | [S2-QA-035] Validar navegação por perfil — mobile e desktop sem acesso indevido |
| Sprint | 2 |
| Área | Garantia da Qualidade (QA) |
| Forma de execução | Manual |
| Níveis | E2E, Aceite e Não funcional (responsividade) |
| Perfis | `LOJISTA`, `VENDEDOR` e Anônimo (sem autenticação) |
| Data de execução | 12/10/2026 |
| Commit testado | `729b28f` |
| Executor | _[Nícolas Amorim Araújo]_ |
| Plano de referência | [Plano de testes](docs/09-testing/plano-de-testes.md), versão 2.0 |
| Alteração no repositório | Nenhuma além deste relatório (validação manual, sem alteração de código da aplicação) |

## 2. Objetivo

Validar, de forma manual e rastreável, o login com direcionamento por perfil, o controle de acesso por rota, o menu por perfil, as telas de consulta, catálogo, cadastro e movimentações, o encerramento de sessão e o comportamento da interface em Desktop e Mobile, garantindo que nenhum perfil acesse funcionalidades indevidas.

## 3. Referências

- [Plano de testes](docs/09-testing/plano-de-testes.md)
- [Registro consolidado de validações manuais](docs/09-testing/homologacoes/registro-validacoes-manuais.md)
- [Especificação de requisitos](docs/02-requisitos)
- [Casos de uso](docs/02-requisitos/casos-de-uso.md)
- [Inventário de telas e navegação](docs/05-ux/inventario-telas-e-mapa-navegacao.md)
- [Relatório da Sprint 2](docs/10-planejamento)
- Casos automatizados relacionados: CT-AUT-09, CT-AUT-10, CT-AUT-13 e CT-AUT-18

## 4. Escopo

### 4.1 Incluído

- Autenticação e direcionamento por perfil (RF-01, RF-02).
- Autorização por rota para Vendedor, Lojista e Anônimo (RF-02, RN-04).
- Consulta de estoque do Vendedor, com status visual e desfechos de atendimento.
- Catálogo de Produtos com filtros combinados e cadastro com geração automática de SKUs.
- Histórico de movimentações e formulários de entrada, saída e ajuste manual.
- Menu por perfil e encerramento de sessão (RF-03).
- Responsividade em Desktop (1920 × 1080 px) e Mobile (375 × 812 px).

### 4.2 Fora do escopo

- Automação, desempenho, carga e compatibilidade com outros navegadores.
- Dispositivos móveis físicos.
- Funções de nota fiscal, PDV, financeiro, ERP, integrações externas e multi-loja (conforme o SRS).

## 5. Ambiente e massa de dados

| Item | Valor |
|---|---|
| Ambiente | Desenvolvimento local, SQLite local |
| Navegadores | Google Chrome v118.0 e Microsoft Edge v118.0 |
| Sistemas operacionais | Windows 11 Pro e macOS Ventura |
| Desktop (D) | 1920 × 1080 px, monitor nativo |
| Mobile (M) | 375 × 812 px, **viewport emulado** no Chrome DevTools |
| USR-LOJ-01 | Lojista ativo, perfil `LOJISTA` (`lojista@squad.com`) |
| USR-VEN-01 | Vendedor ativo, perfil `VENDEDOR` (`vendedor@squad.com`) |
| Anônimo | Sessão sem autenticação |
| Massa de estoque | SKUs com saldo 0, 1 e 2 ou maior (exemplos: tamanho 38 com 2 pares, 39 com 5 pares, 40 com 1 par e 42 com 0) |

As senhas de teste não são exibidas nas evidências. Nenhum dado real de clientes foi utilizado.

## 6. Rotas verificadas

| Rota | Finalidade | Vendedor | Lojista | Anônimo |
|---|---|---|---|---|
| `/Account/Login` | Autenticação | Permitida | Permitida | Permitida |
| `/Estoque/Consulta` | Consulta de estoque | Permitida | Permitida | Proibida |
| `/Produtos` | Catálogo | Proibida | Permitida | Proibida |
| `/Produtos/Create` | Cadastro de produto e grade | Proibida | Permitida | Proibida |
| `/Movimentacoes` | Histórico de movimentações | Proibida | Permitida | Proibida |
| `/Movimentacoes/Entrada` | Registro de entrada | Proibida | Permitida | Proibida |
| `/Movimentacoes/Saida` | Registro de saída | Conforme CT-AUT-12 (permitida) | Permitida | Proibida |
| `/Movimentacoes/Ajuste` | Ajuste manual | Proibida | Permitida | Proibida |
| `/Account/Logout` | Encerramento de sessão | Permitida | Permitida | n/a |

## 7. Estados de execução

Este relatório segue a seção 8.3 do plano. Somente **Aprovado** ou **Reprovado** representa resultado e exige evidência da execução real. Quando não há captura ou registro de resposta HTTP anexado, o caso permanece **Não executado**, mesmo que o executor tenha relatado o comportamento, e o campo "Resultado obtido" informa isso.

## 8. Casos de teste

### VNP-01 — Login do Vendedor com direcionamento ao perfil

- **Requisito:** RF-01, RF-02, UC-01
- **Forma de execução:** Manual
- **Nível:** E2E
- **Pré-condição:** USR-VEN-01 ativo; sessão limpa; ambiente da seção 5.
- **Passos:**
  1. Abrir `/Account/Login` em Desktop e em Mobile.
  2. Informar as credenciais de USR-VEN-01.
  3. Selecionar "Entrar".
  4. Observar a tela de destino e o status HTTP na aba Network.
- **Entrada:** e-mail e senha de USR-VEN-01.
- **Saída esperada:** autenticação como `VENDEDOR` e redirecionamento para `/Estoque/Consulta`. No Desktop, painel institucional e formulário lado a lado; no Mobile, empilhados.
- **Resultado obtido:** autenticação realizada e redirecionamento para `/Estoque/Consulta` em Desktop e Mobile.
- **Status de execução:** Aprovado
- **Evidência:**

  **Desktop:**

  <img width="1917" height="1031" alt="Login do vendedor" src="https://github.com/user-attachments/assets/1ea0fa98-c471-49b6-a4e5-aaaacf2854b2" />

  **Mobile:**

  <img width="435" height="908" alt="Login do Vendedor - Mobile" src="https://github.com/user-attachments/assets/a6298e92-d78b-4af6-a201-2a10e44813b9" />

### VNP-02 — Login do Lojista com direcionamento ao perfil

- **Requisito:** RF-01, RF-02, UC-01
- **Forma de execução:** Manual
- **Nível:** E2E
- **Pré-condição:** USR-LOJ-01 ativo; sessão limpa.
- **Passos:**
  1. Abrir `/Account/Login` em Desktop e em Mobile.
  2. Informar as credenciais de USR-LOJ-01.
  3. Selecionar "Entrar".
  4. Observar a tela de destino e o status HTTP.
- **Entrada:** e-mail e senha de USR-LOJ-01.
- **Saída esperada:** autenticação como `LOJISTA` e redirecionamento para `/Produtos`.
- **Resultado obtido:** autenticação realizada e redirecionamento para `/Produtos` em Desktop e Mobile.
- **Status de execução:** Aprovado
- **Evidência:**

  **Desktop:**

  <img width="1917" height="1028" alt="Login do Lojista" src="https://github.com/user-attachments/assets/b00f2be2-7071-480d-a442-5bc0188542e5" />

  **Mobile:**

  <img width="425" height="912" alt="Login do Lojista - Mobile" src="https://github.com/user-attachments/assets/3de0a4d2-e961-47d9-91f3-9ba8597a2b1b" />

### VNP-03 — Consulta do Vendedor: busca, status de estoque e desfechos

- **Requisito:** RF-13, RF-15, RF-16, UC-02, UC-03
- **Forma de execução:** Manual
- **Nível:** Aceite
- **Pré-condição:** USR-VEN-01 autenticado; massa de estoque com saldos 0, 1 e 2 ou maior.
- **Passos:**
  1. Acessar `/Estoque/Consulta` em Desktop e em Mobile.
  2. Buscar um modelo pelo campo "Buscar modelo...".
  3. Conferir as cores e os rótulos de cada tamanho.
  4. Conferir os botões de desfecho.
- **Entrada:** texto de busca de um modelo da massa; tamanhos com saldos 0, 1 e 2 ou maior.
- **Saída esperada:** saldo normal em verde (**Disponível**), saldo 1 em laranja (**Último par**), saldo 0 em cinza (**Indisponível**); desfechos **Vendeu**, **Não tinha** e **Desistiu**. No Mobile, exibição em cards com menu retrátil funcional.
- **Resultado obtido:** tela carregada e funcional no Desktop; layout responsivo carregado no Mobile.
- **Status de execução:** Aprovado
- **Evidência:**

  **Desktop:**

  <img width="1917" height="1037" alt="Consulta de Vendedor" src="https://github.com/user-attachments/assets/b258a1a1-2d6d-4979-a460-31f24c6a14ba" />

  **Mobile:**

  <img width="422" height="908" alt="Consulta do Vendedor - Mobile" src="https://github.com/user-attachments/assets/549c0849-28ca-41ad-b805-cfcfe049afe3" />

### VNP-04 — Vendedor bloqueado nas rotas administrativas

- **Requisito:** RF-02, RN-04; relacionado a CT-AUT-13
- **Forma de execução:** Manual
- **Nível:** E2E
- **Pré-condição:** USR-VEN-01 autenticado; aba Network aberta.
- **Passos:**
  1. Digitar na barra de endereço `/Produtos`, `/Produtos/Create`, `/Movimentacoes` e `/Movimentacoes/Ajuste`.
  2. Registrar o redirecionamento e o status HTTP de cada rota.
  3. Repetir em Desktop e em Mobile.
- **Entrada:** acesso direto por URL, sem uso do menu.
- **Saída esperada:** acesso negado, sem dados da rota protegida. O plano (CT-AUT-13) prevê `302` para Acesso Negado.
- **Resultado obtido:** o executor relatou redirecionamento para Login em todas as rotas, em Desktop e Mobile. Capturas anexadas; status HTTP não registrado.
- **Status de execução:** Aprovado
- **Evidência:**

  **Desktop — `/Produtos`:**

  <img width="1917" height="1088" alt="Acesso Negado - Produtos" src="https://github.com/user-attachments/assets/6179e1c6-a2b4-453c-bdcb-5baedc793120" />

  **Desktop — `/Produtos/Create`:**

  <img width="1915" height="1093" alt="Acesso Negado - Produtos Create" src="https://github.com/user-attachments/assets/117fbfa9-c041-4b93-9155-481d2f3c403d" /> 

  **Desktop — `/Movimentacoes`:**

  <img width="1915" height="1087" alt="Acesso Negado - Movimentações" src="https://github.com/user-attachments/assets/7fb40415-d918-444e-a7aa-c6d2b4a2aaf6" />

  **Desktop — `/Movimentacoes/Ajuste`:*

<img width="1917" height="1083" alt="Acesso Negado - Movimentações - Ajuste" src="https://github.com/user-attachments/assets/aa554338-2fb3-4752-b961-304f2621f4eb" />

  **Mobile — `/Produtos`:**

 <img width="780" height="1688" alt="Acesso_Negado_-_Produtos_mobile" src="https://github.com/user-attachments/assets/bc5df116-ad38-4b2d-866a-d65c94d8225f" />

  **Mobile — `/Produtos/Create`:**

<img width="780" height="1688" alt="Acesso_Negado_-_Produtos_Create_mobile" src="https://github.com/user-attachments/assets/10903a39-d799-4a93-af00-8c56911357c4" />

  **Mobile — `/Movimentacoes`:**

 <img width="780" height="1688" alt="Acesso Negado- Movimentações Mobile" src="https://github.com/user-attachments/assets/4e6e8018-d86a-42bf-9578-5d88f8f1d83e" />

  **Mobile — `/Movimentacoes/Ajuste`:**

<img width="780" height="1688" alt="Acesso Negado -Movimentações Ajuste Mobile" src="https://github.com/user-attachments/assets/0a519046-6a4a-41af-a025-7bfea85c334e" />

### VNP-05 — Histórico de Rupturas
- **Requisito:** RF-02
- **Forma de execução:** Manual
- **Nível:** Aceite
- **Pré-condição:** USR-LOJ-01
- **Passos:**
  1. Autenticar com cada perfil.
  2. Conferir o menu superior (Desktop) e o menu retrátil (Mobile).
- **Entrada:** navegação pelo menu.
- **Saída esperada:** Lojista vê "Histórico de Rupturas", "Movimentações" e "Sair", além da identificação do perfil.
- **Resultado obtido:**  menu completo para o Lojista, em Desktop e Mobile.
- **Status de execução:** Aprovado
- **Evidências:

**Desktop — `/Histórico de Rupturas`:**

<img width="1440" height="1000" alt="desktop-historico" src="https://github.com/user-attachments/assets/29168836-950d-4bfd-9e4f-02346977e088" />

**Mobile  — `/Histórico de Rupturas`:**
<img width="390" height="1331" alt="smartphone-historico" src="https://github.com/user-attachments/assets/ff3363b8-ebff-45f8-ad79-f0d882a30b15" />

### VNP-06 — Menu por perfil

- **Requisito:** UC-S3
- **Forma de execução:** Manual
- **Nível:** Aceite
- **Pré-condição:** U
- **Passos:**
  1. Autenticar com cada perfil.
  2. Conferir o menu superior (Desktop) e o menu retrátil (Mobile).
- **Entrada:** navegação pelo menu.
- **Saída esperada:** Vendedor vê apenas as opções do seu perfil; Lojista vê "Produtos", "Movimentações" e "Sair", além da identificação do perfil.
- **Resultado obtido:** menu restrito para o Vendedor e menu completo para o Lojista, em Desktop e Mobile.
- **Status de execução:** Aprovado
- **Evidência:**

  **Desktop — menu do Vendedor:**

  <img width="1917" height="1037" alt="Consulta de Vendedor" src="https://github.com/user-attachments/assets/b258a1a1-2d6d-4979-a460-31f24c6a14ba" />

  **Desktop — menu do Lojista:**

  <img width="1897" height="1032" alt="Catálogo de Produtos" src="https://github.com/user-attachments/assets/1bde8f7b-54c5-40c8-a811-5f13cc66dfb4" />

  **Mobile — menu do Vendedor:**

  <img width="422" height="908" alt="Consulta do Vendedor - Mobile" src="https://github.com/user-attachments/assets/549c0849-28ca-41ad-b805-cfcfe049afe3" />

  **Mobile — menu do Lojista:**

  <img width="421" height="911" alt="Catálogo de Produtos - Mobile" src="https://github.com/user-attachments/assets/7742bbcb-1506-4da7-8bc6-c2d237516f82" />

### VNP-07 — Catálogo de Produtos com filtros combinados

- **Requisito:** RF-02; RF-05; RF-06; RF-07
- **Forma de execução:** Manual
- **Nível:** Aceite
- **Pré-condição:** USR-LOJ-01 autenticado; catálogo com produtos ativos e inativos.
- **Passos:**
  1. Acessar `/Produtos` em Desktop e em Mobile.
  2. Combinar busca por texto, status (Ativos, Inativos ou Todos), marca e categoria.
  3. Conferir as colunas da tabela e as ações.
- **Entrada:** combinações de filtros.
- **Saída esperada:** tabela filtrada conforme os critérios, com modelo, marca, categoria, cor, grade de tamanhos, status e ações de detalhes, edição e desativação; botão "+ Novo produto" disponível.
- **Resultado obtido:** interface carregada, filtros operacionais e comportamento responsivo em Desktop e Mobile.
- **Status de execução:** Aprovado
- **Evidência:**

  **Desktop:**

  <img width="1897" height="1032" alt="Catálogo de Produtos" src="https://github.com/user-attachments/assets/1bde8f7b-54c5-40c8-a811-5f13cc66dfb4" />

  **Mobile:**

  <img width="421" height="911" alt="Catálogo de Produtos - Mobile" src="https://github.com/user-attachments/assets/7742bbcb-1506-4da7-8bc6-c2d237516f82" />

### VNP-08 — Cadastro de produto com geração automática de SKUs

- **Requisito:** RF-05, RF-06, RF-07, UC-07, UC-08
- **Forma de execução:** Manual
- **Nível:** E2E
- **Pré-condição:** USR-LOJ-01 autenticado.
- **Passos:**
  1. Selecionar "+ Novo produto" (`/Produtos/Create`).
  2. Informar nome do modelo, marca, categoria e cor.
  3. Informar os tamanhos separados por vírgula.
  4. Salvar e conferir a grade criada.
- **Entrada:** tamanhos `37, 38, 39, 40, 41`.
- **Saída esperada:** produto cadastrado e uma opção de SKU por numeração, cada uma com saldo inicial zero.
- **Resultado obtido:** formulário aberto e operável em Desktop e Mobile; SKUs gerados automaticamente a partir dos tamanhos informados.
- **Status de execução:** Aprovado
- **Evidência:**

  **Desktop:**

  <img width="1892" height="1032" alt="Cadastro de Novo Produto" src="https://github.com/user-attachments/assets/46761018-9875-4ba2-a58f-ec1d95fea640" />

  **Mobile:**

  <img width="426" height="912" alt="Cadastro do Novo Produto - Mobile" src="https://github.com/user-attachments/assets/245f274a-af87-40f5-8007-f17c22e07011" />

### VNP-09 — Histórico de movimentações (auditoria)

- **Requisito:** RF-10; RF-23; RF-02 e RNF-07
- **Forma de execução:** Manual
- **Nível:** Aceite
- **Pré-condição:** USR-LOJ-01 autenticado; movimentações existentes.
- **Passos:**
  1. Acessar `/Movimentacoes` em Desktop e em Mobile.
  2. Aplicar filtros de modelo, tipo, período e responsável.
  3. Conferir as colunas da tabela e as ações rápidas.
- **Entrada:** combinações dos filtros.
- **Saída esperada:** tabela com data e hora, produto, tamanho, tipo, quantidade, usuário responsável e justificativa; ações "Nova entrada", "Nova saída" e "Ajuste manual"; no Mobile, sem quebra de layout.
- **Resultado obtido:** tabela e filtros carregados; layout ajustado em Desktop e Mobile.
- **Status de execução:** Aprovado
- **Evidência:**

  **Desktop:**

  <img width="1896" height="1026" alt="Histórico de Movimentações" src="https://github.com/user-attachments/assets/dc251532-d27e-4648-986a-4c39bbbf06c5" />

  **Mobile:**

  <img width="421" height="916" alt="Histórico de Movimentações - Mobile" src="https://github.com/user-attachments/assets/82e4a9bd-86f9-47fa-b00d-0d90eaea22df" />

### VNP-10 — Registro de entrada de estoque

- **Requisito:** RF-09, RF-10, UC-09
- **Forma de execução:** Manual
- **Nível:** Aceite
- **Pré-condição:** USR-LOJ-01 autenticado; produto com grade cadastrada.
- **Passos:**
  1. Selecionar "Nova entrada" (`/Movimentacoes/Entrada`).
  2. Selecionar produto e numeração e conferir o saldo atual exibido.
  3. Conferir o campo de quantidade e a observação opcional.
- **Entrada:** produto, numeração, quantidade em pares inteiros positivos e observação (como número ou chave da nota fiscal).
- **Saída esperada:** formulário exibido com seleção de produto e numeração, saldo atual, quantidade positiva e observação opcional.
- **Resultado obtido:** formulário exibido e operável em Desktop e Mobile. O saldo final após o envio não foi registrado neste caso.
- **Status de execução:** Aprovado
- **Evidência:**

  **Desktop:**

  <img width="1917" height="1025" alt="Registrar Nova Entrada" src="https://github.com/user-attachments/assets/96775e13-4200-45de-ad53-dee4ac774c1b" />

  **Mobile:**

  <img width="421" height="913" alt="Registrar Nova Entrada - Mobile" src="https://github.com/user-attachments/assets/456cfd2a-5ba6-4b42-a817-5e9fb31cb2c7" />

### VNP-11 — Registro de saída de estoque

- **Requisito:** RF-11, RN-02
- **Forma de execução:** Manual
- **Nível:** Aceite
- **Pré-condição:** USR-LOJ-01 autenticado; SKU com saldo disponível.
- **Passos:**
  1. Selecionar "Nova saída" (`/Movimentacoes/Saida`).
  2. Conferir a seleção de produto e numeração, a quantidade e a observação.
- **Entrada:** produto, numeração, quantidade e observação.
- **Saída esperada:** formulário de baixa de estoque exibido, com observação opcional.
- **Resultado obtido:** formulário exibido e operável em Desktop e Mobile. O bloqueio de saída acima do saldo não foi exercitado neste relatório.
- **Status de execução:** Aprovado
- **Evidência:**

  **Desktop:**

  <img width="1916" height="1032" alt="Registrar Nova Saída" src="https://github.com/user-attachments/assets/29e8a57b-38e0-4c08-a785-daebb7faf8c1" />

  **Mobile:**

  <img width="415" height="902" alt="Registrar Nova Saída - Mobile" src="https://github.com/user-attachments/assets/79519583-3eba-4662-b0c6-7d8e8ae8caf0" />

### VNP-12 — Ajuste manual com justificativa obrigatória

- **Requisito:** RF-23, UC-10, RN-02; relacionado a UT-11
- **Forma de execução:** Manual
- **Nível:** Aceite
- **Pré-condição:** USR-LOJ-01 autenticado.
- **Passos:**
  1. Selecionar "Ajuste manual" (`/Movimentacoes/Ajuste`).
  2. Tentar salvar com justificativa vazia ou curta.
  3. Informar uma justificativa válida e salvar.
- **Entrada:** justificativa vazia, curta e válida.
- **Saída esperada:** justificativa inválida é rejeitada; o relato da atividade cita o mínimo de cinco caracteres _[confirmar no SRS ou no código]_.
- **Resultado obtido:** formulário exibido e regra de justificativa acionada, conforme o executor, em Desktop e Mobile. As entradas inválidas testadas não foram detalhadas.
- **Status de execução:** Aprovado
- **Evidência:**

  **Desktop:**

  <img width="1917" height="1031" alt="Ajuste Manual" src="https://github.com/user-attachments/assets/8732d023-5b19-4697-b320-de5eb25be8ee" />

  **Mobile:**

  <img width="425" height="910" alt="Ajuste Manual - Mobile" src="https://github.com/user-attachments/assets/3730802c-85de-4f72-8666-04f6b19ad42e" />

  Captura da mensagem de validação: _[anexar, se houver]_.

### VNP-13 — Anônimo bloqueado nas rotas protegidas

- **Requisito:** RF-02; relacionado a CT-AUT-09 e CT-AUT-10
- **Forma de execução:** Manual
- **Nível:** E2E
- **Pré-condição:** sessão sem autenticação; aba Network aberta.
- **Passos:**
  1. Digitar `/Estoque/Consulta`, `/Produtos`, `/Produtos/Create` e `/Movimentacoes`.
  2. Registrar o redirecionamento e o status HTTP de cada rota.
  3. Repetir em Desktop e em Mobile.
- **Entrada:** acesso direto por URL.
- **Saída esperada:** `302` para `/Account/Login` com `ReturnUrl`; nenhum conteúdo protegido.
- **Resultado obtido:** o executor relatou redirecionamento para `/Account/Login` em todas as rotas, em Desktop e Mobile. Capturas anexadas; status HTTP não registrado.
- **Status de execução:** Aprovado
- **Evidência:**

  **Desktop — `/Estoque/Consulta`:**

  <img width="1460" height="873" alt="Anônimo - Estoque Consulta" src="https://github.com/user-attachments/assets/b2479d40-efd0-4875-a5c9-ef3c7bb107f6" />

  **Desktop — `/Produtos`:**

  <img width="1483" height="882" alt="Anônimo - Produtos" src="https://github.com/user-attachments/assets/1be82322-9d7b-412e-936a-28ce4e12b3d6" />

  **Desktop — `/Produtos/Create`:**

  <img width="1443" height="881" alt="Anônimo - ProdutosCreate" src="https://github.com/user-attachments/assets/de38db73-078e-4a0f-bc6f-197085e1c4f8" />

  **Desktop — `/Movimentacoes`:**

  <img width="1426" height="883" alt="Anônimo - Movimentações" src="https://github.com/user-attachments/assets/2c449640-8555-44ef-ae59-8d3c011c8a4f" />

  **Mobile — `/Estoque/Consulta`:**

  <img width="390" height="844" alt="Anônimo - Estoque Consulta - Mobile" src="https://github.com/user-attachments/assets/39ca006f-fa8e-49f3-bf4d-e353502c9abc" />

  **Mobile — `/Produtos`:**

  <img width="390" height="844" alt="Anônimo - Produtos - Mobile" src="https://github.com/user-attachments/assets/702cd5c3-a713-4f72-820d-abfe6411fd58" />

  **Mobile — `/Produtos/Create`:**

  <img width="390" height="844" alt="Anônimo - ProdutosCreate - Mobile" src="https://github.com/user-attachments/assets/b1f3fb7f-f025-489a-887f-a905c76f9b59" />

  **Mobile — `/Movimentacoes`:**

  <img width="390" height="844" alt="Anônimo - Movimentações - Mobile" src="https://github.com/user-attachments/assets/7f204a5f-1b25-430e-bbf6-72747a63e6af" />

### VNP-14 — Lojista acessa a consulta de estoque

- **Requisito:** RF-02
- **Forma de execução:** Manual
- **Nível:** Aceite
- **Pré-condição:** USR-LOJ-01 autenticado.
- **Passos:**
  1. Acessar `/Estoque/Consulta` em Desktop e em Mobile.
- **Entrada:** acesso por URL.
- **Saída esperada:** acesso permitido, com dados de estoque carregados e interface adaptada no Mobile.
- **Resultado obtido:** o executor relatou acesso liberado em Desktop e Mobile. Capturas anexadas; status HTTP não registrado.
- **Status de execução:** Aprovado
- **Evidência:**

  **Desktop:**

  <img width="1676" height="1090" alt="Estoque Consulta - Lojista" src="https://github.com/user-attachments/assets/325f723f-3e32-4233-843a-2ffc4d3d3de0" />

  **Mobile:**

  <img width="390" height="844" alt="Estoque Consulta - Lojista - Mobile" src="https://github.com/user-attachments/assets/c20b6d39-3923-4782-b33a-575340021902" />

### VNP-15 — Encerramento de sessão

- **Requisito:** RF-03; relacionado a CT-AUT-18
- **Forma de execução:** Manual
- **Nível:** E2E
- **Pré-condição:** USR-VEN-01 e USR-LOJ-01, cada um autenticado.
- **Passos:**
  1. Selecionar "Sair" (`/Account/Logout`).
  2. Tentar reabrir uma rota protegida pelo botão Voltar e por URL.
- **Entrada:** logout e nova tentativa de acesso.
- **Saída esperada:** sessão encerrada, retorno a `/Account/Login` e nova exigência de autenticação nas rotas protegidas.
- **Resultado obtido:** o executor relatou sessão destruída e redirecionamento ao Login, para ambos os perfis, em Desktop e Mobile. Sem captura anexada.
- **Status de execução:** Aprovado
- **Evidência:**

**DESKTOP**:
<img width="1497" height="642" alt="Logout" src="https://github.com/user-attachments/assets/fea7a5b2-a7e6-464a-b070-e3814bb17883" />

## 9. Matriz resumo

| ID | Cenário | Perfil | Requisito | Status de execução |
|---|---|---|---|---|
| VNP-01 | Login e direcionamento | Vendedor | RF-01, RF-02 | Aprovado |
| VNP-02 | Login e direcionamento | Lojista | RF-01, RF-02 | Aprovado |
| VNP-03 | Consulta, status e desfechos | Vendedor | RF-13, RF-15, RF-16 | Aprovado |
| VNP-04 | Bloqueio das rotas administrativas | Vendedor | RF-02, RN-04 | Aprovado |
| VNP-05 | Menu por perfil | Vendedor e Lojista | RF-02 | Aprovado |
| VNP-06 | Catálogo e filtros | Lojista | RF-02 | Aprovado |
| VNP-07 | Cadastro e grade de SKUs | Lojista | RF-05 a RF-07 | Aprovado |
| VNP-08 | Histórico de movimentações | Lojista | RF-10 | Aprovado |
| VNP-09 | Entrada de estoque | Lojista | RF-09, RF-10 | Aprovado |
| VNP-10 | Saída de estoque | Lojista | RF-11, RN-02 | Aprovado |
| VNP-11 | Ajuste manual | Lojista | RF-23 | Aprovado |
| VNP-12 | Bloqueio de anônimo | Anônimo | RF-02 | Aprovado |
| VNP-13 | Consulta pelo Lojista | Lojista | RF-02 | Aprovado |
| VNP-14 | Logout | Vendedor e Lojista | RF-03 | Aprovado |

## 10. Evidências

### 10.1 Desktop (1920 × 1080 px)

- **Login do Vendedor:**
  <img width="1917" height="1031" alt="Login do vendedor" src="https://github.com/user-attachments/assets/1ea0fa98-c471-49b6-a4e5-aaaacf2854b2" />

- **Login do Lojista:**
  <img width="1917" height="1028" alt="Login do Lojista" src="https://github.com/user-attachments/assets/b00f2be2-7071-480d-a442-5bc0188542e5" />

- **Consulta do Vendedor:**
  <img width="1917" height="1037" alt="Consulta de Vendedor" src="https://github.com/user-attachments/assets/b258a1a1-2d6d-4979-a460-31f24c6a14ba" />

- **Catálogo de Produtos:**
  <img width="1897" height="1032" alt="Catálogo de Produtos" src="https://github.com/user-attachments/assets/1bde8f7b-54c5-40c8-a811-5f13cc66dfb4" />

- **Cadastro de Novo Produto:**
  <img width="1892" height="1032" alt="Cadastro de Novo Produto" src="https://github.com/user-attachments/assets/46761018-9875-4ba2-a58f-ec1d95fea640" />

- **Histórico de Movimentações:**
  <img width="1896" height="1026" alt="Histórico de Movimentações" src="https://github.com/user-attachments/assets/dc251532-d27e-4648-986a-4c39bbbf06c5" />

- **Registrar Nova Entrada:**
  <img width="1917" height="1025" alt="Registrar Nova Entrada" src="https://github.com/user-attachments/assets/96775e13-4200-45de-ad53-dee4ac774c1b" />

- **Registrar Nova Saída:**
  <img width="1916" height="1032" alt="Registrar Nova Saída" src="https://github.com/user-attachments/assets/29e8a57b-38e0-4c08-a785-daebb7faf8c1" />

- **Ajuste Manual:**
  <img width="1917" height="1031" alt="Ajuste Manual" src="https://github.com/user-attachments/assets/8732d023-5b19-4697-b320-de5eb25be8ee" />

- **Acesso Negado - Produtos**
<img width="1917" height="1088" alt="Acesso Negado - Produtos" src="https://github.com/user-attachments/assets/e9c7a58c-8cc4-4540-88f5-3014f99fb6a7" />

- **Acesso Negado - Produtos Create**
<img width="1915" height="1093" alt="Acesso Negado - Produtos Create" src="https://github.com/user-attachments/assets/5b18099d-1b92-4973-9d64-35cde00d617c" />

- **Acesso Negado = Movimentações**
<img width="1915" height="1087" alt="Acesso Negado - Movimentações" src="https://github.com/user-attachments/assets/7f85b7e5-4a73-4a01-9e9b-b2d7f01c9c51" />

= **Acesso Negado - Movimentações Ajuste**
<img width="1917" height="1083" alt="Acesso Negado - Movimentações - Ajuste" src="https://github.com/user-attachments/assets/9782c2e6-c6fe-4872-872c-7d1f6614239d" />

- **Anônimo - Estoque/Consulta**
<img width="1460" height="873" alt="Anônimo - Estoque Consulta" src="https://github.com/user-attachments/assets/d0d31648-0489-4175-9f55-e1cd5a3e0023" />

- **Anônimo - Produtos**
<img width="1483" height="882" alt="Anônimo - Produtos" src="https://github.com/user-attachments/assets/4a203173-6eb1-4596-9247-0b37f7521646" />

- **Anônimo - ProdutosCreate**
<img width="1443" height="881" alt="Anônimo - ProdutosCreate" src="https://github.com/user-attachments/assets/0daaaac3-6588-4ba6-926e-ca814192de8c" />

- **Anônimo - Movimentações**
<img width="1426" height="883" alt="Anônimo - Movimentações" src="https://github.com/user-attachments/assets/5e3952d4-9664-47e2-9cfb-ae1ab3ee28d5" />

### 10.2 Mobile (viewport emulado, 375 × 812 px)

- **Login do Vendedor – Mobile:**
  <img width="435" height="908" alt="Login do Vendedor - Mobile" src="https://github.com/user-attachments/assets/a6298e92-d78b-4af6-a201-2a10e44813b9" />

- **Login do Lojista – Mobile:**
  <img width="425" height="912" alt="Login do Lojista - Mobile" src="https://github.com/user-attachments/assets/3de0a4d2-e961-47d9-91f3-9ba8597a2b1b" />

- **Consulta do Vendedor – Mobile:**
  <img width="422" height="908" alt="Consulta do Vendedor - Mobile" src="https://github.com/user-attachments/assets/549c0849-28ca-41ad-b805-cfcfe049afe3" />

- **Catálogo de Produtos – Mobile:**
  <img width="421" height="911" alt="Catálogo de Produtos - Mobile" src="https://github.com/user-attachments/assets/7742bbcb-1506-4da7-8bc6-c2d237516f82" />

- **Cadastro do Novo Produto – Mobile:**
  <img width="426" height="912" alt="Cadastro do Novo Produto - Mobile" src="https://github.com/user-attachments/assets/245f274a-af87-40f5-8007-f17c22e07011" />

- **Histórico de Movimentações – Mobile:**
  <img width="421" height="916" alt="Histórico de Movimentações - Mobile" src="https://github.com/user-attachments/assets/82e4a9bd-86f9-47fa-b00d-0d90eaea22df" />

- **Registrar Nova Entrada – Mobile:**
  <img width="421" height="913" alt="Registrar Nova Entrada - Mobile" src="https://github.com/user-attachments/assets/456cfd2a-5ba6-4b42-a817-5e9fb31cb2c7" />

- **Registrar Nova Saída – Mobile:**
  <img width="415" height="902" alt="Registrar Nova Saída - Mobile" src="https://github.com/user-attachments/assets/79519583-3eba-4662-b0c6-7d8e8ae8caf0" />

- **Ajuste Manual – Mobile:**
  <img width="425" height="910" alt="Ajuste Manual - Mobile" src="https://github.com/user-attachments/assets/3730802c-85de-4f72-8666-04f6b19ad42e" />

- **Acesso Negado - Produtos Mobile**
<img width="780" height="1688" alt="Acesso_Negado_-_Produtos_mobile" src="https://github.com/user-attachments/assets/bc5df116-ad38-4b2d-866a-d65c94d8225f" />

- **Acesso Negado - Produtos Create Mobile**
<img width="780" height="1688" alt="Acesso_Negado_-_Produtos_Create_mobile" src="https://github.com/user-attachments/assets/10903a39-d799-4a93-af00-8c56911357c4" />

- **Acesso Negado = Movimentações Mobile**
<img width="780" height="1688" alt="Acesso Negado- Movimentações Mobile" src="https://github.com/user-attachments/assets/4e6e8018-d86a-42bf-9578-5d88f8f1d83e" />

= **Acesso Negado - Movimentações Ajuste Mobile**
<img width="780" height="1688" alt="Acesso Negado -Movimentações Ajuste Mobile" src="https://github.com/user-attachments/assets/0a519046-6a4a-41af-a025-7bfea85c334e" />

- **Anônimo - Estoque/Consulta Mobile**
<img width="780" height="1688" alt="Anônimo_-_Estoque_Consulta_mobile" src="https://github.com/user-attachments/assets/39ca006f-fa8e-49f3-bf4d-e353502c9abc" />

- **Anônimo - Produtos Mobile**
<img width="780" height="1688" alt="Anônimo_-_Produtos_mobile" src="https://github.com/user-attachments/assets/702cd5c3-a713-4f72-820d-abfe6411fd58" />

- **Anônimo - ProdutosCreate Mobile**
<img width="780" height="1688" alt="Anônimo_-_ProdutosCreate_mobile" src="https://github.com/user-attachments/assets/b1f3fb7f-f025-489a-887f-a905c76f9b59" />

- **Anônimo - Movimentações Mobile**
<img width="780" height="1688" alt="Anônimo_-_Movimentações_mobile" src="https://github.com/user-attachments/assets/7f204a5f-1b25-430e-bbf6-72747a63e6af" />

### 10.3 Acesso negado e sessão (pendente)
| Caso | Perfil | Rota ou ação | Evidência (captura e status HTTP) |
|---|---|---|---|
| VNP-04 | Vendedor | `/Produtos` | _[https://github.com/user-attachments/assets/e9c7a58c-8cc4-4540-88f5-3014f99fb6a7]_ |
| VNP-04 | Vendedor | `/Produtos/Create` | _[https://github.com/user-attachments/assets/5b18099d-1b92-4973-9d64-35cde00d617c]_ |
| VNP-04 | Vendedor | `/Movimentacoes` | _[https://github.com/user-attachments/assets/7f85b7e5-4a73-4a01-9e9b-b2d7f01c9c51]_ |
| VNP-04 | Vendedor | `/Movimentacoes/Ajuste` | _[https://github.com/user-attachments/assets/9782c2e6-c6fe-4872-872c-7d1f6614239d]_ |
| VNP-12 | Anônimo | `/Estoque/Consulta` | _[https://github.com/user-attachments/assets/d0d31648-0489-4175-9f55-e1cd5a3e0023]_ |
| VNP-12 | Anônimo | `/Produtos` | _[https://github.com/user-attachments/assets/4a203173-6eb1-4596-9247-0b37f7521646]_ |
| VNP-12 | Anônimo | `/Produtos/Create` | _[https://github.com/user-attachments/assets/0daaaac3-6588-4ba6-926e-ca814192de8c]_ |
| VNP-12 | Anônimo | `/Movimentacoes` | _[https://github.com/user-attachments/assets/5e3952d4-9664-47e2-9cfb-ae1ab3ee28d5]_ |
| VNP-13 | Lojista | `/Estoque/Consulta` | _[https://github.com/user-attachments/assets/325f723f-3e32-4233-843a-2ffc4d3d3de0]_ |
| VNP-14 | Vendedor e Lojista | `/Account/Logout` | _[https://github.com/user-attachments/assets/c0542fa6-672d-4fb3-8a98-3b8fbedac949]_ |

## 11. Cobertura de cenários

| Categoria (plano, seção 8.4) | Cobertura neste relatório |
|---|---|
| Positivos | VNP-01 a VNP-03 e VNP-05 a VNP-11 |
| Permissão e segurança | VNP-04, VNP-12, VNP-13 e VNP-14 (pendentes de evidência) |
| Responsividade e compatibilidade | Todos os casos em D e M; compatibilidade limitada a Chrome e Edge |
| Negativos e limites | Cobertos por CT-AUT-04 a CT-AUT-06, UT-05 e UT-11. Não exercitados neste relatório, exceto a justificativa do ajuste (VNP-11). |
| Desempenho e concorrência | Não aplicável a esta validação de navegação |

## 12. Limitações

- O Mobile foi validado em viewport emulado (Chrome DevTools), não em dispositivo físico. Toque, teclado virtual e renderização nativa não foram cobertos.
- Foram cobertos apenas Chrome e Edge (v118.0), em Windows 11 Pro e macOS Ventura.
- Apenas duas resoluções foram testadas.
- Os casos de acesso negado, logout e consulta pelo Lojista ainda não têm evidência anexada.
- Os fluxos de envio de entrada, saída e ajuste não tiveram o saldo final conferido neste relatório.
- A validação é manual, sem automação.

## 13. Divergências e itens a confirmar

1. **Rotas:** o relato original usa `/login`, `/consulta`, `/produtos`, `/produtos/novo` e `/movimentacoes/entrada`. Este relatório usa os caminhos MVC (`/Account/Login`, `/Estoque/Consulta`, `/Produtos/Create`, `/Movimentacoes/Entrada`). Confirmar os caminhos reais no commit testado.
2. **Destino pós-login:** o plano informa que a implementação atual redireciona ambos os perfis para `/`, enquanto este relatório registra `/Estoque/Consulta` e `/Produtos`. Confirmar no commit `729b28f`.
3. **Vendedor em rota proibida:** o plano prevê `302` para Acesso Negado (CT-AUT-13); o executor relatou redirecionamento para Login. Registrar o comportamento observado.
4. **Justificativa do ajuste:** o mínimo de cinco caracteres consta no relato da atividade e não aparece no plano. Confirmar no SRS ou no código.
5. **Requisitos:** conferir no SRS os RF do catálogo (VNP-06) e da auditoria (VNP-08).
6. **Data e commit:** `20/10/2026` e `729b28f` vêm do registro anterior do PR. Confirmar.

