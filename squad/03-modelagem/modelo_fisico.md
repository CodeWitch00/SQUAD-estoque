# Modelo Físico 

> **Para quem é este documento:** desenvolvedores 
> <br>**Banco de dados:** PostgreSQL 

---

## 1. Tipos customizados (ENUMs)

Os ENUMs são criados antes das tabelas, pois as tabelas dependem deles.

```sql
-- Perfis de acesso do sistema (RF-02, RN-04)
CREATE TYPE perfil_usuario AS ENUM ('VENDEDOR', 'LOJISTA');

-- Tipos de movimentação de estoque (RF-10)
CREATE TYPE tipo_movimentacao AS ENUM ('ENTRADA', 'SAIDA', 'AJUSTE');
```

---

## 2. Tabelas

A ordem de criação respeita as dependências entre chaves estrangeiras:
`USUARIO` → `PRODUTO` → `SKU` → `MOVIMENTACAO` e `RUPTURA`

---

### 2.1 USUARIO

```sql
CREATE TABLE usuario (
    id          UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    nome        VARCHAR(150)    NOT NULL,
    email       VARCHAR(255)    NOT NULL,
    senha_hash  VARCHAR(255)    NOT NULL,
    perfil      perfil_usuario  NOT NULL,

    CONSTRAINT uq_usuario_email UNIQUE (email)
);
```

**Decisões de tipo:**
- `UUID` com `gen_random_uuid()` → identificador único sem colisão, sem exposição de sequência
- `VARCHAR(255)` para email → limite padrão de e-mail conforme RFC 5321
- `VARCHAR(255)` para senha_hash → bcrypt com custo 12 produz hash de 60 caracteres; 255 dá margem para outros algoritmos futuros

---

### 2.2 PRODUTO

```sql
CREATE TABLE produto (
    id          UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    nome        VARCHAR(200)    NOT NULL,
    marca       VARCHAR(100)    NOT NULL,
    categoria   VARCHAR(100)    NOT NULL,
    cor         VARCHAR(80)     NOT NULL,
    ativo       BOOLEAN         NOT NULL DEFAULT TRUE
);
```

**Decisões de tipo:**
- Todos os campos descritivos como `VARCHAR` com limites razoáveis para nomes comerciais
- `ativo DEFAULT TRUE` → todo produto criado nasce ativo; só é desativado por ação explícita do lojista

---

### 2.3 SKU

```sql
CREATE TABLE sku (
    id           UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    produto_id   UUID        NOT NULL,
    numeracao    VARCHAR(10) NOT NULL,
    saldo_atual  INTEGER     NOT NULL DEFAULT 0,
    ativo        BOOLEAN     NOT NULL DEFAULT TRUE,

    CONSTRAINT fk_sku_produto
        FOREIGN KEY (produto_id) REFERENCES produto(id),

    CONSTRAINT uq_sku_produto_numeracao
        UNIQUE (produto_id, numeracao),

    CONSTRAINT chk_sku_saldo_nao_negativo
        CHECK (saldo_atual >= 0)
);
```

**Decisões de tipo:**
- `VARCHAR(10)` para numeracao → cobre numerações brasileiras (ex: "33", "44", "45/46") com folga
- `INTEGER` para saldo_atual → saldo de pares de sapato nunca chegará ao limite de INTEGER (2.147.483.647)
- `DEFAULT 0` → SKU nasce sem estoque; estoque é adicionado via entrada
- `CHECK (saldo_atual >= 0)` → segunda linha de defesa contra saldo negativo (RN-02). A primeira é a transação atômica.

---

### 2.4 MOVIMENTACAO

```sql
CREATE TABLE movimentacao (
    id           UUID               PRIMARY KEY DEFAULT gen_random_uuid(),
    sku_id       UUID               NOT NULL,
    tipo         tipo_movimentacao  NOT NULL,
    quantidade   INTEGER            NOT NULL,
    usuario_id   UUID               NOT NULL,
    criado_em    TIMESTAMPTZ        NOT NULL DEFAULT NOW(),
    motivo       TEXT,

    CONSTRAINT fk_movimentacao_sku
        FOREIGN KEY (sku_id) REFERENCES sku(id),

    CONSTRAINT fk_movimentacao_usuario
        FOREIGN KEY (usuario_id) REFERENCES usuario(id),

    CONSTRAINT chk_movimentacao_quantidade_positiva
        CHECK (quantidade > 0)
);
```

**Decisões de tipo:**
- `TIMESTAMPTZ` (timestamp with time zone) → armazena o fuso horário junto; essencial para lojas que possam operar em múltiplos fusos no futuro e para auditoria precisa
- `TEXT` para motivo → sem limite arbitrário de caracteres; o lojista deve poder descrever o motivo livremente
- `motivo` sem `NOT NULL` → o banco permite nulo; a aplicação valida e exige preenchimento quando `tipo = AJUSTE`
- **Sem colunas `updated_at` ou `deleted_at`** → esta tabela é imutável por definição (RN-03)

