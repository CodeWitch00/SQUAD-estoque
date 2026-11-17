# Diagrama de classes vigente

Resumo das nove entidades C# persistidas. Os atributos listados são os que
explicam os vínculos e regras; consulte os [models](../../../src/SquadEstoque.Web/Models/Entities/)
para a definição completa.

```mermaid
classDiagram
    class Usuario {
        +Guid Id
        +string Email
        +string SenhaHash
        +PerfilUsuario Perfil
    }
    class Produto {
        +Guid Id
        +string Nome
        +string ImagemUrl
        +bool Ativo
    }
    class Sku {
        +Guid Id
        +Guid ProdutoId
        +string Numeracao
        +int SaldoAtual
        +DateTime? UltimaAtualizacaoSaldoEm
    }
    class Movimentacao {
        +Guid Id
        +Guid SkuId
        +Guid UsuarioId
        +TipoMovimentacao Tipo
        +int Quantidade
    }
    class Ruptura {
        +Guid Id
        +Guid SkuId
        +Guid UsuarioId
    }
    class Atendimento {
        +Guid Id
        +Guid UsuarioId
        +Guid ProdutoId
        +Guid? SkuId
        +StatusAtendimento Status
        +ResultadoAtendimento? Resultado
    }
    class EntradaFiscal {
        +Guid Id
        +Guid UsuarioId
        +string ChaveAcesso
        +StatusEntradaFiscal Status
    }
    class ItemEntradaFiscal {
        +Guid Id
        +Guid EntradaFiscalId
        +Guid? ProdutoId
        +Guid? SkuId
    }
    class VinculoProdutoFornecedor {
        +Guid Id
        +string CnpjFornecedor
        +string CodigoFornecedor
        +Guid SkuId
    }

    Produto "1" --> "0..*" Sku : possui
    Usuario "1" --> "0..*" Atendimento : conduz
    Usuario "1" --> "0..*" Movimentacao : registra
    Usuario "1" --> "0..*" Ruptura : registra
    Produto "1" --> "0..*" Atendimento : consultado
    Sku "0..1" --> "0..*" Atendimento : selecionado
    Sku "1" --> "0..*" Movimentacao : historico
    Sku "1" --> "0..*" Ruptura : demanda
    Atendimento "0..1" --> "0..1" Movimentacao : venda
    Atendimento "0..1" --> "0..1" Ruptura : nao_tinha
    Usuario "1" --> "0..*" EntradaFiscal : importa
    EntradaFiscal "1" --> "0..*" ItemEntradaFiscal : contem
    Produto "0..1" --> "0..*" ItemEntradaFiscal : associado
    Sku "0..1" --> "0..*" ItemEntradaFiscal : associado
    Sku "1" --> "0..*" VinculoProdutoFornecedor : referencia
```

`Sku.UltimaAtualizacaoSaldoEm`, `Produto.ImagemUrl`, `Atendimento.SkuId` e
`Atendimento.Resultado`, `ItemEntradaFiscal.ProdutoId` e
`ItemEntradaFiscal.SkuId` podem ser nulos. Os vínculos de `Movimentacao` e
`Ruptura` com `Atendimento` também são opcionais. Os tipos CLR exibidos aqui
não são tipos físicos do SQLite; veja o [DER vigente](../der-fisico/der-vigente.md).
