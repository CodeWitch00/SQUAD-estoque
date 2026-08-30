# Especificação de testes do vendedor — consulta, venda e ruptura

## 1. Identificação

| Campo | Valor |
|---|---|
| Projeto | SQUAD Estoque |
| Versão do documento | 1.0 |
| Área | QA |
| Escopo | Consulta de estoque e resultados `Vendeu`, `Não tinha` e `Desistiu` |
| Perfil principal | `VENDEDOR` |
| Requisitos | RF-02, RF-03, RF-10 a RF-20, RNF-01, RNF-02, RNF-06, RNF-07 e RN-02, RN-05 a RN-07 |
| Casos de uso | UC-02 a UC-06 e UC-S2 a UC-S5 |
| Status | Especificado; execução pendente da implementação do módulo do vendedor |

## 2. Objetivo e fontes

Especificar os cenários positivos, negativos, de permissão, concorrência e continuidade do atendimento do vendedor, mantendo rastreabilidade entre requisitos, pré-condições, passos, entradas, resultados esperados e evidências.

Fontes:

- [SRS](../02-requisitos/srs.md): RF-02, RF-03, RF-10 a RF-20, RNF-01, RNF-02, RNF-06, RNF-07 e RN-02, RN-05 a RN-07;
- [casos de uso](../02-requisitos/casos-de-uso.md): UC-02 a UC-06 e UC-S2 a UC-S5;
- [regras do domínio](../01-negocio/dominio.md): RN-02, RN-05, RN-06 e RN-07;
- [inventário de telas e navegação](../05-ux/inventario-telas-e-mapa-navegacao.md): VEN-01 a VEN-06;
- [plano de testes](plano-de-testes.md): UT-06 a UT-10, IT-18 a IT-29 e E2E-03 a E2E-05;
- [testes não funcionais](especificacao-testes-nao-funcionais.md): NFT-MOB-01 a NFT-MOB-05 e NFT-PERF-01 a NFT-PERF-03.

## 3. Estado da implementação e limites

Na baseline atual, as telas e endpoints VEN-02 a VEN-06 ainda estão pendentes. Produto, SKU, saldo, Movimentação e Ruptura existem no domínio, e o teste automatizado `Ruptura_can_be_persisted_without_changing_balance` comprova apenas a persistência isolada de uma ruptura. Ele não comprova consulta, venda, autorização ou registro de ruptura pelo fluxo HTTP do vendedor.

Por isso:

- todos os casos deste documento permanecem com status de execução `Não executado`;
- casos dependentes do endpoint do vendedor não podem ser aprovados pela rota administrativa `/Movimentacoes/Saida`;
- `Vendeu` deve decrementar exatamente uma unidade do SKU consultado, enquanto a saída administrativa aceita quantidade variável e constitui outro fluxo;
- as rotas definitivas devem substituir os marcadores desta especificação quando o módulo for implementado, sem alterar os resultados de negócio esperados;
- um caso só pode receber `Aprovado` ou `Reprovado` após execução real com commit, ambiente, executor e evidência registrados.

### 3.1 Divergência documental conhecida

O UC-05 informa como pré-condição “Produto não encontrado”, mas o mesmo fluxo exige selecionar a numeração e criar uma ruptura vinculada a um SKU. Esta especificação adota o contrato consistente com RF-18, RN-06 e UC-S3: produto e grade consultados, com SKU específico selecionado. A redação do UC-05 deve ser corrigida na fonte.

## 4. Estratégia

| Camada | Uso nesta especificação | Forma prevista |
|---|---|---|
| Unitário | Tamanho mínimo da busca, classificação do saldo e opções de resultado | Automatizado |
| Integração de domínio/persistência | Atomicidade da venda, movimentação, ruptura e integridade referencial | Automatizado |
| Integração HTTP | Autenticação, autorização, validações, respostas e efeitos no banco | Automatizado |
| E2E | Jornadas consulta-venda, consulta-ruptura e desistência-continuidade | Automatizado com Playwright quando a interface estabilizar |
| Não funcional | Responsividade, usabilidade e desempenho da consulta | Automatizado e manual, conforme a especificação não funcional |

Para operações de escrita, verificar sempre o estado antes e depois. Em rejeições, confirmar que saldo, movimentações e rupturas permaneceram inalterados. A venda concorrente deve usar banco e isolamento compatíveis com produção; SQLite em memória não é evidência suficiente da RN-07.

## 5. Ambiente e massa de teste

