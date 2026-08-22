# MONOGRAFIA - SISTEMA SQUAD

> **Título:** Sistema SQUAD: Modelagem e Documentação Acadêmica de um Sistema Web de Controle de Estoque para Rastreamento de Ruptura no Varejo Calçadista

> **Norma:** ABNT NBR 14724:2011

> **Status:** Estrutura Acadêmica Final

---

# ELEMENTOS PRÉ-TEXTUAIS

---

# CAPA *(obrigatório)*

- Nome da Instituição
- Nome do Curso
- Nome dos Autores
- Título
- Subtítulo (se houver)
- Cidade
- Ano

---

# FOLHA DE ROSTO *(obrigatório)*

- Nome dos autores
- Título do trabalho
- Subtítulo (se houver)

Texto de natureza do trabalho:

> Monografia apresentada ao curso de [NOME DO CURSO] da [NOME DA INSTITUIÇÃO], como requisito parcial para obtenção do título de [TÍTULO].

- Nome do orientador
- Cidade
- Ano

---

# FICHA CATALOGRÁFICA *(obrigatório)*

---

# TERMO DE APROVAÇÃO *(obrigatório)*

- Nome dos autores
- Título
- Data de aprovação
- Banca examinadora
- Assinaturas

---

# RESUMO *(obrigatório)*

## Estrutura esperada
- Contextualização breve
- Objetivo
- Metodologia
- Desenvolvimento
- Resultados esperados
- Conclusão

## Palavras-chave
Exemplo:
- Controle de estoque
- Ruptura de estoque
- Engenharia de software
- Modelagem de dados
- Sistemas web

---

# ABSTRACT *(obrigatório)*

> Tradução fiel do resumo para inglês.

## Keywords
- Inventory control
- Stockout
- Software engineering
- Data modeling
- Web systems

---

# LISTA DE FIGURAS *(recomendado)*

---

# LISTA DE QUADROS *(recomendado)*

---

# LISTA DE ABREVIATURAS E SIGLAS *(recomendado)*

| Sigla | Significado |
|---|---|
| SKU | Stock Keeping Unit |
| SRS | Software Requirements Specification |
| RF | Requisito Funcional |
| RN | Regra de Negócio |
| RNF | Requisito Não Funcional |
| MVP | Minimum Viable Product |
| PDV | Ponto de Venda |
| API | Application Programming Interface |

---

# SUMÁRIO *(obrigatório)*

---

# ELEMENTOS TEXTUAIS

---

# 1 INTRODUÇÃO

> Objetivo do capítulo:
> apresentar o contexto, o problema, a relevância, os objetivos e a organização do trabalho.

---

## 1.1 Contextualização

### Tópicos
- Varejo calçadista
- Controle de estoque
- Problemas operacionais no chão de loja
- Ruptura de estoque
- Falta de rastreabilidade
- Impacto na experiência do cliente
- Impacto na tomada de decisão

### Diretrizes
- Linguagem acadêmica
- Evitar linguagem coloquial
- Evitar storytelling excessivo
- Introduzir o problema progressivamente

---

## 1.2 Problema de Pesquisa

### Problema central

> Como a modelagem de um sistema de controle de estoque orientado à rastreabilidade de ruptura pode apoiar a redução de perdas operacionais no varejo calçadista?

---

## 1.3 Justificativa

### Abordar
- Impacto financeiro da ruptura
- Perda invisível de vendas
- Falta de dados estruturados
- Dificuldade operacional
- Relevância da rastreabilidade
- Importância de sistemas especializados

### Justificativa acadêmica
- Aplicação prática da engenharia de software
- Modelagem de sistemas operacionais
- Rastreabilidade de requisitos

### Justificativa tecnológica
- Sistemas web
- Controle transacional
- Integridade de dados

---

## 1.4 Objetivo Geral

> Desenvolver a modelagem e a documentação técnica de um sistema web de controle de estoque voltado à rastreabilidade de ruptura no varejo calçadista.

---

## 1.5 Objetivos Específicos

- Levantar requisitos funcionais e não funcionais
- Definir regras de negócio
- Modelar entidades e relacionamentos
- Estruturar a arquitetura do sistema
- Garantir rastreabilidade entre requisitos e modelagem
- Validar a coerência da solução proposta
- Garantir integridade operacional do estoque

