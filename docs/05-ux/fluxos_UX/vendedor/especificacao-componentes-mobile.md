Especificação de componentes mobile: grade e ações do vendedor

## 1. Objetivo

Especificar os componentes responsivos utilizados pelo vendedor para realizar a busca de modelos, visualizar a grade de numerações, consultar o saldo de cada SKU, selecionar uma numeração e registrar o resultado do atendimento.

A especificação prioriza rapidez, clareza das informações, poucos toques e facilidade de uso em dispositivos móveis, mantendo compatibilidade com desktop e tablet.

A unidade operacional das ações de estoque é o **SKU**, correspondente à combinação entre produto e numeração. As ações de estoque devem sempre ser executadas sobre o SKU selecionado.

---

## 2. Premissas de UX

* O fluxo deve priorizar o uso em smartphone.
* A primeira consulta deve ser realizada em até **2 toques a partir do login**.
* A busca deve ser realizada pelo nome ou parte do nome do modelo.
* Buscas com menos de 2 caracteres não devem gerar requisição ao servidor.
* A grade deve apresentar todas as numerações cadastradas para o modelo.
* Cada SKU deve apresentar numeração, saldo, estado de disponibilidade e última atualização do saldo.
* Uma numeração deve ser selecionada antes da execução de `Vendeu` ou `Não tinha`.
* `Vendeu` e `Não tinha` devem permanecer indisponíveis enquanto nenhum SKU estiver selecionado.
* `Desistiu` não exige seleção de SKU.
* O resultado do atendimento é opcional.
* O vendedor deve conseguir iniciar uma nova consulta sem registrar um resultado.
* Os estados de estoque não devem depender exclusivamente de cores.
* Os componentes devem possuir foco visível e áreas de toque confortáveis.
* A implementação deve utilizar Bootstrap, já existente no projeto.
* Não deve ser introduzido novo framework frontend.
* O fluxo operacional do vendedor é independente da tela administrativa de `/Produtos/Details`.

---

# 3. Fluxo de consulta

O fluxo principal do vendedor é:

```text
Login
  ↓
Tela de consulta
  ↓
Buscar modelo
  ↓
Resultado da busca
  ↓
Selecionar modelo
  ↓
Visualizar grade
  ↓
Selecionar SKU
  ↓
Vendeu / Não tinha
```

O fluxo de `Desistiu` pode ser iniciado após a exibição da grade, sem seleção de SKU:

```text
Grade consultada
  ↓
Desistiu
  ↓
Retorno à consulta
```

Também é possível consultar a grade sem registrar nenhum resultado:

```text
Grade
  ↓
Nova consulta
  ↓
Tela de consulta
```

---

# 4. Atendimento ao requisito de dois toques

O requisito de primeira consulta em até dois toques a partir do login é atendido da seguinte maneira:

### Toque 1

O vendedor toca no campo de busca disponível na tela operacional.

Em seguida, digita pelo menos dois caracteres do nome ou parte do nome do modelo. A busca é executada automaticamente, após um pequeno intervalo sem digitação (*debounce*), sem exigir um toque adicional.

### Toque 2

O vendedor toca no resultado correspondente ao modelo desejado.

A grade do modelo é então apresentada.

A seleção da numeração e o registro de `Vendeu` ou `Não tinha` ocorrem após a primeira consulta e não fazem parte da contagem dos dois toques necessários para chegar ao resultado da consulta.

O teclado virtual deve permitir a execução da busca por `Enter`/`Pesquisar` para antecipar a consulta automática. Essa ação é opcional e não deve ser necessária para que os resultados sejam apresentados.

---

# 5. Campo de busca

## 5.1 Função

O campo permite localizar modelos pelo nome ou por parte do nome.

### Placeholder

```text
Buscar modelo...
```

Marca, categoria e cor podem ser exibidas como informações complementares no resultado, mas não constituem critérios de busca definidos nesta especificação.

## 5.2 Componente

A implementação deve utilizar componentes Bootstrap, como:

* `input-group`;
* `form-control`;
* `btn`, quando necessário.

Exemplo:

```text
┌─────────────────────────────────────┐
│ 🔎 Buscar modelo...                 │
└─────────────────────────────────────┘
```

## 5.3 Validação mínima

A busca exige no mínimo **2 caracteres**.

Enquanto o campo possuir menos de dois caracteres:

* nenhuma requisição deve ser enviada ao servidor;
* a busca não deve ser executada;
* deve ser exibida a mensagem:

```text
Digite pelo menos 2 caracteres para buscar.
```

Espaços isolados não devem ser considerados uma busca válida.

## 5.4 Teclado e foco

* O campo deve receber foco facilmente ao entrar na tela de consulta.
* O teclado virtual deve disponibilizar uma ação de pesquisa.
* Ao atingir pelo menos dois caracteres válidos, a busca deve ser executada automaticamente após um pequeno intervalo sem digitação (*debounce*).
* `Enter` deve executar imediatamente a busca válida, antecipando o intervalo automático.
* O foco deve permanecer visualmente identificado.
* O indicador de foco não deve ser removido.
* O botão de limpar, quando utilizado, deve possuir identificação acessível.

## 5.5 Estados

### Campo vazio

```text
Digite o nome de um modelo para consultar o estoque.
```

### Menos de dois caracteres

```text
Digite pelo menos 2 caracteres para buscar.
```

### Nenhum resultado

```text
Nenhum modelo encontrado. Tente outro nome.
```

### Erro

```text
Não foi possível consultar os modelos. Tente novamente.
```

### Carregamento

Durante a consulta, deve ser apresentado um indicador de carregamento no espaço destinado aos resultados.

---

# 6. Desempenho da consulta

A consulta deve respeitar os seguintes critérios:

* resposta da aplicação abaixo de **500 ms em P95**;
* resultado visível ao usuário em até **3 segundos**, considerando as condições de rede.

O estado de carregamento deve ser apresentado enquanto a consulta estiver sendo processada.

---

# 7. Lista de resultados

Cada resultado representa um modelo encontrado pela busca.

Exemplo mobile:

```text
RESULTADOS

┌─────────────────────────────────────┐
│ Tênis Runner Pro                    │
│ Nike • Esportivo • Preto            │
│ Numerações: 35, 36, 37, 40          │
│                         Ver grade → │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│ Tênis Street Flex                   │
│ Adidas • Casual • Branco            │
│ Numerações: 37, 38, 40, 42          │
│                         Ver grade → │
└─────────────────────────────────────┘
```

## 7.1 Informações

O resultado deve apresentar:

* nome do modelo;
* informações complementares disponíveis, como marca, categoria e cor;
* numerações efetivamente cadastradas, quando exibidas no resultado;
* indicação de que a grade pode ser consultada.

Marca, categoria e cor são informações de apresentação e não critérios de busca.

## 7.2 Interação

O resultado deve possuir área de toque confortável.

Ao selecionar um resultado, o sistema deve apresentar a grade de numerações correspondente ao modelo selecionado.

---

# 8. Grade de numerações

A grade representa os SKUs disponíveis para o modelo selecionado.

Cada combinação de produto e numeração corresponde a um SKU.

A grade deve apresentar todas as numerações cadastradas, independentemente de possuírem saldo ou não.

## 8.1 Informações da célula

Cada célula deve apresentar:

1. Numeração;
2. Saldo atual;
3. Estado de disponibilidade;
4. Data e hora da última atualização do saldo.

Exemplo:

```text
┌───────────────┐
│ Nº 36         │
│ 4 pares       │
│ DISPONÍVEL    │
│ Atualizado:   │
│ 26/08 14:32   │
└───────────────┘
```

A interface apresenta a última atualização do saldo, sem definir a forma de persistência dessa informação.

---

# 9. Seleção do SKU

A seleção da numeração é obrigatória para as ações `Vendeu` e `Não tinha`.

## 9.1 Estado normal

No estado normal, a célula apresenta as informações do SKU sem destaque de seleção.

```text
┌───────────────┐
│ Nº 36         │
│ 4 pares       │
│ DISPONÍVEL    │
│ Atualizado:   │
│ 26/08 14:32   │
└───────────────┘
```

