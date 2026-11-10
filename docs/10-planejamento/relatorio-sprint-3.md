# Relatório da Sprint 3 — Índice de evidências (S3-DOC-039)

**Projeto:** SQUAD Estoque
**Período da Sprint 3:** 08/10/2026 a 11/11/2026
**Escopo planejado:** 40 cartões com identificação `S3`
**Cartão:** S3-DOC-039 — Consolidar evidências da Sprint 3 — PRs, testes e CI
**Prazo formal:** 11/11/2026 23:59 (America/Sao_Paulo)
**Baseline de referência:** `main` em `c6cfdad` (08/11/2026)

Este documento é um índice de resultados e evidências da Sprint 3. Ele relaciona cada cartão aos Pull Requests, commits, testes, CI e validações correspondentes; não presume que todos os cartões planejados foram concluídos. Os nomes dos cartões foram copiados do quadro do Trello sem alteração e o quadro não foi modificado.

O quadro exibia 39 cartões `S3` ativos (31 em Feito, 3 em execução, 3 em A Fazer e 2 em Testes do professor). O 40º cartão planejado, `S3-BE-008`, não está visível no quadro (provável arquivado); ele consta apenas como referência documental em [`s3-db-026-indices-consulta.md`](../09-testing/s3-db-026-indices-consulta.md) e é registrado adiante.

## Como ler este índice

