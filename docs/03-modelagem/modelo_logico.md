# Modelo Lógico 
> **Para quem é este documento:** desenvolvedores 

---

## 1. Diagrama de Relacionamentos

```text
┌──────────────────────────────────────────────────────────────────────────────┐
│                                                                              │
│  USUARIO                                                                     │
│  PK id                                                                       │
│     nome                                                                     │
│     email (unique)                                                           │
│     senha_hash                                                               │
│     perfil [VENDEDOR | LOJISTA]                                              │
│                                                                              │
│       │ 0..N                               │ 0..N                            │
│       │ realiza                            │ registra                        │
│       │ 1..1                               │ 1..1                            │
│       ▼                                    ▼                                 │
│  MOVIMENTACAO                         RUPTURA                               │
│  PK id                                PK id                                 │
│  FK sku_id ──────────┐            FK sku_id ──────────┐                     │
│  FK usuario_id       │            FK usuario_id       │                     │
│     tipo             │               criado_em        │                     │
│     quantidade       │                                │                     │
│     motivo           │                                │                     │
│     criado_em        │ 1..1                           │ 1..1                │
│                      │ gera / registra                │ gera insight        │
│                      │ 0..N                           │ 0..N                │
│                      └──────────► SKU ◄───────────────┘                     │
│                                  PK id                                      │
│                               FK produto_id                                 │
│                                  numeracao                                  │
│                                  saldo_atual                                │
│                                  ativo                                      │
│                                                                              │
│                                       │ 1..1                                │
│                                       │ pertence a                          │
│                                       │ 0..N                                │
│                                       ▼                                     │
│                                  PRODUTO                                    │
│                                  PK id                                      │
│                                     nome                                    │
│                                     marca                                   │
│                                     categoria                               │
│                                     cor                                     │
│                                     ativo                                   │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```
## Cardinalidades

| Relacionamento         | Participação                         |
| ---------------------- | ------------------------------------ |
| PRODUTO → SKU          | PRODUTO (0..N) / SKU (1..1)          |
| SKU → MOVIMENTACAO     | SKU (0..N) / MOVIMENTACAO (1..1)     |
| SKU → RUPTURA          | SKU (0..N) / RUPTURA (1..1)          |
| USUARIO → MOVIMENTACAO | USUARIO (0..N) / MOVIMENTACAO (1..1) |
| USUARIO → RUPTURA      | USUARIO (0..N) / RUPTURA (1..1)      |

### Legenda

| Notação | Significado    |
| ------- | -------------- |
| 0..1    | Zero ou um     |
| 1..1    | Exatamente um  |
| 0..N    | Zero ou muitos |
| 1..N    | Um ou muitos   |

```
```

---

## 2. Tabelas

### 2.1 USUARIO

| Coluna       | Tipo genérico | Nulo | Padrão | Constraint                 | Origem        |
|--------------|---------------|------|--------|----------------------------|---------------|
| `id`         | Identificador | Não  | —      | PK                         | dominio.md    |
| `nome`       | Texto         | Não  | —      | —                          | dominio.md    |
| `email`      | Texto         | Não  | —      | UNIQUE                     | RF-01         |
| `senha_hash` | Texto         | Não  | —      | —                          | RF-04, RNF-04 |
| `perfil`     | Enum          | Não  | —      | IN (VENDEDOR, LOJISTA)     | RF-02         |

**Regra de negócio aplicada:** `perfil` controla o acesso às funções do sistema (RN-04). O campo `senha_hash` nunca armazena a senha original.

---

### 2.2 PRODUTO

| Coluna      | Tipo genérico | Nulo | Padrão | Constraint | Origem                    |
|-------------|---------------|------|--------|------------|---------------------------|
| `id`        | Identificador | Não  | —      | PK         | dominio.md                |
| `nome`      | Texto         | Não  | —      | —          | RF-05                     |
| `marca`     | Texto         | Não  | —      | —          | RF-05                     |
| `categoria` | Texto         | Não  | —      | —          | RF-05                     |
| `cor`       | Texto         | Não  | —      | —          | RF-05                     |
| `ativo`     | Booleano      | Não  | true   | —          |  |

**Regra de negócio aplicada:** quando `ativo = false`, o produto não aparece na busca do vendedor mas seu histórico permanece intacto no banco.

---

### 2.3 SKU

| Coluna        | Tipo genérico | Nulo | Padrão | Constraint                     | Origem                    |
|---------------|---------------|------|--------|--------------------------------|---------------------------|
| `id`          | Identificador | Não  | —      | PK                             | RF-07                     |
| `produto_id`  | Identificador | Não  | —      | FK → PRODUTO(id)               | dominio.md                |
| `numeracao`   | Texto         | Não  | —      | —                              | RF-06                     |
| `saldo_atual` | Inteiro       | Não  | 0      | CHECK (saldo_atual >= 0)       | RF-11, RN-02              |
| `ativo`       | Booleano      | Não  | true   | —                              | |

**Constraint composta:** `UNIQUE (produto_id, numeracao)` → garante RN-01.

**Estados derivados de `saldo_atual` em tempo de consulta, sem coluna extra:**

| Estado       | Condição          |
|--------------|-------------------|
| Disponível   | `saldo_atual > 1` |
| Último par   | `saldo_atual = 1` |
| Indisponível | `saldo_atual = 0` |

---

### 2.4 MOVIMENTACAO

