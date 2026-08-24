# Plano de Testes — Sistema SQUAD

## Metadados do cartão

| Campo | Valor |
|---|---|
| Sprint | Sprint 1 |
| Área | QA |
| Cartão | S1-QA-006 |
| Branch | `docs/s1-qa-006-integrante` |
| Reunião-alvo | 24/08/2026 |
| Prazo formal | 09/09/2026 às 23:59 (America/Sao_Paulo) |
| Responsável | _(preencher com o nome de quem pegou o cartão)_ |
| Status | Em execução |

## Objetivo

Detalhar os casos de teste do catálogo, grade, entrada, saída, ajuste e histórico já
implementados no Sistema SQUAD, cobrindo os requisitos funcionais RF-05 a RF-11 e RF-23,
as regras de negócio RN-01 a RN-04 e os casos de uso UC-07 a UC-10, conforme especificado
na monografia do projeto (capítulos 4 e 5).

Este documento **não altera código** — é um artefato de documentação de QA, entregável do
cartão S1-QA-006.

## Escopo coberto

| Requisito/Regra | Descrição resumida | Origem na monografia |
|---|---|---|
| RF-05 | Cadastro de produto | Quadro 8 (matriz de rastreabilidade) |
| RF-06 | Cadastro de grade de numerações | Quadro 8 |
| RF-07 | Geração automática de SKU | Quadro 8 |
| RF-08 | Impedimento de SKU duplicado | Quadro 8 |
| RF-09 | Registro de quantidade inicial (entrada) | Quadro 8 |
| RF-10 | Registro de movimentação completo (histórico) | Quadro 8 |
| RF-11 | Rejeição de saída com saldo negativo | Quadro 8 |
| RF-23 | Ajuste manual com motivo obrigatório | Quadro 8 |
| RN-01 | SKU como unidade mínima — unicidade (produto_id, numeracao) | Seção 5.5.6 / 5.7.1 |
| RN-02 | Saldo nunca negativo | Seção 5.5.6 / 5.7.1 |
| RN-03 | Movimentação imutável (sem UPDATE/DELETE) | Seção 5.5.6 / 5.7.1 |
| RN-04 | Ajuste restrito ao perfil LOJISTA | Seção 5.5.6 / 5.7.1 |
| UC-07 | Cadastrar Produto | Seção 5.3 |
| UC-08 | Cadastrar Grade de Numerações (dispara UC-S1 Gerar SKU) | Seção 5.3 |
| UC-09 | Registrar Entrada de Estoque | Seção 5.3 |
| UC-10 | Ajuste Manual de Saldo | Seção 5.3 |

> Nota: a numeração de RF utilizada aqui é a da matriz de rastreabilidade (Quadro 8, seção
> 5.7.1), que é a mesma referenciada pelo cartão. O Quadro 1 (seção 4.4.1) usa uma numeração
> de RF diferente para os mesmos itens — mantivemos a do Quadro 8 por ser a que amarra
> requisito, tabela do banco e caso de uso.

## Pré-condições gerais de banco (massa de dados / seed)

Todos os casos de teste abaixo assumem a seguinte massa de dados mínima, que deve existir
(ou ser criada no setup do teste) antes da execução:

| Entidade | Registro | Observações |
|---|---|---|
| USUARIO | `vendedor@loja.com` / perfil `VENDEDOR` | Usado nos testes de permissão negada |
| USUARIO | `lojista@loja.com` / perfil `LOJISTA` | Usado nos testes de cadastro, entrada e ajuste |
| PRODUTO | `id=PROD-01`, nome "Tênis Runner", ativo=true | Produto base para os testes de grade |
| SKU | `id=SKU-01`, produto_id=PROD-01, numeracao="39", saldo_atual=5, ativo=true | SKU com saldo positivo, usado nos testes de saída |
| SKU | `id=SKU-02`, produto_id=PROD-01, numeracao="40", saldo_atual=0, ativo=true | SKU zerado, usado nos testes de saída/ruptura |

