# Relatório da Sprint 2

**Projeto:** SQUAD Estoque
**Período da Sprint 2:** 10/09/2026 a 07/10/2026
**Escopo planejado:** 40 cartões, de S2-UX-001 a S2-DOC-040

Este documento é um índice de resultados e evidências. Ele registra o que está comprovado nos relatórios e no histórico de integração; não presume que todos os cartões planejados foram concluídos.

## Resultado consolidado

| Frente                       | Resultado observado                                                                                                                                                                         | Situação                                                                                         |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| Fluxo do vendedor            | Busca e consulta da grade em viewport móvel; seleção de numeração; resultados`Vendeu`, `Não tinha` e `Desistiu`; continuidade para nova consulta.                               | Implementado em incrementos integrados, com validações específicas registradas                  |
| Venda e estoque              | Venda rápida decrementa uma unidade do SKU selecionado, preserva os demais SKUs e registra uma movimentação associada ao vendedor autenticado. Tentativas sem saldo são rejeitadas.     | Cobertura automatizada registrada; ver relatórios por cenário                                    |
| Ruptura —`Não tinha`     | Registro explícito vinculado ao SKU e ao vendedor, sem alterar saldos ou movimentações; validações também cobrem autorização e estados de saldo.                                    | Cobertura automatizada e validação manual registradas                                            |
| Encerramento —`Desistiu`  | Atendimento encerrado sem gravar movimentação; interface retorna à consulta vazia após confirmação e conserva o contexto em caso de falha.                                            | Cobertura automatizada e validação visual em viewport emulado registradas                        |
| Responsividade e usabilidade | Login validado em desktop e viewport móvel. A validação ampliada do módulo do vendedor em 360×800, 390×844, 430×932 e 1280×800 ocorreu em 09/10, após o período formal da sprint. | Login validado durante a sprint; validação ampliada do vendedor é evidência posterior          |
| Testes automatizados         | Relatórios de incrementos distintos registram execuções entre 82 e 140 testes .NET aprovados; o relatório de 03/10 registra também 13 testes frontend aprovados.                       | Resultados locais documentados; números representam snapshots diferentes e não devem ser somados |
| Integração contínua       | Workflow executa build/testes .NET, testes frontend e deploy. A execução da`main` após a integração em 11/10/2026 passou nos jobs listados abaixo.                                   | Regressão posterior à sprint; não substitui os checks individuais dos PRs                       |

## Evidências organizadas

### Fluxo do vendedor e validações manuais

- [Busca, seleção do produto e grade em viewport móvel — S2-QA-006](../09-testing/execucoes/sprint-2/validacao-fluxo-grade-smartphone-2026-09-18.md). A validação foi em viewport emulado; o relatório registra ressalvas e outros viewports ainda pendentes naquela data.
- [Venda rápida com saldo disponível e movimentação — 22/09](../09-testing/execucoes/sprint-2/validacao-venda-rapida-saldo-disponivel-2026-09-22.md): venda de uma unidade, preservação dos demais SKUs e uma saída vinculada ao vendedor.
- [Ação `Não tinha` — 28/09](../09-testing/execucoes/sprint-2/validacao-nao-tinha-2026-09-28.md): saldos 0, 1 e 3; saldo preservado, ruptura criada, nenhuma movimentação e inspeção em 390×844 e 1366×900.
- [Fluxo `Desistiu` — S2-FE-030](../09-testing/execucoes/sprint-2/s2-fe-030-desistiu.md): retorno à consulta vazia, foco, mensagens e saldos conferidos em viewport móvel 390×844 e desktop 1280×900. O smartphone foi emulado, não um aparelho físico.
- [Login responsivo — evidências de 16 e 18/09](../09-testing/evidencias/sprint-2/login-responsivo/validacao-login-responsivo.md): foco, mensagens de validação, autenticação por perfil e layout móvel; o relatório distingue capturas de 16/09 da validação complementar informada em 18/09.

### Testes automatizados

Os relatórios abaixo correspondem a incrementos diferentes da suíte. As contagens são snapshots por execução, não uma soma cumulativa.

- [Venda rápida com saldo — S2-QA-011](../09-testing/execucoes/sprint-2/validacao-venda-rapida-saldo-disponivel-2026-09-22.md): 82 testes .NET aprovados; build aprovado.
- [Atualização da grade após venda — S2-FE-016](../09-testing/execucoes/sprint-2/s2-fe-016-grade-apos-venda.md): 90 testes aprovados; build Release aprovado.
- [Saldo preservado na ruptura — S2-BE-021](../09-testing/execucoes/sprint-2/s2-be-021-preservar-saldo-ruptura.md): 3 testes focados e suíte de 94 aprovados.
- [Autorização da ruptura — S2-QA-025](../09-testing/execucoes/sprint-2/s2-qa-025-autorizacao-ruptura.md): 97 testes .NET aprovados; build aprovado.
- [Encerramento `Desistiu` — S2-BE-029](../09-testing/execucoes/sprint-2/s2-be-029-desistiu.md): suíte de 113 testes .NET aprovados; build aprovado.
- [Interface e regressão de `Desistiu` — S2-FE-030](../09-testing/execucoes/sprint-2/s2-fe-030-desistiu.md): revisão de 03/10 registra 140 testes .NET e 13 testes frontend aprovados.

