# DER Modelo Lógico

O modelo lógico do SQUAD é organizado em quatro camadas:

1. Catálogo
2. Operação
3. Inteligência Comercial
4. Responsabilidade

---

## Camada 1 — Catálogo

```text
┌──────────────┐               possui                ┌──────────────┐
│   PRODUTO    │ (0..N) ───────────────────── (1..1)│     SKU      │
└──────────────┘                                    └──────────────┘

Item comercial                                     Unidade de estoque
do catálogo                                         controlada por numeração
```

---

## Camada 2 — Operação

```text
┌──────────────┐     gera evento operacional     ┌─────────────────┐
│     SKU      │ (0..N) ────────────────── (1..1)│  MOVIMENTACAO   │
└──────────────┘                                 └─────────────────┘

Unidade consultada                               Registro de entradas,
no estoque                                       saídas e ajustes
```

```text
┌──────────────┐          gera insight           ┌──────────────┐
│     SKU      │ (0..N) ──────────────── (1..1) │   RUPTURA    │
└──────────────┘                                └──────────────┘

Unidade consultada                              Demanda não atendida
no estoque                                      registrada pelo vendedor
```

---

## Camada 3 — Responsabilidade

```text
┌──────────────┐            realiza             ┌─────────────────┐
│   USUARIO    │ (0..N) ─────────────── (1..1) │  MOVIMENTACAO   │
└──────────────┘                               └─────────────────┘

Responsável pelos                               Evento operacional
registros operacionais                          realizado no sistema
```

```text
┌──────────────┐            registra            ┌──────────────┐
│   USUARIO    │ (0..N) ────────────── (1..1)  │   RUPTURA    │
└──────────────┘                               └──────────────┘

Responsável pelo                                Registro de ruptura
registro da ocorrência                          de estoque
```

---

## Cardinalidades

| Relacionamento         | Cardinalidade |
| ---------------------- | ------------- |
| PRODUTO → SKU          | 0..N : 1..1   |
| SKU → MOVIMENTACAO     | 0..N : 1..1   |
| SKU → RUPTURA          | 0..N : 1..1   |
| USUARIO → MOVIMENTACAO | 0..N : 1..1   |
| USUARIO → RUPTURA      | 0..N : 1..1   |

---

## Legenda

| Notação | Significado    |
| ------- | -------------- |
| 0..1    | Zero ou um     |
| 1..1    | Exatamente um  |
| 0..N    | Zero ou muitos |
| 1..N    | Um ou muitos   |

A cardinalidade é apresentada sempre na forma:

```text
(entidade origem) (mín..máx) ─── relacionamento ─── (mín..máx) (entidade destino)
```


### Regras de Negócio Aplicadas
| Regra | Aplicação                                  |
| ----- | ------------------------------------------ |
| RN-01 | SKU é único por produto + numeração        |
| RN-02 | saldo_atual nunca pode ser negativo        |
| RN-03 | movimentações são imutáveis                |
| RN-04 | apenas LOJISTA realiza ajuste              |
| RN-05 | ruptura não altera saldo                   |
| RN-06 | ruptura obrigatoriamente pertence a um SKU |
