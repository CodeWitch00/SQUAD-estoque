# S1-QA-007 - Especificação de testes do vendedor

> Consulta, venda, ruptura e continuidade do atendimento.

## Identificação

| Campo | Valor |
|---|---|
| Projeto | SQUAD Estoque |
| Cartão | S1-QA-007 |
| Área | QA |
| Escopo | Busca curta e inexistente; grade e estados; venda e saldo zero; ruptura; desistência; fluxo não bloqueante; concorrência do último par |
| Rastreabilidade funcional | RF-13 a RF-20 |
| Casos de teste | 15 |
| Dependências | 9 |
| Situação | Planejado, sem aprovação antecipada |

Este documento integra o [plano oficial de testes](plano-de-testes.md) do SQUAD Estoque. As situações refletem a baseline inspecionada e devem ser atualizadas somente com evidência de execução.

## 1. Objetivo e rastreabilidade

Esta especificação define antecipadamente os casos que comprovarão o Módulo Operacional do Vendedor nas próximas Sprints. Ela complementa o plano de testes com entradas, resultados esperados, dependências e níveis de teste para consulta, venda, ruptura e continuidade do atendimento.

| Artefato de origem | Itens cobertos |
|---|---|
| [SRS](../02-requisitos/srs.md) | RF-13 a RF-20 e RN-02, RN-05 a RN-07 |
| [Casos de uso](../02-requisitos/casos-de-uso.md) | UC-02 a UC-06 |
| [Histórias de usuário](../02-requisitos/user-stories.md) | US-02 a US-07 |
| [Inventário de telas](../05-ux/inventario-telas-e-mapa-navegacao.md) | VEN-01 a VEN-06 |
| [Decisões de interface](../05-ux/decisoes/decisoes-de-interface.md) | DI-02 a DI-09 |

Nenhum caso deste documento deve ser marcado como **Aprovado** antes de uma execução registrada contra a aplicação implementada. Protótipo navegável, implementação de infraestrutura e teste isolado de persistência não aprovam uma jornada completa.

## 2. Estado da baseline

Na baseline inspecionada, Produto, SKU, Movimentação e Ruptura já existem no modelo, e há testes automatizados para saída de estoque e persistência isolada de Ruptura. Isso fornece infraestrutura, mas não implementa o atendimento operacional do vendedor.

| Área | Evidência atual | Consequência para esta especificação |
|---|---|---|
| Produto e grade | Entidades `Produto` e `Sku`, com saldo não negativo no SQLite | A massa dos casos pode usar dados reais do modelo atual. |
| Saída genérica | `/Movimentacoes/Saida` aceita `LOJISTA` e `VENDEDOR`, quantidade livre e não exige consulta anterior | Não substitui **Vendeu**, que exige SKU consultado e decremento fixo de 1. |
| Ruptura | Entidade, relacionamentos obrigatórios e teste de persistência sem alterar saldo | Falta a ação HTTP do vendedor; o fluxo continua planejado. |
| Consulta e resultados | Não há controller nem Razor View de VEN-01 a VEN-06 | Casos funcionais do vendedor dependem de implementação. |
| UX | Existe [protótipo navegável](../05-ux/fluxos_UX/vendedor/fluxo-vendedor.html) | Serve para alinhar estados e textos, não como evidência de execução da aplicação. |
| Concorrência | A saída usa transação, mas não existe teste de duas vendas simultâneas do último par | RN-07 permanece planejada e prioritária. |

## 3. Massa de teste e condições

Usar banco SQLite isolado e recriado por execução. Requisições de escrita devem usar vendedor autenticado e token antiforgery válido.

| ID | Dado | Valores |
|---|---|---|
| USR-VEN-01 | Vendedor autenticado | `vendedor@squad.com`, perfil `VENDEDOR` |
| USR-VEN-02 | Segundo vendedor | Usuário distinto, perfil `VENDEDOR` |
| PRD-VEN-01 | Produto pesquisável | Modelo Tênis Urban, ativo |
| PRD-VEN-02 | Produto não correspondente | Modelo Sandália Luna, ativo |
| SKU-VEN-00 | Indisponível | PRD-VEN-01, numeração 36, saldo 0 |
| SKU-VEN-01 | Último par | PRD-VEN-01, numeração 37, saldo 1 |
| SKU-VEN-02 | Disponível no limite | PRD-VEN-01, numeração 38, saldo 2 |
| SKU-VEN-05 | Disponível | PRD-VEN-01, numeração 39, saldo 5 |

