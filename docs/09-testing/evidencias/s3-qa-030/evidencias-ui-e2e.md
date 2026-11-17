# Evidências de UI e banco — S3-QA-030

**Execução:** 02/11/2026 (fluxo principal), 03/11/2026 (reteste de continuidade do `Desistiu`) e 07/11/2026 (reteste do E2E-02 após a correção)
**Baseline:** `test/s3-qa-009-integrante` no commit `2e116c5041dec61f3f868b77bfddc1692c0d745b`; reteste do E2E-02 sobre `main` `748572f` + head `eb26f7d` do PR #95
**Ambiente:** Windows NT 10.0.26200.0, .NET SDK 10.0.403, aplicação `net10.0` em Kestrel HTTP `http://localhost:5186`, SQLite `Estoque.db` descartável, `ASPNETCORE_ENVIRONMENT=Development`, `Demo__SeedUsers=true`
**Navegador:** OpenCode 2.2.5 / Chrome 152.0.7977.167 / Electron 44.9.1, viewport 1000×700, fuso `America/Sao_Paulo`

> Capturas de tela não puderam ser geradas nesta sessão (`browser.screenshot` retorna "Screenshot needs a visible tab"). As evidências abaixo são snapshots de DOM/texto, URLs, requisições HTTP e consultas diretas ao SQLite. Nenhuma senha ou hash é reproduzido.

## 1. LOJISTA — login, logout e rotas protegidas

| Ação | Resultado observado |
|---|---|
| Login `[e-mail omitido]` | Redirecionou para `http://localhost:5186/Produtos` |
| Logout | Redirecionou para `http://localhost:5186/Account/Login` |
| Anônimo em `/Produtos` | `302` para `/Account/Login?ReturnUrl=%2FProdutos` |
| Anônimo em `/Estoque/Consulta` | `302` para `/Account/Login?ReturnUrl=%2FEstoque%2FConsulta` |
| Anônimo em `/Movimentacoes/Entrada` | `302` para `/Account/Login?ReturnUrl=%2FMovimentacoes%2FEntrada` |
| LOJISTA em `/Estoque/Consulta` | `/Account/AccessDenied?ReturnUrl=%2FEstoque%2FConsulta`, texto "Acesso Não Autorizado" |

## 2. LOJISTA — catálogo e grade

Texto observado em `/Produtos`:

```text
Mostrando 1–2 de 2 produtos
Produto Defeito Grade  QA  Teste  Preto  (grade vazia)  Ativo
Tênis E2E QA          Marca QA  Tênis  Azul  38 39 40 45  Ativo
```

O produto `Produto Defeito Grade` permaneceu sem grade após a tentativa de cadastro com "Outras numerações" vazio.

## 3. LOJISTA — movimentações

Texto observado em `/Movimentacoes` (8 registros):

```text
Mostrando 1–8 de 8 movimentações
02/11/2026 18:44:34  Tênis E2E QA  38  SAÍDA   1  Vendedor Teste  —
02/11/2026 18:43:59  Tênis E2E QA  38  SAÍDA   1  Vendedor Teste  —
02/11/2026 18:42:50  Tênis E2E QA  40  AJUSTE  2  Lojista Teste   Ajuste E2E para zerar tamanho 40
02/11/2026 18:42:36  Tênis E2E QA  39  AJUSTE  2  Lojista Teste   Ajuste E2E de inventario
02/11/2026 18:42:23  Tênis E2E QA  40  ENTRADA 2  Lojista Teste   Entrada lote E2E
02/11/2026 18:42:07  Tênis E2E QA  38  ENTRADA 1  Lojista Teste   Entrada E2E
02/11/2026 18:41:37  Tênis E2E QA  39  ENTRADA 1  Lojista Teste   Saldo inicial no cadastro da grade
02/11/2026 18:41:37  Tênis E2E QA  38  ENTRADA 2  Lojista Teste   Saldo inicial no cadastro da grade
```

## 4. LOJISTA — saldos zerados

Texto observado em `/SaldosZerados`:

```text
Tênis E2E QA
Marca QA · Azul
Numerações sem saldo:
40   Atualizado em 02/11/2026 18:42
45   Atualização não registrada
```

Somente os SKUs com saldo 0 foram listados. Os tamanhos 38 (saldo 1) e 39 (saldo 3) não apareceram.

## 5. LOJISTA — histórico de rupturas

Texto observado em `/Rupturas`:

```text
Histórico de rupturas por modelo e numeração. Datas e horários em UTC.
Modelo          Numeração  Ocorrências  Última ruptura (UTC)
Tênis E2E QA    40         3            02/11/2026 21:45
```

As 3 rupturas do tamanho 40 foram agrupadas corretamente e o saldo permaneceu 0.

## 6. LOJISTA — atendimentos

Texto observado em `/Atendimentos` (após o reteste de 03/11):

```text
Pendentes 0
Concluídos 7
Alertas para revisar 4
```

Resultados observados: 2 `Vendeu`, 3 `Não tinha`, 2 `Desistiu`. Os 4 alertas são encerramentos em menos de 5 segundos, incluindo as 3 rupturas e o `Desistiu`.

## 7. VENDEDOR — consulta e estados da grade

Busca `Tênis` em `/Estoque/Consulta` retornou 1 produto. Grade final observada:

```text
Disponíveis 2
Nº 38  Último par   Atualizado em 02/11/2026 18:44  STATUS: ÚLTIMO PAR
Nº 39  3 pares      Atualizado em 02/11/2026 18:42  STATUS: DISPONÍVEL

Indisponíveis 2
Nº 40  Sem estoque  Atualizado em 02/11/2026 18:42  STATUS: INDISPONÍVEL
Nº 45  Sem estoque  Atualização não registrada      STATUS: INDISPONÍVEL
```

Classes CSS observadas: `consulta-grade-item--ultimo-par`, `consulta-grade-item--disponivel`, `consulta-grade-item--indisponivel` (duas ocorrências).

## 8. VENDEDOR — venda

Duas vendas no tamanho 38:

| Antes | Depois | Movimento | Atendimento |
|---:|---:|---|---|
| 3 | 2 | `SAIDA` 1, vendedor, atendimento `E9698AAB-DF5A-4A32-B8E4-F88BA43285F0` | `VENDEU`, saldo consultado 3 |
| 2 | 1 | `SAIDA` 1, vendedor, atendimento `ECD6BD96-E652-4D7D-8A91-86708AFD65AA` | `VENDEU`, saldo consultado 2 |

A UI exibiu `Saldo atualizado: 1 pares` e aplicou `consulta-grade-item--ultimo-par`. Foi registrado o defeito cosmético de pluralização (`1 pares` em vez de `1 par`).

## 9. VENDEDOR — ruptura

Três ações `Não tinha` no tamanho 40 (saldo 0):

- saldo permaneceu 0;
- 3 linhas em `Ruptura`, todas vinculadas ao vendedor e a um atendimento;
- 3 atendimentos concluídos com `NAO_TINHA` e `SaldoConsultado=0`;
- nenhuma movimentação de saída foi criada.

## 10. VENDEDOR — Desistiu e continuidade

Reteste em 03/11/2026:

1. Busca `Tênis`, seleção do produto e do tamanho 39 (saldo 3).
2. Ação `Desistiu`.
3. UI retornou à busca com a mensagem `Atendimento encerrado. Nenhuma movimentação foi registrada.`
4. Atendimento `A20F8EAF-824A-4C89-AB99-79E9218A8C6B` concluído com `Resultado=DESISTIU`, `SaldoConsultado=3`; nenhuma movimentação e nenhuma ruptura.
5. Nova busca `Tênis` retornou o produto e a grade abriu novamente, sem bloqueio.

## 11. VENDEDOR — autorização

Todas as rotas abaixo responderam com `/Account/AccessDenied` e o texto "Acesso Não Autorizado":

```text
/Produtos
/Movimentacoes
/Movimentacoes/Entrada
/Movimentacoes/Ajuste
/SaldosZerados
/Rupturas
/Atendimentos
```

## 12. Reteste do E2E-02 após a correção (07/11/2026)

**Baseline:** `main` `748572f` + head `eb26f7d` do PR #95, banco SQLite recriado.
**Produto:** `Tênis E2E QA Reteste` (`68590ED9-7869-4DD1-8F62-32C39CF5F825`), marca `Marca QA`, categoria `Tênis`, cor `Azul`.

| Passo | Resultado observado |
|---|---|
| Tela de grade com “Outras numerações” vazio | `class="form-control"`, sem `data-val-required`, sem `aria-describedby` e sem `#NumeracoesExtras-error` |
| `POST /Produtos/AdicionarNumeracoes/68590ed9-...` | Redirecionou para `/Produtos/Details/68590ed9-...`; grade salva sem workaround |
| SKUs criados | 37=1, 38=2, 39=1, 40=0 |
| Entrada individual no 38 | 2→3 (`ENTRADA` 1, `Entrada E2E reteste`) |
| Entrada em lote no 40 | 0→2 (`ENTRADA` 2, `Entrada lote E2E reteste`) |
| Estado final | 37=1, 38=3, 39=1, 40=2; 5 movimentações; 0 atendimentos; 0 rupturas |

