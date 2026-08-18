# Dicionário de Dados

# SQUAD - Sistema de Controle de Estoque para Varejo de Calçados

---

# 1. Objetivo

Este documento descreve detalhadamente todas as estruturas de dados persistidas no banco PostgreSQL do sistema SQUAD.

Seu objetivo é servir como referência para:

* Desenvolvimento
* Testes
* Integrações futuras
* Auditoria
* Manutenção do sistema

---

# 2. Tipos Enumerados

## perfil_usuario

Define os perfis de acesso do sistema.

| Valor    |
| -------- |
| VENDEDOR |
| LOJISTA  |

---

## tipo_movimentacao

Define os tipos de movimentação de estoque.

| Valor   |
| ------- |
| ENTRADA |
| SAIDA   |
| AJUSTE  |

---

# 3. Tabela USUARIO

## Descrição

Armazena os usuários autenticados do sistema.

Perfis suportados:

* VENDEDOR
* LOJISTA

---

| Campo      | Tipo           | Obrigatório | Chave  | Descrição                      |
| ---------- | -------------- | ----------- | ------ | ------------------------------ |
| id         | UUID           | Sim         | PK     | Identificador único do usuário |
| nome       | VARCHAR(150)   | Sim         |        | Nome completo                  |
| email      | VARCHAR(255)   | Sim         | UNIQUE | E-mail utilizado para login    |
| senha_hash | VARCHAR(255)   | Sim         |        | Hash da senha                  |
| perfil     | perfil_usuario | Sim         |        | Perfil de acesso               |

---

## Restrições

### PK

```sql
PRIMARY KEY (id)
```

### E-mail único

```sql
UNIQUE (email)
```

---

# 4. Tabela PRODUTO

## Descrição

Representa um modelo de calçado comercializado pela loja.

Não controla estoque diretamente.

O estoque é controlado através dos SKUs.

---

| Campo     | Tipo         | Obrigatório | Chave | Descrição                      |
| --------- | ------------ | ----------- | ----- | ------------------------------ |
| id        | UUID         | Sim         | PK    | Identificador do produto       |
| nome      | VARCHAR(200) | Sim         |       | Nome do modelo                 |
| marca     | VARCHAR(100) | Sim         |       | Marca do produto               |
| categoria | VARCHAR(100) | Sim         |       | Categoria comercial            |
| cor       | VARCHAR(80)  | Sim         |       | Cor do produto                 |
| ativo     | BOOLEAN      | Sim         |       | Indica se o produto está ativo |

---

## Valores padrão

```sql
ativo = TRUE
```

---

# 5. Tabela SKU

## Descrição

Representa a menor unidade controlada pelo sistema.

Cada SKU corresponde a:

```text
Produto + Numeração
```

---

| Campo       | Tipo        | Obrigatório | Chave | Descrição                  |
| ----------- | ----------- | ----------- | ----- | -------------------------- |
| id          | UUID        | Sim         | PK    | Identificador do SKU       |
| produto_id  | UUID        | Sim         | FK    | Produto associado          |
| numeracao   | VARCHAR(10) | Sim         |       | Numeração do calçado       |
| saldo_atual | INTEGER     | Sim         |       | Quantidade disponível      |
| ativo       | BOOLEAN     | Sim         |       | Indica se o SKU está ativo |

---

## Chaves Estrangeiras

```sql
FOREIGN KEY (produto_id)
REFERENCES produto(id)
```

---

## Restrições

### SKU único

```sql
UNIQUE (produto_id, numeracao)
```

Impede duplicação de numeração dentro do mesmo produto.

---

### Saldo não negativo

```sql
CHECK (saldo_atual >= 0)
```

Implementa a regra de negócio:

```text
RN-02 — Saldo nunca negativo
```

---

## Valores padrão

```sql
saldo_atual = 0
ativo = TRUE
```

---

# 6. Tabela MOVIMENTACAO

## Descrição

Armazena todas as alterações realizadas no estoque.

É uma entidade imutável.

Nenhuma movimentação deve ser alterada ou removida.

---

