# Especificação de testes administrativos — produtos e movimentações

## 1. Identificação

| Campo | Valor |
|---|---|
| Projeto | SQUAD Estoque |
| Cartão | S1-QA-006 |
| Área | QA |
| Escopo | Produtos, grade, entrada, saída administrativa, ajuste e histórico |
| Perfis | `LOJISTA`, `VENDEDOR` e usuário anônimo |
| Status | Em revisão |

## 2. Objetivo e fontes

Especificar entradas, ações e resultados esperados dos fluxos administrativos já implementados, mantendo rastreabilidade com:

- [SRS](../02-requisitos/srs.md): RF-05 a RF-11 e RF-23;
- [casos de uso](../02-requisitos/casos-de-uso.md): UC-07 a UC-10 e UC-S1, UC-S4 e UC-S5;
- [regras do domínio](../01-negocio/dominio.md): RN-01 a RN-04;
- [plano de testes](plano-de-testes.md): UT-01 a UT-05, UT-11, UT-12, IT-01 a IT-18 e E2E-02/E2E-06;
- implementação MVC e testes automatizados existentes em `tests/SquadEstoque.Web.Tests`.

Esta é uma especificação de comportamento, não uma duplicação dos testes xUnit.

## 3. Estratégia

Conforme o plano de testes, cada comportamento deve ser validado no nível mais baixo que forneça confiança suficiente:

| Nível | Uso nesta especificação |
|---|---|
| Unitário | Validações de ViewModels e cálculos isoláveis |
| Integração de domínio/persistência | Controllers, EF Core, transações, constraints e efeitos no banco |
| Integração HTTP | Formulários MVC, antiforgery, cookie, autorização, status e redirecionamentos |
| E2E | Jornada crítica de cadastro, grade e entrada; executar quando a interface estiver estável |

Nos testes HTTP, desabilitar redirecionamento automático quando o destino fizer parte do aceite. Operações `POST` devem enviar cookie autenticado e token antiforgery válido. Em qualquer rejeição, verificar a resposta e a ausência de alteração parcial no banco.

### 3.1 Convenções de resposta MVC

O projeto não expõe uma API REST administrativa. Portanto:

- formulário válido normalmente responde `302 Found` para a próxima tela;
- formulário inválido normalmente responde `200 OK`, reapresentando a View com erros;
- usuário anônimo em rota protegida recebe `302` para `/Account/Login?ReturnUrl=...`;
- perfil autenticado sem permissão recebe `302` para `/Account/AccessDenied?ReturnUrl=...`;
- recurso inexistente pode responder `404 Not Found` no `GET`; em alguns `POST`, o controller reapresenta a View com erro e não persiste dados.

## 4. Massa de teste

Os nomes abaixo são aliases legíveis; os IDs efetivamente persistidos devem ser UUIDs válidos e distintos.

| Alias | Dados |
|---|---|
| USR-LOJ-01 | `lojista@squad.com`, perfil `LOJISTA` |
| USR-VEN-01 | `vendedor@squad.com`, perfil `VENDEDOR` |
| PRD-01 | Tênis Runner, ativo |
| PRD-02 | Sandália Verão, ativo |
| SKU-01 | PRD-01, numeração 39, saldo 5, ativo |
| SKU-02 | PRD-01, numeração 40, saldo 0, ativo |
| SKU-03 | PRD-01, numeração 41, saldo 1, ativo |

Cada teste deve criar sua própria massa em SQLite em memória e não depender da ordem de execução de outros casos.

## 5. Casos de teste — produto e grade

### ADM-PROD-01 — Cadastrar produto e grade válidos

- **Rastreabilidade:** RF-05, RF-06, RF-07, UC-07, UC-08, UC-S1; IT-01 e IT-04.
- **Nível:** integração HTTP.
- **Pré-condição:** USR-LOJ-01 autenticado.
- **Entrada:** `POST /Produtos/Create` com `Nome=Sandália Verão`, `Marca=Passo Leve`, `Categoria=Sandália`, `Cor=Bege`, `NumeracoesGrade=35,36,37` e antiforgery válido.
- **Resultado esperado:** `302 Found` para `/Produtos`; um Produto ativo com UUID é persistido; três SKUs ativos, com UUIDs distintos e saldo zero, ficam vinculados ao Produto; o item aparece na listagem.
- **Situação:** planejado. `Produto_requires_identification_fields` cobre somente obrigatoriedade dos campos da entidade.

