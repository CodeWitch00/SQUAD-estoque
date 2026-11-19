# S4-QA-019 — Checklist de aceite da atividade e conferência de par

Este checklist substitui a versão anterior, que cobria apenas a conferência entre integrantes. Ele agora opera em duas camadas:

1. **Critérios formais do enunciado do cartão** — o que precisa estar verdadeiro para a atividade ser considerada entregue.
2. **Conferência de par** — condição necessária para converter o parecer em aceite formal (`S4-P2-13`).

## 1. Identificação

| Campo | Valor / preenchimento |
|---|---|
| Cartão | `S4-QA-019` — Validação final de produção |
| Data da execução | 19/11/2026 |
| Executor | agente de QA |
| Revisor | ______________________________ |
| Data/hora da conferência | ______________________________ |
| Ambiente conferido | Azure publicado: `https://asp-squad-estoque-free-hgabd8asd5f4a9g9.northcentralus-01.azurewebsites.net` |
| Branch usada na validação | `N/A` — nenhuma alteração de repositório |
| Resultado da conferência | ☐ Concordo ☐ Discordo ☐ Concordo com ressalvas |

## 2. Critérios formais da atividade

| # | Critério exigido no enunciado | Onde conferir | Situação |
|---|---|---|---|
| C-01 | Plano final de validação executado | Relatório §4, 25 casos com status | ☑ Atendido |
| C-02 | Cada caso registra caso, esperado, obtido e evidência | Relatório §4 + `02-evidencias-http-e-dom.md` | ☑ Atendido |
| C-03 | Cobertura dos requisitos priorizados com rastreabilidade | Relatório §3.1 — matriz RF/RNF/RN/UC | ☑ Atendido |
| C-04 | Perfis `LOJISTA` e `VENDEDOR` validados | Relatório QA-04 a QA-08 | ☑ Atendido |
| C-05 | Interface validada no build publicado | Relatório QA-10 + evidências §4 | ☑ Atendido, com ressalva de captura |
| C-06 | Banco de dados verificado quanto à persistência | Relatório QA-11 a QA-19; evidências §5–§10 | ☑ Atendido por inferência funcional; ⚠ sem acesso direto |
| C-07 | Segurança validada | Relatório QA-01, QA-02, QA-03, QA-07, QA-08, QA-20 | ☑ Atendido |
| C-08 | Operação validada | Relatório QA-01 e pendências `S4-BE-003`, `S4-DB-004` | ☑ Atendido com pendências registradas |
| C-09 | Contas controladas e nenhum dado real afetado | `01-massa-controlada.md` | ☑ Atendido |
| C-10 | Nenhum defeito corrigido neste cartão | Histórico de mudanças: nenhum commit | ☑ Atendido |
| C-11 | Defeitos vinculados a cartões ou impedimentos registrados | `03-defeitos-impedimentos.md` | ☑ Atendido |
| C-12 | Conferência de outro integrante antes do resultado final | Seção 5 deste checklist | ☐ Pendente |
| C-13 | Entregáveis completos: relatório, evidências, defeitos e parecer | Inventário no relatório §7 | ☑ Atendido |
| C-14 | Parecer objetivo apto/não apto | Relatório §6 | ☑ Atendido |
| C-15 | Branch `N/A`; validação sem alteração do repositório | `git status` da branch atual | ☑ Atendido — nenhum arquivo do repositório foi alterado |

## 3. Conferência funcional pelo par

- [ ] Confirmar que `/health` responde `Healthy` por HTTPS e que HTTP redireciona para HTTPS com HSTS.
- [ ] Confirmar que `/Produtos` anônimo redireciona para `/Account/Login?ReturnUrl=%2FProdutos`.
- [ ] Confirmar login inválido com mensagem `E-mail ou senha inválidos.`
- [ ] Confirmar destinos pós-login: `LOJISTA` → `/Produtos`; `VENDEDOR` → `/Estoque/Consulta`.
- [ ] Confirmar `AccessDenied` do `VENDEDOR` em rotas administrativas e do `LOJISTA` em `/Estoque/Consulta`.
- [ ] Confirmar logout e nova exigência de login em rota protegida.
- [ ] Confirmar que a grade publicada contém `Vendeu`, `Não tinha`, `Desistiu`, seleção de SKU e `Nova consulta`.
- [ ] Confirmar que `QA S4 FINAL 2026-11-19` existe e que `NumeracoesExtras` não possui `data-val-required`.

