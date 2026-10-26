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
| Executor | Nícolas Amorim Araújo |
| Plano de referência | [Plano de testes](../../plano-de-testes.md), versão 2.0 |
| Alteração no repositório | Este relatório e as evidências em `docs/09-testing/evidencias/s2-qa-035/`; remoção de `site.mobile.css` (arquivo fora do escopo do cartão, não carregado por nenhuma página da aplicação) e do relatório antigo `docs/09-testing/Validacao.do.perfil.-.Desktop.e.Mobile.md`. Sem alteração de código da aplicação. |

## 2. Objetivo

Validar, de forma manual e rastreável, o login com direcionamento por perfil, o controle de acesso por rota, o menu por perfil, as telas de consulta, catálogo, cadastro, movimentações e histórico de rupturas, o encerramento de sessão e o comportamento da interface em Desktop e Mobile, garantindo que nenhum perfil acesse funcionalidades indevidas.

## 3. Referências

- [Plano de testes](../../plano-de-testes.md)
- [Registro consolidado de validações manuais](../../homologacoes/registro-validacoes-manuais.md)
- [Especificação de requisitos (SRS)](../../../02-requisitos/srs.md)
- [Casos de uso](../../../02-requisitos/casos-de-uso.md)
- [Inventário de telas e navegação](../../05-ux/inventario-telas-e-mapa-navegacao.md)
- [Relatório da Sprint 2](../../../10-planejamento/relatorio-sprint-2.md)
- Casos automatizados relacionados: CT-AUT-09, CT-AUT-10, CT-AUT-13 e CT-AUT-18

## 4. Escopo

### 4.1 Incluído

- Autenticação e direcionamento por perfil (RF-01, RF-02).
- Autorização por rota para Vendedor, Lojista e Anônimo (RF-02, RN-04).
- Consulta de estoque do Vendedor, com status visual e desfechos de atendimento.
- Catálogo de Produtos com filtros combinados e cadastro com geração automática de SKUs.
- Histórico de movimentações e formulários de entrada, saída e ajuste manual.
- Histórico de Rupturas do Lojista.
- Menu por perfil e encerramento de sessão (RF-03).
- Responsividade em Desktop (1920 × 1080 px) e Mobile (375 × 812 px).

### 4.2 Fora do escopo

- Automação, desempenho, carga e compatibilidade com outros navegadores.
- Dispositivos móveis físicos.
- Funções de nota fiscal, PDV, financeiro, ERP, integrações externas e multi-loja (conforme o SRS).

## 5. Ambiente e massa de dados

| Item | Valor |
|---|---|
| Ambiente | Azure App Service — `asp-squad-estoque-free-hgabd8asd5f4a9g9.northcentralus-01.azurewebsites.net` |
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
| `/Account/AccessDenied` | Página "Acesso Não Autorizado" | Permitida | Permitida | Encaminhada ao Login |
| `/Estoque/Consulta` | Consulta de estoque | Permitida | **Proibida** | Proibida |
| `/Produtos` | Catálogo | Proibida | Permitida | Proibida |
| `/Produtos/Create` | Cadastro de produto e grade | Proibida | Permitida | Proibida |
| `/Movimentacoes` | Histórico de movimentações | Proibida | Permitida | Proibida |
| `/Movimentacoes/Entrada` | Registro de entrada | Proibida | Permitida | Proibida |
| `/Movimentacoes/Saida` | Registro de saída | Permitida | Permitida | Proibida |
| `/Movimentacoes/Ajuste` | Ajuste manual | Proibida | Permitida | Proibida |
| `/Rupturas` | Histórico de Rupturas | Proibida | Permitida | Proibida |
| `/Account/Logout` | Encerramento de sessão | Permitida | Permitida | n/a |

Notas técnicas (conferidas no código do commit testado e da `main` atual, idênticos nestes pontos):

- Rota conventional `{controller=Home}/{action=Index}/{id?}` em `Program.cs`; `LoginPath=/Account/Login` e `AccessDeniedPath=/Account/AccessDenied` na configuração de cookie.
- `EstoqueController` exige `[Authorize(Roles = "VENDEDOR")]` → o LOJISTA é **negado** em `/Estoque/Consulta` (redirecionado para `/Account/AccessDenied`), ao contrário do que afirmava a versão anterior deste relatório.
- `ProdutosController` e `RupturasController` exigem `LOJISTA`; `MovimentacoesController.Saida` aceita `LOJISTA,VENDEDOR`.
- Após o login, `VENDEDOR` é direcionado a `/Estoque/Consulta` e `LOJISTA` a `/Produtos`.
- A tela de Histórico de Rupturas fica em `/Rupturas` (`RupturasController.Index`, view `Views/Rupturas/Index.cshtml`).

## 6.1 Matriz de acesso direto por perfil e viewport

A matriz abaixo consolida os acessos diretos por URL documentados neste relatório. As capturas são evidência visual do destino exibido; **os códigos HTTP não foram coletados** e, portanto, não são afirmados como resultado. O destino exibido nas capturas foi conferido na barra de endereço do navegador. As dimensões das capturas não correspondem todas exatamente à viewport declarada na seção 5; consulte as limitações da seção 12.

