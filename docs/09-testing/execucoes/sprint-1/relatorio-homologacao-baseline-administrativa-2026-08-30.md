# Relatório de homologação — baseline administrativa

## 1. Identificação da execução

| Campo | Valor |
|---|---|
| Data e hora de encerramento | 2026-08-30 17:06 BRT |
| Commit da aplicação | `fbd95e1` |
| Branch | `main` |
| Executor | Codex, em homologação assistida solicitada pela equipe |
| Forma de execução | Manual assistida por requisições HTTP reais, formulários MVC, cookies, antiforgery e inspeção SQLite |
| Ambiente | ASP.NET Core Development local, .NET SDK 10.0.302, Kestrel HTTP local e SQLite descartável |
| Banco de homologação | Base exclusiva criada em `/tmp`; o banco local do projeto não foi alterado |
| Massa | Usuários demo `LOJISTA` e `VENDEDOR`; produto `Homologacao ADM 20260830`; grade 37, 38 e 39 |
| Resultado geral | **REPROVADO COM IMPEDIMENTO DE AUTORIZAÇÃO** |

Esta execução valida os fluxos administrativos implementados antes do início de novas funcionalidades. Nenhum problema encontrado foi corrigido durante a homologação.

O passo a passo consolidado, com evidência e status por cenário, está no [roteiro executado da baseline administrativa](roteiro-executado-baseline-administrativa-2026-08-30.md).

## 2. Resumo

| Resultado | Quantidade |
|---|---:|
| Aprovado | 13 |
| Reprovado | 1 |
| Bloqueado | 0 |
| Não executado | 0 |

O único caso reprovado permite escrita administrativa por `VENDEDOR`. A suíte xUnit permaneceu verde com 22 de 22 testes, demonstrando que a falha de autorização ainda não é detectada pela baseline automatizada.

## 3. Resultados dos casos

### HOM-ADM-01 — Login inválido

| Campo | Registro |
|---|---|
| Requisito/caso relacionado | RF-01; AUT-04 |
| Pré-condição | Usuário lojista existente; sessão anônima |
| Entrada/passos | Enviar `lojista@squad.com` com senha incorreta pelo formulário e token antiforgery válido |
| Resultado esperado | `200 OK`, mensagem genérica e nenhuma sessão autenticada |
| Resultado obtido | `200 OK`; mensagem `E-mail ou senha inválidos.`; apenas cookie antiforgery foi emitido |
| Status de execução | **Aprovado** |
| Evidência | Resposta HTTP e inspeção sanitizada do cookie jar da execução |

### HOM-ADM-02 — Login, sessão e logout por perfil

| Campo | Registro |
|---|---|
| Requisito/caso relacionado | RF-01 a RF-03; AUT-02, AUT-03 e AUT-16 |
| Pré-condição | USR-LOJ-01 e USR-VEN-01 presentes no seed demonstrativo |
| Entrada/passos | Autenticar separadamente `lojista@squad.com` e `vendedor@squad.com` com senha de teste; encerrar a sessão do vendedor; reabrir rota protegida |
| Resultado esperado | Ambos autenticam; sessão preserva o perfil; logout redireciona ao login e invalida acesso posterior |
| Resultado obtido | Ambos os logins retornaram `302` para `/`; logout retornou `302` para `/Account/Login`; acesso posterior a Saída retornou `302` para Login com `ReturnUrl` |
| Status de execução | **Aprovado** |
| Evidência | Status e destinos HTTP registrados na execução |

### HOM-ADM-03 — Cadastrar produto e grade válidos

| Campo | Registro |
|---|---|
| Requisito/caso relacionado | RF-05 a RF-08; ADM-PROD-01 |
| Pré-condição | LOJISTA autenticado; catálogo inicialmente sem a massa desta execução |
| Entrada/passos | Cadastrar `Homologacao ADM 20260830`, Marca QA, categoria Tenis, cor Azul e grade `37,38,39` |
| Resultado esperado | Redirecionar à listagem; persistir um produto ativo e três SKUs ativos, únicos, com saldo zero |
| Resultado obtido | `302` para `/Produtos`; produto `0169FD69-5CE0-46B2-9142-B17194BD7191`; três SKUs distintos; saldos iniciais 0 |
| Status de execução | **Aprovado** |
| Evidência | Resposta HTTP, detalhe MVC e consulta agrupada às tabelas Produto e Sku |

