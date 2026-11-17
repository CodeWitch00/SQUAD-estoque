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

Os PNGs e o SVG preexistentes nesta pasta retratam o desenho inicial do MVP.
Eles não incluem `Atendimento`, `EntradaFiscal`, `ItemEntradaFiscal` nem
`VinculoProdutoFornecedor`. O antigo diagrama de sequência também usa uma rota,
uma tabela e uma operação SQL que não correspondem à implementação atual. Use
os diagramas vigentes acima para apresentar o sistema em operação. O
[DER físico inicial](der-fisico/DER-fisico.md) registra o commit que foi
auditado na época.

Os nomes `DERconceitual-atualizado.png`, `DERlogico-atualizado.png` e
`DER_fisico_SQUAD_final.svg` foram atribuídos antes das migrations posteriores;
“atualizado” e “final” nesses nomes não significam vigente hoje. Os fluxos PNG
de consulta, venda e ajuste também são representações simplificadas da versão
inicial.