| Alias | Dados controlados |
|---|---|
| USR-VEN-01 | Vendedor ativo e autenticável |
| USR-VEN-02 | Segundo vendedor ativo para concorrência |
| USR-LOJ-01 | Lojista ativo para verificações de perfil |
| PRD-01 | Tênis Runner, ativo, com grade 37 a 40 |
| PRD-02 | Sandália Verão, ativa, usada para busca parcial com mais de um resultado |
| SKU-00 | PRD-01, numeração 37, saldo 0 — `Indisponível` |
| SKU-01 | PRD-01, numeração 38, saldo 1 — `Último par` |
| SKU-02 | PRD-01, numeração 39, saldo 2 — `Disponível` |
| SKU-05 | PRD-01, numeração 40, saldo 5 — `Disponível` |

Cada teste automatizado deve criar sua própria massa, controlar data/hora e não depender da ordem de execução. Evidências não podem conter senha, hash completo ou cookie de autenticação.

## 6. Casos de teste — consulta e grade

### VEN-CON-01 — Buscar produto por parte do modelo

- **Rastreabilidade:** RF-13, UC-02; IT-19.
- **Forma de execução:** Automatizado.
- **Nível:** Integração HTTP.
- **Situação:** Dependente do módulo de consulta.
- **Pré-condição:** USR-VEN-01 autenticado; PRD-01 e PRD-02 ativos.
- **Passos:**
  1. Abrir a consulta do vendedor.
  2. Informar parte do nome de um modelo existente.
  3. Acionar a busca.
- **Entrada:** termo `Runner` e, em execução separada, termo parcial que retorne mais de um produto.
- **Saída esperada:** somente produtos ativos correspondentes são exibidos; cada resultado pode ser selecionado; nenhuma alteração de estoque, movimentação ou ruptura ocorre.
- **Resultado obtido:** Não executado.
- **Status de execução:** Não executado.
- **Evidência:** Não gerada.

### VEN-CON-02 — Não buscar com menos de dois caracteres

- **Rastreabilidade:** RF-13, UC-02; UT-07.
- **Forma de execução:** Automatizado.
- **Nível:** Unitário e integração HTTP.
- **Situação:** Dependente do módulo de consulta.
- **Pré-condição:** USR-VEN-01 autenticado; tela de consulta aberta.
- **Passos:** informar cada variação no campo de busca e tentar acionar a consulta.
- **Entrada:** texto vazio, um espaço e `T`; usar `Te` como valor de fronteira válido.
- **Saída esperada:** entradas com menos de dois caracteres úteis não disparam consulta e apresentam orientação clara; `Te` permite a busca; nenhum dado é alterado.
- **Resultado obtido:** Não executado.
- **Status de execução:** Não executado.
- **Evidência:** Não gerada.

### VEN-CON-03 — Informar produto não encontrado

- **Rastreabilidade:** RF-13, UC-02; IT-20.
- **Forma de execução:** Automatizado.
- **Nível:** Integração HTTP.
- **Situação:** Dependente do módulo de consulta.
- **Pré-condição:** USR-VEN-01 autenticado; nenhum produto corresponde ao termo.
- **Passos:** buscar um termo inexistente.
- **Entrada:** `Modelo Inexistente 999`.
- **Saída esperada:** lista vazia e mensagem `Produto não encontrado`; a tela continua disponível para nova consulta; nenhuma alteração é persistida.
- **Resultado obtido:** Não executado.
- **Status de execução:** Não executado.
- **Evidência:** Não gerada.

### VEN-CON-04 — Exibir grade completa e estados de saldo

- **Rastreabilidade:** RF-12, RF-14, RF-15, UC-03; UT-06, IT-21 e IT-22.
- **Forma de execução:** Automatizado e Manual assistido.
- **Nível:** Integração HTTP e E2E.
- **Situação:** Dependente do módulo de consulta; RF-12 também depende do registro da última atualização.
- **Pré-condição:** USR-VEN-01 autenticado; PRD-01 selecionado; SKU-00, SKU-01, SKU-02 e SKU-05 existentes.
- **Passos:**
  1. Buscar e selecionar PRD-01.
  2. Conferir todas as numerações, saldos, estados e horários exibidos.
- **Entrada:** grade 37/saldo 0, 38/saldo 1, 39/saldo 2 e 40/saldo 5.
- **Saída esperada:** todas as numerações aparecem uma única vez, com saldo atual e última atualização; saldo 0 é `Indisponível`, saldo 1 é `Último par` e saldo maior que 1 é `Disponível`; a diferença visual não depende apenas de cor.
- **Resultado obtido:** Não executado.
- **Status de execução:** Não executado.
- **Evidência:** Não gerada.