| Perfil | Viewport | Rota acessada | Resultado esperado | Resultado obtido documentado | Evidência |
|---|---|---|---|---|---|
| LOJISTA | Desktop | `/Produtos` | Permitido; catálogo exibido | Catálogo exibido na captura | [Catálogo — Desktop](../../evidencias/s2-qa-035/catalogo-de-produtos-1bde8f7b.png) |
| LOJISTA | Mobile | `/Produtos` | Permitido; catálogo responsivo | Catálogo exibido na captura | [Catálogo — Mobile](../../evidencias/s2-qa-035/catalogo-de-produtos-mobile-7742bbcb.png) |
| LOJISTA | Desktop | `/Estoque/Consulta` | Negado; a rota exige `VENDEDOR` → `/Account/AccessDenied?ReturnUrl=%2FEstoque%2FConsulta` | **Não confirmado:** a captura rotulada "Lojista" mostra a sessão `VENDEDOR` em `/Estoque/Consulta` e não comprova o acesso do LOJISTA | [Captura — Desktop](../../evidencias/s2-qa-035/estoque-consulta-lojista-325f723f.png) |
| LOJISTA | Mobile | `/Estoque/Consulta` | Negado; a rota exige `VENDEDOR` → `/Account/AccessDenied?ReturnUrl=%2FEstoque%2FConsulta` | **Não confirmado:** a captura rotulada "Lojista" mostra a sessão `VENDEDOR` em `/Estoque/Consulta` e não comprova o acesso do LOJISTA | [Captura — Mobile](../../evidencias/s2-qa-035/estoque-consulta-lojista-mobile-c20b6d39.png) |
| LOJISTA | Desktop | `/Movimentacoes` | Permitido; histórico exibido | Histórico exibido na captura | [Movimentações — Desktop](../../evidencias/s2-qa-035/historico-de-movimentacoes-dc251532.png) |
| LOJISTA | Mobile | `/Movimentacoes` | Permitido; histórico responsivo | Histórico exibido na captura | [Movimentações — Mobile](../../evidencias/s2-qa-035/historico-de-movimentacoes-mobile-82e4a9bd.png) |
| VENDEDOR | Desktop | `/Estoque/Consulta` | Permitido; consulta exibida | Consulta exibida na captura | [Consulta — Desktop](../../evidencias/s2-qa-035/consulta-de-vendedor-b258a1a1.png) |
| VENDEDOR | Mobile | `/Estoque/Consulta` | Permitido; consulta responsiva | Consulta exibida na captura | [Consulta — Mobile](../../evidencias/s2-qa-035/consulta-do-vendedor-mobile-549c0849.png) |
| VENDEDOR | Desktop | `/Produtos` | Negado; sem conteúdo administrativo | Página "Acesso Não Autorizado" exibida (URL final `/Account/AccessDenied?ReturnUrl=…`); sessão `VENDEDOR` ativa | [Bloqueio — Desktop](../../evidencias/s2-qa-035/acesso-negado-produtos-e9c7a58c.png) |
| VENDEDOR | Mobile | `/Produtos` | Negado; sem conteúdo administrativo | Página "Acesso Não Autorizado" exibida; sessão `VENDEDOR` ativa | [Bloqueio — Mobile](../../evidencias/s2-qa-035/acesso-negado-produtos-mobile-bc5df116.png) |
| VENDEDOR | Desktop | `/Produtos/Create` | Negado; sem formulário administrativo | Página "Acesso Não Autorizado" exibida (URL final `/Account/AccessDenied?ReturnUrl=…`); sessão `VENDEDOR` ativa | [Bloqueio — Desktop](../../evidencias/s2-qa-035/acesso-negado-produtos-create-5b18099d.png) |
| VENDEDOR | Mobile | `/Produtos/Create` | Negado; sem formulário administrativo | Página "Acesso Não Autorizado" exibida; sessão `VENDEDOR` ativa | [Bloqueio — Mobile](../../evidencias/s2-qa-035/acesso-negado-produtos-create-mobile-10903a39.png) |
| VENDEDOR | Desktop | `/Movimentacoes` | Negado; sem histórico administrativo | Página "Acesso Não Autorizado" exibida (URL final `/Account/AccessDenied?ReturnUrl=…`); sessão `VENDEDOR` ativa | [Bloqueio — Desktop](../../evidencias/s2-qa-035/acesso-negado-movimentacoes-7f85b7e5.png) |
| VENDEDOR | Mobile | `/Movimentacoes` | Negado; sem histórico administrativo | Página "Acesso Não Autorizado" exibida; sessão `VENDEDOR` ativa | [Bloqueio — Mobile](../../evidencias/s2-qa-035/acesso-negado-movimentacoes-mobile-4e6e8018.png) |
| VENDEDOR | Desktop | `/Movimentacoes/Ajuste` | Negado; sem formulário de ajuste | Página "Acesso Não Autorizado" exibida (URL final `/Account/AccessDenied?ReturnUrl=…`); sessão `VENDEDOR` ativa | [Bloqueio — Desktop](../../evidencias/s2-qa-035/acesso-negado-movimentacoes-ajuste-9782c2e6.png) |
| VENDEDOR | Mobile | `/Movimentacoes/Ajuste` | Negado; sem formulário de ajuste | Página "Acesso Não Autorizado" exibida; sessão `VENDEDOR` ativa | [Bloqueio — Mobile](../../evidencias/s2-qa-035/acesso-negado-movimentacoes-ajuste-mobile-0a519046.png) |
| Anônimo | Desktop | `/Estoque/Consulta` | Redirecionar para `/Account/Login?ReturnUrl=…`; sem conteúdo protegido | Captura documenta a tela de login; HTTP não registrado | [Anônimo — Desktop](../../evidencias/s2-qa-035/anonimo-estoque-consulta-d0d31648.png) |
| Anônimo | Mobile | `/Estoque/Consulta` | Redirecionar para `/Account/Login?ReturnUrl=…`; sem conteúdo protegido | Captura documenta a tela de login; HTTP não registrado | [Anônimo — Mobile](../../evidencias/s2-qa-035/anonimo-estoque-consulta-mobile-39ca006f.png) |
| Anônimo | Desktop | `/Produtos` | Redirecionar para `/Account/Login?ReturnUrl=…`; sem conteúdo protegido | Captura documenta a tela de login; HTTP não registrado | [Anônimo — Desktop](../../evidencias/s2-qa-035/anonimo-produtos-4a203173.png) |
| Anônimo | Mobile | `/Produtos` | Redirecionar para `/Account/Login?ReturnUrl=…`; sem conteúdo protegido | Captura documenta a tela de login; HTTP não registrado | [Anônimo — Mobile](../../evidencias/s2-qa-035/anonimo-produtos-mobile-702cd5c3.png) |
| Anônimo | Desktop | `/Produtos/Create` | Redirecionar para `/Account/Login?ReturnUrl=…`; sem conteúdo protegido | Captura documenta a tela de login; HTTP não registrado | [Anônimo — Desktop](../../evidencias/s2-qa-035/anonimo-produtoscreate-0daaaac3.png) |
| Anônimo | Mobile | `/Produtos/Create` | Redirecionar para `/Account/Login?ReturnUrl=…`; sem conteúdo protegido | Captura documenta a tela de login; HTTP não registrado | [Anônimo — Mobile](../../evidencias/s2-qa-035/anonimo-produtoscreate-mobile-b1f3fb7f.png) |
| Anônimo | Desktop | `/Movimentacoes` | Redirecionar para `/Account/Login?ReturnUrl=…`; sem conteúdo protegido | Captura documenta a tela de login; HTTP não registrado | [Anônimo — Desktop](../../evidencias/s2-qa-035/anonimo-movimentacoes-5e3952d4.png) |
| Anônimo | Mobile | `/Movimentacoes` | Redirecionar para `/Account/Login?ReturnUrl=…`; sem conteúdo protegido | Captura documenta a tela de login; HTTP não registrado | [Anônimo — Mobile](../../evidencias/s2-qa-035/anonimo-movimentacoes-mobile-7f204a5f.png) |