A célula pode ser tocada ou selecionada pelo teclado.

## 9.2 Estado selecionado

Após a seleção:

```text
┌═══════════════┐
║ ✓ Nº 36       ║
║ 4 pares       ║
║ DISPONÍVEL    ║
║ Atualizado:   ║
║ 26/08 14:32   ║
└═══════════════┘

SKU selecionado: Nº 36
```

O estado selecionado deve:

* possuir destaque visual evidente;
* apresentar indicador de seleção;
* manter a numeração visível;
* manter o saldo visível;
* permitir identificar claramente qual SKU será utilizado na ação;
* permitir a seleção de outra numeração;
* manter apenas um SKU selecionado por vez.

As ações devem ser enviadas para o **SKU selecionado**, nunca apenas para o produto.

---

# 10. Disponibilidade das ações

Enquanto nenhum SKU estiver selecionado:

```text
RESULTADO DO ATENDIMENTO

[ Vendeu ]       Desabilitado
[ Não tinha ]    Desabilitado
[ Desistiu ]     Disponível

Selecione uma numeração para registrar
Vendeu ou Não tinha.
```

Após a seleção:

```text
SKU selecionado: Nº 36

[ ✓ Vendeu ]
[ ! Não tinha ]
[ × Desistiu ]
```

`Vendeu` e `Não tinha` exigem SKU selecionado.

`Desistiu` fica disponível após a exibição da grade e pode ser executado independentemente da seleção de SKU.

---

# 11. Estados de estoque

A classificação dos SKUs é:

| Saldo       | Estado       |
| ----------- | ------------ |
| Maior que 1 | Disponível   |
| Igual a 1   | Último par   |
| Igual a 0   | Indisponível |

## 11.1 Disponível

```text
┌───────────────┐
│ Nº 36         │
│ 4 pares       │
│ DISPONÍVEL    │
└───────────────┘
```

Representa saldo superior a uma unidade.

## 11.2 Último par

```text
┌───────────────┐
│ Nº 37         │
│ 1 par         │
│ ÚLTIMO PAR    │
└───────────────┘
```

Representa saldo igual a uma unidade.

## 11.3 Indisponível

```text
┌───────────────┐
│ Nº 38         │
│ 0 pares       │
│ INDISPONÍVEL  │
└───────────────┘
```

Representa saldo igual a zero.

A identificação do estado deve utilizar texto e/ou ícone além de qualquer diferenciação visual por cor.

---

# 12. Ação — Vendeu

## 12.1 Pré-condição

Um SKU deve estar selecionado.

## 12.2 Comportamento

Ao selecionar `Vendeu`:

1. A ação é vinculada ao SKU selecionado.
2. O sistema deve decrementar exatamente **1 unidade** do saldo atual.
3. O saldo não pode se tornar negativo.
4. Deve ser registrada uma movimentação de saída.
5. A movimentação deve identificar o usuário responsável e a data/hora.
6. A interface deve confirmar o sucesso da operação.
7. A grade deve apresentar o saldo atualizado.
8. O vendedor deve conseguir iniciar uma nova consulta.

Mensagem de sucesso:

```text
Venda registrada. Saldo atualizado.
```

## 12.3 Saldo igual a zero

Quando o saldo atual do SKU for zero:

* a venda deve ser rejeitada;
* o saldo não deve ser alterado;
* nenhuma saída deve ser registrada;
* a grade deve continuar apresentando o saldo vigente.

Mensagem:

```text
Não foi possível registrar a venda: saldo indisponível.
```

## 12.4 Concorrência ou saldo insuficiente

Quando outra operação alterar o saldo antes da conclusão da venda, o sistema deve considerar o saldo vigente.

A interface deve:

1. rejeitar a operação caso o saldo não permita a saída;
2. atualizar a célula do SKU;
3. apresentar o saldo vigente;
4. atualizar o estado de disponibilidade;
5. informar o vendedor sobre a atualização.

Exemplo:

```text
Venda não registrada.
O saldo do SKU foi atualizado.

Nº 37
0 pares
INDISPONÍVEL
Atualizado: 26/08/2026 14:36
```