---

## 1.6 Estrutura do Trabalho

### Explicar resumidamente
- Capítulo 1 — Introdução
- Capítulo 2 — Fundamentação teórica
- Capítulo 3 — Metodologia
- Capítulo 4 — Análise do problema e requisitos
- Capítulo 5 — Projeto e modelagem
- Capítulo 6 — Validação
- Capítulo 7 — Considerações finais

---

# 2 FUNDAMENTAÇÃO TEÓRICA

> Objetivo:
> apresentar a base conceitual e científica necessária para sustentar a modelagem proposta.

---

# 2.1 Gestão de Estoque no Varejo

## Conceitos
- Estoque
- Controle de estoque
- Disponibilidade de produto
- Acuracidade de estoque
- Estoque de segurança
- Giro de estoque

## Impactos
- Indisponibilidade
- Perda de vendas
- Impacto operacional
- Impacto financeiro

---

# 2.2 Ruptura de Estoque

## Conceitos
- Ruptura
- Ruptura visível
- Ruptura invisível
- Perda operacional

## Impactos
- Frustração do cliente
- Perda de vendas
- Decisões incorretas de reposição

## Dados operacionais
- Importância da rastreabilidade
- Dados estruturados
- Histórico operacional

---

# 2.3 Sistemas de Informação no Varejo

## Sistemas operacionais
- Sistemas transacionais
- Sistemas de apoio à decisão
- ERP
- Sistemas especializados

## Sistemas web
- Aplicações browser-based
- Mobilidade operacional
- Interfaces responsivas

## Informação operacional
- Dados operacionais
- Apoio à decisão
- Integridade da informação

---

# 2.4 Engenharia de Software

---

## 2.4.1 Engenharia de Requisitos

### Conceitos
- Requisitos funcionais
- Requisitos não funcionais
- Regras de negócio
- Validação de requisitos
- Rastreabilidade

---

## 2.4.2 Casos de Uso

### Conceitos
- Casos de uso
- Fluxos principais
- Fluxos alternativos
- Interação entre ator e sistema

### Objetivos
- Representação comportamental
- Documentação funcional

---

## 2.4.3 Modelagem de Dados

### Conceitos
- Entidades
- Relacionamentos
- Cardinalidade
- Integridade referencial
- Normalização

### Banco de dados relacional
- Controle de consistência
- Constraints
- Integridade transacional

---

## 2.4.4 Arquitetura de Sistemas

### Conceitos
- Arquitetura cliente-servidor
- Sistemas web
- APIs
- Separação de responsabilidades

### Comunicação entre camadas
- Frontend
- Backend
- Banco de dados

---

## 2.4.5 Controle Transacional e Concorrência

### Conceitos
- Atomicidade
- Concorrência
- Race condition
- Transações atômicas

### Integridade operacional
- Consistência de dados
- Controle de saldo
- Prevenção de inconsistências

---

# 3 METODOLOGIA

> Objetivo:
> apresentar a abordagem metodológica utilizada no desenvolvimento do trabalho.

---

# 3.1 Natureza da Pesquisa

## Pesquisa aplicada

### Justificativa
- Desenvolvimento de solução prática
- Resolução de problema operacional real

---

# 3.2 Abordagem da Pesquisa

## Pesquisa qualitativa

### Justificativa
- Análise operacional
- Compreensão do processo de estoque
- Modelagem de solução
- Ausência de análise estatística

---

# 3.3 Objetivos da Pesquisa

## Pesquisa exploratória
- Compreensão do problema

## Pesquisa descritiva
- Descrição dos processos operacionais
- Estruturação da solução proposta

---

# 3.4 Procedimentos Técnicos

## Estudo de caso

### Contexto
- Varejo calçadista
- Operação de estoque
- Processo de atendimento

---

# 3.5 Coleta de Dados

## Técnicas utilizadas
- Observação operacional
- Levantamento documental
- Identificação de gargalos
- Entrevistas informais

---

# 3.6 Etapas do Trabalho

---

## Etapa 1 — Análise do problema

