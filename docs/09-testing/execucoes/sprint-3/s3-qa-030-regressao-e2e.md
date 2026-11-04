# Relatório de execução E2E — S3-QA-030 (v2)

**Cartão:** S3-QA-030 — Executar regressão funcional: vendedor e lojista ponta a ponta
**Sprint:** Sprint 3
**Prazo formal:** 11/11/2026 23:59 (America/Sao_Paulo)
**Reunião-alvo:** 02/11/2026
**Execução:** 03/11/2026, reteste integral após a correção de `DEF-S3QA030-01`
**Responsável pela execução:** agente de QA
**Revisão por outro integrante:** pendente
**Alteração de código:** sim — uma linha no view model da grade, autorizada para corrigir o defeito bloqueante. Nenhuma outra alteração de produto foi feita.

Esta é a segunda versão do registro. A v1 (PR #91) foi encerrada pela revisão porque o E2E-02 falhou, a massa usou a numeração extra 45 como workaround e o relatório expunha a senha das contas de desenvolvimento. Nesta v2 o defeito foi corrigido, a senha foi removida e o cenário oficial 37–40 foi reexecutado sem workaround.

## 1. Baseline e ambiente

| Item | Valor |
|---|---|
| Repositório | `CodeWitch00/SQUAD-estoque` |
| Branch | `test/s3-qa-030-e2e` |
| Commit-base | `9111713d5526dc8f757ed10b6b7d7a6600f04189` (`main`, 03/11/2026 19:53 -03:00) |
| Commit da correção | `ccd27fbb334d05a15064ed9f04f89b26506a2dd0` |
| Sistema operacional | Windows NT 10.0.26200.0 |
| SDK | .NET SDK 10.0.403, aplicação `net10.0` |
| Servidor | Kestrel HTTP em `http://localhost:5186` |
| Banco | SQLite descartável `src/SquadEstoque.Web/Estoque.db` |
| Configuração | `ASPNETCORE_ENVIRONMENT=Development`, `Demo__SeedUsers=true` |
| Navegador | OpenCode 2.2.5 / Chrome 152.0.7977.167 / Electron 44.9.1, viewport 1000×700 |
| Fuso da execução | `America/Sao_Paulo` |
| Credenciais | contas de seed de desenvolvimento fornecidas pelo ambiente; valores omitidos |
| Massa | `Tênis E2E QA V2`, grade oficial 37, 38, 39 e 40; nenhuma numeração extra |

A branch foi atualizada sobre a `main` de `9111713` antes da correção. O banco foi criado do zero e migrado pela própria aplicação, sem escrita manual.

## 2. Correção aplicada

`DEF-S3QA030-01` era causado pelo tipo não anulável do campo:

```csharp
// antes
public string NumeracoesExtras { get; set; } = string.Empty;
```

Com referências anuláveis habilitadas, o ASP.NET Core trata propriedades `string` não anuláveis como obrigatórias. O campo aparecia como opcional, mas bloqueava o envio da grade quando ficava vazio. A correção torna o campo explicitamente opcional:

```csharp
// depois
public string? NumeracoesExtras { get; set; }
```

A alteração está no commit `ccd27fb`, em `src/SquadEstoque.Web/Models/ViewModels/AdicionarNumeracoesViewModel.cs`. Não houve migration, alteração de banco ou mudança no fluxo de numerações extras.

Também foi incluído o teste HTTP `Lojista_saves_official_grade_when_extras_field_is_submitted_empty` em `AdministrativeHttpTests`, que envia `NumeracoesExtras` explicitamente vazio. O teste falha na baseline `9111713` e passa com a correção, cobrindo o defeito na CI.

## 3. Método e evidências

- Execução manual assistida pelo navegador, com inspeção de DOM, URLs, requisições HTTP e consultas diretas ao SQLite.
- Cada operação de estoque foi conferida antes/depois no banco, além da resposta visível na tela.
- Capturas de tela não puderam ser geradas nesta sessão: `browser.screenshot` retorna “Screenshot needs a visible tab”. A evidência é textual/DOM, HTTP e banco.
- Massa e estado final exportados em [`evidencias/s3-qa-030/estado-final-banco.json`](../../evidencias/s3-qa-030/estado-final-banco.json), sem senhas, hashes ou e-mails.
- Snapshots de UI e detalhes de reprodução em [`evidencias/s3-qa-030/evidencias-ui-e2e.md`](../../evidencias/s3-qa-030/evidencias-ui-e2e.md).

Comandos usados para subir a aplicação (sem credenciais):

```powershell
cd src/SquadEstoque.Web
$env:ASPNETCORE_ENVIRONMENT='Development'
$env:Demo__SeedUsers='true'
dotnet run --no-build --urls http://localhost:5186
```

## 4. Resumo dos casos oficiais

| ID | Cenário | Resultado esperado | Resultado obtido | Status | Evidência |
|---|---|---|---|---|---|
| E2E-01 | Login e logout de lojista | Home do perfil; área autorizada; após sair, rota protegida pede login | Login em `/Produtos`; logout em `/Account/Login`; rotas anônimas com 302 e `ReturnUrl` | **Aprovado** | §5.1 |
| E2E-02 | Cadastro até entrada inicial | Produto e SKUs visíveis; saldos e movimentações corretos | Grade oficial 37–40 salva com “Outras numerações” vazio; POST 302; 4 SKUs e saldos iniciais corretos; entradas individual e em lote conferidas | **Aprovado** | §§5.2–5.3 |
| E2E-03 | Consulta e venda | Grade aparece; saldo reduz em 1; confirmação visível | Grade abriu; duas vendas no 38: 3→2→1; confirmações visíveis; ressalva cosmética “1 pares” | **Aprovado** | §5.5 |
| E2E-04 | Consulta e ruptura | Ruptura registrada; saldo não muda | 3 ações `Não tinha` no 40: saldo permaneceu 0, 3 rupturas e nenhuma saída | **Aprovado** | §5.6 |
| E2E-05 | Desistência e continuidade | Nenhum efeito no estoque; uso continua sem bloqueio | `Desistiu` no 39 concluiu sem movimentação/ruptura; retorno à busca; nova consulta abriu a grade | **Aprovado** | §5.7 |
| E2E-06 | Ajuste administrativo | Novo saldo e histórico rastreável; vendedor não acessa | Ajustes 1→3 e 2→0 com motivo e responsável; ajuste sem alteração rejeitado; vendedor recebeu `AccessDenied` | **Aprovado** | §5.4 |
| E2E-07 | Decisão de reposição | Agrupamentos e frequências correspondem à massa | `/SaldosZerados` listou somente 40; `/Rupturas` agrupou 40 com 3 ocorrências | **Aprovado** | §5.8 |

**Conclusão dos casos:** 7 aprovados e 0 reprovados na baseline corrigida. O E2E-02 foi concluído com a grade oficial 37–40, sem numeração extra e sem contornar a validação do cliente.

O aceite formal ainda depende da conferência desta execução por outro integrante, conforme a regra do cartão. O agente de QA não declara aprovação final sozinho.

## 5. Detalhamento por fluxo

### 5.1 Login, logout e autorização

| Perfil | Ação | Esperado | Obtido | Status |
|---|---|---|---|---|
| LOJISTA | Login válido | Entrar na área do lojista | `/Produtos` | Aprovado |
| LOJISTA | Logout | Encerrar sessão | `/Account/Login` | Aprovado |
| Anônimo | Abrir `/Produtos` | Pedir login com retorno | 302 → `/Account/Login?ReturnUrl=%2FProdutos` | Aprovado |
| Anônimo | Abrir `/Estoque/Consulta` | Pedir login com retorno | 302 → `/Account/Login?ReturnUrl=%2FEstoque%2FConsulta` | Aprovado |
| Anônimo | Abrir `/Movimentacoes/Entrada` | Pedir login com retorno | 302 → `/Account/Login?ReturnUrl=%2FMovimentacoes%2FEntrada` | Aprovado |
| Anônimo | Abrir `/Movimentacoes/Ajuste` | Pedir login com retorno | 302 → `/Account/Login?ReturnUrl=%2FMovimentacoes%2FAjuste` | Aprovado |
| Anônimo | Abrir `/SaldosZerados`, `/Rupturas`, `/Atendimentos` | Pedir login com retorno | 302 com `ReturnUrl` para as três rotas | Aprovado |
| VENDEDOR | Abrir `/Produtos`, `/Movimentacoes`, `/Movimentacoes/Entrada`, `/Movimentacoes/Ajuste`, `/SaldosZerados`, `/Rupturas`, `/Atendimentos` | Acesso negado | Todas → `/Account/AccessDenied` com “Acesso Não Autorizado” | Aprovado |
| LOJISTA | Abrir `/Estoque/Consulta` | Acesso negado | `/Account/AccessDenied?ReturnUrl=%2FEstoque%2FConsulta` | Aprovado |

### 5.2 Produto, grade oficial e entrada inicial (LOJISTA)

**Produto:** `Tênis E2E QA V2` (`63A0854A-5CD0-4FFE-B6D8-CC89031B8608`), marca `Marca QA`, categoria `Tênis`, cor `Azul`.

**Esperado:** criar produto, informar a grade oficial 37–40, deixar “Outras numerações” vazio, salvar e registrar entrada inicial.

**Obtido:**

1. O produto foi criado e apareceu em `/Produtos` com a grade `37 38 39 40`.
2. Na tela de grade, o campo “Outras numerações” ficou vazio. O HTML apresentou `required=false`, `validationMessage` vazio e `form.checkValidity() === true`.
3. O clique em “Salvar grade” enviou `POST /Produtos/AdicionarNumeracoes/63a0854a-...` e recebeu **HTTP 302** para os detalhes do produto.
4. A grade foi persistida com quatro SKUs: **37=1, 38=2, 39=1, 40=0**. Nenhuma numeração extra foi criada.
5. As movimentações iniciais foram criadas para 37, 38 e 39 com o motivo `Saldo inicial no cadastro da grade`. O 40 ficou zerado, sem movimentação inicial.
6. Entrada individual no 38: 2→3, `ENTRADA` 1, motivo `Entrada individual E2E v2`.
7. Entrada em lote no 40: 0→2, `ENTRADA` 2, motivo `Entrada em lote E2E v2`.
8. Quantidade 0 na entrada foi bloqueada pelo cliente com a mensagem em português `O valor deve ser maior ou igual a 1.`.

**Conferência do defeito corrigido:** o fluxo anterior (v1) não enviava POST, marcava o campo com `input-validation-error` e não exibia mensagem. Nesta execução o envio ocorreu normalmente com o campo vazio, e os quatro SKUs oficiais foram salvos.

### 5.3 Entrada inicial e saldos

| Momento | 37 | 38 | 39 | 40 |
|---|---:|---:|---:|---:|
| Após salvar a grade | 1 | 2 | 1 | 0 |
| Após entrada individual no 38 | 1 | 3 | 1 | 0 |
| Após entrada em lote no 40 | 1 | 3 | 1 | 2 |
| Após ajustes (39→3 e 40→0) | 1 | 3 | 3 | 0 |
| Após vendas no 38 | 1 | 1 | 3 | 0 |

### 5.4 Ajuste administrativo (LOJISTA)

| SKU | Antes | Novo saldo | Movimento | Motivo | Resultado |
|---|---:|---:|---|---|---|
| 39 | 1 | 3 | `AJUSTE` 2 | `Ajuste E2E v2 de inventario` | Saldo e histórico corretos |
| 40 | 2 | 0 | `AJUSTE` 2 | `Ajuste E2E v2 para zerar tamanho 40` | Saldo zerado e histórico correto |
| 39 | 3 | 3 | Nenhum | `Teste sem alteracao v2` | Rejeitado com “Não há alteração de saldo a ser registrada.” |

O vendedor não acessa `/Movimentacoes/Ajuste` (redirecionado para `AccessDenied`). Os ajustes ficaram registrados com o perfil LOJISTA e os motivos informados.

### 5.5 Consulta e venda (VENDEDOR)

- Busca com 1 caractere (`a`): “Digite pelo menos 2 caracteres para buscar.”
- Busca sem resultado (`zzz`): “Produto não encontrado.”
- Busca `Tênis`: 1 produto encontrado; a grade abriu com 37 (último par), 38 (3 pares), 39 (3 pares) e 40 (sem estoque).
- Venda 1 no 38: 3→2, `SAIDA` 1 vinculada ao atendimento `08041A65-DC67-4A91-BE13-096AB3AFBC7B` e ao vendedor. UI: `Venda registrada para o nº 38.` / `Saldo atualizado: 2 pares.`
- Venda 2 no 38: 2→1, `SAIDA` 1 vinculada ao atendimento `4A274547-7D6D-499D-8930-1E4E522EEFF1`. UI: `Venda registrada para o nº 38.` / `Saldo atualizado: 1 pares.`
- O 38 passou a ser exibido como “Último par”, com a classe `consulta-grade-item--ultimo-par`.
- **Ressalva cosmética:** a confirmação usa “1 pares” em vez de “1 par” (`DEF-S3QA030-02`). Não afeta saldo, movimentação ou autorização.

### 5.6 Ruptura (VENDEDOR)

- Três ações `Não tinha` no tamanho 40, que estava com saldo 0.
- Cada ação retornou `Falta registrada para o nº 40.` e `O saldo do sistema não foi alterado.`
- O saldo permaneceu 0 nas três vezes.
- Foram criadas 3 linhas em `Ruptura`, todas com SKU, vendedor, atendimento e data.
- Os 3 atendimentos foram concluídos como `NAO_TINHA`, com `SaldoConsultado=0`.
- Nenhuma movimentação de saída foi criada.

### 5.7 Desistiu e continuidade (VENDEDOR)

- Tamanho 39 selecionado com saldo 3; o atendimento `888E1B29-A2F0-494B-AACD-11BBEBB305C4` foi criado.
- A ação `Desistiu` concluiu o atendimento com `Resultado=DESISTIU` e `SaldoConsultado=3`.
- Nenhuma movimentação e nenhuma ruptura foram criadas; o saldo permaneceu 3.
- A UI exibiu `Atendimento encerrado.` / `Nenhuma movimentação foi registrada.`
- A nova tela de busca abriu sem bloqueio; uma nova consulta do mesmo produto abriu a grade novamente.

### 5.8 Saldos zerados e histórico de rupturas (LOJISTA)

- `/SaldosZerados` listou somente `Tênis E2E QA V2`, numeração 40. Os tamanhos 37 (1), 38 (1) e 39 (3) não apareceram.
- `/Rupturas` exibiu `Tênis E2E QA V2`, numeração 40, **3 ocorrências**, última em `04/11/2026 02:15` (UTC).
- `/Atendimentos` exibiu 0 pendentes, 6 concluídos e 4 alertas de encerramento em menos de 5 segundos. Os alertas decorrem da velocidade da execução assistida, não de comportamento incorreto do fluxo.
- `/Movimentacoes` exibiu 9 movimentações: 3 entradas iniciais, 1 entrada individual, 1 entrada em lote, 2 ajustes e 2 saídas.

### 5.9 Estado final do banco

| Entidade | Quantidade |
|---|---:|
| Produtos | 1 |
| SKUs | 4 |
| Movimentações | 9 |
| Atendimentos | 6 (todos concluídos) |
| Rupturas | 3 |
| Usuários | 2 |

Saldos finais: **37=1, 38=1, 39=3, 40=0**. Não existe SKU 45.

## 6. Defeitos

### DEF-S3QA030-01 — “Outras numerações” bloqueava a grade sem mensagem visível

| Campo | Valor |
|---|---|
| Severidade original | Alta / bloqueante para o E2E-02 |
| Área | Cadastro de produto / grade (LOJISTA) |
| Status | **Corrigido e validado nesta branch** |
| Correção | `ccd27fbb334d05a15064ed9f04f89b26506a2dd0` |

**Comportamento anterior (v1):** com 37–40 selecionados e “Outras numerações” vazio, nenhum POST era enviado, o campo recebia `input-validation-error` e não havia mensagem visível; o `POST` forçado retornava 200 sem salvar SKUs.

**Validação da correção (v2):**

- `required=false` no campo;
- `validationMessage` vazio e `form.checkValidity() === true`;
- `POST /Produtos/AdicionarNumeracoes/63a0854a-...` → **302**;
- 4 SKUs persistidos (37, 38, 39 e 40), sem numeração extra.

### DEF-S3QA030-02 — Texto de confirmação “1 pares”

| Campo | Valor |
|---|---|
| Severidade sugerida | Baixa / cosmética |
| Área | Consulta do vendedor / confirmação de venda |
| Status | **Permanece** |

Após vender o penúltimo par do tamanho 38, a UI exibiu `Saldo atualizado: 1 pares.`. O correto é `1 par`. Não afeta saldo, movimentação ou autorização.

### DEF-S3QA030-03 — Mensagem de validação em inglês na entrada

| Campo | Valor |
|---|---|
| Severidade sugerida | Baixa / localização |
| Área | Entrada de estoque (LOJISTA) |
| Status | **Não reproduzido nesta baseline** |

Com quantidade 0, a validação do cliente exibiu `O valor deve ser maior ou igual a 1.`, em português. A operação foi corretamente bloqueada.

## 7. Rastreabilidade das evidências

| Evidência | Conteúdo |
|---|---|
| [`evidencias/s3-qa-030/estado-final-banco.json`](../../evidencias/s3-qa-030/estado-final-banco.json) | Exportação de produtos, SKUs, movimentações, atendimentos, rupturas e perfis (sem senhas, hashes ou e-mails) |
| [`evidencias/s3-qa-030/evidencias-ui-e2e.md`](../../evidencias/s3-qa-030/evidencias-ui-e2e.md) | Snapshots de texto/DOM, URLs, respostas HTTP e detalhes da validação da correção |

Consultas de conferência usadas:

```sql
select Id, Nome, Marca, Categoria, Cor, Ativo from Produto;
select Id, ProdutoId, Numeracao, SaldoAtual, Ativo, UltimaAtualizacaoSaldoEm from Sku order by ProdutoId, Numeracao;
select Id, CriadoEm, Tipo, Quantidade, Motivo, SkuId, AtendimentoId, UsuarioId from Movimentacao order by CriadoEm;
select Id, Status, Resultado, SaldoConsultado, SkuId, CriadoEm, ConcluidoEm from Atendimento order by CriadoEm;
select Id, CriadoEm, SkuId, AtendimentoId, UsuarioId from Ruptura order by CriadoEm;
```

## 8. Limitações e pendências

- **Revisão por outro integrante:** pendente. Nenhum par revisou esta v2 até a data deste relatório. O PR deve permanecer sem aprovação final até essa conferência.
- **Capturas de tela:** não geradas nesta sessão; a evidência é textual/HTTP/banco.
- **Ambiente publicado:** não validado. Toda a execução foi local (Kestrel + SQLite).
- **Dispositivos móveis:** não validados nesta rodada; viewport única de 1000×700.
- **Alertas de atendimento:** 4 alertas de encerramento em menos de 5 segundos decorrem da automação assistida; não foram tratados como defeito.
- **Desempenho e concorrência:** fora do escopo deste cartão; não medidos.
- **Escopo de código:** a correção de `DEF-S3QA030-01` foi autorizada explicitamente para este branch, apesar da regra original de “não corrigir código dentro do cartão”. Nenhuma outra alteração de produto foi feita.

## 9. Checklist para revisão por outro integrante

- [ ] Conferir o diff do commit `ccd27fb` e confirmar que a única alteração de produto é a nulabilidade de `NumeracoesExtras`.
- [ ] Executar `dotnet test` e confirmar o teste `Lojista_saves_official_grade_when_extras_field_is_submitted_empty`.
- [ ] Reexecutar o E2E-02 em banco limpo com 37–40, “Outras numerações” vazio e sem workaround, confirmando POST 302 e 4 SKUs.
- [ ] Conferir no SQLite as 9 movimentações, os 6 atendimentos concluídos e as 3 rupturas do tamanho 40.
- [ ] Conferir os saldos finais 37=1, 38=1, 39=3 e 40=0, sem SKU 45.
- [ ] Repetir as rotas de autorização de VENDEDOR e LOJISTA e confirmar os `AccessDenied`.
- [ ] Confirmar que o relatório e as evidências não contêm senhas, hashes ou e-mails.
- [ ] Registrar nome e data da revisão no PR.

## 10. Conclusão

A regressão funcional manual cobriu os dois perfis e todos os fluxos pedidos: login, logout, autorização, produtos, grade, entrada, ajuste, consulta, venda, ruptura, `Desistiu`, saldos zerados e histórico de rupturas. Na baseline `9111713` com a correção `ccd27fb`, os 7 casos oficiais passaram. O E2E-02 foi concluído com a grade oficial 37–40, sem workaround, e o campo “Outras numerações” permaneceu opcional e vazio.

O resultado **não declara aprovação formal do cartão**. Para encerrar, falta a conferência desta execução e do diff por outro integrante. O defeito cosmético `DEF-S3QA030-02` permanece registrado e não bloqueia o aceite.
