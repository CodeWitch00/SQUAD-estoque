# SQUAD
Sistema de Controle de Estoque para Varejo de Calçados 

## Documento de Domínio e Contexto de Negócio

## 1. Contexto e Problema

### 1.1 Cenário Atual

O varejo de calçados de pequeno e médio porte opera, em sua maioria, sem ferramentas de controle de estoque adequadas para o chão de loja. O vendedor conduz o atendimento sem acesso imediato à informação de disponibilidade de produto, o que gera um gargalo operacional recorrente e invisível para a gestão.

> Cena típica: o cliente pede um modelo no número 37. O vendedor vai ao estoque, leva de 3 a 7 minutos, volta de mãos vazias. O cliente já foi embora. A venda se perdeu, e nenhum registro disso existe.

O lojista, ao final do mês, tem visibilidade apenas do que foi vendido. Nunca soube o que poderia ter vendido. As decisões de recompra são baseadas no pedido anterior ou na sugestão do representante, sem base em dados de demanda real.


### 1.2 Situação-Problema

Três causas estruturais sustentam o problema:

- **Causa 1:** O vendedor não tem consulta rápida de estoque durante o atendimento  
- **Causa 2:** A ruptura não deixa rastro. O “não tinha o 37” morre na conversa verbal  
- **Causa 3:** A decisão de compra é desconectada da demanda real por numeração  

---
## 2. Público-Alvo

### 2.1 Personas

#### Vendedor - Usuário Primário
> Opera com celular na mão, cliente esperando, pressão de tempo. Não usará nada que exija mais de 2 toques. Precisa de resposta imediata: tem ou não tem.

- Contexto: chão de loja, atendimento ativo  
- Dispositivo: smartphone via browser  
- Tolerância à complexidade: nenhuma  
- Necessidade crítica: velocidade e clareza visual  


#### Lojista - Usuário de Gestão
> Toma decisões de compra. Não tem tempo para dashboard complexo. Precisa de informação clara sobre o que está faltando e o que está parado.

- Contexto: gestão da loja, decisão de compra  
- Dispositivo: desktop ou tablet  
- Tolerância à complexidade: baixa  
- Necessidade crítica: visão clara de ruptura e estoque 
## 3. Objetivos

### 3.1 Objetivo Geral

Criar uma ferramenta simples e confiável que resolva dois problemas específicos:
- o vendedor saber o que tem em estoque antes de ir buscar
- o lojista enxergar o que está perdendo por ruptura

### 3.2 Metas Mensuráveis

| Meta | Critério de Sucesso |
|------|--------------------|
| Consulta de estoque em menos de 3 segundos | Tempo de resposta da API < 500ms em P95 |
| Registro de ruptura em até 2 interações (seleção de SKU + confirmação) | Vendedor registra sem sair da tela |
| Visualização de rupturas | Dashboard com frequência por SKU |
| Usabilidade | Primeira consulta em até 2 toques e sistema operando via browser mobile|

  >Indicador principal: a informação 'não tinha o 37' passa a existir como dado estruturado no sistema, associada ao SKU específico, auditável e consultável.
---
## 4. Escopo do MVP

### 4.1 O que entra no Escopo

- Autenticação com dois perfis (VENDEDOR / LOJISTA)
- Cadastro de produtos 
- Cadastro de grade
- Controle de estoque por SKU (produto + numeração)
- Consulta de estoque 
- Registro : Vendeu / Não tinha / Desistiu
- Decremento automático de saldo ao marcar *Vendeu*
- Registro de ruptura ao marcar *Não tinha*
- Ajuste manual de saldo
- Visualização de saldos zerados por modelo
- Histórico simples de rupturas por numeração
- O sistema SQUAD opera em paralelo ao PDV existente, porém deve ser tratado como fonte operacional de consulta para o vendedor. Divergências podem ocorrer e devem ser corrigidas através de ajustes manuais registrados no sistema.

### 4.2 Fora do Escopo

- Importação de XML de NF-e  
- PDV e caixa  
- Gestão financeira  
- Multi-loja
- Zona física de estoque
- Sugestão de grade para compra
- Integração com ERP  
- Alertas automáticos de ruptura 
- Relatórios analíticos avançados
- Emissão de nota fiscal  

---
## 5. Regras de Negócio

As regras abaixo definem o comportamento lógico do sistema e devem ser respeitadas rigorosamente durante a implementação.

| ID | Definição | Descrição Detalhada |
| :--- | :--- | :--- |
| **RN-01** |  SKU é a unidade mínima | O SKU é a combinação única de Produto + Numeração. <br> Não podem existir dois SKUs iguais no sistema. |
| **RN-02** |  Saldo nunca negativo | O sistema deve rejeitar qualquer operação de saída (venda) ou O ajuste manual de estoque deve ser sempre do tipo DELTA (adição ou subtração de quantidade), nunca substituição direta do saldo. |
| **RN-03** |  Imutabilidade | Toda movimentação de estoque é permanente. <br> Erros se corrigem com nova movimentação de sentido contrário, nunca editando a original. |
| **RN-04** |  Ajuste manual restrito | Somente o perfil **LOJISTA** pode realizar ajuste manual de saldo.<br> O perfil **VENDEDOR** não tem acesso. |
| **RN-05** |  Ruptura explícita | Ruptura é registrada apenas quando o vendedor informa 'Não tinha'. <br> O sistema não gera ruptura automática por saldo zero. |
| **RN-06** |  Ruptura por SKU | O registro de ruptura deve estar associado a um SKU (produto + número). <br> Não existe ruptura de modelo sem numeração. |
| **RN-07** |  Atomicidade | Saídas devem ser atômicas (leitura + validação + atualização) em uma única transação para evitar erro de concorrência. |

---
> [!IMPORTANTE]
> A conformidade com a **RN-07** é vital para o Passo 9 (Testes) e Passo 10 (CI/CD) do nosso planejamento.

### GLOSSÁRIO

- Produto:
Item do catálogo da loja que representa um modelo de calçado.

- SKU:
Combinação única de Produto + Numeração. Unidade mínima de estoque.

- Grade:
Conjunto de SKUs de um mesmo produto, organizados por numeração.

- Numeração:
Tamanho do calçado (ex: 37, 38, 39).

- Saldo:
Quantidade disponível de um SKU no sistema.

- Ruptura:
Registro explícito de demanda não atendida para um SKU específico.

- Último par:
Estado visual quando saldo do SKU é igual a 1.
---
## 6. Fluxos Principais

### Fluxo do Vendedor
> Vendedor abre o sistema → busca o modelo pelo nome → visualiza grade (saldos por numeração) → vai buscar o produto → registra resultado: Vendeu / Não tinha / Desistiu

- Vendeu: saldo do SKU é decrementado em 1 automaticamente,como estado relevante do SKU.
- Não tinha: o vendedor deve selecionar explicitamente a numeração (SKU) e o sistema registra a ruptura com o sku_id correspondente.



---

### Fluxo do Lojista
> Lojista acessa painel → cadastra produto + grade → registra entrada de estoque → consulta rupturas e saldos zerados → toma decisão de recompra.

- Registro de entrada deve permitir informar múltiplos SKUs em uma única operação, executada de forma transacional.
---