### Atividades
- Identificação de gargalos
- Mapeamento operacional
- Levantamento do cenário atual

---

## Etapa 2 — Levantamento de requisitos

### Atividades
- Definição de requisitos funcionais
- Definição de requisitos não funcionais
- Definição de regras de negócio

---

## Etapa 3 — Modelagem do sistema

### Atividades
- Casos de uso
- Modelagem lógica
- Modelagem física

---

## Etapa 4 — Definição arquitetural

### Atividades
- Arquitetura cliente-servidor
- Estrutura da aplicação web
- Banco de dados

---

## Etapa 5 — Validação da solução

### Atividades
- Revisão documental
- Verificação de consistência
- Validação lógica

---

# 4 ANÁLISE DO PROBLEMA E LEVANTAMENTO DE REQUISITOS

> Objetivo:
> apresentar o cenário operacional e os requisitos que fundamentaram a solução.

---

# 4.1 Cenário Atual (AS-IS)

## Situação operacional
- Processo manual
- Dependência de consulta física
- Falta de rastreabilidade

## Problemas observados
- Ruptura invisível
- Perda operacional
- Falta de dados estruturados

---

# 4.2 Gargalos Identificados

## Gargalos operacionais
- Consulta lenta
- Ausência de visibilidade
- Falta de histórico
- Decisão baseada em percepção

---

# 4.3 Partes Interessadas

---

## 4.3.1 Vendedor

### Características
- Consulta rápida
- Baixa tolerância à complexidade
- Uso via smartphone

---

## 4.3.2 Lojista

### Características
- Visibilidade operacional
- Apoio à decisão
- Controle gerencial

---

# 4.4 Requisitos do Sistema

---

## 4.4.1 Requisitos Funcionais

### Principais requisitos
- Autenticação
- Consulta de estoque
- Controle de SKU
- Registro de movimentação
- Registro de ruptura
- Ajuste manual

---

## 4.4.2 Requisitos Não Funcionais

### Principais requisitos
- Performance
- Disponibilidade
- Segurança
- Responsividade
- Usabilidade mobile

---

# 4.5 Regras de Negócio

## Regras principais
- SKU como unidade mínima
- Saldo não negativo
- Imutabilidade
- Atomicidade
- Ruptura explícita
- Restrição de ajuste manual

---

# 5 PROJETO E MODELAGEM DO SISTEMA

> Objetivo:
> apresentar a estrutura técnica e arquitetural da solução proposta.

---

# 5.1 Visão Geral do Sistema

## Objetivos
- Consulta rápida de estoque
- Registro de ruptura
- Controle operacional

## Escopo operacional
- Operação paralela ao PDV
- Sistema especializado

---

# 5.2 Escopo do MVP

---

## 5.2.1 Funcionalidades incluídas

### Funcionalidades
- Consulta de estoque
- Registro de ruptura
- Controle por SKU
- Movimentação de estoque
- Ajuste manual

---

## 5.2.2 Funcionalidades fora do escopo

### Exclusões
- ERP
- Financeiro
- Multi-loja
- Analytics avançado
- Integração fiscal

---

# 5.3 Casos de Uso

---

## 5.3.1 Diagrama de Casos de Uso

---

## 5.3.2 Principais Fluxos

### Fluxos principais
- Consulta de estoque
- Venda
- Registro de ruptura
- Ajuste manual

---

# 5.4 Arquitetura do Sistema

---

## 5.4.1 Arquitetura Cliente-Servidor

---

## 5.4.2 Aplicação Web Mobile

---

## 5.4.3 Comunicação entre camadas

### Camadas
- Frontend
- Backend
- Banco de dados

---

## 5.4.4 Justificativas arquiteturais

### Decisões
- Aplicação web
- Responsividade
- Separação de responsabilidades

---

## 5.4.5 Tecnologias Utilizadas

### Backend

### Frontend

### Banco de dados

### API

### Browser mobile

### Justificativas técnicas

---

# 5.5 Modelagem de Dados

---

## 5.5.1 Modelo Conceitual

---

## 5.5.2 Modelo Lógico

---

## 5.5.3 Modelo Físico

---

## 5.5.4 Entidades Principais