### VEN-CON-05 — Restringir consulta ao perfil vendedor autenticado

- **Rastreabilidade:** RF-02, RF-03 e proteção do fluxo do vendedor.
- **Forma de execução:** Automatizado.
- **Nível:** Integração HTTP.
- **Situação:** Dependente da definição da rota.
- **Pré-condição:** rota da consulta implementada.
- **Passos:** acessar diretamente a rota como anônimo, como USR-LOJ-01 e como USR-VEN-01.
- **Entrada:** requisições sem cookie e com sessões válidas de `LOJISTA` e `VENDEDOR`.
- **Saída esperada:** anônimo é redirecionado ao login; lojista recebe acesso negado; nenhum dos dois recebe dados da consulta; vendedor autenticado acessa o fluxo e mantém a sessão entre buscas sucessivas.
- **Resultado obtido:** Não executado.
- **Status de execução:** Não executado.
- **Evidência:** Não gerada.

## 7. Casos de teste — resultado Vendeu

### VEN-VEN-01 — Registrar venda de exatamente uma unidade

- **Rastreabilidade:** RF-10, RF-16, RF-17, UC-04, UC-S2, UC-S5; IT-23 e IT-24.
- **Forma de execução:** Automatizado.
- **Nível:** Integração HTTP e persistência.
- **Situação:** Dependente do módulo do vendedor.
- **Pré-condição:** USR-VEN-01 autenticado; SKU-05 consultado com saldo 5.
- **Passos:**
  1. Selecionar SKU-05 na grade.
  2. Escolher `Vendeu` e confirmar.
  3. Reconsultar o SKU e inspecionar a movimentação persistida.
- **Entrada:** SKU-05 e ação `Vendeu`; a operação não recebe quantidade variável.
- **Saída esperada:** saldo passa de 5 para 4; exatamente uma movimentação `SAIDA` de quantidade 1 é registrada com SKU, USR-VEN-01 e data/hora; confirmação e saldo atualizado são exibidos; nenhuma ruptura é criada.
- **Resultado obtido:** Não executado.
- **Status de execução:** Não executado.
- **Evidência:** Não gerada.

### VEN-VEN-02 — Rejeitar venda sem saldo

- **Rastreabilidade:** RF-11, RF-17, RN-02, UC-04, UC-S2, UC-S4; IT-25.
- **Forma de execução:** Automatizado.
- **Nível:** Integração HTTP e persistência.
- **Situação:** Dependente do módulo do vendedor.
- **Pré-condição:** USR-VEN-01 autenticado; SKU-00 consultado com saldo 0.
- **Passos:** selecionar SKU-00 e confirmar `Vendeu`.
- **Entrada:** SKU-00 e ação `Vendeu`.
- **Saída esperada:** operação rejeitada com mensagem clara e saldo atualizado; saldo permanece 0; nenhuma movimentação ou ruptura é criada.
- **Resultado obtido:** Não executado.
- **Status de execução:** Não executado.
- **Evidência:** Não gerada.

### VEN-VEN-03 — Rejeitar venda sem SKU válido

- **Rastreabilidade:** RF-17, RN-02, UC-04 e integridade da operação.
- **Forma de execução:** Automatizado.
- **Nível:** Integração HTTP.
- **Situação:** Dependente do endpoint de venda.
- **Pré-condição:** USR-VEN-01 autenticado.
- **Passos:** enviar a confirmação sem SKU e repeti-la com UUID inexistente ou SKU inativo.
- **Entrada:** `sku_id` ausente, UUID inexistente e SKU inativo.
- **Saída esperada:** todas as variações são rejeitadas; nenhum saldo é alterado; nenhuma movimentação ou ruptura é criada; a resposta não revela detalhes internos.
- **Resultado obtido:** Não executado.
- **Status de execução:** Não executado.
- **Evidência:** Não gerada.

### VEN-VEN-04 — Garantir atomicidade na venda concorrente do último par

