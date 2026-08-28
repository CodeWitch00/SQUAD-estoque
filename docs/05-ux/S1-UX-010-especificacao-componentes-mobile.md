# S1-UX-010 — Especificação de componentes mobile: grade e ações do vendedor

**Sprint:** Sprint 1  
**Área:** UX / Frontend  
**Escopo:** busca de produtos, resultados, grade de numerações, saldos, estados de estoque e ações do vendedor.  
**Base visual:** Bootstrap existente, sem introdução de novo framework frontend.

## 1. Objetivo

Definir os componentes responsivos usados pelo vendedor para localizar rapidamente um produto, consultar sua grade de numerações e identificar a disponibilidade de cada SKU durante o atendimento.

A solução prioriza **velocidade, clareza visual, poucos toques e leitura imediata**, considerando que o vendedor utiliza principalmente um smartphone, com o cliente aguardando atendimento.

## 2. Premissas de UX

- A primeira consulta deve ser possível em até **2 interações**, conforme o objetivo do sistema.
- A consulta deve funcionar integralmente no navegador mobile.
- A informação principal é: **qual numeração existe e qual é o saldo**.
- A grade deve apresentar todas as numerações cadastradas para o produto, inclusive as indisponíveis.
- Os estados não devem depender somente de cor: texto, ícone ou rótulo também deve identificar a situação.
- Os componentes devem ser utilizáveis com toque, teclado e leitor de tela.
- O registro do resultado do atendimento é opcional e não deve bloquear a consulta.
- Não criar funcionalidades fora do escopo do cartão.

## 3. Componentes

### 3.1 Campo de busca

**Função:** localizar produtos pelo nome do modelo.

**Componente Bootstrap sugerido:**
- `input-group`
- `form-control`
- `btn`

**Estrutura:**

```text
┌─────────────────────────────────────┐
│ 🔎 Buscar produto ou modelo...      │
└─────────────────────────────────────┘
```

**Comportamento:**
- Placeholder: `Buscar produto ou modelo...`
- Campo deve receber foco facilmente ao abrir a tela.
- Aceitar entrada pelo teclado físico/virtual.
- Pressionar `Enter` deve executar a busca.
- Botão de busca deve ter rótulo acessível.
- Permitir limpar o conteúdo sem precisar apagar caractere por caractere.
- Não exigir filtros adicionais para a consulta principal.

**Dimensões/uso:**
- Altura confortável para toque, preferencialmente próxima ao padrão `form-control` do Bootstrap.
- Área clicável do botão de busca de pelo menos **44 × 44 px**.
- O campo deve ocupar a largura disponível no mobile.

**Foco:**
- Exibir foco visível e consistente.
- Não remover o `outline` de acessibilidade.

**Validação:**
- Busca vazia: não realizar consulta desnecessária; informar de forma breve que o vendedor deve digitar um produto/modelo.
- Nenhum resultado: `Nenhum produto encontrado. Tente outro nome ou modelo.`
- Erro de comunicação: `Não foi possível consultar os produtos. Tente novamente.`

---

### 3.2 Lista de resultados

Cada resultado deve ser uma área claramente selecionável.

**Exemplo mobile:**

```text
RESULTADOS

┌─────────────────────────────────────┐
│ Tênis Runner Pro                    │
│ Nike • Esportivo • Preto            │
│ Grade: 35–40                        │
│                         Ver grade → │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│ Tênis Street Flex                   │
│ Adidas • Casual • Branco            │
│ Grade: 37–42                        │
│                         Ver grade → │
└─────────────────────────────────────┘
```

**Componente Bootstrap sugerido:**
- `card`
- `list-group`
- `btn btn-link` ou área clicável semanticamente equivalente.

**Regras:**
- Nome do produto/modelo deve ter maior destaque.
- Informações secundárias podem apresentar marca, categoria e cor quando existirem no cadastro.
- Evitar excesso de informação.
- Toda a área do resultado pode ser clicável, desde que mantenha comportamento acessível.
- O resultado selecionado deve levar diretamente à grade.

**Estado sem resultados:**

