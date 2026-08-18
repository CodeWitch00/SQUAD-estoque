# Modelo Físico 

> **Para quem é este documento:** desenvolvedores  
> **Banco de dados:** SQLite 3.x (via Entity Framework Core)  
> **Plataforma:** ASP.NET Core MVC (.NET 10)  

---

## 1. Mapeamento de Tipos e Representação no SQLite

O SQLite possui um sistema de tipagem dinâmica baseado em classes de armazenamento (*storage classes*): `NULL`, `INTEGER`, `REAL`, `TEXT` e `BLOB`. 

A persistência do sistema SQUAD no SQLite através do Entity Framework Core adota os seguintes padrões:

| Conceito Lógico | Tipo SQLite | Representação C# / EF Core | Justificativa |
| :--- | :--- | :--- | :--- |
| **Identificadores (UUID/Guid)** | `TEXT` | `System.Guid` | Formato padrão ISO/RFC 4122 (string de 36 caracteres: `XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX`). |
| **Textos descritivos** (`nome`, `marca`, `categoria`, `cor`, `numeracao`, `email`, `senha_hash`, `motivo`) | `TEXT` | `System.String` | SQLite aloca textos com tamanho variável. Limites máximos são validados na aplicação e por constraints. |
| **Datas e Horários** (`criado_em`) | `TEXT` | `System.DateTime` (UTC) | Padrão ISO 8601 em UTC (`YYYY-MM-DDTHH:MM:SS.FFFFFFFZ`), compatível com ordenação cronológica textual. |
| **Valores Booleanos** (`ativo`) | `INTEGER` | `System.Boolean` | `1` para `TRUE`, `0` para `FALSE`. |
| **Contadores e Saldos** (`saldo_atual`, `quantidade`) | `INTEGER` | `System.Int32` | Valores inteiros com suporte a constraints de validação `CHECK`. |
| **Tipos Enumerados** (`perfil_usuario`, `tipo_movimentacao`) | `INTEGER` ou `TEXT` | `enum` C# | Mapeamento nativo de Enums via EF Core (armazenamento inteiro simples ou string para legibilidade). |

---

## 2. Tabelas

A ordem de criação respeita as dependências de chaves estrangeiras:
`usuario` → `produto` → `sku` → `movimentacao` e `ruptura`.

---

### 2.1 Tabela `usuario`

Armazena os usuários autenticados do sistema e seus perfis de acesso (RF-01 a RF-04, RN-04).

```sql
CREATE TABLE usuario (
    id          TEXT    PRIMARY KEY NOT NULL,
    nome        TEXT    NOT NULL,
    email       TEXT    NOT NULL,
    senha_hash  TEXT    NOT NULL,
    perfil      INTEGER NOT NULL,

    CONSTRAINT uq_usuario_email UNIQUE (email),
    CONSTRAINT chk_usuario_perfil CHECK (perfil IN (0, 1)) -- 0: VENDEDOR, 1: LOJISTA
);
```

**Decisões de Modelagem Física:**
- `id TEXT`: GUID gerado pela aplicação na criação da entidade.
- `email TEXT UNIQUE`: Garante unicidade de cadastro para login (RF-01).
- `senha_hash TEXT`: Armazena o hash seguro gerado via bcrypt com custo $\ge 12$ (RF-04, RNF-04).
- `perfil INTEGER`: Representa o perfil de acesso (`0 = VENDEDOR`, `1 = LOJISTA`) validado por constraint `CHECK`.

---

### 2.2 Tabela `produto`

Representa o modelo comercial de calçado no catálogo (RF-05). Não controla saldo diretamente.

```sql
CREATE TABLE produto (
    id          TEXT    PRIMARY KEY NOT NULL,
    nome        TEXT    NOT NULL,
    marca       TEXT    NOT NULL,
    categoria   TEXT    NOT NULL,
    cor         TEXT    NOT NULL,
    ativo       INTEGER NOT NULL DEFAULT 1,

    CONSTRAINT chk_produto_ativo CHECK (ativo IN (0, 1))
);
```

**Decisões de Modelagem Física:**
- `id TEXT`: GUID identificador do modelo.
- `ativo INTEGER DEFAULT 1`: Flag de exclusão/desativação lógica. Quando `0`, o produto não é retornado na busca do vendedor, mas preserva todo o histórico referencial.

---

### 2.3 Tabela `sku`

