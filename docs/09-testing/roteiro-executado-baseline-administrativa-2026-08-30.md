# Roteiro executado — baseline administrativa

## 1. Identificação

| Campo | Valor |
|---|---|
| Execução | Homologação manual assistida da baseline administrativa |
| Data | 2026-08-30 |
| Commit da aplicação | `fbd95e1` |
| Branch | `main` |
| Ambiente | ASP.NET Core Development, .NET 10.0.302 e SQLite exclusivo em `/tmp` |
| Perfis | Anônimo, `LOJISTA` e `VENDEDOR` |
| Massa principal | Produto `Homologacao ADM 20260830`; grade 37, 38 e 39 |
| Resultado geral | **REPROVADO — 13 aprovados e 1 reprovado** |
| Relatório completo | [Relatório de homologação](relatorio-homologacao-baseline-administrativa-2026-08-30.md) |

O roteiro abaixo registra o que foi realmente executado. Nenhum problema foi corrigido durante a execução. Cookies, tokens antiforgery e HTMLs brutos ficaram apenas na pasta temporária e não foram versionados.

## 2. Legenda das evidências

| Tipo | Conteúdo registrado |
|---|---|
| HTTP | Código de resposta e destino de redirecionamento |
| HTML | Mensagem ou conteúdo renderizado, sem token ou cookie |
| DB | Consulta sanitizada às tabelas Produto, Sku, Movimentacao e Usuario |
| Runner | Resultado do build ou da suíte xUnit |

## 3. Preparação executada

1. Compilar a aplicação com `dotnet build --no-restore`.
2. Criar bancos descartáveis de Estoque e legado em `/tmp`.
3. Subir a aplicação em `http://127.0.0.1:5199`, com seed demonstrativo habilitado.
4. Criar sessões independentes para LOJISTA, VENDEDOR e tentativa inválida.
5. Executar os cenários na ordem abaixo, preservando a massa entre os passos da jornada.
6. Executar a suíte xUnit ao final.
7. Encerrar a aplicação sem alterar o banco local ou o código da aplicação.

Evidência da preparação: build aprovado com zero erros; suíte xUnit aprovada com 22 testes passados, zero falhos e zero ignorados. O aviso NU1900 ocorreu porque a consulta de vulnerabilidades do NuGet estava indisponível e não impediu build ou execução.

## 4. Cenários executados

### 4.1 HOM-ADM-01 — Rejeitar login inválido

| Campo | Registro da execução |
|---|---|
| Passos | Abrir Login; enviar `lojista@squad.com` com senha incorreta e antiforgery válido |
| Esperado | `200 OK`, mensagem genérica e nenhuma autenticação |
| Obtido | `200 OK`; mensagem `E-mail ou senha inválidos.`; nenhum cookie de autenticação |
| Evidência | HTTP `200`; HTML com a mensagem; cookie jar continha somente antiforgery |
| Status | **APROVADO** |

### 4.2 HOM-ADM-02 — Autenticar os dois perfis e encerrar sessão

| Campo | Registro da execução |
|---|---|
| Passos | Autenticar LOJISTA e VENDEDOR em sessões separadas; fazer logout do VENDEDOR; reabrir Saída |
| Esperado | Logins válidos; logout invalida a sessão; rota protegida volta a pedir login |
| Obtido | Ambos os logins retornaram `302` para `/`; logout retornou `302` para Login; Saída após logout retornou `302` para Login com `ReturnUrl` |
| Evidência | Códigos e destinos HTTP das quatro operações |
| Status | **APROVADO** |

### 4.3 HOM-ADM-03 — Cadastrar produto e grade

| Campo | Registro da execução |
|---|---|
| Passos | Como LOJISTA, cadastrar `Homologacao ADM 20260830`, Marca QA, Tenis, Azul e grade `37,38,39` |
| Esperado | Um produto ativo; três SKUs únicos, ativos e com saldo inicial zero |
| Obtido | `302` para Produtos; produto `0169FD69-5CE0-46B2-9142-B17194BD7191`; três SKUs distintos com saldos 0 |
| Evidência | HTTP `302`; DB com `Skus=3`, `SaldoMin=0` e `SaldoMax=0`; detalhe MVC exibiu produto e grade |
| Status | **APROVADO** |

### 4.4 HOM-ADM-04 — Rejeitar grade duplicada