- **Fontes de verdade:** histórico do `origin/main`, Pull Requests, [GitHub Actions](https://github.com/CodeWitch00/SQUAD-estoque/actions) e a documentação em `docs/`.
- **Responsável:** derivado do autor Git/GitHub do commit ou PR (`ImEmmy` = Emmy, `pz69766-blip` = Felipe, `CodeWitch00`/`rayana barbosa` = Rayana, `NicolasAmorim78`/`Nícolas` = Nícolas).
- **Situações:** `Concluído` (integrado à `main`), `Em revisão` (PR aberto aguardando revisão obrigatória de outro integrante), `Parcial` (evidência fora da `main` ou escopo incompleto) e `Pendente` (sem entrega registrada).
- **CI:** os números citados são runs da `main` no workflow [`dotnet.yml`](../../.github/workflows/dotnet.yml). Uma execução global aprovada na `main` não substitui os checks individuais de cada PR.
- **Contagens de testes** de execuções diferentes não são somadas nem comparadas diretamente.

## Resultado consolidado

| Frente | Resultado observado | Situação |
| --- | --- | --- |
| Venda concorrente e transação | Decremento atômico do saldo (S3-BE-001) e venda na mesma transação (S3-BE-002), com rollback comprovado (S3-QA-004) e venda simultânea testada (S3-QA-003) | Integrado e testado |
| Saldos zerados | ViewModel (S3-BE-007), tela do lojista (S3-FE-010), autorização (S3-QA-011), consulta com filtro/agrupamento (S3-QA-009) e exclusão de itens inativos (S3-BE-008, correção `d7912e4`) | Integrado e testado |
| Histórico de rupturas | ViewModels (S3-BE-013), agregação por SKU (S3-BE-014), ordenação estável (S3-BE-015), tela (S3-FE-016), validação de interface (S3-QA-018) e agregação auditada (S3-QA-017) | Integrado e testado |
| Movimentações imutáveis | Bloqueio de edição/exclusão (S3-BE-005) com smoke HTTP (S3-QA-006) e suíte automatizada de imutabilidade | Integrado e testado |
| Ajuste de estoque | Autorização (S3-QA-021), motivo obrigatório e normalizado (S3-BE-019, S3-QA-020) e auditoria de diferença/usuário/motivo/data (S3-QA-022) | Integrado e testado |
| Última atualização do saldo | Registro por SKU (S3-BE-023), exibição nas grades (S3-FE-024) e validação HTTP (S3-QA-025) | Integrado e testado |
| Desempenho | Otimização da consulta do vendedor (S3-BE-027), P95 abaixo de 500 ms (S3-QA-028) e índices de consulta (S3-DB-026) | Integrado e testado |
| Visões do lojista | Relatórios responsivos e acessíveis (S3-FE-031) incluindo acessibilidade operacional (S3-QA-032) | Integrado e validado |
| Segurança e regressão | Regressão de segurança do login (S3-QA-029), auditoria de separação MVC (S3-QA-034) e regressão E2E vendedor/lojista (S3-QA-030, com defeito `DEF-S3QA030-01` corrigido no PR #95) | Integrado; aceite do QA-030 depende de revisão independente |
| Documentação e monografia | Plano de testes atualizado (S3-DOC-036, PR #98), evidências da P2 (S3-DOC-038, PR #97), registro de trechos (S3-DOC-037, PR #96) e este índice (S3-DOC-039) | PRs #96–#98 abertos, aguardando revisão obrigatória de outro integrante |
| Homologações pendentes | Homologação de saldos zerados em mobile/desktop (S3-QA-012), homologação do MVP local (S3-QA-040) e revisão de funcionalidades na monografia (S3-DOC-033, em execução) | Não concluídos; transferidos para a Sprint 4 |

Contagem: 31 cartões concluídos e integrados à `main`; 3 entregues em PRs abertos em revisão (#96, #97, #98); 1 parcial fora da `main` (S3-QA-035); 5 pendentes/em execução (S3-DOC-033, S3-QA-012, S3-QA-040, e este cartão S3-DOC-039 em entrega).

## Índice por cartão

### Cartões concluídos (lista Feito)

| Cartão | Entrega (PR / commit) | Testes e evidências | CI (`main`) | Responsável | Revisão registrada | Situação |
| --- | --- | --- | --- | --- | --- | --- |
| S3-BE-001 — decremento atômico | [#53](https://github.com/CodeWitch00/SQUAD-estoque/pull/53) (merged 09/10) · `523fd0b` (08/10) | [Relatório](../09-testing/execucoes/sprint-3/s3-be-001-decremento-atomico.md); `VendaTransacionalTests` | Run #76 verde (09/10) | Emmy | — | Concluído |
| S3-BE-002 — venda na mesma transação | [#54](https://github.com/CodeWitch00/SQUAD-estoque/pull/54) fechado sem merge · `91beacd` (08/10) integrado via `672e192` | [Relatório](../09-testing/execucoes/sprint-3/s3-be-002-venda-transacional.md) | Run #75 verde (09/10) | Emmy | — | Concluído |
| S3-QA-003 — venda simultânea | [#56](https://github.com/CodeWitch00/SQUAD-estoque/pull/56) (merged 12/10) · `fc221d0` (11/10) | [Relatório](../09-testing/execucoes/sprint-3/s3-qa-003-venda-simultanea.md) e [resultados](../09-testing/execucoes/sprint-3/s3-qa-003-resultados.txt) | Run #80 verde (12/10) | Felipe | Aprovada por `CodeWitch00` em 12/10 | Concluído |
| S3-QA-004 — rollback da venda | `5800bdf` (12/10) via merge `1dacf4d` | `VendaTransacionalTests`; validação registrada no PR/commits | Run #82 verde (12/10) | Rayana | — | Concluído |
| S3-BE-007 — ViewModel de saldos zerados | [#60](https://github.com/CodeWitch00/SQUAD-estoque/pull/60) (merged 16/10) · `bfa420c` (13/10) | [Relatório](../09-testing/execucoes/sprint-3/s3-be-007-viewmodel-saldos-zerados.md); `SaldosZeradosViewModelTests` | Run #84 verde (16/10) | Felipe | — | Concluído |
| S3-QA-021 — autorização do ajuste | [#58](https://github.com/CodeWitch00/SQUAD-estoque/pull/58) (merged 16/10) · `a5c1a86` (12/10) | [Relatório](../09-testing/execucoes/sprint-3/s3-qa-021-autorizacao-ajuste.md); `AjusteAuthorizationHttpTests` | Run #83 verde (16/10) | Felipe | — | Concluído |
| S3-QA-006 — imutabilidade | [#62](https://github.com/CodeWitch00/SQUAD-estoque/pull/62) (merged 16/10) · `ce98e78` (14/10) | `MovimentacaoImutabilidadeHttpTests`, `MovimentacaoSomenteLeituraHttpTests`; [registro manual 14/10](../09-testing/homologacoes/registro-validacoes-manuais.md) | Run #85 verde (16/10) | Emmy | — | Concluído |
| S3-BE-013 — ViewModel de rupturas | [#63](https://github.com/CodeWitch00/SQUAD-estoque/pull/63) fechado sem merge · `88465f1` (14/10) integrado via `2d5861e` | [Relatório](../09-testing/execucoes/sprint-3/s3-be-013-viewmodel-rupturas.md); `RupturasControllerTests` | Run #86 verde (16/10) | Emmy | Aprovação registrada no [#64](https://github.com/CodeWitch00/SQUAD-estoque/pull/64) (16/10) | Concluído |
| S3-BE-005 — histórico somente leitura | `17fcf12` (16/10) via merge `b5e3bfd` | `MovimentacaoSomenteLeituraHttpTests` | Run #88 verde (16/10) | Rayana | — | Concluído |
| S3-FE-010 — exibir saldos zerados | `600bbb2` (16/10) via merge `778f4ae` | `SaldosZeradosConsultaTests` e `SaldosZeradosAuthorizationHttpTests` (S3-QA-011) | Run #89 verde (16/10) | Rayana | — | Concluído |
| S3-QA-011 — autorização de saldos zerados | `SaldosZeradosAuthorizationHttpTests` (criado com `600bbb2`, 16/10) | Suíte na `main`; sem relatório próprio | Run #89 verde (16/10) | Rayana | — | Concluído |
| S3-BE-014 — agregar histórico por SKU | [#65](https://github.com/CodeWitch00/SQUAD-estoque/pull/65) (merged 20/10) · `2ed292d` | `RupturasHistoricoHttpTests` | Run #91 verde (20/10) | Rayana | — | Concluído |
| S3-FE-016 — exibir histórico de rupturas | [#64](https://github.com/CodeWitch00/SQUAD-estoque/pull/64) (merged 20/10) · `381224d` | [Validação de interface 20/10](../09-testing/execucoes/sprint-3/validacao-historico-rupturas-2026-10-20.md) e [capturas](../09-testing/evidencias/historico-rupturas/) | Run #90 verde (20/10) | Emmy | Aprovada por `CodeWitch00` em 20/10 | Concluído |
| S3-QA-018 — homologar histórico de rupturas | [#70](https://github.com/CodeWitch00/SQUAD-estoque/pull/70) (merged 25/10) · validação 20/10 em `381224d` | [Registro manual 20/10](../09-testing/homologacoes/registro-validacoes-manuais.md) | Run #92 verde (25/10) | Rayana | — | Concluído |
| S3-QA-020 — motivo do ajuste | [#69](https://github.com/CodeWitch00/SQUAD-estoque/pull/69) (merged 25/10) · `b44cdcd` (22/10) | [Relatório](../09-testing/execucoes/sprint-3/s3-qa-020-motivo-ajuste.md); `AjusteMotivoHttpTests` | Run #93 verde (25/10) | Felipe | — | Concluído |
| S3-BE-019 — reforçar motivo do ajuste | [#71](https://github.com/CodeWitch00/SQUAD-estoque/pull/71) (merged 27/10) · `c09957d` (26/10) e `7839e48` | `AjusteMotivoHttpTests` e frontend `ajuste-motivo.test.cjs` | Run #95 verde (27/10) | Emmy | — | Concluído |
| S3-BE-015 — ordenar rupturas | [#75](https://github.com/CodeWitch00/SQUAD-estoque/pull/75) (merged 27/10) · `a4e5a3c` | Desempates cobertos na suíte (`RupturasHistoricoHttpTests`) | Run #94 verde (27/10) | Felipe | — | Concluído |
| S3-QA-022 — auditoria do ajuste | [#77](https://github.com/CodeWitch00/SQUAD-estoque/pull/77) (merged 27/10) · `1d314e5` | Aumento e redução auditados na suíte | Run #96 verde (27/10) | Rayana | — | Concluído |
| S3-BE-023 — registrar última atualização | [#72](https://github.com/CodeWitch00/SQUAD-estoque/pull/72) (merged 27/10) · `dd629c5` (26/10) | [Documento](../09-testing/s3-be-023-ultima-atualizacao-saldo.md); `SkuUltimaAtualizacaoMigrationTests` e [evidências](../09-testing/evidencias/s3-be-023/) | Verde na integração (27/10) | Emmy | — | Concluído |
| S3-QA-025 — última atualização | [#83](https://github.com/CodeWitch00/SQUAD-estoque/pull/83) (merged 29/10) · `608f819` | [Documento](../09-testing/s3-qa-025-ultima-atualizacao.md); `UltimaAtualizacaoSaldoHttpTests` | Run #409 verde (29/10) | Felipe | — | Concluído |
| S3-BE-027 — otimizar consulta do vendedor | [#84](https://github.com/CodeWitch00/SQUAD-estoque/pull/84) (merged 29/10) · `91b9e8c` | [Documento](../09-testing/consulta-vendedor-otimizada-2026-10-29.md) e [evidências](../09-testing/evidencias/consulta-otimizada/) | Run #412 verde (29/10) | Emmy | — | Concluído |
| S3-QA-028 — desempenho P95 | [#85](https://github.com/CodeWitch00/SQUAD-estoque/pull/85) (Emmy) e [#86](https://github.com/CodeWitch00/SQUAD-estoque/pull/86) (Rayana, merged 29/10) · `b0c9196` | [Documento](../09-testing/s3-qa-028-p95-consulta.md) e [evidências](../09-testing/evidencias/s3-qa-028/) | Run #417 verde (29/10) | Emmy (execução) e Rayana (integração) | Pareamento Emmy/Rayana entre PRs #85 e #86 | Concluído |
| S3-DB-026 — índices de consulta | [#87](https://github.com/CodeWitch00/SQUAD-estoque/pull/87) (merged 29/10) · `1c68b7b` | [Documento](../09-testing/s3-db-026-indices-consulta.md) e [planos de consulta](../09-testing/s3-db-026-planos-consulta.txt) | Run #421 verde (29/10) | Rayana | — | Concluído |
| S3-QA-017 — agregação de rupturas | [#81](https://github.com/CodeWitch00/SQUAD-estoque/pull/81) (merged 29/10) · `66982d6` | Contagem de rupturas validada na suíte (VNP-05) | Run #406 verde (29/10) | Rayana | — | Concluído |
| S3-FE-024 — exibir última atualização | [#81](https://github.com/CodeWitch00/SQUAD-estoque/pull/81) (merged 29/10) · `95199c4` | Exibição validada junto às grades | Run #406 verde (29/10) | Rayana | — | Concluído |
| S3-QA-009 — consulta de saldos zerados | [#89](https://github.com/CodeWitch00/SQUAD-estoque/pull/89) (merged 31/10) · `1034b31` e `2e116c5` (30/10) | `SaldosZeradosConsultaTests`; casos-limite reforçados | Run #437 verde (31/10) | Nícolas | — | Concluído |
| S3-FE-031 — visões do lojista | [#90](https://github.com/CodeWitch00/SQUAD-estoque/pull/90) (merged 03/11) · `5556818` (01/11) | [Evidências](../05-ux/evidencias/s3-fe-031/README.md) e [resultados](../05-ux/evidencias/s3-fe-031/resultados.json) | Run #442 verde (03/11) | Felipe | — | Concluído |
| S3-QA-032 — acessibilidade operacional | Coberto pela entrega S3-FE-031 ([#90](https://github.com/CodeWitch00/SQUAD-estoque/pull/90)) | Teclado, foco e contraste validados nas [evidências do S3-FE-031](../05-ux/evidencias/s3-fe-031/) | Run #442 verde (03/11) | Felipe | — | Concluído |
| S3-QA-034 — auditoria de separação MVC | [#93](https://github.com/CodeWitch00/SQUAD-estoque/pull/93) (merged 06/11) · `d4e7110` (04/11) | [Auditoria MVC 04/11](../04-arquitetura/auditoria-mvc-2026-11-04.md) | Run #461 verde (06/11) | Emmy | — | Concluído |
| S3-QA-029 — regressão de segurança | `748572f` direto na `main` (06/11) | Regressão de login, bcrypt e perfis reforçada na suíte | Run #465 verde (06/11) | Rayana | — | Concluído |
| S3-QA-030 — regressão E2E | [#95](https://github.com/CodeWitch00/SQUAD-estoque/pull/95) (merged 08/11) · PRs [#91](https://github.com/CodeWitch00/SQUAD-estoque/pull/91)/[#92](https://github.com/CodeWitch00/SQUAD-estoque/pull/92) fechados sem merge | [Relatório E2E](../09-testing/execucoes/sprint-3/s3-qa-030-regressao-e2e.md), [evidências UI](../09-testing/evidencias/s3-qa-030/) e [registro manual 02–07/11](../09-testing/homologacoes/registro-validacoes-manuais.md); `DEF-S3QA030-01` corrigido no PR #95 | Run #470 verde (08/11) | Nícolas | Revisão independente pendente (S4-P2-13) | Concluído; aceite formal pendente |

### Cartões em Testes (professor)

| Cartão | Entrega (PR / commit) | Testes e evidências | CI | Responsável | Situação |
| --- | --- | --- | --- | --- | --- |
| S3-QA-035 — auditar integridade do banco | [#94](https://github.com/CodeWitch00/SQUAD-estoque/pull/94) merged em `dev/emmy` 06/11 · `219d561` | [Auditoria de integridade](../../tree/dev/emmy/docs/09-testing/s3-qa-035-auditoria-integridade.md) — arquivo presente em `dev/emmy`, ausente na `main` | PR verde | Emmy | Parcial: pendente de integração à `main` |
| S3-QA-040 — homologar MVP local | Sem entrega registrada | Homologação de fluxos completos antes de produção | — | — | Pendente (Sprint 4) |

### Cartões em execução

| Cartão | Entrega (PR / commit) | Testes e evidências | CI | Responsável | Situação |
| --- | --- | --- | --- | --- | --- |
| S3-DOC-033 — monografia: funcionalidades | Revisão S3 dos `.docx` na branch `docs/s3-doc-037-integrante` (PR [#96](https://github.com/CodeWitch00/SQUAD-estoque/pull/96)), fora da `main` | Arquivos revisados em `docs/08-monografia/3ºsemestre/` | — | Nícolas | Em execução |
| S3-DOC-037 — monografia: resultados e arquitetura | [#96](https://github.com/CodeWitch00/SQUAD-estoque/pull/96) aberto · `e6f4e75` | [Registro de trechos e evidências](../08-monografia/s3-doc-037-registro-trechos-evidencias.md) (no PR) | Checks do PR verdes | Nícolas | Em revisão |
| S3-QA-012 — homologar saldos zerados | Sem entrega registrada | Leitura em mobile e desktop | — | — | Pendente (Sprint 4) |

### Cartões em A Fazer

| Cartão | Entrega (PR / commit) | Testes e evidências | CI | Responsável | Situação |
| --- | --- | --- | --- | --- | --- |
| S3-DOC-038 — evidências da P2 | [#97](https://github.com/CodeWitch00/SQUAD-estoque/pull/97) aberto · `43aa9b0` | [Índice de evidências da P2](../08-monografia/s3-doc-038-indice-evidencias-p2.md) (no PR) | Checks do PR verdes | Nícolas | Em revisão |
| S3-DOC-036 — plano de testes | [#98](https://github.com/CodeWitch00/SQUAD-estoque/pull/98) aberto · `b390173` | [Plano de testes v2.1](../09-testing/plano-de-testes.md) (no PR) | Checks do PR verdes | Nícolas | Em revisão |
| S3-DOC-039 — este índice | Branch `docs/s3-doc-039-integrante` (este documento) | Este relatório | Checks do PR a verificar | Nícolas | Em entrega |

### Cartão arquivado (40º planejado)

| Cartão | Entrega (PR / commit) | Testes e evidências | CI | Responsável | Situação |
| --- | --- | --- | --- | --- | --- |
| S3-BE-008 — itens inativos nos saldos zerados | Correção `d7912e4` (29/10, Rayana), integrada no [#65](https://github.com/CodeWitch00/SQUAD-estoque/pull/65) | Citado em [`s3-db-026-indices-consulta.md`](../09-testing/s3-db-026-indices-consulta.md): consulta de saldos zerados exclui itens inativos | Run #422 verde (29/10) | Rayana | Corrigido; cartão não visível no quadro |

## Integração contínua

O pipeline [`dotnet.yml`](../../.github/workflows/dotnet.yml) executa `build-and-test` (com etapa de baseline de cobertura de linhas), `frontend-tests` e `deploy-azure` (condicionado a `AZURE_CD_ENABLED`).

- Todas as execuções da `main` entre 08/10 e 08/11 concluíram com sucesso; nenhuma run da `main` foi reprovada no período da sprint.
- [Run #470](https://github.com/CodeWitch00/SQUAD-estoque/actions/runs/42512546507) (`main` em `c6cfdad`, 08/11): `build-and-test`, `frontend-tests` e `deploy-azure` concluíram com sucesso. A baseline da P2 foi publicada no Azure pela esteira; a homologação manual no ambiente publicado permanece pendente (S4-P2-06).
- PRs abertos [#96](https://github.com/CodeWitch00/SQUAD-estoque/pull/96), [#97](https://github.com/CodeWitch00/SQUAD-estoque/pull/97) e [#98](https://github.com/CodeWitch00/SQUAD-estoque/pull/98): `build-and-test` e `frontend-tests` com sucesso; `deploy-azure` ignorado por alterar apenas documentação. Duas runs antigas da branch do PR #96 (`e6f4e75`, runs 471/472) constam como `failure`, embora todos os jobs dessas runs tenham concluído com sucesso ou sido ignorados; consultar o histórico do Actions para a causa registrada.
- Contagens de testes registradas: 310 aprovados na consolidação do [S3-DOC-038](../08-monografia/s3-doc-038-indice-evidencias-p2.md) (§2) e 306 aprovados na reexecução local do [S3-DOC-037](../08-monografia/s3-doc-037-registro-trechos-evidencias.md) (§6.1). São execuções distintas e não devem ser somadas nem comparadas diretamente; a fonte operacional do estado atual é a CI da `main`.

## Validações manuais e homologações

As validações que a CI não demonstra estão consolidadas no [registro de validações manuais](../09-testing/homologacoes/registro-validacoes-manuais.md): imutabilidade (14/10), histórico de rupturas (20/10) e regressão E2E de vendedor e lojista (02–03/11, com reteste do E2E-02 em 07/11 sobre o head do PR #95). A homologação no ambiente publicado do Azure permanece pendente.

## Pendências transferidas para a Sprint 4

Além das pendências do S3-DOC-038 (§11, S4-P2-01 a S4-P2-14), este índice registra:

| Pendência | Origem | Critério de conclusão |
| --- | --- | --- |
| Revisão obrigatória de outro integrante nos PRs [#96](https://github.com/CodeWitch00/SQUAD-estoque/pull/96), [#97](https://github.com/CodeWitch00/SQUAD-estoque/pull/97), [#98](https://github.com/CodeWitch00/SQUAD-estoque/pull/98) e no PR deste documento | S3-DOC-036/037/038/039 | Revisão aprovada e merge |
| Integrar a auditoria do S3-QA-035 (PR #94, hoje em `dev/emmy`) à `main` | S3-QA-035 | Arquivo presente na `main` via PR |
| Aceite formal do S3-QA-030 após revisão independente | S3-QA-030 | Relatório revisado e aceite registrado (S4-P2-13) |
| Concluir S3-QA-012 (homologar saldos zerados em mobile e desktop) | S3-QA-012 | Registro em `registro-validacoes-manuais.md` |
| Concluir S3-QA-040 (homologar MVP local antes de produção) | S3-QA-040 | Fluxos completos validados e registrados |
| Concluir S3-DOC-033 (revisão de funcionalidades na monografia) | S3-DOC-033 | Revisão integrada à `main` |
| Homologação manual da baseline publicada no Azure (deploy verde no run #470) | S4-P2-02/06 | Registro com dispositivo, navegador e resultado |

A tabela completa de pendências da P2 (S4-P2-01 a S4-P2-14) permanece em [S3-DOC-038, §11](../08-monografia/s3-doc-038-indice-evidencias-p2.md).

## Credenciais e dados sensíveis

Em 10/11/2026 foi executada uma varredura por senhas, hashes, tokens e cookies nos arquivos de evidência de `docs/09-testing/` e `docs/05-ux/evidencias/`. As únicas ocorrências são nomes de coluna `SenhaHash` no DDL de migration registrado como evidência e declarações explícitas de exclusão de credenciais nas exportações do banco. Nenhuma credencial, segredo ou dado sensível foi versionado.

## Limites deste relatório

- Contagens de testes de execuções diferentes não devem ser somadas.
- Uma execução global aprovada na `main` não substitui a CI individual de cada PR.
- Viewports emulados não comprovam uso em aparelho físico ou avaliação com usuários reais.
- Os PRs #96, #97 e #98 estavam abertos na data da consolidação; o estado final deles deve ser confirmado no GitHub.
- O quadro do Trello foi consultado apenas em leitura; nomes de cartões foram copiados sem alteração e nenhuma lista ou cartão foi modificado.
- Este relatório não inclui credenciais, dados do banco local nem materiais privados de gestão.
