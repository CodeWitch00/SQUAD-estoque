# S3-DB-026 — Índices de consulta

## Consultas e decisões

| Índice | Consulta real beneficiada | Motivo |
| --- | --- | --- |
| `IX_Produto_Marca` | `ProdutosController.Index`: filtro `Marca == marca`; lista distinta e ordenada de marcas | Busca por igualdade e leitura da lista diretamente pelo índice. |
| `IX_Produto_Categoria` | `ProdutosController.Index`: filtro `Categoria == categoria`; lista distinta e ordenada de categorias | Busca por igualdade e leitura da lista diretamente pelo índice. |
| `IX_Sku_ProdutoId_SaldoZero` | `SaldosZeradosController.Index`: `Any` e grade com `ProdutoId` e `SaldoAtual == 0` | Índice parcial por produto contém só os SKUs zerados. O índice único existente `(ProdutoId, Numeracao)` permanece para a regra de SKU. |
| `IX_Ruptura_SkuId_CriadoEm` | `RupturasController.Index`: agrupamento por SKU, `Count` e `Max(CriadoEm)` | Substitui o índice convencional da FK `SkuId`; cobre SKU e data sem manter dois índices com o mesmo prefixo. |

A busca por trecho usa `LIKE '%termo%'` em nome e marca, que não aproveita um índice B-tree comum nessas colunas. Por isso, não foi criado índice de `Nome` para essa busca. A ordenação final das rupturas depende das contagens agregadas e continua exigindo ordenação após o agrupamento.

**Divergência preexistente:** o cartão S3-BE-008 descreve SKUs ativos, mas a consulta atual de saldos zerados filtra somente `SaldoAtual == 0`. Esta tarefa mantém o comportamento atual; qualquer correção funcional deve ser tratada separadamente.

## Evidência antes e depois

O roteiro [planos-indices-consulta.sql](../../scripts/planos-indices-consulta.sql) cria um SQLite **em memória**, com 10.000 produtos, 30.000 SKUs e 50.000 rupturas sintéticas, índices anteriores relevantes e `PRAGMA automatic_index = OFF`. Executa `ANALYZE`, coleta `EXPLAIN QUERY PLAN`, cria os índices propostos e repete os planos. A saída integral está em [s3-db-026-planos-consulta.txt](s3-db-026-planos-consulta.txt). Reproduzir com:

```sh
sqlite3 :memory: '.read scripts/planos-indices-consulta.sql'
```

Plano coletado com SQLite 3.45.1.

| Consulta representativa | Antes | Depois |
| --- | --- | --- |
| Produto por marca | `SCAN Produto` | `SEARCH Produto USING INDEX IX_Produto_Marca` |
| Marcas/categorias distintas | varredura da tabela e B-tree temporária | varredura dos índices de cobertura |
| SKU zerado por produto | busca por `(ProdutoId, Numeracao)`, com filtro de saldo | busca no índice parcial de cobertura `IX_Sku_ProdutoId_SaldoZero` |
| Rupturas por SKU e data | varredura via índice `SkuId` com leitura adicional da tabela | varredura do índice de cobertura `(SkuId, CriadoEm)` |

Os planos usam SQL representativo dos filtros e agregações dos controllers; não medem tempo de resposta HTTP nem garantem ganho em produção. No caso de saldo zero, o planejador também escolheu um índice de produto para percorrer os IDs após a criação dos índices. A evidência principal é a troca do índice usado na subconsulta de SKU.

## Migration

`AddConsultaIndexes` contém somente criação/remoção dos índices das tabelas `Produto`, `Sku` e `Ruptura`. O `Down` remove os novos índices e restaura `IX_Ruptura_SkuId`. Nenhuma tabela ou dado é alterado. A migration foi gerada e revisada, sem aplicação a banco compartilhado.

## Validação local

- `git diff --check`: aprovado.
- `dotnet ef migrations has-pending-model-changes`: nenhuma alteração pendente.
- Build Web em Release: aprovado, zero avisos e erros.
- Suíte .NET em Release: 297 aprovados, zero falhas, 1 ignorado (medição P95 opcional).