```text
┌─────────────────────────────────────┐
│ Nenhum produto encontrado.          │
│ Tente outro nome ou modelo.         │
└─────────────────────────────────────┘
```

---

## 4. Grade de numerações

A grade representa as combinações de **produto + numeração**, sendo cada combinação um SKU.

Ao selecionar o produto, apresentar a grade completa e o saldo atualizado de cada numeração.

### 4.1 Estrutura mobile recomendada

No mobile, usar cartões compactos ou uma grade flexível de células. Cada célula deve conter:

```text
┌───────────┐
│ Nº 37     │
│ 5 pares   │
│ Disponível│
└───────────┘
```

Exemplo:

```text
GRADE — TÊNIS RUNNER PRO

┌─────────┬─────────┐
│ Nº 35   │ Nº 36   │
│ 3 pares │ 2 pares │
│ Dispon. │ Dispon. │
├─────────┼─────────┤
│ Nº 37   │ Nº 38   │
│ 1 par   │ 0 pares │
│ Último  │ Indisp. │
├─────────┼─────────┤
│ Nº 39   │ Nº 40   │
│ 4 pares │ 0 pares │
│ Dispon. │ Indisp. │
└─────────┴─────────┘
```

**Recomendação Bootstrap:**
- `row`
- `col-6` no mobile para duas células por linha.
- `col-md-3` ou equivalente em desktop para quatro células por linha, conforme a largura disponível.
- `card` ou elemento semântico equivalente para cada SKU.

### 4.2 Informação obrigatória por célula

Cada célula deve exibir:
1. Numeração.
2. Saldo em pares/unidades.
3. Estado: Disponível, Último par ou Indisponível.

O saldo deve permanecer explícito mesmo quando o estado já estiver indicado.

---

## 5. Estados de estoque

A classificação segue o requisito RF-15:

| Saldo | Estado | Apresentação |
|---|---|---|
| Maior que 1 | Disponível | `Disponível — X pares` |
| Igual a 1 | Último par | `Último par — 1 par` |
| Igual a 0 | Indisponível | `Indisponível — 0 pares` |

### 5.1 Disponível

Exemplo:

```text
┌───────────────┐
│ Nº 36         │
│ 4 pares       │
│ DISPONÍVEL    │
└───────────────┘
```

**Uso:** indica que há mais de uma unidade disponível.

### 5.2 Último par

Exemplo:

```text
┌───────────────┐
│ Nº 37         │
│ 1 par         │
│ ÚLTIMO PAR    │
└───────────────┘
```

**Uso:** deve chamar atenção para a necessidade de reposição sem confundir o vendedor: o produto está disponível, mas resta apenas uma unidade.

### 5.3 Indisponível

Exemplo:

```text
┌───────────────┐
│ Nº 38         │
│ 0 pares       │
│ INDISPONÍVEL  │
└───────────────┘
```

**Uso:** deixa claro que não há saldo registrado para aquele SKU.

**Acessibilidade:** não usar somente verde/amarelo/vermelho. O texto do estado deve estar presente na própria célula.

---

## 6. Ações do vendedor

Após visualizar a grade, o vendedor pode registrar o resultado do atendimento:

- **Vendeu**
- **Não tinha**
- **Desistiu**

Essas ações correspondem ao fluxo operacional do vendedor e devem permanecer simples.

### 6.1 Mobile

As ações devem ficar próximas da grade e possuir áreas de toque confortáveis.

```text
RESULTADO DO ATENDIMENTO

┌─────────────────────────────────────┐
│ ✓ Vendeu                            │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│ ! Não tinha                         │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│ × Desistiu                          │
└─────────────────────────────────────┘
```

**Regra importante:** o registro do resultado é opcional. O vendedor pode consultar a grade e continuar o fluxo sem registrar uma dessas opções.

### 6.2 Comportamento das ações

**Vendeu**
- Decrementa automaticamente uma unidade do SKU consultado.
- A operação não pode resultar em saldo negativo.
- Deve registrar a movimentação correspondente.
- Após sucesso, atualizar a informação de saldo exibida.