**Pendência de validação:** para declarar os bloqueios como aprovados segundo o plano, ainda é necessário registrar o comportamento real e, quando exigido pelo caso, o status HTTP na aba Network. Não inventar `302` sem captura ou registro da resposta.

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

  <img width="1917" height="1031" alt="Login do vendedor" src="../../evidencias/s2-qa-035/login-do-vendedor-1ea0fa98.png" />

  **Mobile:**

  <img width="435" height="908" alt="Login do Vendedor - Mobile" src="../../evidencias/s2-qa-035/login-do-vendedor-mobile-a6298e92.png" />

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

  <img width="1917" height="1028" alt="Login do Lojista" src="../../evidencias/s2-qa-035/login-do-lojista-b00f2be2.png" />

  **Mobile:**

  <img width="425" height="912" alt="Login do Lojista - Mobile" src="../../evidencias/s2-qa-035/login-do-lojista-mobile-3de0a4d2.png" />

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

  <img width="1917" height="1037" alt="Consulta de Vendedor" src="../../evidencias/s2-qa-035/consulta-de-vendedor-b258a1a1.png" />

  **Mobile:**

  <img width="422" height="908" alt="Consulta do Vendedor - Mobile" src="../../evidencias/s2-qa-035/consulta-do-vendedor-mobile-549c0849.png" />

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
- **Resultado obtido:** em todas as quatro rotas, em Desktop e Mobile, o destino final foi `/Account/AccessDenied?ReturnUrl=…`, exibindo a página "Acesso Não Autorizado" com a sessão `VENDEDOR` ainda ativa no cabeçalho; nenhum dado protegido foi exibido. Capturas anexadas; status HTTP não registrado. O comportamento observado corresponde ao previsto no plano (Acesso Negado), e não ao "redirecionamento para Login" registrado na versão anterior deste relatório; ver seção 13, item 3.
- **Status de execução:** Aprovado
- **Evidência:**

  **Desktop — `/Produtos`:**

  <img width="1917" height="1088" alt="Acesso Negado - Produtos" src="../../evidencias/s2-qa-035/acesso-negado-produtos-6179e1c6.png" />

  **Desktop — `/Produtos/Create`:**

  <img width="1915" height="1093" alt="Acesso Negado - Produtos Create" src="../../evidencias/s2-qa-035/acesso-negado-produtos-create-117fbfa9.png" />

  **Desktop — `/Movimentacoes`:**

  <img width="1915" height="1087" alt="Acesso Negado - Movimentações" src="../../evidencias/s2-qa-035/acesso-negado-movimentacoes-7fb40415.png" />

  **Desktop — `/Movimentacoes/Ajuste`:**

  <img width="1917" height="1083" alt="Acesso Negado - Movimentações - Ajuste" src="../../evidencias/s2-qa-035/acesso-negado-movimentacoes-ajuste-aa554338.png" />

  **Mobile — `/Produtos`:**

  <img width="780" height="1688" alt="Acesso_Negado_-_Produtos_mobile" src="../../evidencias/s2-qa-035/acesso-negado-produtos-mobile-bc5df116.png" />

  **Mobile — `/Produtos/Create`:**

  <img width="780" height="1688" alt="Acesso_Negado_-_Produtos_Create_mobile" src="../../evidencias/s2-qa-035/acesso-negado-produtos-create-mobile-10903a39.png" />

  **Mobile — `/Movimentacoes`:**

  <img width="780" height="1688" alt="Acesso Negado- Movimentações Mobile" src="../../evidencias/s2-qa-035/acesso-negado-movimentacoes-mobile-4e6e8018.png" />

  **Mobile — `/Movimentacoes/Ajuste`:**

  <img width="780" height="1688" alt="Acesso Negado -Movimentações Ajuste Mobile" src="../../evidencias/s2-qa-035/acesso-negado-movimentacoes-ajuste-mobile-0a519046.png" />

### VNP-05 — Lojista acessa o Histórico de Rupturas

