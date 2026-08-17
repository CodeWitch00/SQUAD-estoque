# Software Requirements Specification (SRS)

## 1. Introdução

### 1.1 Objetivo

Este documento especifica os requisitos de software do sistema SQUAD, uma ferramenta de controle de estoque voltada para o varejo de calçados de pequeno e médio porte.

Ele define com precisão:
- o que o sistema deve fazer
- como deve se comportar
- quais restrições deve respeitar

Serve como contrato entre negócio e desenvolvimento.


### 1.2 Visão Geral do Sistema

O sistema resolve dois problemas principais:

1. O vendedor não sabe se o produto está disponível antes de ir ao estoque  
2. A informação de ruptura não se torna dado estruturado  

O sistema:
- é uma ferramenta de consulta e rastreamento de ruptura
- opera via browser, sem instalação
- não requer instalação
- não substitui ERP
- não gerencia financeiro
- não emite nota fiscal

> Premissa arquitetural: o controle de estoque do SQUAD é paralelo ao PDV existente, não substituto. O lojista precisa entender que os dois sistemas coexistem.


---

## 2. Stakeholders

| Papel        | Interesse Principal                              | Envolvimento |
|--------------|-------------------------------------------------|--------------|
| Lojista (dono) | Decisão de recompra baseada em dados , visibilidade de ruptura. | Alto - define requisitos e valida|
| Vendedor     | Consulta rápida durante atendimento             | Alto - usuário primário do sistema |
| Desenvolvedor| Implementação e manutenção do sistema           | Alto - executa e mantém |

---

## 3. Perfis de Usuário

| Atributo | 3.1 Vendedor | 3.2 Lojista |
| :--- | :--- | :--- |
| **Contexto** |Chão de loja com cliente esperando. Alta pressão de tempo.| Gestão da loja. Toma decisões de compra e reposição.|
| **Dispositivo** | Smartphone (via browser) | Desktop ou Tablet. Interface mais completa.|
| **Nível Técnico** | Baixo. Não usa sistemas complexos durante o atendimento.| Baixo a médio. Usa sistemas simples no dia a dia.|
| **Tolerância à Fricção** | Nenhum. 2 toques é o máximo aceitável.| Baixa. Quer visão clara e objetiva.|
| **Objetivo** | Saber se tem o produto sem ir ao estoque. | Saber o que está faltando e o que está parado.|

---

## 4. Requisitos Funcionais
> Prioridades:
> 🔴 Prioridade Alta (Obrigatório)
> 🟡 Prioridade Média (Importante)
> 🟢 Prioridade Baixa (Desejável)
> ❌ Fora do Escopo

### 4.1 Autenticação

| Código | Descrição | Prioridade |
|--------|----------|-----------|
| RF-01 | O sistema deve permitir que o usuário realize login informando e-mail e senha válidos. | 🔴 Obrigatório |
| RF-02 | O sistema deve suportar dois perfis de acesso - VENDEDOR e LOJISTA - com permissões distintas.| 🔴 Obrigatório |
| RF-03 | O sistema deve manter a sessão autenticada do vendedor ativa pelo período de uso, evitando novo login a cada consulta.| 🔴 Obrigatório |
| RF-04 | O sistema deve armazenar senhas exclusivamente como hash seguro, nunca em texto plano.| 🔴 Obrigatório |

---

### 4.2 Cadastro

| Código | Descrição | Prioridade |
|--------|----------|-----------|
| RF-05 | O sistema deve permitir que o lojista cadastre um produto informando nome, marca, categoria e cor.| 🔴 Obrigatório |
| RF-06 | O sistema deve permitir que o lojista cadastre as numerações disponíveis de um produto, formando sua grade.| 🔴 Obrigatório |
| RF-07 | O sistema deve gerar automaticamente o identificador único de cada SKU (produto + numeração), sem intervenção do usuário.| 🔴 Obrigatório |
| RF-08 | O sistema deve impedir o cadastro de um SKU duplicado (mesma combinação de produto + numeração) e informar o lojista.| 🔴 Obrigatório |

---

### 4.3 Estoque

| Código | Descrição | Prioridade |
|--------|----------|-----------|
| RF-09 | O sistema deve permitir que o lojista registre a quantidade inicial de estoque para cada numeração de um produto.| 🔴 Obrigatório |
| RF-10 | O sistema deve registrar toda movimentação de estoque contendo: tipo (entrada/saída/ajuste), quantidade, usuário responsável e data/hora.| 🔴 Obrigatório |
| RF-11 | O sistema deve rejeitar qualquer operação de saída que resulte em saldo negativo, exibindo mensagem de erro clara.| 🔴 Obrigatório |
| RF-12 | O sistema deve exibir a data e hora da última atualização do saldo de cada SKU.| 🟡Importante |

---

### 4.4 Consulta

| Código | Descrição | Prioridade |
|--------|----------|-----------|
| RF-13 | O sistema deve permitir que o vendedor busque um produto pelo nome do modelo, retornando resultado em até 10 segundos.| 🔴 Obrigatório |
| RF-14 | O sistema deve exibir a grade completa do modelo consultado, mostrando todas as numerações com seu saldo atual.| 🔴 Obrigatório |
| RF-15 | O sistema deve diferenciar visualmente três estados: Disponível (saldo > 1) / Último par (saldo = 1) / Indisponível (saldo = 0).| 🔴 Obrigatório |

---