**Não tinha**
- Cria o registro de ruptura associado ao SKU.
- Não altera o saldo registrado.
- Deve confirmar o registro de forma breve, sem retirar o vendedor do fluxo.

**Desistiu**
- Não altera estoque.
- Não cria movimentação de estoque.
- Não cria registro de ruptura.

## 7. Mensagens e feedback

As mensagens devem ser curtas e aparecer próximas à ação realizada.

**Sucesso — venda:**
`Venda registrada. Saldo atualizado.`

**Sucesso — ruptura:**
`Ruptura registrada.`

**Desistência:**
`Atendimento encerrado sem alteração no estoque.`

**Saldo insuficiente:**
`Não foi possível registrar a venda: saldo insuficiente.`

**Erro genérico:**
`Não foi possível concluir a ação. Tente novamente.`

Evitar modais para mensagens simples. Usar componentes Bootstrap como `alert`, `toast` ou feedback inline conforme o padrão já existente no projeto.

## 8. Teclado e foco

Embora o principal dispositivo seja o smartphone, a interface também deve funcionar em desktop/tablet.

- Ordem de foco: busca → resultados → grade → ações.
- O foco deve ser sempre visível.
- `Tab` deve percorrer os elementos interativos em ordem lógica.
- `Enter`/`Space` devem ativar controles acionáveis.
- Botões não devem ser representados apenas por ícones.
- Ícones devem possuir texto visível ou nome acessível.
- Após uma ação, o foco não deve ser perdido de maneira inesperada.

## 9. Alvos de toque

Para o uso no chão de loja:

- Botões e controles principais: alvo de aproximadamente **44 × 44 px ou maior**.
- Espaçamento suficiente entre ações para evitar toques acidentais.
- Não concentrar três ações em pequenos ícones lado a lado.
- A grade pode ser compacta, mas cada célula que seja interativa deve manter área de toque confortável.
- Não exigir gestos complexos como arrastar ou deslizar para consultar o estoque.

## 10. Responsividade

### Mobile

Prioridade:
1. Busca.
2. Resultados.
3. Produto selecionado.
4. Grade.
5. Ações do vendedor.

Exemplo:

```text
┌───────────────────────────┐
│ SQUAD                     │
├───────────────────────────┤
│ 🔎 Buscar produto...      │
├───────────────────────────┤
│ Tênis Runner Pro          │
│ Grade de numerações       │
│                           │
│ ┌───────┐ ┌───────┐       │
│ │ 35    │ │ 36    │       │
│ │ 3     │ │ 2     │       │
│ │ DISP. │ │ DISP. │       │
│ └───────┘ └───────┘       │
│ ┌───────┐ ┌───────┐       │
│ │ 37    │ │ 38    │       │
│ │ 1     │ │ 0     │       │
│ │ ÚLT.  │ │ INDIS.│       │
│ └───────┘ └───────┘       │
│                           │
│ RESULTADO DO ATENDIMENTO  │
│ [✓ Vendeu]                │
│ [! Não tinha]             │
│ [× Desistiu]              │
└───────────────────────────┘
```

### Desktop/tablet

Em telas maiores, aproveitar o espaço horizontal sem transformar a consulta em dashboard.

```text
┌────────────────────────────────────────────────────────────┐
│ SQUAD                                      Vendedor         │
├────────────────────────────────────────────────────────────┤
│ 🔎 Buscar produto ou modelo...                 [Buscar]    │
├────────────────────────────────────────────────────────────┤
│ Resultados                                                  │
│ ┌────────────────────────┐  ┌───────────────────────────┐  │
│ │ Tênis Runner Pro       │  │ Grade — Runner Pro        │  │
│ │ Nike • Preto           │  │                           │  │
│ │ [Ver grade]            │  │ 35   36   37   38         │  │
│ │                        │  │ 3    2    1    0          │  │
│ └────────────────────────┘  │ DISP DISP ÚLT INDIS       │  │
│                             │                           │  │
│                             │ 39   40                    │  │
│                             │ 4    0                     │  │
│                             │ DISP INDIS                 │  │
│                             └───────────────────────────┘  │
│                                                            │
│ Resultado: [Vendeu] [Não tinha] [Desistiu]                 │
└────────────────────────────────────────────────────────────┘
```