Cada caso de teste indica, quando necessário, pré-condições adicionais específicas.

---

## Casos de teste — Catálogo (Produto)

### TC-01 — Cadastrar produto com dados válidos

- **Requisito/UC:** RF-05, UC-07
- **Prioridade:** Alta
- **Pré-condição:** Usuário autenticado com perfil `LOJISTA`.
- **Dados de entrada:**
  ```json
  { "nome": "Sandália Verão", "marca": "Passo Leve", "categoria": "Sandália", "cor": "Bege" }
  ```
- **Passos:**
  1. Autenticar como `lojista@loja.com`.
  2. Enviar requisição de cadastro de produto com os dados acima.
- **Resultado esperado:** Produto criado com `id` gerado (UUID), `ativo = true` por padrão,
  status HTTP 201. Registro visível na listagem de produtos.

### TC-02 — Cadastrar produto com campo obrigatório ausente

- **Requisito/UC:** RF-05, UC-07
- **Prioridade:** Média
- **Pré-condição:** Usuário autenticado com perfil `LOJISTA`.
- **Dados de entrada:**
  ```json
  { "nome": "", "marca": "Passo Leve", "categoria": "Sandália", "cor": "Bege" }
  ```
- **Passos:**
  1. Autenticar como `lojista@loja.com`.
  2. Enviar requisição de cadastro de produto com `nome` vazio.
- **Resultado esperado:** Requisição rejeitada (HTTP 400), mensagem de erro indicando campo
  obrigatório ausente. Nenhum registro criado na tabela `produto`.

---

## Casos de teste — Grade / SKU

### TC-03 — Cadastrar grade de numeração para produto existente (gera SKU automaticamente)

- **Requisito/UC:** RF-06, RF-07, UC-08 (dispara UC-S1)
- **Prioridade:** Alta
- **Pré-condição:** Produto `PROD-01` cadastrado e ativo.
- **Dados de entrada:**
  ```json
  { "produto_id": "PROD-01", "numeracao": "41" }
  ```
- **Passos:**
  1. Autenticar como `lojista@loja.com`.
  2. Enviar requisição de cadastro de grade para `PROD-01`, numeração "41".
- **Resultado esperado:** Novo SKU criado com `id` (UUID) gerado automaticamente,
  `saldo_atual = 0`, `ativo = true`, vinculado a `PROD-01`. Status HTTP 201.

### TC-04 — Tentar cadastrar SKU duplicado (mesmo produto + numeração)

- **Requisito/UC:** RF-08, RN-01, UC-08 (via UC-S1)
- **Prioridade:** Alta
- **Pré-condição:** SKU `SKU-01` já existe para `PROD-01`, numeração "39".
- **Dados de entrada:**
  ```json
  { "produto_id": "PROD-01", "numeracao": "39" }
  ```
- **Passos:**
  1. Autenticar como `lojista@loja.com`.
  2. Enviar novamente a requisição de cadastro de grade com o mesmo `produto_id` e a mesma
     `numeracao` de um SKU já existente.
- **Resultado esperado:** Requisição rejeitada (HTTP 409 — conflito), mensagem indicando SKU
  duplicado. Constraint `UNIQUE (produto_id, numeracao)` deve impedir a inserção mesmo que a
  validação de aplicação falhe. Nenhum novo registro criado em `sku`.

### TC-05 — Cadastrar numeração para produto inexistente

- **Requisito/UC:** RF-06, UC-08
- **Prioridade:** Média
- **Pré-condição:** Nenhum produto com `id = PROD-999`.
- **Dados de entrada:**
  ```json
  { "produto_id": "PROD-999", "numeracao": "38" }
  ```
- **Passos:**
  1. Autenticar como `lojista@loja.com`.
  2. Enviar requisição de cadastro de grade referenciando `produto_id` inexistente.
- **Resultado esperado:** Requisição rejeitada (HTTP 404 ou 400), sem criação de registro.
  Chave estrangeira `sku.produto_id → produto.id` deve impedir a inserção no nível de banco
  como segunda camada de proteção.