### ADM-PROD-02 — Rejeitar campo obrigatório ausente

- **Rastreabilidade:** RF-05, UC-07; UT-01 e IT-02.
- **Nível:** unitário e integração HTTP.
- **Entrada:** mesmo formulário de ADM-PROD-01, mas com `Nome` vazio.
- **Resultado esperado:** `200 OK`; View de cadastro reapresentada com erro em `Nome`; nenhum Produto ou SKU é persistido.
- **Situação:** parcial — `Produto_requires_identification_fields` cobre o modelo; falta o fluxo HTTP e a ausência de persistência.

### ADM-PROD-03 — Rejeitar grade vazia

- **Rastreabilidade:** RF-06, UC-08; UT-03 e IT-02.
- **Nível:** integração HTTP.
- **Entrada:** produto válido com `NumeracoesGrade` vazia ou composta apenas por espaços.
- **Resultado esperado:** `200 OK`; mensagem de obrigatoriedade da grade; nenhum Produto ou SKU é persistido.
- **Situação:** planejado.

### ADM-PROD-04 — Rejeitar numeração repetida no mesmo formulário

- **Rastreabilidade:** RF-08, RN-01, UC-08; UT-03.
- **Nível:** integração HTTP.
- **Entrada:** produto válido com `NumeracoesGrade=38,39,38` e, em outra execução, variação de maiúsculas/minúsculas quando aplicável à numeração textual.
- **Resultado esperado:** `200 OK`; mensagem identifica a numeração repetida; nenhum Produto ou SKU é persistido.
- **Situação:** planejado.

### ADM-PROD-05 — Impedir SKU duplicado no banco

- **Rastreabilidade:** RF-08, RN-01, UC-S1; IT-05.
- **Nível:** integração de persistência.
- **Entrada:** dois SKUs com o mesmo `ProdutoId` e `Numeracao=38` no mesmo contexto.
- **Resultado esperado:** `SaveChangesAsync` lança `DbUpdateException`; nenhum duplicado permanece persistido, comprovando a constraint única `(ProdutoId, Numeracao)`.
- **Situação:** **existente:** `Sku_with_same_product_and_numeracao_cannot_be_persisted_twice`.

### ADM-PROD-06 — Aceitar a mesma numeração em produtos diferentes

- **Rastreabilidade:** RF-06, RF-08, RN-01; IT-06.
- **Nível:** integração de persistência.
- **Entrada:** PRD-01 e PRD-02, cada um com um SKU de numeração 39.
- **Resultado esperado:** ambos são persistidos com UUIDs diferentes; a unicidade não é aplicada à numeração isolada.
- **Situação:** planejado.

### ADM-PROD-07 — Bloquear acesso indevido ao cadastro

- **Rastreabilidade:** RF-02 e proteção administrativa; IT-03.
- **Nível:** integração HTTP.
- **Entrada:** `GET` e `POST /Produtos/Create` primeiro como USR-VEN-01 e depois como anônimo.
- **Resultado esperado:** vendedor recebe `302` para AccessDenied; anônimo recebe `302` para Login; nenhum Produto ou SKU é persistido.
- **Situação:** parcial — o controller é restrito a `LOJISTA` e o acesso de vendedor a `/Produtos` já é automatizado; falta afirmar especificamente a escrita.

## 6. Casos de teste — entrada de estoque

### ADM-ENT-01 — Registrar entrada válida

- **Rastreabilidade:** RF-09, RF-10, UC-09, UC-S5; IT-07 e IT-09.
- **Nível:** integração de controller/persistência.
- **Pré-condição:** SKU-01 com saldo 5; USR-LOJ-01 autenticado.
- **Entrada:** `SkuId` de SKU-01, `Quantidade=10`, `Motivo=Reposição`.
- **Resultado esperado:** redirecionamento para o detalhe do produto; saldo final 15; exatamente uma movimentação `ENTRADA`, quantidade 10, SKU correto, usuário lojista, data/hora preenchida e motivo `Reposição`.
- **Situação:** parcial — `Entrada_registers_movement_and_increases_balance` cobre saldo, tipo e quantidade; faltam as asserções completas de auditoria.

### ADM-ENT-02 — Rejeitar quantidade zero ou negativa

- **Rastreabilidade:** RF-09; UT-04 e IT-08.
- **Nível:** validação do ViewModel e integração HTTP.
- **Entrada:** repetir com `Quantidade=0` e `Quantidade=-3`.
- **Resultado esperado:** View reapresentada com erro de quantidade; saldo permanece 5; nenhuma movimentação é criada.
- **Situação:** planejado.