No desktop, a lista de resultados pode ocupar uma coluna e a grade a área principal. No mobile, as áreas devem ser empilhadas.

## 11. Estados gerais da interface

### Carregando
- Exibir indicador de carregamento no local do conteúdo.
- Evitar apresentar dados antigos como se fossem atuais.
- Manter o contexto da busca.

### Sem busca
```text
Digite o nome de um produto ou modelo para consultar o estoque.
```

### Sem resultados
```text
Nenhum produto encontrado.
Tente outro nome ou modelo.
```

### Produto sem SKUs/grade
```text
Este produto ainda não possui numerações cadastradas.
```

### Erro de consulta
```text
Não foi possível consultar o estoque.
Tente novamente.
```

### Erro ao registrar ação
```text
Não foi possível concluir a ação.
O estoque pode ter sido atualizado por outro atendimento. Consulte novamente.
```

## 12. Atualização do saldo

O saldo exibido deve representar a informação atualizada do SKU.

Quando `Vendeu` for concluído:
- saldo 2 → saldo 1: estado muda de **Disponível** para **Último par**;
- saldo 1 → saldo 0: estado muda de **Último par** para **Indisponível**.

Nunca permitir que a interface apresente ou confirme um saldo negativo.

## 13. Bootstrap e implementação

A especificação deve ser implementada usando os recursos já disponíveis no projeto, especialmente:

- sistema de grid responsivo (`container`, `row`, `col-*`);
- `form-control`;
- `input-group`;
- `btn`;
- `card`;
- `alert`/`toast` para feedback;
- utilitários de espaçamento e responsividade.

**Não introduzir React, Vue, Angular, Tailwind ou outro framework frontend.**

A implementação deve respeitar a estrutura atual do `SquadEstoque.Web` e reutilizar componentes/estilos já existentes quando houver equivalentes.

## 14. Critérios de revisão

Antes de considerar o artefato concluído, verificar:

- [x] Campo de busca definido.
- [x] Lista de resultados definida.
- [x] Grade completa de numerações definida.
- [x] Saldo definido por SKU.
- [x] Estados Disponível, Último par e Indisponível definidos.
- [x] Ações Vendeu, Não tinha e Desistiu definidas.
- [x] Estados de carregamento, vazio, erro e ausência de resultados previstos.
- [x] Teclado e foco especificados.
- [x] Alvos de toque confortáveis previstos.
- [x] Mobile e desktop exemplificados.
- [x] Bootstrap mantido como base.
- [x] Nenhum framework frontend novo proposto.
- [x] Especificação alinhada ao fluxo real do vendedor.

## 15. Rastreabilidade

| Item | Relação com a especificação |
|---|---|
| RF-13 | Busca de produtos pelo nome do modelo |
| RF-14 | Grade completa com saldo por numeração |
| RF-15 | Estados Disponível, Último par e Indisponível |
| RF-16 | Registro do resultado do atendimento |
| RF-17 | Baixa automática ao registrar Vendeu |
| RF-18 | Registro de ruptura ao selecionar Não tinha |
| RF-19 | Desistiu sem alteração de estoque |
| RF-20 | Registro do resultado é opcional |
| RNF-01 | Operação via browser mobile |
| RNF-07 | Interface responsiva |
| UC-02 | Consultar Estoque |
| UC-03 | Visualizar Grade |
| UC-04 | Registrar: Vendeu |
| UC-05 | Registrar: Não tinha |
| UC-06 | Registrar: Desistiu |

## 16. Fontes internas do projeto

- **Documento de Requisitos / SRS:** RF-13 a RF-20 e requisitos de responsividade.
- **Documento de Visão e Domínio:** persona Vendedor, contexto mobile, consulta rápida e objetivo de até 2 toques.
- **Monografia Final — Squad:** fluxos UC-02 a UC-06 e comportamento das ações do vendedor.
- **Escopo MVP:** consulta de estoque, grade, estados de disponibilidade e registro de resultado.
