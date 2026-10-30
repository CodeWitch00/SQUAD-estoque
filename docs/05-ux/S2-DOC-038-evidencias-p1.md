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
| **Estado** | Consolidado em 29/10/2026, alinhado à `main` `7d76434` |

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
| 11 | Registro de conferência da documentação | Seção 11 |

**Artefatos de referência (normativos deste pacote):**

- Inventário de telas e mapa de navegação: [`docs/05-ux/inventario-telas-e-mapa-navegacao.md`](inventario-telas-e-mapa-navegacao.md)
- Plano de testes: [`docs/09-testing/plano-de-testes.md`](../09-testing/plano-de-testes.md)
- Registro de validações manuais: [`docs/09-testing/homologacoes/registro-validacoes-manuais.md`](../09-testing/homologacoes/registro-validacoes-manuais.md)
- Especificação de testes não funcionais (arquivo histórico): [`docs/09-testing/especificacoes/arquivo-historico/especificacao-testes-nao-funcionais.md`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/docs/09-testing/especificacoes/arquivo-historico/especificacao-testes-nao-funcionais.md)
- Critérios de avaliação do professor (P1): [`docs/10-planejamento/avaliacao-professor.md`](../10-planejamento/avaliacao-professor.md)
- Índice de UX: [`docs/05-ux/README.md`](README.md)

---

## 3. Inventário de Telas MVP × Implementação Real