### 4.5 Resultado do Atendimento

| Código | Descrição | Prioridade |
|--------|----------|-----------|
| RF-16 | O sistema deve oferecer ao vendedor, após a consulta, três opções de resultado: Vendeu / Não tinha / Desistiu.| 🔴 Obrigatório |
| RF-17 | Quando o vendedor registrar resultado Vendeu, o sistema deve decrementar em 1 o saldo do SKU que estava sendo consultado.| 🔴 Obrigatório |
| RF-18 | Quando o vendedor registrar resultado Não tinha, o sistema deve criar um registro de ruptura associado ao SKU consultado, sem alterar o saldo.| 🔴 Obrigatório |
| RF-19 | Quando o vendedor registrar resultado Desistiu, o sistema não deve realizar nenhuma movimentação de estoque.| 🔴 Obrigatório |
| RF-20 | O sistema não deve bloquear o vendedor. O registro de resultado é uma ação disponível, mas não obrigatória para continuar usando o sistema.| 🔴 Obrigatório |

---

### 4.6 Visão do Lojista

| Código | Descrição | Prioridade |
|--------|----------|-----------|
| RF-21 | O sistema deve exibir ao lojista quais SKUs estão com saldo zerado, agrupados por modelo.| 🔴 Obrigatório |
| RF-22 | O sistema deve exibir ao lojista o histórico de rupturas registradas, contendo: modelo, numeração e quantidade de ocorrências de 'Não tinha'.| 🟡 Importante |
| RF-23 | O sistema deve permitir que o lojista realize ajuste manual de saldo de qualquer SKU, registrando o motivo da operação.| 🔴 Obrigatório |

---

## 5. Requisitos Não Funcionais

| Código | Descrição | Prioridade |
|--------|----------|-----------|
| RNF-01 | O sistema deve funcionar via browser mobile (Chrome/Safari) sem necessidade de instalação de aplicativo.| 🔴 Obrigatório |
| RNF-02 | O sistema deve responder a consultas de estoque em menos de 500ms em P95, garantindo resultado visível ao usuário em até 3 segundos considerando a rede.| 🔴 Obrigatório |
| RNF-03 | O sistema deve manter disponibilidade mínima durante o horário de funcionamento da loja (12h/dia, 7 dias por semana).| 🔴 Obrigatório |
| RNF-04 | O sistema deve armazenar senhas usando bcrypt com fator de custo mínimo de 12.| 🔴 Obrigatório |
| RNF-05 | Toda comunicação entre cliente e servidor deve ocorrer exclusivamente via HTTPS.| 🔴 Obrigatório |
| RNF-06 | Um vendedor novo deve conseguir realizar sua primeira consulta de estoque sem treinamento formal, em no máximo 2 toques a partir do login.| 🔴 Obrigatório |
| RNF-07 | O sistema deve ser operável em telas de smartphone | 🟡 Importante |

---

## 6. Regras de Negócio

| Código | Regra de Negócio | Descrição | Critério de Aceite |
| :--- | :--- | :--- | :--- |
| **RN-01** | **SKU como unidade mínima de estoque** | O SKU é a combinação única e indivisível de Produto + Numeração. Não podem existir dois SKUs com a mesma combinação no sistema. | Unicidade garantida por constraint no banco de dados. |
| **RN-02** | **Saldo nunca negativo** | Aplica-se a saídas de estoque (venda/ajuste). O sistema deve rejeitar a operação se o saldo resultante for menor que zero. | Validação antes da persistência. |
| **RN-03** | **Movimentação é imutável** | Registros não podem ser editados ou excluídos,aplica-se a todos os registros de movimentação. Erros são corrigidos com nova movimentação de sentido contrário. | Ausência de endpoints de edição ou deleção de movimentações. |
| **RN-04** | **Ajuste manual restrito** | O perfil VENDEDOR não possui acesso à função de ajuste manual de saldo. | Controle de acesso por perfil validado no backend. |
| **RN-05** | **Ruptura por declaração** | Registro ocorre apenas quando o vendedor seleciona 'Não tinha'. Saldo zerado não implica ruptura automática. | Ruptura só é criada pela ação explícita do vendedor. |
| **RN-06** | **Ruptura associada a SKU específico** | Todo registro de ruptura deve referenciar obrigatoriamente um SKU (produto + numeração específica).Registros de ruptura sem SKU associado são inválidos | Campo `sku_id` é NOT NULL no banco de dados. |
| **RN-07** | **Atomicidade nas saídas** | Leitura + validação + decremento devem ocorrer em uma única transação de banco de dados com nível de isolamento adequado. | Sem race condition; sem race condition entre dois vendedores simultâneos. Saldo nunca resulta em valor negativo mesmo sob concorrência.|

--- 

## 7. Priorização de Requisitos

| Classificação | Critério | Itens |
|--------------|---------|------|
| 🔴 Obrigatório | Sistema não funciona sem. Bloqueia o MVP| RF-01 a RF-11, RF-13 a RF-23, RNF-01 a RNF-06, RN-01 a RN07|
| 🟡 Importante | Agrega valor significativo. Deve entrar no MVP se possível.| RF-12, RF-22, RNF-07 |
| 🟢 Desejável |  Entra em versões futuras.| Histórico detalhado, SKUs parados |
| ❌ Fora de Escopo | Não faz parte do sistema | NF-e, PDV, financeiro, ERP, multi-loja |