Em cada operação, registrar o saldo e as contagens de Movimentação e Ruptura antes e depois. Datas devem ser controladas ou comparadas dentro de uma janela conhecida; os testes não devem depender de espera artificial.

## 4. Casos de teste

Legenda:

- **Planejado:** comportamento especificado que ainda precisa ser executado;
- **Dependente:** a execução aguarda uma implementação identificada na seção 6;
- **Parcial:** existe uma prova de infraestrutura, mas não do fluxo completo.

### 4.1 Consulta e grade

#### VEN-CONS-01

- **Requisitos:** RF-13, UC-02.
- **Entradas e procedimento:** autenticar USR-VEN-01 e buscar `Ur`, parte do nome de PRD-VEN-01 com 2 caracteres.
- **Resultado esperado:** PRD-VEN-01 é retornado em até 10 segundos e PRD-VEN-02 não é incluído indevidamente; a resposta não é tela de erro. A medição de P95 permanece nos testes não funcionais.
- **Nível:** integração HTTP.
- **Situação:** planejado, dependente de D-VEN-01 e D-VEN-02.

#### VEN-CONS-02

- **Requisitos:** RF-13, UC-02.
- **Entradas e procedimento:** tentar buscar com `""` e `U`, contendo 0 e 1 caractere após remover espaços.
- **Resultado esperado:** a busca não é disparada; mensagem pede pelo menos 2 caracteres; nenhuma consulta ao estoque ou alteração de dados ocorre. O servidor também rejeita entrada curta se chamado diretamente.
- **Nível:** unitário e integração HTTP.
- **Situação:** planejado, dependente de D-VEN-02.

#### VEN-CONS-03

- **Requisitos:** RF-13, UC-02.
- **Entradas e procedimento:** buscar `Modelo Inexistente`, termo com 2 ou mais caracteres sem correspondência.
- **Resultado esperado:** lista vazia e mensagem clara de produto não encontrado; resposta bem-sucedida, sem Movimentação ou Ruptura.
- **Nível:** integração HTTP.
- **Situação:** planejado, dependente de D-VEN-01 e D-VEN-02.

#### VEN-CONS-04

- **Requisitos:** RF-14, UC-03.
- **Entradas e procedimento:** buscar e selecionar PRD-VEN-01.
- **Resultado esperado:** a grade exibe exatamente as numerações 36, 37, 38 e 39, com saldos 0, 1, 2 e 5; nenhum SKU ativo do produto é omitido ou duplicado.
- **Nível:** integração HTTP.
- **Situação:** planejado, dependente de D-VEN-02 e D-VEN-03.

#### VEN-CONS-05

- **Requisitos:** RF-15, UC-03.
- **Entradas e procedimento:** exibir a grade de PRD-VEN-01.
- **Resultado esperado:** SKU-VEN-00 mostra **Indisponível**; SKU-VEN-01, **Último par**; SKU-VEN-02 e SKU-VEN-05, **Disponível**. Cada estado possui texto, não apenas cor.
- **Nível:** unitário e integração/visual.
- **Situação:** planejado, dependente de D-VEN-03.

### 4.2 Resultado do atendimento e venda

#### VEN-ATD-01

- **Requisitos:** RF-16.
- **Entradas e procedimento:** consultar PRD-VEN-01 e selecionar SKU-VEN-02.
- **Resultado esperado:** são oferecidas somente as ações contextuais **Vendeu**, **Não tinha** e **Desistiu**, além da possibilidade separada de iniciar outra consulta sem registrar resultado.
- **Nível:** integração/visual.
- **Situação:** planejado, dependente de D-VEN-03 e D-VEN-04.

#### VEN-VENDA-01

- **Requisitos:** RF-17, RN-02, UC-04.
- **Entradas e procedimento:** USR-VEN-01 seleciona SKU-VEN-02, saldo 2, e confirma **Vendeu** uma vez.
- **Resultado esperado:** saldo passa para 1; é criada exatamente uma Movimentação `SAIDA`, quantidade 1, vinculada a USR-VEN-01 e com data/hora; nenhuma Ruptura é criada; confirmação mostra o saldo atualizado.
- **Nível:** integração HTTP.
- **Situação:** planejado, dependente de D-VEN-04 e D-VEN-05.

#### VEN-VENDA-02