- **Requisito:** RF-02, RF-22; UC-12 _[confirmar no SRS]_
- **Forma de execução:** Manual
- **Nível:** Aceite
- **Pré-condição:** USR-LOJ-01 autenticado; registros de ruptura existentes.
- **Passos:**
  1. Autenticar com USR-LOJ-01.
  2. Selecionar "Histórico de rupturas" no menu superior (Desktop) e no menu retrátil (Mobile), que aponta para `/Rupturas`.
  3. Conferir a tela exibida e o status HTTP.
- **Entrada:** navegação pelo menu.
- **Saída esperada:** acesso permitido ao Lojista (`RupturasController` exige `LOJISTA`); histórico simples com modelo, numeração e quantidade de ocorrências; interface sem quebra de layout no Mobile.
- **Resultado obtido:** tela de Histórico de Rupturas acessada pelo Lojista em Desktop e Mobile, conforme capturas. Contagem de ocorrências não conferida contra os registros; status HTTP não registrado.
- **Status de execução:** Aprovado
- **Evidência:**

  **Desktop — Histórico de Rupturas:**

  <img width="1440" height="1000" alt="desktop-historico" src="../../evidencias/s2-qa-035/desktop-historico-29168836.png" />

  **Mobile — Histórico de Rupturas:**

  <img width="390" height="1331" alt="smartphone-historico" src="../../evidencias/s2-qa-035/smartphone-historico-ff3363b8.png" />

### VNP-06 — Menu por perfil

- **Requisito:** RF-02, RF-03 _[confirmar no SRS]_
- **Forma de execução:** Manual
- **Nível:** Aceite
- **Pré-condição:** USR-VEN-01 e USR-LOJ-01 ativos; sessão limpa.
- **Passos:**
  1. Autenticar com cada perfil.
  2. Conferir o menu superior (Desktop) e o menu retrátil (Mobile).
- **Entrada:** navegação pelo menu.
- **Saída esperada:** Vendedor vê apenas as opções do seu perfil; Lojista vê "Histórico de rupturas", "Produtos", "Movimentações", "Saldos zerados", "Atendimentos" e "Sair", além da identificação do perfil (conforme `_Layout.cshtml` na `main` atual).
- **Resultado obtido:** menu restrito para o Vendedor e menu do Lojista exibido em Desktop e Mobile. As capturas do commit testado (`729b28f`) mostram no menu do Lojista apenas "Produtos", "Movimentações" e "Atendimentos"; os itens "Histórico de rupturas" e "Saldos zerados" existem no layout atual da `main` mas não aparecem nessas capturas — ver seção 13, item 7.
- **Status de execução:** Aprovado
- **Evidência:**

  **Desktop — menu do Vendedor:**

  <img width="1917" height="1037" alt="Consulta de Vendedor" src="../../evidencias/s2-qa-035/consulta-de-vendedor-b258a1a1.png" />

  **Desktop — menu do Lojista:**

  <img width="1897" height="1032" alt="Catálogo de Produtos" src="../../evidencias/s2-qa-035/catalogo-de-produtos-1bde8f7b.png" />

  **Mobile — menu do Vendedor:**

  <img width="422" height="908" alt="Consulta do Vendedor - Mobile" src="../../evidencias/s2-qa-035/consulta-do-vendedor-mobile-549c0849.png" />

  **Mobile — menu do Lojista:**

  <img width="421" height="911" alt="Catálogo de Produtos - Mobile" src="../../evidencias/s2-qa-035/catalogo-de-produtos-mobile-7742bbcb.png" />

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

  <img width="1897" height="1032" alt="Catálogo de Produtos" src="../../evidencias/s2-qa-035/catalogo-de-produtos-1bde8f7b.png" />

  **Mobile:**

  <img width="421" height="911" alt="Catálogo de Produtos - Mobile" src="../../evidencias/s2-qa-035/catalogo-de-produtos-mobile-7742bbcb.png" />

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
- **Resultado obtido:** formulário aberto e operável em Desktop e Mobile; segundo o executor, SKUs gerados automaticamente a partir dos tamanhos informados. As capturas anexadas mostram o formulário; captura da grade após salvar não anexada _[anexar, se houver]_.
- **Status de execução:** Aprovado
- **Evidência:**

  **Desktop:**

  <img width="1892" height="1032" alt="Cadastro de Novo Produto" src="../../evidencias/s2-qa-035/cadastro-de-novo-produto-46761018.png" />

  **Mobile:**

  <img width="426" height="912" alt="Cadastro do Novo Produto - Mobile" src="../../evidencias/s2-qa-035/cadastro-do-novo-produto-mobile-245f274a.png" />

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

  <img width="1896" height="1026" alt="Histórico de Movimentações" src="../../evidencias/s2-qa-035/historico-de-movimentacoes-dc251532.png" />

  **Mobile:**

  <img width="421" height="916" alt="Histórico de Movimentações - Mobile" src="../../evidencias/s2-qa-035/historico-de-movimentacoes-mobile-82e4a9bd.png" />

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

  <img width="1917" height="1025" alt="Registrar Nova Entrada" src="../../evidencias/s2-qa-035/registrar-nova-entrada-96775e13.png" />

  **Mobile:**

  <img width="421" height="913" alt="Registrar Nova Entrada - Mobile" src="../../evidencias/s2-qa-035/registrar-nova-entrada-mobile-456cfd2a.png" />

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

  <img width="1916" height="1032" alt="Registrar Nova Saída" src="../../evidencias/s2-qa-035/registrar-nova-saida-29e8a57b.png" />

  **Mobile:**

  <img width="415" height="902" alt="Registrar Nova Saída - Mobile" src="../../evidencias/s2-qa-035/registrar-nova-saida-mobile-79519583.png" />

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
- **Resultado obtido:** formulário exibido em Desktop e Mobile (capturas anexadas). O executor relatou que a regra de justificativa foi acionada, mas não há captura da mensagem de validação nem detalhe das entradas inválidas testadas. Pela seção 7, a regra não pode ser considerada aprovada sem essa evidência.
- **Status de execução:** Não executado
- **Evidência:**

  **Desktop:**

  <img width="1917" height="1031" alt="Ajuste Manual" src="../../evidencias/s2-qa-035/ajuste-manual-8732d023.png" />

  **Mobile:**

  <img width="425" height="910" alt="Ajuste Manual - Mobile" src="../../evidencias/s2-qa-035/ajuste-manual-mobile-3730802c.png" />

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

  <img width="1460" height="873" alt="Anônimo - Estoque Consulta" src="../../evidencias/s2-qa-035/anonimo-estoque-consulta-b2479d40.png" />

  **Desktop — `/Produtos`:**

  <img width="1483" height="882" alt="Anônimo - Produtos" src="../../evidencias/s2-qa-035/anonimo-produtos-1be82322.png" />

  **Desktop — `/Produtos/Create`:**

  <img width="1443" height="881" alt="Anônimo - ProdutosCreate" src="../../evidencias/s2-qa-035/anonimo-produtoscreate-de38db73.png" />

  **Desktop — `/Movimentacoes`:**

  <img width="1426" height="883" alt="Anônimo - Movimentações" src="../../evidencias/s2-qa-035/anonimo-movimentacoes-2c449640.png" />

  **Mobile — `/Estoque/Consulta`:**

  <img width="390" height="844" alt="Anônimo - Estoque Consulta - Mobile" src="../../evidencias/s2-qa-035/anonimo-estoque-consulta-mobile-39ca006f.png" />

  **Mobile — `/Produtos`:**

  <img width="390" height="844" alt="Anônimo - Produtos - Mobile" src="../../evidencias/s2-qa-035/anonimo-produtos-mobile-702cd5c3.png" />

  **Mobile — `/Produtos/Create`:**

  <img width="390" height="844" alt="Anônimo - ProdutosCreate - Mobile" src="../../evidencias/s2-qa-035/anonimo-produtoscreate-mobile-b1f3fb7f.png" />

  **Mobile — `/Movimentacoes`:**

  <img width="390" height="844" alt="Anônimo - Movimentações - Mobile" src="../../evidencias/s2-qa-035/anonimo-movimentacoes-mobile-7f204a5f.png" />