- **Rastreabilidade:** RF-11, RF-17, RN-02, RN-07, UC-S2; IT-18.
- **Forma de execução:** Automatizado.
- **Nível:** Integração concorrente em banco compatível com produção.
- **Situação:** Planejado prioritário; controle explícito de concorrência ainda não implementado.
- **Pré-condição:** SKU-01 com saldo 1; USR-VEN-01 e USR-VEN-02 em sessões independentes; barreira de sincronização preparada.
- **Passos:**
  1. Fazer as duas sessões consultarem o mesmo saldo 1.
  2. Liberar simultaneamente duas confirmações `Vendeu` para SKU-01.
  3. Aguardar ambas e consultar saldo, movimentações e respostas.
- **Entrada:** duas vendas concorrentes de uma unidade para o mesmo SKU.
- **Saída esperada:** exatamente uma venda é confirmada e a outra é rejeitada; saldo final 0; exatamente uma movimentação `SAIDA` de quantidade 1; nunca há saldo negativo ou sucesso duplicado.
- **Resultado obtido:** Não executado.
- **Status de execução:** Não executado.
- **Evidência:** Não gerada.

## 8. Casos de teste — resultado Não tinha e ruptura

### VEN-RUP-01 — Registrar ruptura explícita sem alterar saldo

- **Rastreabilidade:** RF-16, RF-18, RN-05, RN-06, UC-05, UC-S3; IT-23 e IT-26.
- **Forma de execução:** Automatizado.
- **Nível:** Integração HTTP e persistência.
- **Situação:** Parcial; persistência isolada existente, fluxo do vendedor dependente.
- **Pré-condição:** USR-VEN-01 autenticado; SKU-05 consultado com saldo 5.
- **Passos:**
  1. Selecionar SKU-05 na grade.
  2. Escolher `Não tinha` e confirmar.
  3. Consultar saldo, rupturas e movimentações.
- **Entrada:** SKU-05 e ação explícita `Não tinha`.
- **Saída esperada:** exatamente uma Ruptura é criada com SKU-05, USR-VEN-01 e data/hora; saldo permanece 5; nenhuma movimentação é criada; confirmação é exibida.
- **Resultado obtido:** Não executado.
- **Status de execução:** Não executado.
- **Evidência:** Não gerada.

### VEN-RUP-02 — Rejeitar ruptura sem SKU válido

- **Rastreabilidade:** RN-06, UC-05, UC-S3; IT-27.
- **Forma de execução:** Automatizado.
- **Nível:** Integração HTTP e persistência.
- **Situação:** Dependente do endpoint de ruptura.
- **Pré-condição:** USR-VEN-01 autenticado.
- **Passos:** enviar `Não tinha` sem SKU e repetir com UUID inexistente ou SKU inativo.
- **Entrada:** `sku_id` ausente, UUID inexistente e SKU inativo.
- **Saída esperada:** requisições rejeitadas com mensagem clara; nenhuma Ruptura ou movimentação é criada; nenhum saldo é alterado.
- **Resultado obtido:** Não executado.
- **Status de execução:** Não executado.
- **Evidência:** Não gerada.

### VEN-RUP-03 — Não criar ruptura automaticamente para saldo zero

- **Rastreabilidade:** RN-05.
- **Forma de execução:** Automatizado.
- **Nível:** Integração HTTP e persistência.
- **Situação:** Dependente do módulo de consulta.
- **Pré-condição:** SKU-00 com saldo 0 e sem rupturas; USR-VEN-01 autenticado.
- **Passos:** consultar SKU-00, visualizar seu estado e iniciar nova consulta sem escolher `Não tinha`.
- **Entrada:** consulta de SKU com saldo zero sem registro de resultado.
- **Saída esperada:** estado `Indisponível` é exibido, mas nenhuma Ruptura é criada; saldo permanece 0; nova consulta é permitida.
- **Resultado obtido:** Não executado.
- **Status de execução:** Não executado.
- **Evidência:** Não gerada.

### VEN-RUP-04 — Registrar declarações distintas de ruptura

- **Rastreabilidade:** RF-18, RF-22, RN-05, RN-06.
- **Forma de execução:** Automatizado.
- **Nível:** Integração HTTP e persistência.
- **Situação:** Dependente do módulo do vendedor.
- **Pré-condição:** SKU-00 existente; duas sessões ou atendimentos independentes e autenticados.
- **Passos:** registrar `Não tinha` para o mesmo SKU em dois atendimentos distintos.
- **Entrada:** duas declarações explícitas e válidas para SKU-00.
- **Saída esperada:** duas Rupturas independentes são persistidas, cada uma com responsável e data/hora; o saldo não muda; a futura visão do lojista pode contabilizar duas ocorrências.
- **Resultado obtido:** Não executado.
- **Status de execução:** Não executado.
- **Evidência:** Não gerada.

