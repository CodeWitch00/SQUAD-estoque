# Relatório curto de evidências : Sprint 1

Este relatório consolida evidências já versionadas para avaliação, dailies e futura atualização da monografia. Ele não reescreve capítulos da monografia e não registra credenciais, banco local, cookies, tokens ou capturas com dados sensíveis.

## Demonstração
Protótipo navegável: https://codewitch00.github.io/SQUAD-estoque/
Fluxos e arquivos de interface: docs/05-ux/prototipos/README.md
Repositório Github: https://github.com/CodeWitch00/SQUAD-estoque.git 

## Resumo executivo

| Área | Resultado real |
|---|---|
| Protótipos | Login compartilhado, fluxo do vendedor e fluxo do lojista versionados em HTML navegável. |
| UX e navegação | Inventário de telas, mapa de navegação e especificação mobile registrados. |
| Testes | Plano estruturado, especificações por área e suíte xUnit ativa. |
| Incremento MVC | Consulta operacional do vendedor implementada no fluxo `GET /Estoque/Consulta`, com busca, seleção de produto e grade visual por numeração. |
| Perfis | Login redireciona `VENDEDOR` para consulta operacional e `LOJISTA` para área administrativa; menus são filtrados por perfil. |
| Validação automatizada | `dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj`: 60 testes aprovados, 0 falhas, em 03/09/2026. |
| CI | Workflow `.NET Build and Tests` versionado em [dotnet.yml](../../.github/workflows/dotnet.yml), com build, testes e cobertura mínima de 50%. A conferência do status remoto mais recente deve ser feita na página de Actions após cada push. |

## Evidências principais

