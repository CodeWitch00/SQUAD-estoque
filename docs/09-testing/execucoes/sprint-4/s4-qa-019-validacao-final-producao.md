# S4-QA-019 — Relatório final de validação em ambiente publicado

**Cartão:** S4-QA-019 — Validação final de produção
**Data da execução:** 19/11/2026
**Executor:** agente de QA (conferência por outro integrante pendente)
**Branch:** N/A — validação sem alteração de repositório
**Alteração de código:** nenhuma
**Ambiente:** aplicação publicada no Azure, compartilhada de homologação/demonstração
**URL:** `https://asp-squad-estoque-free-hgabd8asd5f4a9g9.northcentralus-01.azurewebsites.net`

## 1. Objetivo

Executar a validação final dos requisitos priorizados, perfis, interface, banco de dados (por inferência funcional), segurança e operação, registrando caso, resultado esperado, resultado obtido e evidência, sem corrigir defeitos neste cartão.

Para este cartão, o ambiente publicado compartilhado do Azure foi tratado como “produção” de referência para a apresentação acadêmica. Não havia um ambiente produtivo autônomo e autorizado para escrita; toda a massa usada foi fictícia e claramente identificada.

## 2. Método

- Execução manual assistida em navegador real (OpenCode/Chromium 152, Electron 44.9.1, Windows).
- Inspeção de DOM, texto visível, URL, redirecionamentos, códigos HTTP e cabeçalhos de resposta.
- Operações de escrita limitadas a um produto fictício criado exclusivamente para este teste.
- Nenhuma senha, hash, cookie completo ou dado real foi registrado nas evidências.
- Não foi possível gerar capturas de tela nesta sessão: o navegador retornou `Screenshot needs a visible tab. Call browser.tabs.focus and keep its desktop window visible.` A evidência é textual, de URL e de HTTP.
- Não houve acesso direto a SQLite/Kudu/azure CLI; a persistência foi comprovada pelas telas da aplicação.
- Os artefatos foram gravados na raiz do workspace, fora do repositório `SQUAD-estoque`.

## 3. Ambiente, contas e massa controlada

| Item | Valor |
|---|---|
| Ambiente | Azure App Service compartilhado, HTTPS |
| Perfis usados | `LOJISTA` e `VENDEDOR` (contas demonstrativas documentadas no projeto) |
| Senhas | Não registradas neste relatório nem nas evidências |
| Produto controlado | `QA S4 FINAL 2026-11-19` |
| ID do produto | `026b3292-709d-4be9-b0be-aafeaa8783ae` |
| Marca / categoria / cor | `Marca QA Final` / `Tênis` / `Azul` |
| SKU 37 | `7022e46e-822f-4e18-9f55-2bd005a4bc23` |
| SKU 38 | `dd1ccaff-d73a-4c1b-8542-10e10c421a44` |
| SKU 39 | `c0007e54-0de7-432e-862e-11af90f71cf7` |
| Atendimento — venda | `0ed9a7f4-70d6-430b-bf52-53973b8c06a3` |
| Atendimento — desistência | `82b407ce-2da4-499d-9c0d-3f11ae845e9e` |
| Atendimento — não tinha | Identificado na listagem `/Atendimentos` em 19/11/2026 17:01 |

Saldos finais do produto controlado: **37 = 3**, **38 = 1**, **39 = 0**.

### 3.1 Matriz de rastreabilidade dos requisitos priorizados

Os IDs abaixo seguem o SRS vigente (`RF-01` a `RF-30`, `RNF-01` a `RNF-07`, `RN-01` a `RN-09`) e o documento de casos de uso (`UC-01` a `UC-12`, `UC-S1` a `UC-S5`). Foram priorizados os requisitos obrigatórios do MVP e os riscos de autorização, persistência e operação.