- **Requisitos:** RF-15, RF-17, RN-02, UC-04.
- **Entradas e procedimento:** USR-VEN-01 seleciona SKU-VEN-01, saldo 1, e confirma **Vendeu**.
- **Resultado esperado:** saldo passa para 0, nunca para valor negativo; existe uma única saída de quantidade 1; a grade atualizada classifica o SKU como **Indisponível**.
- **Nível:** integração HTTP.
- **Situação:** planejado, dependente de D-VEN-03 a D-VEN-05.

#### VEN-VENDA-03

- **Requisitos:** RF-17, RN-02, UC-04.
- **Entradas e procedimento:** tentar **Vendeu** para SKU-VEN-00, cujo saldo já é 0.
- **Resultado esperado:** operação rejeitada com mensagem clara; saldo permanece 0; nenhuma Movimentação ou Ruptura é criada; a grade apresenta o saldo atual.
- **Nível:** integração HTTP.
- **Situação:** planejado, dependente de D-VEN-04 e D-VEN-05.

### 4.3 Ruptura, desistência e continuidade

#### VEN-RUP-01

- **Requisitos:** RF-18, RN-05, RN-06, UC-05.
- **Entradas e procedimento:** USR-VEN-01 consulta PRD-VEN-01, seleciona SKU-VEN-02 e confirma **Não tinha**.
- **Resultado esperado:** é criada exatamente uma Ruptura com `SkuId` de SKU-VEN-02, `UsuarioId` de USR-VEN-01 e data/hora; saldo permanece 2; nenhuma Movimentação é criada.
- **Nível:** integração HTTP.
- **Situação:** parcial, pois a persistência isolada existe; o fluxo depende de D-VEN-04 e D-VEN-06.

#### VEN-RUP-02

- **Requisitos:** RF-18, RN-06, UC-05.
- **Entradas e procedimento:** enviar **Não tinha** sem `SkuId` e, separadamente, com um identificador inexistente.
- **Resultado esperado:** as duas entradas são rejeitadas; nenhuma Ruptura ou Movimentação é criada; nenhum saldo é alterado.
- **Nível:** integração HTTP e persistência.
- **Situação:** planejado, dependente de D-VEN-06.

#### VEN-RUP-03

- **Requisitos:** RN-05.
- **Entradas e procedimento:** consultar SKU-VEN-00, saldo 0, e iniciar nova consulta sem escolher **Não tinha**; depois repetir e escolher explicitamente **Não tinha**.
- **Resultado esperado:** na primeira execução não surge Ruptura automática. Na segunda, surge exatamente uma Ruptura por ação explícita, e o saldo continua 0.
- **Nível:** integração HTTP.
- **Situação:** planejado, dependente de D-VEN-04 e D-VEN-06.

#### VEN-DES-01

- **Requisitos:** RF-19, UC-06.
- **Entradas e procedimento:** após consultar e selecionar SKU-VEN-02, escolher **Desistiu**.
- **Resultado esperado:** nenhuma Movimentação ou Ruptura é criada, o saldo permanece 2 e o vendedor retorna à consulta.
- **Nível:** integração HTTP/E2E.
- **Situação:** planejado, dependente de D-VEN-04 e D-VEN-07.

#### VEN-FLUXO-01

- **Requisitos:** RF-20.
- **Entradas e procedimento:** após consultar e selecionar SKU-VEN-02, usar **Pular registro** e fazer nova consulta.
- **Resultado esperado:** nova consulta é permitida imediatamente; saldo e contagens de Movimentação e Ruptura permanecem inalterados.
- **Nível:** integração HTTP/E2E.
- **Situação:** planejado, dependente de D-VEN-04 e D-VEN-07.

### 4.4 Concorrência do último par

#### VEN-CONC-01

- **Requisitos:** RF-17, RN-02, RN-07, UC-04.
- **Entradas e procedimento:** preparar SKU-VEN-01 com saldo 1. Sincronizar duas confirmações de **Vendeu**, uma por USR-VEN-01 e outra por USR-VEN-02, usando requisições e contextos de banco separados, sem `Thread.Sleep`.
- **Resultado esperado:** exatamente uma venda é confirmada e a outra recebe conflito/saldo indisponível; saldo final 0; exatamente uma Movimentação `SAIDA` de quantidade 1; nenhuma atualização parcial ou saldo negativo.
- **Nível:** integração concorrente.
- **Situação:** planejado prioritário, dependente de D-VEN-05 e D-VEN-08.

## 5. Correspondência com o plano mestre

Os IDs abaixo evitam que a futura automação replique testes equivalentes já catalogados no plano geral.