**Nota sobre imutabilidade (RN-03):** a garantia de imutabilidade é feita na camada de aplicação (sem endpoints de PUT/PATCH/DELETE para esta tabela) e pode ser reforçada com uma rule ou trigger no PostgreSQL se necessário.

---

### 2.5 RUPTURA

```sql
CREATE TABLE ruptura (
    id           UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    sku_id       UUID        NOT NULL,
    usuario_id   UUID        NOT NULL,
    criado_em    TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_ruptura_sku
        FOREIGN KEY (sku_id) REFERENCES sku(id),

    CONSTRAINT fk_ruptura_usuario
        FOREIGN KEY (usuario_id) REFERENCES usuario(id)
);
```

**Decisões de tipo:**
- `TIMESTAMPTZ` pelo mesmo motivo que MOVIMENTACAO — rastreabilidade com fuso horário
- Sem campo de quantidade ou saldo — ruptura não altera estoque (RF-18, RN-05)

---

## 3. Índices

```sql
-- USUARIO
-- Busca por e-mail no login (UC-01, RF-01)
CREATE UNIQUE INDEX idx_usuario_email
    ON usuario (email);

-- PRODUTO
-- Filtrar produtos ativos na busca do vendedor (D-03)
CREATE INDEX idx_produto_ativo
    ON produto (ativo)
    WHERE ativo = TRUE;

-- SKU
-- Carregar grade de numerações de um produto (UC-02, UC-03)
CREATE INDEX idx_sku_produto_id
    ON sku (produto_id);

-- Filtrar SKUs ativos na busca (D-03)
CREATE INDEX idx_sku_ativo
    ON sku (ativo)
    WHERE ativo = TRUE;

-- Listar SKUs com saldo zerado para o lojista (UC-11, RF-21)
CREATE INDEX idx_sku_saldo_zerado
    ON sku (saldo_atual)
    WHERE saldo_atual = 0;

-- MOVIMENTACAO
-- Histórico de movimentações de um SKU
CREATE INDEX idx_movimentacao_sku_id
    ON movimentacao (sku_id);

-- RUPTURA
-- Histórico e contagem de rupturas por SKU (UC-12, RF-22)
CREATE INDEX idx_ruptura_sku_id
    ON ruptura (sku_id);
```

**Nota sobre índices parciais:** os índices `WHERE ativo = TRUE` e `WHERE saldo_atual = 0` são índices parciais do PostgreSQL. Eles indexam apenas as linhas que satisfazem a condição , menores, mais rápidos e mais eficientes para as consultas mais frequentes do sistema.

---

## 4. Transação Atômica de Saída (RN-07)

Esta é a operação mais crítica do sistema. Ela garante que dois vendedores não consigam vender o último par simultaneamente.

```sql
-- Executado pela aplicação ao registrar "Vendeu" (UC-04, UC-S2)
BEGIN;

    -- 1. Lê o saldo atual travando a linha para outras transações (FOR UPDATE)
    --    Qualquer outra transação que tente acessar este SKU vai esperar aqui.
    SELECT saldo_atual
    FROM sku
    WHERE id = $1
    FOR UPDATE;

    -- 2. A aplicação valida: se saldo_atual = 0, faz ROLLBACK e retorna erro ao vendedor
    --    O SQL abaixo só é executado se saldo_atual > 0

    -- 3. Decrementa o saldo em 1
    UPDATE sku
    SET saldo_atual = saldo_atual - 1
    WHERE id = $1;

    -- 4. Registra a movimentação de saída
    INSERT INTO movimentacao (sku_id, tipo, quantidade, usuario_id)
    VALUES ($1, 'SAIDA', 1, $2);

COMMIT;
```

**O que acontece com dois vendedores simultâneos:**

```
Vendedor A                          Vendedor B
──────────────────────────────────────────────────────
BEGIN                               BEGIN
SELECT ... FOR UPDATE (saldo = 1)   SELECT ... FOR UPDATE ← ESPERA
UPDATE saldo = 0
INSERT movimentacao
COMMIT                              ← CONTINUA AQUI
                                    SELECT retorna saldo = 0
                                    Aplicação detecta saldo = 0
                                    ROLLBACK
                                    Erro exibido ao Vendedor B
```

O `CHECK (saldo_atual >= 0)` na tabela funciona como segunda linha de defesa , mesmo que a lógica da aplicação falhe, o banco rejeita o UPDATE.

---

## 5. Consultas Principais

### Busca de produto por nome (UC-02, RF-13)

```sql
SELECT
    p.id,
    p.nome,
    p.marca,
    p.cor
FROM produto p
WHERE p.ativo = TRUE
  AND p.nome ILIKE '%' || $1 || '%'
ORDER BY p.nome
LIMIT 20;
```

---

### Grade de numerações com saldos (UC-03, RF-14, RF-15)

