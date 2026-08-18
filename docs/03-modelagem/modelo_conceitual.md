# DER Modelo Conceitual

Foco: Visão macro dos negócios e regras da organização.
Componentes: Entidades principais, relacionamentos sem cardinalidade detalhada e atributos essenciais.


### Legenda
| Elemento  | Significado         |
| --------- | ------------------- |
| Retângulo | Entidade do sistema |
| Linha     | Relacionamento      |
| (0..1)    |  Zero ou um      |
| (1..1)    | Exatamente um         |
| (0..N) |  Zero ou muitos|
| (1..N) |  Um ou muitos |

```mermaid
flowchart LR
    USUARIO[USUÁRIO]
    PRODUTO[PRODUTO]
    SKU[SKU]
    MOVIMENTACAO[MOVIMENTAÇÃO]
    RUPTURA[RUPTURA]

    PRODUTO (0..N) ─── possui ─── SKU (1..1)

    SKU (0..N) ─── gera evento operacional ─── MOVIMENTAÇÃO (1..1)

    SKU (0..N) ─── gera insight ─── RUPTURA (1..1)

    USUÁRIO (0..N) ─── realiza ─── MOVIMENTAÇÃO (1..1)

    USUÁRIO (0..N) ─── registra ─── RUPTURA (1..1)

```