### TC-06 — Cadastrar a mesma numeração em produtos diferentes (não deve ser bloqueado)

- **Requisito/UC:** RF-06, RF-08, RN-01, UC-08
- **Prioridade:** Média
- **Pré-condição:** Existe um segundo produto `PROD-02` (diferente de `PROD-01`), sem SKU de
  numeração "39" cadastrado.
- **Dados de entrada:**
  ```json
  { "produto_id": "PROD-02", "numeracao": "39" }
  ```
- **Passos:**
  1. Autenticar como `lojista@loja.com`.
  2. Cadastrar grade com numeração "39" para `PROD-02` (mesma numeração já usada em `PROD-01`
     via `SKU-01`).
- **Resultado esperado:** Cadastro aceito normalmente (HTTP 201). Confirma que a unicidade da
  RN-01 é sobre a combinação `(produto_id, numeracao)`, e não sobre a numeração isoladamente.

---

## Casos de teste — Entrada de estoque

### TC-07 — Registrar entrada de estoque válida

- **Requisito/UC:** RF-09, UC-09
- **Prioridade:** Alta
- **Pré-condição:** SKU `SKU-01`, saldo_atual = 5.
- **Dados de entrada:**
  ```json
  { "sku_id": "SKU-01", "tipo": "ENTRADA", "quantidade": 10 }
  ```
- **Passos:**
  1. Autenticar como `lojista@loja.com`.
  2. Registrar entrada de 10 unidades para `SKU-01`.
- **Resultado esperado:** `saldo_atual` de `SKU-01` atualizado para 15. Novo registro criado
  em `movimentacao` com `tipo = ENTRADA`, `quantidade = 10`, `sku_id = SKU-01`,
  `usuario_id` do lojista autenticado e `criado_em` preenchido.

### TC-08 — Registrar entrada com quantidade zero ou negativa

- **Requisito/UC:** RF-09, UC-09
- **Prioridade:** Média
- **Pré-condição:** SKU `SKU-01`, saldo_atual = 5.
- **Dados de entrada:**
  ```json
  { "sku_id": "SKU-01", "tipo": "ENTRADA", "quantidade": 0 }
  ```
- **Passos:**
  1. Autenticar como `lojista@loja.com`.
  2. Registrar entrada com `quantidade = 0` (repetir também com valor negativo, ex. -3).
- **Resultado esperado:** Requisição rejeitada (HTTP 400). Constraint `CHECK (quantidade > 0)`
  na tabela `movimentacao` impede a persistência mesmo se a validação de aplicação falhar.
  `saldo_atual` de `SKU-01` permanece inalterado (5).

### TC-09 — Registrar entrada para SKU inexistente

- **Requisito/UC:** RF-09, UC-09
- **Prioridade:** Baixa
- **Pré-condição:** Nenhum SKU com `id = SKU-999`.
- **Dados de entrada:**
  ```json
  { "sku_id": "SKU-999", "tipo": "ENTRADA", "quantidade": 5 }
  ```
- **Passos:**
  1. Autenticar como `lojista@loja.com`.
  2. Registrar entrada referenciando `sku_id` inexistente.
- **Resultado esperado:** Requisição rejeitada (HTTP 404), sem criação de movimentação.

---

## Casos de teste — Saída de estoque

### TC-10 — Registrar saída com saldo suficiente

- **Requisito/UC:** RF-11 (caminho de sucesso), RN-02
- **Prioridade:** Alta
- **Pré-condição:** SKU `SKU-01`, saldo_atual = 5.
- **Dados de entrada:**
  ```json
  { "sku_id": "SKU-01", "tipo": "SAIDA", "quantidade": 1 }
  ```
- **Passos:**
  1. Autenticar como `vendedor@loja.com`.
  2. Registrar o resultado de atendimento "Vendeu" para `SKU-01`.
- **Resultado esperado:** `saldo_atual` decrementado para 4. Movimentação `tipo = SAIDA`
  registrada com `quantidade = 1`, vinculada ao SKU e ao usuário vendedor.

