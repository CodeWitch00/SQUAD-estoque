# User Stories - Sistema SQUAD

## 1. Visão Geral

As user stories descrevem as funcionalidades do sistema sob a perspectiva do usuário, focando no valor entregue.

**Foco**: valor para o usuário
**Nível**: produto / negócio
**Objetivo**: descrever o porquê da funcionalidade

Formato utilizado:
> Como [ator], quero [ação], para [benefício]

---

## 2. Módulo de Autenticação

### US-01

**Como** vendedor ou lojista  
**Quero** fazer login com e-mail e senha  
**Para** acessar o sistema sem fricção durante o uso  

**Prioridade:** Obrigatório

---

## 3. Módulo do Vendedor

### US-02

**Como** vendedor  
**Quero** buscar um modelo pelo nome enquanto atende o cliente  
**Para** saber a disponibilidade sem ir ao estoque às cegas  

**Prioridade:** Obrigatório

---

### US-03

**Como** vendedor  
**Quero** ver a grade de numerações com indicação visual de disponibilidade / último par/ indisponível  
**Para** informar o cliente imediatamente  

**Prioridade:** Obrigatório

---

### US-04

**Como** vendedor  
**Quero** registrar "Vendeu" com um toque  
**Para** que o saldo seja decrementado sem acessar sistema administrativo  

**Prioridade:** Obrigatório

---

### US-05

**Como** vendedor  
**Quero** registrar "Não tinha" ao não encontrar o produto  
**Para** que a ruptura vire dado estruturado  

**Prioridade:** Obrigatório

---

### US-06

**Como** vendedor  
**Quero** registrar "Desistiu" sem qualquer movimentação de estoque  
**Para** registrar o desfecho sem alterar o estoque  

**Prioridade:** Obrigatório

---

### US-07

**Como** vendedor  
**Quero** poder ignorar o registro de resultado  
**Para** não travar o fluxo de atendimento  

**Prioridade:** Obrigatório

---

## 4. Módulo do Lojista

### US-08

**Como** lojista  
**Quero** cadastrar produtos com nome, marca, categoria e cor  
**Para** montar o catálogo base antes de lançar o estoque  

**Prioridade:** Obrigatório

---

### US-09

**Como** lojista  
**Quero** cadastrar a grade de numerações  
**Para** que o sistema gere os SKUs automaticamente  

**Prioridade:** Obrigatório

---

### US-10

**Como** lojista  
**Quero** registrar entrada de estoque por numeração  
**Para** inicializar e reabastecer o saldo dos SKUS

**Prioridade:** Obrigatório

---

### US-11

**Como** lojista  
**Quero** realizar ajuste manual de saldo com justificativa  
**Para** corrigir divergências do estoque físico  

**Prioridade:** Obrigatório

---

### US-12

**Como** lojista  
**Quero** visualizar SKUs com saldo zero agrupados por modelo
**Para** identificar necessidade de reposição  

**Prioridade:** Obrigatório

---

### US-13

**Como** lojista  
**Quero** visualizar histórico de rupturas  
**Para** tomar decisões de compra baseadas em demanda real  

**Prioridade:** Importante

---

## 5. Critérios de Aceite Críticos

---

### US-02 - Consulta de estoque

- Resultado exibido em menos de 3 segundos a partir da seleção do modelo
- Busca com menos de 2 caracteres não dispara requisição ao servidor
- Modelo inexistente retorna mensagem clara, não tela de erro


---

### US-04 - Registrar venda

- Dois vendedores marcando 'Vendeu' para o mesmo SKU simultaneamente jamais resultam em saldo negativo (RN-02) 
- Saldo já em 0 rejeita a operação antes de persistir, com mensagem de erro clara
- Movimentação de saída registrada com tipo=SAIDA, usuario_id e timestamp (RN-07)

---

### US-05 - Registrar ruptura

- Ruptura criada com sku_id NOT NULL — registro sem SKU é inválido e rejeitado
- Saldo do SKU não é alterado pelo registro de ruptura
- Saldo zero não gera ruptura automática — somente declaração explícita do vendedor
  

---

### US-11 - Ajuste manual

- Apenas lojista pode executar (RN-04)  
- Motivo obrigatório  
- Ajuste registrado de forma imutável com tipo=AJUSTE, motivo, usuario_id e timestamp