### Entidades
- Produto
- SKU
- Movimentação
- Ruptura
- Usuário

---

## 5.5.5 Relacionamentos

---

## 5.5.6 Decisões de Modelagem

### Decisões principais
- Separação Produto/SKU
- Controle de saldo
- Histórico imutável
- Ruptura como entidade
- Constraints relacionais
- Controle transacional

---

# 5.6 Controle Transacional e Integridade

---

## 5.6.1 Atomicidade

### Objetivos
- Evitar saldo negativo
- Garantir consistência

---

## 5.6.2 Concorrência

### Cenários
- Venda simultânea
- Controle do último par

---

## 5.6.3 Integridade Operacional

### Mecanismos
- Constraints
- Transações
- Validações

---

# 5.7 Rastreabilidade

---

## 5.7.1 Matriz de Rastreabilidade

| Problema | Objetivo | RF | RN | UC | Entidade |
|---|---|---|---|---|---|

---

## 5.7.2 Coerência entre requisitos e modelagem

### Validações
- Requisitos x casos de uso
- Requisitos x entidades
- Regras x constraints

---

# 6 VALIDAÇÃO DA SOLUÇÃO PROPOSTA

> Objetivo:
> validar a coerência técnica e operacional da solução modelada.

---

# 6.1 Estratégia de Validação

## Métodos
- Revisão documental
- Consistência lógica
- Verificação arquitetural
- Validação dos requisitos

---

# 6.2 Cenários de Uso

---

## 6.2.1 Consulta de estoque

---

## 6.2.2 Venda simultânea

---

## 6.2.3 Registro de ruptura

---

## 6.2.4 Ajuste manual

---

# 6.3 Validação da Atomicidade

## Cenário analisado
- Dois vendedores
- Último item em estoque

## Objetivos
- Garantir integridade
- Evitar concorrência inconsistente
- Impedir saldo negativo

## Estratégias utilizadas
- Transação atômica
- FOR UPDATE
- Controle transacional

---

# 6.4 Resultados Esperados

## Resultados operacionais
- Redução da ruptura invisível
- Melhor rastreabilidade
- Apoio à decisão
- Integridade operacional

---

# 6.5 Limitações

## Limitações
- Sem integração ERP
- Sem multi-loja
- Sem analytics avançado
- Dependência operacional manual

---

# 7 CONSIDERAÇÕES FINAIS

---

# 7.1 Síntese do Trabalho

## Síntese
- Problema analisado
- Solução proposta
- Modelagem desenvolvida
- Resultados esperados

---

# 7.2 Contribuições

---

## 7.2.1 Contribuições acadêmicas

### Contribuições
- Engenharia de software aplicada
- Modelagem de sistemas
- Rastreabilidade
- Integridade transacional

---

## 7.2.2 Contribuições práticas

### Contribuições
- Apoio operacional
- Controle de ruptura
- Integridade de estoque
- Visibilidade operacional

---

# 7.3 Trabalhos Futuros

## Possibilidades
- Integração ERP
- Inteligência analítica
- Previsão de ruptura
- Multi-loja
- Dashboards avançados

---

# ELEMENTOS PÓS-TEXTUAIS

---

# REFERÊNCIAS *(obrigatório)*

## Tipos de referência
- Livros
- Artigos científicos
- Trabalhos acadêmicos
- Normas
- Sites institucionais

---

# GLOSSÁRIO *(recomendado)*

| Termo | Definição |
|---|---|
| SKU | Unidade mínima de controle |
| Ruptura | Falta de disponibilidade do item |
| Grade | Conjunto de numerações |
| Saldo | Quantidade disponível |
| MVP | Produto mínimo viável |

---

# APÊNDICES

---

# APÊNDICE A — Documento de Domínio

---

# APÊNDICE B — SRS Completo

---

# APÊNDICE C — Casos de Uso Detalhados

---

# APÊNDICE D — Modelo Conceitual

---

# APÊNDICE E — Modelo Lógico

---

# APÊNDICE F — Modelo Físico

---

# APÊNDICE G — Fluxos Operacionais

---

# ANEXOS

---

# ANEXO A — Diagramas Complementares

---

# ANEXO B — Materiais Complementares