---

# 13. Ação — Não tinha

## 13.1 Pré-condição

Um SKU deve estar selecionado.

## 13.2 Comportamento

Ao selecionar `Não tinha`:

1. A ação é vinculada ao SKU selecionado.
2. É criada uma ruptura associada ao SKU.
3. A ruptura é associada ao vendedor responsável pelo atendimento.
4. É registrada a data e hora do atendimento.
5. O saldo do SKU não é alterado.
6. Nenhuma movimentação de estoque é criada.
7. A ruptura não é criada automaticamente somente porque o saldo está zero.
8. O sistema confirma o registro.
9. O vendedor pode iniciar uma nova consulta.

Mensagem:

```text
Ruptura registrada.
```

---

# 14. Ação — Desistiu

A ação `Desistiu` não exige seleção de SKU.

Ao selecionar `Desistiu`:

* o estoque não é alterado;
* nenhuma movimentação é criada;
* nenhuma ruptura é criada;
* o atendimento atual é encerrado;
* o vendedor retorna para a tela de consulta;
* uma nova consulta pode ser iniciada imediatamente.

Mensagem:

```text
Atendimento encerrado. Faça uma nova consulta.
```

---

# 15. Continuidade do atendimento

O fluxo deve permitir a continuidade do atendimento após qualquer resultado.

## 15.1 Após Vendeu

```text
Venda registrada. Saldo atualizado.

[ Nova consulta ]
```

## 15.2 Após Não tinha

```text
Ruptura registrada.

[ Nova consulta ]
```

## 15.3 Após Desistiu

O sistema retorna diretamente para:

```text
Buscar modelo...
```

## 15.4 Sem registrar resultado

O vendedor pode sair da consulta atual e iniciar outra sem registrar `Vendeu`, `Não tinha` ou `Desistiu`.

```text
[ Nova consulta ]
```

Essa ação não altera estoque, não registra movimentação e não cria ruptura.

---

# 16. Mensagens e feedback

As mensagens devem ser curtas, claras e próximas ao contexto da ação.

### Busca inválida

```text
Digite pelo menos 2 caracteres para buscar.
```

### Nenhum modelo encontrado

```text
Nenhum modelo encontrado. Tente outro nome.
```

### Venda realizada

```text
Venda registrada. Saldo atualizado.
```

### Venda rejeitada

```text
Não foi possível registrar a venda: saldo indisponível.
```

### Saldo atualizado por concorrência

```text
Venda não registrada.
O saldo do SKU foi atualizado.
```

### Ruptura registrada

```text
Ruptura registrada.
```

### Desistência

```text
Atendimento encerrado. Faça uma nova consulta.
```

### Erro

```text
Não foi possível concluir a ação. Consulte novamente o saldo do SKU.
```

Os componentes Bootstrap `alert`, `toast` ou feedback inline podem ser utilizados conforme o padrão visual existente no projeto.

---

# 17. Teclado, foco e acessibilidade

A interface deve permitir navegação por teclado e manter foco visível.

Ordem lógica:

```text
Busca
  ↓
Resultados
  ↓
Grade
  ↓
SKU selecionado
  ↓
Ações
  ↓
Nova consulta
```

Regras:

* `Tab` deve percorrer os elementos interativos em ordem lógica.
* `Enter` e `Space` devem ativar controles acionáveis quando aplicável.
* O foco deve permanecer visível.
* Células selecionáveis devem possuir identificação acessível.
* O estado selecionado deve ser perceptível visual e semanticamente.
* Botões não devem depender exclusivamente de ícones.
* Ícones devem possuir texto visível ou identificação acessível.
* O foco não deve ser perdido de forma inesperada após uma ação.

---

# 18. Alvos de toque

Para uso em smartphone:

* controles principais devem possuir alvo de aproximadamente **44 × 44 px ou maior**;
* deve existir espaçamento suficiente entre ações;
* ações não devem depender de pequenos ícones;
* células de SKU interativas devem possuir área de toque confortável;
* não devem ser exigidos gestos complexos para consultar o estoque.