### ADM-ENT-03 — Rejeitar SKU inexistente ou produto inativo

- **Rastreabilidade:** RF-09 e integridade da operação.
- **Nível:** integração de controller/persistência.
- **Entrada:** UUID de SKU inexistente; repetir com SKU vinculado a Produto inativo.
- **Resultado esperado:** View reapresentada com mensagem clara; saldo e histórico não sofrem alteração.
- **Situação:** planejado.

## 7. Casos de teste — saída administrativa

Nesta seção, “saída” significa a operação administrativa de quantidade variável. Ela não substitui o resultado `Vendeu`, que pertence ao fluxo operacional do vendedor e decrementa exatamente um par.

### ADM-SAI-01 — Registrar saída com saldo suficiente

- **Rastreabilidade:** RF-10, RF-11, RN-02, UC-S5; IT-10.
- **Nível:** integração de controller/persistência.
- **Pré-condição:** SKU-01 com saldo 5; usuário lojista.
- **Entrada:** `SkuId` de SKU-01 e `Quantidade=2`.
- **Resultado esperado:** redirecionamento para o detalhe do produto; saldo final 3; exatamente uma movimentação `SAIDA`, quantidade 2, SKU e usuário corretos, com data/hora.
- **Situação:** parcial — `Saida_registers_movement_and_reduces_balance` cobre saldo, tipo e quantidade; faltam as asserções completas de auditoria.

### ADM-SAI-02 — Rejeitar saída com saldo insuficiente

- **Rastreabilidade:** RF-11, RN-02, UC-S4; IT-11.
- **Nível:** integração de controller/persistência.
- **Entrada:** SKU com saldo 1 e `Quantidade=2`.
- **Resultado esperado:** View reapresentada com mensagem de saldo insuficiente; saldo permanece 1; nenhuma movimentação é criada.
- **Situação:** **existente:** `Saida_with_insufficient_balance_is_rejected_by_controller`.

### ADM-SAI-03 — Impedir saldo negativo na persistência

- **Rastreabilidade:** RN-02, UC-S4; IT-12.
- **Nível:** integração de persistência.
- **Entrada:** tentativa direta de persistir SKU com `SaldoAtual=-1`.
- **Resultado esperado:** `DbUpdateException`; nenhum saldo negativo é persistido.
- **Situação:** **existente:** `Sku_with_negative_balance_cannot_be_persisted`.

### ADM-SAI-04 — Concorrência sobre o último item

- **Rastreabilidade:** RF-11, RN-02 e risco de concorrência; IT-18.
- **Nível:** integração concorrente em banco compatível com produção.
- **Entrada:** SKU-03 com saldo 1; duas sessões enviam saída de uma unidade simultaneamente.
- **Resultado esperado:** uma operação confirma e outra é rejeitada; saldo final zero; exatamente uma movimentação `SAIDA`; nunca há saldo negativo.
- **Situação:** **planejado prioritário**. A implementação atual não possui controle explícito de versão ou atualização condicional; este teste pode revelar defeito e não deve ser considerado previamente aprovado.

### ADM-SAI-05 — Corrigir permissão da saída genérica

- **Rastreabilidade:** RF-02 e separação de perfis definida na documentação UX.
- **Nível:** integração HTTP.
- **Entrada:** USR-VEN-01 solicita `GET` e `POST /Movimentacoes/Saida`.
- **Resultado esperado do fluxo-alvo:** acesso negado e nenhuma alteração no estoque.
- **Situação:** **divergência conhecida:** o controller atual permite `LOJISTA,VENDEDOR`. Enquanto isso não for corrigido, o teste do fluxo-alvo falhará. Não classificar a saída genérica como resultado `Vendeu`.

## 8. Casos de teste — ajuste manual

O contrato implementado recebe `SkuId`, `NovoSaldoApurado` e `Motivo`. A direção do ajuste é determinada comparando o saldo atual com o novo saldo; a movimentação guarda o valor absoluto da diferença.

### ADM-AJU-01 — Aumentar saldo com motivo

