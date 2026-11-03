# Relatório de execução E2E — S3-QA-030

**Cartão:** S3-QA-030 — Executar regressão funcional: vendedor e lojista ponta a ponta
**Sprint:** Sprint 3
**Prazo formal:** 11/11/2026 23:59 (America/Sao_Paulo)
**Reunião-alvo:** 02/11/2026
**Execução:** 02/11/2026 (fluxo principal) e 03/11/2026 (reteste de continuidade)
**Responsável pela execução:** agente de QA (a revisão por outro integrante permanece pendente)
**Alteração de código:** nenhuma. Validação somente leitura sobre a baseline.

## 1. Baseline e ambiente

| Item | Valor |
|---|---|
| Repositório | `CodeWitch00/SQUAD-estoque` |
| Branch | `test/s3-qa-009-integrante` |
| Commit | `2e116c5041dec61f3f868b77bfddc1692c0d745b` (30/10/2026) |
| Sistema operacional | Windows NT 10.0.26200.0 |
| SDK | .NET SDK 10.0.403, aplicação `net10.0` |
| Servidor | Kestrel HTTP em `http://localhost:5186` |
| Banco | SQLite descartável `src/SquadEstoque.Web/Estoque.db` |
| Configuração | `ASPNETCORE_ENVIRONMENT=Development`, `Demo__SeedUsers=true` |
| Navegador | OpenCode 2.2.5 / Chrome 152.0.7977.167 / Electron 44.9.1, viewport 1000×700 |
| Fuso | `America/Sao_Paulo` |
| Credenciais | `lojista@squad.com` / `123` e `vendedor@squad.com` / `123` (seed de desenvolvimento) |

A branch foi escolhida por já conter os recursos de saldos zerados, histórico de rupturas e ajustes de p95/índices. A `main` disponível no clone estava atrás desse estado.

## 2. Método e evidências

- Execução manual assistida pelo navegador, com inspeção de DOM, URLs, requisições HTTP e consultas diretas ao SQLite.
- Cada operação de estoque foi conferida antes/depois no banco, além da resposta visível na tela.
- Capturas de tela não puderam ser geradas nesta sessão: `browser.screenshot` retorna “Screenshot needs a visible tab”. Foram usados snapshots de texto/DOM, URLs, códigos HTTP e exportação do banco.
- Massa e estado final exportados em [`evidencias/s3-qa-030/estado-final-banco.json`](../../evidencias/s3-qa-030/estado-final-banco.json).
- Snapshots de UI e detalhes de reprodução em [`evidencias/s3-qa-030/evidencias-ui-e2e.md`](../../evidencias/s3-qa-030/evidencias-ui-e2e.md).

Comandos usados para subir a aplicação:

```powershell
cd src/SquadEstoque.Web
$env:ASPNETCORE_ENVIRONMENT='Development'
$env:Demo__SeedUsers='true'
dotnet run --no-build --urls http://localhost:5186
```

## 3. Resumo dos casos oficiais