Texto observado em `/Movimentacoes`:

```text
Mostrando 1–5 de 5 movimentações
07/11/2026 12:55:39  Tênis E2E QA Reteste  40  ENTRADA  2  Lojista Teste  Entrada lote E2E reteste
07/11/2026 12:55:32  Tênis E2E QA Reteste  38  ENTRADA  1  Lojista Teste  Entrada E2E reteste
07/11/2026 12:55:22  Tênis E2E QA Reteste  39  ENTRADA  1  Lojista Teste  Saldo inicial no cadastro da grade
07/11/2026 12:55:22  Tênis E2E QA Reteste  38  ENTRADA  2  Lojista Teste  Saldo inicial no cadastro da grade
07/11/2026 12:55:22  Tênis E2E QA Reteste  37  ENTRADA  1  Lojista Teste  Saldo inicial no cadastro da grade
```

Estado exportado em `estado-final-banco-reexecucao-e2e02.json`.

**Diferença em relação ao defeito:** o campo não é mais marcado como obrigatório e o `POST` não é bloqueado. O tamanho 37, que antes não era persistido, passou a ser gravado.

## 13. Defeito reproduzível — grade exige "Outras numerações" sem indicar isso

**Produto:** `Produto Defeito Grade` (`EE68C3AD-91ED-4A43-8179-273E4335A0B4`)

**Passos:**

1. LOJISTA abre `/Produtos/AdicionarNumeracoes/ee68c3ad-91ed-4a43-8179-273e4335a0b4?cadastroInicial=True`.
2. Marca os tamanhos 37, 38, 39 e 40, com quantidade 1 em cada.
3. Deixa "Outras numerações" vazio, conforme o texto de apoio "Numerações extras são cadastradas com saldo zero."
4. Clica em "Salvar grade".

**Resultado observado no fluxo normal:**

- Nenhuma requisição `POST` foi enviada.
- O campo recebeu `class="form-control input-validation-error"`.
- O campo tem `data-val-required="The Outras numerações field is required."`.
- `aria-describedby="NumeracoesExtras-error"`, mas **não existe** o elemento `#NumeracoesExtras-error`.
- Nenhuma mensagem de erro visível, nenhum resumo de validação.
- Nenhum SKU foi salvo.

**Resultado observado ao contornar a validação do cliente** (`form.submit()` nativo):

- `POST /Produtos/AdicionarNumeracoes/ee68c3ad-91ed-4a43-8179-273e4335a0b4` → **HTTP 200**, HTML re-renderizado.
- Corpo da requisição continha `Opcoes[3..6].Selecionada=true` e `NumeracoesExtras=`.
- A resposta manteve `input-validation-error` no campo e nenhuma mensagem visível.
- Consulta ao banco: `select ... from Sku where ProdutoId='ee68c3ad-...'` → **0 linhas**.

**Conclusão:** o campo "Outras numerações" é obrigatório de fato, embora a tela o apresente como opcional e não exiba a mensagem. O cadastro da grade fica bloqueado sem feedback compreensível. Foi necessário informar uma numeração extra (45) para concluir a grade do produto `Tênis E2E QA`.

## 14. Estado final do banco

Exportado em `estado-final-banco.json`:

| Entidade | Quantidade |
|---|---:|
| Produtos | 2 |
| SKUs | 4 |
| Movimentações | 8 |
| Atendimentos | 7 (todos concluídos) |
| Rupturas | 3 |
| Usuários | 2 (`[e-mail omitido]`, `[e-mail omitido]`) |

Saldos finais: 38=1, 39=3, 40=0, 45=0.

## 15. Limitações

- Sem capturas de tela; evidência por DOM, URL, HTTP e SQLite.
- Sem ambiente publicado/homologação Azure.
- Sem aparelho físico ou viewports móveis nesta rodada.
- O caso E2E-02 foi executado com desvio na rodada original: grade 38/39/40 + 45, não 37–40, por causa do defeito acima. O reteste de 07/11/2026 foi feito na grade oficial, sem desvio.
- Revisão por outro integrante e aprovação formal permanecem pendentes.