### TC-11 — Registrar saída com saldo insuficiente

- **Requisito/UC:** RF-11, RN-02
- **Prioridade:** Alta
- **Pré-condição:** SKU `SKU-02`, saldo_atual = 0.
- **Dados de entrada:**
  ```json
  { "sku_id": "SKU-02", "tipo": "SAIDA", "quantidade": 1 }
  ```
- **Passos:**
  1. Autenticar como `vendedor@loja.com`.
  2. Tentar registrar "Vendeu" para `SKU-02`, cujo saldo é zero.
- **Resultado esperado:** Operação rejeitada antes de qualquer alteração no banco (HTTP 409 ou
  422). `saldo_atual` de `SKU-02` permanece 0. Nenhuma movimentação de saída é criada.
  Constraint `CHECK (saldo_atual >= 0)` funciona como barreira final caso a validação de
  aplicação seja contornada.

### TC-12 — Saída concorrente no último item disponível

- **Requisito/UC:** RF-11, RN-02
- **Prioridade:** Alta
- **Pré-condição:** SKU `SKU-01` com saldo_atual = 1 (ajustar massa de dados para este valor
  antes do teste).
- **Dados de entrada:** Duas requisições de saída de 1 unidade para `SKU-01`, disparadas de
  forma simultânea por dois usuários vendedores diferentes.
- **Passos:**
  1. Autenticar dois vendedores em sessões distintas.
  2. Disparar as duas requisições de saída o mais próximo possível uma da outra
     (teste de concorrência).
- **Resultado esperado:** Apenas uma das duas operações é confirmada; a segunda é rejeitada
  por saldo insuficiente. `saldo_atual` final é 0, nunca negativo. Apenas uma movimentação de
  `SAIDA` é registrada no histórico.

---

## Casos de teste — Ajuste manual de saldo

> ⚠️ **Observação de risco de modelagem (a confirmar com o time de desenvolvimento):** o
> modelo físico documentado na monografia (Quadro 4, seção 5.5.4) define
> `quantidade INTEGER NOT NULL CHECK (quantidade > 0)` para a tabela `movimentacao`, **sem
> distinção por `tipo`**. Ou seja, como especificado, toda movimentação — incluindo `AJUSTE`
> — deve ter `quantidade` positiva, e o modelo não define nenhum campo explícito para indicar
> se um ajuste aumenta ou reduz o saldo. Os casos abaixo assumem um campo `operacao`
> (`ACRESCIMO` / `REDUCAO`) como possível solução, mas **isso é uma suposição desta QA, não
> algo confirmado na documentação existente**. Antes da execução real destes testes, é
> necessário validar com quem está implementando o backend qual é o contrato real da API para
> ajustes que reduzem saldo — este ponto deve ser levantado como observação no PR deste
> cartão.

### TC-13 — Ajuste manual pelo lojista com motivo informado (redução de saldo)

- **Requisito/UC:** RF-23, RN-04, UC-10
- **Prioridade:** Alta
- **Pré-condição:** SKU `SKU-01`, saldo_atual = 5. Usuário autenticado com perfil `LOJISTA`.
- **Dados de entrada:**
  ```json
  { "sku_id": "SKU-01", "tipo": "AJUSTE", "operacao": "REDUCAO", "quantidade": 2, "motivo": "Divergência encontrada na conferência física" }
  ```
  *(`quantidade` sempre positiva — representa a magnitude do ajuste, conforme
  `CHECK (quantidade > 0)` do modelo físico. `operacao` é o campo assumido para indicar a
  direção; ver observação de risco acima.)*
- **Passos:**
  1. Autenticar como `lojista@loja.com`.
  2. Registrar ajuste de redução de 2 unidades em `SKU-01`, com motivo preenchido.
- **Resultado esperado:** `saldo_atual` atualizado para 3 (5 − 2). Movimentação `tipo = AJUSTE`
  registrada com `quantidade = 2` (positiva), `motivo` preenchido, `usuario_id` do lojista e
  `criado_em`.