| Caso | Requisitos / casos de uso rastreados | Cobertura | Observação |
|---|---|---|---|
| QA-01 | `RNF-05` | Parcial | HTTPS, `/health = 200 Healthy` e HSTS confirmados; redirecionamento HTTP do servidor não foi verificado (307 foi upgrade interno HSTS no Chromium) |
| QA-02 | `RF-01`, `RF-02`, `UC-01` | Aprovado | Redirecionamento com `ReturnUrl` |
| QA-03 | `RF-01`, `UC-01` | Aprovado | Mensagem de credencial inválida exibida |
| QA-04 | `RF-02`, `UC-01` | Aprovado | Destino pós-login do `LOJISTA` |
| QA-05 | `RF-02`, `UC-01` | Aprovado | Destino pós-login do `VENDEDOR` |
| QA-06 | `RF-02`, `RF-05`–`RF-12`, `RF-21`–`RF-23`, `UC-07`–`UC-12` | Aprovado | Rotas administrativas autorizadas |
| QA-07 | `RF-02`, `RN-04` | Aprovado | Vendedor bloqueado em ajuste e demais rotas administrativas |
| QA-08 | `RF-02` | Aprovado | Lojista bloqueado na consulta do vendedor |
| QA-09 | `RF-01`, `RF-03`, `UC-01` | Aprovado | Logout e nova exigência de autenticação |
| QA-10 | `RF-13`–`RF-16`, `RF-20`, `UC-02`–`UC-06` | Aprovado | Build atual da interface do vendedor |
| QA-11 | `RF-05`, `RF-06`, `RF-07`, `RF-09`, `UC-07`, `UC-08`, `UC-S1`, `RN-01` | Aprovado | Regressão de `DEF-S3QA030-01` sem workaround |
| QA-12 | `RF-14`, `RF-15`, `RF-17`, `UC-03`, `UC-04`, `UC-S2`, `UC-S5`, `RN-02`, `RN-07`, `RN-09` | Aprovado | Venda com decremento e saída atômica observável |
| QA-13 | `RF-18`, `RF-22`, `UC-05`, `UC-S3`, `RN-05`, `RN-06`, `RN-09` | Aprovado | Ruptura por declaração, vinculada ao SKU |
| QA-14 | `RF-19`, `RF-20`, `UC-06`, `RN-08`, `RN-09` | Aprovado | Desistência sem efeito no estoque |
| QA-15 | `RF-23`, `UC-10`, `RN-02`, `RN-03` | Aprovado | Ajuste com motivo e diferença absoluta registrada |
| QA-16 | `RF-21`, `UC-11` | Aprovado | Agrupamento por modelo |
| QA-17 | `RF-22`, `UC-12` | Aprovado | Quantidade de ocorrências e última data |
| QA-18 | `RF-24`, `RF-25`, `RN-08`, `RN-09` | Aprovado | Auditabilidade dos atendimentos |
| QA-19 | `RF-10`, `RF-12`, `UC-S5`, `RN-03` | Aprovado | Tipo, quantidade, responsável, data e motivo |
| QA-20 | `RF-04`, `RNF-04` | Aprovado com ressalva | Ausência de credenciais nos artefatos confirmada; hash não inspecionado por falta de acesso ao banco |
| QA-21 | Evidência de aceite | Bloqueado | Sem capturas nesta sessão |
| QA-22 | `RN-01`, `RN-07` | Bloqueado | Sem acesso direto ao banco; integridade inferida pelas telas |
| QA-23 | `RNF-01`, `RNF-06`, `RNF-07` | Não executado | Sem aparelho físico ou viewports registrados |
| QA-24 | `RNF-02`, `RNF-03` | Não executado | P95, cold start e disponibilidade não medidos |
| QA-25 | Processo de aceite / `S4-P2-13` | Bloqueado | Conferência de par pendente |

**Cobertura dos obrigatórios do MVP:** os requisitos `RF-01` a `RF-25` e `RNF-01` a `RNF-07` estão rastreados nesta matriz; `RNF-01`, `RNF-06` e `RNF-07` aparecem como não executados por limitação de dispositivo. `RF-26` a `RF-30` (NF-e e foto) não foram incluídos no plano priorizado deste cartão e permanecem fora do escopo da validação final aqui registrada.