| ID | Cenário | Resultado esperado | Resultado obtido | Status de execução | Evidência |
|---|---|---|---|---|---|
| E2E-01 | Login e logout de lojista | Home do perfil; área autorizada; após sair, rota protegida pede login | Login levou a `/Produtos`; logout levou a `/Account/Login`; rotas anônimas redirecionaram com `ReturnUrl` | **Aprovado** | URLs e snapshots em `evidencias-ui-e2e.md` §§1 |
| E2E-02 | Cadastro até entrada inicial | Produto e SKUs visíveis; saldos e movimentações corretos | Produto criado, mas a grade não pôde ser salva com “Outras numerações” vazio; a reprodução controlada com 37–40 no produto de defeito também falhou. Com workaround (38, 39, 40 e 45), produto e SKUs ficaram visíveis e entradas/saldos corretos | **Reprovado** | Defeito `DEF-S3QA030-01`; `estado-final-banco.json` |
| E2E-03 | Consulta e venda | Grade aparece; saldo reduz em 1; confirmação visível | Grade abriu; duas vendas reduziram 38 de 3→2→1; confirmação visível; ressalva cosmética “1 pares” | **Aprovado** | `evidencias-ui-e2e.md` §§7–8 |
| E2E-04 | Consulta e ruptura | Ruptura registrada; saldo não muda | 3 ações `Não tinha` no 40: saldo permaneceu 0, 3 rupturas e nenhuma saída | **Aprovado** | `evidencias-ui-e2e.md` §§9; `estado-final-banco.json` |
| E2E-05 | Desistência e continuidade | Nenhum efeito no estoque; uso continua sem bloqueio | `Desistiu` no 39 concluiu sem movimentação/ruptura; retorno à busca; nova consulta abriu a grade | **Aprovado** | `evidencias-ui-e2e.md` §§10 |
| E2E-06 | Ajuste administrativo | Novo saldo e histórico rastreável; vendedor não acessa | Ajustes 1→3 e 2→0 com motivo e responsável; vendedor recebeu `AccessDenied` em `/Movimentacoes/Ajuste` | **Aprovado** | `evidencias-ui-e2e.md` §§3 e 11 |
| E2E-07 | Decisão de reposição | Agrupamentos e frequências correspondem à massa | `/SaldosZerados` listou somente 40 e 45; `/Rupturas` agrupou 40 com 3 ocorrências | **Aprovado** | `evidencias-ui-e2e.md` §§4–5 |

**Conclusão dos casos:** 6 aprovados e 1 reprovado (E2E-02). O aceite formal **não está aprovado** enquanto o defeito de grade não for corrigido, a execução não for revisada por outro integrante e o cenário oficial 37–40 não for reexecutado sem workaround.

## 4. Detalhamento por fluxo

### 4.1 Login, logout e autorização

| Perfil | Ação | Esperado | Obtido | Status |
|---|---|---|---|---|
| LOJISTA | Login válido | Entrar na área do lojista | `/Produtos` | Aprovado |
| LOJISTA | Logout | Encerrar sessão | `/Account/Login` | Aprovado |
| Anônimo | Abrir `/Produtos` | Pedir login com retorno | `/Account/Login?ReturnUrl=%2FProdutos` | Aprovado |
| Anônimo | Abrir `/Estoque/Consulta` | Pedir login com retorno | `/Account/Login?ReturnUrl=%2FEstoque%2FConsulta` | Aprovado |
| Anônimo | Abrir `/Movimentacoes/Entrada` | Pedir login com retorno | `/Account/Login?ReturnUrl=%2FMovimentacoes%2FEntrada` | Aprovado |
| VENDEDOR | Abrir `/Produtos`, `/Movimentacoes`, `/Movimentacoes/Entrada`, `/Movimentacoes/Ajuste`, `/SaldosZerados`, `/Rupturas`, `/Atendimentos` | Acesso negado | Todas → `/Account/AccessDenied` com “Acesso Não Autorizado” | Aprovado |
| LOJISTA | Abrir `/Estoque/Consulta` | Acesso negado | `/Account/AccessDenied?ReturnUrl=%2FEstoque%2FConsulta` | Aprovado |

### 4.2 Produto, grade e entrada inicial (LOJISTA)

**Produto usado no fluxo principal:** `Tênis E2E QA` (`67E39FA5-859A-4281-ABB4-A7292C71654F`), marca `Marca QA`, categoria `Tênis`, cor `Azul`.

**Esperado:** criar produto, informar grade 37–40, salvar e registrar entrada inicial.

**Obtido:**

