# S2-QA-024 — Saldo preservado após ruptura

Validação em 30/09/2026, branch `dev/emmy`, base `fb52730`, Windows e SDK
.NET 10.0.400. Requisitos: RF-18, RN-05 e RN-06; cenário VEN-RUP-01.
Dependência funcional S2-BE-021 já integrada na base.

## Teste de integração

`RupturaHttpTests.Nao_tinha_preserves_positive_balance_and_persists_rupture_without_movement`
usa uma `WebApplicationFactory` própria com SQLite relacional em memória.
Complementa a cobertura compartilhada de `EstoqueControllerHttpTests` com
um cenário independente, iniciado sem movimentações nem rupturas.

1. Persiste um produto/SKU ativo com saldo conhecido de 5 pares.
2. Confere o perfil VENDEDOR e autentica pelo login HTTP real, com cookies e antiforgery.
3. Envia apenas SKU e token ao `POST /Estoque/RegistrarNaoTinha` e exige HTTP 200.
4. Recarrega o SKU usando novo escopo de DI e `AsNoTracking`.
5. Exige saldo final igual ao inicial, tabela de movimentações vazia e exatamente
   uma ruptura persistida, vinculada ao SKU e vendedor, com ID válido e data do POST.

Não há chamada de venda, alteração no código de produção ou migration.

## Resultados locais

| Verificação | Resultado |
| --- | --- |
| `dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj` | Aprovado; 0 avisos, 0 erros |
| `dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --filter FullyQualifiedName~RupturaHttpTests` | 1 aprovado, 0 falhas, 0 ignorados |
| `dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj` | 95 aprovados, 0 falhas, 0 ignorados |

Revisão local: cenário isolado, autenticação efetiva, leitura persistida e
asserções sobre tabelas inteiras conferidas. O `git diff --check` global apontou
somente espaços preexistentes em três relatórios locais de venda, fora do cartão.
Esses arquivos foram preservados e não integram o commit.

O cartão especifica validação automatizada; não define roteiro manual adicional.
Não foi repetida validação de interface nesta entrega de testes. A
[evidência manual anterior](validacao-nao-tinha-2026-09-28.md) documenta o fluxo
no navegador e a inspeção do banco, sem constituir nova execução deste cartão.

## Entrega e pendências externas

A branch foi atualizada por fast-forward de `origin/main`, preservando a `main`
local e as alterações locais anteriores. Não houve switch/pull na `main` devido
à árvore suja e à instrução de não alterá-la.

O PR deve referenciar S2-QA-024 e solicitar revisão de `CodeWitch00`.
A aprovação humana é externa a esta validação. O link do cartão foi solicitado
para vincular o PR e registrar evidências no Trello; os cartões históricos
protegidos não foram editados. O resultado do CI será registrado no PR.