### HOM-ADM-04 — Rejeitar grade duplicada

| Campo | Registro |
|---|---|
| Requisito/caso relacionado | RF-08, RN-01; ADM-PROD-04 |
| Pré-condição | LOJISTA autenticado |
| Entrada/passos | Cadastrar `Produto grade invalida` com grade `38,39,38` |
| Resultado esperado | Reapresentar formulário com erro e não persistir Produto ou SKU |
| Resultado obtido | `200 OK`; mensagem de numeração `38` informada mais de uma vez; zero produtos persistidos com o nome usado |
| Status de execução | **Aprovado** |
| Evidência | HTML da validação e consulta de contagem no SQLite |

### HOM-ADM-05 — Registrar entrada válida

| Campo | Registro |
|---|---|
| Requisito/caso relacionado | RF-09, RF-10; ADM-ENT-01 |
| Pré-condição | LOJISTA autenticado; SKU 37 com saldo 0 |
| Entrada/passos | Registrar entrada de 10 pares com motivo `Homologacao entrada` |
| Resultado esperado | Saldo 10; uma movimentação ENTRADA de 10 com lojista, motivo e data/hora; redirecionamento ao produto |
| Resultado obtido | `302` para o detalhe; saldo 10; uma movimentação tipo 0/ENTRADA, quantidade 10, responsável `lojista@squad.com`, motivo e data presentes |
| Status de execução | **Aprovado** |
| Evidência | Resposta HTTP e junção Sku, Movimentacao e Usuario no SQLite |

### HOM-ADM-06 — Rejeitar entrada de quantidade zero

| Campo | Registro |
|---|---|
| Requisito/caso relacionado | RF-09; ADM-ENT-02 |
| Pré-condição | LOJISTA autenticado; SKU 38 com saldo 0 |
| Entrada/passos | Enviar entrada com quantidade 0 |
| Resultado esperado | Reapresentar formulário com erro; saldo e histórico inalterados |
| Resultado obtido | `200 OK`; mensagem `A quantidade deve ser no mínimo 1.`; saldo 0 e zero movimentações para o SKU |
| Status de execução | **Aprovado** |
| Evidência | HTML da validação e consulta ao SQLite |

### HOM-ADM-07 — Registrar saída válida como LOJISTA

| Campo | Registro |
|---|---|
| Requisito/caso relacionado | RF-10, RF-11; ADM-SAI-01 |
| Pré-condição | LOJISTA autenticado; SKU 37 com saldo 10 |
| Entrada/passos | Registrar saída de 3 pares com motivo `Homologacao saida` |
| Resultado esperado | Saldo 7; movimentação SAIDA de 3 com responsável e data/hora; redirecionamento ao produto |
| Resultado obtido | `302` para o detalhe; saldo 7; uma movimentação tipo 1/SAIDA, quantidade 3, responsável `lojista@squad.com`, motivo e data presentes |
| Status de execução | **Aprovado** |
| Evidência | Resposta HTTP e consulta ao SQLite |

### HOM-ADM-08 — Rejeitar saída com saldo insuficiente

| Campo | Registro |
|---|---|
| Requisito/caso relacionado | RF-11, RN-02; ADM-SAI-02 |
| Pré-condição | LOJISTA autenticado; SKU 37 com saldo 7 e duas movimentações |
| Entrada/passos | Tentar saída de 99 pares |
| Resultado esperado | Erro claro; saldo 7; nenhuma movimentação adicional |
| Resultado obtido | `200 OK`; mensagem de saldo disponível 7; saldo permaneceu 7 e total de movimentações permaneceu 2 |
| Status de execução | **Aprovado** |
| Evidência | HTML da validação e contagens no SQLite |

### HOM-ADM-09 — Registrar ajuste válido

