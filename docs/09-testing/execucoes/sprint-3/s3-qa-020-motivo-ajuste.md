# S3-QA-020 - Motivo do ajuste e persistência rastreável

## Escopo

Validação HTTP de `/Movimentacoes/Ajuste`, com login real de LOJISTA,
token antiforgery da sessão e SQLite isolado em memória por cenário.
Referências: RF-10, RF-23 e UC-10. Testes de autorização permanecem no
cartão próprio e em `AjusteAuthorizationHttpTests`.

Arquivo: `tests/SquadEstoque.Web.Tests/AjusteMotivoHttpTests.cs`.

## Cenários

| Entrada | Resultado esperado |
| --- | --- |
| Campo Motivo ausente | Rejeitar e preservar todos os saldos e movimentos anteriores. |
| Motivo vazio | Rejeitar e preservar todos os saldos e movimentos anteriores. |
| Somente espaços | Rejeitar e preservar todos os saldos e movimentos anteriores. |
| Quatro caracteres | Rejeitar por comprimento mínimo. |
| Quatro caracteres com espaços nas extremidades | Rejeitar após normalização. |
| Cinco caracteres com espaços nas extremidades | Aceitar o limite mínimo, persistindo o motivo sem os espaços externos. |
| Motivo válido, saldo de 10 para 15 | Persistir saldo 15 e exatamente um movimento AJUSTE com quantidade 5. |
| Motivo válido, saldo de 10 para 7 | Persistir saldo 7 e exatamente um movimento AJUSTE com quantidade 3. |

Os cenários válidos também conferem SKU, usuário autenticado e data entre o
início e o término do POST. Valores de usuário e data enviados pelo cliente
não substituem os valores do servidor. O movimento anterior e os saldos dos
demais SKUs são preservados. A consulta de verificação usa um novo escopo e
`AsNoTracking`, para verificar o estado persistido.

Os identificadores e valores dos dados próprios do cenário são fixos. O relógio
é verificado por intervalo, pois o comportamento atual usa `DateTime.UtcNow`.

## Validação em 22/10/2026

- Base da main: `0320ca6f093fe9b1eae58a28c67f650dd1e6bdc4`.
- Baseline: 244 aprovados, 0 falhas, 0 ignorados.
- Build da aplicação: aprovado, 0 erros.
- Suíte após a alteração: 252 aprovados, 0 falhas, 0 ignorados; inclui os oito
  novos cenários. Tempo reportado pelo runner: 19 segundos.
- Ambiente: Windows, SDK .NET 10.0.401, runtime .NET 10.0.12.
- Aviso NU1900: consulta de vulnerabilidades do NuGet indisponível por rede.
- A primeira execução focada encontrou quatro expectativas incorretas de
  mensagem no teste: o MVC exibe primeiro os erros de DataAnnotations. As
  expectativas foram corrigidas; a aplicação não precisou ser alterada.
- Não há mudança visual nem roteiro manual específico nas capturas do cartão;
  não foi realizada validação manual de interface. Os cenários foram executados
  automaticamente pelo pipeline HTTP real da aplicação em teste.
- Os resultados locais não substituem GitHub Actions nem revisão da mantenedora.

Reprodução a partir da raiz do repositório:

```sh
dotnet restore tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj --no-restore
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-restore
```

Somente os novos cenários:

```sh
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --filter FullyQualifiedName~AjusteMotivoHttpTests
```

## Decisões e limites

O domínio descreve ajuste por delta, enquanto o controller e a tela recebem
`NovoSaldoApurado` e gravam a diferença absoluta no histórico. Felipe autorizou
explicitamente testar o comportamento atual, sem mudar a regra de negócio.
O mínimo de cinco caracteres após `Trim()` corresponde à implementação atual.

Felipe também confirmou o uso de `dev/felipe`, conforme o CONTRIBUTING,
em vez de criar a branch por cartão mencionada no checklist XP. A sincronização
com a main preservou o teste atual de atendimento pendente na resolução do
conflito em `ConsultaOperacionalHttpTests.cs`.

As atualizações do Trello ficaram sob responsabilidade de Felipe, conforme sua
orientação. O PR deve ser revisado pela mantenedora; este relatório não afirma
aprovação, merge ou conclusão do cartão.
