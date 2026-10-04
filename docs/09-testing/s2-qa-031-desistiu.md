# S2-QA-031 — Desistiu: encerrar atendimento e iniciar outra consulta

A cobertura está consolidada em `DesistiuHttpTests`, usando xUnit,
`SquadEstoqueWebApplicationFactory`, TestServer, autenticação por cookie e SQLite
em memória. Cada cenário usa um banco isolado, sem preparação manual.

## Fluxo validado

O vendedor abre a grade e envia um POST para `/Estoque/RegistrarDesistiu`,
com token antiforgery obtido da página. O servidor confirma o encerramento em
JSON e informa `novaConsultaUrl`. O teste acessa essa URL por HTTP, verifica
que a busca está vazia e realiza uma nova busca e seleção de outro produto
na mesma sessão autenticada.

Os seis cenários combinam saldos 0, 1 e 2 com envio ou ausência de `skuId`.
No JavaScript atual, o POST envia apenas o token; o envio opcional de SKU no
teste verifica que esse contexto adicional também não provoca operação de estoque.
Cada cenário repete o POST e confere:

- confirmação de encerramento e URL de retorno;
- preservação de todos os saldos e dos IDs de movimentações e rupturas após
  cada POST, após retornar à busca e após abrir outro produto;
- campo de busca vazio, sem grade nem ações do atendimento anterior;
- nova busca e seleção de outro produto, com a grade correspondente.

São preservados os testes existentes de autenticação do vendedor, bloqueio do
lojista, antiforgery e rejeição de GET no endpoint. A cobertura de
`Nova_consulta_sem_desfecho_retorna_a_busca_inicial_sem_persistir_resultado`
permanece em `ConsultaOperacionalHttpTests`: essa ação tem fluxo próprio.

## Execução

Na raiz do repositório, com SDK .NET 10:

```sh
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --configuration Release
```

Somente os testes HTTP de Desistiu:

```sh
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --configuration Release --filter FullyQualifiedName~DesistiuHttpTests
```

A suíte de frontend existente requer Node.js e Chromium:

```sh
npm ci
npx playwright install chromium
npm run test:frontend
```

## Limites da cobertura

TestServer exercita requisições HTTP, autenticação, renderização Razor e
persistência. Ele não executa JavaScript nem simula cliques, foco ou alterações
do DOM. Os testes de frontend existentes em
`tests/frontend/consulta-desistiu.test.cjs` exercitam o JavaScript em Chromium,
com HTML e respostas HTTP controlados pelo teste. A validação visual da página
completa continua sendo complementar.