1. O produto foi criado e apareceu em `/Produtos`.
2. A primeira tentativa de salvar a grade com os tamanhos selecionados e “Outras numerações” vazio **não salvou nenhum SKU** e não exibiu mensagem de erro visível. A reprodução controlada com 37, 38, 39 e 40 foi feita no produto `Produto Defeito Grade` e confirmou o mesmo comportamento.
3. Para obter cobertura, foi informada a numeração extra `45`. A grade foi salva com 38 (saldo 2), 39 (saldo 1), 40 (saldo 0) e 45 (saldo 0). O tamanho 37 não foi persistido; o 45 foi uma inclusão artificial.
4. As movimentações de saldo inicial foram criadas: `ENTRADA` 2 no 38 e `ENTRADA` 1 no 39, motivo “Saldo inicial no cadastro da grade”.
5. Entrada individual no 38: 2→3, `ENTRADA` 1, motivo `Entrada E2E`.
6. Entrada em lote no 40: 0→2, `ENTRADA` 2, motivo `Entrada lote E2E`.
7. Entrada com quantidade 0 foi rejeitada no cliente com a mensagem em inglês `Please enter a value greater than or equal to 1.`

**Desvio registrado:** o caso oficial pede grade 37–40. A execução usou 38, 39, 40 e 45 por causa do defeito `DEF-S3QA030-01`. O E2E-02 não pode ser considerado aprovado nessa condição.

### 4.3 Ajuste administrativo (LOJISTA)

| SKU | Antes | Novo saldo | Movimento | Motivo | Resultado |
|---|---:|---:|---|---|---|
| 39 | 1 | 3 | `AJUSTE` 2 | `Ajuste E2E de inventario` | Saldo e histórico corretos |
| 40 | 2 | 0 | `AJUSTE` 2 | `Ajuste E2E para zerar tamanho 40` | Saldo zerado e histórico correto |
| 39 | 3 | 3 | Nenhum | — | Rejeitado com “Não há alteração de saldo a ser registrada.” |

O vendedor não acessa `/Movimentacoes/Ajuste` (redirecionado para `AccessDenied`).

### 4.4 Consulta e venda (VENDEDOR)

- Busca `Tênis` e `tenis` retornou o produto.
- Busca com 1 caractere (`a`) exibiu “Digite pelo menos 2 caracteres para buscar.”
- Busca sem resultado (`zzz`) exibiu “Produto não encontrado.”
- Grade final: 38 com 1 par (“Último par”), 39 com 3 pares (“Disponível”), 40 e 45 sem estoque (“Indisponível”).
- Venda 1 no 38: 3→2, `SAIDA` 1 vinculada ao atendimento e ao vendedor.
- Venda 2 no 38: 2→1, `SAIDA` 1; UI exibiu `Saldo atualizado: 1 pares` e aplicou a classe `consulta-grade-item--ultimo-par`.
- **Ressalva cosmética:** a confirmação usa “1 pares” em vez de “1 par” (`DEF-S3QA030-02`).

### 4.5 Ruptura (VENDEDOR)

- Três ações `Não tinha` no tamanho 40, que estava com saldo 0.
- Saldo permaneceu 0 nas três vezes.
- Foram criadas 3 linhas em `Ruptura`, todas com SKU, vendedor, atendimento e data.
- Os 3 atendimentos foram concluídos como `NAO_TINHA`, com `SaldoConsultado=0`.
- Nenhuma movimentação de saída foi criada.

### 4.6 Desistiu e continuidade (VENDEDOR)

- Tamanho 39 selecionado com saldo 3.
- Ação `Desistiu` concluiu o atendimento `A20F8EAF-824A-4C89-AB99-79E9218A8C6B` com `Resultado=DESISTIU`, `SaldoConsultado=3`.
- Nenhuma movimentação e nenhuma ruptura foram criadas; saldo permaneceu 3.
- A UI retornou à busca com “Atendimento encerrado. Nenhuma movimentação foi registrada.”
- Nova busca `Tênis` retornou o produto e a grade abriu novamente, sem bloqueio.

### 4.7 Saldos zerados e histórico de rupturas (LOJISTA)

