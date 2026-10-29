# Testes

Esta pasta contém apenas documentação estável que não é substituída pelo código, pelo Git ou pela integração contínua.

## Fontes de verdade

| Informação | Fonte |
|---|---|
| Comportamento implementado | [`src/`](../../src/) |
| Testes automatizados | [`tests/`](../../tests/) |
| Resultado de build, testes e cobertura | [GitHub Actions](https://github.com/CodeWitch00/SQUAD-estoque/actions) |
| Alteração, revisão e decisão de merge | commits e Pull Requests |
| Estratégia, níveis e critérios gerais | [plano-de-testes.md](plano-de-testes.md) |
| Validação manual, dispositivo e homologação | [registro consolidado](homologacoes/registro-validacoes-manuais.md) |
| Resumo por sprint | [`docs/10-planejamento/`](../10-planejamento/) |
| Validações e evidências históricas recuperadas | [índice do arquivo histórico](arquivo-historico/README.md) |

## Política de manutenção

- Não criar um relatório Markdown para cada teste automatizado, cartão ou execução local.
- Não copiar para esta pasta logs, TRX, saída de console ou contagens já registradas pela CI.
- Não repetir a implementação em especificações extensas quando o teste automatizado e os requisitos já descrevem o comportamento.
- Registrar aqui somente validações manuais, testes em dispositivo, homologação publicada, limitações e decisões de aceite que não possam ser demonstradas pela CI.
- Capturas destinadas apenas ao acompanhamento do cartão devem permanecer no Trello. Uma captura só deve ser versionada quando for necessária como evidência acadêmica ou de homologação e não contiver dados sensíveis.

Relatórios, capturas e evidências removidos durante simplificações anteriores foram reorganizados em `evidencias/`, `execucoes/` e `especificacoes/arquivo-historico/`. O [índice do arquivo histórico](arquivo-historico/README.md) registra o caminho original e o commit de origem. Consulte-o antes de reutilizar material antigo; alguns arquivos refletem versões ou critérios anteriores.
