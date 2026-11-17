# Modelo de dados vigente

O esquema SQLite da aplicação é definido pelo
[`EstoqueContext`](../../src/SquadEstoque.Web/Data/EstoqueContext.cs), pelas
[migrations](../../src/SquadEstoque.Web/Migrations/Estoque/) e pelo
[`ModelSnapshot`](../../src/SquadEstoque.Web/Migrations/Estoque/EstoqueContextModelSnapshot.cs).
O arquivo local `Estoque.db` não é fonte de verdade. Esta página resume o modelo
em 10/11/2026; os detalhes físicos devem ser conferidos nesses arquivos de código.
O [DER vigente](../06-uml/der-fisico/der-vigente.md) apresenta as nove tabelas
e seus relacionamentos principais.

| Tabela | Papel principal |
| --- | --- |
| `Usuario` | Conta com perfil `VENDEDOR` ou `LOJISTA` e senha em hash. |
| `Produto` | Modelo de calçado e foto opcional. |
| `Sku` | Numeração de um produto, saldo e última atualização do saldo. |
| `Movimentacao` | Histórico de entrada, saída e ajuste vinculado a SKU e usuário; pode vincular um atendimento. |
| `Ruptura` | Demanda não atendida de um SKU, registrada por usuário; pode vincular um atendimento. |
| `Atendimento` | Consulta do vendedor, pendente ou concluída, vinculada a produto e opcionalmente a SKU. |
| `EntradaFiscal` | NF-e importada como rascunho ou confirmada, vinculada ao usuário. |
| `ItemEntradaFiscal` | Item da entrada fiscal, com vínculos opcionais para produto e SKU durante a conferência. |
| `VinculoProdutoFornecedor` | Correspondência entre fornecedor/código do item e SKU. |

As regras centrais no banco incluem: SKU único por `(ProdutoId, Numeracao)`,
saldo não negativo, e-mail único, quantidade de movimentação positiva, chave
de acesso da NF-e única e vínculo único por fornecedor/código. O
[`EstoqueContext`](../../src/SquadEstoque.Web/Data/EstoqueContext.cs) também
define chaves estrangeiras, índices e restrições de estado dos atendimentos.

## Documentos de origem

Os arquivos abaixo registram o desenho acadêmico inicial de cinco tabelas.
Permanecem para rastreabilidade, mas não descrevem o esquema completo vigente:

- [Modelo conceitual](modelo_conceitual.md)
- [Modelo lógico](modelo_logico.md)
- [Modelo físico e SQL proposto](modelo_fisico.md)
- [Dicionário de dados inicial](dicionario-de-dados.md)
- [DER físico auditado na época](../06-uml/der-fisico/DER-fisico.md)