- `/SaldosZerados` listou somente `Tênis E2E QA` com os tamanhos 40 e 45. Os tamanhos 38 (saldo 1) e 39 (saldo 3) não apareceram.
- O tamanho 45 apareceu com “Atualização não registrada”, coerente com o SKU criado sem saldo inicial.
- `/Rupturas` exibiu `Tênis E2E QA`, numeração 40, 3 ocorrências, última em `02/11/2026 21:45` (UTC).
- `/Atendimentos` exibiu 0 pendentes, 7 concluídos e 4 alertas de encerramento em menos de 5 segundos.

### 4.8 Estado final do banco

| Entidade | Quantidade |
|---|---:|
| Produtos | 2 |
| SKUs | 4 |
| Movimentações | 8 |
| Atendimentos | 7 (todos concluídos) |
| Rupturas | 3 |
| Usuários | 2 |

Saldos finais: **38=1, 39=3, 40=0, 45=0**.

## 5. Defeitos reproduzíveis

### DEF-S3QA030-01 — Grade bloqueada por “Outras numerações” obrigatória sem mensagem visível

| Campo | Valor |
|---|---|
| Severidade sugerida | Alta / bloqueante para o E2E-02 |
| Área | Cadastro de produto / grade (LOJISTA) |
| Produto de reprodução | `Produto Defeito Grade` (`EE68C3AD-91ED-4A43-8179-273E4335A0B4`) |
| Status | Reproduzido em 02/11/2026 |

**Passos para reproduzir:**

1. Autenticar como LOJISTA.
2. Abrir `/Produtos/AdicionarNumeracoes/ee68c3ad-91ed-4a43-8179-273e4335a0b4?cadastroInicial=True`.
3. Marcar os tamanhos 37, 38, 39 e 40 com quantidade 1.
4. Deixar “Outras numerações” vazio.
5. Clicar em “Salvar grade”.

**Esperado:** a grade é salva com os tamanhos selecionados, pois o campo é apresentado como opcional (“Numerações extras são cadastradas com saldo zero”).

**Obtido:**

- Nenhuma requisição `POST` é enviada no fluxo normal.
- O campo recebe `class="form-control input-validation-error"`.
- O HTML contém `data-val-required="The Outras numerações field is required."` e `aria-describedby="NumeracoesExtras-error"`, mas o elemento `#NumeracoesExtras-error` **não existe**.
- Não há mensagem de erro visível nem resumo de validação.
- Nenhum SKU é salvo.

**Confirmação no servidor:** ao contornar a validação do cliente com `form.submit()` nativo, o `POST` retornou **HTTP 200**, re-renderizou a página, manteve o campo com erro e continuou sem mensagem visível. Consulta ao SQLite após o POST: `select ... from Sku where ProdutoId='ee68c3ad-...'` → **0 linhas**.

**Impacto:** o lojista não consegue concluir o cadastro da grade sem descobrir sozinho que precisa preencher um campo rotulado como opcional.

**Workaround usado:** informar uma numeração extra (45) para liberar o salvamento. Isso alterou a massa oficial e está registrado como desvio do E2E-02.

### DEF-S3QA030-02 — Texto de confirmação “1 pares”

| Campo | Valor |
|---|---|
| Severidade sugerida | Baixa / cosmética |
| Área | Consulta do vendedor / confirmação de venda |
| Status | Observado em 02/11/2026 |

Após vender o último par do tamanho 38, a UI exibiu `Saldo atualizado: 1 pares`. O correto é `1 par`. Não afeta saldo, movimentação ou autorização.

### DEF-S3QA030-03 — Mensagem de validação em inglês na entrada

| Campo | Valor |
|---|---|
| Severidade sugerida | Baixa / localização |
| Área | Entrada de estoque (LOJISTA) |
| Status | Observado em 02/11/2026 |

Com quantidade 0, a validação do cliente exibiu `Please enter a value greater than or equal to 1.` em vez de uma mensagem em português. A operação foi corretamente bloqueada; o defeito é de idioma.