| Casos desta especificação | Catálogo do plano mestre |
|---|---|
| VEN-CONS-01 a VEN-CONS-03 | UT-07, IT-19 e IT-20 |
| VEN-CONS-04 e VEN-CONS-05 | UT-06, IT-21 e IT-22 |
| VEN-ATD-01 | UT-08 e IT-23 |
| VEN-VENDA-01 a VEN-VENDA-03 | UT-09, IT-24 e IT-25 |
| VEN-RUP-01 e VEN-RUP-02 | IT-26 e IT-27 |
| VEN-RUP-03 | IT-26 e verificação específica de RN-05 |
| VEN-DES-01 e VEN-FLUXO-01 | UT-10, IT-28 e IT-29 |
| VEN-CONC-01 | IT-18 |
| Jornadas completas | E2E-03 a E2E-05 |

## 6. Dependências de implementação

| ID | Dependência | Casos bloqueados ou afetados |
|---|---|---|
| D-VEN-01 | Criar início/rota de consulta autenticada para `VENDEDOR`, substituindo a Home provisória no fluxo do perfil. | VEN-CONS-01, VEN-CONS-03 e jornadas E2E. |
| D-VEN-02 | Implementar consulta por parte do nome do modelo, validação de mínimo de 2 caracteres e estado sem resultado no servidor e na interface. | VEN-CONS-01 a VEN-CONS-04. |
| D-VEN-03 | Criar ViewModel/Razor View da grade completa e classificação testável dos saldos 0, 1 e maior que 1. | VEN-CONS-04, VEN-CONS-05, VEN-ATD-01 e VEN-VENDA-02. |
| D-VEN-04 | Implementar ações contextuais do SKU consultado para **Vendeu**, **Não tinha**, **Desistiu** e pular resultado, com autorização e antiforgery. | VEN-ATD-01 e todos os casos de resultado. |
| D-VEN-05 | Implementar **Vendeu** como decremento fixo de 1 com validação e garantia atômica contra atualização concorrente. A transação genérica atual, sem teste concorrente, não comprova RN-07. | VEN-VENDA-01 a VEN-VENDA-03 e VEN-CONC-01. |
| D-VEN-06 | Implementar criação de Ruptura pelo fluxo do vendedor, exigindo SKU existente e preservando o saldo. | VEN-RUP-01 a VEN-RUP-03. |
| D-VEN-07 | Implementar retorno à consulta sem persistência para **Desistiu** e para pular o registro. | VEN-DES-01 e VEN-FLUXO-01. |
| D-VEN-08 | Disponibilizar fixture determinística de concorrência com SQLite e dois contextos/requisições independentes. | VEN-CONC-01. |
| D-VEN-09 | Restringir a saída administrativa genérica ao `LOJISTA` quando a ação **Vendeu** estiver disponível ao vendedor. | Controle de acesso e prevenção de fluxo alternativo indevido. |

## 7. Divergências e decisões pendentes

1. O [README](../../README.md) menciona consulta por modelo, marca, categoria ou cor, enquanto RF-13, UC-02, US-02 e o protótipo definem busca pelo nome do modelo. Estes casos usam somente o nome do modelo até que o SRS seja alterado por decisão do produto.
2. A pré-condição de UC-05 diz "Produto não encontrado", mas o próprio fluxo exige selecionar uma numeração e RN-06 exige SKU. Os casos interpretam **Não tinha** como ausência física percebida após consultar a grade, nunca como busca sem resultado no sistema.
3. A decisão entre confirmação explícita e feedback com possibilidade de desfazer ainda está pendente em UX. Ela pode alterar passos visuais, mas não pode alterar os efeitos persistidos definidos nos casos.
4. A rota `/Movimentacoes/Saida` é administrativa e aceita quantidade livre; ela não deve ser usada como evidência de aprovação de **Vendeu**.

## 8. Evidências e critério para atualizar a situação

Para cada execução, registrar commit, ambiente, massa, executor, resultado observado e evidência sem dados sensíveis. Casos concorrentes também devem registrar o método de sincronização e os dois resultados individuais.

Um caso só pode mudar para **Aprovado** quando:

- a dependência correspondente estiver implementada;
- a execução tiver ocorrido contra a aplicação, não apenas contra o protótipo;
- saldo, Movimentação e Ruptura tiverem sido verificados antes e depois;
- o resultado corresponder integralmente ao esperado;
- a evidência estiver vinculada ao commit testado.

Até essas condições serem atendidas, manter a situação como **Planejado**, **Dependente** ou **Parcial**, mesmo que a tela esteja prototipada.
