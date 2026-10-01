# S2-QA-031 - resultado Desistiu

Teste automatizado de integração em Chrome. Ele autentica um VENDEDOR, inicia
uma busca com saldo conhecido, seleciona **Desistiu**, confirma a volta à
consulta vazia sem POST e verifica que saldo, movimentações e rupturas do SKU
isolado não mudaram. Uma releitura do SKU confirma o saldo original.

Use somente um banco local de teste. Em `Development`, configure caminhos
descartáveis para `ConnectionStrings__EstoqueContext` e
`ConnectionStrings__LegacyMovieContext`, inicie a aplicação, e habilite
`Demo__SeedUsers=true` somente no ambiente local. Após as migrations criarem o
banco, execute:

```text
python tests/browser/seed-desistiu.py CAMINHO_DO_BANCO_DESCARTAVEL
```

Com Playwright e Chrome disponíveis, execute:

```text
node tests/browser/desistiu-no-result.cjs
```

Variáveis opcionais: `TEST_BASE_URL` (padrão `http://localhost:5281`),
`TEST_DATABASE` (obrigatória), `TEST_PYTHON`, `TEST_BROWSER` e `TEST_OUTPUT`.
O teste registra `resultado.json` e capturas antes e depois.

O SKU fixo pertence apenas a este fixture e o script de preparação falha se os
identificadores já existirem. Rode-o num banco descartável novo e não contra
banco compartilhado. A suíte xUnit do repositório continua sendo validada
separadamente e cobre a regressão da aplicação.