Representa a menor unidade física de controle de estoque: a combinação de um Produto com uma Numeração específica (RF-06 a RF-08, RN-01, RN-02).

```sql
CREATE TABLE sku (
    id           TEXT    PRIMARY KEY NOT NULL,
    produto_id   TEXT    NOT NULL,
    numeracao    TEXT    NOT NULL,
    saldo_atual  INTEGER NOT NULL DEFAULT 0,
    ativo        INTEGER NOT NULL DEFAULT 1,

    CONSTRAINT fk_sku_produto
        FOREIGN KEY (produto_id) REFERENCES produto(id)
        ON DELETE RESTRICT,

    CONSTRAINT uq_sku_produto_numeracao
        UNIQUE (produto_id, numeracao),

    CONSTRAINT chk_sku_saldo_nao_negativo
        CHECK (saldo_atual >= 0),

    CONSTRAINT chk_sku_ativo
        CHECK (ativo IN (0, 1))
);
```

**Decisões de Modelagem Física:**
- `id TEXT`: GUID gerado automaticamente pelo sistema (RF-07).
- `CONSTRAINT uq_sku_produto_numeracao UNIQUE (produto_id, numeracao)`: Implementa a **RN-01** (unicidade de SKU no banco).
- `CONSTRAINT chk_sku_saldo_nao_negativo CHECK (saldo_atual >= 0)`: Segunda linha de defesa para a **RN-02** (saldo nunca negativo).
- `ON DELETE RESTRICT`: Impede a remoção acidental de um produto que contenha SKUs vinculados.

---

### 2.4 Tabela `movimentacao`

Registra o histórico imutável de todas as alterações físicas de saldo de estoque: entradas, saídas (vendas) e ajustes manuais (RF-09, RF-10, RF-17, RF-23, RN-03, RN-04).

```sql
CREATE TABLE movimentacao (
    id           TEXT    PRIMARY KEY NOT NULL,
    sku_id       TEXT    NOT NULL,
    tipo         INTEGER NOT NULL,
    quantidade   INTEGER NOT NULL,
    usuario_id   TEXT    NOT NULL,
    criado_em    TEXT    NOT NULL,
    motivo       TEXT    NULL,

    CONSTRAINT fk_movimentacao_sku
        FOREIGN KEY (sku_id) REFERENCES sku(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_movimentacao_usuario
        FOREIGN KEY (usuario_id) REFERENCES usuario(id)
        ON DELETE RESTRICT,

    CONSTRAINT chk_movimentacao_tipo
        CHECK (tipo IN (0, 1, 2)), -- 0: ENTRADA, 1: SAIDA, 2: AJUSTE

    CONSTRAINT chk_movimentacao_quantidade_positiva
        CHECK (quantidade > 0)
);
```

**Decisões de Modelagem Física:**
- **Imutabilidade (RN-03)**: Esta tabela não possui colunas de atualização ou exclusão. Nenhum comando `UPDATE` ou `DELETE` deve ser executado nesta tabela.
- `motivo TEXT NULL`: Justificativa da operação. Obrigatória por validação de negócio quando `tipo = 2 (AJUSTE)` (RF-23).
- `criado_em TEXT`: Timestamp ISO 8601 gerado em UTC no momento da persistência.

---

### 2.5 Tabela `ruptura`

Registra a demanda não atendida quando o vendedor informa explicitamente "Não tinha" durante o atendimento (RF-18, RF-22, RN-05, RN-06).

```sql
CREATE TABLE ruptura (
    id           TEXT PRIMARY KEY NOT NULL,
    sku_id       TEXT NOT NULL,
    usuario_id   TEXT NOT NULL,
    criado_em    TEXT NOT NULL,

    CONSTRAINT fk_ruptura_sku
        FOREIGN KEY (sku_id) REFERENCES sku(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_ruptura_usuario
        FOREIGN KEY (usuario_id) REFERENCES usuario(id)
        ON DELETE RESTRICT
);
```

**Decisões de Modelagem Física:**
- `sku_id NOT NULL`: Cumpre a **RN-06** (toda ruptura é obrigatoriamente vinculada a um SKU específico).
- Ausência de campo de saldo/quantidade: O registro de ruptura **não altera** o estoque do SKU (RN-05, RF-18).

---

## 3. Índices de Performance

Os índices são criados para otimizar as consultas críticas de atendimento em chão de loja e painéis do lojista:

