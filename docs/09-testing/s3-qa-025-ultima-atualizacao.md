# S3-QA-025 — Última atualização do saldo

## Objetivo e rastreabilidade

Comprovar que `Sku.UltimaAtualizacaoSaldoEm` muda somente após operações de estoque confirmadas. Referências: RF-12, RF-19 e UC-06; cartão **[S3-QA-025] Testar última atualização — entrada, venda e ajuste**.

## Cobertura entregue

`UltimaAtualizacaoSaldoHttpTests` acrescenta 16 cenários HTTP autenticados com antiforgery real e SQLite em memória isolado por caso. Cada cenário começa com data UTC anterior conhecida ou com data nula, representando SKU legado.

| Operação | Saldo inicial → final | Data esperada | Evidência persistida |
| --- | --- | --- | --- |
| Entrada de 2 pares | 2 → 4 | Dentro do intervalo do POST | Uma ENTRADA |
| Venda de 1 par | 2 → 1 | Dentro do intervalo do POST | Uma SAIDA |
| Ajuste para 5 pares | 2 → 5 | Dentro do intervalo do POST | Um AJUSTE |
| Ruptura / Não tinha | 2 → 2 | Inalterada | Uma ruptura, nenhuma movimentação |
| Desistiu após seleção | 2 → 2 | Inalterada | Atendimento concluído como DESISTIU |
| Saída de 3 pares rejeitada | 2 → 2 | Inalterada | Nenhuma movimentação |
| Ajuste para o mesmo saldo | 2 → 2 | Inalterada | Nenhuma movimentação |
| Ajuste sem motivo | 2 → 2 | Inalterada | Nenhuma movimentação |

As verificações usam outro contexto e consultas `AsNoTracking`, conferem UTC e preservação dos demais SKUs. Consulta e seleção também devem preservar saldo e data. O relógio da aplicação usa `DateTime.UtcNow`; os testes delimitam o intervalo imediatamente antes/depois do POST, sem sleeps ou alterações no código de produção.

A suíte existente complementa essa cobertura com rollback de saída após falha de persistência, migração da coluna, entrada fiscal, renderização da data e cenários de Desistiu.

## Resultados locais — 29/10/2026

- Base: `95199c4`, origin/main consultada antes da alteração.
- Baseline: 277 testes aprovados, zero falhas.
- Novos cenários: 16 aprovados, zero falhas.
- Build da aplicação em Release: aprovado, zero erros.
- Suíte completa em Release: 293 aprovados, zero falhas e zero ignorados.
- Ambiente: Windows, SDK .NET 10.
- Aviso NU1900: consulta de vulnerabilidades do NuGet indisponível pela rede no ambiente local. Não representa falha de compilação ou teste; a auditoria de dependências não foi confirmada nessa execução.

Comandos reproduzíveis na raiz do repositório:

```sh
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj --configuration Release
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --configuration Release
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --filter FullyQualifiedName~UltimaAtualizacaoSaldoHttpTests
git diff --check
```

## Validação e fechamento XP

Os fluxos foram exercitados por HTTP automatizado; não foi executada homologação manual em navegador. Para homologação, usar banco de desenvolvimento: conferir a data na grade, confirmar entrada/venda/ajuste e verificar atualização; registrar ruptura e Desistiu, rejeitar saída superior ao saldo e verificar preservação. Como a tela pode formatar somente até minutos, realizar operações em minutos diferentes ou conferir a precisão completa no banco local.

O PR deve passar pelo GitHub Actions e por outro integrante antes do merge. Revisão humana, homologação manual e merge não estão declarados como concluídos neste registro. A governança do workspace restringe alterações e movimentações no Trello; nenhuma foi realizada. Este documento reúne as evidências para posterior vínculo ao cartão mediante autorização específica.