| Tipo | Evidência |
|---|---|
| Protótipo do login | [docs/05-ux/prototipos/login.html](../05-ux/prototipos/login.html) |
| Protótipo do vendedor | [docs/05-ux/prototipos/vendedor/prototipo-vendedor.html](../05-ux/prototipos/vendedor/prototipo-vendedor.html) |
| Protótipo do lojista | [docs/05-ux/prototipos/lojista/prototipo-lojista.html](../05-ux/prototipos/lojista/prototipo-lojista.html) |
| Acesso publicado dos protótipos | [GitHub Pages](https://codewitch00.github.io/SQUAD-estoque/) |
| Inventário e mapa | [inventario-telas-e-mapa-navegacao.md](../05-ux/inventario-telas-e-mapa-navegacao.md), [mapa-navegacao-mvp.svg](../05-ux/mapa-navegacao-mvp.svg) |
| Especificação mobile | [especificacao-componentes-mobile.md](../05-ux/especificacao-componentes-mobile.md) |
| Plano de testes | [plano-de-testes.md](plano-de-testes.md) |
| Testes de autenticação | [especificacao-testes-autenticacao.md](especificacao-testes-autenticacao.md), [AuthenticationAuthorizationTests.cs](../../tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs) |
| Testes administrativos | [especificacao-teste-administrativo.md](especificacao-teste-administrativo.md), [AdministrativeHttpTests.cs](../../tests/SquadEstoque.Web.Tests/AdministrativeHttpTests.cs), [EstoqueDomainPersistenceTests.cs](../../tests/SquadEstoque.Web.Tests/EstoqueDomainPersistenceTests.cs) |
| Testes do vendedor | [especificacao-testes-vendedor.md](especificacao-testes-vendedor.md), [ConsultaEstoqueTests.cs](../../tests/SquadEstoque.Web.Tests/ConsultaEstoqueTests.cs), [EstoqueControllerHttpTests.cs](../../tests/SquadEstoque.Web.Tests/EstoqueControllerHttpTests.cs), [ConsultaOperacionalHttpTests.cs](../../tests/SquadEstoque.Web.Tests/ConsultaOperacionalHttpTests.cs) |
| Validação manual administrativa | [relatorio-homologacao-baseline-administrativa-2026-08-30.md](relatorio-homologacao-baseline-administrativa-2026-08-30.md) |
| Validação manual da consulta | [validacao-consulta-vendedor-2026-08-31.md](validacao-consulta-vendedor-2026-08-31.md) |
| Validação de navegação por perfil | [validacao-navegacao-por-perfil-2026-09-03.md](validacao-navegacao-por-perfil-2026-09-03.md) |
| CI | [GitHub Actions — .NET Build and Tests](https://github.com/CodeWitch00/SQUAD-estoque/actions/workflows/dotnet.yml) |

## PRs, branches e commits rastreáveis

| Evidência | Link |
|---|---|
| PR #1 — fluxo do lojista | https://github.com/CodeWitch00/SQUAD-estoque/pull/1 |
| PR #2 — inventário/mapa de UX | https://github.com/CodeWitch00/SQUAD-estoque/pull/2 |
| PR #12 — atualização da `main` em `dev/nicolas` | https://github.com/CodeWitch00/SQUAD-estoque/pull/12 |
| Branch da grade operacional | https://github.com/CodeWitch00/SQUAD-estoque/tree/feat/s1-be-019-emmy |
| Branch de teste da grade | https://github.com/CodeWitch00/SQUAD-estoque/tree/test/s1-qa-022-emmy |
| Commit de fortalecimento do teste da grade | https://github.com/CodeWitch00/SQUAD-estoque/commit/459a154 |
| Workflow de CI | https://github.com/CodeWitch00/SQUAD-estoque/blob/main/.github/workflows/dotnet.yml |

## Conferência dos 24 cartões

| Cartão | Responsável registrado | Resultado real | Evidência |
|---|---|---|---|
| S1-UX-001 | Rayana | Concluído. Inventário e mapa de navegação versionados. | [inventário](../05-ux/inventario-telas-e-mapa-navegacao.md), [mapa](../05-ux/mapa-navegacao-mvp.svg), PR #2 |
| S1-UX-002 | pz69766-blip; revisão Rayana | Concluído. Protótipo navegável do vendedor revisado e atualizado. | [protótipo vendedor](../05-ux/prototipos/vendedor/prototipo-vendedor.html), commits `b35023b`, `a203217`, `19038bf`, `ce78f33` |
| S1-UX-003 | Emmy; complemento Rayana | Concluído. Fluxo/protótipo administrativo do lojista disponível. | [protótipo lojista](../05-ux/prototipos/lojista/prototipo-lojista.html), PR #1, commits `7af0c2d`, `a365ff2` |
| S1-QA-004 | Rayana | Concluído. Plano de testes estruturado versionado. | [plano de testes](plano-de-testes.md), commit `4417aad` |
| S1-QA-005 | Rayana | Concluído. Casos de autenticação, sessão e perfis especificados. | [especificação de autenticação](especificacao-testes-autenticacao.md), commit `1ca0697` |
| S1-QA-006 | Rayana; colaboração Emmy | Concluído. Casos administrativos especificados. | [especificação administrativa](especificacao-teste-administrativo.md), commits `73e9cc8`, `4244ce8`, `fbd95e1` |
| S1-QA-007 | Felipe; evidência versionada por pz69766-blip/Rayana | Concluído como especificação. Casos do vendedor documentados; execução total depende dos endpoints futuros. | [especificação vendedor](especificacao-testes-vendedor.md), commits `1a43831`, `3566ba9` |
| S1-QA-008 | Rayana | Concluído como especificação. Testes não funcionais definidos; execução de carga, HTTPS e mobile real permanece planejada. | [testes não funcionais](especificacao-testes-nao-funcionais.md), commit `5e48596` |
| S1-UX-009 | Nicolas | Concluído. Diretrizes de identidade visual adicionadas. | [decisões de interface](../05-ux/decisoes/decisoes-de-interface.md), commit `8b33fff` |
| S1-UX-010 | Emmy; revisão Rayana | Concluído. Componentes mobile de busca, resultados e grade especificados. | [especificação mobile](../05-ux/especificacao-componentes-mobile.md), commits `0a8425f`, `7f17175`, `f39b1b1` |
| S1-DOC-011 | Felipe; evidência versionada por pz69766-blip | Concluído. Padrão de evidências registrado. | [README de evidências](../05-ux/evidencias/README.md), commit `ee84a1b` |
| S1-QA-012 | Rayana | Concluído com impedimento registrado. Homologação administrativa executada; 13 cenários aprovados e 1 reprovado por autorização. | [relatório de homologação](relatorio-homologacao-baseline-administrativa-2026-08-30.md), [roteiro executado](roteiro-executado-baseline-administrativa-2026-08-30.md), commit `3566ba9` |
| S1-BE-013 | Rayana | Concluído. ViewModel da consulta criado. | [ConsultaEstoqueViewModel.cs](../../src/SquadEstoque.Web/Models/ConsultaEstoqueViewModel.cs), commit `ec2c901` |
| S1-BE-014 | Rayana | Concluído. Entrada protegida da consulta operacional criada. | [EstoqueController.cs](../../src/SquadEstoque.Web/Controllers/EstoqueController.cs), commits `61648a9`, `4b1164b` |
| S1-BE-015 | Rayana | Concluído. Busca por produto ativo com termo mínimo e ordenação implementada. | [EstoqueController.cs](../../src/SquadEstoque.Web/Controllers/EstoqueController.cs), [ConsultaEstoqueTests.cs](../../tests/SquadEstoque.Web.Tests/ConsultaEstoqueTests.cs), commit `61648a9` |
| S1-QA-016 | Rayana | Concluído. Autorização da consulta e perfis cobertos por testes. | [AuthenticationAuthorizationTests.cs](../../tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs), commit `61648a9` |
| S1-QA-017 | Rayana | Concluído. Busca operacional coberta por testes de integração. | [ConsultaEstoqueTests.cs](../../tests/SquadEstoque.Web.Tests/ConsultaEstoqueTests.cs), [EstoqueControllerHttpTests.cs](../../tests/SquadEstoque.Web.Tests/EstoqueControllerHttpTests.cs), commit `61648a9` |
| S1-FE-018 | Rayana | Concluído. Tela Razor de busca responsiva integrada. | [Consulta.cshtml](../../src/SquadEstoque.Web/Views/Estoque/Consulta.cshtml), [validação 31/08](validacao-consulta-vendedor-2026-08-31.md), commit `61648a9` |
| S1-BE-019 | Emmy; ajuste Rayana | Concluído no fluxo atual da `main`. Produto selecionado carrega SKUs ativos, saldos e estado da grade dentro da consulta operacional. | [EstoqueController.cs](../../src/SquadEstoque.Web/Controllers/EstoqueController.cs), branch `feat/s1-be-019-emmy`, commits `2e09412`, `8fa2aa4` |
| S1-FE-020 | Rayana | Concluído. Grade visual por numeração exibida com saldo e texto de estado, sem ações de venda/ruptura. | [Consulta.cshtml](../../src/SquadEstoque.Web/Views/Estoque/Consulta.cshtml), [site.css](../../src/SquadEstoque.Web/wwwroot/css/site.css), commits `5da7300`, `94a6780`, `5ed917c` |
| S1-FE-021 | Rayana | Concluído. Navegação por perfil e entrada direta do vendedor implementadas. | [validação de navegação](validacao-navegacao-por-perfil-2026-09-03.md), [AccountController.cs](../../src/SquadEstoque.Web/Controllers/AccountController.cs), commit `4b1164b` |
| S1-QA-022 | Emmy; fortalecimento Rayana | Concluído. Testes da grade operacional cobrem ordenação, estados visíveis, ausência de venda/ruptura e saldos inalterados. | [ConsultaOperacionalHttpTests.cs](../../tests/SquadEstoque.Web.Tests/ConsultaOperacionalHttpTests.cs), commits `e7ad14a`, `459a154` |
| S1-QA-023 | Rayana | Parcial. Validação responsiva da busca registrada; conferência manual final da grade em navegador gráfico deve seguir para Sprint 2. | [validação consulta vendedor](validacao-consulta-vendedor-2026-08-31.md), testes automatizados em 03/09/2026 |
| S1-DOC-024 | Rayana | Concluído por este relatório. Evidências da Sprint 1 consolidadas e pendências separadas. | Este arquivo, commit a gerar após revisão |

Todos os 24 cartões possuem responsável registrado, resultado declarado e pelo menos uma evidência associada. Quando o resultado é parcial ou reprovado, isso foi indicado explicitamente.

## Pendências transferidas para Sprint 2

| Pendência | Origem | Motivo |
|---|---|---|
| Validação manual final da grade em navegador gráfico, mobile e desktop | S1-QA-023 | A validação automatizada está verde, mas a conferência visual final da grade precisa de captura real após estabilização do CSS. |
| Decisão definitiva sobre permissão da saída administrativa para `VENDEDOR` | S1-QA-012 / IMP-ADM-001 | A homologação de 30/08 registrou divergência de autorização; o fluxo operacional `Vendeu` não deve ser confundido com saída administrativa. |
| Fluxos `Vendeu`, `Não tinha` e `Desistiu` | Protótipo S1-UX-002; RF-16 a RF-20 | Foram prototipados e especificados, mas não fazem parte do incremento implementado na Sprint 1. |
| Registro e visão de rupturas no sistema | RF-18, RF-22 | Persistência de domínio existe, mas fluxo HTTP/UX e visão gerencial ficam para sprint futura. |
| Saldos zerados e relatórios gerenciais | RF-21, UC-11 e UC-12 | Planejados no MVP, não implementados na fatia da Sprint 1. |
| Testes não funcionais executados em ambiente publicado | RNF-01 a RNF-07 | Casos especificados; execução real depende de ambiente/dispositivo final e dados de homologação. |

## Revisão técnica e textual

| Item revisado | Resultado |
|---|---|
| Coerência com MVC | A consulta operacional permanece no fluxo MVC: Controller, ViewModel, Razor View, EF Core e testes xUnit. |
| Escopo da Sprint 1 | Relatório registra apenas protótipo, testes, baseline administrativa, busca, grade e navegação por perfil. Venda, ruptura e relatórios não foram marcados como entregues. |
| Segurança da evidência | Não foram incluídos senha, hash, cookie, token antiforgery, banco local ou captura com sessão privada. |
| Texto | Padronizado para evidência curta, rastreável e sem reescrever monografia. |
| Testes | Suíte executada em 03/09/2026 com 60 aprovados e 0 falhas. |