---

# 19. Responsividade

## 19.1 Mobile

A prioridade visual deve ser:

1. Busca;
2. Resultados;
3. Modelo selecionado;
4. Grade;
5. SKU selecionado;
6. Ações;
7. Nova consulta.

Exemplo:

```text
┌───────────────────────────┐
│ SQUAD                     │
├───────────────────────────┤
│ 🔎 Buscar modelo...       │
├───────────────────────────┤
│ Tênis Runner Pro          │
│ Grade de numerações       │
│                           │
│ ┌───────┐ ┌───────┐       │
│ │ 35    │ │ 36 ✓  │       │
│ │ 3     │ │ 2     │       │
│ │ DISP. │ │ DISP. │       │
│ │ Atual.│ │ Atual.│       │
│ └───────┘ └───────┘       │
│                           │
│ SKU selecionado: Nº 36   │
│                           │
│ [✓ Vendeu]                │
│ [! Não tinha]             │
│ [× Desistiu]              │
│                           │
│ [Nova consulta]           │
└───────────────────────────┘
```

No mobile, os componentes devem ser organizados verticalmente e ocupar a largura disponível.

Para a grade de SKUs, recomenda-se utilizar `col-6`, apresentando duas células por linha no smartphone.

## 19.2 Desktop/tablet

Em telas maiores, os resultados e a grade podem ocupar áreas distintas.

```text
┌────────────────────────────────────────────────────────────┐
│ SQUAD                                      Vendedor         │
├────────────────────────────────────────────────────────────┤
│ 🔎 Buscar modelo...                            [Buscar]    │
├──────────────────────────┬─────────────────────────────────┤
│ Resultados               │ Grade — Runner Pro              │
│                          │                                 │
│ ┌──────────────────────┐ │ 35       36       37       38  │
│ │ Tênis Runner Pro     │ │ 3        2        1        0   │
│ │ Nike • Preto         │ │ DISP.    DISP.    ÚLT.     INDIS│
│ └──────────────────────┘ │ Atual.   Atual.   Atual.   Atual│
│                          │                                 │
│                          │ SKU selecionado: Nº 36         │
│                          │                                 │
│                          │ [Vendeu] [Não tinha] [Desistiu]│
│                          │                                 │
│                          │ [Nova consulta]                │
└──────────────────────────┴─────────────────────────────────┘
```

Em tablet e desktop, a grade pode utilizar o espaço horizontal disponível. Recomenda-se `col-md-4` para três células por linha em tablets e `col-lg-3` para quatro células por linha em desktops. Em telas menores, os demais componentes devem ser empilhados.

---

# 20. Estados gerais da interface

## 20.1 Carregando

Exibir indicador de carregamento no local do conteúdo.

O contexto da busca deve permanecer identificável.

## 20.2 Sem busca

```text
Digite o nome de um modelo para consultar o estoque.
```

## 20.3 Menos de dois caracteres

```text
Digite pelo menos 2 caracteres para buscar.
```

Nenhuma requisição deve ser enviada ao servidor.

## 20.4 Sem resultados

```text
Nenhum modelo encontrado. Tente outro nome.
```

## 20.5 Modelo sem grade

```text
Este modelo ainda não possui numerações cadastradas.
```

## 20.6 Erro de consulta

```text
Não foi possível consultar o estoque. Tente novamente.
```

## 20.7 Erro de ação

```text
Não foi possível concluir a ação. Consulte novamente o saldo do SKU.
```

---

# 21. Atualização do saldo

A grade deve apresentar sempre o saldo vigente conhecido para cada SKU.

Após uma venda:

```text
Saldo 2
   ↓
Vendeu
   ↓
Saldo 1
   ↓
Último par
```

Após uma segunda venda:

```text
Saldo 1
   ↓
Vendeu
   ↓
Saldo 0
   ↓
Indisponível
```

Uma tentativa de venda com saldo zero deve ser rejeitada.

Em situações de concorrência, a célula correspondente deve ser atualizada com o saldo vigente retornado pelo sistema.