| Campo | Registro da execução |
|---|---|
| Passos | Como LOJISTA, tentar cadastrar outro produto com grade `38,39,38` |
| Esperado | Reapresentar formulário, indicar duplicidade e não persistir |
| Obtido | `200 OK`; mensagem de numeração 38 repetida; zero produtos persistidos com o nome da tentativa |
| Evidência | HTML da validação; DB `ProdutosPersistidos=0` |
| Status | **APROVADO** |

### 4.5 HOM-ADM-05 — Registrar entrada válida

| Campo | Registro da execução |
|---|---|
| Passos | Como LOJISTA, registrar entrada de 10 no SKU 37, motivo `Homologacao entrada` |
| Esperado | Saldo 10; uma ENTRADA de 10 com responsável, motivo e data |
| Obtido | `302` para detalhe; saldo 10; movimentação tipo ENTRADA, quantidade 10, `lojista@squad.com`, motivo e data |
| Evidência | HTTP `302`; junção DB de Sku, Movimentacao e Usuario |
| Status | **APROVADO** |

### 4.6 HOM-ADM-06 — Rejeitar entrada zero

| Campo | Registro da execução |
|---|---|
| Passos | Como LOJISTA, tentar entrada de quantidade 0 no SKU 38 |
| Esperado | Erro; saldo e histórico inalterados |
| Obtido | `200 OK`; mensagem de quantidade mínima 1; saldo 0; zero movimentações |
| Evidência | HTML da validação; DB `SaldoAtual=0`, `Movimentacoes=0` |
| Status | **APROVADO** |

### 4.7 HOM-ADM-07 — Registrar saída válida como LOJISTA

| Campo | Registro da execução |
|---|---|
| Passos | Como LOJISTA, retirar 3 do SKU 37, que possuía saldo 10 |
| Esperado | Saldo 7; uma SAÍDA de 3 com responsável, motivo e data |
| Obtido | `302` para detalhe; saldo 7; movimentação SAÍDA de 3 por `lojista@squad.com` |
| Evidência | HTTP `302`; DB com saldo e segunda movimentação da jornada |
| Status | **APROVADO** |

### 4.8 HOM-ADM-08 — Rejeitar saída com saldo insuficiente

| Campo | Registro da execução |
|---|---|
| Passos | Como LOJISTA, tentar retirar 99 do SKU 37, que possuía saldo 7 |
| Esperado | Mensagem clara; saldo 7; nenhuma movimentação adicional |
| Obtido | `200 OK`; mensagem informou saldo disponível 7; saldo permaneceu 7; movimentações permaneceram 2 |
| Evidência | HTML da validação; DB antes/depois com saldo 7 e contagem 2 |
| Status | **APROVADO** |

### 4.9 HOM-ADM-09 — Registrar ajuste válido

| Campo | Registro da execução |
|---|---|
| Passos | Como LOJISTA, ajustar o SKU 37 de 7 para 12 com motivo `Contagem homologacao` |
| Esperado | Saldo 12; AJUSTE de diferença 5 com responsável, motivo e data |
| Obtido | `302` para detalhe; saldo 12; movimentação AJUSTE de 5 por `lojista@squad.com` |
| Evidência | HTTP `302`; DB com saldo 12 e terceira movimentação |
| Status | **APROVADO** |

### 4.10 HOM-ADM-10 — Rejeitar ajuste com motivo curto

| Campo | Registro da execução |
|---|---|
| Passos | Como LOJISTA, tentar alterar saldo de 12 para 15 usando motivo `x` |
| Esperado | Erro de motivo; saldo 12; nenhuma movimentação adicional |
| Obtido | `200 OK`; mensagem de mínimo de 5 caracteres; saldo 12; movimentações permaneceram 3 |
| Evidência | HTML da validação; DB antes/depois com saldo 12 e contagem 3 |
| Status | **APROVADO** |

### 4.11 HOM-ADM-11 — Consultar histórico

| Campo | Registro da execução |
|---|---|
| Passos | Como LOJISTA, abrir Histórico após entrada, saída e ajuste |
| Esperado | Ordem decrescente; data, produto, SKU, tipo, quantidade, responsável e motivo |
| Obtido | `200 OK`; ordem AJUSTE, SAÍDA, ENTRADA; produto, tamanho 37, quantidades 5/3/10, Lojista Teste e motivos exibidos |
| Evidência | Tabela HTML e DB ordenado por `CriadoEm DESC` |
| Status | **APROVADO** |

### 4.12 HOM-ADM-12 — Bloquear usuário anônimo