## 6. Rastreabilidade das evidências

| Evidência | Conteúdo |
|---|---|
| [`evidencias/s3-qa-030/estado-final-banco.json`](../../evidencias/s3-qa-030/estado-final-banco.json) | Exportação de produtos, SKUs, movimentações, atendimentos, rupturas e usuários (sem hash de senha) |
| [`evidencias/s3-qa-030/evidencias-ui-e2e.md`](../../evidencias/s3-qa-030/evidencias-ui-e2e.md) | Snapshots de texto/DOM, URLs, respostas HTTP e detalhes da reprodução do defeito de grade |

Consultas de conferência usadas:

```sql
select Id, Nome, Marca, Categoria, Cor, Ativo from Produto;
select Id, ProdutoId, Numeracao, SaldoAtual, Ativo from Sku order by ProdutoId, Numeracao;
select Id, CriadoEm, Tipo, Quantidade, Motivo, SkuId, AtendimentoId, UsuarioId from Movimentacao order by CriadoEm;
select Id, Status, Resultado, SaldoConsultado, SkuId, CriadoEm, ConcluidoEm from Atendimento order by CriadoEm;
select Id, CriadoEm, SkuId, AtendimentoId, UsuarioId from Ruptura order by CriadoEm;
```

## 7. Limitações e pendências

- **Revisão por outro integrante:** pendente. Nenhum par revisou esta execução até a data deste relatório.
- **Aprovação formal:** não declarada. O E2E-02 está reprovado e o aceite depende da correção do defeito de grade e de nova execução sem workaround.
- **Capturas de tela:** não geradas nesta sessão; a evidência é textual/HTTP/banco.
- **Ambiente publicado:** não validado. Toda a execução foi local (Kestrel + SQLite).
- **Dispositivos móveis:** não validados nesta rodada; viewport única de 1000×700.
- **Massa oficial:** o E2E-02 usou 38, 39, 40 e 45 em vez de 37–40. Após a correção, reexecutar o caso com a grade oficial e sem numeração extra.
- **Desempenho e concorrência:** fora do escopo deste cartão; não medidos.
- **Registro consolidado:** a execução foi adicionada a `docs/09-testing/homologacoes/registro-validacoes-manuais.md`, sem alteração de código-fonte.

## 8. Checklist para revisão por outro integrante

- [ ] Reexecutar `DEF-S3QA030-01` em banco limpo, com 37–40 selecionados e “Outras numerações” vazio, confirmando ausência de `POST`, ausência de mensagem visível e zero SKUs.
- [ ] Conferir no SQLite as 8 movimentações, os 7 atendimentos concluídos e as 3 rupturas do tamanho 40.
- [ ] Conferir os saldos finais 38=1, 39=3, 40=0 e 45=0.
- [ ] Repetir as rotas de autorização de VENDEDOR e LOJISTA e confirmar os `AccessDenied`.
- [ ] Confirmar que nenhum arquivo do repositório foi alterado e que as evidências anexadas correspondem à execução.

## 9. Conclusão

A regressão funcional manual cobriu os dois perfis e os fluxos solicitados: login/logout, produtos, grade, entrada, ajuste, consulta, venda, ruptura, `Desistiu`, saldos zerados e histórico de rupturas. Seis dos sete casos oficiais passaram na execução local. O E2E-02 falhou por um defeito reproduzível no cadastro da grade: “Outras numerações” é obrigatório de fato, mas a tela não informa isso e não exibe erro visível.

O resultado **não representa aprovação do aceite**. Para encerrar o cartão como aprovado, é necessário:

1. corrigir `DEF-S3QA030-01` (e, se desejado, os defeitos cosméticos `02` e `03`);
2. reexecutar o E2E-02 com a grade oficial 37–40, sem workaround;
3. obter conferência dos defeitos e da execução por outro integrante;
4. anexar as evidências ao registro consolidado, se a equipe decidir manter documentação no repositório.
