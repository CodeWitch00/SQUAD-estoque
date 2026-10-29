# S2-BE-021 — Preservar saldo na ruptura

Validação realizada em 29/09/2026 na branch `dev/rayana`, sobre a base
`6cb134b`. Requisitos: RF-18, RN-05 e RN-06; casos de uso UC-05 e UC-S3.

## Regra validada

O `POST /Estoque/RegistrarNaoTinha` deve acrescentar exatamente uma ruptura
vinculada ao SKU escolhido e ao vendedor autenticado. A operação não pode
alterar o saldo de nenhum SKU nem criar, remover ou modificar movimentações de
entrada, saída ou ajuste.

O endpoint existente já atendia à regra: valida o SKU com uma consulta sem
rastreamento, cria somente a entidade `Ruptura` e persiste essa inclusão. Não
foi necessária alteração no código de produção, no schema ou nas migrations.

Foi identificada uma divergência documental anterior: o plano e a especificação
de testes ainda tratavam o fluxo HTTP de ruptura como dependente, embora o
endpoint e sua cobertura já estivessem implementados. Esta entrega registra o
estado real sem antecipar os cartões de autorização e entradas inválidas.

## Evidência automatizada

Teste:
`EstoqueControllerHttpTests.Registrar_nao_tinha_creates_rupture_without_changing_any_stock_or_movement`.

Cada execução cria um SKU próprio e captura, antes do POST:

- todos os SKUs, incluindo vínculo ao Produto, numeração, saldo e estado ativo;
- todas as movimentações e seus campos persistidos;
- todas as rupturas existentes.

Após o POST, o teste usa novos contextos sem rastreamento e comprova:

| Entrada | Resultado obtido |
|---|---|
| Saldo inicial 0 | Todos os saldos permaneceram iguais; nenhuma movimentação mudou; uma ruptura foi acrescentada |
| Saldo inicial 1 | Todos os saldos permaneceram iguais; nenhuma movimentação mudou; uma ruptura foi acrescentada |
| Saldo inicial 3 | Todos os saldos permaneceram iguais; nenhuma movimentação mudou; uma ruptura foi acrescentada |

A nova ruptura possui o SKU esperado, o usuário obtido da sessão autenticada e
data UTC dentro da janela do POST. A comparação da tabela inteira impede que uma
movimentação indevida em outro SKU passe despercebida.

## Execução

Ambiente: Linux, .NET 10, `WebApplicationFactory` e SQLite relacional em memória.

```sh
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj \
  --configuration Release --no-restore \
  --filter FullyQualifiedName~Registrar_nao_tinha_creates_rupture_without_changing_any_stock_or_movement
```

Resultado focado: **3 aprovados, 0 falhas e 0 ignorados**.

Suíte completa no mesmo estado da branch: **94 aprovados, 0 falhas e 0
ignorados**. A compilação de aplicação e testes ocorreu durante `dotnet test` e
foi aprovada. `git diff --check` não encontrou erros.

A [validação manual da ação Não tinha](validacao-nao-tinha-2026-09-28.md)
complementa a prova com navegador real, saldos 0, 1 e 3, capturas mobile e
desktop e inspeção do banco. Ela também registrou saldo preservado e nenhuma
movimentação criada.

## Limites

Este cartão não altera autorização, tratamento de SKU inválido, concorrência ou
relatório de rupturas. Esses comportamentos possuem cartões e testes próprios.
