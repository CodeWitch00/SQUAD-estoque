# S2-QA-013 - Autorização da venda rápida

Verificação em 22/09/2026. Sprint 2, prazo informado no cartão: 07/10/2026.
Base: `3a19c8223ce2aa94dc1453859f5e23664438f723` (main).
Branch solicitada: `test/s2-qa-013-felipe`.

## Resultado e escopo

O endpoint `POST /Estoque/Vender` está restrito ao perfil VENDEDOR.
Foram reforçados os dois casos de acesso negado já existentes em
`EstoqueControllerHttpTests.cs`, sem duplicar a cobertura de venda autorizada
nem alterar código de produção, permissões, migrations ou dependências.

| Perfil | Resultado esperado e verificado | Persistência |
| --- | --- | --- |
| VENDEDOR autenticado | HTTP 200, resposta JSON de sucesso | Reduz exatamente um par e registra SAIDA de quantidade 1 com o usuário autenticado; demais SKUs preservados. |
| LOJISTA autenticado | HTTP 302 para `/Account/AccessDenied?ReturnUrl=%2FEstoque%2FVender` | Todos os SKUs e todas as movimentações permanecem idênticos. |
| Anônimo | HTTP 302 para `/Account/Login?ReturnUrl=%2FEstoque%2FVender` | Todos os SKUs e todas as movimentações permanecem idênticos. |

## Evidência dos testes

- `EstoqueControllerHttpTests.Vendedor_can_post_venda_for_sku_and_records_authenticated_user`: cobertura existente de venda autorizada e autoria da movimentação.
- `VendaRapidaHttpTests.Vendedor_sells_exactly_one_pair_without_changing_any_other_sku`: cobertura existente de JSON, decremento unitário e preservação dos demais SKUs.
- `EstoqueControllerHttpTests.Vender_denies_unauthorized_access_without_changing_stock_or_movements`: dois casos reforçados nesta entrega, LOJISTA e anônimo.

Os casos negados fazem POST real pelo pipeline MVC, sem seguir redirecionamentos,
com SKU ativo de saldo 3 e token antiforgery emitido para a própria sessão.
O lojista efetua login real por cookie; o anônimo permanece sem autenticação.
O token é obtido em uma página acessível ao respectivo perfil. Não há simulação
da autorização nem remoção de filtros.

Antes da tentativa, o teste cria uma entrada histórica de quantidade 3.
Compara todos os campos persistidos dos SKUs e movimentações antes/depois,
incluindo IDs, saldos, quantidade, tipo, usuário, data e motivo. A leitura final
usa um novo contexto com `AsNoTracking`, para não depender do cache de entidades.
O banco SQLite de testes é isolado do banco local e do Azure.

## Coerência com os requisitos

RF-01 e RF-02 definem autenticação e distinção de perfis; RF-16 e RF-17 e o
UC-04 atribuem a venda rápida ao vendedor; RF-10 e US-04 requerem movimentação
de saída com usuário e data. O atributo `[Authorize(Roles = "VENDEDOR")]` do
`EstoqueController` implementa essa separação. A documentação consultada não
concede ao LOJISTA acesso à venda rápida operacional; a restrição observada é
coerente com o ator do UC-04. Isso não impede operações administrativas próprias
do lojista em outras rotas, que não foram alteradas.

## Validação local

Windows, SDK .NET 10.0.401, configuração Release, 22/09/2026:

- Baseline anterior à mudança: 82 aprovados, 0 falhas, 0 ignorados.
- Build da aplicação: aprovado, 0 erros.
- Build dos testes e suíte completa após a mudança: 82 aprovados, 0 falhas, 0 ignorados.
- `git diff --check`: aprovado.
- A contagem permanece 82 porque foram reforçados casos existentes.
- Aviso NU1900: a consulta de vulnerabilidades do NuGet ficou indisponível por restrição de rede local. Não é falha de compilação ou teste, e não equivale a uma auditoria de dependências aprovada.

Comandos reproduzíveis a partir da raiz:

```sh
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj --configuration Release
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --configuration Release
git diff --check
```

## Limites e rastreabilidade

As cinco capturas fornecidas são a referência do cartão; seus checklists não
foram interpretados como autorização para alterar o Trello. Nenhum cartão foi
aberto, editado ou movido. Não foi realizada homologação manual por navegador
nem teste no Azure; esta entrega verifica autorização HTTP e persistência de
forma automatizada. Revisão da mantenedora, merge e validação integrada continuam
sendo condições separadas para concluir o cartão.

A captura mostra etiqueta Sprint I, mas descrição Sprint 2 e prazo 07/10/2026;
esta evidência segue a descrição, sem corrigir o quadro. O CONTRIBUTING atual
orienta usar `dev/integrante`, enquanto a captura sugere branch por cartão.
A nova branch atende ao pedido explícito do usuário nesta execução. O relatório
registra a divergência sem alterar a política do repositório.

## Fontes

- `src/SquadEstoque.Web/Controllers/EstoqueController.cs`
- `tests/SquadEstoque.Web.Tests/EstoqueControllerHttpTests.cs`
- `tests/SquadEstoque.Web.Tests/VendaRapidaHttpTests.cs`
- `docs/02-requisitos/srs.md`: RF-01, RF-02, RF-10, RF-16 e RF-17
- `docs/02-requisitos/casos-de-uso.md`: UC-04
- `docs/02-requisitos/user-stories.md`: US-04
- `CONTRIBUTING.md` e `.github/workflows/dotnet.yml`
