# Sequências vigentes — venda e ajuste

As requisições usam as rotas MVC atuais, cookie autenticado e validação
antiforgery. O SQLite é acessado pelo Entity Framework Core; não há tabela
`Estoque` nem `SELECT ... FOR UPDATE`.

## Venda rápida de um par

```mermaid
sequenceDiagram
    actor V as VENDEDOR
    participant UI as Tela de consulta
    participant EC as EstoqueController
    participant S as SaidaEstoque
    participant DB as EstoqueContext / SQLite

    V->>UI: Seleciona SKU com saldo e confirma Vendeu
    UI->>EC: POST /Estoque/Vender (skuId, produtoId, atendimentoId)
    EC->>DB: Confere SKU ativo e atendimento do vendedor
    EC->>S: RegistrarVendaRapidaAsync
    S->>DB: BeginTransaction
    S->>DB: UPDATE Sku com SaldoAtual >= 1
    alt Uma linha atualizada
        S->>DB: Adiciona Movimentacao SAIDA e conclui Atendimento
        S->>DB: SaveChanges e Commit
        S-->>EC: Venda registrada e saldo atual
        EC-->>UI: HTTP 200
        UI-->>V: Confirma venda
    else Saldo insuficiente ou conflito
        S-->>EC: Erro; sem commit da venda
        EC-->>UI: HTTP 400 ou 409
        UI-->>V: Informa falha sem venda duplicada
    end
```

A condição `SaldoAtual >= 1` é aplicada no próprio `UPDATE`, dentro da
transação. A movimentação e a conclusão do atendimento só são confirmadas com
o commit. Veja [`EstoqueController.Vender`](../../../src/SquadEstoque.Web/Controllers/EstoqueController.cs)
e [`SaidaEstoque`](../../../src/SquadEstoque.Web/Data/SaidaEstoque.cs).

## Ajuste de saldo apurado

```mermaid
sequenceDiagram
    actor L as LOJISTA
    participant UI as Formulario de ajuste
    participant MC as MovimentacoesController
    participant DB as EstoqueContext / SQLite

    L->>UI: Informa SKU, novo saldo apurado e motivo
    UI->>MC: POST /Movimentacoes/Ajuste
    Note over UI,MC: Cookie LOJISTA e antiforgery exigidos
    MC->>MC: Valida campos e saldo nao negativo
    alt Dados invalidos
        MC-->>UI: Erros de validacao; sem escrita
    else Dados validos
        MC->>DB: BeginTransaction e carrega Sku
        alt SKU ausente ou saldo inalterado
            MC->>DB: Rollback
            MC-->>UI: Erro; saldo preservado
        else Novo saldo diferente
            MC->>DB: SaldoAtual = NovoSaldoApurado
            MC->>DB: Adiciona Movimentacao AJUSTE com abs(novo - anterior)
            MC->>DB: SaveChanges
            alt Gravacao concluida
                MC->>DB: Commit
                MC-->>UI: Redireciona para detalhes do produto
            else Falha de gravacao
                MC->>DB: Rollback
                MC-->>UI: Erro; saldo preservado
            end
        end
    end
```

O ajuste substitui o saldo pelo valor apurado; a movimentação guarda a
diferença absoluta e o motivo. O controller exige `LOJISTA` nas ações GET e
POST. Veja [`MovimentacoesController.Ajuste`](../../../src/SquadEstoque.Web/Controllers/MovimentacoesController.cs).