## 4. Resultado consolidado por caso

| ID | Caso / requisito | Resultado esperado | Resultado obtido | Status | Evidência |
|---|---|---|---|---|---|
| QA-01 | Health check e HTTPS | `/health` responde `Healthy`; HTTP redireciona para HTTPS; HSTS presente | `GET https://.../health` → 200, corpo `Healthy`, HSTS presente; navegação HTTP no Chromium → `307 Internal Redirect` com motivo HSTS, sem resposta HTTP do servidor capturada | **Parcial**; HTTPS/HSTS passaram, redirecionamento do servidor não verificado | `../../evidencias/s4-qa-019/02-evidencias-http-e-dom.md` §1 |
| QA-02 | Acesso anônimo a rota protegida | Redirecionar para login com `ReturnUrl` | `/Produtos` → `/Account/Login?ReturnUrl=%2FProdutos` | **Aprovado** | §2 |
| QA-03 | Login inválido | Permanecer no login e exibir mensagem de erro | Permaneceu em `/Account/Login`; alerta `E-mail ou senha inválidos.` | **Aprovado** | §2 |
| QA-04 | Login LOJISTA | Destino pós-login da área administrativa | Redirecionou para `/Produtos` | **Aprovado** | §3 |
| QA-05 | Login VENDEDOR | Destino pós-login da consulta de estoque | Redirecionou para `/Estoque/Consulta` | **Aprovado** | §3 |
| QA-06 | Rotas autorizadas ao LOJISTA | `/Produtos`, `/Movimentacoes`, `/Movimentacoes/Entrada`, `/Movimentacoes/Ajuste`, `/SaldosZerados`, `/Rupturas`, `/Atendimentos` acessíveis | Todas responderam 200/conteúdo esperado na sessão LOJISTA | **Aprovado** | §3 e §8 |
| QA-07 | Bloqueio do VENDEDOR em rotas administrativas | Redirecionar para `AccessDenied` | Todas as rotas administrativas redirecionaram para `/Account/AccessDenied?ReturnUrl=...` | **Aprovado** | §3 |
| QA-08 | Bloqueio do LOJISTA na consulta do vendedor | `AccessDenied` em `/Estoque/Consulta` | Redirecionou para `/Account/AccessDenied?ReturnUrl=%2FEstoque%2FConsulta` | **Aprovado** | §3 |
| QA-09 | Logout e encerramento de sessão | Logout leva ao login; rota protegida volta a exigir autenticação | Logout → `/Account/Login`; acesso posterior a `/Produtos` redirecionou para login | **Aprovado** | §2 |
| QA-10 | Build publicada e UI atual do vendedor | Botões `Vendeu`, `Não tinha`, `Desistiu`, seleção de SKU e `Nova consulta` disponíveis | Todos os marcadores da versão atual estavam presentes no DOM publicado | **Aprovado** | §4 |
| QA-11 | Cadastro de produto e grade sem workaround (regressão `DEF-S3QA030-01`) | Salvar grade 37/38/39 com “Outras numerações” vazio; sem `data-val-required` | Grade salva sem workaround; SKUs 37=1, 38=2, 39=0 criados; campo `NumeracoesExtras` sem `data-val-required` | **Aprovado** | §5 |
| QA-12 | Consulta e venda — último par | Saldo reduzido; estado “Último par”; `SAIDA` registrada; atendimento `VENDEU` | Venda de 2 pares no SKU 38 → saldo 1, “Último par”; `SAIDA 1`; atendimento `VENDEU` com saldo consultado 2 | **Aprovado** | §6 e §7 |
| QA-13 | Registro de ruptura (`Não tinha`) | Ruptura persistida; saldo inalterado; nenhuma movimentação | SKU 39 permaneceu 0; `Rupturas` exibiu 1 ocorrência em 19/11/2026 17:01; nenhuma `SAIDA` para 39 | **Aprovado** | §7 e §8 |
| QA-14 | Desistência | Atendimento encerrado; nenhuma movimentação; saldo preservado | SKU 37 permaneceu 1; feedback `Atendimento encerrado. Nenhuma movimentação foi registrada.`; atendimento `DESISTIU` | **Aprovado** | §6 e §9 |
| QA-15 | Ajuste manual com motivo | Novo saldo apurado e motivo obrigatórios; histórico rastreável | SKU 37 ajustado de 1 para 3; `AJUSTE 2` com motivo `QA S4 FINAL - recontagem controlada 2026-11-19`; responsável `Lojista Teste` | **Aprovado** | §10 |
| QA-16 | Visão de saldos zerados | SKU com saldo 0 aparece agrupado ao produto | `/SaldosZerados` listou `QA S4 FINAL 2026-11-19`, numeração 39 | **Aprovado** | §8 |
| QA-17 | Histórico de rupturas | Ocorrência visível com modelo, numeração e data | `/Rupturas` exibiu `QA S4 FINAL 2026-11-19`, numeração 39, 1 ocorrência, última em 19/11/2026 17:01 (UTC) | **Aprovado** | §8 |
| QA-18 | Histórico de atendimentos | VENDEU, NÃO TINHA e DESISTIU rastreáveis | `/Atendimentos` exibiu os três resultados para o produto controlado, com vendedor, data e saldo consultado | **Aprovado** | §9 |
| QA-19 | Histórico de movimentações | ENTRADA, SAÍDA e AJUSTE com responsável e motivo | `/Movimentacoes` exibiu `ENTRADA` 37=1, `ENTRADA` 38=2, `SAÍDA` 38=1 e `AJUSTE` 37=+2, com motivos/responsáveis | **Aprovado** | §10 |
| QA-20 | Não exposição de credenciais em evidências | Nenhuma senha, hash ou cookie completo nos artefatos | Confirmado por revisão dos arquivos deste pacote; cabeçalho `Set-Cookie` foi redacionado na ferramenta | **Aprovado** | §11 |
| QA-21 | Capturas de tela | Evidência visual anexada | Não geradas: navegador exige aba visível; falha registrada | **Bloqueado** | §12 |
| QA-22 | Conferência de banco direto | Consulta SQLite/Kudu/CLI | Sem acesso ao banco do App Service; persistência inferida pelas telas | **Bloqueado** | §12 |
| QA-23 | Validação em aparelho móvel físico e viewports | Dispositivo real e viewports registrados | Não executado neste cartão | **Não executado** | §12 |
| QA-24 | Desempenho/P95 e cold start | Medição documentada | Não executado neste cartão; fora do escopo da validação funcional final | **Não executado** | §12 |
| QA-25 | Aceite formal | Conferência de outro integrante e registro de aceite | Pendente; apenas este relatório foi produzido | **Bloqueado** | §12 e `S4-P2-13` |

