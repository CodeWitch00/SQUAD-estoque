# Registro consolidado de validações manuais

Este documento preserva somente resultados que não são demonstrados integralmente pelos testes automatizados e pela CI. Resultados de suíte, comandos e detalhes por cartão devem ser consultados nos Pull Requests e no GitHub Actions.

## Validações realizadas

| Data | Escopo e ambiente | Resultado | Limites ou pendências |
|---|---|---|---|
| 30/08/2026 | Baseline administrativa local, .NET 10, Kestrel, SQLite descartável e requisições MVC reais | 13 cenários aprovados e 1 reprovado | A execução encontrou escrita administrativa permitida ao `VENDEDOR`. É um registro histórico da baseline e não representa o estado atual. |
| 31/08/2026 | Consulta do vendedor em 390 px e 1366 px, com inspeção do HTML e CSS servidos | Fluxo, foco, ordem de teclado e ausência de rolagem horizontal aprovados por inspeção | O Firefox headless falhou ao produzir capturas; recomendou-se conferência em navegador gráfico. |
| 03/09/2026 | Navegação por perfil em ambiente local | Destinos pós-login, menus por perfil, marca e logout aprovados | A cobertura automatizada correspondente está em `AuthenticationAuthorizationTests`. |
| 16 e 18/09/2026 | Login em desktop e viewports 360×800, 390×844 e 430×932 | Layout, foco, mensagens e redirecionamento por perfil informados como aprovados | Navegador, versão e dispositivo da validação complementar de 18/09 não foram registrados. |
| 18/09/2026 | Primeira consulta até a grade, viewport emulado 390×844 | Busca, seleção e grade aprovadas | Não equivale a aparelho físico; outros viewports estavam pendentes naquela data. |
| 28/09/2026 | Ação `Não tinha`, aplicação local, 390×844 e 1366×900 | Saldos 0, 1 e 3 preservados; ruptura vinculada ao vendedor; nenhuma movimentação; foco e bloqueio durante envio aprovados | Navegação assistida por Playwright; não foi usado ambiente compartilhado. |
| 02/10/2026 | Fluxo `Desistiu`, aplicação local, 390×844 e 1280×900 | Retorno à busca, foco, mensagens e preservação dos saldos aprovados | Smartphone validado por emulação de viewport, sem aparelho físico. |
| 09/10/2026 | Responsividade do vendedor em 360×800, 390×844, 430×932 e 1280×800 | Sem rolagem horizontal; alvos de toque, foco, grade e mensagens aprovados | Chromium/Playwright em viewports CSS; aparelho físico e teste com usuários permanecem pendentes. |
| 09/10/2026 | Atendimentos pendentes e auditáveis, ambiente local | Continuidade de atendimentos, retomada, vínculo com resultado e controles anti-burla validados | Registro posterior ao período formal da Sprint 2. A cobertura automatizada e a migration devem ser consultadas no código e na CI. |
| 09/10/2026 | Entrada fiscal por NF-e, ambiente local | Importação em rascunho, conferência, confirmação, vínculo e bloqueio de duplicidade validados | Registro posterior ao período formal da Sprint 2; nenhuma atualização manual do banco local foi executada. |
| 14/10/2026 | RN-03, imutabilidade; dev/emmy baseada em 1dacf4d, .NET 10.0.400, Kestrel local em HTTP e SQLite descartável | Produto, grade e entrada criados pela aplicação; 12 tentativas de Edit/Delete por GET/POST como LOJISTA, VENDEDOR e anônimo recusadas (404/405); todos os campos conferidos por leitura e preservados; histórico 200 para lojista e 302 para acesso negado/login nos demais perfis | Smoke HTTP fora do TestServer; nenhuma escrita direta no banco; sem homologação no Azure ou teste visual. |
| 20/10/2026 | Histórico de rupturas do LOJISTA, commit 381224d; Chromium local com SQLite isolado, desktop 1440×1000 e smartphone emulado 390×844 | Histórico preenchido e vazio legíveis nas capturas; três numerações exibidas em ordem 38 (5), 39 (3), 37 (1); navegação móvel e tela de acesso negado do VENDEDOR registradas | Sem aparelho físico; volume maior de dados, foco por teclado e revisão por outro integrante não comprovados. Capturas e roteiro da S3-QA-018 estão em `gestao_privada/evidencias/s3-qa-018-historico-rupturas/`. |

## Homologação publicada pendente

A venda rápida mobile foi inspecionada no Azure com Firefox 156 em Linux, viewport 390×844 e conta de vendedor. O login e a consulta responderam, mas o ambiente publicado exibia uma versão antiga da grade, sem `Vendeu`, seleção de SKU ou `Nova consulta`. A massa encontrada possuía apenas saldo zero.

O aceite permaneceu bloqueado porque não foi possível validar venda com saldo disponível, último par, nova consulta, toque em aparelho físico ou todos os estados do layout. Para retomar, é necessário publicar uma versão identificada, preparar SKUs com saldos maior que 1, igual a 1 e igual a 0 e registrar dispositivo, sistema e navegador.

## Regra para novas evidências

Uma nova linha deve ser acrescentada somente quando houver validação manual ou de ambiente que a CI não consiga comprovar. O registro deve informar data, commit ou versão, ambiente, resultado e limitações. Execuções automatizadas comuns permanecem no GitHub Actions.
