# Diagramas do SQUAD Estoque

## Versão vigente

Os diagramas abaixo resumem a implementação atual em ASP.NET Core MVC e SQLite.
O código e as migrations continuam sendo a fonte de verdade para detalhes de
rotas, colunas e restrições.

**Commit da implementação conferida:** `26f042289596a3785a4624e4ed9a82d2888c0175`.
As figuras acadêmicas de classes e DER são divididas em duas visões
complementares e selecionam atributos relevantes para caber em A4 retrato.

| Assunto | Diagrama |
| --- | --- |
| Tabelas, chaves e relacionamentos | [DER vigente](der/der-vigente.md) |
| Entidades C# principais | [Classes vigentes](classe/diagrama-classe-vigente.md) |
| Atores e operações disponíveis | [Casos de uso vigentes](casos-de-uso/casos-de-uso-vigentes.md) |
| Consulta, atendimento, ajuste, NF-e e primeiro acesso | [Fluxos vigentes](fluxos/fluxos-vigentes.md) |
| Venda bem-sucedida | [Sequência vigente](sequencia/sequencias-vigentes.md) |

Os diagramas de classes, DER fiscal e sequência têm fontes Mermaid. O DER de
catálogo usa Graphviz DOT como fonte da figura compacta, com Mermaid de
referência estrutural. O diagrama de casos de uso usa fonte Python 3. Os SVGs
e PNGs estão disponíveis nos respectivos diretórios.

## Material histórico

Os diagramas rasterizados da modelagem inicial e os textos UML redundantes
foram removidos desta pasta em 21/11/2026 porque não representam as entidades e
fluxos atuais. A modelagem inicial de dados permanece em `03-modelagem/`,
identificada como histórica para preservar a trilha acadêmica. O
[modelo físico inicial](../03-modelagem/modelo_fisico.md) preserva a proposta
acadêmica auditada na época; não deve ser usado como descrição do esquema atual.