```sql
-- Otimização da busca de usuário no login (RF-01, UC-01)
CREATE UNIQUE INDEX idx_usuario_email
    ON usuario (email);

-- Otimização do filtro de busca de catálogo por produtos ativos (RF-13, UC-02)
CREATE INDEX idx_produto_ativo
    ON produto (ativo);

-- Otimização do carregamento da grade de SKUs por modelo (RF-14, UC-03)
CREATE INDEX idx_sku_produto_id
    ON sku (produto_id);

-- Otimização de filtragem de SKUs ativos
CREATE INDEX idx_sku_ativo
    ON sku (ativo);

-- Otimização do painel de produtos/SKUs com estoque zerado do lojista (RF-21, UC-11)
CREATE INDEX idx_sku_saldo_zerado
    ON sku (saldo_atual)
    WHERE saldo_atual = 0;

-- Otimização do histórico de movimentações por SKU
CREATE INDEX idx_movimentacao_sku_id
    ON movimentacao (sku_id);

-- Otimização da contagem e agrupamento de rupturas por SKU (RF-22, UC-12)
CREATE INDEX idx_ruptura_sku_id
    ON ruptura (sku_id);
```

---

## 4. Transação Atômica de Saída de Estoque (RN-07)

A operação de venda (registro de "Vendeu" — UC-04, UC-S2) é a operação concorrencial crítica do sistema. O SQLite assegura atomicidade através de transações de escrita e do `CHECK (saldo_atual >= 0)`.

### 4.1 Fluxo SQL Transacional no SQLite

```sql
-- Inicia a transação com bloqueio imediato para escrita no SQLite
BEGIN IMMEDIATE;

    -- 1. Decrementa o saldo do SKU somente se houver saldo disponível (> 0)
    UPDATE sku
    SET saldo_atual = saldo_atual - 1
    WHERE id = @sku_id AND saldo_atual > 0;

    -- 2. Se nenhuma linha foi afetada (saldo era 0), a aplicação executa ROLLBACK e retorna erro (RN-02)
    --    Se 1 linha foi atualizada, prossegue com o registro da movimentação:

    -- 3. Insere a movimentação de saída
    INSERT INTO movimentacao (id, sku_id, tipo, quantidade, usuario_id, criado_em, motivo)
    VALUES (@movimentacao_id, @sku_id, 1, 1, @usuario_id, @timestamp_utc, NULL);

COMMIT;
```

---

## 5. Consultas Principais do Sistema

### 5.1 Busca de Produto por Nome (Vendedor: RF-13, UC-02)
```sql
SELECT id, nome, marca, categoria, cor
FROM produto
WHERE ativo = 1
  AND UPPER(nome) LIKE '%' || UPPER(@termo_busca) || '%'
ORDER BY nome
LIMIT 20;
```

### 5.2 Consulta de Grade de Numerações e Saldos (Vendedor: RF-14, RF-15, UC-03)
```sql
SELECT 
    id,
    numeracao,
    saldo_atual,
    CASE 
        WHEN saldo_atual = 0 THEN 'INDISPONIVEL'
        WHEN saldo_atual = 1 THEN 'ULTIMO_PAR'
        ELSE 'DISPONIVEL'
    END AS estado_derivado
FROM sku
WHERE produto_id = @produto_id
  AND ativo = 1
ORDER BY numeracao;
```

### 5.3 SKUs com Saldo Zerado Agrupados por Modelo (Lojista: RF-21, UC-11)
```sql
SELECT 
    p.nome AS produto_nome,
    p.marca,
    p.cor,
    s.id AS sku_id,
    s.numeracao
FROM sku s
JOIN produto p ON p.id = s.produto_id
WHERE s.saldo_atual = 0
  AND s.ativo = 1
  AND p.ativo = 1
ORDER BY p.nome, s.numeracao;
```

### 5.4 Histórico e Ranking de Rupturas por SKU (Lojista: RF-22, UC-12)
```sql
SELECT 
    p.nome AS produto_nome,
    p.marca,
    s.numeracao,
    COUNT(r.id) AS total_rupturas,
    MAX(r.criado_em) AS ultima_ruptura
FROM ruptura r
JOIN sku s ON s.id = r.sku_id
JOIN produto p ON p.id = s.produto_id
GROUP BY p.nome, p.marca, s.numeracao
ORDER BY total_rupturas DESC;
```