| Coluna       | Tipo genérico | Nulo | Padrão          | Constraint                      | Origem              |
|--------------|---------------|------|-----------------|---------------------------------|---------------------|
| `id`         | Identificador | Não  | —               | PK                              | dominio.md          |
| `sku_id`     | Identificador | Não  | —               | FK → SKU(id)                    | RF-10               |
| `tipo`       | Enum          | Não  | —               | IN (ENTRADA, SAIDA, AJUSTE)     | RF-10               |
| `quantidade` | Inteiro       | Não  | —               | CHECK (quantidade > 0)          | RF-10               |
| `usuario_id` | Identificador | Não  | —               | FK → USUARIO(id)                | RF-10               |
| `criado_em`  | Data/Hora     | Não  | agora (sistema) | —                               | RF-10               |
| `motivo`     | Texto longo   | Sim  | null            | —                               | RF-23 |

**Regras de negócio aplicadas:**
- Sem operações de UPDATE ou DELETE nesta tabela → RN-03
- `motivo` é exigido pela aplicação quando `tipo = AJUSTE`, nulo nos demais
- Apenas `perfil = LOJISTA` pode criar `tipo = AJUSTE` → RN-04 (validado na aplicação)
- Leitura + validação + decremento ocorrem em transação única → RN-07

---

### 2.5 RUPTURA

| Coluna       | Tipo genérico | Nulo | Padrão          | Constraint       | Origem     |
|--------------|---------------|------|-----------------|------------------|------------|
| `id`         | Identificador | Não  | —               | PK               | dominio.md |
| `sku_id`     | Identificador | Não  | —               | FK → SKU(id)     | RN-06      |
| `usuario_id` | Identificador | Não  | —               | FK → USUARIO(id) | dominio.md |
| `criado_em`  | Data/Hora     | Não  | agora (sistema) | —                | RF-22      |

**Regras de negócio aplicadas:**
- `sku_id NOT NULL` → RN-06: toda ruptura precisa de um SKU específico
- Sem campo de saldo - não altera estoque (RF-18, RN-05)
- Criada exclusivamente pela ação do vendedor ao marcar "Não tinha" → RN-05

---

## 3. Relacionamentos

| De           | Para    | Coluna FK                 | Obrigatório | Regra de origem                      |
|--------------|---------|---------------------------|-------------|--------------------------------------|
| SKU          | PRODUTO | `SKU.produto_id`          | sim          | RN-01 - SKU = Produto + Numeração    |
| MOVIMENTACAO | SKU     | `MOVIMENTACAO.sku_id`     | sim          | RF-10 - movimentação pertence a SKU  |
| MOVIMENTACAO | USUARIO | `MOVIMENTACAO.usuario_id` | sim          | RF-10 - movimentação tem responsável |
| RUPTURA      | SKU     | `RUPTURA.sku_id`          | sim          | RN-06 - ruptura sem SKU é inválida   |
| RUPTURA      | USUARIO | `RUPTURA.usuario_id`      | sim          | Rastreabilidade do declarante        |

---

## 4. Índices

| Nome do índice            | Tabela       | Colunas       | Motivo                                       |
|---------------------------|--------------|---------------|----------------------------------------------|
| `idx_usuario_email`       | USUARIO      | `email`       | Login - busca por e-mail em toda autenticação|
| `idx_produto_ativo`       | PRODUTO      | `ativo`       | Filtrar apenas produtos ativos na busca      |
| `idx_sku_produto_id`      | SKU          | `produto_id`  | Carregar grade de numerações de um produto   |
| `idx_sku_ativo`           | SKU          | `ativo`       | Filtrar apenas SKUs ativos na busca          |
| `idx_sku_saldo_zerado`    | SKU          | `saldo_atual` | Listar SKUs com saldo zero (RF-21)           |
| `idx_movimentacao_sku_id` | MOVIMENTACAO | `sku_id`      | Histórico de movimentações por SKU           |
| `idx_ruptura_sku_id`      | RUPTURA      | `sku_id`      | Histórico e contagem de rupturas por SKU     |

---

## 5. Rastreabilidade Requisito → Tabela → Coluna

| Requisito | Tabela(s)            | Coluna(s) chave                     |
|-----------|----------------------|-------------------------------------|
| RF-01     | USUARIO              | `email`, `senha_hash`               |
| RF-02     | USUARIO              | `perfil`                            |
| RF-04     | USUARIO              | `senha_hash`                        |
| RF-05     | PRODUTO              | `nome`, `marca`, `categoria`, `cor` |
| RF-06     | SKU                  | `numeracao`                         |
| RF-07     | SKU                  | `id` (gerado automaticamente)       |
| RF-08     | SKU                  | `UNIQUE (produto_id, numeracao)`    |
| RF-09     | MOVIMENTACAO         | `tipo = ENTRADA`                    |
| RF-10     | MOVIMENTACAO         | todas as colunas                    |
| RF-11     | SKU                  | `CHECK (saldo_atual >= 0)`          |
| RF-13/14  | PRODUTO, SKU         | `nome`, `saldo_atual`               |
| RF-15     | SKU                  | `saldo_atual` (derivado em query)   |
| RF-17     | MOVIMENTACAO         | `tipo = SAIDA`                      |
| RF-18     | RUPTURA              | todas as colunas                    |
| RF-21     | SKU                  | `saldo_atual = 0`                   |
| RF-22     | RUPTURA              | `sku_id`, `criado_em`               |
| RF-23     | MOVIMENTACAO         | `tipo = AJUSTE`, `motivo`           |
| RN-01     | SKU                  | `UNIQUE (produto_id, numeracao)`    |
| RN-02     | SKU                  | `CHECK (saldo_atual >= 0)`          |
| RN-03     | MOVIMENTACAO         | sem UPDATE/DELETE                   |
| RN-04     | USUARIO              | `perfil` + validação aplicação      |
| RN-05     | RUPTURA              | sem criação automática              |
| RN-06     | RUPTURA              | `sku_id NOT NULL`                   |
| RN-07     | SKU + MOVIMENTACAO   | transação atômica                   |