```sql
SELECT
    s.id,
    s.numeracao,
    s.saldo_atual,
    CASE
        WHEN s.saldo_atual = 0 THEN 'INDISPONIVEL'
        WHEN s.saldo_atual = 1 THEN 'ULTIMO_PAR'
        ELSE 'DISPONIVEL'
    END AS estado
FROM sku s
WHERE s.produto_id = $1
  AND s.ativo = TRUE
ORDER BY s.numeracao;
```

---

### SKUs com saldo zerado agrupados por modelo (UC-11, RF-21)

```sql
SELECT
    p.nome        AS produto,
    p.marca,
    p.cor,
    s.numeracao,
    s.id          AS sku_id
FROM sku s
JOIN produto p ON p.id = s.produto_id
WHERE s.saldo_atual = 0
  AND s.ativo = TRUE
  AND p.ativo = TRUE
ORDER BY p.nome, s.numeracao;
```

---

### Histórico de rupturas por SKU com contagem (UC-12, RF-22)

```sql
SELECT
    p.nome        AS produto,
    p.marca,
    s.numeracao,
    COUNT(r.id)   AS total_rupturas,
    MAX(r.criado_em) AS ultima_ruptura
FROM ruptura r
JOIN sku s      ON s.id = r.sku_id
JOIN produto p  ON p.id = s.produto_id
GROUP BY p.nome, p.marca, s.numeracao
ORDER BY total_rupturas DESC;
```

---

## 6. Script completo em ordem de execução

```sql
-- ============================================================
-- SQUAD — Script de criação do banco de dados
-- Banco: PostgreSQL 15+
-- ============================================================

-- 1. ENUMs
CREATE TYPE perfil_usuario    AS ENUM ('VENDEDOR', 'LOJISTA');
CREATE TYPE tipo_movimentacao AS ENUM ('ENTRADA', 'SAIDA', 'AJUSTE');

-- 2. Tabelas (ordem respeita dependências FK)
CREATE TABLE usuario (
    id          UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    nome        VARCHAR(150)    NOT NULL,
    email       VARCHAR(255)    NOT NULL,
    senha_hash  VARCHAR(255)    NOT NULL,
    perfil      perfil_usuario  NOT NULL,
    CONSTRAINT uq_usuario_email UNIQUE (email)
);

CREATE TABLE produto (
    id          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    nome        VARCHAR(200) NOT NULL,
    marca       VARCHAR(100) NOT NULL,
    categoria   VARCHAR(100) NOT NULL,
    cor         VARCHAR(80)  NOT NULL,
    ativo       BOOLEAN      NOT NULL DEFAULT TRUE
);

CREATE TABLE sku (
    id           UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    produto_id   UUID        NOT NULL,
    numeracao    VARCHAR(10) NOT NULL,
    saldo_atual  INTEGER     NOT NULL DEFAULT 0,
    ativo        BOOLEAN     NOT NULL DEFAULT TRUE,
    CONSTRAINT fk_sku_produto
        FOREIGN KEY (produto_id) REFERENCES produto(id),
    CONSTRAINT uq_sku_produto_numeracao
        UNIQUE (produto_id, numeracao),
    CONSTRAINT chk_sku_saldo_nao_negativo
        CHECK (saldo_atual >= 0)
);

CREATE TABLE movimentacao (
    id           UUID               PRIMARY KEY DEFAULT gen_random_uuid(),
    sku_id       UUID               NOT NULL,
    tipo         tipo_movimentacao  NOT NULL,
    quantidade   INTEGER            NOT NULL,
    usuario_id   UUID               NOT NULL,
    criado_em    TIMESTAMPTZ        NOT NULL DEFAULT NOW(),
    motivo       TEXT,
    CONSTRAINT fk_movimentacao_sku
        FOREIGN KEY (sku_id) REFERENCES sku(id),
    CONSTRAINT fk_movimentacao_usuario
        FOREIGN KEY (usuario_id) REFERENCES usuario(id),
    CONSTRAINT chk_movimentacao_quantidade_positiva
        CHECK (quantidade > 0)
);

CREATE TABLE ruptura (
    id           UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    sku_id       UUID        NOT NULL,
    usuario_id   UUID        NOT NULL,
    criado_em    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_ruptura_sku
        FOREIGN KEY (sku_id) REFERENCES sku(id),
    CONSTRAINT fk_ruptura_usuario
        FOREIGN KEY (usuario_id) REFERENCES usuario(id)
);

-- 3. Índices
CREATE UNIQUE INDEX idx_usuario_email       ON usuario      (email);
CREATE INDEX idx_produto_ativo              ON produto      (ativo)       WHERE ativo = TRUE;
CREATE INDEX idx_sku_produto_id             ON sku          (produto_id);
CREATE INDEX idx_sku_ativo                  ON sku          (ativo)       WHERE ativo = TRUE;
CREATE INDEX idx_sku_saldo_zerado           ON sku          (saldo_atual) WHERE saldo_atual = 0;
CREATE INDEX idx_movimentacao_sku_id        ON movimentacao (sku_id);
CREATE INDEX idx_ruptura_sku_id             ON ruptura      (sku_id);
```