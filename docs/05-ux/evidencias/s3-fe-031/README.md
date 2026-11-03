# S3-FE-031 — Visões do lojista

Referências: RF-21/UC-11/US-12 (saldos zerados), RF-22/UC-12/US-13 (rupturas), RNF-01 e RNF-07. Escopo aprovado: implementação, validação, commit, envio da branch e PR; Trello mantido intacto.

## Resultado

- Nomes, marcas e cores longos quebram linha sem truncamento. Os grupos de saldos zerados têm título associado por `aria-labelledby`.
- Rupturas usam linhas empilhadas abaixo de 768 px; abaixo de 360 px, rótulo e valor ficam em uma coluna. No desktop, as proporções reservam espaço para numeração, ocorrências e data.
- Agrupamentos, filtros de negócio e prioridade por ocorrências/data permanecem iguais. Nenhum gráfico ou filtro novo.
- Falhas de banco retornam HTTP 503 com mensagem em português, `role="alert"` e link para tentar novamente. A mensagem não revela detalhes técnicos nem confunde falha com ausência de registros; a exceção é registrada no log.
- Estados carregado, vazio e erro estão representados. As páginas são renderizadas no servidor; não foi criado carregamento assíncrono.

## Validação executada — 01/11/2026

Windows, .NET 10, Edge headless (Playwright), banco SQLite descartável e isolado. Dados de demonstração incluem nomes sem espaços próximos ao limite de tamanho, três produtos, 27 SKUs zerados e rupturas com contagens diferentes.

| Verificação | Resultado |
| --- | --- |
| `dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj --no-restore` | Sucesso, zero erros |
| `dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-restore` | 306 aprovados, 0 falhas, 1 ignorado pela configuração (P95) |
| `git diff --check` | Sem erros |
| Duas telas em 320, 390, 768 e 1440 px | Sem rolagem horizontal da página ou texto cortado nas células e títulos inspecionados |
| Link entre relatórios e recuperação após erro | Foco visível e ativação por Enter |
| Estado vazio | Orientação exibida em ambas as telas |
| Falha real de consulta no banco descartável | HTTP 503, alerta e recuperação após restauração da tabela |

O restore em cache emitiu NU1900 por indisponibilidade da consulta de vulnerabilidades do NuGet. Isso não impediu build ou testes. A validação visual foi automatizada e complementada por inspeção das capturas; não representa homologação por outro integrante, teste com leitor de tela ou validação em Safari/dispositivo físico.

Os dois testes novos em `RelatoriosErroHttpTests` autenticam um lojista, tornam indisponível a tabela no SQLite em memória exclusivo do teste e verificam resposta 503, alerta, recuperação disponível e ausência de mensagem de lista vazia ou detalhes SQL.

## Evidências

- [Saldos zerados — 320 px](SaldosZerados-320.png), [390 px](SaldosZerados-390.png), [768 px](SaldosZerados-768.png), [1440 px](SaldosZerados-1440.png).
- [Rupturas — 320 px](Rupturas-320.png), [390 px](Rupturas-390.png), [768 px](Rupturas-768.png), [1440 px](Rupturas-1440.png).
- [Saldos vazios](SaldosZerados-vazio.png), [rupturas vazias](Rupturas-vazio.png).
- [Falha em saldos](SaldosZerados-erro.png), [falha em rupturas](Rupturas-erro.png).
- [Resultados das verificações no navegador](resultados.json).

## Roteiro para revisão

1. Executar a aplicação e entrar como lojista em ambiente de demonstração.
2. Consultar `/SaldosZerados` e `/Rupturas` nas quatro larguras acima, incluindo produtos com textos longos.
3. Conferir agrupamento por produto, prioridade de rupturas, datas e numerações. Usar Tab e Enter nos links.
4. Conferir o estado vazio em banco demonstrativo sem registros correspondentes.
5. Executar os testes de erro; não provocar falhas nem alterar tabelas em banco compartilhado ou de produção.

Revisão por outro integrante e merge permanecem pendentes. Não houve movimentação, edição ou registro de evidências no Trello, conforme autorização.
