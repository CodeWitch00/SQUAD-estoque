# S2-DOC-038 — Evidências da P1 (Protótipos, Plano de Testes e Índice de Evidências)

## 1. Identificação do Card

| Campo | Conteúdo |
|---|---|
| **Card** | S2-DOC-038 |
| **Título** | Consolidar evidências da P1 (protótipos, plano de testes e índice de evidências) |
| **Responsável** | Integrante de documentação |
| **Repositório** | [CodeWitch00/SQUAD-estoque](https://github.com/CodeWitch00/SQUAD-estoque) |
| **Documento versionado** | [`docs/05-ux/S2-DOC-038-evidencias-p1.md`](S2-DOC-038-evidencias-p1.md) |
| **Período formal da Sprint 2 / P1** | 10/09/2026 a 07/10/2026 ([relatório-sprint-2](../10-planejamento/relatorio-sprint-2.md)) |
| **Estado** | Reentregue no PR [#82](https://github.com/CodeWitch00/SQUAD-estoque/pull/82), na branch original, com as correções pedidas nas revisões dos PRs #74 e #78 — revisão e conferência final registradas na Seção 11 |

---

## 2. Escopo e Índice do Pacote P1

Este documento consolida o pacote de evidências da P1 em um único índice versionado. Ele **não** reproduz nem reescreve a monografia nem o plano de testes oficiais: apenas aponta para eles e registra os casos de teste executados, com evidências.

| # | Seção | Artefato |
|---|---|---|
| 1 | Identificação do card | esta seção |
| 2 | Escopo e índice do pacote | esta seção |
| 3 | Inventário de telas MVP × o que existe no código | Seção 3 |
| 4 | Cobertura de testes com os IDs oficiais do plano (`CT-AUT`/`IT`/`E2E`/`NFT`/`UT`) | [`docs/09-testing/plano-de-testes.md`](../09-testing/plano-de-testes.md) |
| 5 | Validação de responsividade | [`docs/09-testing/homologacoes/registro-validacoes-manuais.md`](../09-testing/homologacoes/registro-validacoes-manuais.md) |
| 6 | Índice de capturas de tela (protótipos × implementação) | Seção 6 |
| 7 | Testes automatizados e execução na CI | Seção 7 |
| 8 | Rastreabilidade com os PRs entregues | Seção 8 |
| 9 | Itens pendentes e falhas conhecidas | Seção 9 |
| 10 | Checklist de integridade do pacote | Seção 10 |
| 11 | Conferência final da documentação | Seção 11 |

**Artefatos de referência (normativos deste pacote):**

- Inventário de telas e mapa de navegação: [`docs/05-ux/inventario-telas-e-mapa-navegacao.md`](inventario-telas-e-mapa-navegacao.md)
- Plano de testes: [`docs/09-testing/plano-de-testes.md`](../09-testing/plano-de-testes.md)
- Registro de validações manuais: [`docs/09-testing/homologacoes/registro-validacoes-manuais.md`](../09-testing/homologacoes/registro-validacoes-manuais.md)
- Critérios de avaliação do professor (P1): [`docs/10-planejamento/avaliacao-professor.md`](../10-planejamento/avaliacao-professor.md)
- Índice de UX: [`docs/05-ux/README.md`](README.md)

---

## 3. Inventário de Telas MVP × Implementação Real

> **Regra adotada nesta versão:** os códigos (`AUT`, `VEN`, `LOJ`, `SUP`) são exclusivamente os de [`inventario-telas-e-mapa-navegacao.md`](inventario-telas-e-mapa-navegacao.md) e **mantêm o significado oficial do inventário**. A versão anterior deste documento reutilizou códigos com outros significados (por exemplo, `VEN-02` como "venda rápida", quando no inventário `VEN-02` é "Consulta de produtos") — isso foi corrigido aqui. Telas sem código no inventário oficial são listadas na Seção 3.5 com o rótulo "sem código oficial"; nenhum código oficial é reaproveitado com outro sentido. Toda tela é classificada com base na existência da view no repositório em `main` e na data do commit que a criou ([`git log --diff-filter=A`](https://github.com/CodeWitch00/SQUAD-estoque/commits/main)).

### 3.1 Legenda de situação

| Situação | Significado |
|---|---|
| ✅ **Implementada no período** | View criada entre 10/09/2026 e 07/10/2026 (período formal) |
| 🕐 **Implementada após o período formal** | View existe no código, mas foi criada **depois de 07/10/2026** |
| ⚠️ **Existente com divergência** | Funcionalidade existe, mas sua autorização ou forma difere do inventário normativo |
| ⬜ **Planejada / não implementada** | Não existe view correspondente no repositório |

### 3.2 Fluxo de Autenticação (`AUT` — códigos oficiais)

| Código | Tela (significado oficial) | Rota real | Situação | Evidência |
|---|---|---|---|---|
| AUT-01 | Login | [`/Account/Login`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Account/Login.cshtml) | ✅ Implementada no período | `Views/Account/Login.cshtml`; testes [`AuthenticationAuthorizationTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs) (criado 17/08) |

**Observações verificadas no código:**

- O `AccountController` possui `[Authorize]`/`AllowAnonymous` e o login redireciona por perfil ([`AccountController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/AccountController.cs)):
  - `VENDEDOR` → `/Estoque/Consulta`
  - `LOJISTA` → `/Produtos`
- A opção **"Lembrar de mim"** existe no formulário de login (`Login.cshtml`).
- O inventário oficial não possui código para a tela de recuperação de senha; ela é listada como pendência na Seção 3.5.

### 3.3 Fluxo do Vendedor (`VEN-01` a `VEN-06` — significados oficiais do inventário)

| Código | Tela (significado oficial) | Rota/ação real | Autorização real | Situação | Evidência |
|---|---|---|---|---|---|
| VEN-01 | Início do vendedor | Sem tela própria; o login redireciona `VENDEDOR` para `/Estoque/Consulta` | `VENDEDOR` | ⚠️ Atendida indiretamente pelo redirecionamento pós-login (o inventário prevê entrada do perfil já com acesso à consulta) | [`AccountController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/AccountController.cs) |
| VEN-02 | Consulta de produtos | `/Estoque/Consulta` | `VENDEDOR` | ✅ Implementada no período | [`EstoqueController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/EstoqueController.cs), [`Views/Estoque/Consulta.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Estoque/Consulta.cshtml); testes [`ConsultaEstoqueTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/ConsultaEstoqueTests.cs) |
| VEN-03 | Grade consultada | Grade (numerações, saldos e estados) exibida na própria `/Estoque/Consulta` | `VENDEDOR` | ✅ Implementada no período | [`Views/Estoque/Consulta.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Estoque/Consulta.cshtml) |
| VEN-04 | Resultado "Vendeu" | Ação da grade em `/Estoque/Consulta`; baixa registrada via `/Movimentacoes/Saida` | `LOJISTA,VENDEDOR` | ⚠️ **Divergência:** a saída genérica é compartilhada com o lojista (ver LOJ-07 e Seção 3.6, item 2) | [`MovimentacoesController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/MovimentacoesController.cs); testes [`VendaRapidaHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/VendaRapidaHttpTests.cs) (22/09, no período) |
| VEN-05 | Resultado "Não tinha" | Ação da grade em `/Estoque/Consulta`; ruptura persistida pelo fluxo da consulta | `VENDEDOR` (ação da grade) | ✅ Implementada no período | [`RupturaHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/RupturaHttpTests.cs) (30/09, no período), [`consulta-nao-tinha.test.cjs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/frontend/consulta-nao-tinha.test.cjs) (28/09, no período) |
| VEN-06 | Resultado "Desistiu" | Ação da grade em `/Estoque/Consulta`; sem persistência de movimentação | `VENDEDOR` | ✅ Implementada no período | [`DesistiuHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/DesistiuHttpTests.cs) (01/10, no período), [`consulta-desistiu.test.cjs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/frontend/consulta-desistiu.test.cjs) (02/10, no período) |

> **Nota sobre o histórico de rupturas:** a rota `/Rupturas` é a **LOJ-11** (Histórico de rupturas, Seção 3.4), é **exclusiva do `LOJISTA`** e foi criada em **20/10/2026** — após o período formal. A versão anterior deste documento a classificava como `VEN-03` acessível ao vendedor e como entrega no período; ambos os pontos estão corrigidos.

### 3.4 Fluxo do Lojista (`LOJ-01` a `LOJ-11` — significados oficiais do inventário)

| Código | Tela (significado oficial) | Rota real | Autorização real | Situação | Evidência |
|---|---|---|---|---|---|
| LOJ-01 | Início do lojista | Sem tela própria; o login redireciona `LOJISTA` para `/Produtos` | `LOJISTA` | ⚠️ Atendida indiretamente pelo redirecionamento pós-login | [`AccountController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/AccountController.cs) |
| LOJ-02 | Produtos (catálogo) | `/Produtos` | `LOJISTA` | ✅ Implementada no período | [`ProdutosController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/ProdutosController.cs), [`Views/Produtos/Index.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Produtos/Index.cshtml) |
| LOJ-03 | Novo produto e grade | `/Produtos/Create` (+ grade em `/Produtos/AdicionarNumeracoes`) | `LOJISTA` | ✅ Implementada no período (view da grade criada 03/10/2026) | [`Views/Produtos/Create.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Produtos/Create.cshtml), [`Views/Produtos/AdicionarNumeracoes.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Produtos/AdicionarNumeracoes.cshtml) |
| LOJ-04 | Detalhe do produto/grade | `/Produtos/Details/{id}` | `LOJISTA` | ✅ Implementada no período | [`Views/Produtos/Details.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Produtos/Details.cshtml) |
| LOJ-05 | Editar produto | `/Produtos/Edit/{id}` | `LOJISTA` | ✅ Implementada no período | [`Views/Produtos/Edit.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Produtos/Edit.cshtml) |
| LOJ-06 | Entrada de estoque | `/Movimentacoes/Entrada` (e `/Movimentacoes/EntradaLote`) | `LOJISTA` | ✅ Implementada no período | [`MovimentacoesController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/MovimentacoesController.cs) |
| LOJ-07 | Saída de estoque | `/Movimentacoes/Saida` | `LOJISTA,VENDEDOR` | ⚠️ **Existente com divergência:** o inventário reserva a saída administrativa ao `LOJISTA`; a rota também aceita `VENDEDOR` (ver Seção 3.6, item 2) | [`MovimentacoesController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/MovimentacoesController.cs) |
| LOJ-08 | Ajuste manual | `/Movimentacoes/Ajuste` | `LOJISTA` | ✅ Implementada no período | [`MovimentacoesController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/MovimentacoesController.cs) |
| LOJ-09 | Histórico de movimentações | `/Movimentacoes` | `LOJISTA` | ✅ Implementada no período | [`MovimentacoesController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/MovimentacoesController.cs) |
| LOJ-10 | Saldos zerados | `/SaldosZerados` | `LOJISTA` | 🕐 Implementada após o período formal (view `SaldosZerados/Index.cshtml` criada em 16/10/2026, commit `600bbb2`) | [`SaldosZeradosController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/SaldosZeradosController.cs) |
| LOJ-11 | Histórico de rupturas | `/Rupturas` | `LOJISTA` | 🕐 Implementada após o período formal (view `Rupturas/Index.cshtml` criada em **20/10/2026**, commit `381224d`) | [`RupturasController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/RupturasController.cs) |

### 3.5 Telas de apoio (códigos oficiais) e telas sem código no inventário

| Rota | Código oficial | Autorização real | Situação | Evidência |
|---|---|---|---|---|
| `/Account/AccessDenied` | SUP-01 (apoio técnico) | Público (retorno seguro) | ✅ Existente | [`Views/Account/AccessDenied.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Account/AccessDenied.cshtml) |
| `/Home/Error` | SUP-02 (apoio técnico) | Público | ✅ Existente | [`Views/Home/Error.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Home/Error.cshtml) |
| `/Atendimentos` | **sem código no inventário oficial** | `LOJISTA` | 🕐 Implementada após o período formal (view `Atendimentos/Index.cshtml` criada em 11/10/2026, commit `bd1cdff`) | [`AtendimentosController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/AtendimentosController.cs) |
| `/EntradasFiscais/Importar` | **sem código no inventário oficial** | `LOJISTA` | 🕐 Implementada após o período formal (view criada em 11/10/2026, commit `bd1cdff`) | [`EntradasFiscaisController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/EntradasFiscaisController.cs) |
| Recuperação de senha | **sem código no inventário oficial** | — | ⬜ Planejada / não implementada (não há `RecuperarSenha.cshtml` em `Views/Account`) | `Views/Account/` |

### 3.6 Divergências do inventário registradas por este card

1. **Códigos `SUP`:** no inventário oficial, `SUP-01` e `SUP-02` são telas de apoio (`/Account/AccessDenied` e `/Home/Error`), **não** um fluxo de supervisor. Não existe papel `SUPERVISOR`/`SUP` em nenhum `[Authorize]` da aplicação — todos os controllers exigem apenas `LOJISTA` e/ou `VENDEDOR`. A tela `/Atendimentos`, citada como `SUP-01` na versão anterior deste documento, **não tem código no inventário** e é **exclusiva do `LOJISTA`**.
2. **`/Movimentacoes/Saida` não é exclusiva do lojista:** a rota aceita `LOJISTA,VENDEDOR`, embora o inventário reserve a saída administrativa (LOJ-07) ao lojista e dê ao vendedor apenas "Vendeu" (VEN-04).
3. **`/Rupturas` e `/Atendimentos` são exclusivos do `LOJISTA`:** os controllers [`RupturasController`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/RupturasController.cs) e [`AtendimentosController`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/AtendimentosController.cs) usam `[Authorize(Roles = "LOJISTA")]` na `main`. A versão anterior deste documento os listava como acessíveis ao `VENDEDOR` (itens VEN-03/VEN-04 antigos) — ponto corrigido nesta revisão, a pedido da revisão do PR #74.
4. **Telas pós-período formal:** `Atendimentos/Index` e `EntradasFiscais/Importar` (11/10), `SaldosZerados/Index` (16/10) e `Rupturas/Index` (20/10, commit `381224d`) entraram depois de 07/10/2026 e são classificadas como 🕐 para não confundir entrega formal com evolução posterior. Em particular, **`/Rupturas` foi classificada como ✅ no período na versão anterior, o que está corrigido** — a view foi criada em 20/10/2026.

---

## 4. Cobertura de Testes do Pacote P1 (IDs oficiais do plano)

> Fonte normativa: [`docs/09-testing/plano-de-testes.md`](../09-testing/plano-de-testes.md), cujo catálogo usa identificadores como `CT-AUT-01`, `IT-07`, `E2E-01`, `NFT-07` e `UT-11`. A versão anterior deste documento apresentava os casos como `CT-001` a `CT-015`, que **não existem no catálogo oficial** — a revisão do PR #74 apontou essa divergência. Esta versão usa **diretamente os IDs oficiais**; a correspondência com os IDs anteriores está registrada na Seção 4.1 para rastreabilidade. Cada caso abaixo foi **executado** e registra entrada, resultado esperado, resultado obtido e evidência localizável no repositório.

### 4.1 Correspondência com os IDs da versão anterior (rastreabilidade)

| ID anterior (não oficial) | ID(s) oficial(is) do plano | Observação |
|---|---|---|
| CT-001 | `CT-AUT-03` | Login do vendedor com credenciais válidas (usuário corrigido para `vendedor@squad.com`, conforme o teste referenciado) |
| CT-002 | `CT-AUT-04` e `CT-AUT-05` | Login com credenciais inválidas |
| CT-003 | `CT-AUT-09` e `CT-AUT-10` | Anônimo em rota protegida |
| CT-004 | `CT-AUT-11` a `CT-AUT-15` | Autorização por perfil |
| CT-005 | `IT-19` | Busca parcial com resultado |
| CT-006 | `IT-20` | Busca sem resultado |
| CT-007 | `IT-24` e `IT-25` | Resultado `Vendeu` e venda sem saldo |
| CT-008 | `IT-27` | Ruptura sem SKU inválido |
| CT-009 | `IT-28` e `IT-29` | Resultado `Desistiu` e nova consulta |
| CT-010 | *sem ID oficial* | Importação de NF-e não consta do catálogo do plano (o SRS trata NF-e como fora do escopo do MVP) — ver Seção 4.2 |
| CT-011 | `IT-07` (e `IT-08`) | Entrada válida (a modalidade em lote não tem ID dedicado no catálogo) |
| CT-012 | `IT-15`, `IT-16` e `UT-11` | Ajuste válido, sem motivo e validação do motivo |
| CT-013 | `NFT-07` (e `NFT-01`) | Responsividade/viewport |
| CT-014 | `NFT-01` a `NFT-07` | Não-funcionais (parcial) |
| CT-015 | `IT-30` a `IT-33` (e `E2E-07`) | Saldos zerados e histórico de rupturas |

### 4.2 Tabela-resumo (IDs oficiais)

| Caso(s) oficial(is) | Cenário | Status |
|---|---|---|
| `CT-AUT-02` e `CT-AUT-03` | Login com credenciais válidas (lojista e vendedor) | ✅ Aprovado |
| `CT-AUT-04` e `CT-AUT-05` | Login com credenciais inválidas | ✅ Aprovado |
| `CT-AUT-09` e `CT-AUT-10` | Anônimo em rotas protegidas | ✅ Aprovado |
| `CT-AUT-11` a `CT-AUT-15` | Autorização por perfil | ✅ Aprovado |
| `IT-19` e `IT-20` | Consulta de estoque com e sem resultado | ✅ Aprovado |
| `IT-24` e `IT-25` | Resultado `Vendeu` e venda sem saldo | ✅ Aprovado |
| `IT-26` e `IT-27` | Ruptura (`Não tinha`) e ruptura com SKU inválido | ✅ Aprovado |
| `IT-28` e `IT-29` | Resultado `Desistiu` e nova consulta | ✅ Aprovado |
| `IT-07` e `IT-08` | Entrada válida e inválida | ✅ Aprovado |
| `IT-15`, `IT-16`, `UT-11` | Ajuste de saldo e validação do motivo | ✅ Aprovado (cobertura do motivo ampliada após o período formal, Seção 8) |
| `NFT-07` | Responsividade (viewport 390×844) | ✅ Aprovado (capturas no período; teste automatizado de 09/10, pós-formal) |
| `NFT-01` a `NFT-07` | Não-funcionais (desempenho/acessibilidade) | ⚠️ Parcial |
| `E2E-03` | Jornada de consulta e venda | ⚠️ Parcial (aprovado em navegador local; bloqueado no ambiente publicado, `DEF-VEN-MOB-01`) |
| `IT-30` a `IT-33`, `E2E-07` | Saldos zerados e histórico de rupturas | 🕐 Executados só após o período formal (13–20/10) |
| *sem ID oficial* | Importação de NF-e (`/EntradasFiscais/Importar`) | 🕐 Executada só após o período formal (11/10); fora do catálogo do plano e do escopo do SRS |

### 4.3 Casos detalhados

#### `CT-AUT-02` — Login do lojista com credenciais válidas

- **Entrada:** usuário `lojista@squad.com` / senha `123` (seed de testes), checkbox "Lembrar de mim" desmarcado.
- **Esperado:** autenticação bem-sucedida, cookie autenticado como `LOJISTA` e redirecionamento para `/Produtos`.
- **Obtido:** redirect 302 para `/Produtos`; cookie de autenticação emitido.
- **Evidência:** [`AuthenticationAuthorizationTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs) (criado 17/08/2026).

#### `CT-AUT-03` — Login do vendedor com credenciais válidas

- **Entrada:** usuário `vendedor@squad.com` / senha `123` (seed de testes) — *correção: a versão anterior citava `vendedor@teste.com`, que não existe no teste referenciado*.
- **Esperado:** autenticação como `VENDEDOR` e redirecionamento conforme perfil (`VENDEDOR` → `/Estoque/Consulta`).
- **Obtido:** redirect 302 para `/Estoque/Consulta`; cookie de autenticação emitido.
- **Evidência:** [`AuthenticationAuthorizationTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs) (criado 17/08/2026).

#### `CT-AUT-04` e `CT-AUT-05` — Login com credenciais inválidas

- **Entrada:** usuário válido com senha incorreta (`CT-AUT-05`); e-mail inexistente com qualquer senha (`CT-AUT-04`).
- **Esperado:** permanência na tela de login com mensagem genérica; nenhum cookie de autenticação.
- **Obtido:** resposta 200 na página de login com erro de validação; sem cookie de sessão.
- **Evidência:** [`AuthenticationAuthorizationTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs).

#### `CT-AUT-09` e `CT-AUT-10` — Anônimo em rota protegida

- **Entrada:** `GET /Produtos` (`CT-AUT-09`), `GET /Movimentacoes` (`CT-AUT-10`) e `GET /Estoque/Consulta` sem cookie de autenticação.
- **Esperado:** redirecionamento 302 para `/Account/Login?ReturnUrl=...`.
- **Obtido:** 302 para `/Account/Login` em todas as rotas testadas.
- **Evidência:** [`AuthenticationAuthorizationTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs).

#### `CT-AUT-11` a `CT-AUT-15` — Autorização por perfil

- **Entrada:** sessão de `VENDEDOR` acessando `/Produtos` (`CT-AUT-13`); sessão de `LOJISTA` acessando rotas de vendedor; vendedor enviando `GET`/`POST` em `/Movimentacoes/Ajuste` (`CT-AUT-14`/`CT-AUT-15`).
- **Esperado:** negação conforme `[Authorize(Roles=...)]`; lojista com `200` em Produtos, Movimentações e Ajuste (`CT-AUT-11`).
- **Obtido:** acessos negados conforme atributos dos controllers; acesso permitido em `/Movimentacoes/Saida` para ambos (divergência LOJ-07/VEN-04 registrada na Seção 3.6, item 2).
- **Evidência:** [`AuthenticationAuthorizationTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs) + [`AdministrativeHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AdministrativeHttpTests.cs).

#### `IT-19` e `IT-20` — Consulta de estoque com e sem resultado

- **Entrada:** termo de busca correspondente a SKU cadastrado no seed (`IT-19`); termo `SKU-INEXISTENTE-999` (`IT-20`).
- **Esperado:** `200` com o item correspondente; `200` com estado vazio/mensagem de nenhum resultado.
- **Obtido:** 200 com o produto listado; 200 sem resultados e sem erro de servidor.
- **Evidência:** [`ConsultaEstoqueTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/ConsultaEstoqueTests.cs) + captura desktop Seção 6.

#### `IT-24` e `IT-25` — Resultado `Vendeu` e venda sem saldo

- **Entrada:** SKU com saldo ≥ 1, quantidade 1, sessão `VENDEDOR` (`IT-24`); SKU com saldo 0 (`IT-25`).
- **Esperado:** baixa de exatamente 1 no saldo com movimentação registrada; rejeição sem movimentação quando o saldo é zero.
- **Obtido:** saída registrada e saldo atualizado; venda com saldo zero rejeitada sem movimentação.
- **Evidência:** [`VendaRapidaHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/VendaRapidaHttpTests.cs) (criado 22/09/2026, PR #31).

#### `IT-26` e `IT-27` — Ruptura (`Não tinha`) e ruptura com SKU inválido

- **Entrada:** SKU válido no fluxo `Não tinha` (`IT-26`); SKU inexistente no formulário de ruptura (`IT-27`).
- **Esperado:** ruptura persistida com saldo inalterado; rejeição e nenhuma persistência para SKU inválido.
- **Obtido:** ruptura registrada sem alteração de estoque; ruptura com SKU inválido rejeitada.
- **Evidência:** [`RupturaHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/RupturaHttpTests.cs) (30/09/2026, no período) + [`RupturaInvalidSkuHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/RupturaInvalidSkuHttpTests.cs) (criado 01/10/2026, no período formal).

#### `IT-28` e `IT-29` — Resultado `Desistiu` e nova consulta

- **Entrada:** atendimento existente com motivo de desistência (`IT-28`); nova consulta iniciada após desistir (`IT-29`).
- **Esperado:** registro da desistência sem movimentação/ruptura e retorno à busca; nova consulta permitida sem efeito no estoque.
- **Obtido:** desistência registrada conforme esperado e nova consulta desbloqueada.
- **Evidência:** [`DesistiuHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/DesistiuHttpTests.cs) (criado 01/10/2026).

#### `IT-07` e `IT-08` — Entrada válida e inválida

- **Entrada:** lista de itens com SKU e quantidade, sessão `LOJISTA` (`IT-07`; a modalidade em lote usa `/Movimentacoes/EntradaLote` e não tem ID dedicado no catálogo); quantidade 0 ou negativa (`IT-08`).
- **Esperado:** sucesso com aumento de saldo e movimentação registrada; rejeição sem alteração para quantidade inválida.
- **Obtido:** entradas persistidas e saldo atualizado; quantidades inválidas rejeitadas.
- **Evidência:** [`AdministrativeHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AdministrativeHttpTests.cs) + [`ConsultaOperacionalHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/ConsultaOperacionalHttpTests.cs).

#### `IT-15`, `IT-16` e `UT-11` — Ajuste de saldo e validação do motivo

- **Entrada:** SKU com saldo existente e ajuste informado, sessão `LOJISTA` (`IT-15`); ajuste sem motivo (`IT-16`); motivo vazio, só espaços e texto válido (`UT-11`).
- **Esperado:** saldo ajustado com movimentação `AJUSTE` rastreável; rejeição quando o motivo é ausente/inválido.
- **Obtido:** ajuste aplicado conforme esperado; motivos vazios rejeitados.
- **Evidência:** [`AdministrativeHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AdministrativeHttpTests.cs) + (pós-formal) [`AjusteMotivoHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AjusteMotivoHttpTests.cs), criado **22/10/2026** — casos `Motivo_invalido_rejeita_ajuste_sem_alterar_saldos_ou_historico` e `Motivo_valido_persiste_normalizado_com_diferenca_usuario_e_data`. Cobertura ampliada pelo PR [#71](https://github.com/CodeWitch00/SQUAD-estoque/pull/71) (merge em 27/10/2026 — ver Seção 8).

#### `NFT-07` — Responsividade (viewport 390×844)

- **Entrada:** viewport 390×844 (padrão do registro de homologação) nas telas de consulta e venda.
- **Esperado:** layout sem overflow horizontal, elementos acessíveis ao toque.
- **Obtido:** aprovado nas capturas mobile (Seção 5).
- **Evidência:** [`tests/frontend/consulta-responsividade.test.cjs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/frontend/consulta-responsividade.test.cjs) (criado 09/10/2026, pós-formal) + capturas mobile da Seção 5.

#### `NFT-01` a `NFT-07` — Não-funcionais (desempenho/acessibilidade)

- **Entrada:** critérios de tempo de resposta, headers, usabilidade e acessibilidade do [plano de testes](../09-testing/plano-de-testes.md).
- **Esperado:** evidência automatizada de responsividade + registro manual de validações.
- **Obtido:** **parcial** — não existe `especificacao-testes-nao-funcionais.md` no repositório e não há medições de desempenho (NFT-02) nem testes de usabilidade (NFT-06). A evidência disponível é o teste automatizado de responsividade e o registro manual.
- **Evidência:** [`tests/frontend/consulta-responsividade.test.cjs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/frontend/consulta-responsividade.test.cjs) + [`registro-validacoes-manuais.md`](../09-testing/homologacoes/registro-validacoes-manuais.md). **Pendência registrada na Seção 9.**

#### `E2E-03` — Jornada de consulta e venda

- **Entrada:** vendedor busca PRD-01, escolhe SKU e marca `Vendeu` (jornada do plano).
- **Esperado:** grade aparece, saldo reduz em 1 e confirmação visível.
- **Obtido:** **parcial** — aprovado em navegador local; bloqueado no ambiente publicado conforme `DEF-VEN-MOB-01` (estado registrado no próprio plano).
- **Evidência:** situação mantida conforme [`plano-de-testes.md`](../09-testing/plano-de-testes.md), Seção 11.

#### `IT-30` a `IT-33` e `E2E-07` — Saldos zerados e histórico de rupturas

- **Entrada:** produtos com saldo exatamente zero, agrupados por produto/SKU (`IT-30`); sessão de `VENDEDOR`/anônimo tentando acessar as visões (`IT-31`, `IT-33`); rupturas repetidas para diferentes SKUs (`IT-32`).
- **Esperado:** listagem apenas de saldos zero, agrupamento e frequências corretos, e acesso negado a quem não é `LOJISTA`.
- **Obtido:** aprovado **apenas após o período formal** — não havia cobertura em 07/10/2026 (ViewModel criado 13/10, autorização testada 16/10, histórico de rupturas 20/10).
- **Evidência:** [`SaldosZeradosViewModelTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/SaldosZeradosViewModelTests.cs) (criado **13/10/2026**), [`SaldosZeradosAuthorizationHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/SaldosZeradosAuthorizationHttpTests.cs) (criado **16/10/2026**) e [`RupturasHistoricoHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/RupturasHistoricoHttpTests.cs) (criado **20/10/2026**) — todos **após o período formal**.

#### Importação de NF-e — *sem ID oficial no catálogo do plano*

- **Entrada:** arquivo XML de NF-e válido e tentativa de importação por perfil que não é `LOJISTA`.
- **Esperado:** rascunho criado e confirmação cadastrando produto/SKU/movimento/vínculo; acesso negado a quem não é `LOJISTA`.
- **Obtido:** aprovado **apenas após o período formal** — não havia cobertura em 07/10/2026. A tela `/EntradasFiscais/Importar` foi criada em 11/10/2026 e os testes no mesmo dia. **Ressalva de rastreabilidade:** este cenário não possui ID no catálogo oficial do plano de testes (a importação de NF-e é tratada como fora do escopo do MVP no SRS); ele é registrado aqui como cobertura adicional, sem correspondência obrigatória.
- **Evidência:** [`EntradaFiscalHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/EntradaFiscalHttpTests.cs) (criado **11/10/2026**, **após o período formal**) + [`Views/EntradasFiscais/Importar.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/EntradasFiscais/Importar.cshtml) (criada 11/10/2026). Registro manual de 09/10/2026 em [`registro-validacoes-manuais.md`](../09-testing/homologacoes/registro-validacoes-manuais.md) também marcado como posterior ao período.

---

## 5. Validação de Responsividade

Fonte: [`docs/09-testing/homologacoes/registro-validacoes-manuais.md`](../09-testing/homologacoes/registro-validacoes-manuais.md).

- **Viewport padrão mobile adotado:** **390×844** (registro de homologação; padronizado neste documento — versões anteriores citavam também 375×812, o que foi corrigido).
- **Execução automatizada:** [`tests/frontend/consulta-responsividade.test.cjs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/frontend/consulta-responsividade.test.cjs) (criado 09/10/2026, **após o período formal**), executado via `npm run test:frontend`.
- **Capturas mobile (execução manual):**

| Tela | Dispositivo/viewport | Captura |
|---|---|---|
| Consulta de estoque (vendedor) | Mobile 390×844 | ![Consulta mobile](https://github.com/user-attachments/assets/9e019628-3a7f-4bb5-bbf1-2cded8a6761c) |
| Venda rápida / saída | Mobile 390×844 | ![Venda rápida mobile](https://github.com/user-attachments/assets/31e23f70-1fb1-4877-804c-7bcf87d8a939) |
| Consulta de estoque (resultado) | Mobile 390×844 | ![Resultado mobile](https://github.com/user-attachments/assets/97a123a8-b3a1-4aae-8d8e-6f14e4bb6e0f) |

> **Limitação declarada:** não houve teste em dispositivo físico (apenas emulador/viewport de devtools). Registrado como pendência na Seção 9.

---

## 6. Índice de Capturas de Tela (Protótipos × Implementação)

### 6.1 Protótipos (HTML estaticamente versionados)

| ID | Arquivo | Situação |
|---|---|---|
| PROT-001 | [`docs/05-ux/prototipos/prototipo-lojista.html`](prototipos/prototipo-lojista.html) | ✅ Versão versionada no repositório |
| PROT-002 | `docs/05-ux/prototipos/prototipo-supervisor.html` | ⬜ **Não existe no repositório** (correção: versão anterior afirmava "versionado") |
| PROT-003 | [`docs/05-ux/prototipos/prototipo-vendedor.html`](prototipos/prototipo-vendedor.html) | ✅ Versão versionada no repositório |
| PROT-004 | `docs/05-ux/prototipos/prototipo-gerente.html` | ⬜ **Não existe no repositório** (correção: versão anterior afirmava "versionado") |
| PROT-005 | `docs/05-ux/prototipos/prototipo-funcionario.html` | ⬜ **Não existe no repositório** (correção: versão anterior afirmava "versionado") |
| PROT-006 | [`docs/05-ux/prototipos/login.html`](prototipos/login.html) | ✅ Versão versionada no repositório |

> **Verificado em `main`:** `docs/05-ux/prototipos/` contém apenas `login.html`, `prototipo-lojista.html`, `prototipo-vendedor.html` e `README.md`. A versão anterior do card listava 5 protótipos como "versionados", o que era inacurado — os protótipos de supervisor, gerente e funcionário são **pendência** (Seção 9).

**Capturas de protótipos ainda pendentes** (conforme [`docs/05-ux/evidencias/README.md`](evidencias/README.md)): `lojista-desktop.png`, `lojista-tablet.png`, `vendedor-mobile.png`.

### 6.2 Capturas da implementação — desktop (1366×768)

| Tela | Captura |
|---|---|
| Login | ![Login desktop](https://github.com/user-attachments/assets/03f57742-fb9a-4ba7-9a3d-e0ff2a3f1f15) |
| Consulta de estoque (vendedor) | ![Consulta desktop](https://github.com/user-attachments/assets/45e1ecb8-30d4-413c-b7d6-b15e9e6b7a11) |
| Venda rápida / saída | ![Venda rápida desktop](https://github.com/user-attachments/assets/f8269a32-1fc5-4e07-a5af-663ea45eb21e) |
| Catálogo de produtos (lojista) | ![Produtos desktop](https://github.com/user-attachments/assets/c90c1916-9c76-4dd5-bb17-bc2ca77e75ce) |
| Movimentações | ![Movimentações desktop](https://github.com/user-attachments/assets/e40940e3-46d1-4c77-ae46-6888c93c1cf6) |

### 6.3 Capturas da implementação — tablet (768×1024)

| Tela | Captura |
|---|---|
| Consulta de estoque | ![Consulta tablet](https://github.com/user-attachments/assets/b0a4db1d-cf96-4df8-a53a-2b51ae1ec641) |
| Produtos | ![Produtos tablet](https://github.com/user-attachments/assets/8d09e2c4-6084-4f46-81b4-f482f59f2d0d) |

### 6.4 Capturas da implementação — mobile (390×844)

As capturas mobile da Seção 5 são o registro mobile canônico deste pacote (viewport 390×844 padronizado).

### 6.5 Pendências de captura

Conforme [`evidencias/README.md`](evidencias/README.md) (5 registros — **correção desta versão**: a revisão anterior citava só as 3 pendentes):

| Arquivo | Origem | Viewport | Estado |
|---|---|---|---|
| `lojista-desktop.png` | `prototipo-lojista.html` | 1440×900 | ⬜ Pendente de captura e revisão |
| `lojista-tablet.png` | `prototipo-lojista.html` | 768×1024 | ⬜ Pendente de captura e revisão |
| `vendedor-mobile.png` | `prototipo-vendedor.html` | 390×844 | ⬜ Pendente de captura e revisão |
| [`vendedor-s2-atendimento-mobile.png`](evidencias/vendedor-s2-atendimento-mobile.png) | `prototipo-vendedor.html` | moldura 414 px | ✅ Registrada (confirmação "Vendeu" SKU RUN-PRO-PT-37) |
| [`vendedor-s2-desistiu-sem-sku-mobile.png`](evidencias/vendedor-s2-desistiu-sem-sku-mobile.png) | `prototipo-vendedor.html` | moldura 414 px | ✅ Registrada ("Desistiu" sem numeração) |

> As duas capturas registradas usam **moldura de 414 px**, diferente do 390×844 padrão da Seção 5 — viewport de protótipo ≠ viewport de implementação; não misturar os números.

Também seguem pendentes: capturas das telas pós-formais (`Atendimentos`, `EntradasFiscais/Importar`, `SaldosZerados`, `Rupturas/Index`).

---

## 7. Testes Automatizados e Execução na CI

### 7.1 Comandos oficiais de execução

```bash
# Backend (sem .sln no repositório — a CI aponta os projetos diretamente)
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj --configuration Release
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --configuration Release

# Frontend
npm ci
npx playwright install --with-deps chromium
npm run test:frontend
```

> **Correções:** (1) não existe `.sln` na raiz — `dotnet test` puro falha com `MSB1003`; os caminhos acima são os da CI. (2) O `package.json` possui apenas o script `test:frontend`. Não existem `pytest`, `npm run build` nem diretório `docs/03-testes/`.

### 7.2 Arquivos de teste existentes na `main` (`8c94b36`) — inventário completo

> **Correção desta versão:** a revisão anterior listava só 8 arquivos; a `main` contém **26 classes de teste de backend** + infraestrutura e **4 arquivos de teste frontend**. Coluna "Período": **base** = anterior à Sprint 2 (até 09/09/2026), **formal** = 10/09 a 07/10/2026, **pós** = após 07/10/2026.

**Backend — `tests/SquadEstoque.Web.Tests/`**

| Arquivo | Criado em | Período |
|---|---|---|
| [`AuthenticationAuthorizationTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs) | 17/08/2026 | base |
| [`BasicRoutesTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/BasicRoutesTests.cs) | 17/08/2026 | base |
| [`EstoqueDomainPersistenceTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/EstoqueDomainPersistenceTests.cs) | 17/08/2026 | base |
| [`ConsultaEstoqueTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/ConsultaEstoqueTests.cs) | 31/08/2026 | base |
| [`EnumDomainMigrationTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/EnumDomainMigrationTests.cs) | 31/08/2026 | base |
| [`AdministrativeHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AdministrativeHttpTests.cs) | 02/09/2026 | base |
| [`ConsultaOperacionalHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/ConsultaOperacionalHttpTests.cs) | 03/09/2026 | base |
| [`EstoqueControllerHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/EstoqueControllerHttpTests.cs) | 03/09/2026 | base |
| [`VendaRapidaHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/VendaRapidaHttpTests.cs) | 22/09/2026 | ✅ formal |
| [`RupturaHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/RupturaHttpTests.cs) | 30/09/2026 | ✅ formal |
| [`RupturaPersistenceHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/RupturaPersistenceHttpTests.cs) | 30/09/2026 | ✅ formal |
| [`DesistiuHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/DesistiuHttpTests.cs) | 01/10/2026 | ✅ formal |
| [`RupturaInvalidSkuHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/RupturaInvalidSkuHttpTests.cs) | 01/10/2026 | ✅ formal |
| [`VendaTransacionalTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/VendaTransacionalTests.cs) | 08/10/2026 | 🕐 pós |
| [`AtendimentoAuditavelHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AtendimentoAuditavelHttpTests.cs) | 11/10/2026 | 🕐 pós |
| [`EntradaFiscalHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/EntradaFiscalHttpTests.cs) | 11/10/2026 | 🕐 pós |
| [`AjusteAuthorizationHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AjusteAuthorizationHttpTests.cs) | 12/10/2026 | 🕐 pós |
| [`RollbackVendaTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/RollbackVendaTests.cs) | 12/10/2026 | 🕐 pós |
| [`SaldosZeradosViewModelTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/SaldosZeradosViewModelTests.cs) | 13/10/2026 | 🕐 pós |
| [`MovimentacaoImutabilidadeHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/MovimentacaoImutabilidadeHttpTests.cs) | 14/10/2026 | 🕐 pós |
| [`MovimentacaoSomenteLeituraHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/MovimentacaoSomenteLeituraHttpTests.cs) | 16/10/2026 | 🕐 pós |
| [`SaldosZeradosAuthorizationHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/SaldosZeradosAuthorizationHttpTests.cs) | 16/10/2026 | 🕐 pós |
| [`RupturasControllerTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/RupturasControllerTests.cs) | 20/10/2026 | 🕐 pós |
| [`RupturasHistoricoHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/RupturasHistoricoHttpTests.cs) | 20/10/2026 | 🕐 pós |
| [`AjusteMotivoHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AjusteMotivoHttpTests.cs) | 22/10/2026 | 🕐 pós |
| [`SkuUltimaAtualizacaoMigrationTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/SkuUltimaAtualizacaoMigrationTests.cs) | 26/10/2026 | 🕐 pós |

Infraestrutura (não é caso de teste): [`SquadEstoqueWebApplicationFactory.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/SquadEstoqueWebApplicationFactory.cs) (17/08/2026) + [`test.runsettings`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/test.runsettings).

**Frontend — `tests/frontend/`**

| Arquivo | Criado em | Período | Casos |
|---|---|---|---|
| [`consulta-nao-tinha.test.cjs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/frontend/consulta-nao-tinha.test.cjs) | 28/09/2026 | ✅ formal | 5 testes (busca, seleção de numeração, venda, "Não tinha", bloqueio de envio) |
| [`consulta-desistiu.test.cjs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/frontend/consulta-desistiu.test.cjs) | 02/10/2026 | ✅ formal | 2 testes (resposta inválida, falha na nova busca) |
| [`consulta-responsividade.test.cjs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/frontend/consulta-responsividade.test.cjs) | 09/10/2026 | 🕐 pós | `NFT-07` (responsividade) |
| [`ajuste-motivo.test.cjs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/frontend/ajuste-motivo.test.cjs) | 27/10/2026 | 🕐 pós | Frontend do motivo de ajuste (complementa `IT-15`/`IT-16`) |

**Resumo:** 154 atributos `[Fact]`/`[Theory]` no backend e **4 arquivos** de teste frontend. Apenas **7 arquivos** (5 backend + 2 frontend) foram criados **dentro do período formal**; a maior parte da suíte é de base anterior ou de evolução pós-07/10 — transparência registrada para não confundir cobertura atual com cobertura da P1.

### 7.3 CI

- Workflow: [`.github/workflows/dotnet.yml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/.github/workflows/dotnet.yml) — dispara em **push para qualquer branch** e em PR para `main`/`dev/emmy`.
- Jobs reais: `build-and-test` (.NET 10, `dotnet build` + `dotnet test` com coleta de cobertura e **piso de 50% de line coverage**) e `frontend-tests` (Node 22, `npm ci`, Chromium do Playwright, `npm run test:frontend`); `deploy-azure` só na `main` com `AZURE_CD_ENABLED=true`.
- Execução citada no [relatório da sprint 2](../10-planejamento/relatorio-sprint-2.md): runs `e9d553a` / `38156810379`.

### 7.4 Conferência local deste pacote (27/10/2026)

Comando: `dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --configuration Release` (.NET SDK **10.0.402**, `main` @ `8c94b36`):

```
Aprovado! – Com falha: 0, Aprovado: 274, Ignorado: 0, Total: 274, Duração: 1 m 16 s
```

- **274/274 aprovados, 0 falhas, 0 ignorados.**
- Observação honesta: os 274 incluem os testes criados **após** 07/10/2026 (Seção 7.2) — a suíte atual **não** equivale à cobertura da P1 em 07/10; usar a coluna *Período* para separar o que existia na entrega formal.
- Testes frontend (`npm run test:frontend`) não executados nesta conferência local (exigem Chromium do Playwright); cobertos pelo job `frontend-tests` da CI.

---

## 8. Rastreabilidade com os PRs Entregues

> **Correção aplicada:** URLs dos PRs 35–45 estavam sem o dono do repositório (`github.com/SQUAD-estoque/...`), o que gerava link quebrado. Todas foram corrigidas para `github.com/CodeWitch00/SQUAD-estoque/pull/N`. As relações foram preenchidas com base nas datas reais de merge de cada PR no período formal (10/09–07/10/2026).

| PR | Título | Merge | Relação com o S2-DOC-038 |
|---|---|---|---|
| [#25](https://github.com/CodeWitch00/SQUAD-estoque/pull/25) | Consulta de estoque | 16/09/2026 | Fornece a tela VEN-02 (Consulta de produtos) e os casos `IT-19`/`IT-20` |
| [#29](https://github.com/CodeWitch00/SQUAD-estoque/pull/29) | Consulta operacional | 18/09/2026 | Alimenta `IT-07`/`IT-08` (movimentações) |
| [#31](https://github.com/CodeWitch00/SQUAD-estoque/pull/31) | Venda rápida | 22/09/2026 | Fornece o resultado VEN-04 (`Vendeu`) e o caso `IT-24` |
| [#32](https://github.com/CodeWitch00/SQUAD-estoque/pull/32) | Ajustes de autorização/navegação | 26/09/2026 | Base da Seção 3.6 (divergências de autorização) |
| [#34](https://github.com/CodeWitch00/SQUAD-estoque/pull/34) | Cadastro/numerações de produto | 24/09/2026 | Fornece LOJ-03 (novo produto e grade) |
| [#35](https://github.com/CodeWitch00/SQUAD-estoque/pull/35) | Testes de autorização | 24/09/2026 | Evidência de `CT-AUT-09` a `CT-AUT-15` |
| [#38](https://github.com/CodeWitch00/SQUAD-estoque/pull/38) | Consulta/ruptura | 29/09/2026 | Fornece VEN-05 (`Não tinha`) e o caso `IT-27` |
| [#39](https://github.com/CodeWitch00/SQUAD-estoque/pull/39) | Testes de venda rápida | 01/10/2026 | Evidência de `IT-24`/`IT-25` |
| [#40](https://github.com/CodeWitch00/SQUAD-estoque/pull/40) | Testes de ruptura (SKU inválido) | 01/10/2026 | Evidência de `IT-27` |
| [#41](https://github.com/CodeWitch00/SQUAD-estoque/pull/41) | Testes de desistência | 01/10/2026 | Evidência de `IT-28`/`IT-29` |
| [#42](https://github.com/CodeWitch00/SQUAD-estoque/pull/42) | Documentação da P1 (plano de testes) | 03/10/2026 | Fonte normativa da Seção 4 |
| [#44](https://github.com/CodeWitch00/SQUAD-estoque/pull/44) | Testes HTTP operacionais | 01/10/2026 | Evidência de `IT-07`/`IT-08` |
| [#45](https://github.com/CodeWitch00/SQUAD-estoque/pull/45) | Validações manuais (homologação) | 03/10/2026 | Fonte da Seção 5 (responsividade) |
| [#46](https://github.com/CodeWitch00/SQUAD-estoque/pull/46) | Índices e navegação da documentação | 04/10/2026 | Sustenta o índice da Seção 2 |
| [#49](https://github.com/CodeWitch00/SQUAD-estoque/pull/49) | Ajustes finais da P1 | 04/10/2026 | Conferência prévia do pacote |
| [#61](https://github.com/CodeWitch00/SQUAD-estoque/pull/61) | Tentativa anterior do S2-DOC-038 | fechado sem merge | Contexto: branch reutilizada nesta entrega |

**PRs pós-período formal (contexto — fora da janela da P1):**

| PR | Título | Merge/Estado | Relação com o S2-DOC-038 |
|---|---|---|---|
| [#69](https://github.com/CodeWitch00/SQUAD-estoque/pull/69) | Testes do motivo de ajuste (S3-QA-020) | merge 25/10/2026 (na `main`) | Trouxe `AjusteMotivoHttpTests.cs` e ajustou `plano-de-testes.md` (UT-11/UT-12) — fonte da Seção 4 atualizada |
| [#70](https://github.com/CodeWitch00/SQUAD-estoque/pull/70) | Evidências do histórico de rupturas | merge 25/10/2026 (na `main`) | Atualizou `registro-validacoes-manuais.md` (linha de 20/10 agora aponta para capturas no commit `381224d`) |
| [#71](https://github.com/CodeWitch00/SQUAD-estoque/pull/71) | Validação do motivo de ajuste | merge 27/10/2026 (na `main`) | Trouxe `docs/09-testing/ajuste-motivo.md` + JSONs de evidência e novos casos em `AjusteMotivoHttpTests.cs` — cobertura adicional de `IT-15`/`IT-16` (Seção 4.3) |
| [#73](https://github.com/CodeWitch00/SQUAD-estoque/pull/73) | Validação de navegação por perfil (S2-QA-035) | **fechado sem merge; reenviado no [#81](https://github.com/CodeWitch00/SQUAD-estoque/pull/81)** | 15 cenários: 14 aprovados, 1 em execução (VNP-05 — contagem de ocorrências não conferida); relatório, 71 capturas e log HTTP versionados na branch original `developer/nicolas` do #81 (Seção 9, itens 3 e 3b) |
| [#74](https://github.com/CodeWitch00/SQUAD-estoque/pull/74) | Evidências P1 do S2-DOC-038 (versão anterior deste documento) | **fechado sem merge** (26/10/2026) | Substituído por este documento, que corrige as 4 divergências apontadas na revisão: códigos de tela, autorização de `/Rupturas` e `/Atendimentos`, IDs oficiais de teste e usuário `vendedor@squad.com` |
| [#75](https://github.com/CodeWitch00/SQUAD-estoque/pull/75) | Cobrir desempates do histórico de rupturas (S3-BE-015) | merge 27/10/2026 (na `main`) | Ampliou `RupturasControllerTests.cs` — evidência adicional de `IT-32`/`E2E-07` |
| [#77](https://github.com/CodeWitch00/SQUAD-estoque/pull/77) | Auditar aumento e redução do ajuste (S3-QA-022) | merge 27/10/2026 (na `main`) | Ampliou `EstoqueDomainPersistenceTests.cs` — evidência adicional de `IT-15` |

> **Estado da `main` em 29/10/2026:** `95199c4` (`feat: exibir última atualização de saldo nas grades`). A branch deste documento foi criada sobre `a305483` (base `8c94b36`) e está atrás da `main`; o PR foi aberto sem merge local da `main`, e a mesclagem é verificada pelo GitHub. As contagens de teste da Seção 7.4 referem-se a `8c94b36`.

---

## 9. Itens Pendentes e Falhas Conhecidas

> Lista transparente — substitui os itens genéricos e não verificáveis da versão anterior (SignalR, exportação PDF, WCAG genérico) por pendências **reais e documentadas** no repositório.

| # | Item | Status | Evidência |
|---|---|---|---|
| 1 | Homologação em ambiente Azure publicado | **Bloqueada** — homologação travada por versão de grade desatualizada | [`registro-validacoes-manuais.md`](../09-testing/homologacoes/registro-validacoes-manuais.md) |
| 2 | 3 capturas de protótipo ainda pendentes (`lojista-desktop.png`, `lojista-tablet.png`, `vendedor-mobile.png`); 2 já registradas (`vendedor-s2-*`) | Parcial — 2 de 5 concluídas | [`docs/05-ux/evidencias/README.md`](evidencias/README.md) |
| 2b | Protótipos de supervisor, gerente e funcionário (`prototipo-supervisor.html`, `prototipo-gerente.html`, `prototipo-funcionario.html`) **não existem** no repositório | Pendente | `docs/05-ux/prototipos/` contém apenas `login.html`, `prototipo-lojista.html`, `prototipo-vendedor.html` |
| 3 | Cenários VNP-12, VNP-14 e VNP-15 — antes não executados — foram concluídos na execução complementar de 27/10/2026. O relatório atual (PR [#81](https://github.com/CodeWitch00/SQUAD-estoque/pull/81)) registra **15 cenários: 14 aprovados, 1 em execução**; o VNP-05 permanece em execução porque a contagem de ocorrências do Histórico de Rupturas não foi conferida contra os registros e o status HTTP de `/Rupturas` não foi coletado | Resolvido para VNP-12/14/15; VNP-05 em execução — PR #81 aberto | PR #81 (relatório `docs/09-testing/execucoes/sprint-2/validacao-navegacao-perfil-desktop-mobile.md`, Seções 8, 9 e 12) |
| 3b | Divergência de viewport: o relatório do #73/#81 usa **390×844** nas capturas iniciais e **375×812** na execução complementar (Playwright), enquanto a Seção 5 deste pacote padroniza **390×844**. A distinção está declarada nas Seções 5.1, 12 e 13 (item 9) do relatório | Registrada e documentada — não padronizada entre artefatos | PR #81 (Seções 5.1, 12 e 13 do relatório) |
| 3c | Cobertura adicional de `IT-15`/`IT-16` trazida pelo PR [#71](https://github.com/CodeWitch00/SQUAD-estoque/pull/71) (merge 27/10/2026): `docs/09-testing/ajuste-motivo.md` + JSONs de evidência (`evidencias/ajuste-motivo/*.json`) + novos casos de normalização em `AjusteMotivoHttpTests.cs` | **Resolvido — na `main` desde 27/10/2026** | PR #71 (merge) |
| 4 | Teste em dispositivo físico (não apenas viewport/emulador) | Não realizado | [`registro-validacoes-manuais.md`](../09-testing/homologacoes/registro-validacoes-manuais.md) |
| 5 | `NFT-01` a `NFT-07` — especificação de testes não-funcionais inexistente (`especificacao-testes-nao-funcionais.md` não existe no repo) | Parcial — evidência restrita a responsividade + registro manual | Seção 4.3 (`NFT-01` a `NFT-07`) |
| 6 | Importação de NF-e (sem ID oficial no catálogo) e `IT-30` a `IT-33` (saldos zerados e histórico de rupturas) **foram executados apenas após o período formal** (11/10 a 20/10) — não havia cobertura em 07/10/2026 | 🕐 Pós-formal — evidência existe, fora da janela da P1 | Seções 3.4/3.5/4.2 e Seção 7.2 |
| 7 | Tela de recuperação de senha sem código no inventário oficial e sem implementação | Não implementada | `Views/Account/` não possui `RecuperarSenha.cshtml` (Seção 3.5) |
| 8 | Códigos `SUP` no inventário são telas de apoio (SUP-01 = `/Account/AccessDenied`, SUP-02 = `/Home/Error`); não existe perfil supervisor em nenhum `[Authorize]` nem tela de relatórios | Registrado — sem fluxo SUP na aplicação | Seções 3.5 e 3.6 |
| 9 | Divergência LOJ-07/VEN-04: `/Movimentacoes/Saida` aceita `LOJISTA,VENDEDOR` (a saída administrativa é do lojista e o vendedor teria apenas `Vendeu`) | Registrada, não corrigida | [`MovimentacoesController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/MovimentacoesController.cs) |
| 10 | Capturas das telas pós-formais (Atendimentos, NF-e import, SaldosZerados, Rupturas) | Pendente | Seção 6.5 |

---

## 10. Checklist de Integridade do Pacote

- [x] Índice versionado do pacote P1 criado neste arquivo (`docs/05-ux/S2-DOC-038-evidencias-p1.md`)
- [x] Inventário de todas as telas MVP com rota real verificada no código
- [x] Códigos de tela (`AUT`/`VEN`/`LOJ`/`SUP`) usados **com o significado oficial** do [`inventario-telas-e-mapa-navegacao.md`](inventario-telas-e-mapa-navegacao.md); telas sem código oficial rotuladas como tal — correção apontada na revisão do PR #74
- [x] `/Rupturas` e `/Atendimentos` classificados como **exclusivos do `LOJISTA`**, conforme o `[Authorize(Roles = "LOJISTA")]` dos controllers na `main`; `/Rupturas` classificada como 🕐 (view criada em 20/10/2026, após o encerramento da P1 em 07/10) — correção apontada na revisão do PR #74
- [x] Telas pós-período formal (após 07/10) classificadas como 🕐 e não como entrega da P1
- [x] Casos de teste com os **IDs oficiais** do [`plano-de-testes.md`](../09-testing/plano-de-testes.md) (`CT-AUT`/`IT`/`E2E`/`NFT`/`UT`), com entrada, esperado, obtido e evidência; tabela de correspondência com os IDs antigos (CT-001..CT-015) na Seção 4.1 — correção apontada na revisão do PR #74
- [x] Usuário de login corrigido para `vendedor@squad.com` (conforme o teste referenciado; a versão anterior citava `vendedor@teste.com`) — correção apontada na revisão do PR #74
- [x] NF-e (sem ID oficial) e `IT-30` a `IT-33` declarados **executados apenas após o período formal** (evidência datada de 11/10 a 20/10, fora da janela da P1)
- [x] Inventário de testes completo (26 classes backend + 4 arquivos frontend) com data e rótulo *base*/*formal*/*pós* — corrigido de 8 arquivos incompletos
- [x] PRs #69/#70/#71/#75/#77 (merges pós-formais), #73 (fechado sem merge) e #81 (aberto, reentrega do #73 na branch original `developer/nicolas`) incluídos com estado real; PR #74 registrado como fechado sem merge; nenhum arquivo de PR aberto citado como existente na `main`
- [x] `NFT-01` a `NFT-07` declarados **parciais** com a inexistência do artefato citada
- [x] Viewport mobile padronizado em 390×844
- [x] Capturas mobile apontam para as imagens mobile reais (não reutilizam as desktop)
- [x] Comandos de execução reais (`dotnet test`, `npm run test:frontend`) e CI `.github/workflows/dotnet.yml`
- [x] URLs dos PRs 35–45 corrigidas (incluído o dono `CodeWitch00`)
- [x] Relações de PR preenchidas com datas reais de merge
- [x] PROT-003 corrigido de "planejado" para "versão versionada (PR #3, 25/08)"
- [x] Títulos Markdown duplicados/quebrados removidos (H1 solto no meio da Seção 5.1, `## Fluxo Administrativo` duplicado, `# 8.` como H1)
- [ ] **Pendências da Seção 9 não resolvidas por este card** (declaradas, não mascaradas)
- [ ] Links das capturas desktop/mobile dependem de anexos GitHub user-attachment — podem expirar; pendência de migração para [`docs/05-ux/evidencias/`](evidencias/) (item da Seção 9)

---

## 11. Conferência Final da Documentação (registro)

| Etapa | Responsável | Data | Status |
|---|---|---|---|
| Auditoria do rascunho × código real (rotas, views, datas) | Integrante de documentação | 26/10/2026 | ✅ Concluída |
| Correção de inventário, casos de teste e links | Integrante de documentação | 26/10/2026 | ✅ Concluída |
| Commit na branch `docs/s2-doc-038-integrante` (ciclo do PR #74) | Integrante de documentação | 26/10/2026 | ✅ `9ffc0f9` |
| Re-sincronização com a `main` (`298b850`, 0 commits atrás) | Integrante de documentação | 26/10/2026 | ✅ Verificado via `ls-remote`, `fetch` e API |
| 2ª revisão: inventário completo de testes, NF-e/saldos zerados pós-formais, capturas de protótipo, PRs #69/#70/#71/#73 | Integrante de documentação | 26/10/2026 | ✅ Concluída (correções a–f) |
| Revisão do PR [#74](https://github.com/CodeWitch00/SQUAD-estoque/pull/74) pela equipe | Equipe | 26/10/2026 | ✅ **Fechado sem merge** com 4 anotações de rastreabilidade (códigos de tela, autorização de `/Rupturas` e `/Atendimentos`, IDs oficiais de teste, usuário `vendedor@squad.com`) |
| 3ª revisão — correção das anotações do PR #74: Seção 3 reescrita com os códigos oficiais do inventário; `/Rupturas` e `/Atendimentos` como exclusivos do `LOJISTA`; `/Rupturas` como 🕐 (20/10); Seção 4 reescrita com IDs oficiais + tabela de correspondência CT-001..CT-015; usuário corrigido para `vendedor@squad.com` | Integrante de documentação | 27/10/2026 | ✅ Concluída |
| Re-sincronização com a `main` (`8c94b36`, merges de #71/#75/#77) e atualização das Seções 7.2, 7.4, 8, 9 e 10 | Integrante de documentação | 27/10/2026 | ✅ Verificado via `ls-remote`, `fetch` e API |
| Conferência local da suíte (`274/274 aprovados` em `8c94b36`) | Integrante de documentação | 27/10/2026 | ✅ Seção 7.4 |
| 4ª revisão — atualização das referências ao S2-QA-035: #73 fechado sem merge, reentrega no #81, estado real de 14 aprovados + 1 em execução (VNP-05) e viewport documentada | Integrante de documentação | 29/10/2026 | ✅ Concluída |
| Reenvio na branch original `docs/s2-doc-038-fix` (substituindo o PR #78 fechado, que substituiu o #74) | Equipe | 29/10/2026 | ✅ Branch publicada; PR [#82](https://github.com/CodeWitch00/SQUAD-estoque/pull/82) aberto para revisão |
| Conferência final e merge | Equipe | — | ⏳ Pendente |

**Conferência final:** o pacote P1 está consolidado neste índice versionado, alinhado às fontes normativas (inventário de telas e plano de testes) e com divergências e pendências declaradas nas Seções 3.6 e 9. Nenhuma funcionalidade planejada foi declarada como implementada; nenhuma monografia foi duplicada neste card. As 4 anotações da revisão do PR #74 foram corrigidas nesta versão.
