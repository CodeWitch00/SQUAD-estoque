# S3-BE-013 — ViewModel do histórico de rupturas

## Escopo

Contrato de exibição em `Models/ViewModels/RupturasIndexViewModel.cs`, no namespace `SquadEstoque.Web.Models`, seguindo as convenções dos ViewModels existentes.

- `RupturasIndexViewModel.Rupturas`: coleção tipada, inicializada vazia.
- `RupturaHistoricoViewModel`: produto (nome do modelo), marca, cor, numeração, quantidade de ocorrências e última ocorrência opcional (`DateTime?`).
- Somente dados de exibição do lojista, sem entidades EF, dados de usuários ou atendimentos.
- Sem consulta, agrupamento, ordenação, controller, View ou alteração de banco. A composição dos dados pertence à futura consulta.

## Revisão do contrato

Inspeção do arquivo confirmou os seis campos necessários, strings inicializadas, coleção não nula e data opcional para registros sem última ocorrência disponível. Cada item representa um agregado por modelo e numeração; a quantidade representa ocorrências de ruptura, não saldo ou pares vendidos.

Não há composição, cálculo ou validação neste contrato; por isso não foram adicionados testes unitários de propriedades automáticas. A validação visual de tela não se aplica, pois este cartão entrega apenas o ViewModel.

## Verificações

- `git diff --check`: aprovado.
- `dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj`: aprovado, zero erros. Aviso NU1900 devido à indisponibilidade da consulta de vulnerabilidades do NuGet no ambiente local.
- `dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj`: aprovado, 212 testes, zero falhas e zero ignorados.
