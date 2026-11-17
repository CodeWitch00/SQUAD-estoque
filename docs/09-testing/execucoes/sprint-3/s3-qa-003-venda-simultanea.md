# S3-QA-003 — Venda simultânea do último par

Validação em 11/10/2026 (America/Sao_Paulo), Windows, SDK .NET 10.0.401.
Base: `3609f8b`. Branch: `test/s3-qa-003-felipe`.

## Escopo e rastreabilidade

O teste `Concurrent_saidas_sell_available_stock_only_once`, em
`tests/SquadEstoque.Web.Tests/EstoqueDomainPersistenceTests.cs`, já cobria a
disputa entre duas vendas. Esta entrega amplia esse teste para dez repetições
independentes por cenário e registra as respostas e o estado persistido.
Atende ao cartão S3-QA-003, RF-11 e RN-07. A regra de produção não foi alterada.

Cada caso cria um SQLite em arquivo exclusivo, dois contextos com conexões
independentes e saldo inicial conferido por ambos. As duas tarefas sinalizam
prontidão e aguardam a mesma liberação antes de vender. Não há atraso artificial
usado como sincronização. Um terceiro contexto confere o resultado persistido.
O banco temporário e seus arquivos auxiliares são removidos ao encerrar o caso.

## Resultados locais

| Cenário | Repetições | Resultado |
| --- | ---: | --- |
| Duas vendas rápidas, saldo inicial um | 10 | Um sucesso, uma rejeição por saldo insuficiente, saldo zero e uma saída de um par em cada caso. |
| Duas saídas administrativas, saldo inicial um | 10 | Um sucesso, uma rejeição por saldo insuficiente, saldo zero e uma saída de um par em cada caso. |
| Duas saídas administrativas de três pares, saldo inicial três | 10 | Um sucesso, uma rejeição por saldo insuficiente, saldo zero e uma saída de três pares em cada caso. |

O teste rejeita duas confirmações, qualquer saldo diferente de zero, quantidade
ou identidade incorreta na movimentação e erros diferentes da rejeição esperada.
SKU, usuário e tipo SAIDA são verificados. Cada repetição aparece individualmente
no relatório TRX; sua saída inclui as duas respostas, saldo, contagem e dados da
movimentação. A ordem de quem vence não é predefinida.

| Verificação | Resultado |
| --- | --- |
| Suíte antes da alteração | 175 aprovados, zero falhas e zero ignorados. |
| Concorrência isolada | 30 aprovados, zero falhas e zero ignorados. |
| Build Release | Zero avisos e zero erros. |
| Suíte final Release | 202 aprovados, zero falhas e zero ignorados. |
| Cobertura de linhas | 54,43%, acima do mínimo de 50% do workflow. |
| `git diff --check` | Aprovado. |

## Reprodução

Na raiz do repositório, em Windows ou Ubuntu com SDK .NET 10:

```text
dotnet restore tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
dotnet build tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-restore --configuration Release
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-build --configuration Release --filter FullyQualifiedName~Concurrent_saidas --logger "trx;LogFileName=concorrencia.trx" --results-directory TestResults/s3-qa-003
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-build --configuration Release --collect:"XPlat Code Coverage" --logger "trx;LogFileName=final.trx" --results-directory TestResults/s3-qa-003
```

As saídas individuais da execução final estão em
`s3-qa-003-resultados.txt`, neste diretório. TRX completo e cobertura ficam nos
artefatos locais da entrega, sem versionar bancos ou arquivos de compilação.

## Limites e conclusão

Este teste usa unidades de trabalho independentes diretamente na operação de
persistência, uma das alternativas permitidas pelo cartão. Não representa uma
validação manual em dois navegadores nem um teste de carga. Não houve alteração
de interface, schema, autenticação ou infraestrutura.

Revisão por outra pessoa, resultado do GitHub Actions e merge devem ser
conferidos no Pull Request. A execução local não comprova essas etapas e não
autoriza declarar o cartão concluído. O Trello não foi alterado.