### VNP-14 — Lojista acessa a consulta de estoque

- **Requisito:** RF-02
- **Forma de execução:** Manual
- **Nível:** Aceite
- **Pré-condição:** USR-LOJ-01 autenticado.
- **Passos:**
  1. Acessar `/Estoque/Consulta` em Desktop e em Mobile.
- **Entrada:** acesso por URL.
- **Saída esperada:** a rota exige `VENDEDOR` (`[Authorize(Roles = "VENDEDOR")]` em `EstoqueController`), portanto o LOJISTA deve ser **negado** com destino a `/Account/AccessDenied?ReturnUrl=%2FEstoque%2FConsulta`.
- **Resultado obtido:** **não executado.** As duas capturas anexadas com o rótulo "Estoque Consulta - Lojista" (Desktop e Mobile) mostram, na barra de endereço e no cabeçalho, a URL `/Estoque/Consulta` com a sessão **`VENDEDOR`** ativa — ou seja, não comprovam o acesso do LOJISTA. Não há registro de status HTTP nem captura do destino do LOJISTA em `/Estoque/Consulta`.
- **Status de execução:** Não executado
- **Evidência:**

  Capturas anexadas abaixo são do perfil `VENDEDOR` e **não confirmam** este caso:

  **Desktop:**

  <img width="1676" height="1090" alt="Estoque Consulta - rótulo Lojista, sessão VENDEDOR" src="../../evidencias/s2-qa-035/estoque-consulta-lojista-325f723f.png" />

  **Mobile:**

  <img width="390" height="844" alt="Estoque Consulta - rótulo Lojista, sessão VENDEDOR (Mobile)" src="../../evidencias/s2-qa-035/estoque-consulta-lojista-mobile-c20b6d39.png" />

  _[anexar captura da sessão LOJISTA em `/Estoque/Consulta` mostrando o destino negado]_

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
- **Resultado obtido:** o executor relatou sessão destruída e redirecionamento ao Login, para ambos os perfis, em Desktop e Mobile. Há uma captura em Desktop; não há captura em Mobile nem registro da nova tentativa de acesso (Voltar e URL) para cada perfil. Pela seção 7, o caso permanece sem resultado até a evidência completa ser anexada.
- **Status de execução:** Não executado
- **Evidência:**

  **Desktop:**

  <img width="1497" height="642" alt="Logout" src="../../evidencias/s2-qa-035/logout-fea7a5b2.png" />

  **Mobile:** _[anexar]_

## 9. Matriz resumo

