 # Casos de Uso - Sistema SQUAD

**Foco**: comportamento do sistema

**Nível**: técnico / analítico

**Objetivo**: descrever como o sistema reage a uma ação

## 1. Visão Geral

O sistema possui casos de uso distribuídos entre três atores:

- Vendedor
- Lojista
- Sistema (execuções automáticas)

---

## 2. Lista de Casos de Uso

| Código | Descrição | Ator |
|--------|----------|------|
| UC-01 | Realizar login | Vendedor / Lojista |
| UC-02 | Consultar estoque por modelo | Vendedor |
| UC-03 | Visualizar grade de numerações | Vendedor |
| UC-04 | Registrar resultado: Vendeu | Vendedor |
| UC-05 | Registrar resultado: Não tinha | Vendedor |
| UC-06 | Registrar resultado: Desistiu | Vendedor |
| UC-07 | Cadastrar produto | Lojista |
| UC-08 | Cadastrar grade de numerações | Lojista |
| UC-09 | Registrar entrada de estoque | Lojista |
| UC-10 | Realizar ajuste manual de saldo | Lojista |
| UC-11 | Visualizar saldos zerados | Lojista |
| UC-12 | Visualizar histórico de rupturas | Lojista |
| UC-S1 | Gerar SKU automaticamente | Sistema |
| UC-S2 | Decrementar saldo do SKU | Sistema |
| UC-S3 | Criar registro de ruptura | Sistema |
| UC-S4 | Rejeitar saída com saldo negativo | Sistema |
| UC-S5 | Registrar movimentação de estoque | Sistema |

---

## 3. Especificação dos Casos de Uso

---

### UC-01 - Realizar login

**Ator:** Vendedor / Lojista  
**Pré-condição:** Usuário cadastrado  

#### Fluxo Principal

1. Usuário acessa o sistema via brawser
2. Sistema exibe login
3. Usuário informa credenciais( e-mail e senha) e confirma
4. Sistema valida credenciais no banco
5. Sistema identifica o perfil: VENDEDOR ou LOGISTA
6. Sistema redireciona para tela inicial do perfil correspondente

#### Fluxos Alternativos

- Credenciais inválidas → sistema exibe erro e limpa campo de senha  
- Campo em branco → sistema bloqueia submissão antes de enviar

#### Pós-condição

Sessão autenticada ativa; usuário na tela inicial do seu perfil.
---

### UC-02 - Consultar estoque por modelo

**Ator:** Vendedor  
**Pré-condição:** Usuário autenticado  

#### Fluxo Principal

1. Vendedor acessa campo de busca na tela principal
2. Digita parte do nome do modelo
3. Sistema retorna resultados
4. Vendedor seleciona o modelo desejado
5. Sistema exibe grade completa de numerações com saldos

#### Fluxos Alternativos

- Nenhum modelo encontrado →  sistema exibe 'Produto não encontrado' 
- Menos de 2 caracteres digitados → sistema não dispara busca  

#### Pós-condição

Grade do modelo exibida com saldos por numeração visíveis.

---

### UC-03 - Visualizar grade de numerações

**Ator:** Vendedor  
**Pré-condição:** Produto selecionado  

#### Fluxo Principal

1. Sistema exibe numerações
2. Sistema exibe saldo por SKU
3. Sistema indica estado:
   - disponível
   - último par
   - indisponível

#### Pós-condição

Vendedor tem visibilidade completa do estoque

---

### UC-04 - Registrar resultado: Vendeu

**Ator:** Vendedor  
**Pré-condição:** Grade consultada; Produto localizado  

#### Fluxo Principal

1. Vendedor seleciona numeração
2. Clica em "Vendeu"
3. Sistema executa UC-S2 (decrementar saldo) em transação atômica
4. Sistema confirma e exibe saldo atualizado

#### Fluxo Alternativo

- Saldo = 0 no momento da confirmação → sistema rejeita e exibe erro (RN-02)

#### Pós-condição

Saldo decrementado em 1;  movimentação de saída registrada com data/hora e usuario_id

---

### UC-05 - Registrar resultado: Não tinha

**Ator:** Vendedor  
**Pré-condição:** Grade consultada; Produto não encontrado  

#### Fluxo Principal