**Consolidado corrigido:** 19 casos aprovados, 1 parcial (QA-01), 3 bloqueados por limitação de ambiente/processo e 2 não executados. Nenhum caso funcional foi reprovado nesta rodada; a verificação do redirecionamento do servidor permanece pendente.

## 5. Defeitos conhecidos e pendências

| Item | Classificação | Situação nesta execução | Vínculo / ação |
|---|---|---|---|
| `DEF-S3QA030-01` — grade bloqueada por “Outras numerações” obrigatória sem mensagem visível | Defeito alto do baseline | **Não reproduzido**. A versão publicada não apresenta `data-val-required` em `NumeracoesExtras` e a grade foi salva sem workaround | Correção confirmada em produção; aceite formal ainda depende de `S4-P2-13` |
| `DEF-S3QA030-02` — texto “1 pares” | Defeito cosmético do baseline | Não houve reprodução dedicada nesta rodada; a confirmação capturada usou o estado “Último par” | Mantido como ressalva cosmética conhecida; sem bloqueio para apresentação |
| `DEF-S3QA030-03` — mensagem de validação em inglês na entrada | Defeito de localização do baseline | Não executado o cenário de quantidade 0 nesta rodada | Mantido como ressalva conhecida; sem bloqueio para apresentação |
| `OBS-S4QA019-01` — termo da busca do vendedor não preservado na URL após envio do formulário | Observação de rastreabilidade/usabilidade | Busca retornou produtos e a seleção funcionou; navegação direta com `?termo=T%C3%AAnis` funcionou. Não caracterizei como falha funcional | Registrar como melhoria, se desejar; não há cartão existente identificado; não bloqueia a apresentação |
| `S4-BE-003` — rotação de seed/credenciais demonstrativas | Impedimento operacional | Credenciais compartilhadas continuam em uso no ambiente de demonstração | Pendente antes de qualquer uso real |
| `S4-DB-004` — validação de migrations no Azure | Impedimento operacional | Não executada neste cartão; sem acesso a Kudu/CLI | Manter como pendência de operação |
| `S4-P2-13` — aceite formal do S3-QA-030 | Impedimento de processo | Não registrado por esta sessão | Depende de conferência por outro integrante |