| ID | Cenário | Perfil | Requisito | Status de execução |
|---|---|---|---|---|
| VNP-01 | Login e direcionamento | Vendedor | RF-01, RF-02 | Aprovado |
| VNP-02 | Login e direcionamento | Lojista | RF-01, RF-02 | Aprovado |
| VNP-03 | Consulta, status e desfechos | Vendedor | RF-13, RF-15, RF-16 | Aprovado |
| VNP-04 | Bloqueio das rotas administrativas | Vendedor | RF-02, RN-04 | Aprovado |
| VNP-05 | Histórico de Rupturas | Lojista | RF-02, RF-22 | Aprovado |
| VNP-06 | Menu por perfil | Vendedor e Lojista | RF-02, RF-03 | Aprovado |
| VNP-07 | Catálogo e filtros | Lojista | RF-02, RF-05 a RF-07 | Aprovado |
| VNP-08 | Cadastro e grade de SKUs | Lojista | RF-05 a RF-07 | Aprovado |
| VNP-09 | Histórico de movimentações | Lojista | RF-10, RF-23 | Aprovado |
| VNP-10 | Entrada de estoque | Lojista | RF-09, RF-10 | Aprovado |
| VNP-11 | Saída de estoque | Lojista | RF-11, RN-02 | Aprovado |
| VNP-12 | Ajuste manual | Lojista | RF-23 | Não executado |
| VNP-13 | Bloqueio de anônimo | Anônimo | RF-02 | Aprovado |
| VNP-14 | Consulta pelo Lojista | Lojista | RF-02 | Não executado |
| VNP-15 | Logout | Vendedor e Lojista | RF-03 | Não executado |

**Totais:** 15 casos; 12 Aprovados; 0 Reprovados; 3 Não executados (VNP-12, VNP-14 e VNP-15).

## 10. Evidências

### 10.1 Desktop (1920 × 1080 px)

- **Login do Vendedor:**
  <img width="1917" height="1031" alt="Login do vendedor" src="../../evidencias/s2-qa-035/login-do-vendedor-1ea0fa98.png" />

- **Login do Lojista:**
  <img width="1917" height="1028" alt="Login do Lojista" src="../../evidencias/s2-qa-035/login-do-lojista-b00f2be2.png" />

- **Consulta do Vendedor:**
  <img width="1917" height="1037" alt="Consulta de Vendedor" src="../../evidencias/s2-qa-035/consulta-de-vendedor-b258a1a1.png" />

- **Histórico de Rupturas:**
  <img width="1440" height="1000" alt="desktop-historico" src="../../evidencias/s2-qa-035/desktop-historico-29168836.png" />

- **Catálogo de Produtos:**
  <img width="1897" height="1032" alt="Catálogo de Produtos" src="../../evidencias/s2-qa-035/catalogo-de-produtos-1bde8f7b.png" />

- **Cadastro de Novo Produto:**
  <img width="1892" height="1032" alt="Cadastro de Novo Produto" src="../../evidencias/s2-qa-035/cadastro-de-novo-produto-46761018.png" />

- **Histórico de Movimentações:**
  <img width="1896" height="1026" alt="Histórico de Movimentações" src="../../evidencias/s2-qa-035/historico-de-movimentacoes-dc251532.png" />

- **Registrar Nova Entrada:**
  <img width="1917" height="1025" alt="Registrar Nova Entrada" src="../../evidencias/s2-qa-035/registrar-nova-entrada-96775e13.png" />

- **Registrar Nova Saída:**
  <img width="1916" height="1032" alt="Registrar Nova Saída" src="../../evidencias/s2-qa-035/registrar-nova-saida-29e8a57b.png" />

- **Ajuste Manual:**
  <img width="1917" height="1031" alt="Ajuste Manual" src="../../evidencias/s2-qa-035/ajuste-manual-8732d023.png" />

- **Acesso Negado - Produtos:**
  <img width="1917" height="1088" alt="Acesso Negado - Produtos" src="../../evidencias/s2-qa-035/acesso-negado-produtos-e9c7a58c.png" />

- **Acesso Negado - Produtos Create:**
  <img width="1915" height="1093" alt="Acesso Negado - Produtos Create" src="../../evidencias/s2-qa-035/acesso-negado-produtos-create-5b18099d.png" />

- **Acesso Negado - Movimentações:**
  <img width="1915" height="1087" alt="Acesso Negado - Movimentações" src="../../evidencias/s2-qa-035/acesso-negado-movimentacoes-7f85b7e5.png" />

- **Acesso Negado - Movimentações Ajuste:**
  <img width="1917" height="1083" alt="Acesso Negado - Movimentações - Ajuste" src="../../evidencias/s2-qa-035/acesso-negado-movimentacoes-ajuste-9782c2e6.png" />

- **Anônimo - Estoque/Consulta:**
  <img width="1460" height="873" alt="Anônimo - Estoque Consulta" src="../../evidencias/s2-qa-035/anonimo-estoque-consulta-d0d31648.png" />

- **Anônimo - Produtos:**
  <img width="1483" height="882" alt="Anônimo - Produtos" src="../../evidencias/s2-qa-035/anonimo-produtos-4a203173.png" />

- **Anônimo - ProdutosCreate:**
  <img width="1443" height="881" alt="Anônimo - ProdutosCreate" src="../../evidencias/s2-qa-035/anonimo-produtoscreate-0daaaac3.png" />

- **Anônimo - Movimentações:**
  <img width="1426" height="883" alt="Anônimo - Movimentações" src="../../evidencias/s2-qa-035/anonimo-movimentacoes-5e3952d4.png" />

### 10.2 Mobile (viewport emulado, 375 × 812 px)

- **Login do Vendedor – Mobile:**
  <img width="435" height="908" alt="Login do Vendedor - Mobile" src="../../evidencias/s2-qa-035/login-do-vendedor-mobile-a6298e92.png" />

- **Login do Lojista – Mobile:**
  <img width="425" height="912" alt="Login do Lojista - Mobile" src="../../evidencias/s2-qa-035/login-do-lojista-mobile-3de0a4d2.png" />

- **Consulta do Vendedor – Mobile:**
  <img width="422" height="908" alt="Consulta do Vendedor - Mobile" src="../../evidencias/s2-qa-035/consulta-do-vendedor-mobile-549c0849.png" />

- **Histórico de Rupturas – Mobile:**
  <img width="390" height="1331" alt="smartphone-historico" src="../../evidencias/s2-qa-035/smartphone-historico-ff3363b8.png" />

