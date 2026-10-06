# DER físico implementado - Sistema SQUAD

## Identificação da auditoria

| Item              | Resultado                                                                  |
| ----------------- | -------------------------------------------------------------------------- |
| Branch            | `main`                                                                   |
| Commit de referência do modelo | `dca1bc5bb0ab87b7cd1274da11e8c5522bea49e7`                 |
| Última migration | `20260901000759_AddEnumDomainConstraints`                                |
| ModelSnapshot     | `src/SquadEstoque.Web/Migrations/Estoque/EstoqueContextModelSnapshot.cs` |
| Banco             | SQLite —`src/SquadEstoque.Web/Estoque.db`                               |
| Fonte do diagrama | [`DER-fisicom.md`](DER-fisicom.md) e [`DER_fisico_SQUAD_final.svg`](DER_fisico_SQUAD_final.svg) |

## Diagrama

O código-fonte Mermaid está em [`DER-fisicom.md`](DER-fisicom.md), e a versão renderizada aprovada está em [`DER_fisico_SQUAD_final.svg`](DER_fisico_SQUAD_final.svg).

O desenho contém somente as tabelas realmente implementadas:

- `Usuario`
- `Produto`
- `Sku`
- `Movimentacao`
- `Ruptura`

O SVG foi conferido estruturalmente com o modelo EF Core e contém as mesmas cinco entidades e cinco relacionamentos implementados.

### Cardinalidades e relacionamentos

| Relacionamento | Cardinalidade | FK obrigatória |
| --- | --- | --- |
| `Produto` → `Sku` | `1 : 0..N` | `Sku.ProdutoId -> Produto.Id` |
| `Sku` → `Movimentacao` | `1 : 0..N` | `Movimentacao.SkuId -> Sku.Id` |
| `Sku` → `Ruptura` | `1 : 0..N` | `Ruptura.SkuId -> Sku.Id` |
| `Usuario` → `Movimentacao` | `1 : 0..N` | `Movimentacao.UsuarioId -> Usuario.Id` |
| `Usuario` → `Ruptura` | `1 : 0..N` | `Ruptura.UsuarioId -> Usuario.Id` |

Cada registro dependente possui exatamente um registro principal, porque as FKs são `NOT NULL`. Um `Produto`, `Sku` ou `Usuario` pode existir sem registros dependentes, por isso o lado principal é `0..N`. Nos relacionamentos com `Usuario`, a entidade não possui coleção de navegação no C#, mas a cardinalidade física continua sendo 1:N por causa da FK repetível na tabela dependente.

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
| Cardinalidades conferidas     | 5 relações `1 : 0..N` confirmadas              |
| Constraints conferidas        | PK, FK, UNIQUE e CHECK confirmadas              |
| Índices conferidos           | 6 índices confirmados                          |
| Campo de atualização do SKU | Não existe                                     |
| Migration mais recente        | `20260901000759_AddEnumDomainConstraints`     |
| ModelSnapshot                 | Coerente com a migration mais recente           |
| Commit de referência utilizado | `dca1bc5bb0ab87b7cd1274da11e8c5522bea49e7`   |

## Legenda para a monografia

Figura X — Diagrama Entidade-Relacionamento do modelo implementado do Sistema SQUAD

Fonte: Elaborado pelos autores (2026).

**DER APROVADO PARA A MONOGRAFIA: SIM**