## 9. Casos de teste — continuidade do atendimento

### VEN-PER-01 — Bloquear venda e ruptura fora do perfil vendedor

- **Rastreabilidade:** RF-02 e proteção dos fluxos UC-04 e UC-05.
- **Forma de execução:** Automatizado.
- **Nível:** Integração HTTP.
- **Situação:** Dependente dos endpoints de resultado.
- **Pré-condição:** SKU-05 com saldo 5; endpoints de `Vendeu` e `Não tinha` implementados; token antiforgery válido para isolar a autorização.
- **Passos:** repetir as confirmações de `Vendeu` e `Não tinha` primeiro como anônimo e depois como USR-LOJ-01.
- **Entrada:** SKU-05, cada ação de resultado e usuários sem o perfil `VENDEDOR`.
- **Saída esperada:** anônimo é redirecionado ao login; lojista recebe acesso negado; saldo permanece 5; nenhuma movimentação ou ruptura é criada.
- **Resultado obtido:** Não executado.
- **Status de execução:** Não executado.
- **Evidência:** Não gerada.

### VEN-FLX-01 — Oferecer somente os três resultados previstos

- **Rastreabilidade:** RF-16; UT-08 e IT-23.
- **Forma de execução:** Automatizado e Manual assistido.
- **Nível:** Unitário e E2E.
- **Situação:** Dependente da interface do vendedor.
- **Pré-condição:** USR-VEN-01 autenticado; grade consultada.
- **Passos:** selecionar um SKU e inspecionar as ações de resultado disponíveis.
- **Entrada:** SKU válido selecionado.
- **Saída esperada:** somente `Vendeu`, `Não tinha` e `Desistiu` são apresentadas como resultados; as ações são distinguíveis, legíveis e não confundem `Vendeu` com saída administrativa.
- **Resultado obtido:** Não executado.
- **Status de execução:** Não executado.
- **Evidência:** Não gerada.

### VEN-FLX-02 — Desistir sem persistir efeitos

- **Rastreabilidade:** RF-19, RF-20, UC-06; UT-10 e IT-28.
- **Forma de execução:** Automatizado.
- **Nível:** Integração HTTP e E2E.
- **Situação:** Dependente do módulo do vendedor.
- **Pré-condição:** USR-VEN-01 autenticado; PRD-01 e um SKU consultados.
- **Passos:** escolher `Desistiu` e iniciar outra consulta.
- **Entrada:** atendimento em andamento e ação `Desistiu`.
- **Saída esperada:** nenhuma movimentação ou ruptura é criada; nenhum saldo muda; o vendedor retorna à busca e consegue consultar novamente.
- **Resultado obtido:** Não executado.
- **Status de execução:** Não executado.
- **Evidência:** Não gerada.

### VEN-FLX-03 — Iniciar nova consulta sem registrar resultado

- **Rastreabilidade:** RF-20; IT-29.
- **Forma de execução:** Automatizado e Manual assistido.
- **Nível:** Integração HTTP e E2E.
- **Situação:** Dependente do módulo do vendedor.
- **Pré-condição:** USR-VEN-01 autenticado; grade de PRD-01 aberta, sem resultado registrado.
- **Passos:** abandonar a grade ou usar a ação de nova consulta e buscar PRD-02.
- **Entrada:** transição de uma consulta sem resultado para outra consulta.
- **Saída esperada:** a nova consulta ocorre sem bloqueio ou confirmação obrigatória; nenhum saldo, movimentação ou ruptura do atendimento anterior é alterado.
- **Resultado obtido:** Não executado.
- **Status de execução:** Não executado.
- **Evidência:** Não gerada.

## 10. Validações não funcionais relacionadas

Não duplicar os procedimentos detalhados na especificação não funcional. Associar às jornadas deste documento:

| Área | Casos relacionados | Aplicação |
|---|---|---|
| Browser mobile | NFT-MOB-01 e NFT-MOB-02 | Executar consulta-venda e consulta-ruptura em Chrome Android e Safari iOS |
| Responsividade | NFT-MOB-03 e NFT-MOB-04 | Validar busca, grade, ações, confirmações e erros nos três viewports definidos |
| Usabilidade | NFT-MOB-05 | Primeira consulta em até dois toques após login, sem treinamento |
| Desempenho | NFT-PERF-01 a NFT-PERF-03 | P95 da consulta menor que 500 ms e resultado visível em até 3 s |

