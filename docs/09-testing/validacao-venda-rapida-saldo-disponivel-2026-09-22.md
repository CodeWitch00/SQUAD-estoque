# QA — Venda rápida com saldo disponível

## Escopo e implementação

Branch: `dev/emmy`. Base integrada: `716c582` (inclui S2-BE-007 e S2-BE-008).
A branch local `main` não foi alterada. Não houve alteração de código de produção.

Teste: `VendaRapidaHttpTests.Vendedor_sells_exactly_one_pair_without_changing_any_other_sku`.

- Uma nova `SquadEstoqueWebApplicationFactory` cria bancos SQLite em memória exclusivos desta execução.
- Prepara produto e SKU com identificadores fixos e saldo inicial de 5 pares; outro SKU do mesmo produto possui 7 pares. Os SKUs de outro produto, criados pela factory, também são comparados.
- Autentica `VENDEDOR` pelo formulário real de login, com cookies e antiforgery; obtém novo token após o login.
- Executa `POST /Estoque/Vender` enviando somente SKU e token.
- Confere HTTP 200, JSON, mensagem de sucesso, identificador do SKU e saldo 4.
- Consulta o banco em novo escopo, sem rastreamento: compara o conjunto completo de IDs e todos os campos escalares dos SKUs. Somente o saldo selecionado pode mudar, exatamente de 5 para 4.
- Confere uma única movimentação de saída, de quantidade 1, vinculada ao SKU e ao vendedor autenticado.

Não foram adicionados cenários de saldo zero, autorização ou concorrência.

## Evidências locais

Windows; SDK .NET 10.0.400; execução em 22/09/2026.

```powershell
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --filter FullyQualifiedName~VendaRapidaHttpTests --logger "trx;LogFileName=venda-rapida.trx"
# Aprovado: 1; falhas: 0; ignorados: 0.

dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj
# Compilação com êxito; 0 avisos; 0 erros.

dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --logger "trx;LogFileName=suite-venda-rapida.trx" --collect:"XPlat Code Coverage"
# Aprovados: 82; falhas: 0; ignorados: 0.

git diff --check
# Sem erros.
```

Os relatórios TRX estão em `tests/SquadEstoque.Web.Tests/TestResults/` (artefatos locais ignorados pelo Git).
O teste passou na primeira execução: a implementação existente já atende ao cenário. Não se aplica evidência de falha anterior a uma correção; não houve correção de produção neste cartão.

## Verificação manual e pendências

No endereço Azure informado, o login de vendedor foi realizado e a consulta por `Tênis` retornou `Tênis run`. A grade exibiu tamanho 37 com saldo 0 e não apresentou ação de venda rápida. Assim, login e consulta foram verificados, mas a venda manual pela interface permanece pendente. Nenhuma venda foi enviada ao banco compartilhado; a comprovação da venda com saldo positivo é do teste HTTP isolado.

Por orientação do solicitante, as evidências serão entregues na conversa e ele abrirá o Pull Request. Revisão por outro integrante, atualização do cartão e merge permanecem pendentes. Nenhum cartão histórico foi editado e o cartão não foi movido para Feito.