> **Regra adotada:** os códigos (`AUT`, `VEN`, `LOJ`, `SUP`) são exclusivamente os de [`inventario-telas-e-mapa-navegacao.md`](inventario-telas-e-mapa-navegacao.md) e mantêm o significado oficial do inventário. Telas sem código no inventário oficial são listadas na Seção 3.5 com o rótulo "sem código oficial"; nenhum código oficial é reaproveitado com outro sentido. Toda tela é classificada com base na existência da view no repositório em `main` e na data do commit que a criou ([`git log --diff-filter=A`](https://github.com/CodeWitch00/SQUAD-estoque/commits/main)).

### 3.1 Legenda de situação

| Situação | Significado |
|---|---|
| 📦 **Base (anterior à Sprint 2)** | View criada antes de 10/09/2026; já existia na base do projeto e **não** é entrega da P1 |
| ✅ **Entregue no período formal** | Funcionalidade concluída entre 10/09/2026 e 07/10/2026 |
| 🕐 **Implementada após o período formal** | View criada **depois de 07/10/2026** |
| ⚠️ **Existente com divergência** | Funcionalidade existe, mas sua autorização ou forma difere do inventário normativo |
| ⬜ **Planejada / não implementada** | Não existe view correspondente no repositório |

### 3.2 Fluxo de Autenticação (`AUT` — códigos oficiais)

| Código | Tela (significado oficial) | Rota real | Situação | Evidência |
|---|---|---|---|---|
| AUT-01 | Login | [`/Account/Login`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Account/Login.cshtml) | 📦 Base (view criada em 17/08/2026) | `Views/Account/Login.cshtml`; testes [`AuthenticationAuthorizationTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs) (criado 17/08) |

**Observações verificadas no código:**

- O `AccountController` possui `[Authorize]`/`AllowAnonymous` e o login redireciona por perfil ([`AccountController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/AccountController.cs)):
  - `VENDEDOR` → `/Estoque/Consulta`
  - `LOJISTA` → `/Produtos`
- A opção **"Lembrar de mim"** existe no formulário de login (`Login.cshtml`).
- O inventário oficial não possui código para a tela de recuperação de senha; ela é listada como pendência na Seção 3.5.

### 3.3 Fluxo do Vendedor (`VEN-01` a `VEN-06` — significados oficiais do inventário)

| Código | Tela (significado oficial) | Rota/ação real | Autorização real | Situação | Evidência |
|---|---|---|---|---|---|
| VEN-01 | Início do vendedor | Sem tela própria; o login redireciona `VENDEDOR` para `/Estoque/Consulta` | `VENDEDOR` | 📦 Base — atendida indiretamente pelo redirecionamento pós-login (o inventário prevê entrada do perfil já com acesso à consulta) | [`AccountController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/AccountController.cs) |
| VEN-02 | Consulta de produtos | `/Estoque/Consulta` | `VENDEDOR` | 📦 Base (view criada em 31/08/2026, commit `61648a9`); a **busca tolerante** foi entregue no período formal (PR [#29](https://github.com/CodeWitch00/SQUAD-estoque/pull/29), 18/09) | [`EstoqueController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/EstoqueController.cs), [`Views/Estoque/Consulta.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Estoque/Consulta.cshtml); testes [`ConsultaEstoqueTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/ConsultaEstoqueTests.cs) |
| VEN-03 | Grade consultada | Grade (numerações, saldos e estados) exibida na própria `/Estoque/Consulta` | `VENDEDOR` | 📦 Base (view 31/08/2026); a atualização da grade após a venda foi entregue no período (PR [#35](https://github.com/CodeWitch00/SQUAD-estoque/pull/35), 24/09) | [`Views/Estoque/Consulta.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Estoque/Consulta.cshtml) |
| VEN-04 | Resultado "Vendeu" | Ação da grade em `/Estoque/Consulta`; baixa registrada via `/Movimentacoes/Saida` | `LOJISTA,VENDEDOR` | ⚠️ **Divergência:** a saída genérica é compartilhada com o lojista (ver LOJ-07 e Seção 3.6, item 2). Backend e teste entregues no período (PRs [#25](https://github.com/CodeWitch00/SQUAD-estoque/pull/25) e [#31](https://github.com/CodeWitch00/SQUAD-estoque/pull/31)) | [`MovimentacoesController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/MovimentacoesController.cs); testes [`VendaRapidaHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/VendaRapidaHttpTests.cs) (22/09, no período) |
| VEN-05 | Resultado "Não tinha" | Ação da grade em `/Estoque/Consulta`; ruptura persistida pelo fluxo da consulta | `VENDEDOR` (ação da grade) | ✅ Fluxo entregue no período formal (view base) — PRs [#38](https://github.com/CodeWitch00/SQUAD-estoque/pull/38) e [#41](https://github.com/CodeWitch00/SQUAD-estoque/pull/41) | [`RupturaHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/RupturaHttpTests.cs) (30/09, no período), [`consulta-nao-tinha.test.cjs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/frontend/consulta-nao-tinha.test.cjs) (28/09, no período) |
| VEN-06 | Resultado "Desistiu" | Ação da grade em `/Estoque/Consulta`; sem persistência de movimentação | `VENDEDOR` | ✅ Fluxo entregue no período formal (view base) — PRs [#44](https://github.com/CodeWitch00/SQUAD-estoque/pull/44), [#46](https://github.com/CodeWitch00/SQUAD-estoque/pull/46) e [#49](https://github.com/CodeWitch00/SQUAD-estoque/pull/49) | [`DesistiuHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/DesistiuHttpTests.cs) (01/10, no período), [`consulta-desistiu.test.cjs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/frontend/consulta-desistiu.test.cjs) (02/10, no período) |

> **Nota sobre o histórico de rupturas:** a rota `/Rupturas` é a **LOJ-11** (Histórico de rupturas, Seção 3.4), é **exclusiva do `LOJISTA`** e foi criada em **20/10/2026** — após o período formal.

### 3.4 Fluxo do Lojista (`LOJ-01` a `LOJ-11` — significados oficiais do inventário)

| Código | Tela (significado oficial) | Rota real | Autorização real | Situação | Evidência |
|---|---|---|---|---|---|
| LOJ-01 | Início do lojista | Sem tela própria; o login redireciona `LOJISTA` para `/Produtos` | `LOJISTA` | 📦 Base — atendida indiretamente pelo redirecionamento pós-login | [`AccountController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/AccountController.cs) |
| LOJ-02 | Produtos (catálogo) | `/Produtos` | `LOJISTA` | 📦 Base (view criada em 17/08/2026) | [`ProdutosController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/ProdutosController.cs), [`Views/Produtos/Index.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Produtos/Index.cshtml) |
| LOJ-03 | Novo produto e grade | `/Produtos/Create` (+ grade em `/Produtos/AdicionarNumeracoes`) | `LOJISTA` | 📦 Base (`Create.cshtml`, 17/08/2026) + ✅ **formal** (grade `AdicionarNumeracoes.cshtml`, 03/10/2026) | [`Views/Produtos/Create.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Produtos/Create.cshtml), [`Views/Produtos/AdicionarNumeracoes.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Produtos/AdicionarNumeracoes.cshtml) |
| LOJ-04 | Detalhe do produto/grade | `/Produtos/Details/{id}` | `LOJISTA` | 📦 Base (view criada em 17/08/2026) | [`Views/Produtos/Details.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Produtos/Details.cshtml) |
| LOJ-05 | Editar produto | `/Produtos/Edit/{id}` | `LOJISTA` | 📦 Base (view criada em 17/08/2026) | [`Views/Produtos/Edit.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Produtos/Edit.cshtml) |
| LOJ-06 | Entrada de estoque | `/Movimentacoes/Entrada` (e `/Movimentacoes/EntradaLote`) | `LOJISTA` | 📦 Base (`Entrada.cshtml`, 17/08/2026) + ✅ **formal** (`EntradaLote.cshtml`, 03/10/2026) | [`MovimentacoesController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/MovimentacoesController.cs) |
| LOJ-07 | Saída de estoque | `/Movimentacoes/Saida` | `LOJISTA,VENDEDOR` | ⚠️ **Existente com divergência** (view base): o inventário reserva a saída administrativa ao `LOJISTA`; a rota também aceita `VENDEDOR` (ver Seção 3.6, item 2) | [`MovimentacoesController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/MovimentacoesController.cs) |
| LOJ-08 | Ajuste manual | `/Movimentacoes/Ajuste` | `LOJISTA` | 📦 Base (view criada em 17/08/2026); cobertura de autorização e motivo ampliada após o período (12/10 e 22/10) | [`MovimentacoesController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/MovimentacoesController.cs) |
| LOJ-09 | Histórico de movimentações | `/Movimentacoes` | `LOJISTA` | 📦 Base (view criada em 17/08/2026) | [`MovimentacoesController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/MovimentacoesController.cs) |
| LOJ-10 | Saldos zerados | `/SaldosZerados` | `LOJISTA` | 🕐 Implementada após o período formal (view `SaldosZerados/Index.cshtml` criada em 16/10/2026, commit `600bbb2`) | [`SaldosZeradosController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/SaldosZeradosController.cs) |
| LOJ-11 | Histórico de rupturas | `/Rupturas` | `LOJISTA` | 🕐 Implementada após o período formal (view `Rupturas/Index.cshtml` criada em **20/10/2026**, commit `381224d`) | [`RupturasController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/RupturasController.cs) |

### 3.5 Telas de apoio (códigos oficiais) e telas sem código no inventário

| Rota | Código oficial | Autorização real | Situação | Evidência |
|---|---|---|---|---|
| `/Account/AccessDenied` | SUP-01 (apoio técnico) | Público (retorno seguro) | 📦 Base (view criada em 17/08/2026) | [`Views/Account/AccessDenied.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Account/AccessDenied.cshtml) |
| `/Home/Error` | SUP-02 (apoio técnico) | Público | 📦 Base (view criada em 17/08/2026) | [`Views/Shared/Error.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Shared/Error.cshtml) |
| `/Atendimentos` | **sem código no inventário oficial** | `LOJISTA` | 🕐 Implementada após o período formal (view `Atendimentos/Index.cshtml` criada em 11/10/2026, commit `bd1cdff`) | [`AtendimentosController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/AtendimentosController.cs) |
| `/EntradasFiscais/Importar` | **sem código no inventário oficial** | `LOJISTA` | 🕐 Implementada após o período formal (view criada em 11/10/2026, commit `bd1cdff`) | [`EntradasFiscaisController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/EntradasFiscaisController.cs) |
| Recuperação de senha | **sem código no inventário oficial** | — | ⬜ Planejada / não implementada (não há `RecuperarSenha.cshtml` em `Views/Account`) | `Views/Account/` |

### 3.6 Divergências do inventário registradas por este card

1. **Códigos `SUP`:** no inventário oficial, `SUP-01` e `SUP-02` são telas de apoio (`/Account/AccessDenied` e `/Home/Error`), **não** um fluxo de supervisor. Não existe papel `SUPERVISOR`/`SUP` em nenhum `[Authorize]` da aplicação — todos os controllers exigem apenas `LOJISTA` e/ou `VENDEDOR`. A tela `/Atendimentos` **não tem código no inventário** e é **exclusiva do `LOJISTA`**.
2. **`/Movimentacoes/Saida` não é exclusiva do lojista:** a rota aceita `LOJISTA,VENDEDOR`, embora o inventário reserve a saída administrativa (LOJ-07) ao lojista e dê ao vendedor apenas "Vendeu" (VEN-04).
3. **`/Rupturas` e `/Atendimentos` são exclusivos do `LOJISTA`:** os controllers [`RupturasController`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/RupturasController.cs) e [`AtendimentosController`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/AtendimentosController.cs) usam `[Authorize(Roles = "LOJISTA")]` na `main`.
4. **Telas base × pós-período:** as views principais de `AUT-01`, `LOJ-02` a `LOJ-09` e de `VEN-02/03` foram criadas entre **17/08 e 31/08/2026**, antes do início da P1; elas existem no código, mas **não são entrega da P1**. As duas views criadas dentro do período formal são `Movimentacoes/EntradaLote.cshtml` e `Produtos/AdicionarNumeracoes.cshtml` (ambas em 03/10/2026). Já `Atendimentos/Index` e `EntradasFiscais/Importar` (11/10), `SaldosZerados/Index` (16/10) e `Rupturas/Index` (20/10, commit `381224d`) entraram depois de 07/10/2026 e são classificadas como 🕐.

---

## 4. Cobertura de Testes do Pacote P1 (IDs oficiais do plano)

> Fonte normativa: [`docs/09-testing/plano-de-testes.md`](../09-testing/plano-de-testes.md), cujo catálogo usa identificadores como `CT-AUT-01`, `IT-07`, `E2E-01`, `NFT-07` e `UT-11`. Cada caso abaixo foi **executado** e registra entrada, resultado esperado, resultado obtido e evidência localizável no repositório.

### 4.1 Correspondência entre identificadores de registro e IDs oficiais

| Identificador de registro | ID(s) oficial(is) do plano | Observação |
|---|---|---|
| CT-001 | `CT-AUT-03` | Login do vendedor com credenciais válidas (`vendedor@squad.com`) |
| CT-002 | `CT-AUT-04` e `CT-AUT-05` | Login com credenciais inválidas |
| CT-003 | `CT-AUT-09` e `CT-AUT-10` | Anônimo em rota protegida |
| CT-004 | `CT-AUT-11` a `CT-AUT-15` | Autorização por perfil |
| CT-005 | `IT-19` | Busca parcial com resultado |
| CT-006 | `IT-20` | Busca sem resultado |
| CT-007 | `IT-24` e `IT-25` | Resultado `Vendeu` e venda sem saldo |
| CT-008 | `IT-27` | Ruptura com SKU inválido |
| CT-009 | `IT-28` e `IT-29` | Resultado `Desistiu` e nova consulta |
| CT-010 | *sem ID oficial* | Importação de NF-e não consta do catálogo do plano (o SRS trata NF-e como fora do escopo do MVP) — ver Seção 4.2 |
| CT-011 | `IT-07` (e `IT-08`) | Entrada válida (a modalidade em lote não tem ID dedicado no catálogo) |
| CT-012 | `IT-15`, `IT-16` e `UT-11` | Ajuste válido, sem motivo e validação do motivo |
| CT-013 | `NFT-07` (e `NFT-01`) | Responsividade/viewport |
| CT-014 | `NFT-01` a `NFT-07` | Não-funcionais (parcial) |
| CT-015 | `IT-30` a `IT-33` (e `E2E-07`) | Saldos zerados e histórico de rupturas |

### 4.2 Tabela-resumo (IDs oficiais × status do plano × evidência real)

> A coluna **Status no plano** reproduz o [`plano-de-testes.md`](../09-testing/plano-de-testes.md) na `main` `7d76434`. A coluna **Evidência neste pacote** registra o que realmente existe no repositório — inclusive quando o plano está desatualizado ou quando a evidência é posterior a 07/10/2026.

| Caso(s) oficial(is) | Cenário | Status no plano oficial | Evidência neste pacote |
|---|---|---|---|
| `CT-AUT-02` | Login do lojista e acesso a `/Produtos` | Existente | ✅ `Lojista_can_login_and_access_produtos` (17/08) |
| `CT-AUT-03` | Login do vendedor e acesso a `/Movimentacoes/Saida` | Existente | ✅ `Vendedor_can_login` (302 → `/Estoque/Consulta`) + `Vendedor_can_access_saida` (200) (17/08) |
| `CT-AUT-04` | E-mail inexistente + senha incorreta | Parcial (falta afirmar mensagem e limpeza da senha) | ✅ `Invalid_credentials_do_not_authenticate` + `Invalid_login_does_not_render_submitted_password` (17/08) — cobre os dois pontos pendentes do plano |
| `CT-AUT-05` | E-mail cadastrado + senha incorreta | Planejado | ⬜ Sem teste dedicado: o teste de credenciais inválidas usa e-mail inexistente (`CT-AUT-04`) |
| `CT-AUT-09` | Anônimo em `/Produtos` | Existente | ✅ `Produtos_without_authentication_redirects_to_login` (17/08) |
| `CT-AUT-10` | Anônimo em `/Movimentacoes` | Existente | ✅ `Movimentacoes_without_authentication_redirects_to_login` (17/08) |
| `CT-AUT-11` | Lojista em Produtos, Movimentações e Ajuste | Parcial (falta Ajuste) | ⚠️ Produtos/Movimentações desde 17/08; Ajuste só em 12/10 (**pós-formal**) |
| `CT-AUT-12` | Vendedor em `/Movimentacoes/Saida` | Existente | ✅ `Vendedor_can_access_saida` (17/08) |
| `CT-AUT-13` | Vendedor em `/Produtos` | Existente | ✅ `Vendedor_is_redirected_to_access_denied_for_produtos` (17/08) |
| `CT-AUT-14` e `CT-AUT-15` | Vendedor em `GET`/`POST` de `/Movimentacoes/Ajuste` | Planejado prioritário | 🕐 `AjusteAuthorizationHttpTests` (12/10, **pós-formal**) |
| `IT-19` e `IT-20` | Consulta de estoque com e sem resultado | Dependente | ✅ `ConsultaEstoqueTests` (31/08) + capturas da Seção 6 |
| `IT-24` e `IT-25` | Resultado `Vendeu` e venda sem saldo | Existente | ✅ `VendaRapidaHttpTests` (22/09) + `EstoqueDomainPersistenceTests` |
| `IT-26` | Ruptura (`Não tinha`) | Existente | ✅ `RupturaHttpTests` e `RupturaPersistenceHttpTests` (30/09) |
| `IT-27` | Ruptura com SKU inválido | Planejado/dependente | ⚠️ Plano desatualizado: `RupturaInvalidSkuHttpTests` (01/10) |
| `IT-28` e `IT-29` | Resultado `Desistiu` e nova consulta | Dependente | ⚠️ Plano desatualizado: `DesistiuHttpTests` (01/10) |
| `IT-07` | Entrada válida | Parcial (o teste não afirma todos os campos) | ⚠️ `Entrada_registers_movement_and_increases_balance` existe; cobertura de campos continua parcial |
| `IT-08` | Entrada inválida (quantidade 0/negativa) | Planejado | ⬜ Sem caso dedicado para a entrada individual; há cobertura de lote negativa em `Entrada_lote_rejects_negative_quantity_without_changes` |
| `IT-15` | Ajuste válido | Parcial | ⚠️ `Ajuste_registers_one_traceable_movement_and_updates_balance`; cobertura ampliada em 12/10 e 22/10 (**pós-formal**) |
| `IT-16` | Ajuste sem motivo | Existente | ✅ `Ajuste_without_reason_is_rejected_by_controller` |
| `UT-11` | Validação do motivo de ajuste | Parcial (ampliar cobertura do ViewModel) | 🕐 `AjusteMotivoHttpTests` (22/10) + `ajuste-motivo.test.cjs` (27/10) |
| `NFT-07` | Responsividade (360×800, 390×844, 430×932) | Playwright visual + manual por release | ✅ Teste automatizado cobre os 3 viewports + 1280×800 (09/10, **pós-formal**) + capturas manuais em 390×844 |
| `NFT-01` a `NFT-06` | Não-funcionais (compatibilidade, desempenho, segurança, usabilidade) | Conforme especificação | ⚠️ **Parcial:** a especificação existe no arquivo histórico, mas não há execução de NFT-03/06; NFT-02 tem teste P95 **opt-in** (29/10, pós-formal) |
| `E2E-03` | Jornada de consulta e venda | Parcial | ⚠️ Aprovado em navegador local; bloqueado no ambiente publicado (`DEF-VEN-MOB-01`) |
| `IT-30` a `IT-33` e `E2E-07` | Saldos zerados e histórico de rupturas | `IT-30/31` automatizado; `IT-32/33` e `E2E-07` dependentes | 🕐 Executados só após o período formal (13–20/10) |
| *sem ID oficial* | Importação de NF-e (`/EntradasFiscais/Importar`) | Fora do catálogo do plano e do escopo do SRS | 🕐 Executada só após o período formal (11/10) |

### 4.3 Casos detalhados

#### `CT-AUT-02` — Login do lojista com credenciais válidas

- **Entrada:** usuário `lojista@squad.com` / senha `123` (seed de testes), checkbox "Lembrar de mim" desmarcado.
- **Esperado:** autenticação bem-sucedida, cookie autenticado como `LOJISTA` e `/Produtos` respondendo `200`.
- **Obtido:** login aceito e `GET /Produtos` respondendo `200` com o catálogo.
- **Evidência:** [`AuthenticationAuthorizationTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs) (criado 17/08/2026), caso `Lojista_can_login_and_access_produtos`.

#### `CT-AUT-03` — Login do vendedor com credenciais válidas

- **Entrada:** usuário `vendedor@squad.com` / senha `123` (seed de testes). Em seguida, `GET /Movimentacoes/Saida`.
- **Esperado:** autenticação como `VENDEDOR`, redirecionamento para `/Estoque/Consulta` e `/Movimentacoes/Saida` respondendo `200`.
- **Obtido:** `POST /Account/Login` retorna `302` com `Location: /Estoque/Consulta`; `GET /Movimentacoes/Saida` retorna `200`.
- **Evidência:** [`AuthenticationAuthorizationTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs) (criado 17/08/2026), casos `Vendedor_can_login` e `Vendedor_can_access_saida`.

#### `CT-AUT-04` e `CT-AUT-05` — Login com credenciais inválidas

- **Entrada:** e-mail inexistente com qualquer senha (`CT-AUT-04`); e-mail cadastrado com senha incorreta (`CT-AUT-05`).
- **Esperado:** permanência na tela de login com mensagem genérica; nenhum cookie de autenticação; rota protegida continua exigindo login.
- **Obtido:** `POST /Account/Login` retorna `200` com a mensagem "E-mail ou senha inválidos."; `GET /Produtos` continua retornando `302` para `/Account/Login?ReturnUrl=%2FProdutos`; a senha submetida não é re-renderizada. **`CT-AUT-05` não tem caso dedicado** — o teste existente usa e-mail inexistente (`CT-AUT-04`).
- **Evidência:** [`AuthenticationAuthorizationTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs), casos `Invalid_credentials_do_not_authenticate` e `Invalid_login_does_not_render_submitted_password`.

#### `CT-AUT-09` e `CT-AUT-10` — Anônimo em rota protegida

- **Entrada:** `GET /Produtos` (`CT-AUT-09`) e `GET /Movimentacoes` (`CT-AUT-10`) sem cookie de autenticação.
- **Esperado:** redirecionamento `302` para `/Account/Login?ReturnUrl=...`.
- **Obtido:** `302` para `/Account/Login?ReturnUrl=%2FProdutos` e `/Account/Login?ReturnUrl=%2FMovimentacoes`.
- **Evidência:** [`BasicRoutesTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/BasicRoutesTests.cs) (criado 17/08/2026), casos `Produtos_without_authentication_redirects_to_login` e `Movimentacoes_without_authentication_redirects_to_login`.

#### `CT-AUT-11` a `CT-AUT-15` — Autorização por perfil

- **Entrada:** sessão de `LOJISTA` em Produtos, Movimentações e Ajuste (`CT-AUT-11`); sessão de `VENDEDOR` em `/Movimentacoes/Saida` (`CT-AUT-12`) e `/Produtos` (`CT-AUT-13`); vendedor enviando `GET`/`POST` em `/Movimentacoes/Ajuste` (`CT-AUT-14`/`CT-AUT-15`).
- **Esperado:** `200` para o lojista nas três rotas; `200` para o vendedor em Saída; negação (`302` para Acesso Negado) para o vendedor em Produtos e Ajuste; saldo e histórico inalterados no Ajuste negado.
- **Obtido:** Produtos, Movimentações e Saída conformes desde 17/08; Ajuste do lojista e negação do vendedor comprovados em **12/10/2026** (pós-formal). O vendedor continua autorizado em `/Movimentacoes/Saida` (divergência LOJ-07/VEN-04 registrada na Seção 3.6, item 2).
- **Evidência:** [`AuthenticationAuthorizationTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs) + [`AjusteAuthorizationHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AjusteAuthorizationHttpTests.cs) (criado **12/10/2026**, pós-formal).

#### `IT-19` e `IT-20` — Consulta de estoque com e sem resultado

- **Entrada:** termo de busca correspondente a SKU cadastrado no seed (`IT-19`); termo `SKU-INEXISTENTE-999` (`IT-20`).
- **Esperado:** `200` com o item correspondente; `200` com estado vazio/mensagem de nenhum resultado.
- **Obtido:** `200` com o produto listado; `200` sem resultados e sem erro de servidor.
- **Evidência:** [`ConsultaEstoqueTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/ConsultaEstoqueTests.cs) + captura desktop da Seção 6.

#### `IT-24` e `IT-25` — Resultado `Vendeu` e venda sem saldo

- **Entrada:** SKU com saldo ≥ 1, quantidade 1, sessão `VENDEDOR` (`IT-24`); SKU com saldo 0 (`IT-25`).
- **Esperado:** baixa de exatamente 1 no saldo com movimentação registrada; rejeição sem movimentação quando o saldo é zero.
- **Obtido:** saída registrada e saldo atualizado; venda com saldo zero rejeitada sem movimentação.
- **Evidência:** [`VendaRapidaHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/VendaRapidaHttpTests.cs) (criado 22/09/2026, PR #31) + `EstoqueDomainPersistenceTests.cs`.

#### `IT-26` e `IT-27` — Ruptura (`Não tinha`) e ruptura com SKU inválido

- **Entrada:** SKU válido no fluxo `Não tinha` (`IT-26`); SKU inexistente/divergente no formulário de ruptura (`IT-27`).
- **Esperado:** ruptura persistida com saldo inalterado; rejeição e nenhuma persistência para SKU inválido.
- **Obtido:** ruptura registrada sem alteração de estoque; ruptura com SKU inválido rejeitada sem persistência.
- **Evidência:** [`RupturaHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/RupturaHttpTests.cs) e [`RupturaPersistenceHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/RupturaPersistenceHttpTests.cs) (30/09/2026), [`RupturaInvalidSkuHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/RupturaInvalidSkuHttpTests.cs) (criado 01/10/2026, no período formal). **Observação de rastreabilidade:** o plano ainda marca `IT-27` como "Planejado/dependente"; a evidência já existe e o plano está desatualizado nesse ponto.

#### `IT-28` e `IT-29` — Resultado `Desistiu` e nova consulta

- **Entrada:** atendimento existente com motivo de desistência (`IT-28`); nova consulta iniciada após desistir (`IT-29`).
- **Esperado:** registro da desistência sem movimentação/ruptura e retorno à busca; nova consulta permitida sem efeito no estoque.
- **Obtido:** desistência registrada conforme esperado e nova consulta desbloqueada.
- **Evidência:** [`DesistiuHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/DesistiuHttpTests.cs) (criado 01/10/2026). **Observação de rastreabilidade:** o plano ainda marca `IT-28/IT-29` como "Dependente"; a evidência já existe.

#### `IT-07` e `IT-08` — Entrada válida e inválida

- **Entrada:** lista de itens com SKU e quantidade, sessão `LOJISTA` (`IT-07`; a modalidade em lote usa `/Movimentacoes/EntradaLote` e não tem ID dedicado no catálogo); quantidade 0 ou negativa (`IT-08`).
- **Esperado:** sucesso com aumento de saldo e movimentação registrada; rejeição sem alteração para quantidade inválida.
- **Obtido:** `Entrada_registers_movement_and_increases_balance` comprova o aumento de saldo e a movimentação, mas **não afirma todos os campos** exigidos pelo plano — status **Parcial** mantido. Para `IT-08`, **não há caso dedicado da entrada individual**; existe apenas cobertura da entrada em lote com quantidade negativa (`Entrada_lote_rejects_negative_quantity_without_changes`), que não substitui o caso do plano.
- **Evidência:** [`EstoqueDomainPersistenceTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/EstoqueDomainPersistenceTests.cs).

#### `IT-15`, `IT-16` e `UT-11` — Ajuste de saldo e validação do motivo

- **Entrada:** SKU com saldo existente e ajuste informado, sessão `LOJISTA` (`IT-15`); ajuste sem motivo (`IT-16`); motivo vazio, só espaços e texto válido (`UT-11`).
- **Esperado:** saldo ajustado com movimentação `AJUSTE` rastreável; rejeição quando o motivo é ausente/inválido.
- **Obtido:** ajuste aplicado conforme esperado; motivos vazios rejeitados. `IT-15` e `UT-11` permanecem **Parciais no plano**; a cobertura foi ampliada **após o período formal** por `AjusteMotivoHttpTests` (22/10) e `ajuste-motivo.test.cjs` (27/10).
- **Evidência:** [`EstoqueDomainPersistenceTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/EstoqueDomainPersistenceTests.cs) + (pós-formal) [`AjusteMotivoHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AjusteMotivoHttpTests.cs), criado **22/10/2026** — casos `Motivo_invalido_rejeita_ajuste_sem_alterar_saldos_ou_historico` e `Motivo_valido_persiste_normalizado_com_diferenca_usuario_e_data`. Cobertura ampliada pelos PRs [#69](https://github.com/CodeWitch00/SQUAD-estoque/pull/69) e [#71](https://github.com/CodeWitch00/SQUAD-estoque/pull/71).

#### `NFT-07` — Responsividade (360×800, 390×844 e 430×932)

- **Entrada:** viewports oficiais **360×800**, **390×844** e **430×932** nas telas de consulta e venda (o teste também cobre 1280×800 como referência desktop).
- **Esperado:** layout sem overflow horizontal, elementos acessíveis ao toque (alvos ≥ 44 px).
- **Obtido:** teste automatizado aprovado nos três viewports mobile + desktop; capturas manuais registradas em 390×844 (Seção 5).
- **Evidência:** [`tests/frontend/consulta-responsividade.test.cjs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/frontend/consulta-responsividade.test.cjs) (criado 09/10/2026, pós-formal) + capturas mobile da Seção 5.

#### `NFT-01` a `NFT-06` — Não-funcionais (compatibilidade/desempenho/segurança/usabilidade)

- **Entrada:** critérios de tempo de resposta, headers, usabilidade e acessibilidade do [plano de testes](../09-testing/plano-de-testes.md).
- **Esperado:** evidência automatizada de responsividade + registro manual de validações.
- **Obtido:** **parcial.** A especificação [`especificacao-testes-nao-funcionais.md`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/docs/09-testing/especificacoes/arquivo-historico/especificacao-testes-nao-funcionais.md) **existe** no arquivo histórico, mas não há execução registrada para `NFT-03` (disponibilidade) e `NFT-06` (usabilidade). Para `NFT-02`, existe `ConsultaP95Tests` (29/10, pós-formal), porém o teste de performance é **opt-in** (`RNF02_PERF=1`) e fica `Skip` por padrão na CI — portanto **não há medição P95 registrada na P1**.
- **Evidência:** [`tests/frontend/consulta-responsividade.test.cjs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/frontend/consulta-responsividade.test.cjs) + [`registro-validacoes-manuais.md`](../09-testing/homologacoes/registro-validacoes-manuais.md) + [`ConsultaP95Tests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/ConsultaP95Tests.cs). **Pendência registrada na Seção 9.**

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

- **Viewports oficiais do `NFT-07`:** **360×800**, **390×844** e **430×932** (plano de testes). O registro manual usa **390×844** como viewport de captura; o teste automatizado cobre os três viewports oficiais e também 1280×800 como referência desktop.
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
| PROT-002 | `docs/05-ux/prototipos/prototipo-supervisor.html` | ⬜ Não existe no repositório |
| PROT-003 | [`docs/05-ux/prototipos/prototipo-vendedor.html`](prototipos/prototipo-vendedor.html) | ✅ Versão versionada no repositório |
| PROT-004 | `docs/05-ux/prototipos/prototipo-gerente.html` | ⬜ Não existe no repositório |
| PROT-005 | `docs/05-ux/prototipos/prototipo-funcionario.html` | ⬜ Não existe no repositório |
| PROT-006 | [`docs/05-ux/prototipos/login.html`](prototipos/login.html) | ✅ Versão versionada no repositório |

> **Verificado em `main`:** `docs/05-ux/prototipos/` contém apenas `login.html`, `prototipo-lojista.html`, `prototipo-vendedor.html` e `README.md`. Os protótipos de supervisor, gerente e funcionário são **pendência** (Seção 9). A publicação dos protótipos foi registrada no PR [#28](https://github.com/CodeWitch00/SQUAD-estoque/pull/28) (18/09/2026).

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

As capturas mobile da Seção 5 são o registro mobile canônico deste pacote (viewport de captura 390×844; os três viewports oficiais do `NFT-07` estão cobertos pelo teste automatizado).

### 6.5 Pendências de captura

Conforme [`evidencias/README.md`](evidencias/README.md) (5 registros):

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

> **Observações:** (1) não existe `.sln` na raiz — `dotnet test` puro falha com `MSB1003`; os caminhos acima são os da CI. (2) O `package.json` possui apenas o script `test:frontend`. Não existem `pytest`, `npm run build` nem diretório `docs/03-testes/`.

### 7.2 Arquivos de teste existentes na `main` (`7d76434`) — inventário completo

> A `main` atual (`7d76434`, 29/10/2026) contém **31 arquivos `.cs` de teste no backend** (30 classes de teste + 1 fábrica), **163 atributos `[Fact]`/`[Theory]`** e **4 arquivos de teste frontend**. Coluna "Período": **base** = anterior à Sprint 2 (até 09/09/2026), **formal** = 10/09 a 07/10/2026, **pós** = após 07/10/2026.

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
| [`ConsultaIndexesMigrationTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/ConsultaIndexesMigrationTests.cs) | 29/10/2026 | 🕐 pós |
| [`ConsultaP95Tests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/ConsultaP95Tests.cs) | 29/10/2026 | 🕐 pós |
| [`ConsultaQueryPerformanceTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/ConsultaQueryPerformanceTests.cs) | 29/10/2026 | 🕐 pós |
| [`UltimaAtualizacaoSaldoHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/UltimaAtualizacaoSaldoHttpTests.cs) | 29/10/2026 | 🕐 pós |

Infraestrutura (não é caso de teste): [`SquadEstoqueWebApplicationFactory.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/SquadEstoqueWebApplicationFactory.cs) (17/08/2026) + [`test.runsettings`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/test.runsettings).

> **Ampliação pós-formal:** o [`RupturasHistoricoHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/RupturasHistoricoHttpTests.cs) foi ampliado em 29/10/2026 (PR [#81](https://github.com/CodeWitch00/SQUAD-estoque/pull/81)) com o caso `Historico_http_exibe_contagem_igual_aos_registros_persistidos`, que comprova o VNP-05 (9 rupturas persistidas; contagens 5, 3 e 1; `GET /Rupturas` → `200 OK`).

**Frontend — `tests/frontend/`**

| Arquivo | Criado em | Período | Casos |
|---|---|---|---|
| [`consulta-nao-tinha.test.cjs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/frontend/consulta-nao-tinha.test.cjs) | 28/09/2026 | ✅ formal | 5 testes (busca, seleção de numeração, venda, "Não tinha", bloqueio de envio) |
| [`consulta-desistiu.test.cjs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/frontend/consulta-desistiu.test.cjs) | 02/10/2026 | ✅ formal | 2 testes (resposta inválida, falha na nova busca) |
| [`consulta-responsividade.test.cjs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/frontend/consulta-responsividade.test.cjs) | 09/10/2026 | 🕐 pós | `NFT-07` (responsividade; 360×800, 390×844, 430×932 e 1280×800) |
| [`ajuste-motivo.test.cjs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/frontend/ajuste-motivo.test.cjs) | 27/10/2026 | 🕐 pós | Frontend do motivo de ajuste (complementa `IT-15`/`IT-16`) |

**Resumo:** **163 atributos `[Fact]`/`[Theory]`** no backend (31 arquivos `.cs`, sendo 30 classes de teste + 1 fábrica) e **4 arquivos** de teste frontend. Apenas **7 arquivos** (5 backend + 2 frontend) foram criados **dentro do período formal**; a maior parte da suíte é de base anterior ou de evolução pós-07/10 — transparência registrada para não confundir cobertura atual com cobertura da P1.

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
- Observação: os 274 incluem os testes criados **após** 07/10/2026 (Seção 7.2) — a suíte atual **não** equivale à cobertura da P1 em 07/10; usar a coluna *Período* para separar o que existia na entrega formal.
- **Esta execução não foi repetida em 29/10/2026:** o ambiente usado nesta revisão não possui o SDK `dotnet` instalado. O número acima permanece identificado pela data e pelo commit `8c94b36`; a `main` atual (`7d76434`) tem 163 atributos `[Fact]`/`[Theory]` e não foi reexecutada localmente.
- Testes frontend (`npm run test:frontend`) não executados nesta conferência local (exigem Chromium do Playwright); cobertos pelo job `frontend-tests` da CI.

---

## 8. Rastreabilidade com os PRs Entregues

> Os títulos abaixo foram conferidos na API do GitHub em 29/10/2026. Todas as URLs usam `github.com/CodeWitch00/SQUAD-estoque/pull/N`. As relações foram preenchidas com base nas datas reais de merge de cada PR no período formal (10/09–07/10/2026).

| PR | Título | Merge | Relação com o S2-DOC-038 |
|---|---|---|---|
| [#25](https://github.com/CodeWitch00/SQUAD-estoque/pull/25) | [S2-BE-007] Reutilizar saída de estoque para venda de exatamente um par | 16/09/2026 | Backend de VEN-04/`IT-24` (baixa de exatamente 1 par) |
| [#28](https://github.com/CodeWitch00/SQUAD-estoque/pull/28) | docs: atualizar acesso aos protótipos publicados | 18/09/2026 | Publicação dos protótipos citados na Seção 6 |
| [#29](https://github.com/CodeWitch00/SQUAD-estoque/pull/29) | feat: busca tolerante e fluxo mobile até a grade | 18/09/2026 | Busca de VEN-02 e base de `IT-19`/`IT-20` |
| [#31](https://github.com/CodeWitch00/SQUAD-estoque/pull/31) | test: comprovar venda rapida sem alterar outros SKUs | 22/09/2026 | Evidência de `IT-24` (`VendaRapidaHttpTests`) |
| [#32](https://github.com/CodeWitch00/SQUAD-estoque/pull/32) | test: comprovar rejeicao de venda rapida sem saldo | 26/09/2026 | Evidência de `IT-25` |
| [#34](https://github.com/CodeWitch00/SQUAD-estoque/pull/34) | test: S2-QA-013 comprovar autorização da venda rápida | 24/09/2026 | Evidência de `CT-AUT-12`/`IT-24` |
| [#35](https://github.com/CodeWitch00/SQUAD-estoque/pull/35) | [S2-FE-016] Atualizar grade com saldo confirmado após venda | 24/09/2026 | Atualização da grade (VEN-03) |
| [#38](https://github.com/CodeWitch00/SQUAD-estoque/pull/38) | fix: concluir acao Nao tinha na grade do vendedor | 29/09/2026 | Fluxo VEN-05 |
| [#39](https://github.com/CodeWitch00/SQUAD-estoque/pull/39) | test: S2-QA-024 saldo preservado apos ruptura | 01/10/2026 | Evidência de `IT-26` |
| [#40](https://github.com/CodeWitch00/SQUAD-estoque/pull/40) | test: S2-QA-025 autorizacao do endpoint de ruptura | 01/10/2026 | Evidência de autorização de `IT-27` |
| [#41](https://github.com/CodeWitch00/SQUAD-estoque/pull/41) | feat: confirmar ruptura e continuar atendimento (S2-FE-027) | 01/10/2026 | Fluxo VEN-05 |
| [#42](https://github.com/CodeWitch00/SQUAD-estoque/pull/42) | test: rejeicao segura de ruptura com SKU invalido ou divergente | 03/10/2026 | Evidência de `IT-27` |
| [#44](https://github.com/CodeWitch00/SQUAD-estoque/pull/44) | test: validar fluxo Desistiu sem movimentação (S2-QA-031) | 01/10/2026 | Evidência de `IT-28` |
| [#45](https://github.com/CodeWitch00/SQUAD-estoque/pull/45) | test: validar nova consulta sem desfecho S2-QA-032 | 03/10/2026 | Evidência de `IT-29` |
| [#46](https://github.com/CodeWitch00/SQUAD-estoque/pull/46) | feat: integrar Desistiu a nova consulta (S2-FE-030) | 04/10/2026 | Fluxo VEN-06 |
| [#49](https://github.com/CodeWitch00/SQUAD-estoque/pull/49) | test: adaptar cobertura de Desistiu ao POST atual (S2-QA-031) | 04/10/2026 | Evidência de `IT-28` |

**PRs pós-período formal (contexto — fora da janela da P1):**

| PR | Título | Merge/Estado | Relação com o S2-DOC-038 |
|---|---|---|---|
| [#53](https://github.com/CodeWitch00/SQUAD-estoque/pull/53) | fix: decremento atomico nas saidas de estoque (S3-BE-001) | merge 09/10/2026 (na `main`) | Base transacional de `IT-24`/`IT-25` |
| [#56](https://github.com/CodeWitch00/SQUAD-estoque/pull/56) | test: validar venda simultânea repetida S3-QA-003 | merge 12/10/2026 (na `main`) | Evidência de `IT-18` |
| [#58](https://github.com/CodeWitch00/SQUAD-estoque/pull/58) | test: validar autorização do ajuste de estoque (S3-QA-021) | merge 16/10/2026 (na `main`) | `AjusteAuthorizationHttpTests` (`CT-AUT-14`/`CT-AUT-15`) |
| [#60](https://github.com/CodeWitch00/SQUAD-estoque/pull/60) | feat: criar ViewModel de saldos zerados (S3-BE-007) | merge 16/10/2026 (na `main`) | Base de `IT-30`/`IT-31` |
| [#62](https://github.com/CodeWitch00/SQUAD-estoque/pull/62) | test: comprovar imutabilidade de movimentacoes pela aplicacao | merge 16/10/2026 (na `main`) | Imutabilidade das movimentações |
| [#64](https://github.com/CodeWitch00/SQUAD-estoque/pull/64) | feat: histórico de rupturas do lojista | merge 20/10/2026 (na `main`) | View `/Rupturas` (LOJ-11) |
| [#65](https://github.com/CodeWitch00/SQUAD-estoque/pull/65) | feat: agregar historico de rupturas por SKU (S3-BE-014) | merge 20/10/2026 (na `main`) | Base de `IT-32`/`E2E-07` |
| [#69](https://github.com/CodeWitch00/SQUAD-estoque/pull/69) | test: validar motivo e rastreabilidade do ajuste (S3-QA-020) | merge 25/10/2026 (na `main`) | `AjusteMotivoHttpTests` (`UT-11`/`IT-15`) |
| [#70](https://github.com/CodeWitch00/SQUAD-estoque/pull/70) | docs: consolidar evidências do histórico de rupturas | merge 25/10/2026 (na `main`) | Atualizou `registro-validacoes-manuais.md` (capturas de 20/10, commit `381224d`) |
| [#71](https://github.com/CodeWitch00/SQUAD-estoque/pull/71) | fix: consolidar validacao do motivo de ajuste | merge 27/10/2026 (na `main`) | `docs/09-testing/ajuste-motivo.md` + JSONs de evidência + novos casos de `IT-15`/`UT-11` |
| [#72](https://github.com/CodeWitch00/SQUAD-estoque/pull/72) | feat: registrar ultima alteracao de saldo do SKU (S3-BE-023) | merge 27/10/2026 (na `main`) | `SkuUltimaAtualizacaoMigrationTests`/`UltimaAtualizacaoSaldoHttpTests` |
| [#73](https://github.com/CodeWitch00/SQUAD-estoque/pull/73) | test: validacao-navegacao-perfil-desktop-mobile (S2-QA-035) | fechado em 29/10/2026, sem merge | Conteúdo reenviado no [#81](https://github.com/CodeWitch00/SQUAD-estoque/pull/81), mergeado em 29/10/2026 |
| [#75](https://github.com/CodeWitch00/SQUAD-estoque/pull/75) | test: cobrir desempates do histórico de rupturas (S3-BE-015) | merge 27/10/2026 (na `main`) | Ampliou `RupturasControllerTests.cs` — evidência de `IT-32`/`E2E-07` |
| [#77](https://github.com/CodeWitch00/SQUAD-estoque/pull/77) | test: auditar aumento e reducao do ajuste (S3-QA-022) | merge 27/10/2026 (na `main`) | Ampliou `EstoqueDomainPersistenceTests.cs` — evidência de `IT-15` |
| [#81](https://github.com/CodeWitch00/SQUAD-estoque/pull/81) | test: validacao-navegacao-perfil-desktop-mobile (S2-QA-035) | merge 29/10/2026 (na `main`) | Reentrega do #73; relatório e capturas na `main` — 15 de 15 cenários aprovados, com VNP-05 conferido por teste HTTP |
| [#83](https://github.com/CodeWitch00/SQUAD-estoque/pull/83) | test: validar última atualização do saldo via HTTP [S3-QA-025] | merge 29/10/2026 (na `main`) | `UltimaAtualizacaoSaldoHttpTests.cs` |
| [#84](https://github.com/CodeWitch00/SQUAD-estoque/pull/84) | perf: reduzir custo da busca e grade do vendedor | merge 29/10/2026 (na `main`) | Otimização da consulta do vendedor |
| [#85](https://github.com/CodeWitch00/SQUAD-estoque/pull/85) | test: medir P95 da consulta para RNF-02 (S3-QA-028) | merge 29/10/2026 (na `main`) | `ConsultaP95Tests.cs` (`NFT-02`, opt-in) |
| [#86](https://github.com/CodeWitch00/SQUAD-estoque/pull/86) | test: integrar medição P95 da consulta (S3-QA-028) | merge 29/10/2026 (na `main`) | Integração da medição P95 |
| [#87](https://github.com/CodeWitch00/SQUAD-estoque/pull/87) | perf(db): criar índices de consulta para produtos, saldos e rupturas (S3-DB-026) | merge 29/10/2026 (na `main`) | `ConsultaIndexesMigrationTests.cs` |

> **Estado da `main` em 29/10/2026:** `7d76434` (`docs: formatar tabelas do README`, 19:14). As contagens de teste da Seção 7.4 referem-se a `8c94b36`.

---

## 9. Itens Pendentes e Falhas Conhecidas

> Lista transparente das pendências **reais e documentadas** no repositório.

| # | Item | Status | Evidência |
|---|---|---|---|
| 1 | Homologação em ambiente Azure publicado | **Bloqueada** — homologação travada por versão de grade desatualizada | [`registro-validacoes-manuais.md`](../09-testing/homologacoes/registro-validacoes-manuais.md) |
| 2 | 3 capturas de protótipo ainda pendentes (`lojista-desktop.png`, `lojista-tablet.png`, `vendedor-mobile.png`); 2 já registradas (`vendedor-s2-*`) | Parcial — 2 de 5 concluídas | [`docs/05-ux/evidencias/README.md`](evidencias/README.md) |
| 3 | Protótipos de supervisor, gerente e funcionário (`prototipo-supervisor.html`, `prototipo-gerente.html`, `prototipo-funcionario.html`) **não existem** no repositório | Pendente | `docs/05-ux/prototipos/` contém apenas `login.html`, `prototipo-lojista.html`, `prototipo-vendedor.html` |
| 4 | Validação de navegação por perfil (S2-QA-035): o relatório do PR [#81](https://github.com/CodeWitch00/SQUAD-estoque/pull/81), mergeado em 29/10/2026, registra **15 cenários: 15 aprovados, 0 reprovados, 0 não executados e 0 em execução**. Os cenários VNP-12, VNP-14 e VNP-15 foram concluídos na execução complementar de 27/10/2026, e o VNP-05 foi concluído por conferência HTTP automatizada: 9 rupturas persistidas resultam nas contagens 5, 3 e 1, e `GET /Rupturas` responde `200 OK` | **Resolvido — na `main` desde 29/10/2026** | PR #81 (relatório `docs/09-testing/execucoes/sprint-2/validacao-navegacao-perfil-desktop-mobile.md`, Seções 8, 9 e 13, item 15) e [`RupturasHistoricoHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/RupturasHistoricoHttpTests.cs), caso `Historico_http_exibe_contagem_igual_aos_registros_persistidos` |
| 5 | Divergência de viewport: o relatório do S2-QA-035 (PR [#81](https://github.com/CodeWitch00/SQUAD-estoque/pull/81)) usa **390×844** nas capturas iniciais (Chrome DevTools) e **375×812** na execução complementar (Playwright, `deviceScaleFactor` 2), enquanto a Seção 5 deste pacote padroniza **390×844** para captura e o `NFT-07` exige **360×800/390×844/430×932**. A distinção está declarada nas Seções 5.1, 12 e 13 (item 9) do relatório | Registrada e documentada — não padronizada entre artefatos | PR #81 (Seções 5.1, 12 e 13 do relatório) |
| 6 | Cobertura adicional de `IT-15`/`IT-16` trazida pelos PRs [#69](https://github.com/CodeWitch00/SQUAD-estoque/pull/69) e [#71](https://github.com/CodeWitch00/SQUAD-estoque/pull/71) (merges de 25/10 e 27/10/2026): `docs/09-testing/ajuste-motivo.md` + JSONs de evidência (`evidencias/ajuste-motivo/*.json`) + novos casos de normalização em `AjusteMotivoHttpTests.cs` | **Resolvido — na `main` desde 27/10/2026** | PRs #69 e #71 (merges) |
| 7 | Teste em dispositivo físico (não apenas viewport/emulador) | Não realizado | [`registro-validacoes-manuais.md`](../09-testing/homologacoes/registro-validacoes-manuais.md) |
| 8 | `NFT-01` a `NFT-06` — a especificação **existe** em [`especificacao-testes-nao-funcionais.md`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/docs/09-testing/especificacoes/arquivo-historico/especificacao-testes-nao-funcionais.md), mas não há execução registrada de `NFT-03` (disponibilidade) nem `NFT-06` (usabilidade); `NFT-02` tem teste P95 **opt-in** (`RNF02_PERF=1`), que fica `Skip` por padrão na CI | Parcial — evidência restrita a responsividade + registro manual; sem medição P95 na P1 | Seção 4.3 (`NFT-01` a `NFT-06`) e `ConsultaP95Tests.cs` |
| 9 | Importação de NF-e (sem ID oficial no catálogo) e `IT-30` a `IT-33` (saldos zerados e histórico de rupturas) **foram executados apenas após o período formal** (11/10 a 20/10) — não havia cobertura em 07/10/2026 | 🕐 Pós-formal — evidência existe, fora da janela da P1 | Seções 3.4/3.5/4.2 e Seção 7.2 |
| 10 | Tela de recuperação de senha sem código no inventário oficial e sem implementação | Não implementada | `Views/Account/` não possui `RecuperarSenha.cshtml` (Seção 3.5) |
| 11 | Códigos `SUP` no inventário são telas de apoio (SUP-01 = `/Account/AccessDenied`, SUP-02 = `/Home/Error`); não existe perfil supervisor em nenhum `[Authorize]` nem tela de relatórios | Registrado — sem fluxo SUP na aplicação | Seções 3.5 e 3.6 |
| 12 | Divergência LOJ-07/VEN-04: `/Movimentacoes/Saida` aceita `LOJISTA,VENDEDOR` (a saída administrativa é do lojista e o vendedor teria apenas `Vendeu`) | Registrada, não resolvida | [`MovimentacoesController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/MovimentacoesController.cs) |
| 13 | Capturas das telas pós-formais (Atendimentos, NF-e import, SaldosZerados, Rupturas) | Pendente | Seção 6.5 |

---

## 10. Checklist de Integridade do Pacote

- [x] Índice versionado do pacote P1 criado neste arquivo (`docs/05-ux/S2-DOC-038-evidencias-p1.md`)
- [x] Inventário de todas as telas MVP com rota real verificada no código
- [x] Códigos de tela (`AUT`/`VEN`/`LOJ`/`SUP`) usados **com o significado oficial** do [`inventario-telas-e-mapa-navegacao.md`](inventario-telas-e-mapa-navegacao.md); telas sem código oficial rotuladas como tal
- [x] `/Rupturas` e `/Atendimentos` classificados como **exclusivos do `LOJISTA`**, conforme o `[Authorize(Roles = "LOJISTA")]` dos controllers na `main`; `/Rupturas` classificada como 🕐 (view criada em 20/10/2026, após o encerramento da P1 em 07/10)
- [x] Datas de criação das views conferidas com `git log --diff-filter=A`: as views principais de `AUT`/`LOJ`/`VEN` são **base** (17/08–31/08); as duas views formais são `Movimentacoes/EntradaLote.cshtml` e `Produtos/AdicionarNumeracoes.cshtml` (03/10); `SaldosZerados`, `Rupturas`, `Atendimentos` e `EntradasFiscais` são pós-formais
- [x] Casos de teste com os **IDs oficiais** do [`plano-de-testes.md`](../09-testing/plano-de-testes.md) (`CT-AUT`/`IT`/`E2E`/`NFT`/`UT`), com entrada, esperado, obtido e evidência; tabela de correspondência com os identificadores de registro (CT-001..CT-015) na Seção 4.1
- [x] Status de cada caso alinhado ao plano oficial (`Existente`/`Parcial`/`Planejado`/`Dependente`), com a evidência real ao lado — inclusive quando o plano está desatualizado (`IT-27`/`IT-28`/`IT-29`)
- [x] NF-e (sem ID oficial) e `IT-30` a `IT-33` declarados **executados apenas após o período formal** (evidência datada de 11/10 a 20/10, fora da janela da P1)
- [x] Inventário de testes completo na `main` `7d76434`: **31 arquivos `.cs` de backend (30 classes + 1 fábrica), 163 atributos `[Fact]`/`[Theory]` e 4 arquivos frontend**, com data e rótulo *base*/*formal*/*pós*
- [x] PRs de entrega e de evolução pós-formal incluídos com estado real; nenhum arquivo de PR aberto citado como existente na `main`
- [x] Status do S2-QA-035 conferido no relatório mergeado (PR #81): **15 de 15 cenários aprovados**, com VNP-05 comprovado por teste HTTP
- [x] `NFT-01` a `NFT-06` declarados **parciais**; a existência da especificação no arquivo histórico é reconhecida e a ausência de execução de `NFT-02`/`NFT-03`/`NFT-06` é registrada
- [x] Viewports do `NFT-07` definidos em **360×800, 390×844 e 430×932**; capturas manuais seguem em 390×844
- [x] Capturas mobile apontam para as imagens mobile reais (não reutilizam as desktop)
- [x] Comandos de execução reais (`dotnet test`, `npm run test:frontend`) e CI `.github/workflows/dotnet.yml`
- [x] Relações de PR preenchidas com datas reais de merge
- [x] Links de testes, PRs e capturas verificados; `Error.cshtml` aponta para `Views/Shared/Error.cshtml`
- [ ] **Pendências da Seção 9 não resolvidas por este card** (declaradas, não mascaradas)
- [ ] Links das capturas desktop/mobile dependem de anexos GitHub user-attachment — podem expirar; pendência de migração para [`docs/05-ux/evidencias/`](evidencias/) (item da Seção 9)

---

## 11. Registro de Conferência da Documentação

| Etapa | Data | Status |
|---|---|---|
| Auditoria do documento × código real (rotas, views, datas) | 26/10/2026 | ✅ Concluída |
| Inventário de telas e casos de teste com os IDs oficiais do plano | 26/10/2026 | ✅ Concluída |
| Inventário completo de testes e evidências pós-formais | 27/10/2026 | ✅ Concluída |
| Referências ao S2-QA-035 e viewport documentada | 29/10/2026 | ✅ Concluída |
| Auditoria contra a `main` `7d76434`: datas de criação das views, status reais do plano, inventário de testes, títulos e estados dos PRs, links, especificação não funcional e viewports do `NFT-07` | 29/10/2026 | ✅ Concluída |
| Conferência do relatório do S2-QA-035 (PR #81) na `main`: 15/15 cenários aprovados e VNP-05 com verificação HTTP | 29/10/2026 | ✅ Concluída |
| Conferência local da suíte (`274/274 aprovados` em `8c94b36`) | 27/10/2026 | ✅ Seção 7.4 (não reexecutada em 29/10 — SDK `dotnet` indisponível) |
| Verificação de links e integridade das tabelas | 29/10/2026 | ✅ 197 links verificados; 0 quebrados; 0 tabelas irregulares |
| Conferência final e envio para revisão no PR #88 | 29/10/2026 | ✅ Concluída |

**Conferência final:** o pacote P1 está consolidado neste índice versionado, alinhado às fontes normativas (inventário de telas e plano de testes) e com divergências e pendências declaradas nas Seções 3.6 e 9. Nenhuma funcionalidade planejada foi declarada como implementada; nenhuma monografia foi duplicada neste card.