### TC-14 — Ajuste manual sem motivo informado

- **Requisito/UC:** RF-23, UC-10
- **Prioridade:** Alta
- **Pré-condição:** SKU `SKU-01`, saldo_atual = 5. Usuário autenticado com perfil `LOJISTA`.
- **Dados de entrada:**
  ```json
  { "sku_id": "SKU-01", "tipo": "AJUSTE", "operacao": "REDUCAO", "quantidade": 2, "motivo": "" }
  ```
- **Passos:**
  1. Autenticar como `lojista@loja.com`.
  2. Tentar registrar ajuste sem preencher `motivo` (campo vazio ou ausente).
- **Resultado esperado:** Requisição rejeitada (HTTP 400), mensagem indicando que o motivo é
  obrigatório para ajustes. `saldo_atual` de `SKU-01` permanece inalterado (5). Nenhuma
  movimentação criada.

### TC-15 — Ajuste manual resultando em saldo negativo

- **Requisito/UC:** RF-23, RN-02, UC-10
- **Prioridade:** Alta
- **Pré-condição:** SKU `SKU-01`, saldo_atual = 5. Usuário autenticado com perfil `LOJISTA`.
- **Dados de entrada:**
  ```json
  { "sku_id": "SKU-01", "tipo": "AJUSTE", "operacao": "REDUCAO", "quantidade": 10, "motivo": "Teste de limite de saldo" }
  ```
- **Passos:**
  1. Autenticar como `lojista@loja.com`.
  2. Tentar registrar ajuste de redução de 10 unidades, que levaria o saldo a -5.
- **Resultado esperado:** Requisição rejeitada (HTTP 409 ou 422). `saldo_atual` permanece
  inalterado (5). Confirma que a RN-02 (saldo nunca negativo) também é aplicada em ajustes
  manuais, não apenas em saídas de venda.

### TC-16 — Ajuste manual tentado por usuário com perfil VENDEDOR (permissão incorreta)

- **Requisito/UC:** RN-04, UC-10
- **Prioridade:** Alta
- **Pré-condição:** SKU `SKU-01`, saldo_atual = 5. Usuário autenticado com perfil `VENDEDOR`.
- **Dados de entrada:**
  ```json
  { "sku_id": "SKU-01", "tipo": "AJUSTE", "operacao": "ACRESCIMO", "quantidade": 3, "motivo": "Tentativa indevida" }
  ```
- **Passos:**
  1. Autenticar como `vendedor@loja.com`.
  2. Tentar registrar um ajuste manual de saldo.
- **Resultado esperado:** Requisição rejeitada (HTTP 403 — acesso negado). `saldo_atual`
  permanece inalterado. Nenhuma movimentação criada. Confirma que a restrição de perfil
  (RN-04) é validada no backend, e não apenas ocultada na interface.

### TC-17 — Ajuste manual sem autenticação válida

- **Requisito/UC:** RN-04, UC-10
- **Prioridade:** Média
- **Pré-condição:** Nenhum token de autenticação enviado, ou token expirado/inválido.
- **Dados de entrada:**
  ```json
  { "sku_id": "SKU-01", "tipo": "AJUSTE", "operacao": "ACRESCIMO", "quantidade": 3, "motivo": "Sem login" }
  ```
- **Passos:**
  1. Enviar requisição de ajuste manual sem cabeçalho de autenticação (ou com token inválido).
- **Resultado esperado:** Requisição rejeitada (HTTP 401 — não autenticado), antes mesmo da
  validação de perfil. Nenhuma movimentação criada.

---

## Casos de teste — Histórico e imutabilidade

### TC-18 — Consultar histórico de movimentações de um SKU

- **Requisito/UC:** RF-10
- **Prioridade:** Alta
- **Pré-condição:** `SKU-01` possui ao menos uma movimentação de cada tipo (ENTRADA, SAIDA,
  AJUSTE), previamente registradas.
- **Passos:**
  1. Autenticar como `lojista@loja.com`.
  2. Consultar o histórico de movimentações de `SKU-01`.