- **Catálogo de Produtos – Mobile:**
  <img width="421" height="911" alt="Catálogo de Produtos - Mobile" src="../../evidencias/s2-qa-035/catalogo-de-produtos-mobile-7742bbcb.png" />

- **Cadastro do Novo Produto – Mobile:**
  <img width="426" height="912" alt="Cadastro do Novo Produto - Mobile" src="../../evidencias/s2-qa-035/cadastro-do-novo-produto-mobile-245f274a.png" />

- **Histórico de Movimentações – Mobile:**
  <img width="421" height="916" alt="Histórico de Movimentações - Mobile" src="../../evidencias/s2-qa-035/historico-de-movimentacoes-mobile-82e4a9bd.png" />

- **Registrar Nova Entrada – Mobile:**
  <img width="421" height="913" alt="Registrar Nova Entrada - Mobile" src="../../evidencias/s2-qa-035/registrar-nova-entrada-mobile-456cfd2a.png" />

- **Registrar Nova Saída – Mobile:**
  <img width="415" height="902" alt="Registrar Nova Saída - Mobile" src="../../evidencias/s2-qa-035/registrar-nova-saida-mobile-79519583.png" />

- **Ajuste Manual – Mobile:**
  <img width="425" height="910" alt="Ajuste Manual - Mobile" src="../../evidencias/s2-qa-035/ajuste-manual-mobile-3730802c.png" />

- **Acesso Negado - Produtos Mobile:**
  <img width="780" height="1688" alt="Acesso_Negado_-_Produtos_mobile" src="../../evidencias/s2-qa-035/acesso-negado-produtos-mobile-bc5df116.png" />

- **Acesso Negado - Produtos Create Mobile:**
  <img width="780" height="1688" alt="Acesso_Negado_-_Produtos_Create_mobile" src="../../evidencias/s2-qa-035/acesso-negado-produtos-create-mobile-10903a39.png" />

- **Acesso Negado - Movimentações Mobile:**
  <img width="780" height="1688" alt="Acesso Negado- Movimentações Mobile" src="../../evidencias/s2-qa-035/acesso-negado-movimentacoes-mobile-4e6e8018.png" />

- **Acesso Negado - Movimentações Ajuste Mobile:**
  <img width="780" height="1688" alt="Acesso Negado -Movimentações Ajuste Mobile" src="../../evidencias/s2-qa-035/acesso-negado-movimentacoes-ajuste-mobile-0a519046.png" />

- **Anônimo - Estoque/Consulta Mobile:**
  <img width="780" height="1688" alt="Anônimo_-_Estoque_Consulta_mobile" src="../../evidencias/s2-qa-035/anonimo-estoque-consulta-mobile-39ca006f.png" />

- **Anônimo - Produtos Mobile:**
  <img width="780" height="1688" alt="Anônimo_-_Produtos_mobile" src="../../evidencias/s2-qa-035/anonimo-produtos-mobile-702cd5c3.png" />

- **Anônimo - ProdutosCreate Mobile:**
  <img width="780" height="1688" alt="Anônimo_-_ProdutosCreate_mobile" src="../../evidencias/s2-qa-035/anonimo-produtoscreate-mobile-b1f3fb7f.png" />

- **Anônimo - Movimentações Mobile:**
  <img width="780" height="1688" alt="Anônimo_-_Movimentações_mobile" src="../../evidencias/s2-qa-035/anonimo-movimentacoes-mobile-7f204a5f.png" />

### 10.3 Rastreabilidade das evidências de acesso e sessão

Capturas indexadas por caso. Nenhum status HTTP foi registrado em nenhuma linha; a coluna permanece pendente.

| Caso | Perfil | Rota ou ação | Captura | Status HTTP |
|---|---|---|---|---|
| VNP-04 | Vendedor | `/Produtos` | [Captura](../../evidencias/s2-qa-035/acesso-negado-produtos-e9c7a58c.png) | Não registrado |
| VNP-04 | Vendedor | `/Produtos/Create` | [Captura](../../evidencias/s2-qa-035/acesso-negado-produtos-create-5b18099d.png) | Não registrado |
| VNP-04 | Vendedor | `/Movimentacoes` | [Captura](../../evidencias/s2-qa-035/acesso-negado-movimentacoes-7f85b7e5.png) | Não registrado |
| VNP-04 | Vendedor | `/Movimentacoes/Ajuste` | [Captura](../../evidencias/s2-qa-035/acesso-negado-movimentacoes-ajuste-9782c2e6.png) | Não registrado |
| VNP-13 | Anônimo | `/Estoque/Consulta` | [Captura](../../evidencias/s2-qa-035/anonimo-estoque-consulta-d0d31648.png) | Não registrado |
| VNP-13 | Anônimo | `/Produtos` | [Captura](../../evidencias/s2-qa-035/anonimo-produtos-4a203173.png) | Não registrado |
| VNP-13 | Anônimo | `/Produtos/Create` | [Captura](../../evidencias/s2-qa-035/anonimo-produtoscreate-0daaaac3.png) | Não registrado |
| VNP-13 | Anônimo | `/Movimentacoes` | [Captura](../../evidencias/s2-qa-035/anonimo-movimentacoes-5e3952d4.png) | Não registrado |
| VNP-14 | Lojista (captura mostra sessão VENDEDOR) | `/Estoque/Consulta` | [Captura](../../evidencias/s2-qa-035/estoque-consulta-lojista-325f723f.png) | Não registrado |
| VNP-15 | Vendedor e Lojista | `/Account/Logout` | [Captura](../../evidencias/s2-qa-035/logout-fea7a5b2.png) | Não registrado |

## 11. Cobertura de cenários