---

## 6. Script Completo DDL em Ordem de Execução

```sql
-- ============================================================
-- SQUAD — Script DDL do Banco de Dados SQLite
-- ============================================================

-- Ativação do suporte a chaves estrangeiras no SQLite
PRAGMA foreign_keys = ON;

-- 1. Tabela USUARIO
CREATE TABLE IF NOT EXISTS usuario (
    id          TEXT    PRIMARY KEY NOT NULL,
    nome        TEXT    NOT NULL,
    email       TEXT    NOT NULL,
    senha_hash  TEXT    NOT NULL,
    perfil      INTEGER NOT NULL,
    CONSTRAINT uq_usuario_email UNIQUE (email),
    CONSTRAINT chk_usuario_perfil CHECK (perfil IN (0, 1))
);

-- 2. Tabela PRODUTO
CREATE TABLE IF NOT EXISTS produto (
    id          TEXT    PRIMARY KEY NOT NULL,
    nome        TEXT    NOT NULL,
    marca       TEXT    NOT NULL,
    categoria   TEXT    NOT NULL,
    cor         TEXT    NOT NULL,
    ativo       INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT chk_produto_ativo CHECK (ativo IN (0, 1))
);

-- 3. Tabela SKU
CREATE TABLE IF NOT EXISTS sku (
    id           TEXT    PRIMARY KEY NOT NULL,
    produto_id   TEXT    NOT NULL,
    numeracao    TEXT    NOT NULL,
    saldo_atual  INTEGER NOT NULL DEFAULT 0,
    ativo        INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT fk_sku_produto FOREIGN KEY (produto_id) REFERENCES produto(id) ON DELETE RESTRICT,
    CONSTRAINT uq_sku_produto_numeracao UNIQUE (produto_id, numeracao),
    CONSTRAINT chk_sku_saldo_nao_negativo CHECK (saldo_atual >= 0),
    CONSTRAINT chk_sku_ativo CHECK (ativo IN (0, 1))
);

-- 4. Tabela MOVIMENTACAO
CREATE TABLE IF NOT EXISTS movimentacao (
    id           TEXT    PRIMARY KEY NOT NULL,
    sku_id       TEXT    NOT NULL,
    tipo         INTEGER NOT NULL,
    quantidade   INTEGER NOT NULL,
    usuario_id   TEXT    NOT NULL,
    criado_em    TEXT    NOT NULL,
    motivo       TEXT    NULL,
    CONSTRAINT fk_movimentacao_sku FOREIGN KEY (sku_id) REFERENCES sku(id) ON DELETE RESTRICT,
    CONSTRAINT fk_movimentacao_usuario FOREIGN KEY (usuario_id) REFERENCES usuario(id) ON DELETE RESTRICT,
    CONSTRAINT chk_movimentacao_tipo CHECK (tipo IN (0, 1, 2)),
    CONSTRAINT chk_movimentacao_quantidade_positiva CHECK (quantidade > 0)
);

-- 5. Tabela RUPTURA
CREATE TABLE IF NOT EXISTS ruptura (
    id           TEXT PRIMARY KEY NOT NULL,
    sku_id       TEXT NOT NULL,
    usuario_id   TEXT NOT NULL,
    criado_em    TEXT NOT NULL,
    CONSTRAINT fk_ruptura_sku FOREIGN KEY (sku_id) REFERENCES sku(id) ON DELETE RESTRICT,
    CONSTRAINT fk_ruptura_usuario FOREIGN KEY (usuario_id) REFERENCES usuario(id) ON DELETE RESTRICT
);

-- 6. Índices
CREATE UNIQUE INDEX IF NOT EXISTS idx_usuario_email       ON usuario (email);
CREATE INDEX IF NOT EXISTS idx_produto_ativo              ON produto (ativo);
CREATE INDEX IF NOT EXISTS idx_sku_produto_id             ON sku (produto_id);
CREATE INDEX IF NOT EXISTS idx_sku_ativo                  ON sku (ativo);
CREATE INDEX IF NOT EXISTS idx_sku_saldo_zerado           ON sku (saldo_atual) WHERE saldo_atual = 0;
CREATE INDEX IF NOT EXISTS idx_movimentacao_sku_id        ON movimentacao (sku_id);
CREATE INDEX IF NOT EXISTS idx_ruptura_sku_id             ON ruptura (sku_id);
```