| Campo      | Tipo              | Obrigatório | Chave | Descrição                     |
| ---------- | ----------------- | ----------- | ----- | ----------------------------- |
| id         | UUID              | Sim         | PK    | Identificador da movimentação |
| sku_id     | UUID              | Sim         | FK    | SKU movimentado               |
| tipo       | tipo_movimentacao | Sim         |       | Tipo da movimentação          |
| quantidade | INTEGER           | Sim         |       | Quantidade movimentada        |
| usuario_id | UUID              | Sim         | FK    | Usuário responsável           |
| criado_em  | TIMESTAMPTZ       | Sim         |       | Data e hora da movimentação   |
| motivo     | TEXT              | Não         |       | Justificativa da movimentação |

---

## Chaves Estrangeiras

```sql
FOREIGN KEY (sku_id)
REFERENCES sku(id)
```

```sql
FOREIGN KEY (usuario_id)
REFERENCES usuario(id)
```

---

## Restrições

### Quantidade positiva

```sql
CHECK (quantidade > 0)
```

---

## Valores padrão

```sql
criado_em = NOW()
```

---

## Observações

A aplicação exige preenchimento de motivo para ajustes de estoque.

---

# 7. Tabela RUPTURA

## Descrição

Registra demandas não atendidas.

Uma ruptura ocorre quando o vendedor informa:

```text
Não tinha
```

---

| Campo      | Tipo        | Obrigatório | Chave | Descrição                 |
| ---------- | ----------- | ----------- | ----- | ------------------------- |
| id         | UUID        | Sim         | PK    | Identificador da ruptura  |
| sku_id     | UUID        | Sim         | FK    | SKU solicitado            |
| usuario_id | UUID        | Sim         | FK    | Usuário que registrou     |
| criado_em  | TIMESTAMPTZ | Sim         |       | Data e hora da ocorrência |

---

## Chaves Estrangeiras

```sql
FOREIGN KEY (sku_id)
REFERENCES sku(id)
```

```sql
FOREIGN KEY (usuario_id)
REFERENCES usuario(id)
```

---

## Valores padrão

```sql
criado_em = NOW()
```

---

## Regras

* Não altera estoque.
* Sempre vinculada a um SKU.
* Sempre vinculada a um usuário.

---

# 8. Relacionamentos

```text
PRODUTO (0..N) ─── SKU (1..1)

SKU (0..N) ─── MOVIMENTAÇÃO (1..1)

SKU (0..N) ─── RUPTURA (1..1)

USUÁRIO (0..N) ─── MOVIMENTAÇÃO (1..1)

USUÁRIO (0..N) ─── RUPTURA (1..1)
```

---

# 9. Índices

## USUARIO

```sql
idx_usuario_email
```

Finalidade:

* Busca de usuário durante login.

---

## PRODUTO

```sql
idx_produto_ativo
```

Finalidade:

* Filtrar produtos ativos.

---

## SKU

```sql
idx_sku_produto_id
```

Finalidade:

* Carregar grade de numerações.

---

```sql
idx_sku_ativo
```

Finalidade:

* Filtrar SKUs ativos.

---

```sql
idx_sku_saldo_zerado
```

Finalidade:

* Listar SKUs sem estoque.

---

## MOVIMENTACAO

```sql
idx_movimentacao_sku_id
```

Finalidade:

* Consultar histórico de movimentações.

---

## RUPTURA

```sql
idx_ruptura_sku_id
```

Finalidade:

* Consultar histórico e estatísticas de rupturas.

---

# 10. Regras de Integridade

## RN-01

SKU é único por:

```text
Produto + Numeração
```

---

## RN-02

Saldo nunca pode ser negativo.

---

## RN-03

Movimentações são imutáveis.

---

## RN-04

Ajustes apenas por usuários LOJISTA.

---

## RN-05

Ruptura somente mediante registro explícito.

---

## RN-06

Toda ruptura deve estar vinculada a um SKU válido.

---

## RN-07

Operações de saída devem ser atômicas utilizando transações e bloqueio de linha (`SELECT ... FOR UPDATE`).