| Campo | Registro |
|---|---|
| Requisito/caso relacionado | RF-10, RF-23, RN-04; ADM-AJU-01 |
| Pré-condição | LOJISTA autenticado; SKU 37 com saldo 7 |
| Entrada/passos | Ajustar saldo apurado para 12 com motivo `Contagem homologacao` |
| Resultado esperado | Saldo 12; movimentação AJUSTE de diferença 5, com motivo, responsável e data/hora |
| Resultado obtido | `302` para o detalhe; saldo 12; movimentação tipo 2/AJUSTE, quantidade 5, responsável `lojista@squad.com`, motivo e data presentes |
| Status de execução | **Aprovado** |
| Evidência | Resposta HTTP e consulta ao SQLite |

### HOM-ADM-10 — Rejeitar ajuste com motivo curto

| Campo | Registro |
|---|---|
| Requisito/caso relacionado | RF-23; ADM-AJU-03 |
| Pré-condição | LOJISTA autenticado; SKU 37 com saldo 12 e três movimentações |
| Entrada/passos | Tentar novo saldo 15 com motivo `x` |
| Resultado esperado | Erro de motivo; saldo 12; nenhuma movimentação adicional |
| Resultado obtido | `200 OK`; mensagem de mínimo de 5 caracteres; saldo permaneceu 12 e total de movimentações permaneceu 3 |
| Status de execução | **Aprovado** |
| Evidência | HTML da validação e contagens no SQLite |

### HOM-ADM-11 — Exibir histórico administrativo

| Campo | Registro |
|---|---|
| Requisito/caso relacionado | RF-10, RN-03; ADM-HIS-01 |
| Pré-condição | Entrada, saída e ajuste válidos registrados pelo LOJISTA |
| Entrada/passos | Abrir `GET /Movimentacoes` e comparar a tabela com o banco |
| Resultado esperado | Exibir mais recente primeiro, com data, produto, numeração, tipo, quantidade, responsável e motivo |
| Resultado obtido | `200 OK`; ordem AJUSTE, SAÍDA, ENTRADA; produto, tamanho 37, quantidades 5/3/10, Lojista Teste e os três motivos exibidos |
| Status de execução | **Aprovado** |
| Evidência | Tabela HTML e consulta SQLite ordenada por `CriadoEm DESC` |

### HOM-ADM-12 — Bloquear rotas para usuário anônimo

| Campo | Registro |
|---|---|
| Requisito/caso relacionado | RF-02; ADM-PROD-07, ADM-HIS-02 e ADM-AJU-06 |
| Pré-condição | Sem sessão autenticada |
| Entrada/passos | Acessar Produtos, Histórico, Entrada, Saída e Ajuste diretamente |
| Resultado esperado | Todas retornam `302` para Login com `ReturnUrl`; nenhum conteúdo protegido é entregue |
| Resultado obtido | As cinco rotas retornaram `302` para Login com o respectivo `ReturnUrl` |
| Status de execução | **Aprovado** |
| Evidência | Cabeçalhos HTTP de cada rota |

### HOM-ADM-13 — Validar matriz de rotas por perfil

| Campo | Registro |
|---|---|
| Requisito/caso relacionado | RF-02, RN-04; ADM-PROD-07, ADM-HIS-02 e ADM-AJU-06 |
| Pré-condição | Sessões independentes de LOJISTA e VENDEDOR |
| Entrada/passos | Abrir Produtos, Histórico, Entrada, Saída e Ajuste com cada perfil |
| Resultado esperado | LOJISTA acessa os cinco fluxos; VENDEDOR recebe AccessDenied em Produtos, Histórico, Entrada e Ajuste |
| Resultado obtido | LOJISTA recebeu `200` nas cinco rotas; VENDEDOR recebeu `302` para AccessDenied nas quatro rotas proibidas |
| Status de execução | **Aprovado**, exceto a autorização específica de Saída tratada em HOM-ADM-14 |
| Evidência | Matriz de códigos e destinos HTTP |

### HOM-ADM-14 — Bloquear saída administrativa para VENDEDOR