Não foi identificado nenhum novo defeito funcional bloqueante nesta execução.

## 6. Parecer final

**Parecer: APTO para apresentação acadêmica, COM RESSALVAS.** O redirecionamento HTTP emitido pelo servidor não está confirmado nesta execução.

**Não apto para aceite formal neste momento.**

Justificativa objetiva:

1. Os fluxos críticos priorizados — autenticação, autorização por perfil, cadastro de produto/grade, entrada, saída, ajuste, consulta, venda, ruptura, desistência, saldos zerados, histórico de movimentações, rupturas e atendimentos — foram executados no ambiente publicado e produziram os efeitos esperados, com persistência observável pela aplicação.
2. A correção de `DEF-S3QA030-01`, que antes bloqueava o cadastro da grade oficial, está presente no build publicado e foi validada sem workaround.
3. O ambiente respondeu por HTTPS, com health check `Healthy` e HSTS. O redirecionamento HTTP→HTTPS observado no Chromium foi interno ao navegador; a resposta do servidor para HTTP ainda precisa de confirmação.
4. As ressalvas são de processo e cobertura: não há conferência de outro integrante, não foi possível anexar capturas, não houve acesso direto ao banco, não foram validados dispositivo físico/P95, o redirecionamento HTTP do servidor está pendente e permanecem as pendências operacionais `S4-BE-003` e `S4-DB-004`.
5. A conferência de par é condição necessária para converter este resultado em aceite formal, conforme `S4-P2-13`.

Para a apresentação acadêmica, o sistema pode ser demonstrado com o produto controlado descrito neste relatório. Para aceite formal, o parecer só deve ser revertido após a conferência documentada no checklist `../../evidencias/s4-qa-019/05-checklist-conferencia-par.md`.

## 7. Inventário de artefatos

| Arquivo | Conteúdo |
|---|---|
| `s4-qa-019-validacao-final-producao.md` | Relatório consolidado, resultados e parecer |
| `../../evidencias/s4-qa-019/01-massa-controlada.md` | Identificadores, SKUs, atendimentos e estado final |
| `../../evidencias/s4-qa-019/02-evidencias-http-e-dom.md` | Evidências textuais de HTTP, URL e DOM por caso |
| `../../evidencias/s4-qa-019/03-defeitos-impedimentos.md` | Defeitos conhecidos, observação e pendências |
| `../../evidencias/s4-qa-019/04-limitacoes.md` | Limitações da sessão e escopo não coberto |
| `../../evidencias/s4-qa-019/05-checklist-conferencia-par.md` | Checklist de aceite da atividade e conferência de outro integrante |

Os artefatos, originalmente coletados fora do repositório, estão versionados nos caminhos listados neste inventário pelo PR #117.
