# S4-QA-019 — Massa controlada e estado final

## Produto fictício criado para a validação

| Campo | Valor |
|---|---|
| Nome | `QA S4 FINAL 2026-11-19` |
| ID | `026b3292-709d-4be9-b0be-aafeaa8783ae` |
| Marca | `Marca QA Final` |
| Categoria | `Tênis` |
| Cor | `Azul` |
| Finalidade | Massa exclusiva desta validação; não é dado real nem de cliente |

## SKUs

| Numeração | SKU ID | Saldo inicial | Saldo final | Observação |
|---:|---|---:|---:|---|
| 37 | `7022e46e-822f-4e18-9f55-2bd005a4bc23` | 1 | 3 | Recebeu `SAIDA`? Não. Recebeu `AJUSTE +2`. |
| 38 | `dd1ccaff-d73a-4c1b-8542-10e10c421a44` | 2 | 1 | Recebeu `SAIDA 1` na venda; estado final “Último par”. |
| 39 | `c0007e54-0de7-432e-862e-11af90f71cf7` | 0 | 0 | Registrou 1 ruptura; nenhuma movimentação. |

## Atendimentos

| Resultado | Numeração | Atendimento ID | Saldo consultado | Data/hora exibida |
|---|---:|---|---:|---|
| `VENDEU` | 38 | `0ed9a7f4-70d6-430b-bf52-53973b8c06a3` | 2 | 19/11/2026 17:00 |
| `NAO_TINHA` | 39 | Não capturado na listagem | 0 | 19/11/2026 17:01 |
| `DESISTIU` | 37 | `82b407ce-2da4-499d-9c0d-3f11ae845e9e` | 1 | 19/11/2026 17:01 |

## Movimentações persistidas para o produto controlado

| Data/hora | Tipo | Numeração | Quantidade | Responsável | Motivo |
|---|---|---:|---:|---|---|
| 19/11/2026 16:59:28 | `ENTRADA` | 37 | 1 | `Lojista Teste` | `Saldo inicial no cadastro da grade` |
| 19/11/2026 16:59:28 | `ENTRADA` | 38 | 2 | `Lojista Teste` | `Saldo inicial no cadastro da grade` |
| 19/11/2026 17:00:42 | `SAÍDA` | 38 | 1 | `Vendedor Teste` | `—` |
| 19/11/2026 17:03:58 | `AJUSTE` | 37 | 2 | `Lojista Teste` | `QA S4 FINAL - recontagem controlada 2026-11-19` |

Não foi criada movimentação para a numeração 39, coerente com o saldo inicial zero e com a regra de que `Não tinha` gera ruptura, não saída.

## Ruptura

| Produto | Numeração | Ocorrências | Última ruptura exibida |
|---|---:|---:|---|
| `QA S4 FINAL 2026-11-19` | 39 | 1 | 19/11/2026 17:01 (UTC) |

## Segurança dos dados

- Nenhuma senha, hash de senha, token antiforgery completo ou valor de cookie foi copiado para este pacote.
- As contas usadas foram as contas demonstrativas documentadas no projeto.
- Nenhum dado de cliente ou Estoque real foi alterado.