### Pull requests integrados à `main`

Os seguintes PRs constam como integrados à `main` na consolidação de evidências da sprint:

| PRs                                                                                                                                                                                | Escopo resumido                                                                       |
| ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------- |
| [#18](https://github.com/CodeWitch00/SQUAD-estoque/pull/18), [#22](https://github.com/CodeWitch00/SQUAD-estoque/pull/22), [#23](https://github.com/CodeWitch00/SQUAD-estoque/pull/23) | Documentação, integração do fluxo do vendedor e ViewModel do atendimento          |
| [#25](https://github.com/CodeWitch00/SQUAD-estoque/pull/25), [#29](https://github.com/CodeWitch00/SQUAD-estoque/pull/29), [#35](https://github.com/CodeWitch00/SQUAD-estoque/pull/35) | Venda rápida de uma unidade, busca/grade móvel e atualização da grade após venda |
| [#27](https://github.com/CodeWitch00/SQUAD-estoque/pull/27)                                                                                                                         | Responsividade do login                                                               |
| [#31](https://github.com/CodeWitch00/SQUAD-estoque/pull/31), [#32](https://github.com/CodeWitch00/SQUAD-estoque/pull/32), [#34](https://github.com/CodeWitch00/SQUAD-estoque/pull/34) | Testes de venda, rejeição sem saldo e autorização                                 |
| [#38](https://github.com/CodeWitch00/SQUAD-estoque/pull/38), [#39](https://github.com/CodeWitch00/SQUAD-estoque/pull/39), [#42](https://github.com/CodeWitch00/SQUAD-estoque/pull/42) | Ação`Não tinha`, preservação do saldo e rejeição de SKU inválido/divergente |
| [#49](https://github.com/CodeWitch00/SQUAD-estoque/pull/49)                                                                                                                         | Testes do encerramento de atendimento`Desistiu`                                     |

PRs relacionados que foram fechados sem merge, incluindo [#37](https://github.com/CodeWitch00/SQUAD-estoque/pull/37), [#48](https://github.com/CodeWitch00/SQUAD-estoque/pull/48), [#50](https://github.com/CodeWitch00/SQUAD-estoque/pull/50), [#51](https://github.com/CodeWitch00/SQUAD-estoque/pull/51) e [#52](https://github.com/CodeWitch00/SQUAD-estoque/pull/52), não são contabilizados como entregas integradas.

### CI

- [Workflow .NET e frontend](../../.github/workflows/dotnet.yml)
- [Execuções no GitHub Actions](https://github.com/CodeWitch00/SQUAD-estoque/actions/workflows/dotnet.yml)
- [Execução da `main` no commit `e9d553a`](https://github.com/CodeWitch00/SQUAD-estoque/actions/runs/38156810379): em 11/10/2026, `build-and-test`, `frontend-tests` e `deploy-azure` concluíram com sucesso.

A execução acima é uma verificação da `main` após a integração, não evidência de que cada PR individual passou em CI. Para comprovar o check de um PR, consultar a execução vinculada ao próprio PR no GitHub.

### Evidências posteriores ao período

- [Responsividade do módulo do vendedor — 09/10](../09-testing/execucoes/pos-sprint/validacao-responsividade-vendedor-2026-10-09.md): 17 testes frontend e 167 testes .NET aprovados; viewports de 360×800, 390×844, 430×932 e 1280×800. A validação foi com Chromium/Playwright em viewports CSS; aparelho físico e usuários reais permanecem pendentes.
- [Atendimentos auditáveis e entrada fiscal — 09/10](../09-testing/execucoes/pos-sprint/validacao-atendimentos-auditaveis-2026-10-09.md) e [entrada fiscal](../09-testing/execucoes/pos-sprint/validacao-entrada-fiscal-2026-10-09.md) são registros posteriores ao período formal; não são contabilizados como entrega da Sprint 2 neste relatório.

## Referência para futura atualização da monografia

Usar este relatório como índice de evidências, não como fonte única para descrever resultados. Confirmar o commit, as datas e a abrangência de cada teste antes de atualizar metodologia ou resultados da monografia. Os documentos acadêmicos permanecem em [docs/08-monografia](../08-monografia/).

## Limites deste relatório

- Contagens de testes de execuções diferentes não devem ser somadas.
- Uma execução global aprovada na `main` não substitui a CI individual de cada PR.
- Viewports emulados não comprovam uso em aparelho físico ou avaliação com usuários reais.
- Este relatório não inclui credenciais, dados do banco local nem materiais privados de gestão.