- **Rastreabilidade:** RF-10, RF-23, RN-04, UC-10, UC-S5; IT-15.
- **Nível:** integração de controller/persistência.
- **Entrada:** saldo atual 2, `NovoSaldoApurado=6`, `Motivo=Contagem física`.
- **Resultado esperado:** saldo final 6; movimentação `AJUSTE` com quantidade 4, motivo, SKU, usuário lojista e data/hora; redirecionamento ao detalhe do produto.
- **Situação:** parcial — `Ajuste_registers_movement_and_updates_balance` cobre saldo, tipo, quantidade e motivo; faltam usuário e data/hora.

### ADM-AJU-02 — Reduzir saldo com motivo

- **Rastreabilidade:** RF-10, RF-23, RN-02, UC-10; UT-12 e IT-15.
- **Nível:** integração de controller/persistência.
- **Entrada:** saldo atual 5, `NovoSaldoApurado=3`, motivo válido.
- **Resultado esperado:** saldo final 3; movimentação `AJUSTE` com quantidade 2 e dados de auditoria completos.
- **Situação:** planejado.

### ADM-AJU-03 — Rejeitar motivo vazio, curto ou excessivo

- **Rastreabilidade:** RF-23, UC-10; UT-11 e IT-16.
- **Nível:** validação do ViewModel, controller e integração HTTP.
- **Entrada:** repetir com motivo vazio, somente espaços, menos de 5 caracteres e mais de 500 caracteres.
- **Resultado esperado:** View reapresentada com erro; saldo e histórico inalterados.
- **Situação:** parcial — `Ajuste_without_reason_is_rejected_by_controller` cobre motivo vazio; demais limites estão planejados.

### ADM-AJU-04 — Rejeitar novo saldo negativo

- **Rastreabilidade:** RF-23, RN-02, UC-10; IT-17.
- **Nível:** unitário/controller.
- **Entrada:** saldo atual 5, `NovoSaldoApurado=-1`, motivo válido.
- **Resultado esperado:** View reapresentada com erro; saldo permanece 5; nenhuma movimentação é criada.
- **Situação:** planejado.

### ADM-AJU-05 — Rejeitar ajuste sem alteração

- **Rastreabilidade:** RF-23 e integridade do histórico.
- **Nível:** integração de controller/persistência.
- **Entrada:** saldo atual 5 e `NovoSaldoApurado=5`, motivo válido.
- **Resultado esperado:** View reapresentada informando que não há alteração; nenhuma movimentação redundante é criada.
- **Situação:** planejado.

### ADM-AJU-06 — Bloquear vendedor e anônimo

- **Rastreabilidade:** RN-04, UC-10; CT-AUT-14 e CT-AUT-15.
- **Nível:** integração HTTP.
- **Entrada:** `GET` e `POST /Movimentacoes/Ajuste` como USR-VEN-01 e como anônimo.
- **Resultado esperado:** vendedor recebe redirecionamento para AccessDenied; anônimo recebe redirecionamento para Login; saldo e histórico permanecem inalterados.
- **Situação:** planejado prioritário.

## 9. Casos de teste — histórico e imutabilidade

### ADM-HIS-01 — Consultar histórico completo

- **Rastreabilidade:** RF-10, RN-03; IT-09 e IT-14.
- **Nível:** integração HTTP.
- **Pré-condição:** movimentações de entrada, saída e ajuste vinculadas a SKUs e usuários.
- **Entrada:** USR-LOJ-01 envia `GET /Movimentacoes`.
- **Resultado esperado:** `200 OK`; lista global ordenada do registro mais recente para o mais antigo; cada linha apresenta data/hora, produto/SKU, tipo, quantidade, responsável e motivo quando aplicável.
- **Situação:** planejado. A rota atual não filtra por SKU.

### ADM-HIS-02 — Bloquear histórico para vendedor e anônimo

- **Rastreabilidade:** RF-02 e proteção administrativa.
- **Nível:** integração HTTP.
- **Entrada:** `GET /Movimentacoes` como USR-VEN-01 e como anônimo.
- **Resultado esperado:** vendedor recebe redirecionamento para AccessDenied; anônimo recebe redirecionamento para Login; nenhum conteúdo do histórico é retornado.
- **Situação:** parcial — acesso do lojista e redirecionamento anônimo possuem cobertura; falta o caso de vendedor.

### ADM-HIS-03 — Não permitir edição ou exclusão pública