| Campo | Registro |
|---|---|
| Requisito/caso relacionado | RF-02 e separação de perfis; ADM-SAI-05 |
| Pré-condição | VENDEDOR autenticado; SKU 37 com saldo 12; token antiforgery válido |
| Entrada/passos | Abrir `GET /Movimentacoes/Saida`; enviar saída de 1 par com motivo `Prova autorizacao indevida` |
| Resultado esperado | GET e POST negados; saldo permanece 12; nenhuma nova movimentação |
| Resultado obtido | GET retornou `200`; POST retornou `302` para `/`; saldo caiu para 11; quarta movimentação SAIDA criada com responsável `vendedor@squad.com` |
| Status de execução | **REPROVADO — IMPEDIMENTO** |
| Evidência | Status HTTP; saldo final 11; movimentação tipo 1, quantidade 1, motivo, vendedor e data/hora confirmados no SQLite |

## 4. Impedimento registrado

### IMP-ADM-001 — VENDEDOR altera estoque pela saída administrativa

| Campo | Registro |
|---|---|
| Severidade sugerida | P0 — escrita administrativa por perfil não autorizado no fluxo-alvo |
| Origem | HOM-ADM-14 / ADM-SAI-05 |
| Comportamento | O controller aceita `LOJISTA,VENDEDOR` no GET e POST de `/Movimentacoes/Saida` |
| Impacto comprovado | VENDEDOR reduziu saldo e criou movimentação administrativa de quantidade livre |
| Decisão neste cartão | Não corrigir; encaminhar para planejamento e alinhamento definitivo de autorização |
| Critério para reteste | Backend deve aplicar a permissão aprovada para o fluxo administrativo; GET e POST devem ser retestados, incluindo ausência de alteração no banco |

Existe uma contradição documental: o manual de ambiente descreve Saída como capacidade do VENDEDOR, enquanto o inventário UX e a especificação administrativa reservam a saída genérica ao LOJISTA e tratam `Vendeu` como fluxo operacional separado. Produto/QA deve confirmar o contrato; até essa decisão, a homologação permanece reprovada.

## 5. Evidência consolidada da massa

Estado final após o caso de autorização reprovado:

| Produto | SKU | Saldo final | Movimentações |
|---|---|---:|---:|
| Homologacao ADM 20260830 | 37 | 11 | 4 |
| Homologacao ADM 20260830 | 38 | 0 | 0 |
| Homologacao ADM 20260830 | 39 | 0 | 0 |

Movimentações do SKU 37:

| Ordem | Tipo | Quantidade | Responsável | Motivo |
|---:|---|---:|---|---|
| 1 | ENTRADA | 10 | lojista@squad.com | Homologacao entrada |
| 2 | SAÍDA | 3 | lojista@squad.com | Homologacao saida |
| 3 | AJUSTE | 5 | lojista@squad.com | Contagem homologacao |
| 4 | SAÍDA | 1 | vendedor@squad.com | Prova autorizacao indevida |

Os HTMLs, cookies e tokens brutos permaneceram somente na pasta temporária da execução e não foram versionados, para evitar exposição de artefatos de sessão. Este relatório contém a evidência sanitizada e reproduzível.

## 6. Verificações de apoio

| Verificação | Resultado |
|---|---|
| `dotnet build --no-restore` | Aprovado, zero erros; aviso NU1900 por indisponibilidade da consulta de vulnerabilidades do NuGet |
| `dotnet test --no-restore` | Aprovado: 22 passados, 0 falhos, 0 ignorados |
| Execução inicial do runner no sandbox | Abortada por permissão de socket local; repetida com permissão adequada e aprovada; não é defeito da aplicação |
| Subida inicial com launch profile | Porta do perfil não disponível no sandbox; repetida sem launch profile em `127.0.0.1:5199`; não é defeito funcional |

## 7. Conclusão

A baseline administrativa não está homologada para encerramento do cartão porque IMP-ADM-001 permite alteração de estoque por perfil divergente do fluxo-alvo documentado. Os demais fluxos executados — login, produto e grade, entrada, saída por LOJISTA, ajuste, histórico e proteções restantes — atenderam aos resultados esperados.

Nenhuma correção foi realizada. O próximo passo é planejar a decisão de permissão e, após a alteração aprovada, reexecutar HOM-ADM-13 e HOM-ADM-14, além da regressão completa.
