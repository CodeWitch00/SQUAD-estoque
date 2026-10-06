# DER físico implementado - Sistema SQUAD

## Identificação da auditoria

| Item              | Resultado                                                                  |
| ----------------- | -------------------------------------------------------------------------- |
| Branch            | `main`                                                                   |
| Commit            | `327efc588bf4e4480f2475a5537cf869e0153ad8`                               |
| Última migration | `20260901000759_AddEnumDomainConstraints`                                |
| ModelSnapshot     | `src/SquadEstoque.Web/Migrations/Estoque/EstoqueContextModelSnapshot.cs` |
| Banco             | SQLite —`src/SquadEstoque.Web/Estoque.db`                               |
| Fonte do diagrama | [`DER-fisico.mmd`](DER-fisico.mmd)  |

## Diagrama

O código-fonte Mermaid está em [`DER-fisico.mmd`](DER-fisico.mmd).

O desenho contém somente as tabelas realmente implementadas:

- `Usuario`
- `Produto`
- `Sku`
- `Movimentacao`
- `Ruptura`

Não foi gerado SVG porque não há Mermaid CLI, PlantUML ou Graphviz disponível no ambiente. Não foram instaladas dependências para renderização.

## Restrições e índices

### Chaves e FKs

- PK em `Usuario.Id`, `Produto.Id`, `Sku.Id`, `Movimentacao.Id` e `Ruptura.Id`.
- `Sku.ProdutoId -> Produto.Id`.
- `Movimentacao.SkuId -> Sku.Id`.
- `Movimentacao.UsuarioId -> Usuario.Id`.
- `Ruptura.SkuId -> Sku.Id`.
- `Ruptura.UsuarioId -> Usuario.Id`.
- Todas as FKs usam `ON DELETE RESTRICT`.

### Checks

- `Usuario.Perfil IN (0, 1)`.
- `Sku.SaldoAtual >= 0`.
- `Movimentacao.Quantidade > 0`.
- `Movimentacao.Tipo IN (0, 1, 2)`.

### Índices

| Nome                           | Tabela           | Colunas                  | Único |
| ------------------------------ | ---------------- | ------------------------ | -----: |
| `IX_Usuario_Email`           | `Usuario`      | `Email`                |    Sim |
| `IX_Sku_ProdutoId_Numeracao` | `Sku`          | `ProdutoId, Numeracao` |    Sim |
| `IX_Movimentacao_SkuId`      | `Movimentacao` | `SkuId`                |   Não |
| `IX_Movimentacao_UsuarioId`  | `Movimentacao` | `UsuarioId`            |   Não |
| `IX_Ruptura_SkuId`           | `Ruptura`      | `SkuId`                |   Não |
| `IX_Ruptura_UsuarioId`       | `Ruptura`      | `UsuarioId`            |   Não |

Não foram incluídos índices planejados ou ausentes, como índices sobre `Ativo`, `SaldoAtual` ou índice parcial de saldo zero.

## Tipos físicos

| Tipo C#      | Tipo SQLite |
| ------------ | ----------- |
| `Guid`     | `TEXT`    |
| `string`   | `TEXT`    |
| `DateTime` | `TEXT`    |
| `int`      | `INTEGER` |
| `bool`     | `INTEGER` |
| `enum`     | `INTEGER` |

Não foram utilizados `UUID`, `VARCHAR`, `TIMESTAMPTZ`, `BOOLEAN` nativo ou `ENUM` nativo.

## Verificação do SKU

**O modelo físico atualmente implementado não possui atributo de data de última atualização na entidade `Sku`.**

Não existem `UpdatedAt`, `AtualizadoEm`, `UltimaAtualizacao` ou campo equivalente em `Models/Entities/Sku.cs`, nas migrations, no ModelSnapshot ou no schema SQLite.

## Evidências conferidas

- `src/SquadEstoque.Web/Models/Entities/Usuario.cs`
- `src/SquadEstoque.Web/Models/Entities/Produto.cs`
- `src/SquadEstoque.Web/Models/Entities/Sku.cs`
- `src/SquadEstoque.Web/Models/Entities/Movimentacao.cs`
- `src/SquadEstoque.Web/Models/Entities/Ruptura.cs`
- `src/SquadEstoque.Web/Data/EstoqueContext.cs`
- `src/SquadEstoque.Web/Migrations/Estoque/20260817120442_InitialSquadSchema.cs`
- `src/SquadEstoque.Web/Migrations/Estoque/20260901000759_AddEnumDomainConstraints.cs`
- `src/SquadEstoque.Web/Migrations/Estoque/EstoqueContextModelSnapshot.cs`
- Schema SQLite de `src/SquadEstoque.Web/Estoque.db`

## Validação

| Item                          | Situação                                      |
| ----------------------------- | ----------------------------------------------- |
| Entidades conferidas          | 5 entidades confirmadas                         |
| Campos conferidos             | Models, migrations, snapshot e SQLite coerentes |
| Relacionamentos conferidos    | 5 relações 1:N confirmadas                    |
| Constraints conferidas        | PK, FK, UNIQUE e CHECK confirmadas              |
| Índices conferidos           | 6 índices confirmados                          |
| Campo de atualização do SKU | Não existe                                     |
| Migration mais recente        | `20260901000759_AddEnumDomainConstraints`     |
| ModelSnapshot                 | Coerente com a migration mais recente           |
| Commit utilizado              | `327efc588bf4e4480f2475a5537cf869e0153ad8`    |

## Legenda para a monografia

Figura X — Diagrama Entidade-Relacionamento do modelo implementado do Sistema SQUAD

Fonte: Elaborado pelos autores (2026).

**DER APROVADO PARA A MONOGRAFIA: SIM**