| Categoria (plano, seção 8.4) | Cobertura neste relatório |
|---|---|
| Positivos | VNP-01 a VNP-03 e VNP-05 a VNP-11 aprovados; VNP-12 não executado por falta de evidência da validação |
| Permissão e segurança | VNP-04, VNP-06 e VNP-13 aprovados com capturas e sem status HTTP; VNP-14 não executado (evidência mostra perfil errado); VNP-15 não executado (evidência incompleta) |
| Responsividade e compatibilidade | Todos os casos em D e M; compatibilidade limitada a Chrome e Edge |
| Negativos e limites | Cobertos por CT-AUT-04 a CT-AUT-06, UT-05 e UT-11. Não exercitados neste relatório; a justificativa do ajuste (VNP-12) segue sem evidência |
| Desempenho e concorrência | Não aplicável a esta validação de navegação |

## 12. Limitações

- O Mobile foi validado em viewport emulado (Chrome DevTools), não em dispositivo físico. Toque, teclado virtual e renderização nativa não foram cobertos.
- Foram cobertos apenas Chrome e Edge (v118.0), em Windows 11 Pro e macOS Ventura.
- Apenas duas resoluções foram testadas. As dimensões das capturas (Mobile entre 390 e 435 px de largura; Desktop do Histórico de Rupturas e do acesso anônimo abaixo de 1920 px) não coincidem exatamente com as resoluções declaradas na seção 5.
- O status HTTP não foi registrado nos casos VNP-04, VNP-13 e VNP-14, que se apoiam apenas em capturas e no relato do executor.
- VNP-12 (mensagem de validação), VNP-14 (acesso do LOJISTA a `/Estoque/Consulta`) e VNP-15 (logout em Mobile e nova tentativa de acesso) não têm evidência completa e permanecem **Não executado**.
- A validação foi executada contra o App Service em produção (seção 5), não em ambiente local; não há controle da massa de dados nem da disponibilidade do ambiente.
- Os fluxos de envio de entrada, saída e ajuste não tiveram o saldo final conferido neste relatório.
- O Histórico de Rupturas (VNP-05) não teve a contagem de ocorrências conferida contra os registros.
- A validação é manual, sem automação.

## 13. Divergências e itens a confirmar

1. **Rotas:** resolvido. O relato original usava `/login`, `/consulta`, `/produtos`, `/produtos/novo` e `/movimentacoes/entrada`; este relatório usa os caminhos MVC conferidos no código (`/Account/Login`, `/Estoque/Consulta`, `/Produtos/Create`, `/Movimentacoes/Entrada`) e a rota do Histórico de Rupturas é `/Rupturas` (seção 6).
2. **Destino pós-login:** resolvido no código. O plano informa que a implementação atual redireciona ambos os perfis para `/`, mas `AccountController.Login` direciona `VENDEDOR` para `/Estoque/Consulta` e `LOJISTA` para `/Produtos` — comportamento registrado em VNP-01 e VNP-02. O plano está desatualizado neste ponto.
3. **Vendedor em rota proibida:** resolvido. As capturas mostram URL final `/Account/AccessDenied?ReturnUrl=…` com a página "Acesso Não Autorizado", ou seja, o comportamento corresponde ao previsto no plano (CT-AUT-13) e não ao "redirecionamento para Login" da versão anterior deste relatório. O status HTTP em si não foi registrado na aba Network.
4. **Justificativa do ajuste:** o mínimo de cinco caracteres consta no relato da atividade e não aparece no plano. Confirmar no SRS ou no código.
5. **Requisitos:** conferir no SRS os RF e UC do catálogo (VNP-07), do histórico de movimentações (VNP-09), do Histórico de Rupturas (VNP-05, RF-22 e UC-12) e do menu (VNP-06, RF-02 e RF-03).
6. **Data e commit:** o cabeçalho registra 12/10/2026 e o commit `729b28f`; o registro anterior do PR traz `20/10/2026`. Confirmar qual data de execução é a correta.
7. **Menu do Lojista:** as capturas do commit testado (`729b28f`) mostram apenas "Produtos", "Movimentações" e "Atendimentos"; `_Layout.cshtml` da `main` atual inclui ainda "Histórico de rupturas" e "Saldos zerados". Confirmar se a validação deve ser reexecutada sobre a `main` atual.
8. **Ambiente:** o PR declarava "ambiente local com SQLite", mas as capturas mostram o endereço `asp-squad-estoque-free-hgabd8asd5f4a9g9.northcentralus-01.azurewebsites.net`. A seção 5 foi corrigida para o App Service; confirmar se a intenção era validar em ambiente local.
9. **Viewports:** as capturas Mobile têm 390 × 844 px, 780 × 1688 px (2x de 390 × 844) ou entre 415 e 435 px de largura, e a do Histórico de Rupturas tem 390 × 1331 px (captura de página inteira), contra os 375 × 812 px declarados. Confirmar a viewport realmente usada ou ajustar a seção 5.
10. **Evidências duplicadas e anexos removidos:** as capturas de acesso negado e de anônimo aparecem com identificadores diferentes na seção 8 e na seção 10. Dois anexos originais do PR (`c0542fa6-…` e `82e4a9bd-…0d8e8ae8caf0`) retornam **404** no GitHub e foram substituídos, nesta versão, pelas capturas equivalentes já versionadas em `docs/09-testing/evidencias/s2-qa-035/`. Confirmar se são o mesmo material.
11. **VNP-08:** confirmar se existe captura da grade de SKUs após salvar; as capturas anexadas mostram o formulário.
12. **Status alterados:** VNP-12 e VNP-15 estavam como Aprovado sem evidência completa e foram para **Não executado**, conforme a seção 7. VNP-14 também foi para **Não executado** porque suas capturas mostram o perfil `VENDEDOR`, e não o `LOJISTA`. Se as evidências faltantes forem anexadas, voltam a Aprovado.