Esses casos permanecem `Não executado` até que ambiente, massa, ferramenta e evidência sejam registrados.

## 11. Jornadas E2E prioritárias

| ID | Jornada | Casos detalhados | Resultado esperado | Situação |
|---|---|---|---|---|
| E2E-03 | Consulta e venda | VEN-CON-01, VEN-CON-04, VEN-VEN-01 | Grade correta, venda unitária, auditoria e saldo atualizado | Dependente |
| E2E-04 | Consulta e ruptura | VEN-CON-01, VEN-CON-04, VEN-RUP-01 | Ruptura auditável e saldo inalterado | Dependente |
| E2E-05 | Desistência e continuidade | VEN-FLX-02 e VEN-FLX-03 | Nenhum efeito persistido e nova consulta disponível | Dependente |

## 12. Matriz de rastreabilidade

| Requisito/regra | Casos principais |
|---|---|
| RF-02 e RF-03 | VEN-CON-05 e VEN-PER-01 |
| RF-10 e RF-11 | VEN-VEN-01, VEN-VEN-02 e VEN-VEN-04 |
| RF-12 | VEN-CON-04 |
| RF-13 | VEN-CON-01 a VEN-CON-03 |
| RF-14 e RF-15 | VEN-CON-04 |
| RF-16 | VEN-VEN-01, VEN-RUP-01 e VEN-FLX-01 |
| RF-17 | VEN-VEN-01 a VEN-VEN-04 |
| RF-18 | VEN-RUP-01, VEN-RUP-02 e VEN-RUP-04 |
| RF-19 | VEN-FLX-02 |
| RF-20 | VEN-RUP-03, VEN-FLX-02 e VEN-FLX-03 |
| RN-02 | VEN-VEN-02 a VEN-VEN-04 |
| RN-05 | VEN-RUP-01, VEN-RUP-03 e VEN-RUP-04 |
| RN-06 | VEN-RUP-01, VEN-RUP-02 e VEN-RUP-04 |
| RN-07 | VEN-VEN-04 |
| UC-02 e UC-03 | VEN-CON-01 a VEN-CON-04 |
| UC-04 e UC-S2 | VEN-VEN-01 a VEN-VEN-04 |
| UC-05 e UC-S3 | VEN-RUP-01 e VEN-RUP-02 |
| UC-06 | VEN-FLX-02 |
| RNF-01, RNF-02, RNF-06 e RNF-07 | Seção 10 |

## 13. Referência à automação existente

| Teste automatizado | Cobertura válida | Limite |
|---|---|---|
| `Ruptura_can_be_persisted_without_changing_balance` | Parte de VEN-RUP-01: entidade persiste e saldo não muda | Não cobre ação HTTP, autenticação, responsável correto, feedback ou ausência de movimentação |

Nenhum teste automatizado atual cobre VEN-CON-01 a VEN-CON-05, VEN-VEN-01 a VEN-VEN-04, VEN-PER-01 ou VEN-FLX-01 a VEN-FLX-03. A rota `/Movimentacoes/Saida` não deve ser listada como automação de `Vendeu`.

## 14. Critérios de aceite

- Todos os casos P0/P1 aplicáveis estão aprovados por execução real e possuem evidência.
- A venda decrementa exatamente uma unidade, registra auditoria completa e nunca produz saldo negativo.
- VEN-VEN-04 prova atomicidade em banco compatível com produção.
- A ruptura só nasce de `Não tinha`, sempre referencia SKU e vendedor e nunca altera saldo.
- Consulta, venda, ruptura e continuidade funcionam no browser mobile e atendem aos limites não funcionais.
- Rejeições não deixam alterações parciais.
- A saída administrativa não é usada como evidência do fluxo `Vendeu`.
- Nenhum caso `Dependente`, `Planejado` ou `Não executado` é contabilizado como aprovado.

## 15. Registro de revisões

| Versão | Data | Tipo | Revisor | Resultado registrado |
|---|---|---|---|---|
| 1.0 | 2026-08-30 | Técnica | Codex, em revisão assistida solicitada pela equipe | Rastreabilidade, regras de saldo, ruptura e concorrência conferidas; divergência do UC-05 registrada |
| 1.0 | 2026-08-30 | Textual | Codex, em revisão assistida solicitada pela equipe | Terminologia de consulta, `Vendeu`, `Não tinha`, ruptura, situação e execução uniformizada |

Revisões assistidas não substituem o aceite do responsável de QA ou do produto quando exigido para release.
