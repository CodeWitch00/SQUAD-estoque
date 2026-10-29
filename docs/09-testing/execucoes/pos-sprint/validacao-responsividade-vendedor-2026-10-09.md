# Validação de responsividade do módulo do vendedor

**Data:** 09/10/2026  
**Branch:** `dev/rayana`  
**Escopo:** responsividade, alvos de toque, foco e navegação do atendimento do vendedor.

## Resultado

Validação automatizada e visual assistida concluída nos viewports previstos no plano de testes:

- mobile: 360×800, 390×844 e 430×932;
- desktop: 1280×800.

Os cenários cobertos incluem busca/resultados, grade de numerações, estados `Vendeu`, `Não tinha` e `Desistiu`, mensagens, seleção de SKU e `Nova consulta`.

Foi verificado que:

- não há rolagem horizontal nos layouts avaliados;
- cards, ações e nova consulta mantêm alvo mínimo de toque de 44×44 CSS px;
- a grade se adapta para duas colunas no mobile e amplia progressivamente no desktop;
- numeração, saldo e estado permanecem textuais e legíveis, com marcador complementar;
- o foco visível e a ordem de navegação existentes foram preservados;
- a seleção mantém o painel de atendimento acessível sem sobrepor o conteúdo de forma inesperada.

## Evidências visuais

A validação visual foi realizada localmente nos viewports de referência 360×800, 390×844, 430×932 e 1280×800. As capturas foram usadas apenas durante a análise e não são mantidas no repositório.

## Verificações executadas

```text
git diff --check
npm run test:frontend
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-restore
```

Resultado: 17 testes frontend aprovados e 167 testes .NET aprovados.

As capturas foram obtidas com Chromium/Playwright em viewports CSS. A validação em aparelho físico Android/iOS e com usuários reais continua sendo a etapa manual de release prevista no plano de testes; não foi simulada como concluída.
