# S2-QA-031 — Desistiu sem registrar resultado

Cobertura em `ConsultaOperacionalHttpTests.Desistiu_retorna_a_consulta_preserva_estoque_e_permite_proximo_atendimento`.

O teste usa xUnit e a `SquadEstoqueWebApplicationFactory` existente, com
TestServer, autenticação por cookie e SQLite em memória. Não exige Chrome,
Playwright, Node, Python, servidor externo ou preparação manual de banco.

## Execução

Na raiz do repositório, com o SDK .NET 10:

```sh
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --configuration Release
```

Para executar somente os três cenários deste cartão:

```sh
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --configuration Release --filter FullyQualifiedName~Desistiu
```

## Critérios cobertos

Para saldos 0, 1 e 2, o vendedor autenticado abre um produto e sua grade.
O teste confere que a página oferece Desistiu habilitado e carrega o script
do atendimento. Verifica estaticamente que o ramo Desistiu do script servido
navega para `/Estoque/Consulta` e retorna imediatamente.

Em seguida, acessa esse destino pelo TestServer e verifica:

- resposta HTTP 200 com estado inicial e campo de busca vazio;
- ausência da grade e das ações do atendimento anterior;
- preservação de todos os saldos e dos identificadores de movimentações e rupturas;
- nova busca e seleção de outro produto na mesma sessão autenticada;
- preservação dos dados também após abrir o próximo atendimento.

## Limite da cobertura

TestServer não executa JavaScript. A verificação do ramo de navegação é um
contrato estático, sensível a refatorações do script, e não comprova a execução
do evento de clique, seleção de rádio ou alterações do DOM. A navegação HTTP,
autenticação, renderização Razor e persistência são exercitadas de verdade.
O clique real permanece como validação manual complementar; não há alegação
de teste de navegador ou garantia de ausência de POST durante um clique real.

## Particularidade existente no Windows

O teste anterior `Selected_product_shows_ordered_grade_with_balance_and_text_status`
compara um trecho de HTML com quebra de linha LF literal. Um checkout que
converte `Consulta.cshtml` para CRLF pode falhar nessa comparação, independentemente
do fluxo Desistiu. A validação local utilizou nessa página as quebras LF do
conteúdo versionado, sem alterar seu conteúdo no PR. Os novos testes usam
expressões que aceitam ambos os formatos.