## 4. Conferência de massa, persistência e rastreabilidade

- [ ] Localizar o produto `026b3292-709d-4be9-b0be-aafeaa8783ae`.
- [ ] Conferir saldos finais: 37 = 3, 38 = 1, 39 = 0.
- [ ] Conferir SKUs 37 `7022e46e-...`, 38 `dd1ccaff-...`, 39 `c0007e54-...`.
- [ ] Conferir em `/Movimentacoes`: `ENTRADA` 37=1, `ENTRADA` 38=2, `SAÍDA` 38=1 e `AJUSTE` 37=+2 com motivo `QA S4 FINAL - recontagem controlada 2026-11-19`.
- [ ] Conferir em `/Atendimentos`: `VENDEU` na 38, `NÃO TINHA` na 39 e `DESISTIU` na 37.
- [ ] Conferir em `/Rupturas`: 39 com 1 ocorrência em 19/11/2026 17:01 (UTC).
- [ ] Conferir em `/SaldosZerados`: produto controlado com a numeração 39.
- [ ] Confirmar que não existe `SAIDA` para 39 e que a desistência não gerou movimentação.
- [ ] Confirmar que nenhuma senha, hash, cookie ou token foi gravado nos artefatos.

## 5. Conferência de pendências e limitações

- [ ] `DEF-S3QA030-01`: confirmar que a correção está publicada e que não houve workaround.
- [ ] `DEF-S3QA030-02` e `DEF-S3QA030-03`: reconhecer que não foram reproduzidos de forma dedicada nesta rodada.
- [ ] `OBS-S4QA019-01`: decidir se o termo da busca não preservado na URL deve gerar cartão de melhoria.
- [ ] `S4-BE-003`: acompanhar rotação/remoção das credenciais demonstrativas antes de uso real.
- [ ] `S4-DB-004`: acompanhar validação das migrations no Azure.
- [ ] `S4-P2-13`: registrar o aceite formal somente após esta conferência.
- [ ] Reconhecer ausência de capturas de tela, de acesso direto ao banco, de aparelho físico e de medição de P95/cold start.

## 6. Pré-checklist para um eventual PR

Este item **não foi executado**; ele registra as condições que deverão ser satisfeitas se o cartão for posteriormente publicado em um pull request.

- [ ] Decisão formal sobre a restrição original “Branch: N/A — sem alteração do repositório”.
- [ ] Usar worktree isolado a partir de `origin/main`, nunca a branch suja `ops/s4-ops-009-integrante`.
- [ ] Copiar somente os artefatos `S4-QA-019-*` para:
  - `../../execucoes/sprint-4/s4-qa-019-validacao-final-producao.md`
  - `./` (pasta `s4-qa-019/`)
- [ ] Ajustar os links relativos das evidências após a mudança de pasta.
- [ ] Não incluir os 15 arquivos modificados/untracked já existentes na branch do usuário.
- [ ] Não incluir código, correção de defeitos, senhas, cookies ou tokens.
- [ ] Conferir `git status` e `git diff --stat` antes do commit.
- [ ] Commit atômico, push e `gh pr create` com base em `main`.
- [ ] Não fazer merge automático; aguardar revisão.

A prova documental dessa prontidão é mantida fora do repositório, na raiz do workspace.

## 7. Registro final do par

```text
Parecer do revisor:
______________________________________________________________________
______________________________________________________________________

Resposta por critério discorde (C-xx): _________________________________
______________________________________________________________________

Assinatura/identificação: ____________________________________________
Data: ____________________
```