- **Rastreabilidade:** RN-03; IT-14.
- **Nível:** inspeção de rotas e integração HTTP.
- **Entrada:** tentar localizar/acessar rotas públicas de edição e exclusão de movimentações usando um ID existente.
- **Resultado esperado:** não há actions públicas de `Edit`, `Delete`, `PUT` ou `DELETE`; o registro original permanece inalterado e continua no histórico.
- **Situação:** planejado. A imutabilidade é garantida pelo desenho da aplicação e pela ausência de endpoints; acesso administrativo direto ao banco não faz parte desse fluxo.

## 10. Controle transversal de segurança

### ADM-SEG-01 — Rejeitar POST sem antiforgery

- **Rastreabilidade:** estratégia de integração HTTP do plano de testes.
- **Nível:** integração HTTP.
- **Entrada:** repetir um `POST` de Produto, Entrada e Ajuste sem token antiforgery e com token inválido.
- **Resultado esperado:** requisição rejeitada antes da action; nenhum Produto, SKU, saldo ou movimentação é alterado.
- **Situação:** planejado.

## 11. Referência à automação existente

Não duplicar os testes abaixo; ampliar suas asserções quando a cobertura for parcial.

| Teste automatizado | Casos relacionados |
|---|---|
| `Produto_requires_identification_fields` | ADM-PROD-02 |
| `Sku_with_same_product_and_numeracao_cannot_be_persisted_twice` | ADM-PROD-05 |
| `Sku_with_negative_balance_cannot_be_persisted` | ADM-SAI-03 |
| `Entrada_registers_movement_and_increases_balance` | ADM-ENT-01 |
| `Saida_registers_movement_and_reduces_balance` | ADM-SAI-01 |
| `Saida_with_insufficient_balance_is_rejected_by_controller` | ADM-SAI-02 |
| `Ajuste_registers_movement_and_updates_balance` | ADM-AJU-01 |
| `Ajuste_without_reason_is_rejected_by_controller` | ADM-AJU-03 |
| `Lojista_can_login_and_access_produtos` | apoio a ADM-PROD-07 |
| `Lojista_can_access_movimentacoes` | apoio a ADM-HIS-01 |
| `Produtos_without_authentication_redirects_to_login` | apoio a ADM-PROD-07 |
| `Movimentacoes_without_authentication_redirects_to_login` | apoio a ADM-HIS-02 |

## 12. Matriz de rastreabilidade resumida

| Requisito/regra | Casos principais |
|---|---|
| RF-05 | ADM-PROD-01, ADM-PROD-02 |
| RF-06 e RF-07 | ADM-PROD-01, ADM-PROD-03, ADM-PROD-06 |
| RF-08 e RN-01 | ADM-PROD-04 a ADM-PROD-06 |
| RF-09 | ADM-ENT-01 a ADM-ENT-03 |
| RF-10 e UC-S5 | ADM-ENT-01, ADM-SAI-01, ADM-AJU-01, ADM-AJU-02, ADM-HIS-01 |
| RF-11 e RN-02 | ADM-SAI-02 a ADM-SAI-04, ADM-AJU-02, ADM-AJU-04 |
| RF-23 e UC-10 | ADM-AJU-01 a ADM-AJU-06 |
| RN-03 | ADM-HIS-01 e ADM-HIS-03 |
| RN-04 | ADM-AJU-01, ADM-AJU-02 e ADM-AJU-06 |
| UC-07 e UC-08 | ADM-PROD-01 a ADM-PROD-06 |
| UC-09 | ADM-ENT-01 a ADM-ENT-03 |

## 13. Critérios de aceite

- Todos os casos marcados **existente** continuam verdes na CI.
- Casos parciais recebem as asserções ausentes sem duplicar preparação desnecessariamente.
- ADM-SAI-04 e ADM-AJU-06 são P0, conforme a priorização de risco do plano de testes.
- Nenhum cenário rejeitado pode deixar saldo ou histórico parcialmente alterado.
- A saída genérica de `VENDEDOR` permanece registrada como divergência até a autorização ser alinhada ao fluxo-alvo; não declarar cobertura de `Vendeu` por essa rota.
- A especificação deve ser revista quando controllers, rotas ou requisitos mudarem.

## 14. Execuções registradas

| Data | Commit | Escopo | Resultado | Evidência |
|---|---|---|---|---|
| 2026-08-30 | `fbd95e1` | Baseline administrativa: login, produto/grade, entrada, saída, ajuste, histórico e perfis | **Reprovado com impedimento IMP-ADM-001** | [Relatório](relatorio-homologacao-baseline-administrativa-2026-08-30.md) e [roteiro executado](roteiro-executado-baseline-administrativa-2026-08-30.md) |