| Campo | Registro da execução |
|---|---|
| Passos | Sem autenticação, abrir Produtos, Histórico, Entrada, Saída e Ajuste |
| Esperado | Todas redirecionam ao Login com `ReturnUrl` |
| Obtido | As cinco rotas retornaram `302` para Login com os destinos de retorno corretos |
| Evidência | Cabeçalhos `Location` das cinco respostas |
| Status | **APROVADO** |

### 4.13 HOM-ADM-13 — Validar rotas permitidas e proibidas por perfil

| Campo | Registro da execução |
|---|---|
| Passos | Com LOJISTA e VENDEDOR, abrir Produtos, Histórico, Entrada, Saída e Ajuste |
| Esperado | LOJISTA acessa os cinco fluxos; VENDEDOR é negado em Produtos, Histórico, Entrada e Ajuste |
| Obtido | LOJISTA recebeu `200` nas cinco rotas; VENDEDOR recebeu `302` para AccessDenied nas quatro rotas proibidas |
| Evidência | Matriz HTTP: LOJISTA `200/200/200/200/200`; VENDEDOR `302/302/302/200/302` |
| Status | **APROVADO COM RESSALVA** — o `200` de Saída é avaliado no cenário seguinte |

### 4.14 HOM-ADM-14 — Impedir saída administrativa pelo VENDEDOR

| Campo | Registro da execução |
|---|---|
| Passos | Como VENDEDOR, abrir Saída; retirar 1 do SKU 37, que possuía saldo 12; consultar banco |
| Esperado | GET e POST negados; saldo permanece 12; nenhuma movimentação adicional |
| Obtido | GET `200`; POST `302` para `/`; saldo caiu para 11; SAÍDA de 1 registrada por `vendedor@squad.com` |
| Evidência | HTTP GET `200`; HTTP POST `302`; DB `SaldoAtual=11`; quarta movimentação com motivo `Prova autorizacao indevida` |
| Status | **REPROVADO — IMP-ADM-001** |

## 5. Lista objetiva de confirmações da baseline

- Login inválido é rejeitado sem criar sessão.
- LOJISTA e VENDEDOR autenticam com seus perfis e o logout invalida a sessão.
- Usuário anônimo é redirecionado ao Login em todas as rotas administrativas verificadas.
- LOJISTA acessa Produtos, Histórico, Entrada, Saída e Ajuste.
- VENDEDOR é bloqueado em Produtos, Histórico, Entrada e Ajuste.
- Cadastro válido cria um produto e toda a grade com IDs únicos e saldo inicial zero.
- Numeração repetida na grade é rejeitada sem persistência parcial.
- Entrada válida atualiza saldo e registra quantidade, responsável, motivo e data.
- Entrada zero é rejeitada sem alterar saldo ou histórico.
- Saída válida do LOJISTA reduz o saldo e registra auditoria.
- Saída acima do saldo é rejeitada sem persistência parcial.
- Ajuste válido atualiza saldo e registra a diferença e o motivo.
- Ajuste com motivo curto é rejeitado sem persistência parcial.
- Histórico apresenta entrada, saída e ajuste em ordem decrescente com os dados previstos.
- Build aprovado e suíte automatizada com 22/22 testes aprovados.

## 6. Lista objetiva de falhas e impedimentos

### Falha confirmada

- **IMP-ADM-001:** VENDEDOR acessa e executa a saída administrativa. O teste comprovou alteração de saldo e criação de movimentação, embora o fluxo-alvo administrativo determine acesso exclusivo do LOJISTA.

### Divergência que exige decisão

- O manual de ambiente afirma que o VENDEDOR pode realizar saída, enquanto o inventário UX e a especificação administrativa separam `Vendeu` da saída genérica e reservam esta última ao LOJISTA. Produto/QA deve definir o contrato antes da correção.

### Observações ambientais, não classificadas como defeito

- A consulta de vulnerabilidades do NuGet ficou indisponível e gerou NU1900; build e testes concluíram normalmente.
- O runner precisou de permissão para socket local fora do sandbox; depois disso, os 22 testes passaram.
- A aplicação foi iniciada sem launch profile em uma porta local controlada; isso não alterou o comportamento funcional avaliado.

## 7. Encaminhamento

1. Planejar a decisão de autorização da saída administrativa.
2. Não usar a rota atual como evidência da futura ação operacional `Vendeu`.
3. Após a decisão e eventual correção em outro cartão, reexecutar HOM-ADM-13 e HOM-ADM-14.
4. Executar a regressão completa antes de alterar o status geral para aprovado.

Nenhuma correção de aplicação foi realizada neste roteiro.
