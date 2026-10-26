# S3-BE-023 — Última alteração efetiva de saldo

Data: 26/10/2026. Requisito: RF-12.
Branch: `feat/s3-be-023-emmy`, criada de `dev/emmy` em `c09957d`.

## Implementação

- Campo `Sku.UltimaAtualizacaoSaldoEm` (`DateTime?`), gravado em UTC e materializado com `DateTimeKind.Utc`.
- Valor inicial `null` para SKUs existentes: instante anterior desconhecido, sem inventar data de movimentação.
- Atualização em entrada individual, lote, entrada fiscal confirmada, saldo inicial positivo, venda, saída e ajuste efetivo.
- Saída/venda atualizam saldo e data no mesmo comando SQL e na mesma transação da movimentação.
- Ruptura, Desistiu, saldo insuficiente, ajuste sem diferença, motivo inválido e falha de persistência preservam a data persistida.

## Migration

`20261026220326_AddSkuUltimaAtualizacaoSaldo` adiciona somente uma coluna nullable `TEXT` ao SKU. O Down remove somente essa coluna. Os demais campos e constraints permanecem iguais.

SQL revisável: [migration.sql](evidencias/s3-be-023/migration.sql).
Aplicação manual por `dotnet ef database update`, em SQLite isolado: [migration-aplicada.txt](evidencias/s3-be-023/migration-aplicada.txt). A execução terminou em `Done.` e registrou a migration em `__EFMigrationsHistory`. Nenhum banco compartilhado foi usado.

O teste `SkuUltimaAtualizacaoMigrationTests` aplica a versão anterior, insere SKU com saldo 7, atualiza o schema, verifica saldo preservado e data null, grava e relê UTC, reverte e reaplica a migration.

## Validação

- `git diff --check` sem erros.
- Build: [build.txt](evidencias/s3-be-023/build.txt).
- Suíte automatizada: 269 aprovados, 0 falhas e 0 ignorados. Logs: [testes.txt](evidencias/s3-be-023/testes.txt).
- `dotnet ef migrations has-pending-model-changes`: nenhuma mudança pendente.
- Cobertura funcional: operações confirmadas e rejeitadas, rollback transacional, entrada fiscal, lote com quantidades vazias/zero, atendimento Desistiu e migração de dados existentes.

A exibição da data nas telas não integra este cartão. A revisão de CodeWitch00 será solicitada no PR para `dev/emmy`. A aprovação depende da revisão no GitHub.