1. Vendedor seleciona numeração solicitada pelo cliente
2. Clica em "Não tinha"
3. Sistema executa UC-S3 (criar registro de ruptura)
4. Sistema confirma sem alterar saldo

#### Fluxo Alternativo

- sku_id indisponível → sistema rejeita criação da ruptura(RN-06)


#### Pós-condição

Ruptura registrada com sku_id, vendedor_id e criado_em; saldo inalterado

---

### UC-06 - Registrar resultado: Desistiu

**Ator:** Vendedor 
**Pré-condição:** Grade consultada

#### Fluxo Principal

1. Vendedor clica em "Desistiu"
2. Sistema não executa nenhuma movimentação de estoque
3. Sistema retorna para tela de busca

#### Pós-condição

Nenhuma movimentação realizada; vendedor disponível para nova consulta.

---

### UC-07 - Cadastrar produto

**Ator:** Lojista  

#### Fluxo Principal

1. Lojista acessa cadastro
2. Informa dados do produto
3. Sistema salva registro

#### Pós-condição

Produto disponível para uso

---

### UC-08 - Cadastrar grade de numerações

**Ator:** Lojista  

#### Fluxo Principal

1. Lojista define numerações
2. Sistema gera SKUs automaticamente (UC-S1) 

#### Pós-condição

SKUs criados

---

### UC-09 - Registrar entrada de estoque

**Ator:** Lojista  

#### Fluxo Principal

1. Lojista informa quantidade
2. Sistema registra movimentação
3. Sistema atualiza saldo

#### Pós-condição

Estoque atualizado

---

### UC-10 - Ajuste manual de saldo

**Ator:** Lojista (exclusivo,RN-04)
**Pré-condição:** Lojista autenticado; SKU existente 

#### Fluxo Principal

1. Lojista seleciona o SKU a ajustar
2. Informa quantidade e motivo 
3. Sistema valida: saldo resultante >= 0 (RN-02)
4. Sistema aciona UC-S5 do tipo AJUSTE

#### Fluxos Alternativos

- Saldo resultante negativo → sistema rejeita (RN-02)
- Motivo não informado → sistema bloqueia submissão  

#### Pós-condição


Saldo corrigido; ajuste rastreável e imutável com motivo e responsável.
---

### UC-11 - Visualizar saldos zerados

**Ator:** Lojista  

#### Fluxo Principal

1. Sistema lista SKUs com saldo zero
2. Agrupa por modelo

#### Pós-condição

Lojista identifica rupturas potenciais

---

### UC-12 - Visualizar histórico de rupturas

**Ator:** Lojista  

#### Fluxo Principal

1. Sistema exibe rupturas
2. Mostra SKU e frequência

#### Pós-condição

Base para decisão de compra

---

## 4. Casos de Uso do Sistema (Automáticos)

---

### UC-S1 - Gerar SKU automaticamente

1. Recebe produto + numeração
2. Gera identificador único
3. Garante unicidade (RN-01)

---

### UC-S2 - Decrementar saldo do SKU
**Ator:** Sistema (disparado por UC-04)
**Pré-condição:** UC-04 acionado; SKU identificado.

1. Sistema inicia transação atômica com isolamento adequado(RN-07)
2. Sistema lê saldo atual com lock de linha
3. Sistema valida: saldo_atual > 0 (RN-02)
4. Sistema decrementa saldo em 1
5. Sistema aciona UC-S5 do tipo SAIDA
6. Sistema confirma transação

### Fluxo Principal
Saldo = 0: sistema rejeita; rollback completo; erro exibido ao vendedor

### Fluxo Pós-condição
Saldo decrementado atomicamente; race condition impossível; movimentação registrada

---

### UC-S3 — Criar registro de ruptura

**Ator:** Sistema (disparado por UC-05)

**Pré-condição:** UC-05 acionado; SKU identificado  (RN-06)


1. Recebe sku_id
2. Cria registro
3. Persiste dados

### Fluxo Principal
sku_id ausente: ruptura não é criada (RN-06)

### Pós-condição
Ruptura associada ao SKU específico; saldo inalterado; dado auditável e consultável.

---

### UC-S4 - Rejeitar saída inválida

1. Detecta saldo insuficiente
2. Cancela operação
3. Retorna erro

---

### UC-S5 - Registrar movimentação

1. Recebe tipo (entrada/saída/ajuste)
2. Registra dados
3. Persiste histórico imutável