A última atualização deve ser apresentada no formato de data e hora, sem definir nesta especificação a estratégia de persistência desse dado.

---

# 22. Implementação MVC

A implementação deve seguir a arquitetura **ASP.NET Core MVC** existente no projeto.

## 22.1 Controllers

Os Controllers devem:

* receber as requisições;
* tratar as operações da tela;
* acessar o `EstoqueContext` conforme a estrutura existente;
* processar as operações de consulta e atendimento;
* retornar os dados necessários para as Views.

## 22.2 ViewModels

Os ViewModels devem representar os dados específicos da tela, incluindo:

* resultado da busca;
* modelo selecionado;
* lista de SKUs;
* numeração;
* saldo;
* estado de disponibilidade;
* última atualização;
* SKU selecionado;
* mensagens e estados da interface quando necessários.

## 22.3 Razor Views

As Razor Views devem cuidar da apresentação:

* campo de busca;
* lista de resultados;
* grade;
* estados de estoque;
* seleção do SKU;
* ações do vendedor;
* mensagens;
* estados de carregamento;
* continuidade para nova consulta.

## 22.4 Bootstrap

Bootstrap deve fornecer os componentes responsivos e utilitários necessários, incluindo:

* `container`;
* `row`;
* `col-*`;
* `form-control`;
* `input-group`;
* `btn`;
* `card`;
* `alert`;
* `toast`;
* utilitários de espaçamento e responsividade.

Não devem ser introduzidos:

* React;
* Vue;
* Angular;
* Tailwind;
* SPA;
* Repository Pattern;
* MediatR;
* novas camadas arquiteturais.

A tela `/Produtos/Details` pertence ao fluxo administrativo do lojista e não deve ser utilizada como fluxo operacional do vendedor.

---

# 23. Rastreabilidade

| Requisito/Item | Relação com a especificação                                                                             |
| -------------- | ------------------------------------------------------------------------------------------------------- |
| RF-10          | Registro de movimentação de saída com usuário e data/hora                                               |
| RF-11          | Impedimento de saldo negativo                                                                           |
| RF-12          | Exibição da última atualização do saldo                                                                 |
| RF-13          | Busca por nome ou parte do nome do modelo                                                               |
| RF-14          | Exibição da grade completa e saldo por numeração                                                        |
| RF-15          | Estados Disponível, Último par e Indisponível                                                           |
| RF-16          | Registro dos resultados Vendeu, Não tinha e Desistiu                                                    |
| RF-17          | Decremento de exatamente uma unidade em Vendeu                                                          |
| RF-18          | Registro de ruptura vinculada ao SKU selecionado                                                        |
| RF-19          | Desistiu sem alteração de estoque                                                                       |
| RF-20          | Resultado opcional e possibilidade de nova consulta                                                     |
| RN-02          | Saldo não pode ser negativo                                                                             |
| RN-05          | Ruptura depende de declaração explícita do vendedor                                                     |
| RN-06          | Ruptura vinculada ao SKU e ao vendedor                                                                  |
| RN-07          | Controle da operação de saída em cenário de concorrência                                                |
| RNF-02         | Resposta da aplicação abaixo de 500 ms em P95 e resultado visível em até 3 segundos considerando a rede |
| RNF-06         | Primeira consulta em até dois toques a partir do login                                                  |
| RNF-07         | Interface responsiva                                                                                    |
| US-02          | Busca de modelo com mínimo de caracteres                                                                |
| US-03          | Visualização da grade e disponibilidade                                                                 |
| US-04          | Registro de Vendeu sobre o SKU selecionado                                                              |
| US-05          | Registro de Não tinha sobre o SKU selecionado                                                          |
| US-06          | Registro de Desistiu                                                                                    |
| US-07          | Resultado opcional e continuidade sem registro                                                         |
| VEN-01         | Início do vendedor com acesso imediato à consulta                                                       |
| VEN-02         | Busca de modelos e apresentação dos resultados                                                         |
| VEN-03         | Visualização do saldo e última atualização                                                              |
| VEN-04         | Resultado Vendeu, com decremento e atualização do saldo                                                 |
| VEN-05         | Resultado Não tinha, com registro explícito de ruptura                                                  |
| VEN-06         | Resultado Desistiu e retorno à consulta                                                                 |
| UC-02          | Consultar Estoque                                                                                       |
| UC-03          | Visualizar Grade                                                                                        |
| UC-04          | Registrar Vendeu                                                                                        |
| UC-05          | Registrar Não tinha                                                                                     |
| UC-06          | Registrar Desistiu                                                                                      |