- **Resultado esperado:** Lista retornada em ordem cronológica (por `criado_em`), contendo
  `tipo`, `quantidade`, `usuario_id` e `motivo` (quando aplicável) de cada movimentação.
  Status HTTP 200.

### TC-19 — Tentar alterar (UPDATE) um registro de movimentação existente

- **Requisito/UC:** RN-03
- **Prioridade:** Alta
- **Pré-condição:** Movimentação existente em `SKU-01` (ex.: a criada no TC-07).
- **Passos:**
  1. Verificar se existe endpoint de atualização de movimentação na API (esperado: não
     existir).
  2. Caso exista qualquer via de acesso (ex.: acesso direto ao banco em ambiente de teste),
     tentar executar um `UPDATE` sobre o registro de movimentação.
- **Resultado esperado:** Não deve existir rota HTTP para atualizar movimentações
  (comportamento por ausência de endpoint). Caso testado diretamente no banco, nenhuma
  constraint aplicativa impede o UPDATE nesse nível — a garantia de imutabilidade é
  arquitetural (ausência de caminho de escrita), o que deve ser registrado como observação
  de risco caso o acesso direto ao banco não seja bloqueado em produção.

### TC-20 — Tentar excluir (DELETE) um registro de movimentação existente

- **Requisito/UC:** RN-03
- **Prioridade:** Alta
- **Pré-condição:** Movimentação existente em `SKU-01`.
- **Passos:**
  1. Verificar se existe endpoint de exclusão de movimentação na API (esperado: não existir).
- **Resultado esperado:** Não deve existir rota HTTP para excluir movimentações. O histórico
  deve permanecer íntegro e completo em qualquer consulta subsequente (TC-18).

---

## Rastreabilidade — Requisito/Regra × Caso de teste

| Requisito/Regra | Casos de teste |
|---|---|
| RF-05 | TC-01, TC-02 |
| RF-06 | TC-03, TC-05, TC-06 |
| RF-07 | TC-03 |
| RF-08 | TC-04, TC-06 |
| RF-09 | TC-07, TC-08, TC-09 |
| RF-10 | TC-18 |
| RF-11 | TC-10, TC-11, TC-12 |
| RF-23 | TC-13, TC-14, TC-15 |
| RN-01 | TC-04, TC-06 |
| RN-02 | TC-10, TC-11, TC-12, TC-15 |
| RN-03 | TC-19, TC-20 |
| RN-04 | TC-16, TC-17 |
| UC-07 | TC-01, TC-02 |
| UC-08 | TC-03, TC-04, TC-05, TC-06 |
| UC-09 | TC-07, TC-08, TC-09 |
| UC-10 | TC-13, TC-14, TC-15, TC-16, TC-17 |

Cobertura confirmada: todos os requisitos e regras do escopo do cartão (RF-05 a RF-11,
RF-23, RN-01 a RN-04, UC-07 a UC-10) possuem ao menos um caso de teste associado. Os quatro
cenários obrigatórios do cartão estão cobertos: SKU duplicado (TC-04), saldo insuficiente
(TC-11), ajuste sem motivo (TC-14) e permissão incorreta (TC-16).

## Revisão

| Campo | Valor |
|---|---|
| Revisor | _(preencher — outro integrante do squad)_ |
| Data da revisão | _(preencher)_ |
| Status | Pendente de revisão |
| Observações | _(preencher após revisão)_ |

### Pendência para a equipe de modelagem/backend

O modelo físico documentado (Quadro 4, seção 5.5.4) não especifica como a direção de um
ajuste manual (aumento vs. redução de saldo) é representada, já que `quantidade` deve ser
sempre positiva (`CHECK (quantidade > 0)`) para todos os tipos de movimentação. Os casos
TC-13 a TC-17 assumem um campo `operacao` (`ACRESCIMO`/`REDUCAO`) como possível solução, mas
isso precisa ser confirmado com quem estiver implementando o endpoint de ajuste antes da
execução real destes testes.
