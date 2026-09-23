# Validação da venda rápida com saldo disponível — 22/09/2026

## Escopo

Cartão S2-QA-011: comprovar que a venda rápida de um SKU com saldo disponível reduz seu estoque em exatamente um par, sem alterar outros SKUs.

Rota validada: `POST /Estoque/Vender`, como `VENDEDOR` autenticado.

## Resultado

| Cenário | Resultado esperado | Situação |
| --- | --- | --- |
| Venda do SKU selecionado, com saldo inicial de 5 pares | Resposta de sucesso e saldo final de 4 pares | Aprovado |
| Demais SKUs, inclusive de outro produto | Permanecem sem alterações | Aprovado |
| Movimentação de saída | Um registro de 1 par, vinculado ao SKU e ao vendedor | Aprovado |

## Evidência automatizada

O teste de integração `VendaRapidaHttpTests.Vendedor_sells_exactly_one_pair_without_changing_any_other_sku` usa SQLite em memória isolado, login real com cookie e token antiforgery. O estado persistido é conferido em outro escopo, sem rastreamento do contexto usado na preparação dos dados.

- Teste do cartão: 1 aprovado, 0 falhas.
- Suíte completa: 82 aprovados, 0 falhas.
- Build: aprovado. `git diff --check`: sem erros.

O teste passou na primeira execução; não houve correção de código de produção neste cartão. Saldo zero, autorização e concorrência pertencem a outros cenários e não foram cobertos aqui.
