# Diagramas do SQUAD Estoque

## Versão vigente

Os diagramas abaixo resumem a implementação atual em ASP.NET Core MVC e SQLite.
O código e as migrations continuam sendo a fonte de verdade para detalhes de
rotas, colunas e restrições.

| Assunto | Diagrama |
| --- | --- |
| Tabelas, chaves e relacionamentos | [DER vigente](der-fisico/der-vigente.md) |
| Entidades C# principais | [Classes vigentes](classe/diagrama-classe-vigente.md) |
| Atores e operações disponíveis | [Casos de uso vigentes](casos-de-uso/casos-de-uso-vigentes.md) |
| Consulta, atendimento, ajuste, NF-e e primeiro acesso | [Fluxos vigentes](fluxos/fluxos-vigentes.md) |
| Venda e ajuste com transações | [Sequências vigentes](sequencia/sequencias-vigentes.md) |

Esses arquivos usam Mermaid e são renderizados diretamente pelo GitHub.

## Material histórico

Os diagramas rasterizados da modelagem inicial e os textos UML redundantes
foram removidos desta pasta em 21/11/2026 porque não representam as entidades e
fluxos atuais. A modelagem inicial de dados permanece em `03-modelagem/`,
identificada como histórica para preservar a trilha acadêmica. O
[modelo físico inicial](../03-modelagem/modelo_fisico.md) preserva a proposta
acadêmica auditada na época; não deve ser usado como descrição do esquema atual.