---

# 24. Critérios de revisão

* [x] Campo de busca definido.
* [x] Busca restrita ao nome ou parte do nome do modelo.
* [x] Placeholder definido como `Buscar modelo...`.
* [x] Mínimo de 2 caracteres definido.
* [x] Nenhuma chamada ao servidor antes de 2 caracteres.
* [x] Lista de resultados definida.
* [x] Grade completa de numerações definida.
* [x] Saldo por SKU definido.
* [x] Última atualização do saldo definida.
* [x] Estado normal da célula definido.
* [x] Estado selecionado da célula definido.
* [x] Seleção explícita do SKU definida.
* [x] SKU selecionado identificado claramente.
* [x] `Vendeu` bloqueado sem SKU selecionado.
* [x] `Não tinha` bloqueado sem SKU selecionado.
* [x] `Desistiu` disponível sem seleção de SKU.
* [x] Ações vinculadas ao SKU selecionado.
* [x] Estados Disponível, Último par e Indisponível definidos.
* [x] Teclado e foco definidos.
* [x] Alvos de toque definidos.
* [x] Mensagens de validação, sucesso e erro definidas.
* [x] Fluxo de dois toques explicado.
* [x] Comportamento de Vendeu definido.
* [x] Decremento de exatamente uma unidade definido.
* [x] Registro de movimentação definido.
* [x] Usuário e data/hora da movimentação definidos.
* [x] Rejeição de venda com saldo zero definida.
* [x] Tratamento de concorrência e atualização do saldo vigente definido.
* [x] Comportamento de Não tinha definido.
* [x] Ruptura vinculada ao SKU e vendedor definida.
* [x] Data/hora da ruptura definida.
* [x] Saldo não alterado em Não tinha.
* [x] Saldo zero não gera ruptura automaticamente.
* [x] Comportamento de Desistiu definido.
* [x] Retorno à consulta após Desistiu definido.
* [x] Continuidade após Vendeu definida.
* [x] Continuidade após Não tinha definida.
* [x] Nova consulta sem registrar resultado definida.
* [x] RNF-02 incluído.
* [x] RNF-06 incluído e explicado.
* [x] Rastreabilidade completa.
* [x] ASP.NET Core MVC definido como arquitetura.
* [x] Controllers, ViewModels e Razor Views definidos.
* [x] Bootstrap definido como base visual.
* [x] Novos frameworks frontend não utilizados.
* [x] Fluxo administrativo de `/Produtos/Details` separado do fluxo operacional do vendedor.
* [x] Exemplos mobile e desktop apresentados.

---

## 25. Fontes internas

- [SRS / Documento de Requisitos](../02-requisitos/srs.md) — requisitos funcionais, regras de negócio e requisitos não funcionais relacionados à consulta, estoque, grade, movimentações e rupturas.

- [Casos de Uso](../02-requisitos/casos-de-uso.md) — fluxos UC-02, UC-03, UC-04, UC-05 e UC-06 relacionados à consulta, visualização da grade e ações do vendedor.

- [User Stories](../02-requisitos/user-stories.md) — histórias US-02 a US-07 relacionadas à busca, visualização da grade e ações do vendedor.

- [Arquitetura](../04-arquitetura/arquitetura.md) — definição da arquitetura ASP.NET Core MVC, Controllers, ViewModels, Razor Views e Bootstrap.

- [Inventário de Telas e Mapa de Navegação](inventario-telas-e-mapa-navegacao.md) — telas, fluxos de navegação e comportamentos previstos para o módulo do vendedor.

- [Mapa de Navegação MVP](mapa-navegacao-mvp.svg) — representação visual dos fluxos de navegação do MVP.
